using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DotElectric.TemplateEditor.Converters;

public sealed class StringToTextAlignmentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            "Center" => TextAlignment.Center,
            "Right" => TextAlignment.Right,
            _ => TextAlignment.Left
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            TextAlignment t when t == TextAlignment.Center => "Center",
            TextAlignment t when t == TextAlignment.Right => "Right",
            _ => "Left"
        };
    }
}
