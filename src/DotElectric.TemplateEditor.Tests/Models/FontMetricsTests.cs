using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Tests.Helpers;
using Moq;

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

    // ---- IFontMetrics interface via mock ----

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

    [Fact]
    public void IFontMetrics_Mock_IsInitialized_DefaultFalse()
    {
        var mock = new Mock<IFontMetrics>();
        mock.Setup(m => m.IsInitialized).Returns(false);

        Assert.False(mock.Object.IsInitialized);
    }

    [Fact]
    public void IFontMetrics_Mock_CanVerifyInitializeCalled()
    {
        var mock = new Mock<IFontMetrics>(MockBehavior.Loose);

        mock.Object.Initialize();

        mock.Verify(m => m.Initialize(), Times.Once);
    }

    [Fact]
    public void IFontMetrics_Mock_CanVerifyResetCalled()
    {
        var mock = new Mock<IFontMetrics>(MockBehavior.Loose);

        mock.Object.Reset();

        mock.Verify(m => m.Reset(), Times.Once);
    }

    [Fact]
    public void IFontMetrics_Mock_CanVerifyInitializeWithTestValuesCalled()
    {
        var mock = new Mock<IFontMetrics>(MockBehavior.Loose);

        mock.Object.InitializeWithTestValues(1.0, 0.5, "ГОСТ А");

        mock.Verify(m => m.InitializeWithTestValues(1.0, 0.5, "ГОСТ А"), Times.Once);
    }

    // ---- Fresh instance (non-Default singleton) ----

    [Fact]
    public void FreshInstance_IsInitialized_False()
    {
        var fm = new FontMetrics();

        Assert.False(fm.IsInitialized);
    }

    [Fact]
    public void FreshInstance_GetHeightRatio_ReturnsFallbackOne()
    {
        var fm = new FontMetrics();

        Assert.Equal(1.0, fm.GetHeightRatio("ГОСТ А"));
        Assert.Equal(1.0, fm.GetHeightRatio("ГОСТ Б"));
    }

    [Fact]
    public void FreshInstance_GetAdvWidthRatio_GostA_ReturnsHeuristic()
    {
        var fm = new FontMetrics();

        Assert.Equal(0.5, fm.GetAdvWidthRatio("ГОСТ А"));
    }

    [Fact]
    public void FreshInstance_GetAdvWidthRatio_GostB_ReturnsHeuristic()
    {
        var fm = new FontMetrics();

        Assert.Equal(0.65, fm.GetAdvWidthRatio("ГОСТ Б"));
    }

    [Fact]
    public void FreshInstance_GetAdvWidthRatio_Unknown_ReturnsDefaultFallback()
    {
        var fm = new FontMetrics();

        Assert.Equal(0.6, fm.GetAdvWidthRatio("Unknown"));
    }

    // ---- Multiple font types with InitializeWithTestValues ----

    [Fact]
    public void InitializeWithTestValues_MultipleFonts_BothAccessible()
    {
        var fm = new FontMetrics();
        fm.InitializeWithTestValues(1.1, 0.5, "ГОСТ А");
        fm.InitializeWithTestValues(1.2, 0.65, "ГОСТ Б");

        Assert.Equal(1.1, fm.GetHeightRatio("ГОСТ А"), precision: 4);
        Assert.Equal(1.2, fm.GetHeightRatio("ГОСТ Б"), precision: 4);
        Assert.Equal(0.5, fm.GetAdvWidthRatio("ГОСТ А"), precision: 4);
        Assert.Equal(0.65, fm.GetAdvWidthRatio("ГОСТ Б"), precision: 4);
    }

    [Fact]
    public void InitializeWithTestValues_Reinitialize_OverwritesValues()
    {
        var fm = new FontMetrics();
        fm.InitializeWithTestValues(1.1, 0.5, "ГОСТ А");
        fm.InitializeWithTestValues(2.2, 0.75, "ГОСТ А");

        Assert.Equal(2.2, fm.GetHeightRatio("ГОСТ А"), precision: 4);
        Assert.Equal(0.75, fm.GetAdvWidthRatio("ГОСТ А"), precision: 4);
    }

    // ---- IsInitialized after InitializeWithTestValues ----

    [Fact]
    public void IsInitialized_AfterInitializeWithTestValues_True()
    {
        var fm = new FontMetrics();
        fm.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");

        Assert.True(fm.IsInitialized);
    }

    // ---- Reset after initialization ----

    [Fact]
    public void Reset_AfterInitialize_ClearsState()
    {
        var fm = new FontMetrics();
        fm.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        Assert.True(fm.IsInitialized);

        fm.Reset();

        Assert.False(fm.IsInitialized);
        Assert.Equal(1.0, fm.GetHeightRatio("ГОСТ Б"));
        Assert.Equal(0.65, fm.GetAdvWidthRatio("ГОСТ Б"));
    }

    [Fact]
    public void Reset_FreshInstance_NoOp()
    {
        var fm = new FontMetrics();
        Assert.False(fm.IsInitialized);

        fm.Reset();

        Assert.False(fm.IsInitialized);
    }

    // ---- Initialize() real path (fallbacks in testhost; real TTF loading is
    //      impossible in testhost: Application.ResourceAssembly is locked to the
    //      test assembly and FontFamily/GlyphTypeface cannot open TTF streams) ----

    [Fact]
    public void Initialize_WithoutResourceAssembly_AppliesFallbackRatios()
    {
        WpfContext.Execute(() =>
        {
            var fm = new FontMetrics();
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
            var fm = new FontMetrics();

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
            var fm = new FontMetrics();
            fm.Initialize();
            Assert.True(fm.IsInitialized);

            fm.Reset();

            Assert.False(fm.IsInitialized);
            Assert.Equal(1.0, fm.GetHeightRatio("ГОСТ А"));
            Assert.Equal(0.5, fm.GetAdvWidthRatio("ГОСТ А"));
        });
    }

    [Fact]
    public void LoadFont_UnknownFamily_AppliesFallbackDefaults()
    {
        WpfContext.Execute(() =>
        {
            var fm = new FontMetrics();
            var method = typeof(FontMetrics).GetMethod(
                "LoadFont", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            method!.Invoke(fm, new object[] { "Тест", "NonExistentFamily", 0.7, 0.4 });

            Assert.Equal(0.7, fm.GetHeightRatio("Тест"));
            Assert.Equal(0.4, fm.GetAdvWidthRatio("Тест"));
        });
    }

    // ---- Unknown font behavior ----

    [Fact]
    public void GetHeightRatio_UnknownFontName_ReturnsFallbackOne()
    {
        var fm = new FontMetrics();

        Assert.Equal(1.0, fm.GetHeightRatio("NonExistentFont"));
        Assert.Equal(1.0, fm.GetHeightRatio("SomeRandomFont"));
    }

    [Fact]
    public void GetAdvWidthRatio_UnknownFontName_ReturnsDefaultFallback()
    {
        var fm = new FontMetrics();

        Assert.Equal(0.6, fm.GetAdvWidthRatio("SomeRandomFont"));
    }

    // ---- Null / empty font name edge cases ----

    [Fact]
    public void GetHeightRatio_Null_ThrowsArgumentNullException()
    {
        var fm = new FontMetrics();

        Assert.Throws<ArgumentNullException>(() => fm.GetHeightRatio(null!));
    }

    [Fact]
    public void GetHeightRatio_EmptyString_ReturnsFallbackOne()
    {
        var fm = new FontMetrics();

        Assert.Equal(1.0, fm.GetHeightRatio(""));
    }

    [Fact]
    public void GetAdvWidthRatio_Null_ThrowsArgumentNullException()
    {
        var fm = new FontMetrics();

        Assert.Throws<ArgumentNullException>(() => fm.GetAdvWidthRatio(null!));
    }

    [Fact]
    public void GetAdvWidthRatio_EmptyString_ReturnsDefaultFallback()
    {
        var fm = new FontMetrics();

        Assert.Equal(0.6, fm.GetAdvWidthRatio(""));
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

        var result = FontMetrics.ComputeAverageAdvanceWidth(
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

        var result = FontMetrics.ComputeAverageAdvanceWidth(
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

        var result = FontMetrics.ComputeAverageAdvanceWidth(
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

        var result = FontMetrics.ComputeAverageAdvanceWidth(
            charToGlyphMap, advanceWidths, new[] { 65, 66 }, fallbackWidth: 100.0);

        Assert.Equal(100.0, result);
    }

    [Fact]
    public void HandleFallbackWithLog_AppliesDefaultRatios_NoThrow()
    {
        var fm = new FontMetrics();
        var method = typeof(FontMetrics).GetMethod(
            "HandleFallbackWithLog",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        method!.Invoke(fm, new object[] { "Тест", "some error", 0.7, 0.4 });

        Assert.Equal(0.7, fm.GetHeightRatio("Тест"));
        Assert.Equal(0.4, fm.GetAdvWidthRatio("Тест"));
    }
}
