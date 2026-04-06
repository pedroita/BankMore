using Refit;

namespace BankMore.Transferencia.Infrastructure.HttpClients;

public record MovimentarRequest(
    string IdRequisicao,
    string? NumeroConta,
    decimal Valor,
    string Tipo);

public interface IContaCorrenteApi
{
    [Post("/api/conta-corrente/movimentar")]
    Task MovimentarAsync(
        [Body] MovimentarRequest request,
        [Header("Authorization")] string authorization);
}
