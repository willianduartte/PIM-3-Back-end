using laVaiPizza.Application.Funcionarios;

namespace laVaiPizza.Api.Endpoints;

public static class FuncionarioEndpoints
{
    public static void MapFuncionarioEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/funcionarios").WithTags("Funcionários");

        group.MapGet("/", async (FuncionarioService service) =>
            Results.Ok(await service.GetFuncionariosAsync()));

        group.MapPost("/", async (FuncionarioRequest request, FuncionarioService service) =>
        {
            var f = await service.CreateFuncionarioAsync(request);
            return Results.Created($"/funcionarios/{f.Id}", f);
        });

        group.MapDelete("/{id:int}", async (int id, FuncionarioService service) =>
        {
            await service.DeleteFuncionarioAsync(id);
            return Results.NoContent();
        });
    }
}