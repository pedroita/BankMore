using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace BankMore.Transferencia.IntegrationTests;

public abstract class TransferenciaTestBase : IClassFixture<TransferenciaWebFactory>
{
    protected readonly HttpClient Client;
    protected readonly TransferenciaWebFactory Factory;

    private const string SecretKey = "BankMore@SuperSecretKey2024!ContaCorrente#256bits";
    private const string Issuer = "BankMore.ContaCorrente";

    protected TransferenciaTestBase(TransferenciaWebFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    // Gera token JWT válido com numeroConta embutido — simula o token da API Conta Corrente
    protected string GerarToken(string numeroConta)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("numeroConta", numeroConta),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Issuer,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    protected void UsarToken(string numeroConta)
    {
        var token = GerarToken(numeroConta);
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }
}
