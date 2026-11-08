namespace laVaiPizza.Domain.Entities;

/// <summary>
/// Pizza do cardapio. Nunca e apagada: e desativada (<see cref="Ativa"/> = false)
/// para preservar o historico de pedidos.
/// </summary>
public class Pizza
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;

    /// <summary>
    /// Caminho relativo ao site (ex.: assets/images/pizza.png) ou, para fotos enviadas
    /// pelo admin, relativo a API (ex.: /uploads/arquivo.jpg).
    /// </summary>
    public string? ImagemUrl { get; set; }

    public bool Ativa { get; set; } = true;

    public ICollection<PizzaPreco> Precos { get; set; } = new List<PizzaPreco>();
}
