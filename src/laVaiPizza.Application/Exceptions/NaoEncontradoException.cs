namespace laVaiPizza.Application.Exceptions;

/// <summary>Recurso inexistente (pizza, pedido, promocao...). A API converte em 404.</summary>
public class NaoEncontradoException : Exception
{
    public NaoEncontradoException(string mensagem) : base(mensagem) { }
}
