using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DotElectric.TemplateEditor.Converters;

public sealed class FontNameToFamilyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string fontName)
        {
            return fontName switch
            {
                "ГОСТ А" => new FontFamily("pack://application:,,,/Resources/Fonts/#GOST Type AU"),
                "ГОСТ Б" => new FontFamily("pack://application:,,,/Resources/Fonts/#GOST Type BU"),
                _ => new FontFamily("Segoe UI")
            };
        }
        return new FontFamily("Segoe UI");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("This converter is one-way only.");
}
