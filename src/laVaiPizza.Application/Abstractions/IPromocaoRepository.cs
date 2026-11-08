using laVaiPizza.Domain.Entities;

namespace laVaiPizza.Application.Abstractions;

public interface IPromocaoRepository
{
    /// <summary>Promocoes ativas e dentro do periodo de vigencia no instante informado.</summary>
    Task<IReadOnlyList<Promocao>> GetVigentesAsync(DateTime agora, CancellationToken cancellationToken = default);

    // ---- Area administrativa ----

    /// <summary>Todas as promocoes (com a pizza), somente leitura.</summary>
    Task<IReadOnlyList<Promocao>> GetTodasAsync(CancellationToken cancellationToken = default);

    /// <summary>Promocao (com a pizza) rastreada pelo EF para ser alterada.</summary>
    Task<Promocao?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    void Adicionar(Promocao promocao);

    void Remover(Promocao promocao);

    Task SalvarAsync(CancellationToken cancellationToken = default);
}
