using System.Diagnostics.CodeAnalysis;

namespace DotElectric.Document;

/// <summary>
/// Каталог шрифтов — единственный владелец строковой идентичности шрифтов
/// (сериализуется в .tdel), внутренних имён файлов шрифтов и запасных
/// коэффициентов метрик. Запасной поставщик метрик, WPF-реализация метрик
/// и правила рендера делегируют каталогу; неизвестное имя шрифта ведёт себя
/// как шрифт по умолчанию (спека #162). Прецеденты — <c>SheetFormatCatalog</c>,
/// <c>ObjectTypeCatalog</c>.
/// </summary>
public static class FontCatalog
{
    private static readonly FontDescriptor[] Fonts =
    [
        new(DocumentDefaults.DefaultFontName, "GOST Type AU", 1.0, 0.5),
        new("ГОСТ Б", "GOST Type BU", 1.0, 0.65)
    ];

    private static readonly Dictionary<string, FontDescriptor> ByName =
        Fonts.ToDictionary(f => f.Name, StringComparer.Ordinal);

    /// <summary>
    /// Имя шрифта по умолчанию — из единого источника дефолтов
    /// документной библиотеки.
    /// </summary>
    public static string DefaultName => DocumentDefaults.DefaultFontName;

    /// <summary>
    /// Все шрифты каталога.
    /// </summary>
    public static IReadOnlyList<FontDescriptor> All { get; } = Fonts;

    /// <summary>
    /// Получить шрифт по имени. Идентичность точная (регистрозависимая);
    /// неизвестный шрифт — <see cref="ArgumentException"/>.
    /// </summary>
    public static FontDescriptor Get(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (TryGet(name, out var entry))
            return entry;

        throw new ArgumentException($"Неизвестный шрифт: {name}", nameof(name));
    }

    /// <summary>
    /// Попробовать получить шрифт по имени (семантика <see cref="Get"/>,
    /// но без исключений).
    /// </summary>
    public static bool TryGet(string? name, [NotNullWhen(true)] out FontDescriptor? entry)
    {
        entry = null;
        return name != null && ByName.TryGetValue(name, out entry);
    }

    /// <summary>
    /// Входит ли имя в каталог (семантика <see cref="TryGet"/>).
    /// </summary>
    public static bool Contains(string? name) => TryGet(name, out _);

    /// <summary>
    /// Нормализация имени: известное имя — без изменений; неизвестное
    /// или отсутствующее — имя шрифта по умолчанию.
    /// </summary>
    public static string Resolve(string? name)
        => TryGet(name, out var entry) ? entry.Name : DefaultName;
}
