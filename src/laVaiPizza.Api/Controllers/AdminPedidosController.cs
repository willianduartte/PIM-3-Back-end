using laVaiPizza.Api.Security;
using laVaiPizza.Application.Pedidos;
using laVaiPizza.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace laVaiPizza.Api.Controllers;

/// <summary>Fila de pedidos da equipe. O que cada cargo ve e faz e decidido no servidor.</summary>
[ApiController]
[Route("api/v1/admin/pedidos")]
[Produces("application/json")]
[Authorize(Roles = $"{Cargos.Gerente},{Cargos.Pizzaiolo},{Cargos.Entregador}")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class AdminPedidosController : ControllerBase
{
    private readonly PedidoAdminService _service;

    public AdminPedidosController(PedidoAdminService service)
    {
        _service = service;
    }

    /// <summary>
    /// Fila do cargo logado. O Gerente pode filtrar por status
    /// (sem filtro, ve os pedidos em aberto; os mais antigos primeiro).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PedidoAdminResponse>>> Listar([FromQuery] string? status, CancellationToken ct)
        => Ok(await _service.ListarFilaAsync(User.ToUsuarioAtual(), status, ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PedidoAdminResponse>> Obter(int id, CancellationToken ct)
        => Ok(await _service.ObterAsync(User.ToUsuarioAtual(), id, ct));

    /// <summary>Avanca o pedido para o proximo status (um passo por vez).</summary>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PedidoAdminResponse>> AtualizarStatus(int id, StatusRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarStatusAsync(User.ToUsuarioAtual(), id, request.Status, ct));
}
