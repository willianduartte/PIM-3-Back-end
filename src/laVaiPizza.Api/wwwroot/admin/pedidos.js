// Fila de pedidos em tempo real: consulta o servidor a cada 5 segundos (polling).
// Cada cargo recebe do servidor somente os pedidos e o botão que pode usar.
;(() => {
  'use strict'

  const INTERVALO = 5000
  const quadro = document.getElementById('quadro')
  const cargo = quadro.dataset.cargo
  const urlFila = quadro.dataset.urlFila
  const urlStatus = quadro.dataset.urlStatus // /admin/pedidos/0/status (o 0 vira o id)
  const urlEntrar = quadro.dataset.urlEntrar
  const token = document.querySelector('#formToken input[name="__RequestVerificationToken"]').value

  const EM_PREPARO = 'Em Preparação'
  const AGUARDANDO = 'Aguardando Coleta'
  const SAIU = 'Saiu para Entrega'
  const ENTREGUE = 'Entregue'

  const COLUNAS_DO_CARGO = {
    Gerente: [EM_PREPARO, AGUARDANDO, SAIU],
    Pizzaiolo: [EM_PREPARO],
    Entregador: [AGUARDANDO, SAIU]
  }
  const TEXTO_DO_BOTAO = {
    [AGUARDANDO]: 'Pronta para coleta',
    [SAIU]: 'Saí para entregar',
    [ENTREGUE]: 'Marcar como entregue'
  }

  const el = id => document.getElementById(id)
  const cartoes = new Map() // id do pedido -> elemento do cartão
  const colunas = new Map() // status -> elemento da lista
  const contadores = new Map() // status -> badge com a quantidade
  const conhecidos = new Set()
  let primeiraCarga = true
  let timer = null
  let parado = false
  let verEntregues = false
  const tituloOriginal = document.title

  const moeda = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })

  function criar(tag, classe, texto) {
    const e = document.createElement(tag)
    if (classe) e.className = classe
    if (texto !== undefined) e.textContent = texto
    return e
  }

  // ------------------------------------------------------------ colunas

  function montarColunas() {
    const status = [...COLUNAS_DO_CARGO[cargo]]
    if (verEntregues) status.push(ENTREGUE)

    const tamanho = status.length === 1 ? 'col-12 col-lg-8' : `col-12 col-lg-${Math.floor(12 / status.length)}`
    quadro.replaceChildren()
    colunas.clear()
    contadores.clear()
    cartoes.clear()

    status.forEach(nome => {
      const coluna = criar('section', tamanho)
      coluna.setAttribute('aria-label', 'Pedidos: ' + nome)

      const caixa = criar('div', 'coluna-pedidos shadow-sm')
      const topo = criar('div', 'd-flex justify-content-between align-items-center p-3 border-bottom')
      const titulo = criar('h2', 'h6 mb-0', nome)
      const contador = criar('span', 'badge text-bg-secondary', '0')
      topo.append(titulo, contador)

      const lista = criar('div', 'lista')
      lista.dataset.status = nome
      caixa.append(topo, lista)
      coluna.append(caixa)
      quadro.append(coluna)

      colunas.set(nome, lista)
      contadores.set(nome, contador)
    })
  }

  // ------------------------------------------------------------ cartões

  function selo(pedido) {
    if (pedido.status === ENTREGUE) return criar('span', 'badge text-bg-success', 'Entregue')
    const estimado = pedido.tempoEstimado || 40
    const classe = pedido.minutosDesdeCriacao > estimado ? 'text-bg-danger'
      : pedido.minutosDesdeCriacao > estimado * 0.8 ? 'text-bg-warning' : 'text-bg-success'
    const atraso = pedido.minutosDesdeCriacao > estimado ? ' (atrasado)' : ''
    return criar('span', 'badge ' + classe, `há ${pedido.minutosDesdeCriacao} min${atraso}`)
  }

  function preencherCartao(cartao, pedido) {
    cartao.dataset.status = pedido.status
    cartao.setAttribute('aria-label', `Pedido ${pedido.id}`)

    const corpo = criar('div', 'card-body p-3')

    const linhaTopo = criar('div', 'd-flex justify-content-between align-items-start gap-2')
    const id = criar('strong', '', `#${pedido.id} `)
    id.append(criar('span', 'text-secondary fw-normal', pedido.codigoAcompanhamento))
    linhaTopo.append(id, selo(pedido))

    const cliente = criar('div', 'small', `${pedido.cliente} · ${pedido.bairro}`)

    const itens = criar('ul', 'itens small')
    pedido.itens.forEach(i => itens.append(criar('li', '', `${i.quantidade}× ${i.pizza} (${i.tamanho})`)))

    corpo.append(linhaTopo, cliente, itens)

    // Telefone e endereço só chegam para quem precisa (o Pizzaiolo não recebe)
    if (pedido.endereco) corpo.append(criar('div', 'small text-secondary mb-1', pedido.endereco))
    if (pedido.telefone) corpo.append(criar('div', 'small text-secondary mb-1', 'Tel.: ' + pedido.telefone))

    const rodape = criar('div', 'd-flex justify-content-between align-items-center mt-2 gap-2')
    rodape.append(criar('strong', '', moeda.format(pedido.total)))

    if (pedido.proximoStatusPermitido) {
      const botao = criar('button', 'btn btn-lavai btn-sm', TEXTO_DO_BOTAO[pedido.proximoStatusPermitido])
      botao.type = 'button'
      botao.addEventListener('click', () => mover(pedido, botao))
      rodape.append(botao)
    }
    corpo.append(rodape)

    const quem = []
    if (pedido.preparadoPor) quem.push('Preparo: ' + pedido.preparadoPor)
    if (pedido.entreguePor) quem.push('Entrega: ' + pedido.entreguePor)
    if (quem.length) corpo.append(criar('div', 'small text-secondary mt-2', quem.join(' · ')))

    cartao.replaceChildren(corpo)
  }

  function atualizarColunas(pedidos) {
    const porColuna = new Map([...colunas.keys()].map(s => [s, []]))
    const presentes = new Set()

    pedidos.forEach(pedido => {
      if (!porColuna.has(pedido.status)) return
      presentes.add(pedido.id)

      let cartao = cartoes.get(pedido.id)
      const assinatura = JSON.stringify(pedido)
      if (!cartao) {
        cartao = criar('article', 'card pedido-card')
        cartoes.set(pedido.id, cartao)
      }
      // Só refaz o cartão se algo mudou (assim o botão não "pisca" enquanto alguém clica)
      if (cartao.dataset.assinatura !== assinatura) {
        preencherCartao(cartao, pedido)
        cartao.dataset.assinatura = assinatura
      }
      porColuna.get(pedido.status).push({ pedido, cartao })
    })

    cartoes.forEach((cartao, id) => {
      if (!presentes.has(id)) {
        cartao.remove()
        cartoes.delete(id)
      }
    })

    porColuna.forEach((itens, status) => {
      // Mais antigos primeiro (quem espera há mais tempo aparece no topo); entregues: mais recentes primeiro
      itens.sort((a, b) => (status === ENTREGUE
        ? new Date(b.pedido.dataHora) - new Date(a.pedido.dataHora)
        : new Date(a.pedido.dataHora) - new Date(b.pedido.dataHora)))

      const lista = colunas.get(status)
      const desejada = itens.map(i => i.cartao)
      const atual = [...lista.querySelectorAll(':scope > .pedido-card')]
      const mesmaOrdem = atual.length === desejada.length && atual.every((c, i) => c === desejada[i])
      if (!mesmaOrdem) lista.append(...desejada)

      const vazio = lista.querySelector(':scope > .sem-pedidos')
      if (desejada.length === 0 && !vazio) lista.append(criar('p', 'text-secondary small text-center my-3 sem-pedidos', 'Nenhum pedido aqui.'))
      if (desejada.length > 0 && vazio) vazio.remove()

      contadores.get(status).textContent = String(desejada.length)
    })
  }

  // ------------------------------------------------------------ novidades

  function avisarNovos(pedidos) {
    const novos = pedidos.filter(p => !conhecidos.has(p.id))
    pedidos.forEach(p => conhecidos.add(p.id))
    if (primeiraCarga || novos.length === 0) return

    novos.forEach(p => cartoes.get(p.id)?.classList.add('novo'))
    setTimeout(() => novos.forEach(p => cartoes.get(p.id)?.classList.remove('novo')), 7000)
    el('avisos').textContent = `Novo pedido: ${novos.map(p => '#' + p.id).join(', ')}`
  }

  // Mostra no título da aba quantos pedidos esperam uma ação deste usuário
  function atualizarTitulo(pedidos) {
    const pendentes = pedidos.filter(p => p.proximoStatusPermitido).length
    document.title = pendentes > 0 ? `(${pendentes}) ${tituloOriginal}` : tituloOriginal
  }

  // ------------------------------------------------------------ rede

  function mostrarErro(texto) {
    const caixa = el('alertaErro')
    caixa.replaceChildren()
    if (texto) caixa.append(texto)
    caixa.hidden = !texto
    return caixa
  }

  function sessaoExpirada() {
    parado = true
    clearTimeout(timer)
    const caixa = mostrarErro('Sua sessão expirou. ')
    const link = criar('a', 'alert-link', 'Entrar novamente')
    link.href = urlEntrar
    caixa.append(link)
    el('statusConexao').textContent = 'Desconectado'
  }

  async function buscarFila(status) {
    const url = status ? `${urlFila}?status=${encodeURIComponent(status)}` : urlFila
    const resposta = await fetch(url, { headers: { 'X-Requested-With': 'fetch', Accept: 'application/json' } })
    if (resposta.status === 401 || resposta.status === 403) {
      sessaoExpirada()
      throw new Error('sessao')
    }
    if (!resposta.ok) throw new Error('Falha ao carregar a fila.')
    return resposta.json()
  }

  async function carregar() {
    clearTimeout(timer)
    try {
      const requisicoes = [buscarFila()]
      if (verEntregues) requisicoes.push(buscarFila(ENTREGUE))
      const [abertos, entregues = []] = await Promise.all(requisicoes)

      const pedidos = [...abertos, ...entregues.slice(0, 10)]
      atualizarColunas(pedidos)
      avisarNovos(pedidos)
      atualizarTitulo(pedidos)
      primeiraCarga = false

      mostrarErro('')
      el('statusConexao').textContent = 'Atualizado às ' + new Date().toLocaleTimeString('pt-BR')
    } catch (erro) {
      if (erro.message === 'sessao') return
      el('statusConexao').textContent = 'Sem conexão. Tentando de novo...'
    }

    agendar()
  }

  function agendar() {
    clearTimeout(timer)
    if (parado || document.hidden) return // ao voltar para a aba, visibilitychange retoma
    timer = setTimeout(carregar, INTERVALO)
  }

  async function mover(pedido, botao) {
    const proximo = pedido.proximoStatusPermitido
    botao.disabled = true
    botao.textContent = 'Salvando...'

    try {
      const resposta = await fetch(urlStatus.replace('/0/', `/${pedido.id}/`), {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          RequestVerificationToken: token,
          'X-Requested-With': 'fetch'
        },
        body: JSON.stringify({ status: proximo })
      })

      if (resposta.status === 401) return sessaoExpirada()
      if (!resposta.ok) {
        const corpo = await resposta.json().catch(() => ({}))
        mostrarErro(corpo.erro || 'Não foi possível atualizar o pedido.')
      } else {
        mostrarErro('')
        el('avisos').textContent = `Pedido #${pedido.id}: ${proximo}`
      }
    } catch {
      mostrarErro('Sem conexão com o servidor. Tente novamente.')
    }

    carregar() // mostra o resultado (ou o que outra pessoa fez) imediatamente
  }

  // ------------------------------------------------------------ início

  const alternarEntregues = el('verEntregues')
  if (alternarEntregues) {
    alternarEntregues.addEventListener('change', () => {
      verEntregues = alternarEntregues.checked
      primeiraCarga = true
      montarColunas()
      carregar()
    })
  }

  document.addEventListener('visibilitychange', () => {
    if (!document.hidden && !parado) carregar()
  })

  montarColunas()
  carregar()
})()
