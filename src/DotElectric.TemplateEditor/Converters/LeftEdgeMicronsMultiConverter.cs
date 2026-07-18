using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// Вычисляет X левого края из конкретных свойств (для MultiBinding).
/// Принимает: startMicronsX (Line), endMicronsX (Line), micronsX (Rectangle/Text), zoom.
/// Возвращает пиксели (мм * zoom).
/// </summary>
public sealed class LeftEdgeMicronsMultiConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 4) return 0.0;

        var isLine = values[0] != DependencyProperty.UnsetValue && values[0] is long;
        var zoom = values[3] as double? ?? 1.0;

        long leftMicrons;
        if (isLine)
        {
            var startX = (long)values[0];
            var endX = values[1] as long? ?? startX;
            leftMicrons = Math.Min(startX, endX);
        }
        else
        {
            leftMicrons = values[2] as long? ?? 0L;
        }

        return Coordinate.ToMm(leftMicrons) * zoom;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This multi-value converter is one-way only.");
    }
}
