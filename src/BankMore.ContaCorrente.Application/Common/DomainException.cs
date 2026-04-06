using BankMore.ContaCorrente.Domain.Enums;

namespace BankMore.ContaCorrente.Application.Common;

public class DomainException : Exception
{
    public TipoFalha TipoFalha { get; }

    public DomainException(string mensagem, TipoFalha tipoFalha)
        : base(mensagem)
    {
        TipoFalha = tipoFalha;
    }
}
