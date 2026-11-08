using System.Linq.Expressions;
using laVaiPizza.Application.Abstractions;
using laVaiPizza.Application.Exceptions;
using laVaiPizza.Domain.Constants;
using laVaiPizza.Domain.Entities;

namespace laVaiPizza.Application.Pedidos;

/// <summary>
/// Fila de pedidos e troca de status pela equipe. As permissoes por cargo valem aqui no servidor:
///  - Pizzaiolo: ve "Em Preparação" e move para "Aguardando Coleta".
///  - Entregador: ve "Aguardando Coleta" e os proprios "Saiu para Entrega"; pega o pedido e entrega.
///  - Gerente: ve tudo e pode executar qualquer etapa (fica registrado como responsavel).
/// O status so avanca um passo por vez, na ordem do fluxo.
/// </summary>
public class PedidoAdminService
{
    private const int LimiteFila = 100;

    private readonly IPedidoRepository _pedidoRepository;

    public PedidoAdminService(IPedidoRepository pedidoRepository)
    {
        _pedidoRepository = pedidoRepository;
    }

    /// <summary>
    /// Fila do cargo. O filtro por status so vale para o Gerente; sem filtro ele ve os pedidos em aberto.
    /// </summary>
    public async Task<IReadOnlyList<PedidoAdminResponse>> ListarFilaAsync(
        UsuarioAtual usuario, string? status, CancellationToken cancellationToken = default)
    {
        var maisRecentesPrimeiro = false;
        Expression<Func<Pedido, bool>> filtro;

        switch (usuario.Cargo)
        {
            case Cargos.Pizzaiolo:
                filtro = p => p.Status == StatusPedido.EmPreparacao;
                break;

            case Cargos.Entregador:
                var id = usuario.FuncionarioId;
                filtro = p => p.Status == StatusPedido.AguardandoColeta
                              || (p.Status == StatusPedido.SaiuParaEntrega && p.FuncionarioEntregaId == id);
                break;

            case Cargos.Gerente:
                if (string.IsNullOrWhiteSpace(status))
                {
                    filtro = p => p.Status != StatusPedido.Entregue;
                }
                else
                {
                    var escolhido = NormalizarStatus(status);
                    filtro = p => p.Status == escolhido;
                    maisRecentesPrimeiro = escolhido == StatusPedido.Entregue; // historico: os ultimos primeiro
                }
                break;

            default:
                throw new AcessoNegadoException("Cargo sem acesso aos pedidos.");
        }

        var pedidos = await _pedidoRepository.GetFilaAsync(filtro, maisRecentesPrimeiro, LimiteFila, cancellationToken);
        return pedidos.Select(p => Mapear(usuario, p)).ToList();
    }

    public async Task<PedidoAdminResponse> ObterAsync(UsuarioAtual usuario, int id, CancellationToken cancellationToken = default)
    {
        var pedido = await _pedidoRepository.GetDetalheAsync(id, cancellationToken)
            ?? throw new NaoEncontradoException("Pedido não encontrado.");

        if (!PodeVer(usuario, pedido))
            throw new AcessoNegadoException("Este pedido não está na sua fila.");

        return Mapear(usuario, pedido);
    }

    public async Task<PedidoAdminResponse> AtualizarStatusAsync(
        UsuarioAtual usuario, int id, string? novoStatusTexto, CancellationToken cancellationToken = default)
    {
        var novo = NormalizarStatus(novoStatusTexto);

        var pedido = await _pedidoRepository.GetDetalheAsync(id, cancellationToken)
            ?? throw new NaoEncontradoException("Pedido não encontrado.");

        if (!PodeVer(usuario, pedido))
            throw new AcessoNegadoException("Este pedido não está na sua fila.");

        // 1) So avanca um passo, na ordem do fluxo
        var indiceAtual = StatusPedido.Fluxo.ToList().IndexOf(pedido.Status);
        if (indiceAtual == StatusPedido.Fluxo.Count - 1)
            throw new NegocioException("Este pedido já foi entregue.");

        var proximo = StatusPedido.Fluxo[indiceAtual + 1];
        if (novo != proximo)
            throw new NegocioException($"Transição inválida: o pedido está em '{pedido.Status}' e o próximo passo é '{proximo}'.");

        // 2) Quem pode executar cada passo
        var cargoDoPasso = novo == StatusPedido.AguardandoColeta ? Cargos.Pizzaiolo : Cargos.Entregador;
        if (usuario.Cargo != Cargos.Gerente && usuario.Cargo != cargoDoPasso)
            throw new AcessoNegadoException($"Seu cargo ({usuario.Cargo}) não pode mover o pedido para '{novo}'.");

        if (novo == StatusPedido.Entregue
            && usuario.Cargo == Cargos.Entregador
            && pedido.FuncionarioEntregaId != usuario.FuncionarioId)
            throw new AcessoNegadoException("Somente o entregador que saiu com o pedido pode concluir a entrega.");

        // 3) Responsavel de cada etapa (o trigger copia para o historico)
        int? preparaId = novo == StatusPedido.AguardandoColeta ? usuario.FuncionarioId : null;
        int? entregaId = novo == StatusPedido.SaiuParaEntrega
            || (novo == StatusPedido.Entregue && pedido.FuncionarioEntregaId is null)
            ? usuario.FuncionarioId
            : null;

        // 4) UPDATE condicional: se outra pessoa mexeu antes, avisa em vez de sobrescrever
        var atualizado = await _pedidoRepository.TentarAtualizarStatusAsync(
            id, pedido.Status, novo, preparaId, entregaId, cancellationToken);

        if (!atualizado)
            throw new ConflitoException("Este pedido acabou de ser atualizado por outra pessoa. Atualize a fila.");

        var recarregado = await _pedidoRepository.GetDetalheAsync(id, cancellationToken)
            ?? throw new NaoEncontradoException("Pedido não encontrado.");

        return Mapear(usuario, recarregado);
    }

