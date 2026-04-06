using BankMore.ContaCorrente.Application.Common;
using BankMore.ContaCorrente.Domain.Enums;
using BankMore.ContaCorrente.Domain.Interfaces;
using BankMore.ContaCorrente.Domain.ValueObjects;
using MediatR;

namespace BankMore.ContaCorrente.Application.Queries.Login;

public record LoginQuery(string Identificador, string Senha) : IRequest<LoginResponse>;

public record LoginResponse(string Token);

public class LoginHandler : IRequestHandler<LoginQuery, LoginResponse>
{
    private readonly IContaCorrenteRepository _repository;
    private readonly IJwtService _jwtService;

    public LoginHandler(IContaCorrenteRepository repository, IJwtService jwtService)
    {
        _repository = repository;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> Handle(LoginQuery request, CancellationToken cancellationToken)
    {
        Domain.Entities.ContaCorrente? conta = null;

        if (Cpf.Validar(request.Identificador))
        {
            var cpfLimpo = request.Identificador.Replace(".", "").Replace("-", "").Trim();
            conta = await _repository.ObterPorCpfAsync(cpfLimpo);
        }
        else
        {
            conta = await _repository.ObterPorNumeroContaAsync(request.Identificador);
        }

        if (conta is null || !BCrypt.Net.BCrypt.Verify(request.Senha, conta.SenhaHash))
            throw new DomainException("Credenciais inválidas.", TipoFalha.USER_UNAUTHORIZED);

        var token = _jwtService.GerarToken(conta.NumeroConta);
        return new LoginResponse(token);
    }
}
