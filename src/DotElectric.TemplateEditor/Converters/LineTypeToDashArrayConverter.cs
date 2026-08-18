using System.Globalization;
using System.Windows.Data;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// Тонкий адаптер XAML к dash-карте RenderRules (LineType → StrokeDashArray).
/// </summary>
public sealed class LineTypeToDashArrayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is LineType lineType ? RenderRules.DashArrayFor(lineType) : null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("This converter is one-way only.");
}
