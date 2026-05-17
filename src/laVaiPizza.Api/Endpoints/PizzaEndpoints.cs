using laVaiPizza.Application.Pizzas;

namespace laVaiPizza.Api.Endpoints;

public static class PizzaEndpoints
{
    public static void MapPizzaEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/pizzas").WithTags("Pizzas");

        group.MapGet("/", async (PizzaService service) =>
            Results.Ok(await service.GetPizzasAsync()));

        group.MapGet("/{id:int}", async (int id, PizzaService service) =>
        {
            var pizza = await service.GetPizzaByIdAsync(id);
            return pizza is not null ? Results.Ok(pizza) : Results.NotFound();
        });

        group.MapPost("/", async (CreatePizzaRequest request, PizzaService service) =>
        {
            var pizza = await service.CreatePizzaAsync(request);
            return Results.Created($"/pizzas/{pizza.Id}", pizza);
        });

        group.MapDelete("/{id:int}", async (int id, PizzaService service) =>
        {
            await service.DeletePizzaAsync(id);
            return Results.NoContent();
        });
    }
}