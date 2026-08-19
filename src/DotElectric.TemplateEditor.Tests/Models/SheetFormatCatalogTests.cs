using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Tests.Models;

/// <summary>
/// Тесты каталога стандартных форматов листа (шов №1 кандидата 5 обзора №4).
/// Литеральные ожидания — пины байт-в-байт: каталог не читается из себя в ожиданиях.
/// </summary>
public class SheetFormatCatalogTests
{
    // === Состав и порядок ===

    [Fact]
    public void All_ReturnsTenFormats_InMenuOrder()
    {
        var names = SheetFormatCatalog.All.Select(f => f.Name).ToArray();

        Assert.Equal(
            ["A0", "A1", "A2", "A3", "A4", "A4×2", "A3×2", "A2×2", "A1×2", "A0×2"],
            names);
    }

    [Theory]
    [InlineData("A0", 1_189_000, 841_000, SheetOrientation.Landscape)]
    [InlineData("A1", 841_000, 594_000, SheetOrientation.Landscape)]
    [InlineData("A2", 594_000, 420_000, SheetOrientation.Landscape)]
    [InlineData("A3", 420_000, 297_000, SheetOrientation.Landscape)]
    [InlineData("A4", 297_000, 210_000, SheetOrientation.Portrait)]
    [InlineData("A4×2", 594_000, 210_000, SheetOrientation.Portrait)]
    [InlineData("A3×2", 840_000, 297_000, SheetOrientation.Portrait)]
    [InlineData("A2×2", 1_188_000, 420_000, SheetOrientation.Portrait)]
    [InlineData("A1×2", 1_682_000, 594_000, SheetOrientation.Portrait)]
    [InlineData("A0×2", 2_378_000, 841_000, SheetOrientation.Portrait)]
    public void All_ReturnsCorrectDimensionsAndDefaultOrientation(
        string name,
        long expectedLongMicrons,
        long expectedShortMicrons,
        SheetOrientation expectedDefault)
    {
        var entry = Assert.Single(SheetFormatCatalog.All, f => f.Name == name);

        Assert.Equal(expectedLongMicrons, entry.LongSideMicrons);
        Assert.Equal(expectedShortMicrons, entry.ShortSideMicrons);
        Assert.Equal(expectedDefault, entry.DefaultOrientation);
    }

    // === Get ===

    [Fact]
    public void Get_KnownFormat_ReturnsEntry()
    {
        var entry = SheetFormatCatalog.Get("A3");

        Assert.Equal("A3", entry.Name);
        Assert.Equal(420_000, entry.LongSideMicrons);
        Assert.Equal(297_000, entry.ShortSideMicrons);
        Assert.Equal(SheetOrientation.Landscape, entry.DefaultOrientation);
    }

    [Theory]
    [InlineData("a4", "A4")]
    [InlineData("A4x2", "A4×2")]
    [InlineData("a4×2", "A4×2")]
    [InlineData("A0X2", "A0×2")]
    [InlineData("a0x2", "A0×2")]
    public void Get_CaseInsensitiveAndLatinX_NormalizesToCanonical(string input, string expectedName)
    {
        var entry = SheetFormatCatalog.Get(input);

        Assert.Equal(expectedName, entry.Name);
    }

    [Fact]
    public void Get_UnknownFormat_ThrowsArgumentExceptionWithFormatInMessage()
    {
        var ex = Assert.Throws<ArgumentException>(() => SheetFormatCatalog.Get("A5"));

        Assert.Contains("Неизвестный формат листа: A5", ex.Message);
    }

    [Fact]
    public void Get_EmptyString_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => SheetFormatCatalog.Get(""));
    }

    [Fact]
    public void Get_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => SheetFormatCatalog.Get(null!));
    }

    // === TryGet ===

    [Fact]
    public void TryGet_KnownFormat_ReturnsTrueAndEntry()
    {
        var found = SheetFormatCatalog.TryGet("A1×2", out var entry);

        Assert.True(found);
        Assert.NotNull(entry);
        Assert.Equal("A1×2", entry.Name);
    }

    [Theory]
    [InlineData("A5")]
    [InlineData("")]
    [InlineData("Custom")]
    public void TryGet_UnknownFormat_ReturnsFalse(string input)
    {
        var found = SheetFormatCatalog.TryGet(input, out var entry);

        Assert.False(found);
        Assert.Null(entry);
    }

    [Fact]
    public void TryGet_Null_ReturnsFalse()
    {
        var found = SheetFormatCatalog.TryGet(null, out var entry);

        Assert.False(found);
        Assert.Null(entry);
    }

    // === Contains ===

    [Theory]
    [InlineData("A0")]
    [InlineData("a4")]
    [InlineData("A4X2")]
    [InlineData("A4×2")]
    [InlineData("a0x2")]
    public void Contains_KnownFormat_ReturnsTrue(string input)
    {
        Assert.True(SheetFormatCatalog.Contains(input));
    }

    [Theory]
    [InlineData("A5")]
    [InlineData("")]
    [InlineData("Custom")]
    public void Contains_UnknownFormat_ReturnsFalse(string input)
    {
        Assert.False(SheetFormatCatalog.Contains(input));
    }

    [Fact]
    public void Contains_Null_ReturnsFalse()
    {
        Assert.False(SheetFormatCatalog.Contains(null));
    }

    // === Normalize ===

    [Theory]
    [InlineData("a4x2", "A4×2")]
    [InlineData("A3X2", "A3×2")]
    [InlineData("a0", "A0")]
    public void Normalize_ReturnsCanonicalName(string input, string expected)
    {
        Assert.Equal(expected, SheetFormatCatalog.Normalize(input));
    }

    // === Дефолтный формат ===

    [Fact]
    public void DefaultName_IsA3()
    {
        Assert.Equal("A3", SheetFormatCatalog.DefaultName);
    }

    [Fact]
    public void Default_ReturnsA3Entry()
    {
        var entry = SheetFormatCatalog.Default;

        Assert.Equal("A3", entry.Name);
        Assert.Equal(SheetOrientation.Landscape, entry.DefaultOrientation);
    }
}
