// Dashboard: cartoes e graficos (Chart.js). Os dados vem das procedures do banco.
;(() => {
  'use strict'

  const raiz = document.getElementById('dashboard')
  const urlResumo = raiz.dataset.urlResumo
  const urlDados = raiz.dataset.urlDados
  const urlCsv = raiz.dataset.urlCsv

  const moeda = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })
  const inteiro = new Intl.NumberFormat('pt-BR')
  const MESES = ['Jan', 'Fev', 'Mar', 'Abr', 'Mai', 'Jun', 'Jul', 'Ago', 'Set', 'Out', 'Nov', 'Dez']
  const CORES = ['#f27405', '#3c2a21', '#2f9e44', '#d63b2f', '#f4b942', '#0d6efd', '#8e44ad', '#6c757d']
  const COR_STATUS = {
    'Em Preparação': '#f27405',
    'Aguardando Coleta': '#f4b942',
    'Saiu para Entrega': '#0d6efd',
    Entregue: '#2f9e44'
  }

  const graficos = {}
  const el = id => document.getElementById(id)

  // ------------------------------------------------------------ utilidades

  function dataIso(data) {
    const p = n => String(n).padStart(2, '0')
    return `${data.getFullYear()}-${p(data.getMonth() + 1)}-${p(data.getDate())}`
  }

  // "Últimos N dias" (0 = todo o período) como inicio/fim para as procedures
  function periodo() {
    const dias = Number(el('periodo').value)
    if (!dias) return {}
    const fim = new Date()
    const inicio = new Date()
    inicio.setDate(inicio.getDate() - dias)
    return { inicio: dataIso(inicio), fim: dataIso(fim) }
  }

  function montarUrl(modelo, nome, parametros) {
    const consulta = new URLSearchParams()
    Object.entries(parametros || {}).forEach(([k, v]) => {
      if (v !== undefined && v !== null && v !== '') consulta.set(k, v)
    })
    const texto = consulta.toString()
    const base = modelo.replace('__nome__', nome)
    return texto ? `${base}?${texto}` : base
  }

  function mostrarErro(texto) {
    const caixa = el('erroDashboard')
    caixa.textContent = texto
    caixa.hidden = !texto
  }

  async function buscar(nome, parametros) {
    const resposta = await fetch(montarUrl(urlDados, nome, parametros), { headers: { 'X-Requested-With': 'fetch' } })
    if (resposta.status === 401) {
      window.location.reload() // sessão expirou: o servidor leva para o login
      throw new Error('Sessão expirada.')
    }
    if (!resposta.ok) {
      const corpo = await resposta.json().catch(() => ({}))
      throw new Error(corpo.erro || 'Não foi possível carregar os dados.')
    }
    return resposta.json()
  }

  // Cada gráfico tem um link "CSV" que baixa exatamente o que está na tela
  function ligarCsv(nome, parametros) {
    const link = document.querySelector(`[data-csv="${nome}"]`)
    if (link) link.href = montarUrl(urlCsv, nome, parametros)
  }

  function desenhar(id, configuracao) {
    if (graficos[id]) graficos[id].destroy()
    graficos[id] = new Chart(el(id), configuracao)
  }

  // Alternativa em tabela para quem usa leitor de tela (e para conferir os números)
  function tabela(idContainer, colunas, linhas) {
    const t = document.createElement('table')
    t.className = 'table table-sm table-striped mb-0'

    const cabecalho = t.createTHead().insertRow()
    colunas.forEach(c => {
      const th = document.createElement('th')
      th.scope = 'col'
      th.textContent = c
      cabecalho.append(th)
    })

    const corpo = t.createTBody()
    linhas.forEach(linha => {
      const tr = corpo.insertRow()
      linha.forEach(valor => (tr.insertCell().textContent = valor))
    })

    const contentor = el(idContainer)
    contentor.tabIndex = 0 // tabela larga rola na horizontal: precisa ser alcancavel pelo teclado
    contentor.setAttribute('role', 'region')
    contentor.setAttribute('aria-label', 'Dados do grafico em tabela')
    contentor.replaceChildren(t)
  }

  const opcoesBase = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { display: false } }
  }

  // ------------------------------------------------------------ cartões

  const FORMATO_CARTAO = {
    pedidosHoje: v => inteiro.format(v),
    faturamentoHoje: v => moeda.format(v),
    pedidosMes: v => inteiro.format(v),
    faturamentoMes: v => moeda.format(v),
    ticketMedioMes: v => moeda.format(v),
    pedidosEmAberto: v => inteiro.format(v),
    tempoMedioEntregaMin: v => (v === null || v === undefined ? '-' : `${Math.round(v)} min`)
  }

  async function atualizarCartoes() {
    const resposta = await fetch(urlResumo, { headers: { 'X-Requested-With': 'fetch' } })
    if (resposta.status === 401) return window.location.reload()
    if (!resposta.ok) throw new Error('Não foi possível atualizar os indicadores.')

    const resumo = await resposta.json()
    Object.entries(FORMATO_CARTAO).forEach(([campo, formatar]) => {
      const alvo = document.querySelector(`[data-campo="${campo}"]`)
      if (alvo) alvo.textContent = formatar(resumo[campo])
    })
    el('atualizadoEm').textContent = 'Atualizado às ' + new Date().toLocaleTimeString('pt-BR')
  }

  // ------------------------------------------------------------ gráficos

  async function graficoMensal() {
    const ano = el('anoMensal').value
    const dados = await buscar('faturamento-mensal', { ano })
    ligarCsv('faturamento-mensal', { ano })

    desenhar('gMensal', {
      type: 'bar',
      data: {
        labels: dados.map(d => MESES[d.mes - 1]),
        datasets: [{ label: 'Faturamento', data: dados.map(d => d.faturamento), backgroundColor: CORES[0] }]
      },
      options: {
        ...opcoesBase,
        scales: { y: { beginAtZero: true, ticks: { callback: v => moeda.format(v) } } },
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              label: c => moeda.format(c.parsed.y),
              afterLabel: c => `${inteiro.format(dados[c.dataIndex].quantidadePedidos)} pedidos · ticket ${moeda.format(dados[c.dataIndex].ticketMedio)}`
            }
          }
        }
      }
    })
    tabela('tMensalTabela', ['Mês', 'Pedidos', 'Faturamento', 'Ticket médio'],
      dados.map(d => [MESES[d.mes - 1], inteiro.format(d.quantidadePedidos), moeda.format(d.faturamento), moeda.format(d.ticketMedio)]))
  }

  async function graficoDiario() {
    const dias = el('diasDiario').value
    const dados = await buscar('faturamento-diario', { dias })
    ligarCsv('faturamento-diario', { dias })

    const rotulo = d => d.dia.slice(8, 10) + '/' + d.dia.slice(5, 7)
    desenhar('gDiario', {
      type: 'line',
      data: {
        labels: dados.map(rotulo),
        datasets: [{
          label: 'Faturamento',
          data: dados.map(d => d.faturamento),
          borderColor: CORES[0],
          backgroundColor: 'rgba(242, 116, 5, 0.15)',
          fill: true,
          tension: 0.3,
          pointRadius: dados.length > 40 ? 0 : 3
        }]
      },
      options: {
        ...opcoesBase,
        scales: { y: { beginAtZero: true, ticks: { callback: v => moeda.format(v) } } },
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              label: c => moeda.format(c.parsed.y),
              afterLabel: c => `${dados[c.dataIndex].quantidadePedidos} pedidos`
            }
          }
        }
      }
    })
    tabela('tDiarioTabela', ['Dia', 'Pedidos', 'Faturamento'],
      dados.map(d => [rotulo(d), inteiro.format(d.quantidadePedidos), moeda.format(d.faturamento)]))
  }

  async function graficoRanking() {
    const parametros = { ...periodo(), top: 10 }
    const dados = await buscar('ranking-pizzas', parametros)
    ligarCsv('ranking-pizzas', parametros)

    desenhar('gRanking', {
      type: 'bar',
      data: {
        labels: dados.map(d => d.pizza),
        datasets: [{ label: 'Pizzas vendidas', data: dados.map(d => d.quantidadeVendida), backgroundColor: CORES[0] }]
      },
      options: {
        ...opcoesBase,
        indexAxis: 'y',
        scales: { x: { beginAtZero: true, ticks: { precision: 0 } } },
        plugins: {
          legend: { display: false },
          tooltip: {
            callbacks: {
              label: c => `${inteiro.format(c.parsed.x)} vendidas`,
              afterLabel: c => `Faturamento ${moeda.format(dados[c.dataIndex].faturamento)}`
            }
          }
        }
      }
    })
    tabela('tRankingTabela', ['Pizza', 'Vendidas', 'Faturamento'],
      dados.map(d => [d.pizza, inteiro.format(d.quantidadeVendida), moeda.format(d.faturamento)]))
  }

  async function graficoTamanho() {
    const parametros = periodo()
    const dados = await buscar('vendas-por-tamanho', parametros)
    ligarCsv('vendas-por-tamanho', parametros)

    desenhar('gTamanho', {
      type: 'doughnut',
      data: {
        labels: dados.map(d => d.tamanho),
        datasets: [{ data: dados.map(d => d.quantidadeVendida), backgroundColor: CORES }]
      },
      options: {
        ...opcoesBase,
        plugins: { legend: { display: true, position: 'bottom' } }
      }
    })
    tabela('tTamanhoTabela', ['Tamanho', 'Vendidas', 'Faturamento'],
      dados.map(d => [d.tamanho, inteiro.format(d.quantidadeVendida), moeda.format(d.faturamento)]))
  }

  async function graficoStatus() {
    const parametros = periodo()
    const dados = await buscar('pedidos-por-status', parametros)
    ligarCsv('pedidos-por-status', parametros)

    desenhar('gStatus', {
      type: 'doughnut',
      data: {
        labels: dados.map(d => d.status),
        datasets: [{ data: dados.map(d => d.quantidade), backgroundColor: dados.map(d => COR_STATUS[d.status] || CORES[5]) }]
      },
      options: {
        ...opcoesBase,
        plugins: { legend: { display: true, position: 'bottom' } }
      }
    })
    tabela('tStatusTabela', ['Status', 'Pedidos'], dados.map(d => [d.status, inteiro.format(d.quantidade)]))
  }

  async function graficoHorarios() {
    const dias = el('diasHorarios').value
    const dados = await buscar('horarios-pico', { dias })
    ligarCsv('horarios-pico', { dias })

    const maximo = Math.max(...dados.map(d => d.quantidadePedidos))
    desenhar('gHorarios', {
      type: 'bar',
      data: {
        labels: dados.map(d => `${d.hora}h`),
        datasets: [{
          label: 'Pedidos',
          data: dados.map(d => d.quantidadePedidos),
          // O(s) horário(s) de pico ficam em destaque
          backgroundColor: dados.map(d => (maximo > 0 && d.quantidadePedidos === maximo ? CORES[3] : CORES[0]))
        }]
      },
      options: {
        ...opcoesBase,
        scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
      }
    })
    tabela('tHorariosTabela', ['Hora', 'Pedidos'], dados.map(d => [`${d.hora}h`, inteiro.format(d.quantidadePedidos)]))
  }

  async function carregarGraficos() {
    try {
      await Promise.all([graficoMensal(), graficoDiario(), graficoRanking(), graficoTamanho(), graficoStatus(), graficoHorarios()])
      mostrarErro('')
    } catch (erro) {
      mostrarErro(erro.message)
    }
  }

  // Cada filtro recarrega só o gráfico que ele controla
  const recarregar = funcao => () => funcao().then(() => mostrarErro('')).catch(e => mostrarErro(e.message))
  el('anoMensal').addEventListener('change', recarregar(graficoMensal))
  el('diasDiario').addEventListener('change', recarregar(graficoDiario))
  el('diasHorarios').addEventListener('change', recarregar(graficoHorarios))
  el('periodo').addEventListener('change', () => {
    ;[graficoRanking, graficoTamanho, graficoStatus].forEach(f => recarregar(f)())
  })

  // ------------------------------------------------------------ atualização automática

  carregarGraficos()

  setInterval(() => {
    if (!document.hidden) atualizarCartoes().catch(() => {})
  }, 10000)

  // Gráficos acompanham os pedidos novos sem recarregar a página
  setInterval(() => {
    if (!document.hidden) carregarGraficos()
  }, 60000)
})()
