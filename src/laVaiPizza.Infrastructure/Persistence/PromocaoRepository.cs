using laVaiPizza.Application.Abstractions;
using laVaiPizza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace laVaiPizza.Infrastructure.Persistence;

public class PromocaoRepository : IPromocaoRepository
{
    private readonly AppDbContext _context;

    public PromocaoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Promocao>> GetVigentesAsync(DateTime agora, CancellationToken cancellationToken = default)
    {
        return await _context.Promocoes
            .AsNoTracking()
            .Where(p => p.Ativa && p.Inicio <= agora && p.Fim >= agora)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Promocao>> GetTodasAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Promocoes
            .AsNoTracking()
            .Include(p => p.Pizza)
            .OrderByDescending(p => p.Fim).ThenBy(p => p.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<Promocao?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _context.Promocoes
            .Include(p => p.Pizza)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public void Adicionar(Promocao promocao) => _context.Promocoes.Add(promocao);

    public void Remover(Promocao promocao) => _context.Promocoes.Remove(promocao);

    public Task SalvarAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
}
