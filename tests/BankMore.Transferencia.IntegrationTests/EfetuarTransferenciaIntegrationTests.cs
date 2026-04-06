using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BankMore.Transferencia.Domain.Interfaces;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace BankMore.Transferencia.IntegrationTests;

public class EfetuarTransferenciaIntegrationTests : TransferenciaTestBase
{
    public EfetuarTransferenciaIntegrationTests(TransferenciaWebFactory factory) : base(factory) { }

    [Fact]
    public async Task Transferir_Valida_DeveRetornar204()
    {
        // Arrange
        UsarToken("11111111");
        Factory.ContaCorrenteClientMock
            .MovimentarAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);

        // Act
        var response = await Client.PostAsJsonAsync("/api/transferencia/transferir", new
        {
            idRequisicao = $"transf-{Guid.NewGuid()}",
            numeroContaDestino = "22222222",
            valor = 100m
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Transferir_Valida_DeveDebitarOrigemECreditarDestino()
    {
        // Arrange
        UsarToken("33333333");
        var idRequisicao = $"transf-{Guid.NewGuid()}";

        Factory.ContaCorrenteClientMock
            .MovimentarAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);

        // Act
        await Client.PostAsJsonAsync("/api/transferencia/transferir", new
        {
            idRequisicao,
            numeroContaDestino = "44444444",
            valor = 250m
        });

        // Assert — débito chamado na origem
        await Factory.ContaCorrenteClientMock.Received(1)
            .MovimentarAsync(
                $"{idRequisicao}-debito",
                "33333333",
                250m,
                "D",
                Arg.Any<string>());

        // Assert — crédito chamado no destino
        await Factory.ContaCorrenteClientMock.Received(1)
            .MovimentarAsync(
                $"{idRequisicao}-credito",
                "44444444",
                250m,
                "C",
                Arg.Any<string>());
    }

    [Fact]
    public async Task Transferir_Idempotente_NaoDeveDuplicar()
    {
        // Arrange
        UsarToken("55555555");
        var idRequisicao = $"transf-idem-{Guid.NewGuid()}";

        Factory.ContaCorrenteClientMock
            .MovimentarAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(Task.CompletedTask);

        // Act — envia duas vezes
        await Client.PostAsJsonAsync("/api/transferencia/transferir", new
        {
            idRequisicao,
            numeroContaDestino = "66666666",
            valor = 100m
        });

        await Client.PostAsJsonAsync("/api/transferencia/transferir", new
        {
            idRequisicao,
            numeroContaDestino = "66666666",
            valor = 100m
        });

        // Assert — client foi chamado só uma vez (na primeira)
        await Factory.ContaCorrenteClientMock.Received(2) // débito + crédito apenas uma vez
            .MovimentarAsync(Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task Transferir_ValorNegativo_DeveRetornar400()
    {
        // Arrange
        UsarToken("77777777");

        // Act
        var response = await Client.PostAsJsonAsync("/api/transferencia/transferir", new
        {
            idRequisicao = $"transf-{Guid.NewGuid()}",
            numeroContaDestino = "88888888",
            valor = -50m
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("tipo").GetString().Should().Be("INVALID_VALUE");
    }

    [Fact]
    public async Task Transferir_MesmaContaOrigemDestino_DeveRetornar400()
    {
        // Arrange
        UsarToken("99999999");

        // Act
        var response = await Client.PostAsJsonAsync("/api/transferencia/transferir", new
        {
            idRequisicao = $"transf-{Guid.NewGuid()}",
            numeroContaDestino = "99999999", // mesma conta do token
            valor = 100m
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("tipo").GetString().Should().Be("INVALID_ACCOUNT");
    }

    [Fact]
    public async Task Transferir_FalhaNoCredito_DeveEstornarERetornar400()
    {
        // Arrange
        UsarToken("10101010");
        var idRequisicao = $"transf-estorno-{Guid.NewGuid()}";

        // Débito OK
        Factory.ContaCorrenteClientMock
            .MovimentarAsync($"{idRequisicao}-debito", Arg.Any<string?>(), Arg.Any<decimal>(), "D", Arg.Any<string>())
            .Returns(Task.CompletedTask);

        // Crédito falha
        Factory.ContaCorrenteClientMock
            .MovimentarAsync($"{idRequisicao}-credito", Arg.Any<string?>(), Arg.Any<decimal>(), "C", Arg.Any<string>())
            .ThrowsAsync(new Exception("Conta destino inexistente"));

        // Estorno OK
        Factory.ContaCorrenteClientMock
            .MovimentarAsync($"{idRequisicao}-estorno", Arg.Any<string?>(), Arg.Any<decimal>(), "C", Arg.Any<string>())
            .Returns(Task.CompletedTask);

        // Act
        var response = await Client.PostAsJsonAsync("/api/transferencia/transferir", new
        {
            idRequisicao,
            numeroContaDestino = "20202020",
            valor = 100m
        });

        // Assert — retorna erro
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("tipo").GetString().Should().Be("TRANSFER_ERROR");

        // Assert — estorno foi chamado
        await Factory.ContaCorrenteClientMock.Received(1)
            .MovimentarAsync($"{idRequisicao}-estorno", Arg.Any<string?>(), 100m, "C", Arg.Any<string>());
    }

    [Fact]
    public async Task Transferir_SemToken_DeveRetornar403()
    {
        // Act — sem token
        var response = await Client.PostAsJsonAsync("/api/transferencia/transferir", new
        {
            idRequisicao = "transf-sem-token",
            numeroContaDestino = "12345678",
            valor = 100m
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
