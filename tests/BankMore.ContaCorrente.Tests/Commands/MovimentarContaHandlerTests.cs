using BankMore.ContaCorrente.Application.Commands.MovimentarConta;
using BankMore.ContaCorrente.Application.Common;
using BankMore.ContaCorrente.Domain.Enums;
using BankMore.ContaCorrente.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace BankMore.ContaCorrente.Tests.Commands;

public class MovimentarContaHandlerTests
{
    private readonly IContaCorrenteRepository _contaRepo;
    private readonly IMovimentoRepository _movimentoRepo;
    private readonly MovimentarContaHandler _handler;

    public MovimentarContaHandlerTests()
    {
        _contaRepo = Substitute.For<IContaCorrenteRepository>();
        _movimentoRepo = Substitute.For<IMovimentoRepository>();
        _handler = new MovimentarContaHandler(_contaRepo, _movimentoRepo);
    }

    private Domain.Entities.ContaCorrente CriarContaAtiva(string numero = "12345678") =>
        (Domain.Entities.ContaCorrente)Activator.CreateInstance(
            typeof(Domain.Entities.ContaCorrente),
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
            null,
            Array.Empty<object>(),
            null)!
        .Apply(c =>
        {
            typeof(Domain.Entities.ContaCorrente).GetProperty("NumeroConta")!.SetValue(c, numero);
            typeof(Domain.Entities.ContaCorrente).GetProperty("Ativo")!.SetValue(c, true);
            typeof(Domain.Entities.ContaCorrente).GetProperty("Nome")!.SetValue(c, "Ana Silva");
        });

    [Fact]
    public async Task Handle_RequisicaoDuplicada_DeveIgnorarIdempotente()
    {
        var command = new MovimentarContaCommand("req-dup", "12345678", null, 100m, "C");
        _movimentoRepo.ExisteIdRequisicaoAsync("req-dup").Returns(true);

        await _handler.Handle(command, CancellationToken.None);

        await _movimentoRepo.DidNotReceive().InserirAsync(Arg.Any<Domain.Entities.Movimento>());
    }

    [Fact]
    public async Task Handle_ValorNegativo_DeveLancarDomainException()
    {
        var command = new MovimentarContaCommand("req-001", "12345678", null, -50m, "C");
        _movimentoRepo.ExisteIdRequisicaoAsync(Arg.Any<string>()).Returns(false);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.TipoFalha == TipoFalha.INVALID_VALUE);
    }

    [Fact]
    public async Task Handle_TipoInvalido_DeveLancarDomainException()
    {
        var command = new MovimentarContaCommand("req-002", "12345678", null, 100m, "X");
        _movimentoRepo.ExisteIdRequisicaoAsync(Arg.Any<string>()).Returns(false);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.TipoFalha == TipoFalha.INVALID_TYPE);
    }

    [Fact]
    public async Task Handle_DebitoContaDiferente_DeveLancarInvalidType()
    {
        var command = new MovimentarContaCommand("req-003", "12345678", "99999999", 100m, "D");
        _movimentoRepo.ExisteIdRequisicaoAsync(Arg.Any<string>()).Returns(false);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.TipoFalha == TipoFalha.INVALID_TYPE);
    }

    [Fact]
    public async Task Handle_ContaNaoEncontrada_DeveLancarInvalidAccount()
    {
        var command = new MovimentarContaCommand("req-004", "12345678", null, 100m, "C");
        _movimentoRepo.ExisteIdRequisicaoAsync(Arg.Any<string>()).Returns(false);
        _contaRepo.ObterPorNumeroContaAsync("12345678").Returns((Domain.Entities.ContaCorrente?)null);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.TipoFalha == TipoFalha.INVALID_ACCOUNT);
    }
}

file static class ObjectExtensions
{
    public static T Apply<T>(this T obj, Action<T> action) { action(obj); return obj; }
}
