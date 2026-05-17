using laVaiPizza.Application.Abstractions;
using laVaiPizza.Domain.Entities;

namespace laVaiPizza.Application.Pedidos;

public class PedidoService
{
    private readonly IPedidoRepository _pedidoRepository;

    public PedidoService(IPedidoRepository pedidoRepository)
    {
        _pedidoRepository = pedidoRepository;
    }

    public async Task<IEnumerable<PedidoResponse>> GetPedidosAsync()
    {
        var pedidos = await _pedidoRepository.GetAllAsync();
        return pedidos.Select(p => new PedidoResponse(
            p.Id,
            p.Status,
            p.ValorTotal,
            p.Endereco ?? "",
            p.TempoEstimado,
            p.DataHora));
    }

    public async Task<PedidoResponse> CreatePedidoAsync(PedidoRequest request)
    {
        var pedido = new Pedido
        {
            ClienteId = request.ClienteId,
            ValorTotal = request.ValorTotal,
            Endereco = request.Endereco,
            Status = "Em Preparação",
            DataHora = DateTime.Now,
            TempoEstimado = request.TempoEstimado,
            FuncionarioPreparaId = request.FuncionarioPreparaId,
            FuncionarioEntregaId = request.FuncionarioEntregaId
        };
        foreach (var item in request.Itens)
        {
            pedido.PedidoPizzas.Add(new PedidoPizza
            {
                PizzaId = item.PizzaId,
                Quantidade = item.Quantidade
            });
        }

        await _pedidoRepository.AddAsync(pedido);

        return new PedidoResponse(
            pedido.Id,
            pedido.Status,
            pedido.ValorTotal,
            pedido.Endereco ?? "",
            pedido.TempoEstimado,
            pedido.DataHora);
    }
}