namespace DotElectric.Document.Tests;

/// <summary>
/// Тесты каталога шрифтов — единственного владельца строковой идентичности шрифтов,
/// внутренних имён файлов шрифтов и запасных коэффициентов метрик (спека #162).
/// Прецедент — тесты каталогов форматов листа и типов объекта.
/// </summary>
public class FontCatalogTests
{
    [Fact]
    public void All_ReturnsExactlyTwoEntries()
    {
        Assert.Equal(2, FontCatalog.All.Count);
    }

    [Fact]
    public void All_ContainsGostAAndGostB()
    {
        var names = FontCatalog.All.Select(d => d.Name).ToList();

        Assert.Contains("ГОСТ А", names);
        Assert.Contains("ГОСТ Б", names);
    }

    [Theory]
    [InlineData("ГОСТ А", "GOST Type AU", 1.0, 0.5)]
    [InlineData("ГОСТ Б", "GOST Type BU", 1.0, 0.65)]
    public void Get_KnownFont_ReturnsDescriptorWithIdentityAndFallbackRatios(
        string name, string familyName, double fallbackHeight, double fallbackWidth)
    {
        var descriptor = FontCatalog.Get(name);

        Assert.Equal(name, descriptor.Name);
        Assert.Equal(familyName, descriptor.FamilyName);
        Assert.Equal(fallbackHeight, descriptor.FallbackHeightRatio);
        Assert.Equal(fallbackWidth, descriptor.FallbackWidthRatio);
    }

    [Fact]
    public void DefaultName_IsDocumentDefault()
    {
        Assert.Equal(DocumentDefaults.DefaultFontName, FontCatalog.DefaultName);
        Assert.Equal("ГОСТ А", FontCatalog.DefaultName);
    }

    [Theory]
    [InlineData("ГОСТ А")]
    [InlineData("ГОСТ Б")]
    public void Contains_KnownFont_ReturnsTrue(string name)
    {
        Assert.True(FontCatalog.Contains(name));
    }

    [Theory]
    [InlineData("гост а")]
    [InlineData("GOST A")]
    [InlineData("Arial")]
    [InlineData("")]
    [InlineData(null)]
    public void Contains_UnknownOrNull_ReturnsFalse(string? name)
    {
        Assert.False(FontCatalog.Contains(name));
    }

    [Theory]
    [InlineData("ГОСТ А")]
    [InlineData("ГОСТ Б")]
    public void TryGet_KnownFont_ReturnsDescriptor(string name)
    {
        var found = FontCatalog.TryGet(name, out var descriptor);

        Assert.True(found);
        Assert.NotNull(descriptor);
        Assert.Equal(name, descriptor.Name);
    }

    [Theory]
    [InlineData("Unknown")]
    [InlineData("")]
    [InlineData(null)]
    public void TryGet_UnknownOrNull_ReturnsFalse(string? name)
    {
        var found = FontCatalog.TryGet(name, out var descriptor);

        Assert.False(found);
        Assert.Null(descriptor);
    }

    [Fact]
    public void Get_UnknownFont_ThrowsArgumentException()
    {
        var ex = Assert.Throws<ArgumentException>(() => FontCatalog.Get("Futura"));

        Assert.Contains("Futura", ex.Message);
    }

    [Fact]
    public void Get_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => FontCatalog.Get(null!));
    }

    [Theory]
    [InlineData("ГОСТ А", "ГОСТ А")]
    [InlineData("ГОСТ Б", "ГОСТ Б")]
    public void Resolve_KnownFont_ReturnsSameName(string name, string expected)
    {
        Assert.Equal(expected, FontCatalog.Resolve(name));
    }

    [Theory]
    [InlineData("Futura")]
    [InlineData("гост а")]
    [InlineData("")]
    [InlineData(null)]
    public void Resolve_UnknownOrNull_ReturnsDefaultName(string? name)
    {
        Assert.Equal(FontCatalog.DefaultName, FontCatalog.Resolve(name));
    }

    [Fact]
    public void FallbackRatios_LiveOnlyInCatalog_WpfFallbacksMatch()
    {
        var gostA = FontCatalog.Get("ГОСТ А");
        var gostB = FontCatalog.Get("ГОСТ Б");

        Assert.Equal(1.0, gostA.FallbackHeightRatio);
        Assert.Equal(0.5, gostA.FallbackWidthRatio);
        Assert.Equal(1.0, gostB.FallbackHeightRatio);
        Assert.Equal(0.65, gostB.FallbackWidthRatio);
    }
}
