using laVaiPizza.Application.Abstractions;
using Microsoft.AspNetCore.Identity;

namespace laVaiPizza.Api.Security;

/// <summary>
/// Verifica a senha com o PasswordHasher nativo do ASP.NET Core (PBKDF2 com sal aleatorio),
/// o mesmo formato dos hashes gravados no script do banco.
/// </summary>
public class IdentityPasswordHasher : ISenhaHasher
{
    private static readonly PasswordHasher<object> Hasher = new();

    public bool Verificar(string senhaHash, string senhaInformada)
    {
        var resultado = Hasher.VerifyHashedPassword(new object(), senhaHash, senhaInformada);
        return resultado != PasswordVerificationResult.Failed;
    }
}
