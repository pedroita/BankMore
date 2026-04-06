using BankMore.Transferencia.Domain.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace BankMore.Transferencia.IntegrationTests;

public class TransferenciaWebFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    // Mock do client HTTP — evita dependência real da API Conta Corrente
    public IContaCorrenteClient ContaCorrenteClientMock { get; } =
        Substitute.For<IContaCorrenteClient>();

    public TransferenciaWebFactory()
    {
        _connectionString = $"Data Source=transf_test_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _connectionString,
                ["Jwt:SecretKey"] = "BankMore@SuperSecretKey2024!ContaCorrente#256bits",
                ["Jwt:Issuer"] = "BankMore.ContaCorrente",
                ["Jwt:ExpiracaoMinutos"] = "60",
                ["ContaCorrenteApi:BaseUrl"] = "https://localhost:9999" // não usado pois mockamos
            });
        });

        builder.ConfigureServices(services =>
        {
            // Substitui o client real pelo mock
            services.RemoveAll<IContaCorrenteClient>();
            services.AddScoped(_ => ContaCorrenteClientMock);
        });
    }
}
