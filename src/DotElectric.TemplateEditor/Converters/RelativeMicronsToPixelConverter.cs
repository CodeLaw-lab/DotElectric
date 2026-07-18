using System.Globalization;
using System.Windows.Data;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// Конвертер разницы координат в микронах → пиксели.
/// Формула: |valueMicrons - baseMicrons| * toMm * zoom
/// </summary>
public sealed class RelativeMicronsToPixelConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 3) return 0.0;
        if (values[0] is not long valueMicrons) return 0.0;
        if (values[1] is not long baseMicrons) return 0.0;
        if (values[2] is not double zoom || zoom <= 0) return 0.0;

        var diffMicrons = Math.Abs(valueMicrons - baseMicrons);
        return Coordinate.ToMm(diffMicrons) * zoom;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("This multi-value converter is one-way only.");
}
