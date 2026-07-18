using System.Globalization;
using System.Windows.Data;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Converters;

/// <summary>
/// TextType → строка (для ComboBox).
/// </summary>
public sealed class TextTypeToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            TextType.Text => "Текст",
            TextType.Dimension => "Размер",
            TextType.Tolerance => "Допуск",
            TextType.Note => "Примечание",
            TextType.Label => "Обозначение",
            _ => value?.ToString() ?? ""
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            "Текст" => TextType.Text,
            "Размер" => TextType.Dimension,
            "Допуск" => TextType.Tolerance,
            "Примечание" => TextType.Note,
            "Обозначение" => TextType.Label,
            _ => TextType.Text
        };
    }
}
