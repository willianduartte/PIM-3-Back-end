using laVaiPizza.Domain.Entities;

namespace laVaiPizza.Application.Abstractions;

/// <summary>Contrato de acesso a dados de pizzas (equivalente ao DAO das aulas).</summary>
public interface IPizzaRepository
{
    /// <summary>Pizzas ativas com seus precos por tamanho (somente leitura).</summary>
    Task<IReadOnlyList<Pizza>> GetAtivasComPrecosAsync(CancellationToken cancellationToken = default);

    /// <summary>Pizzas ativas (com precos) entre os ids informados; ids inativos/inexistentes nao voltam.</summary>
    Task<IReadOnlyList<Pizza>> GetAtivasComPrecosPorIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default);

    // ---- Area administrativa ----

    /// <summary>Todas as pizzas, ativas ou nao, com precos (somente leitura).</summary>
    Task<IReadOnlyList<Pizza>> GetTodasComPrecosAsync(CancellationToken cancellationToken = default);

    /// <summary>Pizza com precos, rastreada pelo EF para ser alterada e salva.</summary>
    Task<Pizza?> GetByIdParaAtualizacaoAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> ExisteAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> NomeExisteAsync(string nome, int? ignorarId, CancellationToken cancellationToken = default);

    void Adicionar(Pizza pizza);

    Task SalvarAsync(CancellationToken cancellationToken = default);

    /// <summary>Historico de alteracoes de preco (mais recentes primeiro), de uma pizza ou de todas.</summary>
    Task<IReadOnlyList<HistoricoPreco>> GetHistoricoPrecosAsync(int? pizzaId, int maximo, CancellationToken cancellationToken = default);
}
