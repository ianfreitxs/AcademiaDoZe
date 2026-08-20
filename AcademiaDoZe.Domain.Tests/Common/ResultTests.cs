// Ian Freitas de Sousa
using AcademiaDoZe.Domain.Common;

namespace AcademiaDoZe.Domain.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Deve_Criar_Result_Success()
    {
        var result = Result<int>.Success(10);
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(10, result.Value);
        Assert.Empty(result.Notifications);
    }

    [Fact]
    public void Deve_Criar_Result_Failure_ComLista()
    {
        var result = Result<string>.Failure([new Notification("Campo", "Erro")]);
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Null(result.Value);
        Assert.Single(result.Notifications);
    }

    [Fact]
    public void Deve_Criar_Result_Failure_ComDados()
    {
        var result = Result<int>.Failure("Campo", "Erro");
        Assert.True(result.IsFailure);
        Assert.Equal("Campo", result.Notifications.First().Propriedade);
    }

    [Fact]
    public void Deve_Criar_Result_Failure_ComNotificacao()
    {
        var result = Result<int>.Failure(new Notification("Campo", "Erro"));
        Assert.True(result.IsFailure);
        Assert.Equal("Erro", result.Notifications.First().Mensagem);
    }

    [Fact]
    public void Deve_Criar_Notificacao_ComValores()
    {
        var notification = new Notification("Campo", "Mensagem");
        Assert.Equal("Campo", notification.Propriedade);
        Assert.Equal("Mensagem", notification.Mensagem);
    }
}
