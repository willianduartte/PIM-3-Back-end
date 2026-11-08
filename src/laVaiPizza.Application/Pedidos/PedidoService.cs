using System.Net.Mail;
using System.Security.Cryptography;
using laVaiPizza.Application.Abstractions;
using laVaiPizza.Application.Exceptions;
using laVaiPizza.Domain.Constants;
using laVaiPizza.Domain.Entities;
using laVaiPizza.Domain.Services;

namespace laVaiPizza.Application.Pedidos;

/// <summary>
/// Criacao e acompanhamento de pedidos feitos sem login. O servidor recalcula tudo
/// (precos, promocoes, taxa e total) e ignora qualquer valor vindo do cliente.
/// </summary>
public class PedidoService
{
    public const decimal TaxaEntregaPadrao = 8.00m;
    private const int MaximoPizzasPorPedido = 40;
    private const string AlfabetoCodigo = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // sem 0/O/1/I

    private readonly IPedidoRepository _pedidoRepository;
    private readonly IPizzaRepository _pizzaRepository;
    private readonly IPromocaoRepository _promocaoRepository;

    public PedidoService(
        IPedidoRepository pedidoRepository,
        IPizzaRepository pizzaRepository,
        IPromocaoRepository promocaoRepository)
    {
        _pedidoRepository = pedidoRepository;
        _pizzaRepository = pizzaRepository;
        _promocaoRepository = promocaoRepository;
    }

    public async Task<PedidoResponse> CriarAsync(PedidoRequest request, CancellationToken cancellationToken = default)
    {
        var dadosCliente = request.Cliente ?? throw new NegocioException("Informe os dados do cliente.");
        var itensRequest = request.Itens ?? throw new NegocioException("Informe os itens do pedido.");

        if (!string.IsNullOrWhiteSpace(dadosCliente.Email)
            && !MailAddress.TryCreate(dadosCliente.Email.Trim(), out _))
            throw new NegocioException("E-mail inválido.");

        var calculo = await CalcularAsync(itensRequest, cancellationToken);

        // Monta e grava (cliente + pedido + itens numa transacao)
        var pedido = new Pedido
        {
            CodigoAcompanhamento = await GerarCodigoAsync(cancellationToken),
            Subtotal = calculo.Subtotal,
            Desconto = calculo.Desconto,
            TaxaEntrega = calculo.TaxaEntrega,
            Total = calculo.Total,
            Status = StatusPedido.EmPreparacao,
            DataHora = DateTime.Now,
            TempoEstimado = calculo.TempoEstimado,
            Cliente = new Cliente
            {
                Nome = dadosCliente.Nome.Trim(),
                Telefone = dadosCliente.Telefone.Trim(),
                Email = string.IsNullOrWhiteSpace(dadosCliente.Email) ? null : dadosCliente.Email.Trim(),
                Logradouro = dadosCliente.Logradouro.Trim(),
                Numero = dadosCliente.Numero.Trim(),
                Complemento = string.IsNullOrWhiteSpace(dadosCliente.Complemento) ? null : dadosCliente.Complemento.Trim(),
                Bairro = dadosCliente.Bairro.Trim(),
                Cidade = string.IsNullOrWhiteSpace(dadosCliente.Cidade) ? "São Paulo" : dadosCliente.Cidade.Trim(),
                Cep = FormatarCep(dadosCliente.Cep)
            },
            Itens = calculo.Itens
        };

        await _pedidoRepository.AdicionarAsync(pedido, cancellationToken);

        return new PedidoResponse(
            pedido.Id,
            pedido.CodigoAcompanhamento,
            pedido.Status,
            pedido.DataHora,
            calculo.TempoEstimado,
            calculo.ItensResposta,
            calculo.Promocoes,
            calculo.Subtotal,
            calculo.Desconto,
            calculo.TaxaEntrega,
            calculo.Total);
    }

