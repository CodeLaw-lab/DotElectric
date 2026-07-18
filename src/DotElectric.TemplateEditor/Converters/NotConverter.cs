using System.Globalization;
using System.Windows.Data;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// Инвертирует булево значение.
/// </summary>
public sealed class NotConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && !b;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && !b;
}
