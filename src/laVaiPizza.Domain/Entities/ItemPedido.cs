namespace laVaiPizza.Domain.Entities;

/// <summary>
/// Item do pedido (associativa entre Pedido e Pizza). <see cref="PrecoUnitario"/> guarda o
/// preco no momento da venda, entao mudar o preco da pizza nao altera o faturamento passado.
/// </summary>
public class ItemPedido
{
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;

    public int PizzaId { get; set; }
    public Pizza Pizza { get; set; } = null!;

    public string Tamanho { get; set; } = string.Empty;
    public int Quantidade { get; set; } = 1;
    public decimal PrecoUnitario { get; set; }
}
