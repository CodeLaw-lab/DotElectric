using System.Text.RegularExpressions;

namespace DotElectric.Document;

/// <summary>
/// Чистая проверка формата цвета (правило V-005) — запасной вариант шва валидации
/// в документной библиотеке. Приложение поручает ей свою проверку цвета, копий кода нет.
/// </summary>
public static class HexColorValidation
{
    /// <summary>
    /// Проверить формат цвета: пустое значение — ошибка, "Transparent" допустим,
    /// иначе #RRGGBB или #AARRGGBB. null = корректно, иначе текст ошибки.
    /// </summary>
    public static string? Validate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Цвет не может быть пустым.";

        if (value.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
            return null;

        var hex = value.TrimStart('#');
        if (hex.Length is not (6 or 8))
            return "Цвет должен быть в формате #RRGGBB или #AARRGGBB.";

        return Regex.IsMatch(hex, @"^[0-9A-Fa-f]{6,8}$")
            ? null
            : "Цвет должен содержать только HEX-символы (0-9, A-F).";
    }

    private sealed class DefaultValidationService : IValidationService
    {
        public string? ValidateHexColor(string? value) => Validate(value);
    }

    /// <summary>
    /// Запасной поставщик проверки цвета — позволяет создавать валидатор документа
    /// без параметров внутри библиотеки.
    /// </summary>
    public static IValidationService Default { get; } = new DefaultValidationService();
}
