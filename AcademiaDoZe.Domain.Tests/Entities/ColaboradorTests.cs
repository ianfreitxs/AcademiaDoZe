// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class ColaboradorTests
{
    private static Logradouro GetValidLogradouro() => Helpers.GetValidLogradouro();
    private static AcademiaDoZe.Domain.ValueObjects.Arquivo GetValidArquivo() => Helpers.GetValidArquivo();

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Deve_Tratar_DataAdmissao(bool useDefault)
    {
        var data = useDefault ? default : DateOnly.FromDateTime(DateTime.Today.AddYears(-1));
        var result = Colaborador.Criar(1, "Fulano", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)), "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo(), data, ColaboradorTipo.Atendente, ColaboradorVinculo.CLT);
        Assert.Equal(!useDefault, result.IsSuccess);
    }

    [Theory]
    [InlineData(ColaboradorTipo.Administrador, ColaboradorVinculo.Estagio, false)]
    [InlineData(ColaboradorTipo.Administrador, ColaboradorVinculo.CLT, true)]
    [InlineData(ColaboradorTipo.Atendente, ColaboradorVinculo.CLT, true)]
    [InlineData(ColaboradorTipo.Instrutor, ColaboradorVinculo.Estagio, true)]
    public void Deve_Validar_TipoEVinculo(ColaboradorTipo tipo, ColaboradorVinculo vinculo, bool expected)
    {
        var result = Colaborador.Criar(1, "Fulano", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)), "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo(), DateOnly.FromDateTime(DateTime.Today.AddYears(-1)), tipo, vinculo);
        Assert.Equal(expected, result.IsSuccess);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Deve_Falhar_DataAdmissaoFutura(int daysOffset)
    {
        var result = Colaborador.Criar(1, "Fulano", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)), "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo(), DateOnly.FromDateTime(DateTime.Today.AddDays(daysOffset)), ColaboradorTipo.Atendente, ColaboradorVinculo.CLT);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "DATA_ADMISSAO_MAIOR_QUE_ATUAL");
    }

    [Theory]
    [InlineData(999, 1)]
    [InlineData(1, 999)]
    [InlineData(999, 999)]
    public void Deve_Falhar_TipoOuVinculoInvalido(int tipoValue, int vinculoValue)
    {
        var result = Colaborador.Criar(1, "Fulano", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)), "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo(), DateOnly.FromDateTime(DateTime.Today.AddYears(-1)), (ColaboradorTipo)tipoValue, (ColaboradorVinculo)vinculoValue);
        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Deve_Falhar_NomeObrigatorio()
    {
        Assert.True(Colaborador.Criar(1, "", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)), "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo(), DateOnly.FromDateTime(DateTime.Today.AddYears(-1)), ColaboradorTipo.Atendente, ColaboradorVinculo.CLT).IsFailure);
    }

    [Fact]
    public void Deve_Falhar_DataNascimentoInvalida()
    {
        Assert.True(Colaborador.Criar(1, "Fulano", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-10)), "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo(), DateOnly.FromDateTime(DateTime.Today.AddYears(-1)), ColaboradorTipo.Atendente, ColaboradorVinculo.CLT).IsFailure);
    }

    [Fact]
    public void Deve_Criar_Administrador_CLT()
    {
        var result = Colaborador.Criar(2, "Administrador", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)), "(11) 91234-5678", "admin@example.com", GetValidLogradouro(), "10", "Sala", "Abcdef", GetValidArquivo(), DateOnly.FromDateTime(DateTime.Today.AddYears(-2)), ColaboradorTipo.Administrador, ColaboradorVinculo.CLT);
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Deve_Falhar_EmailInvalido()
    {
        Assert.True(Colaborador.Criar(1, "Fulano", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)), "(11) 91234-5678", "email", GetValidLogradouro(), "123", "", "Abcdef", GetValidArquivo(), DateOnly.FromDateTime(DateTime.Today.AddYears(-1)), ColaboradorTipo.Atendente, ColaboradorVinculo.CLT).IsFailure);
    }

    [Fact]
    public void Deve_Falhar_SenhaInvalida()
    {
        Assert.True(Colaborador.Criar(1, "Fulano", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)), "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "123", "", "abcdef", GetValidArquivo(), DateOnly.FromDateTime(DateTime.Today.AddYears(-1)), ColaboradorTipo.Atendente, ColaboradorVinculo.CLT).IsFailure);
    }

    [Fact]
    public void Deve_Falhar_EnderecoInvalido()
    {
        Assert.True(Colaborador.Criar(1, "Fulano", "529.982.247-25", DateOnly.FromDateTime(DateTime.Today.AddYears(-30)), "(11) 91234-5678", "user@example.com", GetValidLogradouro(), "", "", "Abcdef", GetValidArquivo(), DateOnly.FromDateTime(DateTime.Today.AddYears(-1)), ColaboradorTipo.Atendente, ColaboradorVinculo.CLT).IsFailure);
    }
}
