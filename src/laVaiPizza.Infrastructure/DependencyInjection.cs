using laVaiPizza.Application.Abstractions;
using laVaiPizza.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace laVaiPizza.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IPizzaRepository, PizzaRepository>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IPedidoRepository, PedidoRepository>();
        services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();
        services.AddScoped<ILoginRepository, LoginRepository>();

        return services;
    }
}