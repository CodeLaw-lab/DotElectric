using System.Globalization;
using System.Windows.Data;
using DotElectric.TemplateEditor.Helpers;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// Тонкий адаптер XAML к карте шрифтов RenderRules.
/// </summary>
public sealed class FontNameToFamilyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => RenderRules.FontFamilyFor(value as string);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("This converter is one-way only.");
}
