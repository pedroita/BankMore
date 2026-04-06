using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace BankMore.ContaCorrente.IntegrationTests;

public class ConsultarSaldoIntegrationTests : IntegrationTestBase
{
    public ConsultarSaldoIntegrationTests(ContaCorrenteWebFactory factory) : base(factory) { }

    [Fact]
    public async Task ConsultarSaldo_ContaSemMovimentos_DeveRetornarZero()
    {
        // Arrange
        var numeroConta = await CadastrarContaAsync("893.013.170-08", "Felipe Saldo", "Senha@123");
        var token = await LoginAsync(numeroConta);
        UsarToken(token);

        // Act
        var response = await Client.GetAsync("/api/conta-corrente/saldo");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("saldo").GetDecimal().Should().Be(0m);
        body.GetProperty("numeroConta").GetString().Should().Be(numeroConta);
    }

    [Fact]
    public async Task ConsultarSaldo_AposMovimentacoes_DeveRetornarSaldoCorreto()
    {
        // Arrange
        var numeroConta = await CadastrarContaAsync("051.778.730-90", "Gabi Saldo", "Senha@123");
        var token = await LoginAsync(numeroConta);
        UsarToken(token);

        await Client.PostAsJsonAsync("/api/conta-corrente/movimentar", new
        {
            idRequisicao = $"c1-{Guid.NewGuid()}",
            numeroConta = (string?)null,
            valor = 500m,
            tipo = "C"
        });

        await Client.PostAsJsonAsync("/api/conta-corrente/movimentar", new
        {
            idRequisicao = $"d1-{Guid.NewGuid()}",
            numeroConta = (string?)null,
            valor = 200m,
            tipo = "D"
        });

        // Act
        var response = await Client.GetAsync("/api/conta-corrente/saldo");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("saldo").GetDecimal().Should().Be(300m);
    }

    [Fact]
    public async Task ConsultarSaldo_SemToken_DeveRetornar403()
    {
        // Act
        var response = await Client.GetAsync("/api/conta-corrente/saldo");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}

public class InativarContaIntegrationTests : IntegrationTestBase
{
    public InativarContaIntegrationTests(ContaCorrenteWebFactory factory) : base(factory) { }

    [Fact]
    public async Task Inativar_SenhaCorreta_DeveRetornar204()
    {
        // Arrange
        var numeroConta = await CadastrarContaAsync("867.899.990-47", "Hana Inativa", "Senha@123");
        var token = await LoginAsync(numeroConta);
        UsarToken(token);

        // Act
        var response = await Client.PatchAsJsonAsync("/api/conta-corrente/inativar", new
        {
            senha = "Senha@123"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Inativar_ContaInativa_NaoDevePermitirMovimentacao()
    {
        // Arrange
        var numeroConta = await CadastrarContaAsync("191.560.440-00", "Igor Inativo", "Senha@123");
        var token = await LoginAsync(numeroConta);
        UsarToken(token);

        // Inativa
        await Client.PatchAsJsonAsync("/api/conta-corrente/inativar", new { senha = "Senha@123" });

        // Act — tenta movimentar conta inativa
        var response = await Client.PostAsJsonAsync("/api/conta-corrente/movimentar", new
        {
            idRequisicao = $"req-{Guid.NewGuid()}",
            numeroConta = (string?)null,
            valor = 100m,
            tipo = "C"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("tipo").GetString().Should().Be("INACTIVE_ACCOUNT");
    }

    [Fact]
    public async Task Inativar_SenhaErrada_DeveRetornar401()
    {
        // Arrange
        var numeroConta = await CadastrarContaAsync("769.867.010-00", "Julia Senha", "Senha@123");
        var token = await LoginAsync(numeroConta);
        UsarToken(token);

        // Act
        var response = await Client.PatchAsJsonAsync("/api/conta-corrente/inativar", new
        {
            senha = "SenhaErrada"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
