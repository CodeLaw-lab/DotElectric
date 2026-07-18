using System.Globalization;
using System.Windows.Data;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// IsNull → Bool (для отображения/скрытия элементов).
/// </summary>
public sealed class IsNullConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value == null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This converter is one-way only.");
    }
}
