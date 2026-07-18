using System.Globalization;
using System.Windows.Data;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// Зум (double) → строка в процентах.
/// </summary>
public sealed class ZoomToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            double zoom => $"{(int)Math.Round(zoom * 100)}%",
            null => "100%",
            _ => value.ToString() ?? ""
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s && s.EndsWith("%"))
        {
            if (double.TryParse(s.TrimEnd('%'), out var percent))
                return percent / 100.0;
        }
        return 1.0;
    }
}
