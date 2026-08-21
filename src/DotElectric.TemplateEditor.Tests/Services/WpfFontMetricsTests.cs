using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tests.Helpers;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Services;

[Collection("FontMetrics")]
public class WpfFontMetricsTests : IDisposable
{
    public WpfFontMetricsTests()
    {
        WpfFontMetrics.Default.Reset();
    }

    public void Dispose()
    {
        WpfFontMetrics.Default.Reset();
    }

    // ---- Default state (no initialization) ----

    [Fact]
    public void IsInitialized_Default_False()
    {
        Assert.False(WpfFontMetrics.Default.IsInitialized);
    }

    [Fact]
    public void GetHeightRatio_Default_ReturnsFallbackOne()
    {
        Assert.Equal(1.0, WpfFontMetrics.Default.GetHeightRatio("ГОСТ А"));
        Assert.Equal(1.0, WpfFontMetrics.Default.GetHeightRatio("ГОСТ Б"));
    }

    [Fact]
    public void GetAdvWidthRatio_Default_GostA_ReturnsHeuristic()
    {
        Assert.Equal(0.5, WpfFontMetrics.Default.GetAdvWidthRatio("ГОСТ А"));
    }

    [Fact]
    public void GetAdvWidthRatio_Default_GostB_ReturnsHeuristic()
    {
        Assert.Equal(0.65, WpfFontMetrics.Default.GetAdvWidthRatio("ГОСТ Б"));
    }

    [Fact]
    public void GetAdvWidthRatio_Default_Unknown_ReturnsDefaultFontRatio()
    {
        Assert.Equal(
            WpfFontMetrics.Default.GetAdvWidthRatio("ГОСТ А"),
            WpfFontMetrics.Default.GetAdvWidthRatio("Unknown"));
    }

    // ---- IFontMetrics contract (read-only) via mock ----

    [Fact]
    public void IFontMetrics_Mock_CanSetupGetHeightRatio()
    {
        var mock = new Mock<IFontMetrics>();
        mock.Setup(m => m.GetHeightRatio("ГОСТ А")).Returns(1.5);

        Assert.Equal(1.5, mock.Object.GetHeightRatio("ГОСТ А"));
    }

    [Fact]
    public void IFontMetrics_Mock_CanSetupGetAdvWidthRatio()
    {
        var mock = new Mock<IFontMetrics>();
        mock.Setup(m => m.GetAdvWidthRatio("ГОСТ Б")).Returns(0.75);

        Assert.Equal(0.75, mock.Object.GetAdvWidthRatio("ГОСТ Б"));
    }

    // ---- Fresh instance (non-Default singleton) ----

    [Fact]
    public void FreshInstance_IsInitialized_False()
    {
        var fm = new WpfFontMetrics();

        Assert.False(fm.IsInitialized);
    }

    [Fact]
    public void FreshInstance_GetHeightRatio_ReturnsFallbackOne()
    {
        var fm = new WpfFontMetrics();

        Assert.Equal(1.0, fm.GetHeightRatio("ГОСТ А"));
        Assert.Equal(1.0, fm.GetHeightRatio("ГОСТ Б"));
    }

    [Fact]
    public void FreshInstance_GetAdvWidthRatio_GostA_ReturnsHeuristic()
    {
        var fm = new WpfFontMetrics();

        Assert.Equal(0.5, fm.GetAdvWidthRatio("ГОСТ А"));
    }

    [Fact]
    public void FreshInstance_GetAdvWidthRatio_GostB_ReturnsHeuristic()
    {
        var fm = new WpfFontMetrics();

        Assert.Equal(0.65, fm.GetAdvWidthRatio("ГОСТ Б"));
    }

    [Fact]
    public void FreshInstance_GetAdvWidthRatio_Unknown_ReturnsDefaultFontRatio()
    {
        var fm = new WpfFontMetrics();

        Assert.Equal(fm.GetAdvWidthRatio("ГОСТ А"), fm.GetAdvWidthRatio("Unknown"));
    }

