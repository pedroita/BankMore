using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace BankMore.Transferencia.Infrastructure.Database;

public class DatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public void Initialize()
    {
        var schemaPath = Path.Combine(AppContext.BaseDirectory, "Database", "schema.sql");
        var sql = File.ReadAllText(schemaPath);

        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        connection.Execute(sql);
    }
}
