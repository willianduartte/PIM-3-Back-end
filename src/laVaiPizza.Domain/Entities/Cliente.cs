namespace laVaiPizza.Domain.Entities;

public sealed class Cliente
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string Endereco { get; set; } = string.Empty;

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}