using laVaiPizza.Api.Security;
using laVaiPizza.Application.Exceptions;
using laVaiPizza.Application.Pedidos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace laVaiPizza.Api.Areas.Admin.Controllers;

/// <summary>
/// Base das paginas da area administrativa: exige o login por cookie e valida o token
/// antiforgery em todo POST. Nao aparece no Swagger (e pagina, nao API).
/// </summary>
[Area("Admin")]
[Authorize(AuthenticationSchemes = AdminAuth.Scheme)]
[AutoValidateAntiforgeryToken]
[ApiExplorerSettings(IgnoreApi = true)]
public abstract class AdminControllerBase : Controller
{
    protected UsuarioAtual Usuario => User.ToUsuarioAtual();

    /// <summary>Resposta JSON de erro para as chamadas fetch() (a tela mostra a mensagem).</summary>
    protected IActionResult JsonErro(Exception erro)
    {
        var (status, mensagem) = erro switch
        {
            NegocioException => (StatusCodes.Status400BadRequest, erro.Message),
            NaoEncontradoException => (StatusCodes.Status404NotFound, erro.Message),
            AcessoNegadoException => (StatusCodes.Status403Forbidden, erro.Message),
            ConflitoException => (StatusCodes.Status409Conflict, erro.Message),
            _ => throw erro // erro inesperado: segue para o tratamento global (500)
        };

        return StatusCode(status, new { erro = mensagem });
    }

    protected void Sucesso(string mensagem) => TempData["Sucesso"] = mensagem;

    protected void Erro(string mensagem) => TempData["Erro"] = mensagem;
}
