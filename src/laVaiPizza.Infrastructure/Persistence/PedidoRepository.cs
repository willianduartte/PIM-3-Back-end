using laVaiPizza.Application.Abstractions;
using laVaiPizza.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace laVaiPizza.Infrastructure.Persistence;

public class PedidoRepository : IPedidoRepository
{
    private readonly AppDbContext _context;

    public PedidoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Pedido>> GetAllAsync()
    {
        return await _context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.PedidoPizzas)
            .ToListAsync();
    }

    public async Task<Pedido?> GetByIdAsync(int id)
    {
        return await _context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.PedidoPizzas)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task AddAsync(Pedido pedido)
    {
        await _context.Pedidos.AddAsync(pedido);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateStatusAsync(int id, string status)
    {
        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido != null)
        {
            pedido.Status = status;
            await _context.SaveChangesAsync();
        }
    }
}