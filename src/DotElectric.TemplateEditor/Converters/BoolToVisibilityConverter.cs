using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// Bool → Visibility конвертер.
/// True = Visible, False = Collapsed (или наоборот через Invert).
/// </summary>
public sealed class BoolToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var boolValue = value is bool b && b;
        if (Invert) boolValue = !boolValue;
        return boolValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var visible = value is Visibility v && v == Visibility.Visible;
        return Invert ? !visible : visible;
    }
}
