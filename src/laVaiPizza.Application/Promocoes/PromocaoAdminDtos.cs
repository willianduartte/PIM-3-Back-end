using System.ComponentModel.DataAnnotations;

namespace laVaiPizza.Application.Promocoes;

/// <summary>
/// Cria/altera promocao. Tipo: PERCENTUAL, VALOR_FIXO ou FRETE_GRATIS. PizzaId nulo = vale para o
/// pedido todo; ValorMinimo so existe em promocao do pedido todo.
/// </summary>
public record PromocaoRequest(
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    string Nome,
    [Required(ErrorMessage = "Informe o tipo.")]
    string Tipo,
    [Range(0, 99999.99, ErrorMessage = "O valor deve ser positivo.")]
    decimal Valor,
    [Range(0, 99999.99, ErrorMessage = "O valor mínimo deve ser positivo.")]
    decimal? ValorMinimo,
    int? PizzaId,
    DateTime Inicio,
    DateTime Fim,
    bool Ativa = true);

public record PromocaoAdminResponse(
    int Id,
    string Nome,
    string Tipo,
    decimal Valor,
    decimal? ValorMinimo,
    int? PizzaId,
    string? Pizza,
    DateTime Inicio,
    DateTime Fim,
    bool Ativa,
    bool VigenteAgora);
