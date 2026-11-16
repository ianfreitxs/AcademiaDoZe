// Ian Freitas de Sousa
using AcademiaDoZe.Application.DTOs;
using AcademiaDoZe.Presentation.AppMaui.ViewModels;

namespace AcademiaDoZe.Presentation.AppMaui.Views;

public partial class MatriculaListPage : ContentPage
{
    public MatriculaListPage(MatriculaListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MatriculaListViewModel viewModel)
            await viewModel.LoadMatriculasCommand.ExecuteAsync(null);
    }

    private async void OnMoreClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button
            || button.BindingContext is not MatriculaDto matricula
            || BindingContext is not MatriculaListViewModel viewModel)
            return;

        var escolha = await DisplayActionSheetAsync("Ações", "Cancelar", null, "Editar", "Excluir");

        if (escolha == "Editar")
            await viewModel.EditMatriculaCommand.ExecuteAsync(matricula);
        else if (escolha == "Excluir")
            await viewModel.DeleteMatriculaCommand.ExecuteAsync(matricula);
    }
}