    /// <summary>
    /// Calcula o pedido com a mesma regra de CriarAsync, mas sem gravar nada.
    /// O carrinho do site usa para mostrar desconto, frete e total exatos.
    /// </summary>
    public async Task<SimulacaoResponse> SimularAsync(SimulacaoRequest request, CancellationToken cancellationToken = default)
    {
        var itensRequest = request.Itens ?? throw new NegocioException("Informe os itens do pedido.");
        var calculo = await CalcularAsync(itensRequest, cancellationToken);

        return new SimulacaoResponse(
            calculo.ItensResposta,
            calculo.Promocoes,
            calculo.Subtotal,
            calculo.Desconto,
            calculo.TaxaEntrega,
            calculo.Total,
            calculo.TempoEstimado);
    }

    private sealed record Calculo(
        List<ItemPedido> Itens,
        List<ItemPedidoResponse> ItensResposta,
        List<PromocaoAplicadaResponse> Promocoes,
        decimal Subtotal,
        decimal Desconto,
        decimal TaxaEntrega,
        decimal Total,
        int TempoEstimado);

    /// <summary>Valida os itens e calcula precos, promocoes, taxa e total a partir do banco.</summary>
    private async Task<Calculo> CalcularAsync(IEnumerable<ItemPedidoRequest> itensRequest, CancellationToken cancellationToken)
    {
        // 1) Normaliza os itens e junta linhas repetidas (mesma pizza e tamanho)
        var linhas = new Dictionary<(int PizzaId, string Tamanho), int>();
        foreach (var item in itensRequest)
        {
            var tamanho = TamanhosPizza.Normalizar(item.Tamanho)
                ?? throw new NegocioException($"Tamanho inválido: '{item.Tamanho}'. Use Pequena, Media ou Grande.");

            var chave = (item.PizzaId, tamanho);
            linhas[chave] = linhas.GetValueOrDefault(chave) + item.Quantidade;
        }

        if (linhas.Count == 0)
            throw new NegocioException("O pedido precisa ter ao menos um item.");

        if (linhas.Values.Sum() > MaximoPizzasPorPedido)
            throw new NegocioException($"O pedido pode ter no máximo {MaximoPizzasPorPedido} pizzas.");

        // 2) Busca no banco as pizzas ativas e os precos reais
        var ids = linhas.Keys.Select(k => k.PizzaId).Distinct().ToList();
        var pizzas = (await _pizzaRepository.GetAtivasComPrecosPorIdsAsync(ids, cancellationToken))
            .ToDictionary(p => p.Id);
        var promocoes = await _promocaoRepository.GetVigentesAsync(DateTime.Now, cancellationToken);

        // 3) Calcula item a item
        var itens = new List<ItemPedido>();
        var itensResposta = new List<ItemPedidoResponse>();
        var subtotal = 0m;
        var descontoPizzas = 0m;
        var descontosPorPromocao = new Dictionary<int, (Promocao Promocao, decimal Desconto)>();

        foreach (var ((pizzaId, tamanho), quantidade) in linhas.OrderBy(l => l.Key.PizzaId).ThenBy(l => TamanhosPizza.Ordem(l.Key.Tamanho)))
        {
            if (!pizzas.TryGetValue(pizzaId, out var pizza))
                throw new NegocioException($"A pizza {pizzaId} não existe ou não está disponível.");

            var preco = pizza.Precos.FirstOrDefault(p => p.Tamanho == tamanho)
                ?? throw new NegocioException($"A pizza '{pizza.Nome}' não tem o tamanho {tamanho}.");

            var (promocaoPizza, descontoUnitario) = SelecaoDePromocoes.MelhorParaPizza(promocoes, pizza.Id, preco.Preco);

            subtotal += preco.Preco * quantidade;
            descontoPizzas += descontoUnitario * quantidade;

            if (promocaoPizza is not null)
            {
                var acumulado = descontosPorPromocao.TryGetValue(promocaoPizza.Id, out var atual) ? atual.Desconto : 0m;
                descontosPorPromocao[promocaoPizza.Id] = (promocaoPizza, acumulado + descontoUnitario * quantidade);
            }

            // PrecoUnitario guarda o preco de tabela no momento da venda
            itens.Add(new ItemPedido
            {
                PizzaId = pizza.Id,
                Tamanho = tamanho,
                Quantidade = quantidade,
                PrecoUnitario = preco.Preco
            });

            itensResposta.Add(new ItemPedidoResponse(
                pizza.Id,
                pizza.Nome,
                tamanho,
                quantidade,
                preco.Preco,
                descontoUnitario > 0 ? preco.Preco - descontoUnitario : null,
                (preco.Preco - descontoUnitario) * quantidade));
        }

        // 4) Promocao do pedido e frete
        var (promocaoPedido, descontoPedido) =
            SelecaoDePromocoes.MelhorDoPedido(promocoes, subtotal, subtotal - descontoPizzas);
        if (promocaoPedido is not null)
            descontosPorPromocao[promocaoPedido.Id] = (promocaoPedido, descontoPedido);

        var taxa = SelecaoDePromocoes.FreteGratis(promocoes, subtotal) is null ? TaxaEntregaPadrao : 0m;
        var desconto = descontoPizzas + descontoPedido;
        var quantidadeTotal = linhas.Values.Sum();

        return new Calculo(
            itens,
            itensResposta,
            descontosPorPromocao.Values
                .Select(d => new PromocaoAplicadaResponse(d.Promocao.Id, d.Promocao.Nome, d.Desconto))
                .ToList(),
            subtotal,
            desconto,
            taxa,
            subtotal - desconto + taxa,
            Math.Min(30 + 3 * quantidadeTotal, 60));
    }
    public async Task<AcompanhamentoResponse> AcompanharAsync(string codigo, CancellationToken cancellationToken = default)
    {
        var pedido = await _pedidoRepository.GetByCodigoAsync(codigo.Trim().ToUpperInvariant(), cancellationToken)
            ?? throw new NaoEncontradoException("Pedido não encontrado. Confira o código.");

        var indiceAtual = StatusPedido.Fluxo.ToList().IndexOf(pedido.Status);
        var entregue = pedido.Status == StatusPedido.Entregue;

        var etapas = StatusPedido.Fluxo
            .Select((status, indice) => new EtapaResponse(
                status,
                indice < indiceAtual || (entregue && indice == indiceAtual),
                indice == indiceAtual,
                pedido.HistoricoStatus
                    .Where(h => h.Status == status)
                    .OrderBy(h => h.AlteradoEm)
                    .Select(h => (DateTime?)h.AlteradoEm)
                    .FirstOrDefault()))
            .ToList();

        var primeiroNome = pedido.Cliente.Nome.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;

        return new AcompanhamentoResponse(
            pedido.CodigoAcompanhamento,
            pedido.Status,
            pedido.DataHora,
            pedido.TempoEstimado,
            primeiroNome,
            pedido.Cliente.Bairro,
            pedido.Itens
                .OrderBy(i => i.Pizza.Nome).ThenBy(i => TamanhosPizza.Ordem(i.Tamanho))
                .Select(i => new ItemAcompanhamentoResponse(i.Pizza.Nome, i.Tamanho, i.Quantidade))
                .ToList(),
            pedido.Total,
            etapas);
    }

    private async Task<string> GerarCodigoAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            var bytes = RandomNumberGenerator.GetBytes(6);
            var chars = bytes.Select(b => AlfabetoCodigo[b % AlfabetoCodigo.Length]).ToArray();
            var codigo = "LV" + new string(chars);

            if (!await _pedidoRepository.CodigoExisteAsync(codigo, cancellationToken))
                return codigo;
        }
    }

    private static string FormatarCep(string cep)
    {
        var digitos = new string(cep.Where(char.IsDigit).ToArray());
        return $"{digitos[..5]}-{digitos[5..]}";
    }
}
