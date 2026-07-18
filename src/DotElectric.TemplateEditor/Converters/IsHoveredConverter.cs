using System.Globalization;
using System.Windows.Data;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// Конвертер для проверки: является ли текущий объект hovered.
/// Принимает [object, hoveredObject]. Возвращает true если object == hoveredObject.
/// </summary>
public sealed class IsHoveredConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 2) return false;
        var obj = values[0];
        var hovered = values[1];
        return ReferenceEquals(obj, hovered);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("This multi-value converter is one-way only.");
}
