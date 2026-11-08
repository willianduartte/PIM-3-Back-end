using laVaiPizza.Application.Abstractions;

namespace laVaiPizza.Application.Promocoes;

public class PromocaoService
{
    private readonly IPromocaoRepository _promocaoRepository;

    public PromocaoService(IPromocaoRepository promocaoRepository)
    {
        _promocaoRepository = promocaoRepository;
    }

    /// <summary>Promocoes vigentes agora (o site usa para banners e para mostrar o preco riscado).</summary>
    public async Task<IReadOnlyList<PromocaoResponse>> GetVigentesAsync(CancellationToken cancellationToken = default)
    {
        var promocoes = await _promocaoRepository.GetVigentesAsync(DateTime.Now, cancellationToken);

        return promocoes
            .Select(p => new PromocaoResponse(p.Id, p.Nome, p.Tipo, p.Valor, p.ValorMinimo, p.PizzaId, p.Inicio, p.Fim))
            .ToList();
    }
}
