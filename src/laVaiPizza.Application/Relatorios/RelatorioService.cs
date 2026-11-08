using laVaiPizza.Application.Abstractions;
using laVaiPizza.Application.Exceptions;

namespace laVaiPizza.Application.Relatorios;

/// <summary>Valida os filtros dos relatorios e chama o repositorio (procedures).</summary>
public class RelatorioService
{
    private readonly IRelatorioRepository _repository;

    public RelatorioService(IRelatorioRepository repository)
    {
        _repository = repository;
    }

    public Task<ResumoDashboard> ResumoAsync(CancellationToken ct = default)
        => _repository.ObterResumoAsync(ct);

    public Task<IReadOnlyList<FaturamentoMensal>> FaturamentoMensalAsync(int? ano, CancellationToken ct = default)
    {
        if (ano is < 2000 or > 2100)
            throw new NegocioException("Informe um ano entre 2000 e 2100.");
        return _repository.ObterFaturamentoMensalAsync(ano, ct);
    }

    public Task<IReadOnlyList<FaturamentoDiario>> FaturamentoDiarioAsync(int dias, CancellationToken ct = default)
    {
        if (dias is < 1 or > 366)
            throw new NegocioException("Informe de 1 a 366 dias.");
        return _repository.ObterFaturamentoDiarioAsync(dias, ct);
    }

    public Task<IReadOnlyList<RankingPizza>> RankingPizzasAsync(DateOnly? inicio, DateOnly? fim, int top, CancellationToken ct = default)
    {
        ValidarPeriodo(inicio, fim);
        if (top is < 1 or > 50)
            throw new NegocioException("O top deve ficar entre 1 e 50.");
        return _repository.ObterRankingPizzasAsync(inicio, fim, top, ct);
    }

    public Task<IReadOnlyList<VendaPorTamanho>> VendasPorTamanhoAsync(DateOnly? inicio, DateOnly? fim, CancellationToken ct = default)
    {
        ValidarPeriodo(inicio, fim);
        return _repository.ObterVendasPorTamanhoAsync(inicio, fim, ct);
    }

    public Task<IReadOnlyList<PedidosPorStatus>> PedidosPorStatusAsync(DateOnly? inicio, DateOnly? fim, CancellationToken ct = default)
    {
        ValidarPeriodo(inicio, fim);
        return _repository.ObterPedidosPorStatusAsync(inicio, fim, ct);
    }

    public Task<IReadOnlyList<HorarioPico>> HorariosPicoAsync(int dias, CancellationToken ct = default)
    {
        if (dias is < 1 or > 366)
            throw new NegocioException("Informe de 1 a 366 dias.");
        return _repository.ObterHorariosPicoAsync(dias, ct);
    }

    public Task<IReadOnlyList<VendaPorPizza>> VendasPorPizzaAsync(CancellationToken ct = default)
        => _repository.ObterVendasPorPizzaAsync(ct);

    private static void ValidarPeriodo(DateOnly? inicio, DateOnly? fim)
    {
        if (inicio is not null && fim is not null && fim < inicio)
            throw new NegocioException("A data final deve ser igual ou posterior à inicial.");
    }
}
