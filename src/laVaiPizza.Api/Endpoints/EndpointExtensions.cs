namespace laVaiPizza.Api.Endpoints;

public static class EndpointExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        app.MapStatusEndpoints();
        app.MapAuthEndpoints();
        app.MapPizzaEndpoints();
        app.MapHealthEndpoints();
        app.MapClienteEndpoints();
        app.MapPedidoEndpoints();
        app.MapFuncionarioEndpoints();

        return app;
    }
}