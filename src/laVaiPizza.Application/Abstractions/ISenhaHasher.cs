namespace laVaiPizza.Application.Abstractions;

/// <summary>
/// Verificacao de senha. A implementacao (PasswordHasher do ASP.NET Core) fica na camada Api,
/// entao a Application nao depende de framework web.
/// </summary>
public interface ISenhaHasher
{
    /// <summary>True se a senha informada corresponde ao hash guardado no banco.</summary>
    bool Verificar(string senhaHash, string senhaInformada);
}
