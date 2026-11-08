namespace laVaiPizza.Application.Abstractions;

/// <summary>
/// Relatorios do dashboard. Cada metodo executa uma stored procedure (ou a view) do banco:
/// o calculo pesado fica no SQL Server e a API so repassa o resultado.
/// </summary>
public interface IRelatorioRepository
{
    Task<Relatorios.ResumoDashboard> ObterResumoAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Relatorios.FaturamentoMensal>> ObterFaturamentoMensalAsync(int? ano, CancellationToken ct = default);
    Task<IReadOnlyList<Relatorios.FaturamentoDiario>> ObterFaturamentoDiarioAsync(int dias, CancellationToken ct = default);
    Task<IReadOnlyList<Relatorios.RankingPizza>> ObterRankingPizzasAsync(DateOnly? inicio, DateOnly? fim, int top, CancellationToken ct = default);
    Task<IReadOnlyList<Relatorios.VendaPorTamanho>> ObterVendasPorTamanhoAsync(DateOnly? inicio, DateOnly? fim, CancellationToken ct = default);
    Task<IReadOnlyList<Relatorios.PedidosPorStatus>> ObterPedidosPorStatusAsync(DateOnly? inicio, DateOnly? fim, CancellationToken ct = default);
    Task<IReadOnlyList<Relatorios.HorarioPico>> ObterHorariosPicoAsync(int dias, CancellationToken ct = default);
    Task<IReadOnlyList<Relatorios.VendaPorPizza>> ObterVendasPorPizzaAsync(CancellationToken ct = default);
}
