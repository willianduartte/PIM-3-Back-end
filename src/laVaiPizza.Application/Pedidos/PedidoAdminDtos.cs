using System.ComponentModel.DataAnnotations;

namespace laVaiPizza.Application.Pedidos;

/// <summary>Quem esta chamando a API (extraido do token JWT).</summary>
public record UsuarioAtual(int FuncionarioId, string Cargo);

public record StatusRequest(
    [Required(ErrorMessage = "Informe o novo status.")]
    string Status);

public record ItemPedidoAdminResponse(string Pizza, string Tamanho, int Quantidade, decimal PrecoUnitario);

public record HistoricoStatusResponse(string Status, DateTime Em, string? Funcionario);

/// <summary>
/// Pedido visto pela equipe. Telefone e Endereco vem nulos para o Pizzaiolo
/// (ele so precisa saber o que preparar). ProximoStatusPermitido e o status que o usuario logado
/// pode aplicar agora (nulo quando nao pode mover o pedido): a tela so mostra esse botao.
/// </summary>
public record PedidoAdminResponse(
    int Id,
    string CodigoAcompanhamento,
    string Status,
    DateTime DataHora,
    int? TempoEstimado,
    string Cliente,
    string? Telefone,
    string? Endereco,
    string Bairro,
    IReadOnlyList<ItemPedidoAdminResponse> Itens,
    decimal Subtotal,
    decimal Desconto,
    decimal TaxaEntrega,
    decimal Total,
    string? PreparadoPor,
    string? EntreguePor,
    IReadOnlyList<HistoricoStatusResponse> Historico,
    string? ProximoStatusPermitido,
    int MinutosDesdeCriacao);
