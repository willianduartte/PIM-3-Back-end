using laVaiPizza.Api.Security;
using laVaiPizza.Application.Auth;
using laVaiPizza.Application.Funcionarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace laVaiPizza.Api.Controllers;

/// <summary>Login dos funcionarios. O cliente da pizzaria pede sem login.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Entra com e-mail e senha e recebe o token JWT (envie como "Bearer {token}").</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var resposta = await _authService.LoginAsync(request, cancellationToken);

        if (resposta is null)
        {
            // Mesma mensagem para e-mail inexistente e senha errada (nao revela qual falhou).
            return Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Credenciais inválidas",
                detail: "E-mail ou senha incorretos.");
        }

        return Ok(resposta);
    }

    /// <summary>Dados do funcionario dono do token (qualquer cargo autenticado).</summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(FuncionarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<FuncionarioResponse> Me()
    {
        var id = int.TryParse(User.FindFirst(JwtClaims.FuncionarioId)?.Value, out var valor) ? valor : 0;

        return Ok(new FuncionarioResponse(
            id,
            User.FindFirst(JwtClaims.Name)?.Value ?? string.Empty,
            User.FindFirst("email")?.Value ?? string.Empty,
            User.FindFirst(JwtClaims.Role)?.Value ?? string.Empty));
    }
}
