namespace laVaiPizza.Domain.Entities;

/// <summary>Preco de uma pizza em um tamanho (chave composta: PizzaId + Tamanho).</summary>
public class PizzaPreco
{
    public int PizzaId { get; set; }
    public Pizza Pizza { get; set; } = null!;

    /// <summary>Pequena, Media ou Grande (veja <see cref="Constants.TamanhosPizza"/>).</summary>
    public string Tamanho { get; set; } = string.Empty;

    public decimal Preco { get; set; }

    public DateTime AtualizadoEm { get; set; } = DateTime.Now;

    /// <summary>Quem alterou o preco; o trigger trg_AuditoriaPreco copia para o historico.</summary>
    public int? AtualizadoPorFuncionarioId { get; set; }
}
