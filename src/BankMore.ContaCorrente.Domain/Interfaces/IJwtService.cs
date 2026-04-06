namespace BankMore.ContaCorrente.Domain.Interfaces;

public interface IJwtService
{
    string GerarToken(string numeroConta);
    string? ObterNumeroConta(string token);
}
