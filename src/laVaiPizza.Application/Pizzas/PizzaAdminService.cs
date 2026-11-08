using laVaiPizza.Application.Abstractions;
using laVaiPizza.Application.Exceptions;
using laVaiPizza.Domain.Constants;
using laVaiPizza.Domain.Entities;

namespace laVaiPizza.Application.Pizzas;

/// <summary>
/// Cadastro de pizzas, precos e fotos (area do Gerente). Pizza nunca e apagada: e desativada.
/// Toda alteracao descarta o cache do cardapio para o site mostrar o dado novo.
/// </summary>
public class PizzaAdminService
{
    private const long TamanhoMaximoImagem = 2 * 1024 * 1024; // 2 MB

    private readonly IPizzaRepository _pizzaRepository;
    private readonly IImagemStorage _imagemStorage;
    private readonly PizzaService _pizzaService;

    public PizzaAdminService(IPizzaRepository pizzaRepository, IImagemStorage imagemStorage, PizzaService pizzaService)
    {
        _pizzaRepository = pizzaRepository;
        _imagemStorage = imagemStorage;
        _pizzaService = pizzaService;
    }

    public async Task<IReadOnlyList<PizzaAdminResponse>> ListarAsync(CancellationToken cancellationToken = default)
    {
        var pizzas = await _pizzaRepository.GetTodasComPrecosAsync(cancellationToken);
        return pizzas.Select(Mapear).ToList();
    }

    public async Task<PizzaAdminResponse> ObterAsync(int id, CancellationToken cancellationToken = default)
    {
        return Mapear(await BuscarAsync(id, cancellationToken));
    }

    public async Task<PizzaAdminResponse> CriarAsync(PizzaCriarRequest request, int funcionarioId, CancellationToken cancellationToken = default)
    {
        var nome = request.Nome.Trim();
        await GarantirNomeLivreAsync(nome, null, cancellationToken);

        var agora = DateTime.Now;
        var pizza = new Pizza
        {
            Nome = nome,
            Descricao = request.Descricao?.Trim() ?? string.Empty,
            Ativa = true,
            Precos = new List<PizzaPreco>
            {
                NovoPreco(TamanhosPizza.Pequena, request.PrecoPequena, funcionarioId, agora),
                NovoPreco(TamanhosPizza.Media, request.PrecoMedia, funcionarioId, agora),
                NovoPreco(TamanhosPizza.Grande, request.PrecoGrande, funcionarioId, agora)
            }
        };

        _pizzaRepository.Adicionar(pizza);
        await _pizzaRepository.SalvarAsync(cancellationToken);
        _pizzaService.InvalidarCardapio();

        return Mapear(pizza);
    }

    public async Task<PizzaAdminResponse> AtualizarAsync(int id, PizzaAtualizarRequest request, CancellationToken cancellationToken = default)
    {
        var pizza = await BuscarAsync(id, cancellationToken);
        var nome = request.Nome.Trim();
        await GarantirNomeLivreAsync(nome, id, cancellationToken);

        pizza.Nome = nome;
        pizza.Descricao = request.Descricao?.Trim() ?? string.Empty;

        await _pizzaRepository.SalvarAsync(cancellationToken);
        _pizzaService.InvalidarCardapio();
        return Mapear(pizza);
    }

    public Task<PizzaAdminResponse> DesativarAsync(int id, CancellationToken cancellationToken = default)
        => AlterarAtivaAsync(id, false, cancellationToken);

    public Task<PizzaAdminResponse> ReativarAsync(int id, CancellationToken cancellationToken = default)
        => AlterarAtivaAsync(id, true, cancellationToken);

    public async Task<PizzaAdminResponse> AtualizarPrecoAsync(
        int id, string tamanhoTexto, decimal novoPreco, int funcionarioId, CancellationToken cancellationToken = default)
    {
        var tamanho = TamanhosPizza.Normalizar(tamanhoTexto)
            ?? throw new NegocioException($"Tamanho inválido: '{tamanhoTexto}'. Use Pequena, Media ou Grande.");

        if (novoPreco != Math.Round(novoPreco, 2))
            throw new NegocioException("O preço deve ter no máximo 2 casas decimais.");

        var pizza = await BuscarAsync(id, cancellationToken);
        var preco = pizza.Precos.FirstOrDefault(p => p.Tamanho == tamanho)
            ?? throw new NaoEncontradoException($"A pizza '{pizza.Nome}' não tem o tamanho {tamanho}.");

        if (preco.Preco != novoPreco)
        {
            // O trigger trg_AuditoriaPreco grava o historico (preco antigo, novo, quem e quando).
            preco.Preco = novoPreco;
            preco.AtualizadoEm = DateTime.Now;
            preco.AtualizadoPorFuncionarioId = funcionarioId;

            await _pizzaRepository.SalvarAsync(cancellationToken);
            _pizzaService.InvalidarCardapio();
        }

        return Mapear(pizza);
    }

