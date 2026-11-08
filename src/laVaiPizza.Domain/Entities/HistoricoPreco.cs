namespace laVaiPizza.Domain.Entities;

/// <summary>Registro de cada mudanca de preco (gravado pelo trigger trg_AuditoriaPreco).</summary>
public class HistoricoPreco
{
    public int Id { get; set; }
    public int PizzaId { get; set; }
    public Pizza Pizza { get; set; } = null!;

    public string Tamanho { get; set; } = string.Empty;
    public decimal PrecoAntigo { get; set; }
    public decimal PrecoNovo { get; set; }
    public DateTime AlteradoEm { get; set; }

    public int? AlteradoPorFuncionarioId { get; set; }
    public Funcionario? AlteradoPorFuncionario { get; set; }
}
