using laVaiPizza.Application.Exceptions;
using laVaiPizza.Application.Pedidos;
using laVaiPizza.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace laVaiPizza.Api.Areas.Admin.Controllers;

/// <summary>
/// Fila de pedidos da equipe. A pagina carrega as colunas por fetch() a cada 5 segundos; o que cada
/// cargo ve e pode fazer e decidido por PedidoAdminService (o mesmo da API).
/// </summary>
[Route("admin/pedidos")]
[Authorize(Roles = $"{Cargos.Gerente},{Cargos.Pizzaiolo},{Cargos.Entregador}")]
public class PedidosController : AdminControllerBase
{
    private readonly PedidoAdminService _service;

    public PedidosController(PedidoAdminService service)
    {
        _service = service;
    }

    [HttpGet("")]
    public IActionResult Index() => View();

    [HttpGet("fila")]
    public async Task<IActionResult> Fila(string? status, CancellationToken ct)
    {
        try
        {
            return Json(await _service.ListarFilaAsync(Usuario, status, ct));
        }
        catch (Exception erro) when (erro is NegocioException or AcessoNegadoException)
        {
            return JsonErro(erro);
        }
    }

    [HttpPost("{id:int}/status")]
    public async Task<IActionResult> AtualizarStatus(int id, [FromBody] StatusRequest request, CancellationToken ct)
    {
        try
        {
            return Json(await _service.AtualizarStatusAsync(Usuario, id, request.Status, ct));
        }
        catch (Exception erro) when (erro is NegocioException or NaoEncontradoException
                                      or AcessoNegadoException or ConflitoException)
        {
            return JsonErro(erro);
        }
    }
}
