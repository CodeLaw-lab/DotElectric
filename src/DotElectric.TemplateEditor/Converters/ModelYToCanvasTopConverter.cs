using System.Globalization;
using System.Windows.Data;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// Конвертер координаты Y из микрон в WPF-пиксели для Canvas.Top.
/// Формула: y_wpf = (sheetHeightMm - ToMm(micronsY)) * zoom
/// </summary>
public sealed class ModelYToCanvasTopConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 3) return 0.0;
        if (values[0] is not long micronsY) return 0.0;
        if (values[1] is not double sheetHeightMm) return 0.0;
        if (values[2] is not double zoom || zoom <= 0) return 0.0;

        return (sheetHeightMm - Coordinate.ToMm(micronsY)) * zoom;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("This multi-value converter is one-way only.");
}
