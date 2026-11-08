using laVaiPizza.Api.Infrastructure;
using laVaiPizza.Application.Exceptions;
using laVaiPizza.Application.Relatorios;
using laVaiPizza.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace laVaiPizza.Api.Controllers;

/// <summary>
/// Relatorios do dashboard (somente Gerente). Cada rota executa uma stored procedure do banco.
/// Com <c>?formato=csv</c> a resposta vira um arquivo CSV para baixar; o padrao e JSON.
/// </summary>
[ApiController]
[Route("api/v1/admin/relatorios")]
[Produces("application/json")]
[Authorize(Roles = Cargos.Gerente)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class AdminRelatoriosController : ControllerBase
{
    private readonly RelatorioService _service;

    public AdminRelatoriosController(RelatorioService service)
    {
        _service = service;
    }

    /// <summary>Cartoes do dashboard: pedidos e faturamento de hoje e do mes, ticket medio, em aberto, tempo medio.</summary>
    [HttpGet("resumo")]
    public async Task<IActionResult> Resumo([FromQuery] string? formato, CancellationToken ct)
        => Responder(new[] { await _service.ResumoAsync(ct) }, formato, "resumo", unico: true);

    /// <summary>Faturamento dos 12 meses de um ano (padrao: ano atual).</summary>
    [HttpGet("faturamento-mensal")]
    public async Task<IActionResult> FaturamentoMensal([FromQuery] int? ano, [FromQuery] string? formato, CancellationToken ct)
        => Responder(await _service.FaturamentoMensalAsync(ano, ct), formato, "faturamento-mensal");

    /// <summary>Faturamento dia a dia dos ultimos N dias (padrao 30).</summary>
    [HttpGet("faturamento-diario")]
    public async Task<IActionResult> FaturamentoDiario([FromQuery] int dias = 30, [FromQuery] string? formato = null, CancellationToken ct = default)
        => Responder(await _service.FaturamentoDiarioAsync(dias, ct), formato, "faturamento-diario");

    /// <summary>Pizzas mais vendidas, com periodo opcional (inicio/fim no formato aaaa-mm-dd).</summary>
    [HttpGet("ranking-pizzas")]
    public async Task<IActionResult> RankingPizzas(
        [FromQuery] DateOnly? inicio, [FromQuery] DateOnly? fim, [FromQuery] int top = 10,
        [FromQuery] string? formato = null, CancellationToken ct = default)
        => Responder(await _service.RankingPizzasAsync(inicio, fim, top, ct), formato, "ranking-pizzas");

    /// <summary>Vendas por tamanho (Pequena, Media, Grande), com periodo opcional.</summary>
    [HttpGet("vendas-por-tamanho")]
    public async Task<IActionResult> VendasPorTamanho(
        [FromQuery] DateOnly? inicio, [FromQuery] DateOnly? fim, [FromQuery] string? formato = null, CancellationToken ct = default)
        => Responder(await _service.VendasPorTamanhoAsync(inicio, fim, ct), formato, "vendas-por-tamanho");

    /// <summary>Quantidade de pedidos em cada status, com periodo opcional.</summary>
    [HttpGet("pedidos-por-status")]
    public async Task<IActionResult> PedidosPorStatus(
        [FromQuery] DateOnly? inicio, [FromQuery] DateOnly? fim, [FromQuery] string? formato = null, CancellationToken ct = default)
        => Responder(await _service.PedidosPorStatusAsync(inicio, fim, ct), formato, "pedidos-por-status");

    /// <summary>Pedidos por hora do dia nos ultimos N dias (padrao 30): mostra os horarios de pico.</summary>
    [HttpGet("horarios-pico")]
    public async Task<IActionResult> HorariosPico([FromQuery] int dias = 30, [FromQuery] string? formato = null, CancellationToken ct = default)
        => Responder(await _service.HorariosPicoAsync(dias, ct), formato, "horarios-pico");

    /// <summary>Vendas por pizza e tamanho (usa a view RelatorioVendasPorPizza).</summary>
    [HttpGet("vendas-por-pizza")]
    public async Task<IActionResult> VendasPorPizza([FromQuery] string? formato = null, CancellationToken ct = default)
        => Responder(await _service.VendasPorPizzaAsync(ct), formato, "vendas-por-pizza");

    private IActionResult Responder<T>(IReadOnlyList<T> dados, string? formato, string nomeArquivo, bool unico = false)
    {
        switch (formato?.Trim().ToLowerInvariant())
        {
            case null or "" or "json":
                return unico ? Ok(dados[0]) : Ok(dados);

            case "csv":
                var arquivo = $"{nomeArquivo}-{DateTime.Now:yyyyMMdd-HHmm}.csv";
                return File(CsvWriter.Gerar(dados), "text/csv; charset=utf-8", arquivo);

            default:
                throw new NegocioException("Formato inválido. Use json ou csv.");
        }
    }
}
