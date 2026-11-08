using System.ComponentModel.DataAnnotations;

namespace laVaiPizza.Application.Pizzas;

/// <summary>Cadastro de pizza: os tres precos sao obrigatorios (Pequena, Media e Grande).</summary>
public record PizzaCriarRequest(
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    string Nome,
    [StringLength(300, ErrorMessage = "A descrição deve ter no máximo 300 caracteres.")]
    string? Descricao,
    [Range(0.01, 9999.99, ErrorMessage = "O preço da Pequena deve ser maior que zero.")]
    decimal PrecoPequena,
    [Range(0.01, 9999.99, ErrorMessage = "O preço da Média deve ser maior que zero.")]
    decimal PrecoMedia,
    [Range(0.01, 9999.99, ErrorMessage = "O preço da Grande deve ser maior que zero.")]
    decimal PrecoGrande);

public record PizzaAtualizarRequest(
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    string Nome,
    [StringLength(300, ErrorMessage = "A descrição deve ter no máximo 300 caracteres.")]
    string? Descricao);

public record PrecoAtualizarRequest(
    [Range(0.01, 9999.99, ErrorMessage = "O preço deve ser maior que zero.")]
    decimal Preco);

public record PizzaPrecoAdminResponse(string Tamanho, decimal Preco, DateTime AtualizadoEm);

public record PizzaAdminResponse(
    int Id,
    string Nome,
    string Descricao,
    string? ImagemUrl,
    bool Ativa,
    IReadOnlyList<PizzaPrecoAdminResponse> Precos);

public record HistoricoPrecoResponse(
    int Id,
    int PizzaId,
    string Pizza,
    string Tamanho,
    decimal PrecoAntigo,
    decimal PrecoNovo,
    DateTime AlteradoEm,
    string? AlteradoPor);