    // ---- Initialize() real path (fallbacks in testhost; real TTF loading is
    //      impossible in testhost: Application.ResourceAssembly is locked to the
    //      test assembly and FontFamily/GlyphTypeface cannot open TTF streams) ----

    [Fact]
    public void Initialize_WithoutResourceAssembly_AppliesFallbackRatios()
    {
        WpfContext.Execute(() =>
        {
            var fm = new WpfFontMetrics();
            fm.Initialize();

            Assert.True(fm.IsInitialized);
            Assert.Equal(1.0, fm.GetHeightRatio("ГОСТ А"));
            Assert.Equal(0.5, fm.GetAdvWidthRatio("ГОСТ А"));
            Assert.Equal(0.65, fm.GetAdvWidthRatio("ГОСТ Б"));
        });
    }

    [Fact]
    public void Initialize_CalledTwice_IsIdempotent()
    {
        WpfContext.Execute(() =>
        {
            var fm = new WpfFontMetrics();

            fm.Initialize();
            fm.Initialize();

            Assert.True(fm.IsInitialized);
            Assert.Equal(1.0, fm.GetHeightRatio("ГОСТ А"));
            Assert.Equal(0.5, fm.GetAdvWidthRatio("ГОСТ А"));
            Assert.Equal(0.65, fm.GetAdvWidthRatio("ГОСТ Б"));
        });
    }

    [Fact]
    public void Reset_AfterInitialize_ReturnsToFallbacks()
    {
        WpfContext.Execute(() =>
        {
            var fm = new WpfFontMetrics();
            fm.Initialize();
            Assert.True(fm.IsInitialized);

            fm.Reset();

            Assert.False(fm.IsInitialized);
            Assert.Equal(1.0, fm.GetHeightRatio("ГОСТ А"));
            Assert.Equal(0.5, fm.GetAdvWidthRatio("ГОСТ А"));
        });
    }

    [Fact]
    public void Reset_FreshInstance_NoOp()
    {
        var fm = new WpfFontMetrics();
        Assert.False(fm.IsInitialized);

        fm.Reset();

        Assert.False(fm.IsInitialized);
    }

