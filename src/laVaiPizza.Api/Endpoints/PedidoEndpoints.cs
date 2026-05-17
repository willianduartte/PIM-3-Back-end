using laVaiPizza.Application.Pedidos;

namespace laVaiPizza.Api.Endpoints;

public static class PedidoEndpoints
{
    public static void MapPedidoEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/pedidos").WithTags("Pedidos");

        group.MapGet("/", async (PedidoService service) =>
            Results.Ok(await service.GetPedidosAsync()));

        group.MapPost("/", async (PedidoRequest request, PedidoService service) =>
        {
            var pedido = await service.CreatePedidoAsync(request);
            return Results.Created($"/pedidos/{pedido.Id}", pedido);
        });
    }
}