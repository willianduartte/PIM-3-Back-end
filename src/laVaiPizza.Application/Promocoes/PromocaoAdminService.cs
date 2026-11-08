using laVaiPizza.Application.Abstractions;
using laVaiPizza.Application.Exceptions;
using laVaiPizza.Application.Pizzas;
using laVaiPizza.Domain.Constants;
using laVaiPizza.Domain.Entities;

namespace laVaiPizza.Application.Promocoes;

/// <summary>CRUD de promocoes (area do Gerente). Toda alteracao descarta o cache do cardapio.</summary>
public class PromocaoAdminService
{
    private readonly IPromocaoRepository _promocaoRepository;
    private readonly IPizzaRepository _pizzaRepository;
    private readonly PizzaService _pizzaService;

    public PromocaoAdminService(
        IPromocaoRepository promocaoRepository,
        IPizzaRepository pizzaRepository,
        PizzaService pizzaService)
    {
        _promocaoRepository = promocaoRepository;
        _pizzaRepository = pizzaRepository;
        _pizzaService = pizzaService;
    }

    public async Task<IReadOnlyList<PromocaoAdminResponse>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var promocoes = await _promocaoRepository.GetTodasAsync(cancellationToken);
        return promocoes.Select(Mapear).ToList();
    }

    public async Task<PromocaoAdminResponse> ObterAsync(int id, CancellationToken cancellationToken = default)
    {
        return Mapear(await BuscarAsync(id, cancellationToken));
    }

    public async Task<PromocaoAdminResponse> CriarAsync(PromocaoRequest request, CancellationToken cancellationToken = default)
    {
        var promocao = new Promocao();
        await PreencherAsync(promocao, request, cancellationToken);

        _promocaoRepository.Adicionar(promocao);
        await _promocaoRepository.SalvarAsync(cancellationToken);
        _pizzaService.InvalidarCardapio();

        // Recarrega para trazer o nome da pizza no retorno
        return Mapear(await BuscarAsync(promocao.Id, cancellationToken));
    }

    public async Task<PromocaoAdminResponse> AtualizarAsync(int id, PromocaoRequest request, CancellationToken cancellationToken = default)
    {
        var promocao = await BuscarAsync(id, cancellationToken);
        await PreencherAsync(promocao, request, cancellationToken);

        await _promocaoRepository.SalvarAsync(cancellationToken);
        _pizzaService.InvalidarCardapio();

        return Mapear(await BuscarAsync(id, cancellationToken));
    }

    /// <summary>Remove a promocao (nenhum pedido aponta para ela: o pedido guarda so os valores).</summary>
    public async Task RemoverAsync(int id, CancellationToken cancellationToken = default)
    {
        var promocao = await BuscarAsync(id, cancellationToken);
        _promocaoRepository.Remover(promocao);
        await _promocaoRepository.SalvarAsync(cancellationToken);
        _pizzaService.InvalidarCardapio();
    }

    // ------------------------------------------------------------------ auxiliares

    private async Task<Promocao> BuscarAsync(int id, CancellationToken cancellationToken)
    {
        return await _promocaoRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NaoEncontradoException("Promoção não encontrada.");
    }

    private async Task PreencherAsync(Promocao promocao, PromocaoRequest request, CancellationToken cancellationToken)
    {
        var tipo = TiposPromocao.Todos.FirstOrDefault(t => string.Equals(t, request.Tipo?.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new NegocioException("Tipo inválido. Use PERCENTUAL, VALOR_FIXO ou FRETE_GRATIS.");

        if (request.Valor != Math.Round(request.Valor, 2) || (request.ValorMinimo is { } min && min != Math.Round(min, 2)))
            throw new NegocioException("Os valores devem ter no máximo 2 casas decimais.");

        if (request.Fim < request.Inicio)
            throw new NegocioException("A data final deve ser igual ou posterior à inicial.");

        var valor = request.Valor;
        switch (tipo)
        {
            case TiposPromocao.Percentual:
                if (valor <= 0 || valor > 100)
                    throw new NegocioException("O percentual deve ser maior que 0 e no máximo 100.");
                break;
            case TiposPromocao.ValorFixo:
                if (valor <= 0)
                    throw new NegocioException("O desconto em reais deve ser maior que zero.");
                break;
            case TiposPromocao.FreteGratis:
                if (request.PizzaId is not null)
                    throw new NegocioException("Frete grátis vale para o pedido todo, não para uma pizza.");
                valor = 0m;
                break;
        }

        if (request.PizzaId is not null)
        {
            if (request.ValorMinimo is not null)
                throw new NegocioException("O valor mínimo só se aplica a promoções do pedido todo.");

            if (!await _pizzaRepository.ExisteAsync(request.PizzaId.Value, cancellationToken))
                throw new NegocioException("A pizza informada não existe.");
        }

        promocao.Nome = request.Nome.Trim();
        promocao.Tipo = tipo;
        promocao.Valor = valor;
        promocao.ValorMinimo = request.ValorMinimo;
        promocao.PizzaId = request.PizzaId;
        promocao.Inicio = request.Inicio;
        promocao.Fim = request.Fim;
        promocao.Ativa = request.Ativa;
    }

    private static PromocaoAdminResponse Mapear(Promocao p)
    {
        return new PromocaoAdminResponse(
            p.Id, p.Nome, p.Tipo, p.Valor, p.ValorMinimo, p.PizzaId, p.Pizza?.Nome,
            p.Inicio, p.Fim, p.Ativa, p.EstaVigente(DateTime.Now));
    }
}
