using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DotElectric.TemplateEditor.Helpers;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// Тонкий адаптер XAML к anchor-политике RenderRules: Y верхнего края слота объекта.
/// Разбор параметров MultiBinding остаётся здесь (линия/текст/прямоугольник различаются
/// позициями значений в массиве); формулы anchor'ов и Y-flip — в RenderRules.
/// Принимает 8 параметров: lineStartY, lineEndY, rectY, rectH, textY, textHeight, sheetHeightMm, zoom.
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
            topMicrons = RenderRules.LineTopMicrons(startY, endY);
        }
        else if (isText)
        {
            var micronsY = (long)values[4];
            var textHeight = values[5] as long? ?? 0L;
            topMicrons = RenderRules.BoxTopMicrons(micronsY, textHeight);
        }
        else
        {
            var rectY = values[2] as long? ?? 0L;
            var rectH = values[3] as long? ?? 0L;
            topMicrons = RenderRules.BoxTopMicrons(rectY, rectH);
        }

        return RenderRules.ModelYToTop(topMicrons, sheetHeightMm, zoom);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This multi-value converter is one-way only.");
    }
}
