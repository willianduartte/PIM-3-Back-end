namespace laVaiPizza.Api.Endpoints;

public static class StatusEndpoints
{
    public static IEndpointRouteBuilder MapStatusEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => Results.Ok(new { message = "Template Minimal API online." }))
            .WithTags("Status");

        return app;
    }
}
