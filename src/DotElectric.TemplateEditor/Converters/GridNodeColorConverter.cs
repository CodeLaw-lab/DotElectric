using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// HEX-строка цвета узлов сетки → Brush. null/пусто/невалидно → null (темо-зависимый цвет по умолчанию).
/// </summary>
public sealed class GridNodeColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string hex || string.IsNullOrWhiteSpace(hex))
            return null;
        try
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            brush.Freeze();
            return brush;
        }
        catch
        {
            return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
