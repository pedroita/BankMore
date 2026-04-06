using Dapper;
using BankMore.Transferencia.Domain.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using TransferenciaEntity = BankMore.Transferencia.Domain.Entities.Transferencia;


namespace BankMore.Transferencia.Infrastructure.Repositories;

public class TransferenciaRepository : ITransferenciaRepository
{
    private readonly string _connectionString;

    public TransferenciaRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task InserirAsync(TransferenciaEntity transferencia)
    {
        const string sql = @"
            INSERT INTO transferencia
                (idtransferencia, idcontacorrente_origem, idcontacorrente_destino, datamovimento, valor)
            VALUES
                (@IdTransferencia, @IdContaCorrenteOrigem, @IdContaCorrenteDestino, @DataMovimento, @Valor)";

        using var conn = new SqliteConnection(_connectionString);
        await conn.ExecuteAsync(sql, new
        {
            transferencia.IdTransferencia,
            transferencia.IdContaCorrenteOrigem,
            transferencia.IdContaCorrenteDestino,
            transferencia.DataMovimento,
            transferencia.Valor
        });
    }
}
