using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace BankMore.ContaCorrente.IntegrationTests;

public class CadastrarContaIntegrationTests : IntegrationTestBase
{
    public CadastrarContaIntegrationTests(ContaCorrenteWebFactory factory) : base(factory) { }

    [Fact]
    public async Task Cadastrar_CpfValido_DeveRetornar200ComNumeroConta()
    {
        // Arrange
        var request = new { cpf = "529.982.247-25", nome = "Ana Silva", senha = "Senha@123" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/conta-corrente/cadastrar", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("numeroConta").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Cadastrar_CpfInvalido_DeveRetornar400ComInvalidDocument()
    {
        // Arrange
        var request = new { cpf = "111.111.111-11", nome = "Teste", senha = "Senha@123" };

        // Act
        var response = await Client.PostAsJsonAsync("/api/conta-corrente/cadastrar", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("tipo").GetString().Should().Be("INVALID_DOCUMENT");
    }
}
