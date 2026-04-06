namespace BankMore.Transferencia.Domain.Entities;

public class Transferencia
{
    public string IdTransferencia { get; set; } = string.Empty;
    public string IdContaCorrenteOrigem { get; set; } = string.Empty;
    public string IdContaCorrenteDestino { get; set; } = string.Empty;
    public string DataMovimento { get; set; } = string.Empty;
    public decimal Valor { get; set; }

    public Transferencia() { }

    public Transferencia(string idRequisicao, string contaOrigem, string contaDestino, decimal valor)
    {
        IdTransferencia = idRequisicao;
        IdContaCorrenteOrigem = contaOrigem;
        IdContaCorrenteDestino = contaDestino;
        DataMovimento = DateTime.UtcNow.ToString("dd/MM/yyyy");
        Valor = valor;
    }
}
