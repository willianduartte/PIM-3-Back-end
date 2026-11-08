using laVaiPizza.Application.Pizzas;
using Microsoft.AspNetCore.Mvc;

namespace laVaiPizza.Api.Controllers;

/// <summary>Cardapio publico (nao exige login).</summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class PizzasController : ControllerBase
{
    private readonly PizzaService _pizzaService;

    public PizzasController(PizzaService pizzaService)
    {
        _pizzaService = pizzaService;
    }

    /// <summary>Lista as pizzas ativas com precos por tamanho e promocao aplicada.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PizzaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PizzaResponse>>> Get(CancellationToken cancellationToken)
    {
        var cardapio = await _pizzaService.GetCardapioAsync(cancellationToken);
        return Ok(cardapio);
    }

    /// <summary>Busca uma pizza ativa pelo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PizzaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PizzaResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var pizza = await _pizzaService.GetPizzaAsync(id, cancellationToken);
        return pizza is null ? NotFound() : Ok(pizza);
    }
}
