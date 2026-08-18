using System.Globalization;
using System.Windows.Data;
using DotElectric.TemplateEditor.Helpers;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// Тонкий адаптер XAML к парсеру hex-цветов RenderRules.
/// </summary>
public sealed class HexToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => RenderRules.BrushFromHex(value as string);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This converter is one-way only.");
    }
}
