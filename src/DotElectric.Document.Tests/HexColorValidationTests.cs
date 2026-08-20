namespace DotElectric.Document.Tests;

public class HexColorValidationTests
{
    [Fact]
    public void HexColorValidation_ValidHex_ReturnsNull()
    {
        Assert.Null(HexColorValidation.Validate("#FF0000"));
        Assert.Null(HexColorValidation.Validate("#000000"));
        Assert.Null(HexColorValidation.Validate("#123ABC"));
        Assert.Null(HexColorValidation.Validate("#AABBCCDD"));
    }

    [Fact]
    public void HexColorValidation_Transparent_ReturnsNull()
    {
        Assert.Null(HexColorValidation.Validate("Transparent"));
    }

    [Fact]
    public void HexColorValidation_InvalidHex_ReturnsError()
    {
        Assert.NotNull(HexColorValidation.Validate("not-a-color"));
        Assert.NotNull(HexColorValidation.Validate("#GGG"));
        Assert.NotNull(HexColorValidation.Validate("#12345"));
        Assert.NotNull(HexColorValidation.Validate(""));
    }

    [Fact]
    public void HexColorValidation_Empty_ReturnsError()
    {
        Assert.NotNull(HexColorValidation.Validate(""));
        Assert.NotNull(HexColorValidation.Validate(null));
    }
}
