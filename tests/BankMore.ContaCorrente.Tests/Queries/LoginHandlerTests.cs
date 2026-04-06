using BankMore.ContaCorrente.Application.Common;
using BankMore.ContaCorrente.Application.Queries.Login;
using BankMore.ContaCorrente.Domain.Enums;
using BankMore.ContaCorrente.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace BankMore.ContaCorrente.Tests.Queries;

public class LoginHandlerTests
{
    private readonly IContaCorrenteRepository _repository;
    private readonly IJwtService _jwtService;
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _repository = Substitute.For<IContaCorrenteRepository>();
        _jwtService = Substitute.For<IJwtService>();
        _handler = new LoginHandler(_repository, _jwtService);
    }

    [Fact]
    public async Task Handle_CredenciaisInvalidas_DeveLancarUserUnauthorized()
    {
        var query = new LoginQuery("12345678", "senhaErrada");
        _repository.ObterPorNumeroContaAsync("12345678").Returns((Domain.Entities.ContaCorrente?)null);

        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.TipoFalha == TipoFalha.USER_UNAUTHORIZED);
    }

    [Fact]
    public async Task Handle_ContaNaoEncontrada_DeveLancarUserUnauthorized()
    {
        var query = new LoginQuery("99999999", "qualquerSenha");
        _repository.ObterPorNumeroContaAsync("99999999").Returns((Domain.Entities.ContaCorrente?)null);

        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.TipoFalha == TipoFalha.USER_UNAUTHORIZED);
    }
}
