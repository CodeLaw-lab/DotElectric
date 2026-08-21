namespace DotElectric.Document.Tests;

/// <summary>
/// Тесты запасного поставщика метрик — тонкого делегата в каталог шрифтов
/// (спека #162): собственных запасных констант у поставщика нет.
/// </summary>
public class FallbackFontMetricsTests
{
    private static FallbackFontMetrics Metrics => FallbackFontMetrics.Instance;

    [Theory]
    [InlineData("ГОСТ А")]
    [InlineData("ГОСТ Б")]
    [InlineData("Unknown")]
    [InlineData("")]
    public void GetHeightRatio_AnyName_ReturnsCatalogFallbackOne(string fontName)
    {
        Assert.Equal(1.0, Metrics.GetHeightRatio(fontName));
    }

    [Theory]
    [InlineData("ГОСТ А", 0.5)]
    [InlineData("ГОСТ Б", 0.65)]
    public void GetAdvWidthRatio_KnownFont_ReturnsCatalogFallback(string fontName, double expected)
    {
        Assert.Equal(expected, Metrics.GetAdvWidthRatio(fontName));
    }

    [Theory]
    [InlineData("Unknown")]
    [InlineData("")]
    [InlineData(null)]
    public void GetAdvWidthRatio_UnknownOrNull_ReturnsDefaultFontRatio(string? fontName)
    {
        Assert.Equal(Metrics.GetAdvWidthRatio(FontCatalog.DefaultName), Metrics.GetAdvWidthRatio(fontName!));
    }

    [Fact]
    public void Instance_IsSingleton()
    {
        Assert.Same(FallbackFontMetrics.Instance, FallbackFontMetrics.Instance);
    }
}
