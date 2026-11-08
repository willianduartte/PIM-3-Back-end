namespace laVaiPizza.Domain.Entities;

public class Funcionario
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    /// <summary>Gerente, Pizzaiolo ou Entregador (veja <see cref="Constants.Cargos"/>).</summary>
    public string Cargo { get; set; } = string.Empty;

    public string? Telefone { get; set; }

    public int LoginId { get; set; }
    public Login Login { get; set; } = null!;
}
