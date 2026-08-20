// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class AcessoAlunoTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Deve_Validar_Aluno(bool alunoNull)
    {
        var aluno = alunoNull ? null : Helpers.GetValidAluno();
        var result = AcessoAluno.Criar(1, aluno, DateTime.Today.AddHours(10));

        if (alunoNull)
        {
            Assert.True(result.IsFailure);
            Assert.Contains(result.Notifications, n => n.Mensagem == "ALUNO_INVALIDO");
        }
        else
        {
            Assert.True(result.IsSuccess);
            Assert.Equal(aluno!.Id, result.Value!.AlunoId);
        }
    }

    [Theory]
    [InlineData(5, 59)]
    [InlineData(22, 1)]
    [InlineData(0, 0)]
    [InlineData(23, 0)]
    public void Deve_Falhar_HorarioForaDoIntervalo(int hour, int minute)
    {
        var result = AcessoAluno.Criar(1, Helpers.GetValidAluno(), DateTime.Today.AddHours(hour).AddMinutes(minute));
        Assert.True(result.IsFailure);
        Assert.Contains(result.Notifications, n => n.Mensagem == "DATA_HORA_INTERVALO_INVALIDO");
    }

    [Theory]
    [InlineData(6)]
    [InlineData(10)]
    [InlineData(14)]
    [InlineData(22)]
    [InlineData(8)]
    public void Deve_Permitir_HorarioValido(int hour)
    {
        var aluno = Helpers.GetValidAluno();
        var dataHora = DateTime.Today.AddHours(hour);
        var result = AcessoAluno.Criar(1, aluno, dataHora);
        Assert.True(result.IsSuccess);
        Assert.Equal(aluno.Id, result.Value!.AlunoId);
        Assert.Equal(dataHora, result.Value.DataHora);
    }
}
