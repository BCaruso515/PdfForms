using System.Collections;

namespace PdfForms.Controls;

public partial class BindingNavigator : ContentView
{
    private int _count;

    public BindingNavigator()
	{
		InitializeComponent();
    }

    public static readonly BindableProperty ItemSourceProperty = BindableProperty.Create(
        propertyName: nameof(ItemSource),
        returnType: typeof(IEnumerable),
        declaringType: typeof(BindingNavigator),
        propertyChanged: ItemSourcePropertyChanged,
        defaultBindingMode: BindingMode.OneWay);

    public required IEnumerable ItemSource
    {
        get => (IEnumerable)GetValue(ItemSourceProperty);
        set => SetValue(ItemSourceProperty, value);
    }

    private static void ItemSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var controls = (BindingNavigator)bindable;
        controls._count = ((IEnumerable<object>)controls.ItemSource).Count();
    }

    public static readonly BindableProperty IndexProperty = BindableProperty.Create(
        propertyName: nameof(Index),
        returnType: typeof(int),
        declaringType: typeof(BindingNavigator),
        defaultBindingMode: BindingMode.TwoWay);

    public int Index
    {
        get => (int)GetValue(IndexProperty);
        set => SetValue(IndexProperty, value);
    }
    
    private void MoveFirst_Clicked(object sender, EventArgs e)
    {
        if (_count > 0) Index = 0;
    }

    private void MovePrevious_Clicked(object sender, EventArgs e)
    {
        if (Index > -1) Index--;
    }

    private void MoveNext_Clicked(object sender, EventArgs e)
    {
        if (Index < _count -1) Index++;
    }

    private void MoveLast_Clicked(object sender, EventArgs e)
    {
        Index = _count - 1; 
    }
}