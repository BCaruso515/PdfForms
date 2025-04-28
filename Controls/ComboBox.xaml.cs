using System.Collections;
using System.Windows.Input;
using CommunityToolkit.Maui.Views;


namespace PdfForms.Controls;

public partial class ComboBox : ContentView
{
	public ComboBox()
	{
		InitializeComponent();
	}
    public required DataTemplate ItemTemplate { get; set; }

    public required string DisplayMember { get; set; }

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        propertyName: nameof(SelectedItem),
        returnType: typeof(object),
        declaringType: typeof(ComboBox),
        propertyChanged: SelectedItemPropertyChanged,
        defaultBindingMode: BindingMode.TwoWay);

    public object SelectedItem
    {
        get => (object)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    private static void SelectedItemPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var controls = (ComboBox)bindable;

        if (newValue != null)
        {
            var propertyInfo = newValue.GetType().GetProperty(controls.DisplayMember);
            if (propertyInfo != null)
            {
                var value = propertyInfo.GetValue(newValue, null);
                if (value != null)
                {
                    controls.displayLabel.Text = value.ToString();
                }
            }
        }
    }

    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        propertyName: nameof(Placeholder),
        returnType: typeof(string),
        declaringType: typeof(ComboBox),
        propertyChanged: PlaceholderPropertyChanged,
        defaultBindingMode: BindingMode.OneWay);

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    private static void PlaceholderPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var controls = (ComboBox)bindable;

        if (controls.SelectedItem == null)
        {
            if (newValue != null)
            {
                controls.displayLabel.Text = newValue.ToString();
            }
        }
    }

    public static readonly BindableProperty ItemSourceProperty = BindableProperty.Create(
        propertyName: nameof(ItemSource),
        returnType: typeof(IEnumerable),
        declaringType: typeof(ComboBox),
        defaultBindingMode: BindingMode.OneWay);

    public IEnumerable ItemSource
    {
        get => (IEnumerable)GetValue(ItemSourceProperty);
        set => SetValue(ItemSourceProperty, value);
    }

    public event EventHandler<EventArgs>? OpenComboBoxEvent;

    public static readonly BindableProperty OpenComboBoxCommandProperty = BindableProperty.Create(
       propertyName: nameof(OpenComboBoxCommand),
       returnType: typeof(ICommand),
       declaringType: typeof(ComboBox),
       defaultBindingMode: BindingMode.TwoWay);

    public ICommand OpenComboBoxCommand
    {
        get => (ICommand)GetValue(OpenComboBoxCommandProperty);
        set => SetValue(OpenComboBoxCommandProperty, value);
    }

    public static readonly BindableProperty DisplayComboBoxControlProperty = BindableProperty.Create(
      propertyName: nameof(DisplayComboBoxControl),
      returnType: typeof(bool),
      declaringType: typeof(ComboBox),
      propertyChanged: DisplayComboBoxControlPropertyChanged,
      defaultBindingMode: BindingMode.TwoWay);

    public bool DisplayComboBoxControl
    {
        get => (bool)GetValue(DisplayComboBoxControlProperty);
        set => SetValue(DisplayComboBoxControlProperty, value);
    }

    private async static void DisplayComboBoxControlPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var controls = (ComboBox)bindable;

        if (newValue != null)
        {
            if ((bool)newValue)
            {
                var popup = new ComboBoxPopup(controls.ItemSource, controls.ItemTemplate);
                var result = await Shell.Current.ShowPopupAsync(popup);

                if (result != null)
                {
                    controls.SelectedItem = result;
                }
                controls.DisplayComboBoxControl = false;
            }
        }
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        OpenComboBoxCommand?.Execute(null);
        OpenComboBoxEvent?.Invoke(sender, e);
        DisplayComboBoxControl = true;
    }
}
