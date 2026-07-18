using System.Globalization;
using System.Windows.Data;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// Конвертер LineType → StrokeDashArray для WPF.
/// </summary>
public sealed class LineTypeToDashArrayConverter : IValueConverter
{
    private static readonly System.Windows.Media.DoubleCollection _dashed = new() { 10, 5 };
    private static readonly System.Windows.Media.DoubleCollection _dashDot = new() { 10, 5, 2, 5 };
    private static readonly System.Windows.Media.DoubleCollection _dashDotDot = new() { 10, 5, 2, 5, 2, 5 };

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            LineType.Solid => null,
            LineType.Dashed => _dashed,
            LineType.DashDot => _dashDot,
            LineType.DashDotDot => _dashDotDot,
            _ => null
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("This converter is one-way only.");
}
