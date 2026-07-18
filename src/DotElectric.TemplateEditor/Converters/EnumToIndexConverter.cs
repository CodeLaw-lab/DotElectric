using System.Globalization;
using System.Windows.Data;

namespace DotElectric.TemplateEditor.Converters;

public class EnumToIndexConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is null) return -1;
        var enumType = value.GetType();
        if (!enumType.IsEnum) return -1;

        var values = Enum.GetValues(enumType);
        for (var i = 0; i < values.Length; i++)
        {
            if (Equals(values.GetValue(i), value))
                return i;
        }
        return -1;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
