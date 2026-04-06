using BankMore.ContaCorrente.Application.Common;
using BankMore.ContaCorrente.Domain.Enums;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Domain.ValueObjects;
using MediatR;

namespace BankMore.ContaCorrente.Application.Commands.CadastrarConta;

public record CadastrarContaCommand(string Cpf, string Nome, string Senha) : IRequest<CadastrarContaResponse>;

public record CadastrarContaResponse(string NumeroConta);

public class CadastrarContaHandler : IRequestHandler<CadastrarContaCommand, CadastrarContaResponse>
{
    private readonly IContaCorrenteRepository _repository;

    public CadastrarContaHandler(IContaCorrenteRepository repository)
    {
        _repository = repository;
    }

    public async Task<CadastrarContaResponse> Handle(CadastrarContaCommand request, CancellationToken cancellationToken)
    {
        if (!Cpf.Validar(request.Cpf))
            throw new DomainException("CPF inválido.", TipoFalha.INVALID_DOCUMENT);

        var cpfLimpo = request.Cpf.Replace(".", "").Replace("-", "").Trim();
        var senhaHash = BCrypt.Net.BCrypt.HashPassword(request.Senha);

        var conta = new Domain.Entities.ContaCorrente(cpfLimpo, request.Nome, senhaHash);
        await _repository.InserirAsync(conta);

        return new CadastrarContaResponse(conta.NumeroConta);
    }
}
