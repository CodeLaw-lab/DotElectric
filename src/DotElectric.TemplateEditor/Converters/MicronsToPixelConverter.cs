using System.Globalization;
using System.Windows.Data;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// Конвертер длины из микрон в WPF-пиксели.
/// Формула: length_wpf = ToMm(microns) * zoom
/// </summary>
public sealed class MicronsToPixelConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 2) return 0.0;
        if (values[0] is not long microns) return 0.0;
        if (values[1] is not double zoom || zoom <= 0) return 0.0;

        return Coordinate.ToMm(microns) * zoom;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("This multi-value converter is one-way only.");
}
