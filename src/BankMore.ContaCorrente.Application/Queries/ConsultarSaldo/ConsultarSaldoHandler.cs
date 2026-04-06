using BankMore.ContaCorrente.Application.Common;
using BankMore.ContaCorrente.Domain.Enums;
using BankMore.ContaCorrente.Domain.Interfaces;
using MediatR;

namespace BankMore.ContaCorrente.Application.Queries.ConsultarSaldo;

public record ConsultarSaldoQuery(string NumeroConta) : IRequest<ConsultarSaldoResponse>;

public record ConsultarSaldoResponse(
    string NumeroConta,
    string NomeTitular,
    DateTime DataHoraConsulta,
    decimal Saldo);

public class ConsultarSaldoHandler : IRequestHandler<ConsultarSaldoQuery, ConsultarSaldoResponse>
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly IMovimentoRepository _movimentoRepository;

    public ConsultarSaldoHandler(
        IContaCorrenteRepository contaRepository,
        IMovimentoRepository movimentoRepository)
    {
        _contaRepository = contaRepository;
        _movimentoRepository = movimentoRepository;
    }

    public async Task<ConsultarSaldoResponse> Handle(ConsultarSaldoQuery request, CancellationToken cancellationToken)
    {
        var conta = await _contaRepository.ObterPorNumeroContaAsync(request.NumeroConta)
            ?? throw new DomainException("Conta corrente não encontrada.", TipoFalha.INVALID_ACCOUNT);

        if (!conta.Ativo)
            throw new DomainException("Conta corrente inativa.", TipoFalha.INACTIVE_ACCOUNT);

        var saldo = await _movimentoRepository.ObterSaldoAsync(request.NumeroConta);

        return new ConsultarSaldoResponse(
            conta.NumeroConta,
            conta.Nome,
            DateTime.UtcNow,
            saldo);
    }
}
