using CommunityToolkit.Maui.Views;
using System.Collections;

namespace PdfForms.Controls;

public partial class ComboBoxPopup : Popup
{
    public ComboBoxPopup(IEnumerable itemSource, DataTemplate itemTemplate)
    {
        InitializeComponent();

        cvResults.ItemsSource = itemSource;
        cvResults.ItemTemplate = itemTemplate;
    }

    private async void ComboBoxPopup_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var currentItem = e.CurrentSelection.FirstOrDefault();
        await CloseAsync(currentItem);
    }
}