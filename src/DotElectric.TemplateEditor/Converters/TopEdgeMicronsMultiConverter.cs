using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// Вычисляет Y верхнего края из конкретных свойств (для MultiBinding).
/// Line: startY, endY → max(startY, endY).
/// Rectangle: micronsY, heightMicrons → micronsY + heightMicrons (BottomMicronsY).
/// Text: micronsY, heightMicrons → micronsY + heightMicrons (BottomMicronsY).
/// Принимает 8 параметров: lineStartY, lineEndY, rectY, rectH, textY, textHeight, sheetHeightMm, zoom.
/// Возвращает пиксели: (sheetHeightMm - ToMm(topMicrons)) * zoom.
/// </summary>
public sealed class TopEdgeMicronsMultiConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 8) return 0.0;

        var isLine = values[0] != DependencyProperty.UnsetValue && values[0] is long;
        var isText = values[5] != DependencyProperty.UnsetValue && values[5] is long;
        var sheetHeightMm = values[6] as double? ?? 0.0;
        var zoom = values[7] as double? ?? 1.0;

        long topMicrons;
        if (isLine)
        {
            var startY = (long)values[0];
            var endY = values[1] as long? ?? startY;
            topMicrons = Math.Max(startY, endY);
        }
        else if (isText)
        {
            var micronsY = (long)values[4];
            var textHeight = values[5] as long? ?? 0L;
            topMicrons = micronsY + textHeight;
        }
        else
        {
            var rectY = values[2] as long? ?? 0L;
            var rectH = values[3] as long? ?? 0L;
            topMicrons = rectY + rectH;
        }

        return (sheetHeightMm - Coordinate.ToMm(topMicrons)) * zoom;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This multi-value converter is one-way only.");
    }
}
