// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class AlunoTests
{
    private static Logradouro GetValidLogradouro() => Helpers.GetValidLogradouro();
    private static AcademiaDoZe.Domain.ValueObjects.Arquivo GetValidArquivo() => Helpers.GetValidArquivo();

    [Theory]
    [InlineData(" João da Silva ")]
    [InlineData("Maria")]
    [InlineData("Carlos Teste")]
    public void Deve_Criar_ComNomeValido(string nome)
    {
        var result = Aluno.Criar(1, nome, "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)), "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo());
        Assert.True(result.IsSuccess);
        Assert.Equal(nome.Trim(), result.Value!.Nome);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Deve_Falhar_NomeVazio(string? nome)
    {
        var result = Aluno.Criar(1, nome, "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)), "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo());
        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData("default")]
    [InlineData("under12")]
    public void Deve_Falhar_DataNascimentoInvalida(string scenario)
    {
        var data = scenario == "default" ? default : DateOnly.FromDateTime(DateTime.Today.AddYears(-10));
        var result = Aluno.Criar(1, "João", "529.982.247-25", data, "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo());
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Deve_Falhar_CpfInvalido()
    {
        var result = Aluno.Criar(1, "João", "123.456.789-00", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)), "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo());
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Deve_Falhar_TelefoneInvalido()
    {
        Assert.True(Aluno.Criar(1, "João", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)), "1234", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo()).IsFailure);
    }

    [Fact]
    public void Deve_Falhar_EmailInvalido()
    {
        Assert.True(Aluno.Criar(1, "João", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)), "(11) 91234-5678", "email", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo()).IsFailure);
    }

    [Fact]
    public void Deve_Falhar_SenhaInvalida()
    {
        Assert.True(Aluno.Criar(1, "João", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)), "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "abcdef", GetValidArquivo()).IsFailure);
    }

    [Fact]
    public void Deve_Falhar_EnderecoInvalido()
    {
        Assert.True(Aluno.Criar(1, "João", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)), "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "", "", "Abcdef", GetValidArquivo()).IsFailure);
    }

    [Fact]
    public void Deve_Falhar_LogradouroNulo()
    {
        Assert.True(Aluno.Criar(1, "João", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)), "(11) 91234-5678", "user@example.com", null, "123", "", "Abcdef", GetValidArquivo()).IsFailure);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(99)]
    public void Deve_Preservar_Id(int id)
    {
        var result = Aluno.Criar(id, "João", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)), "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo());
        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value!.Id);
    }

    [Fact]
    public void Deve_Usar_ValoresNormalizados()
    {
        var result = Aluno.Criar(1, " João ", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-25)), "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo());
        Assert.True(result.IsSuccess);
        Assert.Equal("João", result.Value!.Nome);
        Assert.Equal("52998224725", result.Value.Cpf.Valor);
        Assert.Equal("11912345678", result.Value.Telefone.Valor);
    }
}
