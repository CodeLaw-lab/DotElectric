using System.Globalization;
using System.Windows.Data;
using DotElectric.TemplateEditor.Helpers;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// Тонкий адаптер XAML к Y-flip RenderRules: координата Y из микрон в WPF-пиксели для Canvas.Top.
/// Guards MultiBinding (число/типы значений, zoom &gt; 0) остаются здесь.
/// </summary>
public sealed class ModelYToCanvasTopConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 3) return 0.0;
        if (values[0] is not long micronsY) return 0.0;
        if (values[1] is not double sheetHeightMm) return 0.0;
        if (values[2] is not double zoom || zoom <= 0) return 0.0;

        return RenderRules.ModelYToTop(micronsY, sheetHeightMm, zoom);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("This multi-value converter is one-way only.");
}
