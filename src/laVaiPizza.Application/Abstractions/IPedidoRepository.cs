using System.Linq.Expressions;
using laVaiPizza.Domain.Entities;

namespace laVaiPizza.Application.Abstractions;

/// <summary>Contrato de acesso a dados de pedidos (equivalente ao DAO das aulas).</summary>
public interface IPedidoRepository
{
    Task<bool> CodigoExisteAsync(string codigo, CancellationToken cancellationToken = default);

    /// <summary>Grava pedido, cliente e itens numa unica transacao.</summary>
    Task AdicionarAsync(Pedido pedido, CancellationToken cancellationToken = default);

    /// <summary>Pedido com cliente, itens (com pizza) e historico de status, somente leitura.</summary>
    Task<Pedido?> GetByCodigoAsync(string codigo, CancellationToken cancellationToken = default);

    // ---- Area administrativa ----

    /// <summary>Pedidos que atendem ao filtro, com todos os detalhes (somente leitura).</summary>
    Task<IReadOnlyList<Pedido>> GetFilaAsync(
        Expression<Func<Pedido, bool>> filtro, bool maisRecentesPrimeiro, int maximo, CancellationToken cancellationToken = default);

    /// <summary>Pedido com cliente, itens, funcionarios e historico (somente leitura).</summary>
    Task<Pedido?> GetDetalheAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Troca o status somente se o pedido ainda estiver em <paramref name="statusAtual"/>
    /// (um UPDATE condicional, seguro contra duas pessoas clicando ao mesmo tempo).
    /// Devolve false quando alguem ja alterou o pedido.
    /// </summary>
    Task<bool> TentarAtualizarStatusAsync(
        int id, string statusAtual, string novoStatus, int? funcionarioPreparaId, int? funcionarioEntregaId,
        CancellationToken cancellationToken = default);
}
