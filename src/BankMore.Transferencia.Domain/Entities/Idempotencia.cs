namespace BankMore.Transferencia.Domain.Entities;

public class Idempotencia
{
    public string ChaveIdempotencia { get; set; } = string.Empty;
    public string? Requisicao { get; set; }
    public string? Resultado { get; set; }

    public Idempotencia() { }

    public Idempotencia(string chave, string requisicao, string resultado)
    {
        ChaveIdempotencia = chave;
        Requisicao = requisicao;
        Resultado = resultado;
    }
}
