using Dapper;
using BankMore.Transferencia.Domain.Entities;
using BankMore.Transferencia.Domain.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace BankMore.Transferencia.Infrastructure.Repositories;

public class IdempotenciaRepository : IIdempotenciaRepository
{
    private readonly string _connectionString;

    public IdempotenciaRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<Idempotencia?> ObterPorChaveAsync(string chave)
    {
        const string sql = "SELECT * FROM idempotencia WHERE chave_idempotencia = @Chave";
        using var conn = new SqliteConnection(_connectionString);
        return await conn.QueryFirstOrDefaultAsync<Idempotencia>(sql, new { Chave = chave });
    }

    public async Task InserirAsync(Idempotencia idempotencia)
    {
        const string sql = @"
            INSERT OR IGNORE INTO idempotencia (chave_idempotencia, requisicao, resultado)
            VALUES (@ChaveIdempotencia, @Requisicao, @Resultado)";

        using var conn = new SqliteConnection(_connectionString);
        await conn.ExecuteAsync(sql, new
        {
            idempotencia.ChaveIdempotencia,
            idempotencia.Requisicao,
            idempotencia.Resultado
        });
    }
}
