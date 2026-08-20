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
