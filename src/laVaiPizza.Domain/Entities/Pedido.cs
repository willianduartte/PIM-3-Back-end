using laVaiPizza.Domain.Constants;

namespace laVaiPizza.Domain.Entities;

public class Pedido
{
    public int Id { get; set; }

    /// <summary>Codigo que o cliente usa para acompanhar o pedido (sem login).</summary>
    public string CodigoAcompanhamento { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }
    public decimal Desconto { get; set; }
    public decimal TaxaEntrega { get; set; }
    public decimal Total { get; set; }

    public string Status { get; set; } = StatusPedido.EmPreparacao;
    public DateTime DataHora { get; set; } = DateTime.Now;
    public int? TempoEstimado { get; set; }

    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int? FuncionarioPreparaId { get; set; }
    public Funcionario? FuncionarioPrepara { get; set; }

    public int? FuncionarioEntregaId { get; set; }
    public Funcionario? FuncionarioEntrega { get; set; }

    public ICollection<ItemPedido> Itens { get; set; } = new List<ItemPedido>();
    public ICollection<PedidoStatusHistorico> HistoricoStatus { get; set; } = new List<PedidoStatusHistorico>();
}
