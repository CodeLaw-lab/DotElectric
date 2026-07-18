using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DotElectric.TemplateEditor.Converters;

public sealed class BoolToTextWrappingConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is true ? TextWrapping.Wrap : TextWrapping.NoWrap;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is TextWrapping wrap && wrap == TextWrapping.Wrap;
    }
}
