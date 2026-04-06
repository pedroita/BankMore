using BankMore.ContaCorrente.Application.Common;
using BankMore.ContaCorrente.Application.Queries.ConsultarSaldo;
using BankMore.ContaCorrente.Domain.Enums;
using BankMore.ContaCorrente.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace BankMore.ContaCorrente.Tests.Queries;

public class ConsultarSaldoHandlerTests
{
    private readonly IContaCorrenteRepository _contaRepo;
    private readonly IMovimentoRepository _movimentoRepo;
    private readonly ConsultarSaldoHandler _handler;

    public ConsultarSaldoHandlerTests()
    {
        _contaRepo = Substitute.For<IContaCorrenteRepository>();
        _movimentoRepo = Substitute.For<IMovimentoRepository>();
        _handler = new ConsultarSaldoHandler(_contaRepo, _movimentoRepo);
    }

    [Fact]
    public async Task Handle_ContaNaoEncontrada_DeveLancarInvalidAccount()
    {
        var query = new ConsultarSaldoQuery("00000000");
        _contaRepo.ObterPorNumeroContaAsync("00000000").Returns((Domain.Entities.ContaCorrente?)null);

        var act = async () => await _handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.TipoFalha == TipoFalha.INVALID_ACCOUNT);
    }
}
