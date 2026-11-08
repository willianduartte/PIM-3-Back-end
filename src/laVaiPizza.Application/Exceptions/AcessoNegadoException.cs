namespace laVaiPizza.Application.Exceptions;

/// <summary>Funcionario autenticado sem permissao para a acao (regra por cargo). A API converte em 403.</summary>
public class AcessoNegadoException : Exception
{
    public AcessoNegadoException(string mensagem) : base(mensagem) { }
}
