using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DotElectric.TemplateEditor.Converters;

public sealed class HexToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string hex || string.IsNullOrWhiteSpace(hex))
            return new SolidColorBrush(Colors.Black);

        if (hex.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
            return new SolidColorBrush(Colors.Transparent);

        try
        {
            return new BrushConverter().ConvertFromString(hex) ?? new SolidColorBrush(Colors.Black);
        }
        catch
        {
            return new SolidColorBrush(Colors.Black);
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This converter is one-way only.");
    }
}
