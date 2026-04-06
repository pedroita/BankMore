using Dapper;
using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace BankMore.ContaCorrente.Infrastructure.Repositories;

public class MovimentoRepository : IMovimentoRepository
{
    private readonly string _connectionString;

    public MovimentoRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    private SqliteConnection CriarConexao() => new(_connectionString);

    public async Task InserirAsync(Movimento movimento)
    {
        const string sql = @"
            INSERT INTO MOVIMENTO (ID, IDREQUISICAO, NUMEROCONTA, VALOR, TIPO, CRIADOEM)
            VALUES (@Id, @IdRequisicao, @NumeroConta, @Valor, @Tipo, @CriadoEm)";
        using var conn = CriarConexao();
        await conn.ExecuteAsync(sql, new
        {
            movimento.Id,
            movimento.IdRequisicao,
            movimento.NumeroConta,
            movimento.Valor,
            Tipo = movimento.Tipo.ToString(),
            CriadoEm = movimento.CriadoEm.ToString("o")
        });
    }

    public async Task<bool> ExisteIdRequisicaoAsync(string idRequisicao)
    {
        const string sql = "SELECT COUNT(1) FROM MOVIMENTO WHERE IDREQUISICAO = @IdRequisicao";
        using var conn = CriarConexao();
        var count = await conn.ExecuteScalarAsync<int>(sql, new { IdRequisicao = idRequisicao });
        return count > 0;
    }

    public async Task<decimal> ObterSaldoAsync(string numeroConta)
    {
        const string sql = @"
            SELECT COALESCE(
                SUM(CASE WHEN TIPO = 'C' THEN VALOR ELSE -VALOR END),
            0) FROM MOVIMENTO WHERE NUMEROCONTA = @NumeroConta";
        using var conn = CriarConexao();
        return await conn.ExecuteScalarAsync<decimal>(sql, new { NumeroConta = numeroConta });
    }
}
