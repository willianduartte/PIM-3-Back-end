namespace laVaiPizza.Api.Security;

/// <summary>
/// Configuracao do JWT (secao "Jwt"). A chave NAO fica no repositorio: use
/// "dotnet user-secrets" no desenvolvimento ou a variavel de ambiente Jwt__Key em outros ambientes.
/// </summary>
public class JwtOptions
{
    public const string Secao = "Jwt";

    /// <summary>HS256 exige chave de pelo menos 256 bits (32 caracteres/bytes).</summary>
    public const int TamanhoMinimoChave = 32;

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;

    /// <summary>Validade do token em minutos (padrao: 8 horas, um turno de trabalho).</summary>
    public int ExpiresMinutes { get; set; } = 480;
}
