using BankMore.ContaCorrente.Application.Common;
using BankMore.ContaCorrente.Domain.Entities;
using BankMore.ContaCorrente.Domain.Enums;
using BankMore.ContaCorrente.Domain.Interfaces;
using MediatR;

namespace BankMore.ContaCorrente.Application.Commands.MovimentarConta;

public record MovimentarContaCommand(
    string IdRequisicao,
    string NumeroContaLogado,
    string? NumeroConta,
    decimal Valor,
    string Tipo) : IRequest;

public class MovimentarContaHandler : IRequestHandler<MovimentarContaCommand>
{
    private readonly IContaCorrenteRepository _contaRepository;
    private readonly IMovimentoRepository _movimentoRepository;

    public MovimentarContaHandler(
        IContaCorrenteRepository contaRepository,
        IMovimentoRepository movimentoRepository)
    {
        _contaRepository = contaRepository;
        _movimentoRepository = movimentoRepository;
    }

    public async Task Handle(MovimentarContaCommand request, CancellationToken cancellationToken)
    {
        if (await _movimentoRepository.ExisteIdRequisicaoAsync(request.IdRequisicao))
            return;

        var numeroConta = string.IsNullOrWhiteSpace(request.NumeroConta)
            ? request.NumeroContaLogado
            : request.NumeroConta;

        if (request.Valor <= 0)
            throw new DomainException("Apenas valores positivos são aceitos.", TipoFalha.INVALID_VALUE);

        if (!Enum.TryParse<TipoMovimento>(request.Tipo?.ToUpper(), out var tipoMovimento))
            throw new DomainException("Tipo de movimento inválido. Use C (Crédito) ou D (Débito).", TipoFalha.INVALID_TYPE);

        if (numeroConta != request.NumeroContaLogado && tipoMovimento != TipoMovimento.C)
            throw new DomainException("Apenas crédito é permitido para conta diferente do usuário logado.", TipoFalha.INVALID_TYPE);

        var conta = await _contaRepository.ObterPorNumeroContaAsync(numeroConta)
            ?? throw new DomainException("Conta corrente não encontrada.", TipoFalha.INVALID_ACCOUNT);

        if (!conta.Ativo)
            throw new DomainException("Conta corrente inativa.", TipoFalha.INACTIVE_ACCOUNT);

        if (tipoMovimento == TipoMovimento.D)
        {
            var saldoAtual = await _movimentoRepository.ObterSaldoAsync(numeroConta);
            if (saldoAtual < request.Valor)
                throw new DomainException("Saldo insuficiente para realizar o débito.", TipoFalha.INSUFFICIENT_FUNDS);
        }

        var movimento = new Movimento(request.IdRequisicao, numeroConta, request.Valor, tipoMovimento);
        await _movimentoRepository.InserirAsync(movimento);
    }
}