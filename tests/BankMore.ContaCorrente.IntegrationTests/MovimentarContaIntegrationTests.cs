using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace BankMore.ContaCorrente.IntegrationTests;

public class MovimentarContaIntegrationTests : IntegrationTestBase
{
    public MovimentarContaIntegrationTests(ContaCorrenteWebFactory factory) : base(factory) { }

    [Fact]
    public async Task Movimentar_Credito_DeveRetornar204()
    {
        // Arrange
        var numeroConta = await CadastrarContaAsync("155.665.390-07", "Ana Mov", "Senha@123");
        var token = await LoginAsync(numeroConta);
        UsarToken(token);

        // Act
        var response = await Client.PostAsJsonAsync("/api/conta-corrente/movimentar", new
        {
            idRequisicao = $"req-{Guid.NewGuid()}",
            numeroConta = (string?)null,
            valor = 500m,
            tipo = "C"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Movimentar_DebitoComSaldoSuficiente_DeveRetornar204()
    {
        // Arrange
        var numeroConta = await CadastrarContaAsync("288.274.656-79", "Bob Mov", "Senha@123");
        var token = await LoginAsync(numeroConta);
        UsarToken(token);

        // Credita primeiro
        await Client.PostAsJsonAsync("/api/conta-corrente/movimentar", new
        {
            idRequisicao = $"req-cred-{Guid.NewGuid()}",
            numeroConta = (string?)null,
            valor = 300m,
            tipo = "C"
        });

        // Act — débito
        var response = await Client.PostAsJsonAsync("/api/conta-corrente/movimentar", new
        {
            idRequisicao = $"req-deb-{Guid.NewGuid()}",
            numeroConta = (string?)null,
            valor = 100m,
            tipo = "D"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Movimentar_DebitoSemSaldo_DeveRetornar400ComInsufficientFunds()
    {
        // Arrange
        var numeroConta = await CadastrarContaAsync("290.591.690-04", "Carlos Mov", "Senha@123");
        var token = await LoginAsync(numeroConta);
        UsarToken(token);

        // Act
        var response = await Client.PostAsJsonAsync("/api/conta-corrente/movimentar", new
        {
            idRequisicao = $"req-{Guid.NewGuid()}",
            numeroConta = (string?)null,
            valor = 999m,
            tipo = "D"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("tipo").GetString().Should().Be("INSUFFICIENT_FUNDS");
    }

    [Fact]
    public async Task Movimentar_IdRequisicaoDuplicado_DeveIgnorarIdempotente()
    {
        // Arrange
        var numeroConta = await CadastrarContaAsync("348.602.680-36", "Diana Idem", "Senha@123");
        var token = await LoginAsync(numeroConta);
        UsarToken(token);
        var idRequisicao = $"req-idem-{Guid.NewGuid()}";

        // Act — envia duas vezes com mesmo ID
        await Client.PostAsJsonAsync("/api/conta-corrente/movimentar", new
        {
            idRequisicao,
            numeroConta = (string?)null,
            valor = 100m,
            tipo = "C"
        });

        await Client.PostAsJsonAsync("/api/conta-corrente/movimentar", new
        {
            idRequisicao,
            numeroConta = (string?)null,
            valor = 100m,
            tipo = "C"
        });

        // Assert — saldo deve ser 100 e não 200
        var saldoResponse = await Client.GetAsync("/api/conta-corrente/saldo");
        var saldo = await saldoResponse.Content.ReadFromJsonAsync<JsonElement>();
        saldo.GetProperty("saldo").GetDecimal().Should().Be(100m);
    }

    [Fact]
    public async Task Movimentar_TipoInvalido_DeveRetornar400ComInvalidType()
    {
        // Arrange
        var numeroConta = await CadastrarContaAsync("517.837.890-04", "Eva Tipo", "Senha@123");
        var token = await LoginAsync(numeroConta);
        UsarToken(token);

        // Act
        var response = await Client.PostAsJsonAsync("/api/conta-corrente/movimentar", new
        {
            idRequisicao = $"req-{Guid.NewGuid()}",
            numeroConta = (string?)null,
            valor = 100m,
            tipo = "X"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("tipo").GetString().Should().Be("INVALID_TYPE");
    }

    [Fact]
    public async Task Movimentar_SemToken_DeveRetornar403()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/conta-corrente/movimentar", new
        {
            idRequisicao = "req-sem-token",
            numeroConta = (string?)null,
            valor = 100m,
            tipo = "C"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
