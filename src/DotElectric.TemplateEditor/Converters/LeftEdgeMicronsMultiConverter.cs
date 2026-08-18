using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// Тонкий адаптер XAML к anchor-политике RenderRules: X левого края слота объекта.
/// Принимает: startMicronsX (Line), endMicronsX (Line), micronsX (Rectangle/Text), zoom.
/// Возвращает пиксели (мм * zoom); по X нет flip — только масштаб.
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
            leftMicrons = RenderRules.LineLeftMicrons(startX, endX);
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
