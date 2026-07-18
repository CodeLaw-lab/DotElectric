using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// Конвертер шага сетки: double ↔ "X мм".
/// </summary>
public sealed partial class GridStepToStringConverter : IValueConverter
{
    [GeneratedRegex(@"[^\d,\.]+$")]
    private static partial Regex NonNumericSuffix();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d) return $"{d:0} мм";
        if (value is int i) return $"{i} мм";
        return value?.ToString() ?? "5 мм";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var text = value?.ToString()?.Trim();
        if (string.IsNullOrEmpty(text))
            return 5.0;

        text = NonNumericSuffix().Replace(text, "");
        text = text.Replace(",", ".");

        return double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : Binding.DoNothing;
    }
}
