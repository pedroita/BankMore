using BankMore.ContaCorrente.Application.Common;
using BankMore.ContaCorrente.Domain.Enums;
using BankMore.ContaCorrente.Domain.Interfaces;
using MediatR;

namespace BankMore.ContaCorrente.Application.Commands.InativarConta;

public record InativarContaCommand(string NumeroConta, string Senha) : IRequest;

public class InativarContaHandler : IRequestHandler<InativarContaCommand>
{
    private readonly IContaCorrenteRepository _repository;

    public InativarContaHandler(IContaCorrenteRepository repository)
    {
        _repository = repository;
    }

    public async Task Handle(InativarContaCommand request, CancellationToken cancellationToken)
    {
        var conta = await _repository.ObterPorNumeroContaAsync(request.NumeroConta)
            ?? throw new DomainException("Conta corrente não encontrada.", TipoFalha.INVALID_ACCOUNT);

        if (!BCrypt.Net.BCrypt.Verify(request.Senha, conta.SenhaHash))
            throw new DomainException("Senha inválida.", TipoFalha.USER_UNAUTHORIZED);

        await _repository.InativarAsync(request.NumeroConta);
    }
}
