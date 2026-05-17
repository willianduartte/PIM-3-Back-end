namespace laVaiPizza.Domain.Entities;

public sealed class Login
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public Funcionario? Funcionario { get; set; }
}