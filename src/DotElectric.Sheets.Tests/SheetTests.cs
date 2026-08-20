namespace DotElectric.Sheets.Tests;

public class SheetTests
{
    [Theory]
    [InlineData("A0", 1189000, 841000)]
    [InlineData("A1", 841000, 594000)]
    [InlineData("A2", 594000, 420000)]
    [InlineData("A3", 420000, 297000)]
    [InlineData("A4", 210000, 297000)]
    public void FromFormat_ValidFormat_SetsCorrectDimensions(string format, long expectedWidth, long expectedHeight)
    {
        var sheet = Sheet.FromFormat(format);

        Assert.Equal(format, sheet.Format);
        Assert.Equal(expectedWidth, sheet.WidthMicrons);
        Assert.Equal(expectedHeight, sheet.HeightMicrons);
    }

    [Fact]
    public void FromFormat_InvalidFormat_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Sheet.FromFormat("A5"));
    }

    [Fact]
    public void FromFormat_LowerCase_Works()
    {
        var sheet = Sheet.FromFormat("a3");
        Assert.Equal("A3", sheet.Format);
    }

    [Theory]
    [InlineData("A0", SheetOrientation.Landscape, 1189_000, 841_000)]
    [InlineData("A1", SheetOrientation.Landscape, 841_000, 594_000)]
    [InlineData("A2", SheetOrientation.Landscape, 594_000, 420_000)]
    [InlineData("A3", SheetOrientation.Landscape, 420_000, 297_000)]
    [InlineData("A4", SheetOrientation.Portrait, 210_000, 297_000)]
    [InlineData("A4×2", SheetOrientation.Portrait, 210_000, 594_000)]
    [InlineData("A3×2", SheetOrientation.Portrait, 297_000, 840_000)]
    [InlineData("A2×2", SheetOrientation.Portrait, 420_000, 1_188_000)]
    [InlineData("A1×2", SheetOrientation.Portrait, 594_000, 1_682_000)]
    [InlineData("A0×2", SheetOrientation.Portrait, 841_000, 2_378_000)]
    public void FromFormat_AllStandardFormats_ReturnsCorrectDimensions(
        string format, SheetOrientation expectedOrientation, long expectedWidth, long expectedHeight)
    {
        var sheet = Sheet.FromFormat(format);

        Assert.Equal(format, sheet.Format);
        Assert.Equal(expectedOrientation, sheet.Orientation);
        Assert.Equal(expectedWidth, sheet.WidthMicrons);
        Assert.Equal(expectedHeight, sheet.HeightMicrons);
    }

    [Fact]
    public void Custom_SetsCustomFormatAndDimensions()
    {
        var sheet = Sheet.Custom(500.0, 350.0);

        Assert.Equal("Custom", sheet.Format);
        Assert.Equal(500000, sheet.WidthMicrons);
        Assert.Equal(350000, sheet.HeightMicrons);
    }

    [Fact]
    public void WidthMm_ReturnsCorrectValue()
    {
        var sheet = Sheet.FromFormat("A4");
        Assert.Equal(210.0, sheet.WidthMm, tolerance: 0.001);
    }

    [Fact]
    public void HeightMm_ReturnsCorrectValue()
    {
        var sheet = Sheet.FromFormat("A4");
        Assert.Equal(297.0, sheet.HeightMm, tolerance: 0.001);
    }

    [Fact]
    public void Unit_AlwaysMm()
    {
        var sheet = Sheet.FromFormat("A4");
        Assert.Equal("mm", sheet.Unit);
    }

    [Theory]
    [InlineData("A4×2", 594000, 210000)]
    [InlineData("A3×2", 840000, 297000)]
    [InlineData("A2×2", 1188000, 420000)]
    [InlineData("A1×2", 1682000, 594000)]
    [InlineData("A0×2", 2378000, 841000)]
    public void FromFormat_HalfFormats_CorrectDimensions(string format, long expectedWide, long expectedNarrow)
    {
        var sheet = Sheet.FromFormat(format);
        Assert.Equal(format, sheet.Format);
        Assert.Equal(SheetOrientation.Portrait, sheet.Orientation);
        Assert.Equal(expectedNarrow, sheet.WidthMicrons);
        Assert.Equal(expectedWide, sheet.HeightMicrons);
    }

    [Theory]
    [InlineData("a4×2")]
    [InlineData("A4X2")]
    [InlineData("a4x2")]
    [InlineData("A4x2")]
    public void FromFormat_HalfFormat_CaseInsensitive_NormalizesToUnicode(string input)
    {
        var sheet = Sheet.FromFormat(input);
        Assert.Equal("A4×2", sheet.Format);
    }
}
