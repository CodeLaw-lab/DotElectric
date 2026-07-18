using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;

namespace DotElectric.TemplateEditor.Helpers;

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

    public static string? ValidateRotation(int rotation)
    {
        if (rotation < 0 || rotation > 359)
            return "Угол поворота должен быть в диапазоне 0-359°.";
        return null;
    }

    public static string? ValidateHexColor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Цвет не может быть пустым.";

        if (value.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
            return null;

        var hex = value.TrimStart('#');
        if (hex.Length is not (6 or 8))
            return "Цвет должен быть в формате #RRGGBB или #AARRGGBB.";

        return System.Text.RegularExpressions.Regex.IsMatch(hex, @"^[0-9A-Fa-f]{6,8}$")
            ? null
            : "Цвет должен содержать только HEX-символы (0-9, A-F).";
    }

    private sealed class ValidationServiceInstance : IValidationService
    {
        public string? ValidateHexColor(string? value) => ValidationService.ValidateHexColor(value);
    }

    public static readonly IValidationService Default = new ValidationServiceInstance();
}
