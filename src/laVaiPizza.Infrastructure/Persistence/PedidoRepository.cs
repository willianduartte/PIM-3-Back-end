using System.Linq.Expressions;
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

    public Task<bool> CodigoExisteAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return _context.Pedidos.AsNoTracking().AnyAsync(p => p.CodigoAcompanhamento == codigo, cancellationToken);
    }

    public async Task AdicionarAsync(Pedido pedido, CancellationToken cancellationToken = default)
    {
        // Um unico SaveChanges = uma unica transacao: ou grava tudo ou nada.
        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task<Pedido?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return _context.Pedidos
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Cliente)
            .Include(p => p.Itens).ThenInclude(i => i.Pizza)
            .Include(p => p.HistoricoStatus)
            .FirstOrDefaultAsync(p => p.CodigoAcompanhamento == codigo, cancellationToken);
    }

    public async Task<IReadOnlyList<Pedido>> GetFilaAsync(
        Expression<Func<Pedido, bool>> filtro, bool maisRecentesPrimeiro, int maximo, CancellationToken cancellationToken = default)
    {
        var consulta = ComDetalhes().Where(filtro);

        consulta = maisRecentesPrimeiro
            ? consulta.OrderByDescending(p => p.DataHora)
            : consulta.OrderBy(p => p.DataHora);

        return await consulta.Take(maximo).ToListAsync(cancellationToken);
    }

    public Task<Pedido?> GetDetalheAsync(int id, CancellationToken cancellationToken = default)
    {
        return ComDetalhes().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<bool> TentarAtualizarStatusAsync(
        int id, string statusAtual, string novoStatus, int? funcionarioPreparaId, int? funcionarioEntregaId,
        CancellationToken cancellationToken = default)
    {
        var linhas = await _context.Pedidos
            .Where(p => p.Id == id && p.Status == statusAtual)
            .ExecuteUpdateAsync(set => set
                .SetProperty(p => p.Status, novoStatus)
                .SetProperty(p => p.FuncionarioPreparaId, p => funcionarioPreparaId ?? p.FuncionarioPreparaId)
                .SetProperty(p => p.FuncionarioEntregaId, p => funcionarioEntregaId ?? p.FuncionarioEntregaId),
                cancellationToken);

        return linhas == 1;
    }

    private IQueryable<Pedido> ComDetalhes()
    {
        return _context.Pedidos
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.Cliente)
            .Include(p => p.Itens).ThenInclude(i => i.Pizza)
            .Include(p => p.FuncionarioPrepara)
            .Include(p => p.FuncionarioEntrega)
            .Include(p => p.HistoricoStatus).ThenInclude(h => h.Funcionario);
    }
}
