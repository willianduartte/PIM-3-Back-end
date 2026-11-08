// Comportamentos comuns do admin. Fica em arquivo proprio porque a politica de seguranca (CSP)
// das paginas /admin nao permite JavaScript dentro do HTML.

// Formularios com data-confirmar="mensagem" pedem confirmacao antes de enviar.
document.addEventListener('submit', evento => {
  const mensagem = evento.target.dataset ? evento.target.dataset.confirmar : null
  if (mensagem && !window.confirm(mensagem)) evento.preventDefault()
})
