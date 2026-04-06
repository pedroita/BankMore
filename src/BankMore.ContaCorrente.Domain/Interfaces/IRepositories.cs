using BankMore.ContaCorrente.Domain.Entities;

namespace BankMore.ContaCorrente.Domain.Interfaces;

public interface IContaCorrenteRepository
{
    Task<Entities.ContaCorrente?> ObterPorNumeroContaAsync(string numeroConta);
    Task<Entities.ContaCorrente?> ObterPorCpfAsync(string cpf);
    Task InserirAsync(Entities.ContaCorrente conta);
    Task InativarAsync(string numeroConta);
}

public interface IMovimentoRepository
{
    Task InserirAsync(Movimento movimento);
    Task<bool> ExisteIdRequisicaoAsync(string idRequisicao);
    Task<decimal> ObterSaldoAsync(string numeroConta);
}
