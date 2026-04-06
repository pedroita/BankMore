namespace BankMore.ContaCorrente.Domain.Entities;

public class ContaCorrente
{
    public Guid Id { get; set; }
    public string NumeroConta { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }

    public ContaCorrente() { }

    public ContaCorrente(string cpf, string nome, string senhaHash)
    {
        Id = Guid.NewGuid();
        NumeroConta = GerarNumeroConta();
        Cpf = cpf;
        Nome = nome;
        SenhaHash = senhaHash;
        Ativo = true;
        CriadoEm = DateTime.UtcNow;
    }

    public void Inativar() => Ativo = false;

    private static string GerarNumeroConta() =>
        new Random().Next(10000000, 99999999).ToString();
}