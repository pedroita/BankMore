using BankMore.Transferencia.Domain.Enums;

namespace BankMore.Transferencia.Application.Common;

public class DomainException : Exception
{
    public TipoFalha TipoFalha { get; }

    public DomainException(string mensagem, TipoFalha tipoFalha)
        : base(mensagem)
    {
        TipoFalha = tipoFalha;
    }
}
