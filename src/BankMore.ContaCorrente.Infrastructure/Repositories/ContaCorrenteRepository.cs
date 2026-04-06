using Dapper;
using BankMore.ContaCorrente.Domain.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace BankMore.ContaCorrente.Infrastructure.Repositories;

public class ContaCorrenteRepository : IContaCorrenteRepository
{
    private readonly string _connectionString;

    public ContaCorrenteRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    private SqliteConnection CriarConexao() => new(_connectionString);

    public async Task<Domain.Entities.ContaCorrente?> ObterPorNumeroContaAsync(string numeroConta)
    {
        const string sql = "SELECT * FROM CONTACORRENTE WHERE NUMEROCONTA = @NumeroConta";
        using var conn = CriarConexao();
        return await conn.QueryFirstOrDefaultAsync<Domain.Entities.ContaCorrente>(sql, new { NumeroConta = numeroConta });
    }

    public async Task<Domain.Entities.ContaCorrente?> ObterPorCpfAsync(string cpf)
    {
        const string sql = "SELECT * FROM CONTACORRENTE WHERE CPF = @Cpf";
        using var conn = CriarConexao();
        return await conn.QueryFirstOrDefaultAsync<Domain.Entities.ContaCorrente>(sql, new { Cpf = cpf });
    }

    public async Task InserirAsync(Domain.Entities.ContaCorrente conta)
    {
        const string sql = @"
            INSERT INTO CONTACORRENTE (ID, NUMEROCONTA, CPF, NOME, SENHAHASH, ATIVO, CRIADOEM)
            VALUES (@Id, @NumeroConta, @Cpf, @Nome, @SenhaHash, @Ativo, @CriadoEm)";
        using var conn = CriarConexao();
        await conn.ExecuteAsync(sql, new
        {
            conta.Id,
            conta.NumeroConta,
            conta.Cpf,
            conta.Nome,
            conta.SenhaHash,
            Ativo = conta.Ativo ? 1 : 0,
            CriadoEm = conta.CriadoEm.ToString("o")
        });
    }

    public async Task InativarAsync(string numeroConta)
    {
        const string sql = "UPDATE CONTACORRENTE SET ATIVO = 0 WHERE NUMEROCONTA = @NumeroConta";
        using var conn = CriarConexao();
        await conn.ExecuteAsync(sql, new { NumeroConta = numeroConta });
    }
}
