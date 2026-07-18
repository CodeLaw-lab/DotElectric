using System.Globalization;
using System.Windows.Data;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// Микроны → строка в мм (3 знака после запятой).
/// </summary>
public sealed class MicronsToMmConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            long microns => Coordinate.ToMm(microns).ToString("F3", CultureInfo.InvariantCulture),
            double micronsD => Coordinate.ToMm((long)micronsD).ToString("F3", CultureInfo.InvariantCulture),
            null => "",
            _ => value.ToString() ?? ""
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string s)
            return Coordinate.ParseMm(s);
        return 0L;
    }
}
