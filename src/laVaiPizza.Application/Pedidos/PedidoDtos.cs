namespace laVaiPizza.Application.Pedidos;
public record PedidoItemRequest(int PizzaId, int Quantidade);

public record PedidoRequest(
    int ClienteId,
    List<PedidoItemRequest> Itens,
    decimal ValorTotal,
    string Endereco,
    int? TempoEstimado,
    int? FuncionarioPreparaId,
    int? FuncionarioEntregaId
);

public record PedidoResponse(
    int Id,
    string Status,
    decimal ValorTotal,
    string Endereco,
    int? TempoEstimado,
    DateTime DataHora
);