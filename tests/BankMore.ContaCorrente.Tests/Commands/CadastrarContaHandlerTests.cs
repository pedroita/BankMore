using BankMore.ContaCorrente.Application.Commands.CadastrarConta;
using BankMore.ContaCorrente.Application.Common;
using BankMore.ContaCorrente.Domain.Enums;
using BankMore.ContaCorrente.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace BankMore.ContaCorrente.Tests.Commands;

public class CadastrarContaHandlerTests
{
    private readonly IContaCorrenteRepository _repository;
    private readonly CadastrarContaHandler _handler;

    public CadastrarContaHandlerTests()
    {
        _repository = Substitute.For<IContaCorrenteRepository>();
        _handler = new CadastrarContaHandler(_repository);
    }

    [Fact]
    public async Task Handle_CpfValido_DeveCriarContaERetornarNumeroConta()
    {
        var command = new CadastrarContaCommand("529.982.247-25", "Ana Silva", "Senha@123");
        _repository.InserirAsync(Arg.Any<Domain.Entities.ContaCorrente>()).Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.NumeroConta.Should().NotBeNullOrEmpty();
        await _repository.Received(1).InserirAsync(Arg.Any<Domain.Entities.ContaCorrente>());
    }

    [Fact]
    public async Task Handle_CpfInvalido_DeveLancarDomainException()
    {
        var command = new CadastrarContaCommand("111.111.111-11", "Ana Silva", "Senha@123");

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.TipoFalha == TipoFalha.INVALID_DOCUMENT);
    }

    [Fact]
    public async Task Handle_CpfValidoSemMascara_DeveCriarConta()
    {
        var command = new CadastrarContaCommand("52998224725", "Ana Silva", "Senha@123");
        _repository.InserirAsync(Arg.Any<Domain.Entities.ContaCorrente>()).Returns(Task.CompletedTask);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.NumeroConta.Should().NotBeNullOrEmpty();
    }
}
