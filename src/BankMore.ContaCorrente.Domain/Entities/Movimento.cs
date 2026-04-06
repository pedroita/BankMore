using BankMore.ContaCorrente.Domain.Enums;

namespace BankMore.ContaCorrente.Domain.Entities;

public class Movimento
{
    public Guid Id { get; set; }
    public string IdRequisicao { get; set; } = string.Empty;
    public string NumeroConta { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public TipoMovimento Tipo { get; set; }
    public DateTime CriadoEm { get; set; }

    public Movimento() { }

    public Movimento(string idRequisicao, string numeroConta, decimal valor, TipoMovimento tipo)
    {
        Id = Guid.NewGuid();
        IdRequisicao = idRequisicao;
        NumeroConta = numeroConta;
        Valor = valor;
        Tipo = tipo;
        CriadoEm = DateTime.UtcNow;
    }
}