    // ------------------------------------------------------------------ auxiliares

    private static string NormalizarStatus(string? texto)
    {
        return StatusPedido.Fluxo.FirstOrDefault(s => string.Equals(s, texto?.Trim(), StringComparison.OrdinalIgnoreCase))
            ?? throw new NegocioException(
                $"Status inválido. Use: {string.Join(", ", StatusPedido.Fluxo)}.");
    }

    private static bool PodeVer(UsuarioAtual usuario, Pedido pedido)
    {
        return usuario.Cargo switch
        {
            Cargos.Gerente => true,
            Cargos.Pizzaiolo => pedido.Status == StatusPedido.EmPreparacao,
            Cargos.Entregador => pedido.Status == StatusPedido.AguardandoColeta
                                 || pedido.FuncionarioEntregaId == usuario.FuncionarioId,
            _ => false
        };
    }

    private static PedidoAdminResponse Mapear(UsuarioAtual usuario, Pedido p)
    {
        var veContato = usuario.Cargo != Cargos.Pizzaiolo;
        var c = p.Cliente;

        var endereco = $"{c.Logradouro}, {c.Numero}"
                       + (string.IsNullOrWhiteSpace(c.Complemento) ? "" : $" - {c.Complemento}")
                       + $" - {c.Bairro} - {c.Cidade} - CEP {c.Cep}";

        return new PedidoAdminResponse(
            p.Id,
            p.CodigoAcompanhamento,
            p.Status,
            p.DataHora,
            p.TempoEstimado,
            c.Nome,
            veContato ? c.Telefone : null,
            veContato ? endereco : null,
            c.Bairro,
            p.Itens
                .OrderBy(i => i.Pizza.Nome).ThenBy(i => TamanhosPizza.Ordem(i.Tamanho))
                .Select(i => new ItemPedidoAdminResponse(i.Pizza.Nome, i.Tamanho, i.Quantidade, i.PrecoUnitario))
                .ToList(),
            p.Subtotal,
            p.Desconto,
            p.TaxaEntrega,
            p.Total,
            p.FuncionarioPrepara?.Nome,
            p.FuncionarioEntrega?.Nome,
            p.HistoricoStatus
                .OrderBy(h => h.AlteradoEm).ThenBy(h => h.Id)
                .Select(h => new HistoricoStatusResponse(h.Status, h.AlteradoEm, h.Funcionario?.Nome))
                .ToList(),
            ProximoStatusPermitido(usuario, p),
            // Calculado no servidor: assim o relogio do navegador ou o fuso nao interferem
            (int)Math.Max(0, (DateTime.Now - p.DataHora).TotalMinutes));
    }

    /// <summary>Proximo status que este usuario pode aplicar ao pedido (mesmas regras de AtualizarStatusAsync).</summary>
    private static string? ProximoStatusPermitido(UsuarioAtual usuario, Pedido pedido)
    {
        var indice = StatusPedido.Fluxo.ToList().IndexOf(pedido.Status);
        if (indice < 0 || indice == StatusPedido.Fluxo.Count - 1 || !PodeVer(usuario, pedido))
            return null;

        var proximo = StatusPedido.Fluxo[indice + 1];
        var cargoDoPasso = proximo == StatusPedido.AguardandoColeta ? Cargos.Pizzaiolo : Cargos.Entregador;

        if (usuario.Cargo != Cargos.Gerente && usuario.Cargo != cargoDoPasso)
            return null;

        if (proximo == StatusPedido.Entregue
            && usuario.Cargo == Cargos.Entregador
            && pedido.FuncionarioEntregaId != usuario.FuncionarioId)
            return null;

        return proximo;
    }
}
