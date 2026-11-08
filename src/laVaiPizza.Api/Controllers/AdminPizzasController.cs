using laVaiPizza.Api.Security;
using laVaiPizza.Application.Pizzas;
using laVaiPizza.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace laVaiPizza.Api.Controllers;

/// <summary>Cadastro de pizzas, precos e fotos. Somente o Gerente.</summary>
[ApiController]
[Route("api/v1/admin/pizzas")]
[Produces("application/json")]
[Authorize(Roles = Cargos.Gerente)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class AdminPizzasController : ControllerBase
{
    private readonly PizzaAdminService _service;

    public AdminPizzasController(PizzaAdminService service)
    {
        _service = service;
    }

    /// <summary>Todas as pizzas, inclusive as desativadas.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PizzaAdminResponse>>> Listar(CancellationToken ct)
        => Ok(await _service.ListarAsync(ct));

    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PizzaAdminResponse>> Obter(int id, CancellationToken ct)
        => Ok(await _service.ObterAsync(id, ct));

    /// <summary>Cadastra uma pizza com os precos dos tres tamanhos.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PizzaAdminResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PizzaAdminResponse>> Criar(PizzaCriarRequest request, CancellationToken ct)
    {
        var pizza = await _service.CriarAsync(request, User.GetFuncionarioId(), ct);
        return CreatedAtAction(nameof(Obter), new { id = pizza.Id }, pizza);
    }

    /// <summary>Altera nome e descricao.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PizzaAdminResponse>> Atualizar(int id, PizzaAtualizarRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarAsync(id, request, ct));

    /// <summary>Altera o preco de um tamanho (Pequena, Media ou Grande). O historico e gravado pelo banco.</summary>
    [HttpPut("{id:int}/precos/{tamanho}")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PizzaAdminResponse>> AtualizarPreco(
        int id, string tamanho, PrecoAtualizarRequest request, CancellationToken ct)
        => Ok(await _service.AtualizarPrecoAsync(id, tamanho, request.Preco, User.GetFuncionarioId(), ct));

    /// <summary>
    /// "Exclui" a pizza do cardapio: ela e desativada, nunca apagada, para manter o historico de pedidos.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PizzaAdminResponse>> Desativar(int id, CancellationToken ct)
        => Ok(await _service.DesativarAsync(id, ct));

    /// <summary>Volta a pizza para o cardapio.</summary>
    [HttpPost("{id:int}/reativar")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PizzaAdminResponse>> Reativar(int id, CancellationToken ct)
        => Ok(await _service.ReativarAsync(id, ct));

    /// <summary>Envia a foto da pizza (PNG, JPG ou WEBP, ate 2 MB) no campo "arquivo".</summary>
    [HttpPost("{id:int}/imagem")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(3_000_000)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PizzaAdminResponse>> EnviarImagem(int id, IFormFile arquivo, CancellationToken ct)
    {
        await using var stream = arquivo.OpenReadStream();
        return Ok(await _service.AtualizarImagemAsync(id, stream, arquivo.Length, ct));
    }

    /// <summary>Historico de alteracoes de preco (todas as pizzas ou so uma), mais recentes primeiro.</summary>
    [HttpGet("historico-precos")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<HistoricoPrecoResponse>>> HistoricoPrecos(
        [FromQuery] int? pizzaId, [FromQuery] int maximo = 100, CancellationToken ct = default)
        => Ok(await _service.HistoricoPrecosAsync(pizzaId, maximo, ct));
}