    /// <summary>
    /// Troca a foto. O tipo e identificado pelo conteudo do arquivo (PNG, JPG ou WEBP),
    /// nunca pelo nome ou pelo Content-Type enviados pelo cliente.
    /// </summary>
    public async Task<PizzaAdminResponse> AtualizarImagemAsync(
        int id, Stream conteudo, long tamanho, CancellationToken cancellationToken = default)
    {
        var pizza = await BuscarAsync(id, cancellationToken);

        if (tamanho <= 0)
            throw new NegocioException("Envie um arquivo de imagem.");
        if (tamanho > TamanhoMaximoImagem)
            throw new NegocioException("A imagem deve ter no máximo 2 MB.");

        var extensao = await DetectarExtensaoAsync(conteudo, cancellationToken)
            ?? throw new NegocioException("Formato inválido. Envie uma imagem PNG, JPG ou WEBP.");

        conteudo.Position = 0;
        var antiga = pizza.ImagemUrl;
        pizza.ImagemUrl = await _imagemStorage.SalvarAsync(conteudo, extensao, cancellationToken);

        await _pizzaRepository.SalvarAsync(cancellationToken);
        _imagemStorage.Remover(antiga);
        _pizzaService.InvalidarCardapio();

        return Mapear(pizza);
    }

    public async Task<IReadOnlyList<HistoricoPrecoResponse>> HistoricoPrecosAsync(
        int? pizzaId, int maximo, CancellationToken cancellationToken = default)
    {
        if (pizzaId is not null && !await _pizzaRepository.ExisteAsync(pizzaId.Value, cancellationToken))
            throw new NaoEncontradoException("Pizza não encontrada.");

        maximo = Math.Clamp(maximo, 1, 500);
        var historico = await _pizzaRepository.GetHistoricoPrecosAsync(pizzaId, maximo, cancellationToken);

        return historico
            .Select(h => new HistoricoPrecoResponse(
                h.Id, h.PizzaId, h.Pizza.Nome, h.Tamanho, h.PrecoAntigo, h.PrecoNovo, h.AlteradoEm,
                h.AlteradoPorFuncionario?.Nome))
            .ToList();
    }

    // ------------------------------------------------------------------ auxiliares

    private async Task<PizzaAdminResponse> AlterarAtivaAsync(int id, bool ativa, CancellationToken cancellationToken)
    {
        var pizza = await BuscarAsync(id, cancellationToken);

        if (pizza.Ativa != ativa)
        {
            pizza.Ativa = ativa;
            await _pizzaRepository.SalvarAsync(cancellationToken);
            _pizzaService.InvalidarCardapio();
        }

        return Mapear(pizza);
    }

    private async Task<Pizza> BuscarAsync(int id, CancellationToken cancellationToken)
    {
        return await _pizzaRepository.GetByIdParaAtualizacaoAsync(id, cancellationToken)
            ?? throw new NaoEncontradoException("Pizza não encontrada.");
    }

    private async Task GarantirNomeLivreAsync(string nome, int? ignorarId, CancellationToken cancellationToken)
    {
        if (await _pizzaRepository.NomeExisteAsync(nome, ignorarId, cancellationToken))
            throw new NegocioException($"Já existe uma pizza chamada '{nome}'.");
    }

    private static PizzaPreco NovoPreco(string tamanho, decimal valor, int funcionarioId, DateTime agora)
    {
        if (valor != Math.Round(valor, 2))
            throw new NegocioException("Os preços devem ter no máximo 2 casas decimais.");

        return new PizzaPreco
        {
            Tamanho = tamanho,
            Preco = valor,
            AtualizadoEm = agora,
            AtualizadoPorFuncionarioId = funcionarioId
        };
    }

    private static async Task<string?> DetectarExtensaoAsync(Stream conteudo, CancellationToken cancellationToken)
    {
        var cabecalho = new byte[12];
        conteudo.Position = 0;
        var lidos = await conteudo.ReadAsync(cabecalho.AsMemory(0, cabecalho.Length), cancellationToken);
        if (lidos < 12) return null;

        if (cabecalho[0] == 0x89 && cabecalho[1] == 0x50 && cabecalho[2] == 0x4E && cabecalho[3] == 0x47)
            return ".png";
        if (cabecalho[0] == 0xFF && cabecalho[1] == 0xD8 && cabecalho[2] == 0xFF)
            return ".jpg";
        // WEBP: "RIFF" + tamanho (4 bytes) + "WEBP"
        if (cabecalho[0] == 'R' && cabecalho[1] == 'I' && cabecalho[2] == 'F' && cabecalho[3] == 'F'
            && cabecalho[8] == 'W' && cabecalho[9] == 'E' && cabecalho[10] == 'B' && cabecalho[11] == 'P')
            return ".webp";

        return null;
    }

    private static PizzaAdminResponse Mapear(Pizza pizza)
    {
        return new PizzaAdminResponse(
            pizza.Id,
            pizza.Nome,
            pizza.Descricao,
            pizza.ImagemUrl,
            pizza.Ativa,
            pizza.Precos
                .OrderBy(p => TamanhosPizza.Ordem(p.Tamanho))
                .Select(p => new PizzaPrecoAdminResponse(p.Tamanho, p.Preco, p.AtualizadoEm))
                .ToList());
    }
}
