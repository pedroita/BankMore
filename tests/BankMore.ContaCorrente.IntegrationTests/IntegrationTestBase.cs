using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace BankMore.ContaCorrente.IntegrationTests;

public abstract class IntegrationTestBase : IClassFixture<ContaCorrenteWebFactory>
{
    protected readonly HttpClient Client;

    protected readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected IntegrationTestBase(ContaCorrenteWebFactory factory)
    {
        Client = factory.CreateClient();
    }

    protected async Task<string> CadastrarContaAsync(
        string cpf = "529.982.247-25",
        string nome = "Usuário Teste",
        string senha = "Senha@123")
    {
        var response = await Client.PostAsJsonAsync("/api/conta-corrente/cadastrar", new
        {
            cpf,
            nome,
            senha
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("numeroConta").GetString()!;
    }

    protected async Task<string> LoginAsync(string identificador, string senha = "Senha@123")
    {
        var response = await Client.PostAsJsonAsync("/api/conta-corrente/login", new
        {
            identificador,
            senha
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("token").GetString()!;
    }

    protected void UsarToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    protected async Task MovimentarAsync(string idRequisicao, decimal valor, string tipo, string? numeroConta = null)
    {
        await Client.PostAsJsonAsync("/api/conta-corrente/movimentar", new
        {
            idRequisicao,
            numeroConta,
            valor,
            tipo
        });
    }
}
