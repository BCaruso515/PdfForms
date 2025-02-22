using PdfForms.ViewModels;

namespace PdfForms.Views;

public partial class FormEditorPage : ContentPage
{
	public FormEditorPage(FormEditorViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}