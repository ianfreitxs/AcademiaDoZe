// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Entities;

namespace AcademiaDoZe.Domain.Tests.Entities;

public class AcessoColaboradorTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Deve_Validar_Colaborador(bool colaboradorNull)
    {
        var colaborador = colaboradorNull ? null : Helpers.GetValidColaborador();
        var result = AcessoColaborador.Criar(1, colaborador, DateTime.Today.AddHours(10));

        if (colaboradorNull)
        {
            Assert.True(result.IsFailure);
            Assert.Contains(result.Notifications, n => n.Mensagem == "COLABORADOR_INVALIDO");
        }
        else
        {
            Assert.True(result.IsSuccess);
            Assert.Equal(colaborador!.Id, result.Value!.ColaboradorId);
        }
    }

    [Theory]
    [InlineData(5, 59)]
    [InlineData(22, 1)]
    [InlineData(0, 0)]
    [InlineData(23, 0)]
    public void Deve_Falhar_HorarioForaDoIntervalo(int hour, int minute)
    {
        var result = AcessoColaborador.Criar(1, Helpers.GetValidColaborador(), DateTime.Today.AddHours(hour).AddMinutes(minute));
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
        var colaborador = Helpers.GetValidColaborador();
        var dataHora = DateTime.Today.AddHours(hour);
        var result = AcessoColaborador.Criar(1, colaborador, dataHora);
        Assert.True(result.IsSuccess);
        Assert.Equal(colaborador.Id, result.Value!.ColaboradorId);
        Assert.Equal(dataHora, result.Value.DataHora);
    }
}
