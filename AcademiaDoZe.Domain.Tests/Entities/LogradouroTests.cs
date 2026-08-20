// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class LogradouroTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Deve_Falhar_NomeVazio(string? nome)
    {
        var result = Logradouro.Criar(1, "12345-678", nome, "Bairro", "Cidade", "SP", "Brasil");
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "NOME_OBRIGATORIO");
    }

    [Theory]
    [InlineData(" s p ", "SP")]
    [InlineData(" sp ", "SP")]
    [InlineData("S P", "SP")]
    [InlineData(" rj ", "RJ")]
    public void Deve_Normalizar_Estado(string input, string expected)
    {
        var result = Logradouro.Criar(1, "12345-678", "Rua", "Bairro", "Cidade", input, "Brasil");
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value!.Estado);
    }

    [Fact]
    public void Deve_Falhar_BairroObrigatorio()
    {
        Assert.True(Logradouro.Criar(1, "12345-678", "Rua", "", "Cidade", "SP", "Brasil").IsFailure);
    }

    [Fact]
    public void Deve_Falhar_CidadeObrigatoria()
    {
        Assert.True(Logradouro.Criar(1, "12345-678", "Rua", "Bairro", "", "SP", "Brasil").IsFailure);
    }

    [Fact]
    public void Deve_Falhar_EstadoObrigatorio()
    {
        Assert.True(Logradouro.Criar(1, "12345-678", "Rua", "Bairro", "Cidade", "", "Brasil").IsFailure);
    }

    [Fact]
    public void Deve_Falhar_PaisObrigatorio()
    {
        Assert.True(Logradouro.Criar(1, "12345-678", "Rua", "Bairro", "Cidade", "SP", "").IsFailure);
    }

    [Fact]
    public void Deve_Falhar_CepInvalido()
    {
        Assert.True(Logradouro.Criar(1, "123", "Rua", "Bairro", "Cidade", "SP", "Brasil").IsFailure);
    }

    [Theory]
    [InlineData("Rua")]
    [InlineData("Avenida")]
    [InlineData("Travessa")]
    public void Deve_Criar_ComNomeValido(string nome)
    {
        var result = Logradouro.Criar(10, "12345-678", nome, "Bairro", "Cidade", "sp", "Brasil");
        Assert.True(result.IsSuccess);
        Assert.Equal(nome, result.Value!.Nome);
    }

    [Fact]
    public void Deve_Normalizar_Cep()
    {
        var result = Logradouro.Criar(1, "12345-678", "Rua", "Bairro", "Cidade", "SP", "Brasil");
        Assert.Equal("12345678", result.Value!.Cep.Valor);
    }

    [Fact]
    public void Deve_Atribuir_Id()
    {
        var result = Logradouro.Criar(99, "12345-678", "Rua", "Bairro", "Cidade", "SP", "Brasil");
        Assert.Equal(99, result.Value!.Id);
    }
}
