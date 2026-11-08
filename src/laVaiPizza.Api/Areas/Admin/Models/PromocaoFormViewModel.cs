using System.ComponentModel.DataAnnotations;
using laVaiPizza.Application.Pizzas;
using laVaiPizza.Application.Promocoes;
using laVaiPizza.Domain.Constants;

namespace laVaiPizza.Api.Areas.Admin.Models;

public class PromocaoFormViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escolha o tipo.")]
    [Display(Name = "Tipo")]
    public string Tipo { get; set; } = TiposPromocao.Percentual;

    [Range(0, 99999.99, ErrorMessage = "O valor deve ser positivo.")]
    [Display(Name = "Valor")]
    public decimal Valor { get; set; }

    [Range(0, 99999.99, ErrorMessage = "O valor mínimo deve ser positivo.")]
    [Display(Name = "Valor mínimo do pedido")]
    public decimal? ValorMinimo { get; set; }

    [Display(Name = "Pizza")]
    public int? PizzaId { get; set; }

    [Required(ErrorMessage = "Informe o início.")]
    [Display(Name = "Início")]
    public DateTime? Inicio { get; set; }

    [Required(ErrorMessage = "Informe o fim.")]
    [Display(Name = "Fim")]
    public DateTime? Fim { get; set; }

    [Display(Name = "Ativa")]
    public bool Ativa { get; set; } = true;

    /// <summary>Pizzas para o seletor "vale para".</summary>
    public IReadOnlyList<PizzaAdminResponse> Pizzas { get; set; } = Array.Empty<PizzaAdminResponse>();

    public PromocaoRequest ToRequest() => new(
        Nome, Tipo, Valor, ValorMinimo, PizzaId, Inicio!.Value, Fim!.Value, Ativa);

    public static PromocaoFormViewModel De(PromocaoAdminResponse p) => new()
    {
        Id = p.Id, Nome = p.Nome, Tipo = p.Tipo, Valor = p.Valor, ValorMinimo = p.ValorMinimo,
        PizzaId = p.PizzaId, Inicio = p.Inicio, Fim = p.Fim, Ativa = p.Ativa
    };
}
