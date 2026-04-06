using BankMore.Transferencia.Application.Commands.EfetuarTransferencia;
using BankMore.Transferencia.Application.Common;
using BankMore.Transferencia.Domain.Entities;
using BankMore.Transferencia.Domain.Enums;
using BankMore.Transferencia.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace BankMore.Transferencia.Tests.Commands;

public class EfetuarTransferenciaHandlerTests
{
    private readonly ITransferenciaRepository _transferenciaRepo;
    private readonly IIdempotenciaRepository _idempotenciaRepo;
    private readonly IContaCorrenteClient _contaCorrenteClient;
    private readonly EfetuarTransferenciaHandler _handler;

    public EfetuarTransferenciaHandlerTests()
    {
        _transferenciaRepo = Substitute.For<ITransferenciaRepository>();
        _idempotenciaRepo = Substitute.For<IIdempotenciaRepository>();
        _contaCorrenteClient = Substitute.For<IContaCorrenteClient>();
        _handler = new EfetuarTransferenciaHandler(
            _transferenciaRepo,
            _idempotenciaRepo,
            _contaCorrenteClient);
    }

    [Fact]
    public async Task Handle_RequisicaoDuplicada_DeveIgnorarIdempotente()
    {
        // Arrange
        var command = new EfetuarTransferenciaCommand("req-dup", "11111111", "22222222", 100m, "token");
        _idempotenciaRepo.ObterPorChaveAsync("req-dup")
            .Returns(new Idempotencia("req-dup", "{}", "{}"));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _contaCorrenteClient.DidNotReceive()
            .MovimentarAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_ValorNegativo_DeveLancarDomainException()
    {
        // Arrange
        var command = new EfetuarTransferenciaCommand("req-001", "11111111", "22222222", -50m, "token");
        _idempotenciaRepo.ObterPorChaveAsync(Arg.Any<string>()).Returns((Idempotencia?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.TipoFalha == TipoFalha.INVALID_VALUE);
    }

    [Fact]
    public async Task Handle_MesmaContaOrigemDestino_DeveLancarDomainException()
    {
        // Arrange
        var command = new EfetuarTransferenciaCommand("req-002", "11111111", "11111111", 100m, "token");
        _idempotenciaRepo.ObterPorChaveAsync(Arg.Any<string>()).Returns((Idempotencia?)null);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.TipoFalha == TipoFalha.INVALID_ACCOUNT);
    }

    [Fact]
    public async Task Handle_FalhaNoCredito_DeveEstornarDebito()
    {
        // Arrange
        var command = new EfetuarTransferenciaCommand("req-003", "11111111", "22222222", 100m, "token");
        _idempotenciaRepo.ObterPorChaveAsync(Arg.Any<string>()).Returns((Idempotencia?)null);

        // Débito OK, crédito falha
        _contaCorrenteClient
            .MovimentarAsync("req-003-debito", Arg.Any<string?>(), 100m, "D", Arg.Any<string>())
            .Returns(Task.CompletedTask);

        _contaCorrenteClient
            .MovimentarAsync("req-003-credito", Arg.Any<string?>(), 100m, "C", Arg.Any<string>())
            .ThrowsAsync(new Exception("Conta destino inativa"));

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert — lança erro de transferência
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.TipoFalha == TipoFalha.TRANSFER_ERROR);

        // Assert — estorno foi chamado
        await _contaCorrenteClient.Received(1)
            .MovimentarAsync("req-003-estorno", Arg.Any<string?>(), 100m, "C", Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_TransferenciaValida_DevePersistirERegistrarIdempotencia()
    {
        // Arrange
        var command = new EfetuarTransferenciaCommand("req-004", "11111111", "22222222", 200m, "token");
        _idempotenciaRepo.ObterPorChaveAsync(Arg.Any<string>()).Returns((Idempotencia?)null);
        _contaCorrenteClient.MovimentarAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _transferenciaRepo.Received(1).InserirAsync(Arg.Any<Domain.Entities.Transferencia>());
        await _idempotenciaRepo.Received(1).InserirAsync(Arg.Any<Idempotencia>());
    }
}
