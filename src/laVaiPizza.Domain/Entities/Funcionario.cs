using laVaiPizza.Domain.Entities;

public sealed class Funcionario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public int? LoginId { get; set; }
    public Login? Login { get; set; }
}