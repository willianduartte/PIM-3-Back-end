using laVaiPizza.Domain.Constants;

namespace laVaiPizza.Domain.Entities;

/// <summary>
/// Promocao. Com <see cref="PizzaId"/> nulo vale para o pedido todo; com valor,
/// vale somente para aquela pizza.
/// </summary>
public class Promocao
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    /// <summary>PERCENTUAL, VALOR_FIXO ou FRETE_GRATIS (veja <see cref="TiposPromocao"/>).</summary>
    public string Tipo { get; set; } = TiposPromocao.Percentual;

    public decimal Valor { get; set; }

    /// <summary>Subtotal minimo do pedido para a promocao valer (opcional).</summary>
    public decimal? ValorMinimo { get; set; }

    public DateTime Inicio { get; set; }
    public DateTime Fim { get; set; }
    public bool Ativa { get; set; } = true;

    public int? PizzaId { get; set; }
    public Pizza? Pizza { get; set; }

    public bool EstaVigente(DateTime agora) => Ativa && agora >= Inicio && agora <= Fim;

    /// <summary>
    /// Calcula o desconto sobre um subtotal. Mesma regra da function CalcularDesconto
    /// do banco: arredonda em 2 casas e nunca passa do subtotal.
    /// </summary>
    public decimal CalcularDesconto(decimal subtotal)
    {
        if (subtotal <= 0) return 0m;

        var desconto = Tipo switch
        {
            TiposPromocao.Percentual => Math.Round(subtotal * Valor / 100m, 2, MidpointRounding.AwayFromZero),
            TiposPromocao.ValorFixo => Valor,
            _ => 0m
        };

        return Math.Clamp(desconto, 0m, subtotal);
    }
}
