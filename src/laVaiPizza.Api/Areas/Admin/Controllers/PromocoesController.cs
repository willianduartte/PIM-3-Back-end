using laVaiPizza.Api.Areas.Admin.Models;
using laVaiPizza.Application.Exceptions;
using laVaiPizza.Application.Pizzas;
using laVaiPizza.Application.Promocoes;
using laVaiPizza.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace laVaiPizza.Api.Areas.Admin.Controllers;

/// <summary>Cadastro de promocoes. Somente Gerente.</summary>
[Route("admin/promocoes")]
[Authorize(Roles = Cargos.Gerente)]
public class PromocoesController : AdminControllerBase
{
    private readonly PromocaoAdminService _service;
    private readonly PizzaAdminService _pizzas;

    public PromocoesController(PromocaoAdminService service, PizzaAdminService pizzas)
    {
        _service = service;
        _pizzas = pizzas;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _service.ListarAsync(ct));

    [HttpGet("nova")]
    public async Task<IActionResult> Nova(CancellationToken ct)
    {
        var agora = DateTime.Now;
        return View("Form", await ComPizzas(new PromocaoFormViewModel
        {
            Inicio = new DateTime(agora.Year, agora.Month, agora.Day, 0, 0, 0),
            Fim = new DateTime(agora.Year, agora.Month, agora.Day, 23, 59, 0).AddDays(7)
        }, ct));
    }

    [HttpPost("nova")]
    public async Task<IActionResult> Nova(PromocaoFormViewModel modelo, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var promocao = await _service.CriarAsync(modelo.ToRequest(), ct);
                Sucesso($"Promoção \"{promocao.Nome}\" criada.");
                return RedirectToAction(nameof(Index));
            }
            catch (NegocioException erro)
            {
                ModelState.AddModelError(string.Empty, erro.Message);
            }
        }

        return View("Form", await ComPizzas(modelo, ct));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        try
        {
            var promocao = await _service.ObterAsync(id, ct);
            return View("Form", await ComPizzas(PromocaoFormViewModel.De(promocao), ct));
        }
        catch (NaoEncontradoException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> Editar(int id, PromocaoFormViewModel modelo, CancellationToken ct)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var promocao = await _service.AtualizarAsync(id, modelo.ToRequest(), ct);
                Sucesso($"Promoção \"{promocao.Nome}\" salva.");
                return RedirectToAction(nameof(Index));
            }
            catch (NegocioException erro)
            {
                ModelState.AddModelError(string.Empty, erro.Message);
            }
            catch (NaoEncontradoException)
            {
                return NotFound();
            }
        }

        modelo.Id = id;
        return View("Form", await ComPizzas(modelo, ct));
    }

    [HttpPost("{id:int}/remover")]
    public async Task<IActionResult> Remover(int id, CancellationToken ct)
    {
        try
        {
            await _service.RemoverAsync(id, ct);
            Sucesso("Promoção removida.");
        }
        catch (NaoEncontradoException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<PromocaoFormViewModel> ComPizzas(PromocaoFormViewModel modelo, CancellationToken ct)
    {
        modelo.Pizzas = await _pizzas.ListarAsync(ct);
        return modelo;
    }
}
