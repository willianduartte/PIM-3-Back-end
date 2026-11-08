using laVaiPizza.Domain.Constants;
using laVaiPizza.Domain.Entities;

namespace laVaiPizza.Domain.Services;

/// <summary>
/// Regra unica de qual promocao vale em cada situacao. E usada pelo cardapio (preco que o
/// cliente ve) e pelo pedido (preco que o cliente paga), para os dois nunca divergirem.
///
/// Regras:
/// 1. Promocao de uma pizza (PizzaId preenchido) reduz o preco unitario daquela pizza.
///    Vale a que mais desconta; nao acumulam entre si. "ValorMinimo" nao se aplica a elas.
/// 2. Promocao do pedido (PizzaId nulo, PERCENTUAL ou VALOR_FIXO) incide sobre o valor ja
///    reduzido pelas promocoes de pizza. Vale a que mais desconta; nao acumulam entre si.
///    "ValorMinimo" e comparado com o subtotal original (sem nenhum desconto).
/// 3. Frete gratis (PizzaId nulo) zera a taxa de entrega; "ValorMinimo" e comparado com o
///    subtotal original.
///
/// Todas as listas recebidas devem conter somente promocoes ja vigentes.
/// </summary>
public static class SelecaoDePromocoes
{
    /// <summary>Melhor promocao de uma pizza e o desconto por unidade que ela da.</summary>
    public static (Promocao? Promocao, decimal Desconto) MelhorParaPizza(
        IEnumerable<Promocao> vigentes, int pizzaId, decimal precoUnitario)
    {
        Promocao? melhor = null;
        var melhorDesconto = 0m;

        foreach (var promocao in vigentes)
        {
            if (promocao.PizzaId != pizzaId || promocao.Tipo == TiposPromocao.FreteGratis)
                continue;

            var desconto = promocao.CalcularDesconto(precoUnitario);
            if (desconto > melhorDesconto)
            {
                melhor = promocao;
                melhorDesconto = desconto;
            }
        }

        return (melhor, melhorDesconto);
    }

    /// <summary>Melhor promocao do pedido todo e o desconto em reais sobre <paramref name="baseDesconto"/>.</summary>
    public static (Promocao? Promocao, decimal Desconto) MelhorDoPedido(
        IEnumerable<Promocao> vigentes, decimal subtotalOriginal, decimal baseDesconto)
    {
        Promocao? melhor = null;
        var melhorDesconto = 0m;

        foreach (var promocao in vigentes)
        {
            if (promocao.PizzaId is not null || promocao.Tipo == TiposPromocao.FreteGratis)
                continue;

            if (!AtendeMinimo(promocao, subtotalOriginal))
                continue;

            var desconto = promocao.CalcularDesconto(baseDesconto);
            if (desconto > melhorDesconto)
            {
                melhor = promocao;
                melhorDesconto = desconto;
            }
        }

        return (melhor, melhorDesconto);
    }

    /// <summary>Promocao de frete gratis que vale para o pedido, se houver.</summary>
    public static Promocao? FreteGratis(IEnumerable<Promocao> vigentes, decimal subtotalOriginal)
    {
        return vigentes.FirstOrDefault(p =>
            p.PizzaId is null
            && p.Tipo == TiposPromocao.FreteGratis
            && AtendeMinimo(p, subtotalOriginal));
    }

    private static bool AtendeMinimo(Promocao promocao, decimal subtotalOriginal)
    {
        return promocao.ValorMinimo is null || subtotalOriginal >= promocao.ValorMinimo.Value;
    }
}
