using BankMore.ContaCorrente.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace BankMore.ContaCorrente.Tests;

public class CpfValueObjectTests
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    [InlineData("111.444.777-35")]
    public void Cpf_Valido_DevePassarNaValidacao(string cpf)
    {
        var resultado = Cpf.Validar(cpf);

        resultado.Should().BeTrue();
    }

    [Theory]
    [InlineData("111.111.111-11")]
    [InlineData("000.000.000-00")]
    [InlineData("123.456.789-00")]
    [InlineData("")]
    [InlineData("abc")]
    public void Cpf_Invalido_DeveRetornarFalso(string cpf)
    {
        var resultado = Cpf.Validar(cpf);

        resultado.Should().BeFalse();
    }

    [Fact]
    public void Cpf_ValorComMascara_DeveLimparEArmazenar()
    {
        var cpf = new Cpf("529.982.247-25");

        cpf.Valor.Should().Be("52998224725");
    }
}
