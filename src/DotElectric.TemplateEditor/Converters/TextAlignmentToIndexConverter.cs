using System.Globalization;
using System.Windows.Data;

namespace DotElectric.TemplateEditor.Converters;

public sealed class TextAlignmentToIndexConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            "Center" => 1,
            "Right" => 2,
            _ => 0
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            1 => "Center",
            2 => "Right",
            _ => "Left"
        };
    }
}
