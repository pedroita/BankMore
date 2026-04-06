using BankMore.Transferencia.Domain.Interfaces;
using BankMore.Transferencia.Infrastructure.Database;
using BankMore.Transferencia.Infrastructure.HttpClients;
using BankMore.Transferencia.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace BankMore.Transferencia.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ITransferenciaRepository, TransferenciaRepository>();
        services.AddScoped<IIdempotenciaRepository, IdempotenciaRepository>();
        services.AddSingleton<DatabaseInitializer>();

        var contaCorrenteUrl = configuration["ContaCorrenteApi:BaseUrl"]!;
        services
            .AddRefitClient<IContaCorrenteApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri(contaCorrenteUrl));

        services.AddScoped<IContaCorrenteClient, ContaCorrenteClient>();

        return services;
    }
}
