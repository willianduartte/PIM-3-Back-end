using laVaiPizza.Domain.Entities;

namespace laVaiPizza.Application.Abstractions;

public interface IPedidoRepository
{
    Task<IEnumerable<Pedido>> GetAllAsync();
    Task<Pedido?> GetByIdAsync(int id);
    Task AddAsync(Pedido pedido);
    Task UpdateStatusAsync(int id, string status);
}