using PdfForms.ViewModels;

namespace PdfForms.Views;

public partial class FormEditorPage : ContentPage
{
	private FormEditorViewModel _viewModel;

	public FormEditorPage(FormEditorViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
	}

    protected async override void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.Appearing();
    }
}