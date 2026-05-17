using laVaiPizza.Application.Clientes;

namespace laVaiPizza.Api.Endpoints;

public static class ClienteEndpoints
{
    public static void MapClienteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/clientes").WithTags("Clientes");

        group.MapGet("/", async (ClienteService service) =>
            Results.Ok(await service.GetClientesAsync()));

        group.MapPost("/", async (ClienteRequest request, ClienteService service) =>
        {
            var cliente = await service.CreateClienteAsync(request);
            return Results.Created($"/clientes/{cliente.Id}", cliente);
        });
    }
}