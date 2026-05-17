using laVaiPizza.Application.Abstractions;
using laVaiPizza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace laVaiPizza.Infrastructure.Persistence;

public class PizzaRepository : IPizzaRepository
{
    private readonly AppDbContext _context;

    public PizzaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Pizza>> GetAllAsync()
    {
        return await _context.Pizzas.ToListAsync();
    }

    public async Task<Pizza?> GetByIdAsync(int id)
    {
        return await _context.Pizzas
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(Pizza pizza)
    {
        await _context.Pizzas.AddAsync(pizza);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Pizza pizza)
    {
        _context.Pizzas.Update(pizza);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var pizza = await GetByIdAsync(id);
        if (pizza != null)
        {
            _context.Pizzas.Remove(pizza);
            await _context.SaveChangesAsync();
        }
    }
}