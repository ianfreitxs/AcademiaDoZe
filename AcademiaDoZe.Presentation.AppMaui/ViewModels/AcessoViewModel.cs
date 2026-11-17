// Ian Freitas de Sousa
using AcademiaDoZe.Application.Interfaces;
using CommunityToolkit.Mvvm.Input;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class AcessoViewModel : BaseViewModel
{
    private readonly IAcessoAlunoService _acessoAlunoService;
    private readonly IAcessoColaboradorService _acessoColaboradorService;
    private string _cpfAluno = string.Empty;
    private string _cpfColaborador = string.Empty;
    private string _mensagemAluno = string.Empty;
    private string _mensagemColaborador = string.Empty;

    public string CpfAluno
    {
        get => _cpfAluno;
        set => SetProperty(ref _cpfAluno, value);
    }

    public string CpfColaborador
    {
        get => _cpfColaborador;
        set => SetProperty(ref _cpfColaborador, value);
    }

    public string MensagemAluno
    {
        get => _mensagemAluno;
        set => SetProperty(ref _mensagemAluno, value);
    }

    public string MensagemColaborador
    {
        get => _mensagemColaborador;
        set => SetProperty(ref _mensagemColaborador, value);
    }

    public AcessoViewModel(IAcessoAlunoService acessoAlunoService, IAcessoColaboradorService acessoColaboradorService)
    {
        _acessoAlunoService = acessoAlunoService ?? throw new ArgumentNullException(nameof(acessoAlunoService));
        _acessoColaboradorService = acessoColaboradorService ?? throw new ArgumentNullException(nameof(acessoColaboradorService));
        Title = "Entrada e Saída";
    }

    [RelayCommand]
    private async Task RegistrarAcessoAlunoAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(CpfAluno))
        {
            await Shell.Current.DisplayAlertAsync("Atenção", "Informe o CPF do aluno.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            var resultado = await _acessoAlunoService.RegistrarEntradaOuSaidaAsync(CpfAluno);
            MensagemAluno = resultado.Mensagem;
            await Shell.Current.DisplayAlertAsync(resultado.Titulo, resultado.Mensagem, "OK");
        }
        catch (Exception ex)
        {
            MensagemAluno = ex.Message;
            await Shell.Current.DisplayAlertAsync("Não foi possível registrar", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RegistrarAcessoColaboradorAsync()
    {
        if (IsBusy)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(CpfColaborador))
        {
            await Shell.Current.DisplayAlertAsync("Atenção", "Informe o CPF do colaborador.", "OK");
            return;
        }

        try
        {
            IsBusy = true;
            var resultado = await _acessoColaboradorService.RegistrarEntradaOuSaidaAsync(CpfColaborador);
            MensagemColaborador = resultado.Mensagem;
            await Shell.Current.DisplayAlertAsync(resultado.Titulo, resultado.Mensagem, "OK");
        }
        catch (Exception ex)
        {
            MensagemColaborador = ex.Message;
            await Shell.Current.DisplayAlertAsync("Não foi possível registrar", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
