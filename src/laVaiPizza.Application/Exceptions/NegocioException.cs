namespace laVaiPizza.Application.Exceptions;

/// <summary>
/// Violacao de regra de negocio (ex.: transicao de status nao permitida). A API converte
/// em resposta 400 com a mensagem; ela e escrita para o usuario ler.
/// </summary>
public class NegocioException : Exception
{
    public NegocioException(string mensagem) : base(mensagem) { }
}
