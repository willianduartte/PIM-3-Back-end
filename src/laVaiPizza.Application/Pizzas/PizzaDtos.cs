namespace laVaiPizza.Application.Pizzas;

/// <summary>Promocao especifica de uma pizza, para exibir o selo no cardapio.</summary>
public record PromocaoResumoResponse(int Id, string Nome, string Tipo, decimal Valor);

/// <summary>Preco de um tamanho. PrecoPromocional so vem preenchido quando ha promocao.</summary>
public record PizzaPrecoResponse(string Tamanho, decimal Preco, decimal? PrecoPromocional);

/// <summary>Pizza do cardapio com todos os tamanhos (Pequena, Media, Grande).</summary>
public record PizzaResponse(
    int Id,
    string Nome,
    string Descricao,
    string? ImagemUrl,
    IReadOnlyList<PizzaPrecoResponse> Precos,
    PromocaoResumoResponse? Promocao);
