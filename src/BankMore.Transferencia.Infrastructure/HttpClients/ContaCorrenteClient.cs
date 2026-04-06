using BankMore.Transferencia.Domain.Interfaces;

namespace BankMore.Transferencia.Infrastructure.HttpClients;

public class ContaCorrenteClient : IContaCorrenteClient
{
    private readonly IContaCorrenteApi _api;

    public ContaCorrenteClient(IContaCorrenteApi api)
    {
        _api = api;
    }

    public async Task MovimentarAsync(
        string idRequisicao,
        string? numeroConta,
        decimal valor,
        string tipo,
        string token)
    {
        var request = new MovimentarRequest(idRequisicao, numeroConta, valor, tipo);
        await _api.MovimentarAsync(request, $"Bearer {token}");
    }
}
