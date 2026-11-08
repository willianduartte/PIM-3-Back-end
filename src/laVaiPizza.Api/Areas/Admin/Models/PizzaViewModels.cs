using System.ComponentModel.DataAnnotations;
using laVaiPizza.Application.Pizzas;

namespace laVaiPizza.Api.Areas.Admin.Models;

/// <summary>Cadastro de pizza nova (nome, descricao e os tres precos).</summary>
public class PizzaNovaViewModel
{
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "A descrição deve ter no máximo 300 caracteres.")]
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }

    [Required(ErrorMessage = "Informe o preço da Pequena.")]
    [Range(0.01, 9999.99, ErrorMessage = "O preço deve ser maior que zero.")]
    [Display(Name = "Preço Pequena")]
    public decimal? PrecoPequena { get; set; }

    [Required(ErrorMessage = "Informe o preço da Média.")]
    [Range(0.01, 9999.99, ErrorMessage = "O preço deve ser maior que zero.")]
    [Display(Name = "Preço Média")]
    public decimal? PrecoMedia { get; set; }

    [Required(ErrorMessage = "Informe o preço da Grande.")]
    [Range(0.01, 9999.99, ErrorMessage = "O preço deve ser maior que zero.")]
    [Display(Name = "Preço Grande")]
    public decimal? PrecoGrande { get; set; }
}

/// <summary>Dados editaveis de uma pizza ja cadastrada (precos e foto tem formularios proprios).</summary>
public class PizzaDadosViewModel
{
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "A descrição deve ter no máximo 300 caracteres.")]
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }
}

public class PizzaEditarViewModel
{
    public PizzaAdminResponse Pizza { get; set; } = null!;
    public PizzaDadosViewModel Dados { get; set; } = new();
}

public class HistoricoPrecosViewModel
{
    public int? PizzaId { get; set; }
    public IReadOnlyList<PizzaAdminResponse> Pizzas { get; set; } = Array.Empty<PizzaAdminResponse>();
    public IReadOnlyList<HistoricoPrecoResponse> Itens { get; set; } = Array.Empty<HistoricoPrecoResponse>();
}
