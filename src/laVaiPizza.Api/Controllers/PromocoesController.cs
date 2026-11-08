using laVaiPizza.Application.Promocoes;
using Microsoft.AspNetCore.Mvc;

namespace laVaiPizza.Api.Controllers;

/// <summary>Promocoes vigentes para o site e o app (nao exige login).</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class PromocoesController : ControllerBase
{
    private readonly PromocaoService _promocaoService;

    public PromocoesController(PromocaoService promocaoService)
    {
        _promocaoService = promocaoService;
    }

    /// <summary>Promocoes ativas e dentro do periodo de vigencia agora.</summary>
    [HttpGet("vigentes")]
    [ProducesResponseType(typeof(IReadOnlyList<PromocaoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PromocaoResponse>>> GetVigentes(CancellationToken cancellationToken)
    {
        var promocoes = await _promocaoService.GetVigentesAsync(cancellationToken);
        return Ok(promocoes);
    }
}
