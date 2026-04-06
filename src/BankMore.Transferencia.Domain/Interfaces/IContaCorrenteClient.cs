namespace BankMore.Transferencia.Domain.Interfaces;

public interface IContaCorrenteClient
{
    Task MovimentarAsync(string idRequisicao, string? numeroConta, decimal valor, string tipo, string token);
}
