using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Infrastructure.Database;
using BankMore.ContaCorrente.Infrastructure.Repositories;
using BankMore.ContaCorrente.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BankMore.ContaCorrente.Infrastructure;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IContaCorrenteRepository, ContaCorrenteRepository>();
        services.AddScoped<IMovimentoRepository, MovimentoRepository>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddSingleton<DatabaseInitializer>();
        return services;
    }
}
