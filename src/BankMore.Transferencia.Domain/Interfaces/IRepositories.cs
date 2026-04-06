using BankMore.Transferencia.Domain.Entities;
using TransferenciaEntity = BankMore.Transferencia.Domain.Entities.Transferencia;

namespace BankMore.Transferencia.Domain.Interfaces;

public interface ITransferenciaRepository
{
    Task InserirAsync(TransferenciaEntity transferencia);
}

public interface IIdempotenciaRepository
{
    Task<Idempotencia?> ObterPorChaveAsync(string chave);
    Task InserirAsync(Idempotencia idempotencia);
}