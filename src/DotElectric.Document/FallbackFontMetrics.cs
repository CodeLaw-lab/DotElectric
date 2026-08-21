namespace DotElectric.Document;

/// <summary>
/// Запасной поставщик метрик шрифта для потребителей без редактора —
/// тонкий делегат в каталог шрифтов: запасные коэффициенты живут только
/// в <see cref="FontCatalog"/> (спека #162). Неизвестное имя шрифта
/// нормализуется к шрифту по умолчанию.
/// </summary>
internal sealed class FallbackFontMetrics : IFontMetrics
{
    public static readonly FallbackFontMetrics Instance = new();

    public double GetHeightRatio(string fontName)
        => FontCatalog.Get(FontCatalog.Resolve(fontName)).FallbackHeightRatio;

    public double GetAdvWidthRatio(string fontName)
        => FontCatalog.Get(FontCatalog.Resolve(fontName)).FallbackWidthRatio;
}
