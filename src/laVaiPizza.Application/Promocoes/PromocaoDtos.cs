namespace laVaiPizza.Application.Promocoes;

/// <summary>Promocao vigente. PizzaId nulo = vale para o pedido todo.</summary>
public record PromocaoResponse(
    int Id,
    string Nome,
    string Tipo,
    decimal Valor,
    decimal? ValorMinimo,
    int? PizzaId,
    DateTime Inicio,
    DateTime Fim);
