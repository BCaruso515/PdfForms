
namespace PdfForms.Views;
[QueryProperty(nameof(FileName), nameof(FileName))]
public partial class ViewPage : ContentPage
{
    public string? FileName { get; set; }

    public ViewPage()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        WebView1.Source = FileName;
    }
}