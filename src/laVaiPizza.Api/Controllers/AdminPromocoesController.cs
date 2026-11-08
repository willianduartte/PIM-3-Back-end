using laVaiPizza.Application.Promocoes;
using laVaiPizza.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace laVaiPizza.Api.Controllers;

/// <summary>Cadastro de promocoes. Somente o Gerente.</summary>
[ApiController]
[Route("api/v1/admin/promocoes")]
[Produces("application/json")]
[Authorize(Roles = Cargos.Gerente)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class AdminPromocoesController : ControllerBase
{
    private readonly PromocaoAdminService _service;

    public AdminPromocoesController(PromocaoAdminService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PromocaoAdminResponse>>> Listar(CancellationToken ct)
        => Ok(await _service.ListarAsync(ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromocaoAdminResponse>> Obter(int id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    [HttpPost]
    [ProducesResponseType(typeof(PromocaoAdminResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PromocaoAdminResponse>> Criar(PromocaoRequest request, CancellationToken ct)
    {
        var promocao = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Obter), new { id = promocao.Id }, promocao);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromocaoAdminResponse>> Atualizar(int id, PromocaoRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(int id, CancellationToken ct)
    {
        await _service.RemoverAsync(id, ct);
        return NoContent();
    }
}
