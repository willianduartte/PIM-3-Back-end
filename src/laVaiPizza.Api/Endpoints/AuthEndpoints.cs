using laVaiPizza.Application.Auth;

namespace laVaiPizza.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth").WithTags("Autenticação");

        group.MapPost("/login", async (LoginRequest request, AuthService service) =>
        {
            var result = await service.LoginAsync(request);
            return result is not null ? Results.Ok(result) : Results.Unauthorized();
        });

        group.MapPost("/registrar", async (RegisterRequest request, AuthService service) =>
        {
            var result = await service.RegisterAsync(request);
            return Results.Ok(result);
        });
    }
}