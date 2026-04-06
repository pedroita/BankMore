namespace BankMore.ContaCorrente.Domain.ValueObjects;

public class Cpf
{
    public string Valor { get; }

    public Cpf(string valor)
    {
        var cpf = valor?.Replace(".", "").Replace("-", "").Trim() ?? "";
        if (!Validar(cpf))
            throw new ArgumentException("CPF inválido.");
        Valor = cpf;
    }

    public static bool Validar(string cpf)
    {
        cpf = cpf?.Replace(".", "").Replace("-", "").Trim() ?? "";
        if (cpf.Length != 11 || cpf.All(c => c == cpf[0])) return false;

        int[] multiplicadores1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] multiplicadores2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        var soma = cpf.Take(9).Select((c, i) => int.Parse(c.ToString()) * multiplicadores1[i]).Sum();
        var resto = soma % 11;
        var digito1 = resto < 2 ? 0 : 11 - resto;

        soma = cpf.Take(10).Select((c, i) => int.Parse(c.ToString()) * multiplicadores2[i]).Sum();
        resto = soma % 11;
        var digito2 = resto < 2 ? 0 : 11 - resto;

        return cpf[9] == char.Parse(digito1.ToString()) && cpf[10] == char.Parse(digito2.ToString());
    }

    public override string ToString() => Valor;
}
