using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DotElectric.TemplateEditor.Helpers;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// Тонкий адаптер XAML к карте выравнивания RenderRules (строка → TextAlignment).
/// </summary>
public sealed class StringToTextAlignmentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => RenderRules.TextAlignmentFor(value as string);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            TextAlignment t when t == TextAlignment.Center => "Center",
            TextAlignment t when t == TextAlignment.Right => "Right",
            _ => "Left"
        };
    }
}
