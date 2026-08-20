using DotElectric.TemplateEditor.Constants;

namespace DotElectric.TemplateEditor.Helpers;

/// <summary>
/// Проверки полей панели свойств редактора (координата, размер, содержимое текста,
/// размер шрифта). Проверка формата цвета — в документной библиотеке
/// (<see cref="HexColorValidation"/>), потребители вызывают её напрямую.
/// </summary>
public static class ValidationService
{
    public static string? ValidateCoordinate(long value)
    {
        if (value < 0)
            return $"Координата не может быть отрицательной (текущая: {Coordinate.FormatMm(value)}).";
        return null;
    }

    public static string? ValidateDimension(long value)
    {
        if (value <= 0)
            return $"Размер должен быть положительным (текущий: {Coordinate.FormatMm(value)}).";
        if (value < EditorSettings.MinDimensionMicrons)
            return $"Минимальный размер — 0.4 мм ({EditorSettings.MinDimensionMicrons} микрон).";
        return null;
    }

    public static string? ValidateTextContent(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return "Содержимое текста не может быть пустым.";
        return null;
    }

    public static string? ValidateFontSize(long fontSizeMicrons)
    {
        if (fontSizeMicrons <= 0)
            return $"Размер шрифта должен быть положительным (текущий: {Coordinate.FormatMm(fontSizeMicrons)}).";
        if (fontSizeMicrons < EditorSettings.MinFontSizeMicrons)
            return $"Минимальный размер шрифта — 1 мм ({EditorSettings.MinFontSizeMicrons} микрон).";
        return null;
    }
}
