namespace laVaiPizza.Domain.Entities;

public sealed class Pedido
{
    public int Id { get; set; }
    public DateTime DataHora { get; set; } = DateTime.Now;
    public string Status { get; set; } = "Em Preparação";
    public decimal ValorTotal { get; set; }
    public string? Endereco { get; set; }
    public int? TempoEstimado { get; set; }
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int? FuncionarioPreparaId { get; set; }
    public Funcionario? FuncionarioPrepara { get; set; }

    public int? FuncionarioEntregaId { get; set; }
    public Funcionario? FuncionarioEntrega { get; set; }
    public ICollection<PedidoPizza> PedidoPizzas { get; set; } = new List<PedidoPizza>();
}