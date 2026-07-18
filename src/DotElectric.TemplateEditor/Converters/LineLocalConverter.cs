using System.Globalization;
using System.Windows.Data;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Converters;

public sealed class LineLocalConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 3) return 0.0;
        if (values[0] is not long coord1) return 0.0;
        if (values[1] is not long coord2) return 0.0;
        if (values[^1] is not double zoom || zoom <= 0) return 0.0;

        var param = parameter as string ?? "";

        long localCoord;
        if (param is "X1" or "X2")
        {
            var minX = Math.Min(coord1, coord2);
            localCoord = param == "X1" ? coord1 - minX : coord2 - minX;
        }
        else if (param is "Y1" or "Y2")
        {
            var maxY = Math.Max(coord1, coord2);
            localCoord = param == "Y1" ? maxY - coord1 : maxY - coord2;
        }
        else return 0.0;

        return Coordinate.ToMm(localCoord) * zoom;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("This multi-value converter is one-way only.");
}
