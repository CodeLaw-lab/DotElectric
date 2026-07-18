using System.Globalization;
using System.Windows.Data;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// LineType → строка (для ComboBox).
/// </summary>
public sealed class LineTypeToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            LineType.Solid => "Сплошная",
            LineType.Dashed => "Штриховая",
            LineType.DashDot => "Штрихпунктирная",
            LineType.DashDotDot => "Штрихпунктирная с двумя штрихами",
            _ => value?.ToString() ?? ""
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            "Сплошная" => LineType.Solid,
            "Штриховая" => LineType.Dashed,
            "Штрихпунктирная" => LineType.DashDot,
            "Штрихпунктирная с двумя штрихами" => LineType.DashDotDot,
            _ => LineType.Solid
        };
    }
}
