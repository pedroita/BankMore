using BankMore.ContaCorrente.Infrastructure.Database;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;

namespace BankMore.ContaCorrente.IntegrationTests;

public class ContaCorrenteWebFactory : WebApplicationFactory<Program>
{
    // Banco em memória compartilhado durante os testes
    private readonly SqliteConnection _connection;
    private readonly string _connectionString;

    public ContaCorrenteWebFactory()
    {
        _connectionString = $"Data Source=test_{Guid.NewGuid():N};Mode=Memory;Cache=Shared";
        _connection = new SqliteConnection(_connectionString);
        _connection.Open(); // mantém o banco em memória vivo durante o teste
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
                ["Jwt:ExpiracaoMinutos"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Garante que o TypeHandler do Guid está registrado
            SqlMapper.AddTypeHandler(new GuidTypeHandler());
            SqlMapper.RemoveTypeMap(typeof(Guid));
            SqlMapper.RemoveTypeMap(typeof(Guid?));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}