    [Fact]
    public void LoadFont_UnknownFamily_AppliesFallbackDefaults()
    {
        WpfContext.Execute(() =>
        {
            var fm = new WpfFontMetrics();
            var method = typeof(WpfFontMetrics).GetMethod(
                "LoadFont", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            method!.Invoke(fm, new object[] { "Тест", "NonExistentFamily", 0.7, 0.4 });

            Assert.Equal(0.7, fm.GetHeightRatio("Тест"));
            Assert.Equal(0.4, fm.GetAdvWidthRatio("Тест"));
        });
    }

    // ---- Unknown font behavior: неизвестное имя ведёт себя как шрифт
    //      по умолчанию (спека #162) ----

    [Fact]
    public void GetHeightRatio_UnknownFontName_ReturnsDefaultFontRatio()
    {
        var fm = new WpfFontMetrics();

        Assert.Equal(fm.GetHeightRatio("ГОСТ А"), fm.GetHeightRatio("NonExistentFont"));
        Assert.Equal(fm.GetHeightRatio("ГОСТ А"), fm.GetHeightRatio("SomeRandomFont"));
    }

    [Fact]
    public void GetAdvWidthRatio_UnknownFontName_ReturnsDefaultFontRatio()
    {
        var fm = new WpfFontMetrics();

        Assert.Equal(fm.GetAdvWidthRatio("ГОСТ А"), fm.GetAdvWidthRatio("SomeRandomFont"));
    }

    // ---- Null / empty font name edge cases ----

    [Fact]
    public void GetHeightRatio_Null_ThrowsArgumentNullException()
    {
        var fm = new WpfFontMetrics();

        Assert.Throws<ArgumentNullException>(() => fm.GetHeightRatio(null!));
    }

    [Fact]
    public void GetHeightRatio_EmptyString_ReturnsDefaultFontRatio()
    {
        var fm = new WpfFontMetrics();

        Assert.Equal(fm.GetHeightRatio("ГОСТ А"), fm.GetHeightRatio(""));
    }

    [Fact]
    public void GetAdvWidthRatio_Null_ThrowsArgumentNullException()
    {
        var fm = new WpfFontMetrics();

        Assert.Throws<ArgumentNullException>(() => fm.GetAdvWidthRatio(null!));
    }

    [Fact]
    public void GetAdvWidthRatio_EmptyString_ReturnsDefaultFontRatio()
    {
        var fm = new WpfFontMetrics();

        Assert.Equal(fm.GetAdvWidthRatio("ГОСТ А"), fm.GetAdvWidthRatio(""));
    }

    // ---- ComputeAverageAdvanceWidth (pure internal static, no STA) ----

    [Fact]
    public void ComputeAverageAdvanceWidth_SampleChars_FoundAll()
    {
        var charToGlyphMap = new Dictionary<int, ushort>
        {
            [65] = 1, // 'A'
            [66] = 2  // 'B'
        };
        var advanceWidths = new Dictionary<ushort, double>
        {
            [1] = 200,
            [2] = 400
        };

        var result = WpfFontMetrics.ComputeAverageAdvanceWidth(
            charToGlyphMap, advanceWidths, new[] { 65, 66 }, fallbackWidth: 100.0);

        Assert.Equal(300.0, result);
    }

    [Fact]
    public void ComputeAverageAdvanceWidth_MissingGlyphs_Skips()
    {
        var charToGlyphMap = new Dictionary<int, ushort>
        {
            [65] = 1 // 'A' есть, 'B'(66) отсутствует
        };
        var advanceWidths = new Dictionary<ushort, double>
        {
            [1] = 200,
            [2] = 400
        };

        var result = WpfFontMetrics.ComputeAverageAdvanceWidth(
            charToGlyphMap, advanceWidths, new[] { 65, 66 }, fallbackWidth: 100.0);

        Assert.Equal(200.0, result);
    }

    [Fact]
    public void ComputeAverageAdvanceWidth_MissingWidths_Skips()
    {
        var charToGlyphMap = new Dictionary<int, ushort>
        {
            [65] = 1, // 'A'
            [66] = 2  // 'B' — glyph есть
        };
        var advanceWidths = new Dictionary<ushort, double>
        {
            [1] = 200 // для glyph 2 ширины нет
        };

        var result = WpfFontMetrics.ComputeAverageAdvanceWidth(
            charToGlyphMap, advanceWidths, new[] { 65, 66 }, fallbackWidth: 100.0);

        Assert.Equal(200.0, result);
    }

    [Fact]
    public void ComputeAverageAdvanceWidth_AllMissing_ReturnsFallback()
    {
        var charToGlyphMap = new Dictionary<int, ushort>();
        var advanceWidths = new Dictionary<ushort, double>
        {
            [1] = 200,
            [2] = 400
        };

        var result = WpfFontMetrics.ComputeAverageAdvanceWidth(
            charToGlyphMap, advanceWidths, new[] { 65, 66 }, fallbackWidth: 100.0);

        Assert.Equal(100.0, result);
    }

    [Fact]
    public void HandleFallbackWithLog_AppliesDefaultRatios_NoThrow()
    {
        var fm = new WpfFontMetrics();
        var method = typeof(WpfFontMetrics).GetMethod(
            "HandleFallbackWithLog",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        method!.Invoke(fm, new object[] { "Тест", "some error", 0.7, 0.4 });

        Assert.Equal(0.7, fm.GetHeightRatio("Тест"));
        Assert.Equal(0.4, fm.GetAdvWidthRatio("Тест"));
    }
}
