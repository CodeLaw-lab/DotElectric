using System.Globalization;
using System.Windows.Data;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// Конвертер координаты X из микрон в WPF-пиксели для Canvas.Left.
/// Формула: x_wpf = ToMm(micronsX) * zoom
/// </summary>
public sealed class ModelXToCanvasLeftConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 2) return 0.0;
        if (values[0] is not long micronsX) return 0.0;
        if (values[1] is not double zoom || zoom <= 0) return 0.0;

        return Coordinate.ToMm(micronsX) * zoom;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("This multi-value converter is one-way only.");
}
