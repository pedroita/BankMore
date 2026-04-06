namespace BankMore.ContaCorrente.Domain.Enums;

public enum TipoMovimento
{
    C, // Crédito
    D  // Débito
}

public enum TipoFalha
{
    INVALID_DOCUMENT,
    INVALID_ACCOUNT,
    INACTIVE_ACCOUNT,
    INVALID_VALUE,
    INVALID_TYPE,
    USER_UNAUTHORIZED,
    INSUFFICIENT_FUNDS
}