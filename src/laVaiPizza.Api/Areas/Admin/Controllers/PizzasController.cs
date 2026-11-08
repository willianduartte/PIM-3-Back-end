using laVaiPizza.Api.Areas.Admin.Models;
using laVaiPizza.Api.Infrastructure;
using laVaiPizza.Api.Security;
using laVaiPizza.Application.Exceptions;
using laVaiPizza.Application.Pizzas;
using laVaiPizza.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace laVaiPizza.Api.Areas.Admin.Controllers;

/// <summary>Cadastro de pizzas, precos, fotos e historico de precos. Somente Gerente.</summary>
[Route("admin/pizzas")]
[Authorize(Roles = Cargos.Gerente)]
public class PizzasController : AdminControllerBase
{
    private readonly PizzaAdminService _service;

    public PizzasController(PizzaAdminService service)
    {
        _service = service;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
        => View(await _service.ListarAsync(ct));

    // ------------------------------------------------------------ nova

    [HttpGet("nova")]
    public IActionResult Nova() => View(new PizzaNovaViewModel());

    [HttpPost("nova")]
    public async Task<IActionResult> Nova(PizzaNovaViewModel modelo, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(modelo);

        try
        {
            var pizza = await _service.CriarAsync(
                new PizzaCriarRequest(modelo.Nome, modelo.Descricao,
                    modelo.PrecoPequena!.Value, modelo.PrecoMedia!.Value, modelo.PrecoGrande!.Value),
                User.GetFuncionarioId(), ct);

            Sucesso($"Pizza \"{pizza.Nome}\" cadastrada. Agora você pode enviar a foto.");
            return RedirectToAction(nameof(Editar), new { id = pizza.Id });
        }
        catch (NegocioException erro)
        {
            ModelState.AddModelError(string.Empty, erro.Message);
            return View(modelo);
        }
    }

    // ------------------------------------------------------------ editar

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        try
        {
            var pizza = await _service.ObterAsync(id, ct);
            return View(new PizzaEditarViewModel
            {
                Pizza = pizza,
                Dados = new PizzaDadosViewModel { Nome = pizza.Nome, Descricao = pizza.Descricao }
            });
        }
        catch (NaoEncontradoException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:int}")]
    public async Task<IActionResult> Editar(int id, [Bind(Prefix = "Dados")] PizzaDadosViewModel dados, CancellationToken ct)
    {
        try
        {
            if (ModelState.IsValid)
            {
                await _service.AtualizarAsync(id, new PizzaAtualizarRequest(dados.Nome, dados.Descricao), ct);
                Sucesso("Dados da pizza salvos.");
                return RedirectToAction(nameof(Editar), new { id });
            }
        }
        catch (NegocioException erro)
        {
            ModelState.AddModelError(string.Empty, erro.Message);
        }
        catch (NaoEncontradoException)
        {
            return NotFound();
        }

        // Erro de validacao: mostra o formulario de novo com o que foi digitado
        return View(new PizzaEditarViewModel { Pizza = await _service.ObterAsync(id, ct), Dados = dados });
    }

    [HttpPost("{id:int}/preco")]
    public async Task<IActionResult> AlterarPreco(int id, string tamanho, decimal preco, CancellationToken ct)
    {
        try
        {
            if (preco <= 0 || preco > 9999.99m)
                throw new NegocioException("O preço deve ser maior que zero e no máximo R$ 9.999,99.");

            await _service.AtualizarPrecoAsync(id, tamanho, preco, User.GetFuncionarioId(), ct);
            Sucesso($"Preço da {tamanho} atualizado para {Formatos.Moeda(preco)}.");
        }
        catch (NegocioException erro)
        {
            Erro(erro.Message);
        }
        catch (NaoEncontradoException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Editar), new { id });
    }

    [HttpPost("{id:int}/imagem")]
    [RequestSizeLimit(3_000_000)]
    public async Task<IActionResult> EnviarImagem(int id, IFormFile? arquivo, CancellationToken ct)
    {
        try
        {
            if (arquivo is null || arquivo.Length == 0)
                throw new NegocioException("Escolha uma imagem para enviar.");

            await using var stream = arquivo.OpenReadStream();
            await _service.AtualizarImagemAsync(id, stream, arquivo.Length, ct);
            Sucesso("Foto atualizada.");
        }
        catch (NegocioException erro)
        {
            Erro(erro.Message);
        }
        catch (NaoEncontradoException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Editar), new { id });
    }

    [HttpPost("{id:int}/desativar")]
    public async Task<IActionResult> Desativar(int id, CancellationToken ct)
    {
        try
        {
            var pizza = await _service.DesativarAsync(id, ct);
            Sucesso($"\"{pizza.Nome}\" saiu do cardápio. O histórico de pedidos foi mantido.");
        }
        catch (NaoEncontradoException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("{id:int}/reativar")]
    public async Task<IActionResult> Reativar(int id, CancellationToken ct)
    {
        try
        {
            var pizza = await _service.ReativarAsync(id, ct);
            Sucesso($"\"{pizza.Nome}\" voltou para o cardápio.");
        }
        catch (NaoEncontradoException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    // ------------------------------------------------------------ historico de precos

    [HttpGet("historico")]
    public async Task<IActionResult> Historico(int? pizzaId, CancellationToken ct)
    {
        try
        {
            return View(new HistoricoPrecosViewModel
            {
                PizzaId = pizzaId,
                Pizzas = await _service.ListarAsync(ct),
                Itens = await _service.HistoricoPrecosAsync(pizzaId, 200, ct)
            });
        }
        catch (NaoEncontradoException)
        {
            return NotFound();
        }
    }

    [HttpGet("historico.csv")]
    public async Task<IActionResult> HistoricoCsv(int? pizzaId, CancellationToken ct)
    {
        try
        {
            var itens = await _service.HistoricoPrecosAsync(pizzaId, 500, ct);
            return File(CsvWriter.Gerar(itens), "text/csv; charset=utf-8", $"historico-precos-{DateTime.Now:yyyyMMdd-HHmm}.csv");
        }
        catch (NaoEncontradoException)
        {
            return NotFound();
        }
    }
}
