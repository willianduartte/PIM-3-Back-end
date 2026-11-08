// Formulario de promocao: mostra so os campos que fazem sentido para o tipo escolhido.
;(() => {
  const tipo = document.getElementById('Tipo')
  const pizza = document.getElementById('PizzaId')
  const grupoValor = document.getElementById('grupoValor')
  const grupoPizza = document.getElementById('grupoPizza')
  const grupoMinimo = document.getElementById('grupoMinimo')
  const rotuloValor = document.getElementById('rotuloValor')
  const valor = grupoValor.querySelector('input')
  const minimo = grupoMinimo.querySelector('input')

  function atualizar() {
    const freteGratis = tipo.value === 'FRETE_GRATIS'

    // Frete grátis não tem valor e vale sempre para o pedido todo
    grupoValor.hidden = freteGratis
    grupoPizza.hidden = freteGratis
    if (freteGratis) {
      valor.value = '0'
      pizza.value = ''
    }

    rotuloValor.textContent = tipo.value === 'PERCENTUAL' ? 'Percentual de desconto (%)' : 'Desconto em reais (R$)'

    // Valor mínimo só existe em promoção do pedido todo
    const valeParaUmaPizza = pizza.value !== ''
    grupoMinimo.hidden = valeParaUmaPizza
    if (valeParaUmaPizza) minimo.value = ''
  }

  tipo.addEventListener('change', atualizar)
  pizza.addEventListener('change', atualizar)
  atualizar()
})()
