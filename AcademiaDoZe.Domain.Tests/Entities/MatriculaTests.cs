// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Entities;
using AcademiaDoZe.Domain.Enums;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class MatriculaTests
{
    [Theory]
    [InlineData(999, false)]
    [InlineData((int)MatriculaPlano.Mensal, true)]
    [InlineData((int)MatriculaPlano.Trimestral, true)]
    [InlineData((int)MatriculaPlano.Semestral, true)]
    [InlineData((int)MatriculaPlano.Anual, true)]
    public void Deve_Validar_Plano(int value, bool expected)
    {
        var result = Matricula.Criar(1, Helpers.GetValidAluno(), (MatriculaPlano)value, DateOnly.FromDateTime(DateTime.Today), "Objetivo", MatriculaRestricoes.None, null);
        Assert.Equal(expected, result.IsSuccess);
    }

    [Fact]
    public void Deve_Falhar_AlunoNulo()
    {
        var result = Matricula.Criar(1, null, MatriculaPlano.Mensal, DateOnly.FromDateTime(DateTime.Today), "Objetivo", MatriculaRestricoes.None, null);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "ALUNO_INVALIDO");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Deve_Validar_DataInicio(bool useDefault)
    {
        var inicio = useDefault ? default : DateOnly.FromDateTime(DateTime.Today);
        var result = Matricula.Criar(1, Helpers.GetValidAluno(), MatriculaPlano.Mensal, inicio, "Objetivo", MatriculaRestricoes.None, null);
        Assert.Equal(!useDefault, result.IsSuccess);
    }

    [Theory]
    [InlineData(MatriculaPlano.Mensal, 1)]
    [InlineData(MatriculaPlano.Trimestral, 3)]
    [InlineData(MatriculaPlano.Semestral, 6)]
    [InlineData(MatriculaPlano.Anual, 12)]
    public void Deve_Calcular_DataFim(MatriculaPlano plano, int meses)
    {
        var inicio = DateOnly.FromDateTime(DateTime.Today);
        var result = Matricula.Criar(1, Helpers.GetValidAluno(), plano, inicio, "Objetivo", MatriculaRestricoes.None, null);
        Assert.True(result.IsSuccess);
        Assert.Equal(inicio.AddMonths(meses), result.Value!.DataFim);
        Assert.Equal(1, result.Value.AlunoId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Deve_Falhar_ObjetivoObrigatorio(string? objetivo)
    {
        var result = Matricula.Criar(1, Helpers.GetValidAluno(), MatriculaPlano.Mensal, DateOnly.FromDateTime(DateTime.Today), objetivo, MatriculaRestricoes.None, null);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "OBJETIVO_OBRIGATORIO");
    }

    [Theory]
    [InlineData(MatriculaRestricoes.None, false)]
    [InlineData(MatriculaRestricoes.Diabetes, true)]
    [InlineData(MatriculaRestricoes.Diabetes | MatriculaRestricoes.Alergias, true)]
    public void Deve_Validar_Restricoes(MatriculaRestricoes restricoes, bool usarLaudo)
    {
        var result = Matricula.Criar(1, Helpers.GetValidAluno(), MatriculaPlano.Mensal, DateOnly.FromDateTime(DateTime.Today), "Objetivo", restricoes, usarLaudo ? Helpers.GetValidArquivo() : null, usarLaudo ? "Observacoes" : "");
        Assert.Equal(restricoes == MatriculaRestricoes.None || usarLaudo, result.IsSuccess);
    }

    [Theory]
    [InlineData(15, true)]
    [InlineData(16, false)]
    [InlineData(20, false)]
    public void Deve_Validar_LaudoParaMenor(int idade, bool expectFailure)
    {
        var result = Matricula.Criar(1, Helpers.GetValidAluno(DateOnly.FromDateTime(DateTime.Today.AddYears(-idade))), MatriculaPlano.Mensal, DateOnly.FromDateTime(DateTime.Today), "Objetivo", MatriculaRestricoes.None, null);
        Assert.Equal(expectFailure, result.IsFailure);
    }

    [Theory]
    [InlineData(" observa testo ", "observa testo")]
    [InlineData(" obs outro ", "obs outro")]
    [InlineData("obs", "obs")]
    public void Deve_Normalizar_Observacoes(string input, string expected)
    {
        var result = Matricula.Criar(1, Helpers.GetValidAluno(), MatriculaPlano.Mensal, DateOnly.FromDateTime(DateTime.Today), "Objetivo", MatriculaRestricoes.Diabetes, Helpers.GetValidArquivo(), input);
        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value!.ObservacoesRestricoes);
    }

    [Theory]
    [InlineData(MatriculaRestricoes.Diabetes)]
    [InlineData(MatriculaRestricoes.PressaoAlta)]
    [InlineData(MatriculaRestricoes.Labirintite)]
    [InlineData(MatriculaRestricoes.Alergias)]
    [InlineData(MatriculaRestricoes.ProblemasRespiratorios)]
    [InlineData(MatriculaRestricoes.RemedioContinuo)]
    public void Deve_Exigir_Laudo_Para_Restricao(MatriculaRestricoes restricao)
    {
        var result = Matricula.Criar(1, Helpers.GetValidAluno(), MatriculaPlano.Mensal, DateOnly.FromDateTime(DateTime.Today), "Objetivo", restricao, null);
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "RESTRICOES_LAUDO_OBRIGATORIO");
    }
}
