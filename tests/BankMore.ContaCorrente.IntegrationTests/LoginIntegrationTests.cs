using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace BankMore.ContaCorrente.IntegrationTests;

public class LoginIntegrationTests : IntegrationTestBase
{
    public LoginIntegrationTests(ContaCorrenteWebFactory factory) : base(factory) { }

    [Fact]
    public async Task Login_ComNumeroConta_DeveRetornarToken()
    {
        // Arrange
        var numeroConta = await CadastrarContaAsync("111.444.777-35", "João Login", "Senha@123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/conta-corrente/login", new
        {
            identificador = numeroConta,
            senha = "Senha@123"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_ComCpf_DeveRetornarToken()
    {
        // Arrange
        await CadastrarContaAsync("706.502.960-65", "Maria Login", "Senha@123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/conta-corrente/login", new
        {
            identificador = "706.502.960-65",
            senha = "Senha@123"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_SenhaErrada_DeveRetornar401()
    {
        // Arrange
        var numeroConta = await CadastrarContaAsync("568.994.040-30", "Pedro Login", "Senha@123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/conta-corrente/login", new
        {
            identificador = numeroConta,
            senha = "SenhaErrada"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("tipo").GetString().Should().Be("USER_UNAUTHORIZED");
    }

    [Fact]
    public async Task Login_ContaInexistente_DeveRetornar401()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/conta-corrente/login", new
        {
            identificador = "00000000",
            senha = "Senha@123"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
