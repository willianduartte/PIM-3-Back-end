namespace laVaiPizza.Domain.Entities;

/// <summary>Credencial de acesso. Somente funcionarios possuem login.</summary>
public class Login
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;

    /// <summary>Hash gerado pelo PasswordHasher do ASP.NET Core (nunca a senha em texto).</summary>
    public string SenhaHash { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public Funcionario? Funcionario { get; set; }
}
