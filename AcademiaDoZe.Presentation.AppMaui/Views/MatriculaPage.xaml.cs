// Ian Freitas de Sousa
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class MatriculaPage : ContentPage
{
    public MatriculaPage(MatriculaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MatriculaViewModel viewModel)
        {
            await viewModel.InitializeAsync();
            CalculateFimFromPlanoAndInicio();
        }
    }

    private void PlanoPicker_SelectedIndexChanged(object? sender, EventArgs e)
    {
        CalculateFimFromPlanoAndInicio();
    }

    private void InicioDatePicker_DateSelected(object? sender, DateChangedEventArgs e)
    {
        CalculateFimFromPlanoAndInicio();
    }

    private void CalculateFimFromPlanoAndInicio()
    {
        if (BindingContext is not MatriculaViewModel viewModel)
            return;

        var plano = viewModel.Matricula.Plano;
        var inicio = viewModel.Matricula.DataInicio;

        DateOnly fim = plano switch
        {
            AppMatriculaPlano.Mensal => inicio.AddMonths(1),
            AppMatriculaPlano.Trimestral => inicio.AddMonths(3),
            AppMatriculaPlano.Semestral => inicio.AddMonths(6),
            AppMatriculaPlano.Anual => inicio.AddYears(1),
            _ => inicio
        };

        viewModel.Matricula.DataFim = fim;
        FimDatePicker.Date = fim.ToDateTime(TimeOnly.MinValue);
    }
}
