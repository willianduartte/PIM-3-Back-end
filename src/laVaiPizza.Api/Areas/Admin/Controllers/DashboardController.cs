using System.Collections;
using laVaiPizza.Api.Infrastructure;
using laVaiPizza.Application.Exceptions;
using laVaiPizza.Application.Relatorios;
using laVaiPizza.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace laVaiPizza.Api.Areas.Admin.Controllers;

/// <summary>Dashboard com cartoes e graficos (Chart.js). Somente Gerente. Os dados vem das procedures do banco.</summary>
[Route("admin/dashboard")]
[Authorize(Roles = Cargos.Gerente)]
public class DashboardController : AdminControllerBase
{
    private readonly RelatorioService _relatorios;

    public DashboardController(RelatorioService relatorios)
    {
        _relatorios = relatorios;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        // Os cartoes ja vem preenchidos na pagina; depois o JavaScript atualiza a cada 10 s.
        return View(await _relatorios.ResumoAsync(ct));
    }

    [HttpGet("resumo")]
    public async Task<IActionResult> Resumo(CancellationToken ct)
        => Json(await _relatorios.ResumoAsync(ct));

    [HttpGet("dados/{nome}")]
    public async Task<IActionResult> Dados(
        string nome, int? ano, int? dias, int? top, DateOnly? inicio, DateOnly? fim, CancellationToken ct)
    {
        try
        {
            var resultado = await BuscarAsync(nome, ano, dias, top, inicio, fim, ct);
            return resultado is null ? NotFound(new { erro = "Relatório inexistente." }) : Json(resultado.Value.Dados);
        }
        catch (NegocioException erro)
        {
            return BadRequest(new { erro = erro.Message });
        }
    }

    [HttpGet("csv/{nome}")]
    public async Task<IActionResult> Csv(
        string nome, int? ano, int? dias, int? top, DateOnly? inicio, DateOnly? fim, CancellationToken ct)
    {
        try
        {
            var resultado = await BuscarAsync(nome, ano, dias, top, inicio, fim, ct);
            if (resultado is null) return NotFound();

            var arquivo = $"{nome}-{DateTime.Now:yyyyMMdd-HHmm}.csv";
            return File(CsvWriter.Gerar(resultado.Value.Dados, resultado.Value.Tipo), "text/csv; charset=utf-8", arquivo);
        }
        catch (NegocioException erro)
        {
            return BadRequest(new { erro = erro.Message });
        }
    }

    private async Task<(IEnumerable Dados, Type Tipo)?> BuscarAsync(
        string nome, int? ano, int? dias, int? top, DateOnly? inicio, DateOnly? fim, CancellationToken ct)
    {
        switch (nome)
        {
            case "faturamento-mensal":
                return (await _relatorios.FaturamentoMensalAsync(ano, ct), typeof(FaturamentoMensal));
            case "faturamento-diario":
                return (await _relatorios.FaturamentoDiarioAsync(dias ?? 30, ct), typeof(FaturamentoDiario));
            case "ranking-pizzas":
                return (await _relatorios.RankingPizzasAsync(inicio, fim, top ?? 10, ct), typeof(RankingPizza));
            case "vendas-por-tamanho":
                return (await _relatorios.VendasPorTamanhoAsync(inicio, fim, ct), typeof(VendaPorTamanho));
            case "pedidos-por-status":
                return (await _relatorios.PedidosPorStatusAsync(inicio, fim, ct), typeof(PedidosPorStatus));
            case "horarios-pico":
                return (await _relatorios.HorariosPicoAsync(dias ?? 30, ct), typeof(HorarioPico));
            case "vendas-por-pizza":
                return (await _relatorios.VendasPorPizzaAsync(ct), typeof(VendaPorPizza));
            default:
                return null;
        }
    }
}
