namespace laVaiPizza.Application.Pizzas;
public record CreatePizzaRequest(
    string Nome,
    string Tamanho,
    decimal Preco
);
public record PizzaResponse(
    int Id,
    string Nome,
    string Tamanho,
    decimal Preco
);