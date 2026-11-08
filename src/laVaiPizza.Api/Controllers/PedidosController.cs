using laVaiPizza.Application.Pedidos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace laVaiPizza.Api.Controllers;

/// <summary>Pedidos feitos pelo cliente, sem login. O acompanhamento usa o codigo do pedido.</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class PedidosController : ControllerBase
{
    private readonly PedidoService _pedidoService;

    public PedidosController(PedidoService pedidoService)
    {
        _pedidoService = pedidoService;
    }

    /// <summary>Cria um pedido. O servidor calcula precos, promocoes, taxa e total.</summary>
    [HttpPost]
    [EnableRateLimiting("pedido")]
    [ProducesResponseType(typeof(PedidoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<PedidoResponse>> Criar([FromBody] PedidoRequest request, CancellationToken cancellationToken)
    {
        var pedido = await _pedidoService.CriarAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Acompanhar), new { codigo = pedido.CodigoAcompanhamento }, pedido);
    }

    /// <summary>Calcula o carrinho (desconto, frete e total) sem criar pedido.</summary>
    [HttpPost("simular")]
    [EnableRateLimiting("simular")]
    [ProducesResponseType(typeof(SimulacaoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SimulacaoResponse>> Simular([FromBody] SimulacaoRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _pedidoService.SimularAsync(request, cancellationToken));
    }

    /// <summary>Consulta o andamento do pedido pelo codigo (ex.: LV7K2P9Q).</summary>
    [HttpGet("acompanhar/{codigo}")]
    [EnableRateLimiting("acompanhar")]
    [ProducesResponseType(typeof(AcompanhamentoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AcompanhamentoResponse>> Acompanhar(string codigo, CancellationToken cancellationToken)
    {
        return Ok(await _pedidoService.AcompanharAsync(codigo, cancellationToken));
    }
}

