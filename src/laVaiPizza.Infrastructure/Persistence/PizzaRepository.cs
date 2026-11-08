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

    public async Task<IReadOnlyList<Pizza>> GetAtivasComPrecosAsync(CancellationToken cancellationToken = default)
    {
        // AsNoTracking: consulta somente leitura, sem custo de rastreamento do EF.
        return await _context.Pizzas
            .AsNoTracking()
            .Include(p => p.Precos)
            .Where(p => p.Ativa)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Pizza>> GetAtivasComPrecosPorIdsAsync(
        IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default)
    {
        return await _context.Pizzas
            .AsNoTracking()
            .Include(p => p.Precos)
            .Where(p => p.Ativa && ids.Contains(p.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Pizza>> GetTodasComPrecosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Pizzas
            .AsNoTracking()
            .Include(p => p.Precos)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<Pizza?> GetByIdParaAtualizacaoAsync(int id, CancellationToken cancellationToken = default)
    {
        return _context.Pizzas
            .Include(p => p.Precos)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public Task<bool> ExisteAsync(int id, CancellationToken cancellationToken = default)
    {
        return _context.Pizzas.AsNoTracking().AnyAsync(p => p.Id == id, cancellationToken);
    }

    public Task<bool> NomeExisteAsync(string nome, int? ignorarId, CancellationToken cancellationToken = default)
    {
        // O banco compara sem diferenciar maiusculas/minusculas (collation padrao do SQL Server).
        return _context.Pizzas.AsNoTracking()
            .AnyAsync(p => p.Nome == nome && (ignorarId == null || p.Id != ignorarId), cancellationToken);
    }

    public void Adicionar(Pizza pizza) => _context.Pizzas.Add(pizza);

    public Task SalvarAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);

    public async Task<IReadOnlyList<HistoricoPreco>> GetHistoricoPrecosAsync(
        int? pizzaId, int maximo, CancellationToken cancellationToken = default)
    {
        return await _context.HistoricoPrecos
            .AsNoTracking()
            .Include(h => h.Pizza)
            .Include(h => h.AlteradoPorFuncionario)
            .Where(h => pizzaId == null || h.PizzaId == pizzaId)
            .OrderByDescending(h => h.AlteradoEm).ThenByDescending(h => h.Id)
            .Take(maximo)
            .ToListAsync(cancellationToken);
    }
}
