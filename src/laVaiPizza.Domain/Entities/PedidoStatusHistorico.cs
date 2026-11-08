namespace laVaiPizza.Domain.Entities;

/// <summary>Linha do tempo do pedido (gravada pelo trigger trg_HistoricoStatusPedido).</summary>
public class PedidoStatusHistorico
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;

    public string Status { get; set; } = string.Empty;
    public DateTime AlteradoEm { get; set; }

    public int? FuncionarioId { get; set; }
    public Funcionario? Funcionario { get; set; }
}
