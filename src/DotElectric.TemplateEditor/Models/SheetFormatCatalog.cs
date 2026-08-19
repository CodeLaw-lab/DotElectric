using System.Diagnostics.CodeAnalysis;

namespace DotElectric.TemplateEditor.Models;

/// <summary>
/// Каталог стандартных форматов листа — единственный источник фиксированного набора
/// форматов (базовые A0–A4 и полуформаты). Потребители: модель листа, валидатор,
/// настройки приложения, меню «Новый шаблон», диалог произвольного размера.
/// Пользовательский формат (произвольные размеры) в каталог не входит.
/// </summary>
public static class SheetFormatCatalog
{
    /// <summary>
    /// Каноническое имя формата по умолчанию.
    /// Константа — для дефолтных параметров сигнатур и POCO-настроек.
    /// </summary>
    public const string DefaultName = "A3";

    private static readonly SheetFormat[] Formats =
    [
        new("A0", 1_189_000, 841_000, SheetOrientation.Landscape),
        new("A1", 841_000, 594_000, SheetOrientation.Landscape),
        new("A2", 594_000, 420_000, SheetOrientation.Landscape),
        new("A3", 420_000, 297_000, SheetOrientation.Landscape),
        new("A4", 297_000, 210_000, SheetOrientation.Portrait),
        new("A4×2", 594_000, 210_000, SheetOrientation.Portrait),
        new("A3×2", 840_000, 297_000, SheetOrientation.Portrait),
        new("A2×2", 1_188_000, 420_000, SheetOrientation.Portrait),
        new("A1×2", 1_682_000, 594_000, SheetOrientation.Portrait),
        new("A0×2", 2_378_000, 841_000, SheetOrientation.Portrait)
    ];

    private static readonly Dictionary<string, SheetFormat> ByName =
        Formats.ToDictionary(f => f.Name, StringComparer.Ordinal);

    /// <summary>
    /// Все форматы каталога. Порядок соответствует меню «Новый шаблон».
    /// </summary>
    public static IReadOnlyList<SheetFormat> All { get; } = Formats;

    /// <summary>
    /// Формат по умолчанию (<see cref="DefaultName"/>).
    /// </summary>
    public static SheetFormat Default => Get(DefaultName);

    /// <summary>
    /// Получить формат по имени. Регистр не учитывается; латинская «X»
    /// в записи полуформатов эквивалентна «×».
    /// Неизвестный формат — <see cref="ArgumentException"/>.
    /// </summary>
    public static SheetFormat Get(string format)
    {
        ArgumentNullException.ThrowIfNull(format);

        if (TryGet(format, out var entry))
            return entry;

        throw new ArgumentException($"Неизвестный формат листа: {format}", nameof(format));
    }

    /// <summary>
    /// Попробовать получить формат по имени (семантика <see cref="Get"/>,
    /// но без исключений).
    /// </summary>
    public static bool TryGet(string? format, [NotNullWhen(true)] out SheetFormat? entry)
    {
        entry = null;
        return format != null && ByName.TryGetValue(NormalizeKey(format), out entry);
    }

    /// <summary>
    /// Входит ли имя в каталог (семантика <see cref="TryGet"/>).
    /// </summary>
    public static bool Contains(string? format) => TryGet(format, out _);

    /// <summary>
    /// Каноническое имя формата (нормализация регистра и «X» → «×»).
    /// Неизвестный формат — <see cref="ArgumentException"/>.
    /// </summary>
    public static string Normalize(string format) => Get(format).Name;

    private static string NormalizeKey(string format)
        => format.ToUpperInvariant().Replace("X", "×");
}
