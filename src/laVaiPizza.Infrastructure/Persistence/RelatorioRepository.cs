using laVaiPizza.Application.Abstractions;
using laVaiPizza.Application.Relatorios;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace laVaiPizza.Infrastructure.Persistence;

/// <summary>
/// Executa as stored procedures do script_banco.sql e mapeia o resultado direto para records
/// (tipos sem chave registrados no AppDbContext, lidos com FromSql). Os valores sempre vao como parametros, nunca concatenados.
/// </summary>
public class RelatorioRepository : IRelatorioRepository
{
    private readonly AppDbContext _context;

    public RelatorioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ResumoDashboard> ObterResumoAsync(CancellationToken ct = default)
    {
        var linhas = await _context.Set<ResumoDashboard>().FromSql($"EXEC ObterResumoDashboard").ToListAsync(ct);
        return linhas.Single();
    }

    public async Task<IReadOnlyList<FaturamentoMensal>> ObterFaturamentoMensalAsync(int? ano, CancellationToken ct = default)
    {
        var pAno = new SqlParameter("@Ano", SqlDbType.Int) { Value = (object?)ano ?? DBNull.Value };
        return await _context.Set<FaturamentoMensal>().FromSqlRaw("EXEC ObterFaturamentoMensal @Ano", pAno)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<FaturamentoDiario>> ObterFaturamentoDiarioAsync(int dias, CancellationToken ct = default)
    {
        return await _context.Set<FaturamentoDiario>().FromSql($"EXEC ObterFaturamentoDiario @Dias = {dias}")
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<RankingPizza>> ObterRankingPizzasAsync(
        DateOnly? inicio, DateOnly? fim, int top, CancellationToken ct = default)
    {
        return await _context.Set<RankingPizza>().FromSqlRaw(
                "EXEC ObterRankingPizzas @DataInicio, @DataFim, @Top",
                Data("@DataInicio", inicio), Data("@DataFim", fim), new SqlParameter("@Top", top))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VendaPorTamanho>> ObterVendasPorTamanhoAsync(
        DateOnly? inicio, DateOnly? fim, CancellationToken ct = default)
    {
        return await _context.Set<VendaPorTamanho>().FromSqlRaw(
                "EXEC ObterVendasPorTamanho @DataInicio, @DataFim",
                Data("@DataInicio", inicio), Data("@DataFim", fim))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PedidosPorStatus>> ObterPedidosPorStatusAsync(
        DateOnly? inicio, DateOnly? fim, CancellationToken ct = default)
    {
        return await _context.Set<PedidosPorStatus>().FromSqlRaw(
                "EXEC ObterPedidosPorStatus @DataInicio, @DataFim",
                Data("@DataInicio", inicio), Data("@DataFim", fim))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<HorarioPico>> ObterHorariosPicoAsync(int dias, CancellationToken ct = default)
    {
        return await _context.Set<HorarioPico>().FromSql($"EXEC ObterHorariosPico @Dias = {dias}")
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VendaPorPizza>> ObterVendasPorPizzaAsync(CancellationToken ct = default)
    {
        return await _context.Set<VendaPorPizza>().FromSql($"SELECT PizzaId, Pizza, Tamanho, QuantidadeVendida, Faturamento FROM RelatorioVendasPorPizza ORDER BY Pizza, Tamanho")
            .ToListAsync(ct);
    }

    private static SqlParameter Data(string nome, DateOnly? valor)
        => new(nome, SqlDbType.Date) { Value = valor is null ? DBNull.Value : valor.Value.ToDateTime(TimeOnly.MinValue) };
}
