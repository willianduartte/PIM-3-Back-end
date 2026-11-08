using laVaiPizza.Application.Funcionarios;
using laVaiPizza.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace laVaiPizza.Api.Controllers;

/// <summary>Equipe da pizzaria. Somente o Gerente enxerga.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[Authorize(Roles = Cargos.Gerente)]
public class FuncionariosController : ControllerBase
{
    private readonly FuncionarioService _funcionarioService;

    public FuncionariosController(FuncionarioService funcionarioService)
    {
        _funcionarioService = funcionarioService;
    }

    /// <summary>Lista os funcionarios (sem o hash da senha).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FuncionarioResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<FuncionarioResponse>>> Get(CancellationToken cancellationToken)
    {
        return Ok(await _funcionarioService.ListarAsync(cancellationToken));
    }
}
