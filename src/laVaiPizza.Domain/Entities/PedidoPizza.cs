namespace laVaiPizza.Domain.Entities;

public sealed class PedidoPizza
{
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;
    public int PizzaId { get; set; }
    public Pizza Pizza { get; set; } = null!;
    public int Quantidade { get; set; } 
}