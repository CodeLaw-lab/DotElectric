using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Tests.Models;

[Collection("FontMetrics")]
public class FontMetricsTests : IDisposable
{
    public FontMetricsTests()
    {
        FontMetrics.Default.Reset();
    }

    public void Dispose()
    {
        FontMetrics.Default.Reset();
    }

    // ---- Default state (no initialization) ----

    [Fact]
    public void IsInitialized_Default_False()
    {
        Assert.False(FontMetrics.Default.IsInitialized);
    }

    [Fact]
    public void GetHeightRatio_Default_ReturnsFallbackOne()
    {
        Assert.Equal(1.0, FontMetrics.Default.GetHeightRatio("ГОСТ А"));
        Assert.Equal(1.0, FontMetrics.Default.GetHeightRatio("ГОСТ Б"));
    }

    [Fact]
    public void GetAdvWidthRatio_Default_GostA_ReturnsHeuristic()
    {
        Assert.Equal(0.5, FontMetrics.Default.GetAdvWidthRatio("ГОСТ А"));
    }

    [Fact]
    public void GetAdvWidthRatio_Default_GostB_ReturnsHeuristic()
    {
        Assert.Equal(0.65, FontMetrics.Default.GetAdvWidthRatio("ГОСТ Б"));
    }

    [Fact]
    public void GetAdvWidthRatio_Default_Unknown_ReturnsDefaultFallback()
    {
        Assert.Equal(0.6, FontMetrics.Default.GetAdvWidthRatio("Unknown"));
    }

    // ---- After InitializeWithTestValues ----

    [Fact]
    public void GetHeightRatio_AfterInit_ReturnsSetValue()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");

        Assert.Equal(1.1719, FontMetrics.Default.GetHeightRatio("ГОСТ Б"), precision: 4);
    }

    [Fact]
    public void GetAdvWidthRatio_AfterInit_ReturnsSetValue()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");

        Assert.Equal(0.55, FontMetrics.Default.GetAdvWidthRatio("ГОСТ Б"), precision: 4);
    }

    [Fact]
    public void GetHeightRatio_AfterInit_UnknownFont_ReturnsFallback()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");

        Assert.Equal(1.0, FontMetrics.Default.GetHeightRatio("NonExistent"));
    }
}
