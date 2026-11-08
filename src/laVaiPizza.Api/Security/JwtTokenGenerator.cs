using System.Text;
using laVaiPizza.Application.Abstractions;
using laVaiPizza.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace laVaiPizza.Api.Security;

/// <summary>Emite o JWT assinado (HS256) com o cargo do funcionario como "role".</summary>
public class JwtTokenGenerator : ITokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public TokenGerado Gerar(Funcionario funcionario)
    {
        var expiraEm = DateTime.UtcNow.AddMinutes(_options.ExpiresMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Expires = expiraEm,
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = funcionario.Id.ToString(),
                [JwtRegisteredClaimNames.Email] = funcionario.Login.Email,
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString(),
                [JwtClaims.Name] = funcionario.Nome,
                [JwtClaims.Role] = funcionario.Cargo,
                [JwtClaims.FuncionarioId] = funcionario.Id
            },
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
                SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new TokenGerado(token, expiraEm);
    }
}
