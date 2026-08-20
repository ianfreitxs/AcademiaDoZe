// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.ValueObjects;

public class ValueObjectsTests
{
    [Theory]
    [InlineData("123")]
    [InlineData("12-345")]
    [InlineData("1234567")]
    [InlineData("123456789")]
    [InlineData("abcdefghi")]
    [InlineData("1-2-3")]
    public void Deve_Falhar_Cep_DigitosInvalidos(string input)
    {
        Assert.True(Cep.Criar(input).IsFailure);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Cep_Obrigatorio(string? input)
    {
        var result = Cep.Criar(input);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CEP_OBRIGATORIO");
    }

    [Theory]
    [InlineData("12345-678", "12345678")]
    [InlineData("12345678", "12345678")]
    [InlineData(" 12345-678 ", "12345678")]
    public void Deve_Criar_Cep_Valido(string input, string expected)
    {
        var result = Cep.Criar(input);
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value!.Valor);
        Assert.Equal(expected, result.Value.ToString());
    }

    [Theory]
    [InlineData("529.982.247-25", "52998224725")]
    [InlineData("52998224725", "52998224725")]
    public void Deve_Criar_Cpf_Valido(string input, string expected)
    {
        var result = Cpf.Criar(input);
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value!.Valor);
        Assert.Equal(expected, result.Value.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Cpf_Obrigatorio(string? input)
    {
        var result = Cpf.Criar(input);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CPF_OBRIGATORIO");
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("dfgdf")]
    [InlineData("123")]
    [InlineData("1234567890")]
    public void Deve_Falhar_Cpf_DigitosInvalidos(string input)
    {
        Assert.True(Cpf.Criar(input).IsFailure);
    }

    [Theory]
    [InlineData("123.456.789-00")]
    [InlineData("111.111.111-11")]
    [InlineData("000.000.000-00")]
    public void Deve_Falhar_Cpf_Validador(string input)
    {
        var result = Cpf.Criar(input);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "CPF_INVALIDO");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("nome.sobrenome@example.com")]
    [InlineData("user@empresa.com.br")]
    [InlineData("a@b.co")]
    [InlineData("x@y.io")]
    public void Deve_Criar_Email_Valido(string input)
    {
        Assert.True(Email.Criar(input).IsSuccess);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abc")]
    [InlineData("@teste.com")]
    [InlineData("a@")]
    [InlineData("a@.")]
    [InlineData("a@teste.")]
    [InlineData("a..b@teste.com")]
    public void Deve_Falhar_Email_Invalido(string? input)
    {
        Assert.True(Email.Criar(input).IsFailure);
    }

    [Theory]
    [InlineData("(11) 91234-5678", "11912345678")]
    [InlineData("11912345678", "11912345678")]
    [InlineData("11 91234-5678", "11912345678")]
    [InlineData("11-91234-5678", "11912345678")]
    public void Deve_Criar_Telefone_Valido(string input, string expected)
    {
        var result = Telefone.Criar(input);
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value!.Valor);
        Assert.Equal(expected, result.Value.ToString());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("1234")]
    [InlineData("(1)2345")]
    [InlineData("123456789012")]
    public void Deve_Falhar_Telefone_Invalido(string? input)
    {
        Assert.True(Telefone.Criar(input).IsFailure);
    }

    [Theory]
    [InlineData("Abcdef")]
    [InlineData("Senha1")]
    [InlineData("A12345")]
    [InlineData("SenhaForte")]
    public void Deve_Criar_Senha_Valida(string input)
    {
        Assert.True(Senha.Criar(input).IsSuccess);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("abcdef")]
    [InlineData("ABC")]
    [InlineData("abc123")]
    public void Deve_Falhar_Senha_Invalida(string? input)
    {
        Assert.True(Senha.Criar(input).IsFailure);
    }

    [Fact]
    public void Deve_Criar_Arquivo_Valido()
    {
        var result = Arquivo.Criar([1, 2, 3]);
        Assert.True(result.IsSuccess);
        Assert.Equal(3, result.Value!.Conteudo.Length);
    }

    [Fact]
    public void Deve_Falhar_Arquivo_Nulo()
    {
        Assert.True(Arquivo.Criar(null).IsFailure);
    }

    [Fact]
    public void Deve_Criar_Arquivo_NoLimite()
    {
        var result = Arquivo.Criar(new byte[15 * 1024 * 1024]);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Deve_Falhar_Arquivo_AcimaDoLimite()
    {
        var result = Arquivo.Criar(new byte[(15 * 1024 * 1024) + 1]);
        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData("10", "Bloco A")]
    [InlineData("1", "")]
    [InlineData("10A", "Casa")]
    [InlineData("20", "Fundos")]
    public void Deve_Criar_Endereco_Valido(string numero, string complemento)
    {
        var result = Endereco.Criar(Helpers.GetValidLogradouro(), numero, complemento);
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value!.LogradouroId);
        Assert.Equal(numero, result.Value.Numero);
        Assert.Equal(complemento, result.Value.Complemento);
    }

    [Fact]
    public void Deve_Falhar_Endereco_LogradouroNulo()
    {
        Assert.True(Endereco.Criar(null, "1", "").IsFailure);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_Endereco_NumeroVazio(string? numero)
    {
        Assert.True(Endereco.Criar(Helpers.GetValidLogradouro(), numero, "").IsFailure);
    }
}
