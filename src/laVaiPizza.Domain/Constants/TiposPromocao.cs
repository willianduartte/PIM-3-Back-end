namespace laVaiPizza.Domain.Constants;

/// <summary>
/// Tipos de promocao (espelha o CHECK da coluna Promocao.Tipo).
/// </summary>
public static class TiposPromocao
{
    /// <summary>Valor = percentual de desconto (ex.: 10 = 10%).</summary>
    public const string Percentual = "PERCENTUAL";

    /// <summary>Valor = desconto em reais.</summary>
    public const string ValorFixo = "VALOR_FIXO";

    /// <summary>Zera a taxa de entrega.</summary>
    public const string FreteGratis = "FRETE_GRATIS";

    public static readonly IReadOnlyList<string> Todos = new[] { Percentual, ValorFixo, FreteGratis };
}
