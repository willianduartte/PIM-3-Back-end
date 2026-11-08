namespace laVaiPizza.Application.Relatorios;

// Cada record tem o mesmo nome das colunas devolvidas pela procedure/view correspondente.

public record ResumoDashboard(
    int PedidosHoje,
    decimal FaturamentoHoje,
    int PedidosMes,
    decimal FaturamentoMes,
    decimal TicketMedioMes,
    int PedidosEmAberto,
    decimal? TempoMedioEntregaMin);

public record FaturamentoMensal(int Mes, int QuantidadePedidos, decimal Faturamento, decimal TicketMedio);

public record FaturamentoDiario(DateOnly Dia, int QuantidadePedidos, decimal Faturamento);

public record RankingPizza(int PizzaId, string Pizza, int QuantidadeVendida, decimal Faturamento);

public record VendaPorTamanho(string Tamanho, int QuantidadeVendida, decimal Faturamento);

public record PedidosPorStatus(string Status, int Quantidade);

public record HorarioPico(int Hora, int QuantidadePedidos);

/// <summary>Linha da view RelatorioVendasPorPizza (pizza x tamanho).</summary>
public record VendaPorPizza(int PizzaId, string Pizza, string Tamanho, int QuantidadeVendida, decimal Faturamento);
