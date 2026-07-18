using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace DotElectric.TemplateEditor.Converters;

public sealed class IsObjectSelectedConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length < 2) return false;
        if (values[0] is not object obj) return false;
        if (values[1] is not IList list) return false;
        return list.Contains(obj);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException("This multi-value converter is one-way only.");
}
