using System.Globalization;
using System.Windows.Data;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// String equality → Bool. Сравнивает значение с ConverterParameter.
/// Используется для ToggleButton + ActiveTool.
/// </summary>
public sealed class EqualToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var param = parameter?.ToString();
        var strValue = value?.ToString();
        return string.Equals(strValue, param, StringComparison.OrdinalIgnoreCase);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b && b)
            return parameter?.ToString() ?? string.Empty;
        return Binding.DoNothing;
    }
}
