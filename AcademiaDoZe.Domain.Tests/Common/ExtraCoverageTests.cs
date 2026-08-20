// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Enums;
using AcademiaDoZe.Domain.ValueObjects;

namespace AcademiaDoZe.Domain.Tests.Common;

public class ExtraCoverageTests
{
    [Theory]
    [InlineData(ColaboradorTipo.Administrador)]
    [InlineData(ColaboradorTipo.Atendente)]
    [InlineData(ColaboradorTipo.Instrutor)]
    public void Deve_Conhecer_TiposColaborador(ColaboradorTipo tipo)
    {
        Assert.True(Enum.IsDefined(tipo));
    }

    [Theory]
    [InlineData(ColaboradorVinculo.CLT)]
    [InlineData(ColaboradorVinculo.Estagio)]
    public void Deve_Conhecer_VinculosColaborador(ColaboradorVinculo vinculo)
    {
        Assert.True(Enum.IsDefined(vinculo));
    }

    [Theory]
    [InlineData(MatriculaPlano.Mensal, 0)]
    [InlineData(MatriculaPlano.Trimestral, 1)]
    [InlineData(MatriculaPlano.Semestral, 2)]
    [InlineData(MatriculaPlano.Anual, 3)]
    public void Deve_Conhecer_Planos(MatriculaPlano plano, int expected)
    {
        Assert.Equal(expected, (int)plano);
    }

    [Theory]
    [InlineData(MatriculaRestricoes.None)]
    [InlineData(MatriculaRestricoes.Diabetes)]
    [InlineData(MatriculaRestricoes.PressaoAlta)]
    [InlineData(MatriculaRestricoes.Labirintite)]
    [InlineData(MatriculaRestricoes.Alergias)]
    [InlineData(MatriculaRestricoes.ProblemasRespiratorios)]
    [InlineData(MatriculaRestricoes.RemedioContinuo)]
    public void Deve_Conhecer_Restricoes(MatriculaRestricoes restricao)
    {
        Assert.True(Enum.IsDefined(restricao));
    }

    [Theory]
    [InlineData("12345-678")]
    [InlineData("12345678")]
    [InlineData("12.345-678")]
    [InlineData("123-45678")]
    [InlineData("1234.5678")]
    public void Deve_Aceitar_Formatacoes_De_Cep(string input)
    {
        Assert.True(Cep.Criar(input).IsSuccess);
    }

    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    public void Deve_Aceitar_Formatacoes_De_Cpf(string input)
    {
        Assert.True(Cpf.Criar(input).IsSuccess);
    }

    [Theory]
    [InlineData("(11) 91234-5678")]
    [InlineData("11912345678")]
    [InlineData("11 91234-5678")]
    [InlineData("11-91234-5678")]
    [InlineData("11.91234-5678")]
    public void Deve_Aceitar_Formatacoes_De_Telefone(string input)
    {
        Assert.True(Telefone.Criar(input).IsSuccess);
    }
}
