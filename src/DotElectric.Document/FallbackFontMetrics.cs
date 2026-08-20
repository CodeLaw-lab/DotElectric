namespace DotElectric.Document;

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
