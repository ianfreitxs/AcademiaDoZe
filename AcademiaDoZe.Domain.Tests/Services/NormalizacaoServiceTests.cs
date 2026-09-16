// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Services;

namespace AcademiaDoZe.Domain.Tests.Services;

public class NormalizacaoServiceTests
{
    [Theory]
    [InlineData(null, true)]
    [InlineData("", true)]
    [InlineData(" ", true)]
    [InlineData("texto", false)]
    [InlineData(" texto ", false)]
    public void Deve_TextoVazioOuNulo_RetornarEsperado(string? input, bool expected)
    {
        Assert.Equal(expected, NormalizacaoService.TextoVazioOuNulo(input));
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" a b c ", "a b c")]
    [InlineData("a\tb\nc", "a b c")]
    [InlineData("  um   dois  ", "um dois")]
    [InlineData("x  y", "x y")]
    public void Deve_Normalizar_Espacos(string? input, string expected)
    {
        Assert.Equal(expected, NormalizacaoService.LimparEspacos(input));
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("a b c", "abc")]
    [InlineData(" a b ", "ab")]
    [InlineData("a\tb", "ab")]
    [InlineData("1 2 3", "123")]
    public void Deve_Remover_Todos_Espacos(string? input, string expected)
    {
        Assert.Equal(expected, NormalizacaoService.LimparTodosEspacos(input));
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("abc", "ABC")]
    [InlineData("áéíõç", "ÁÉÍÕÇ")]
    [InlineData("Sp", "SP")]
    [InlineData("brasil", "BRASIL")]
    public void Deve_Converter_ParaMaiusculo(string? input, string expected)
    {
        Assert.Equal(expected, NormalizacaoService.ParaMaiusculo(input));
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("a1b2c3", "123")]
    [InlineData("(11) 91234-5678", "11912345678")]
    [InlineData("no-digits", "")]
    [InlineData("12.345-67", "1234567")]
    public void Deve_Manter_Apenas_Digitos(string? input, string expected)
    {
        Assert.Equal(expected, NormalizacaoService.LimparEDigitos(input));
    }
}