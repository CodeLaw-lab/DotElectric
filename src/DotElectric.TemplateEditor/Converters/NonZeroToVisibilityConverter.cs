using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// Numeric value → Visibility. >0 = Visible, 0 = Collapsed.
/// </summary>
public sealed class NonZeroToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var count = value switch
        {
            int i => i,
            long l => (int)l,
            double d => (int)d,
            float f => (int)f,
            _ => 0
        };
        return count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("This converter is one-way only.");
    }
}
