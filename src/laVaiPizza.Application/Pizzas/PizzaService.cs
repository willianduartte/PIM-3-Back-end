using laVaiPizza.Application.Abstractions;
using laVaiPizza.Domain.Constants;
using laVaiPizza.Domain.Entities;
using laVaiPizza.Domain.Services;
using Microsoft.Extensions.Caching.Memory;

namespace laVaiPizza.Application.Pizzas;

/// <summary>
/// Regras do cardapio. O resultado fica em IMemoryCache e e invalidado quando o
/// admin cria/altera/desativa pizza, preco ou promocao (InvalidarCardapio).
/// </summary>
public class PizzaService
{
    private const string CacheKeyCardapio = "cardapio";
    private static readonly TimeSpan DuracaoCache = TimeSpan.FromMinutes(5);

    private readonly IPizzaRepository _pizzaRepository;
    private readonly IPromocaoRepository _promocaoRepository;
    private readonly IMemoryCache _cache;

    public PizzaService(
        IPizzaRepository pizzaRepository,
        IPromocaoRepository promocaoRepository,
        IMemoryCache cache)
    {
        _pizzaRepository = pizzaRepository;
        _promocaoRepository = promocaoRepository;
        _cache = cache;
    }

    public async Task<IReadOnlyList<PizzaResponse>> GetCardapioAsync(CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(CacheKeyCardapio, out IReadOnlyList<PizzaResponse>? emCache) && emCache is not null)
            return emCache;

        var pizzas = await _pizzaRepository.GetAtivasComPrecosAsync(cancellationToken);
        var promocoes = await _promocaoRepository.GetVigentesAsync(DateTime.Now, cancellationToken);

        IReadOnlyList<PizzaResponse> cardapio = pizzas
            .Select(pizza => Mapear(pizza, promocoes))
            .ToList();

        _cache.Set(CacheKeyCardapio, cardapio, DuracaoCache);
        return cardapio;
    }

    public async Task<PizzaResponse?> GetPizzaAsync(int id, CancellationToken cancellationToken = default)
    {
        var cardapio = await GetCardapioAsync(cancellationToken);
        return cardapio.FirstOrDefault(p => p.Id == id);
    }

    /// <summary>Descarta o cache para o proximo GET ler o banco de novo.</summary>
    public void InvalidarCardapio() => _cache.Remove(CacheKeyCardapio);

    private static PizzaResponse Mapear(Pizza pizza, IReadOnlyList<Promocao> promocoes)
    {
        // A mesma regra do pedido (SelecaoDePromocoes): o preco exibido e o preco cobrado.
        var precos = pizza.Precos
            .OrderBy(p => TamanhosPizza.Ordem(p.Tamanho))
            .Select(p =>
            {
                var (_, desconto) = SelecaoDePromocoes.MelhorParaPizza(promocoes, pizza.Id, p.Preco);
                decimal? precoPromocional = desconto > 0 ? p.Preco - desconto : null;
                return new PizzaPrecoResponse(p.Tamanho, p.Preco, precoPromocional);
            })
            .ToList();

        // Promocao em destaque (selo): a que mais desconta no maior preco da pizza.
        var maiorPreco = pizza.Precos.Select(p => p.Preco).DefaultIfEmpty(0m).Max();
        var (destaque, _) = SelecaoDePromocoes.MelhorParaPizza(promocoes, pizza.Id, maiorPreco);

        return new PizzaResponse(
            pizza.Id,
            pizza.Nome,
            pizza.Descricao,
            pizza.ImagemUrl,
            precos,
            destaque is null ? null : new PromocaoResumoResponse(destaque.Id, destaque.Nome, destaque.Tipo, destaque.Valor));
    }
}
