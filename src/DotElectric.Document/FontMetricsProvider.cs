namespace DotElectric.Document;

/// <summary>
/// Эмбиентный слот текущей реализации метрик шрифта (ADR-0003).
/// Документная библиотека не зависит от WPF: модель текста читает метрики из слота.
/// Редактор при старте записывает сюда WPF-реализацию (WpfFontMetrics);
/// потребитель без редактора получает запасного поставщика с константными метриками.
/// </summary>
public static class FontMetricsProvider
{
    /// <summary>
    /// Текущий поставщик метрик. Никогда не null — по умолчанию запасной поставщик.
    /// </summary>
    public static IFontMetrics Current { get; private set; } = FallbackFontMetrics.Instance;

    /// <summary>
    /// Установить поставщика метрик (вызывается редактором при старте и тестами).
    /// </summary>
    public static void SetCurrent(IFontMetrics metrics)
        => Current = metrics ?? throw new ArgumentNullException(nameof(metrics));

    /// <summary>
    /// Вернуть запасного поставщика (уборка в тестах, аварийный сброс).
    /// </summary>
    public static void Reset() => Current = FallbackFontMetrics.Instance;
}

/// <summary>
/// Запасной поставщик метрик шрифта — константные значения без загрузки шрифтов:
/// высота 1.0; ширина ГОСТ А 0.5, ГОСТ Б 0.65, неизвестный шрифт 0.6
/// (значения байт-в-байт совпадают с запасными значениями WPF-реализации).
/// </summary>
internal sealed class FallbackFontMetrics : IFontMetrics
{
    public static readonly FallbackFontMetrics Instance = new();

    public double GetHeightRatio(string fontName) => 1.0;

    public double GetAdvWidthRatio(string fontName) => fontName switch
    {
        "ГОСТ А" => 0.5,
        "ГОСТ Б" => 0.65,
        _ => 0.6
    };
}
