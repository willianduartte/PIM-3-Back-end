using System.ComponentModel.DataAnnotations;

namespace laVaiPizza.Application.Pedidos;

/// <summary>Dados de quem recebe o pedido (nao exige login). As anotacoes ficam nos parametros do construtor.</summary>
public record ClientePedidoRequest(
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 150 caracteres.")]
    string Nome,
    [Required(ErrorMessage = "Informe o telefone.")]
    [RegularExpression(@"^\(?\d{2}\)?\s?9?\d{4}-?\d{4}$", ErrorMessage = "Telefone inválido. Use DDD + número, ex.: (11) 91234-5678.")]
    string Telefone,
    [StringLength(100, ErrorMessage = "O e-mail deve ter no máximo 100 caracteres.")]
    string? Email,
    [Required(ErrorMessage = "Informe a rua.")]
    [StringLength(150, ErrorMessage = "A rua deve ter no máximo 150 caracteres.")]
    string Logradouro,
    [Required(ErrorMessage = "Informe o número.")]
    [StringLength(10, ErrorMessage = "O número deve ter no máximo 10 caracteres.")]
    string Numero,
    [StringLength(60, ErrorMessage = "O complemento deve ter no máximo 60 caracteres.")]
    string? Complemento,
    [Required(ErrorMessage = "Informe o bairro.")]
    [StringLength(80, ErrorMessage = "O bairro deve ter no máximo 80 caracteres.")]
    string Bairro,
    [StringLength(80, ErrorMessage = "A cidade deve ter no máximo 80 caracteres.")]
    string? Cidade,
    [Required(ErrorMessage = "Informe o CEP.")]
    [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "CEP inválido. Use 00000-000.")]
    string Cep);

/// <summary>Um item do carrinho. O preco NAO e enviado: o servidor sempre usa o preco do banco.</summary>
public record ItemPedidoRequest(
    [Range(1, int.MaxValue, ErrorMessage = "Pizza inválida.")]
    int PizzaId,
    [Required(ErrorMessage = "Informe o tamanho.")]
    string Tamanho,
    [Range(1, 20, ErrorMessage = "A quantidade de cada item deve ser de 1 a 20.")]
    int Quantidade);

public record PedidoRequest(
    [Required(ErrorMessage = "Informe os dados do cliente.")]
    ClientePedidoRequest? Cliente,
    [Required(ErrorMessage = "Informe os itens do pedido.")]
    [MinLength(1, ErrorMessage = "O pedido precisa ter ao menos um item.")]
    [MaxLength(30, ErrorMessage = "O pedido pode ter no máximo 30 itens.")]
    List<ItemPedidoRequest>? Itens);

/// <summary>Itens do carrinho para simular o total (nada e gravado).</summary>
public record SimulacaoRequest(
    [Required(ErrorMessage = "Informe os itens do pedido.")]
    [MinLength(1, ErrorMessage = "O pedido precisa ter ao menos um item.")]
    [MaxLength(30, ErrorMessage = "O pedido pode ter no máximo 30 itens.")]
    List<ItemPedidoRequest>? Itens);

public record ItemPedidoResponse(
    int PizzaId,
    string Pizza,
    string Tamanho,
    int Quantidade,
    decimal PrecoUnitario,
    decimal? PrecoPromocional,
    decimal TotalItem);

public record PromocaoAplicadaResponse(int Id, string Nome, decimal Desconto);

/// <summary>Resultado da criacao do pedido, com os valores calculados pelo servidor.</summary>
public record PedidoResponse(
    int Id,
    string CodigoAcompanhamento,
    string Status,
    DateTime DataHora,
    int TempoEstimado,
    IReadOnlyList<ItemPedidoResponse> Itens,
    IReadOnlyList<PromocaoAplicadaResponse> Promocoes,
    decimal Subtotal,
    decimal Desconto,
    decimal TaxaEntrega,
    decimal Total);

/// <summary>Etapa da linha do tempo. Horario vem preenchido quando a etapa ja aconteceu.</summary>
public record EtapaResponse(string Status, bool Concluida, bool Atual, DateTime? Horario);

public record ItemAcompanhamentoResponse(string Pizza, string Tamanho, int Quantidade);

/// <summary>
/// Visao publica do pedido: nao expoe nome de funcionario nem o endereco completo
/// (so o primeiro nome do cliente e o bairro).
/// </summary>
public record AcompanhamentoResponse(
    string CodigoAcompanhamento,
    string Status,
    DateTime DataHora,
    int? TempoEstimado,
    string Cliente,
    string Bairro,
    IReadOnlyList<ItemAcompanhamentoResponse> Itens,
    decimal Total,
    IReadOnlyList<EtapaResponse> Etapas);

/// <summary>Valores calculados para o carrinho, sem criar pedido.</summary>
public record SimulacaoResponse(
    IReadOnlyList<ItemPedidoResponse> Itens,
    IReadOnlyList<PromocaoAplicadaResponse> Promocoes,
    decimal Subtotal,
    decimal Desconto,
    decimal TaxaEntrega,
    decimal Total,
    int TempoEstimado);