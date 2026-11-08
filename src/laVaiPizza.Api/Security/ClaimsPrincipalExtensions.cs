using System.Security.Claims;
using laVaiPizza.Application.Pedidos;

namespace laVaiPizza.Api.Security;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Id do funcionario dono do token (0 se a claim nao existir).</summary>
    public static int GetFuncionarioId(this ClaimsPrincipal user)
        => int.TryParse(user.FindFirst(JwtClaims.FuncionarioId)?.Value, out var id) ? id : 0;

    public static UsuarioAtual ToUsuarioAtual(this ClaimsPrincipal user)
        => new(user.GetFuncionarioId(), user.FindFirst(JwtClaims.Role)?.Value ?? string.Empty);
}
