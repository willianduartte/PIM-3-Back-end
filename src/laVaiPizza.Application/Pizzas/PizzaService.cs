using laVaiPizza.Application.Abstractions;
using laVaiPizza.Domain.Entities;

namespace laVaiPizza.Application.Pizzas;

public class PizzaService
{
    private readonly IPizzaRepository _pizzaRepository;

    public PizzaService(IPizzaRepository pizzaRepository)
    {
        _pizzaRepository = pizzaRepository;
    }

    public async Task<IEnumerable<PizzaResponse>> GetPizzasAsync()
    {
        var pizzas = await _pizzaRepository.GetAllAsync();

        return pizzas.Select(p => new PizzaResponse(
            p.Id,
            p.Nome,
            p.Tamanho,
            p.Preco));
    }

    public async Task<PizzaResponse?> GetPizzaByIdAsync(int id)
    {
        var p = await _pizzaRepository.GetByIdAsync(id);
        if (p == null) return null;

        return new PizzaResponse(p.Id, p.Nome, p.Tamanho, p.Preco);
    }

    public async Task<PizzaResponse> CreatePizzaAsync(CreatePizzaRequest request)
    {
        var pizza = new Pizza
        {
            Nome = request.Nome,
            Tamanho = request.Tamanho,
            Preco = request.Preco
        };

        await _pizzaRepository.AddAsync(pizza);

        return new PizzaResponse(pizza.Id, pizza.Nome, pizza.Tamanho, pizza.Preco);
    }

    public async Task DeletePizzaAsync(int id)
    {
        await _pizzaRepository.DeleteAsync(id);
    }
}