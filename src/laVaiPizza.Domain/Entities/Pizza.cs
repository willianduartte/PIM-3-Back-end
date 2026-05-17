namespace laVaiPizza.Domain.Entities;

public sealed class Pizza
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tamanho { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public ICollection<PedidoPizza> PedidoPizzas { get; set; } = new List<PedidoPizza>();
}