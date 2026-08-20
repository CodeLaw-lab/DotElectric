namespace DotElectric.TemplateEditor.Helpers;

/// <summary>
/// Проверки полей панели свойств редактора (координата, размер, содержимое текста,
/// размер шрифта, цвет). Проверка формата цвета делегируется документной библиотеке
/// (<see cref="HexColorValidation"/>) — копий кода нет.
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
        if (value < PhysicalConstants.MinDimensionMicrons)
            return $"Минимальный размер — 0.4 мм ({PhysicalConstants.MinDimensionMicrons} микрон).";
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
        if (fontSizeMicrons < PhysicalConstants.MinFontSizeMicrons)
            return $"Минимальный размер шрифта — 1 мм ({PhysicalConstants.MinFontSizeMicrons} микрон).";
        return null;
    }

    /// <summary>
    /// Проверка формата цвета — делегирование документной библиотеке (копий кода нет).
    /// </summary>
    public static string? ValidateHexColor(string? value) => HexColorValidation.Validate(value);

    /// <summary>
    /// Запасной поставщик проверки цвета (документная библиотека).
    /// </summary>
    public static readonly IValidationService Default = HexColorValidation.Default;
}
