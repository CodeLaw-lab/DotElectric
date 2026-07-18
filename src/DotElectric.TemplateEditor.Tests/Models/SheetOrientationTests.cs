using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Tests.Models;

/// <summary>
/// Тесты для SheetOrientation enum и Sheet.FromFormat с ориентацией.
/// </summary>
public class SheetOrientationTests
{
    // === SheetOrientation enum ===

    [Fact]
    public void Enum_HasPortraitValue()
    {
        var portrait = SheetOrientation.Portrait;
        Assert.Equal("Portrait", portrait.ToString());
    }

    [Fact]
    public void Enum_HasLandscapeValue()
    {
        var landscape = SheetOrientation.Landscape;
        Assert.Equal("Landscape", landscape.ToString());
    }

    // === GetDefaultOrientation ===

    [Theory]
    [InlineData("A0")]
    [InlineData("A1")]
    [InlineData("A2")]
    [InlineData("A3")]
    public void GetDefaultOrientation_A0toA3_ReturnsLandscape(string format)
    {
        var orientation = Sheet.GetDefaultOrientation(format);
        Assert.Equal(SheetOrientation.Landscape, orientation);
    }

    [Fact]
    public void GetDefaultOrientation_A4_ReturnsPortrait()
    {
        var orientation = Sheet.GetDefaultOrientation("A4");
        Assert.Equal(SheetOrientation.Portrait, orientation);
    }

    [Theory]
    [InlineData("A4×2")]
    [InlineData("A4X2")]
    [InlineData("A3×2")]
    [InlineData("A3X2")]
    [InlineData("A2×2")]
    [InlineData("A2X2")]
    [InlineData("A1×2")]
    [InlineData("A1X2")]
    [InlineData("A0×2")]
    [InlineData("A0X2")]
    public void GetDefaultOrientation_HalfFormats_ReturnsPortrait(string format)
    {
        var orientation = Sheet.GetDefaultOrientation(format);
        Assert.Equal(SheetOrientation.Portrait, orientation);
    }

    // === FromFormat с ориентацией ===

    [Theory]
    [InlineData("A0", SheetOrientation.Landscape, 1189, 841)]
    [InlineData("A0", SheetOrientation.Portrait, 841, 1189)]
    [InlineData("A1", SheetOrientation.Landscape, 841, 594)]
    [InlineData("A1", SheetOrientation.Portrait, 594, 841)]
    [InlineData("A2", SheetOrientation.Landscape, 594, 420)]
    [InlineData("A2", SheetOrientation.Portrait, 420, 594)]
    [InlineData("A3", SheetOrientation.Landscape, 420, 297)]
    [InlineData("A3", SheetOrientation.Portrait, 297, 420)]
    [InlineData("A4", SheetOrientation.Landscape, 297, 210)]
    [InlineData("A4", SheetOrientation.Portrait, 210, 297)]
    [InlineData("A4×2", SheetOrientation.Portrait, 210, 594)]
    [InlineData("A4×2", SheetOrientation.Landscape, 594, 210)]
    [InlineData("A3×2", SheetOrientation.Portrait, 297, 840)]
    [InlineData("A3×2", SheetOrientation.Landscape, 840, 297)]
    [InlineData("A2×2", SheetOrientation.Portrait, 420, 1188)]
    [InlineData("A2×2", SheetOrientation.Landscape, 1188, 420)]
    public void FromFormat_WithOrientation_ReturnsCorrectDimensions(
        string format,
        SheetOrientation orientation,
        int expectedWidthMm,
        int expectedHeightMm)
    {
        var sheet = Sheet.FromFormat(format, orientation);

        Assert.Equal(format, sheet.Format);
        Assert.Equal(orientation, sheet.Orientation);
        Assert.Equal(expectedWidthMm * 1000, sheet.WidthMicrons);
        Assert.Equal(expectedHeightMm * 1000, sheet.HeightMicrons);
    }

    // === FromFormat без ориентации (использует дефолт) ===

    [Fact]
    public void FromFormat_A4WithoutOrientation_DefaultsToPortrait()
    {
        var sheet = Sheet.FromFormat("A4");

        Assert.Equal("A4", sheet.Format);
        Assert.Equal(SheetOrientation.Portrait, sheet.Orientation);
        Assert.Equal(210_000, sheet.WidthMicrons);
        Assert.Equal(297_000, sheet.HeightMicrons);
    }

    [Theory]
    [InlineData("A0")]
    [InlineData("A1")]
    [InlineData("A2")]
    [InlineData("A3")]
    public void FromFormat_A0toA3WithoutOrientation_DefaultsToLandscape(string format)
    {
        var sheet = Sheet.FromFormat(format);

        Assert.Equal(format, sheet.Format);
        Assert.Equal(SheetOrientation.Landscape, sheet.Orientation);
    }

    [Theory]
    [InlineData("A4×2")]
    [InlineData("A3×2")]
    [InlineData("A2×2")]
    [InlineData("A1×2")]
    [InlineData("A0×2")]
    public void FromFormat_HalfFormatsWithoutOrientation_DefaultsToPortrait(string format)
    {
        var sheet = Sheet.FromFormat(format);

        Assert.Equal(format, sheet.Format);
        Assert.Equal(SheetOrientation.Portrait, sheet.Orientation);
    }

    // === A4 Portrait vs Landscape ===

    [Fact]
    public void A4_Portrait_WidthLessThanHeight()
    {
        var portrait = Sheet.FromFormat("A4", SheetOrientation.Portrait);
        Assert.True(portrait.WidthMicrons < portrait.HeightMicrons);
    }

    [Fact]
    public void A4_Landscape_WidthGreaterThanHeight()
    {
        var landscape = Sheet.FromFormat("A4", SheetOrientation.Landscape);
        Assert.True(landscape.WidthMicrons > landscape.HeightMicrons);
    }

    [Fact]
    public void A4_Portrait_HasSameAreaAsLandscape()
    {
        var portrait = Sheet.FromFormat("A4", SheetOrientation.Portrait);
        var landscape = Sheet.FromFormat("A4", SheetOrientation.Landscape);

        var portraitArea = portrait.WidthMicrons * portrait.HeightMicrons;
        var landscapeArea = landscape.WidthMicrons * landscape.HeightMicrons;

        Assert.Equal(portraitArea, landscapeArea);
    }

    // === Custom sheet с ориентацией ===

    [Fact]
    public void Custom_Sheet_HasCustomDimensions()
    {
        var sheet = Sheet.Custom(500, 700);

        Assert.Equal("Custom", sheet.Format);
        Assert.Equal(500_000, sheet.WidthMicrons);
        Assert.Equal(700_000, sheet.HeightMicrons);
    }

    // === Invalid format ===

    [Theory]
    [InlineData("A5")]
    [InlineData("B4")]
    [InlineData("")]
    [InlineData("invalid")]
    public void FromFormat_InvalidFormat_ThrowsArgumentException(string format)
    {
        Assert.Throws<ArgumentException>(() => Sheet.FromFormat(format));
    }
}
