namespace laVaiPizza.Application.Exceptions;

/// <summary>Operacao em conflito com o estado atual (ex.: pedido ja alterado por outra pessoa). A API converte em 409.</summary>
public class ConflitoException : Exception
{
    public ConflitoException(string mensagem) : base(mensagem) { }
}
