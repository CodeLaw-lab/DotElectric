using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Tests.Models;

public class CoordinateTests
{
    // ===== ToMicrons =====

    [Fact]
    public void ToMicrons_PositiveValue_ReturnsCorrectMicrons()
    {
        var result = Coordinate.ToMicrons(5.5);
        Assert.Equal(5500, result);
    }

    [Fact]
    public void ToMicrons_Zero_ReturnsZero()
    {
        var result = Coordinate.ToMicrons(0);
        Assert.Equal(0, result);
    }

    [Fact]
    public void ToMicrons_NegativeValue_ReturnsCorrectMicrons()
    {
        var result = Coordinate.ToMicrons(-3.14);
        Assert.Equal(-3140, result);
    }

    [Fact]
    public void ToMicrons_Fractional_RoundsCorrectly()
    {
        // 0.0005 мм = 0.5 микрон → Math.Round(0.5) = 0 ( banker's rounding)
        var result = Coordinate.ToMicrons(0.0005);
        // 0.001 мм = 1 микрон
        var result2 = Coordinate.ToMicrons(0.001);
        Assert.Equal(0, result);
        Assert.Equal(1, result2);
    }

    // ===== ToMm =====

    [Fact]
    public void ToMm_PositiveValue_ReturnsCorrectMm()
    {
        var result = Coordinate.ToMm(5500);
        Assert.Equal(5.5, result, tolerance: 0.0001);
    }

    [Fact]
    public void ToMm_Zero_ReturnsZero()
    {
        var result = Coordinate.ToMm(0);
        Assert.Equal(0.0, result, tolerance: 0.0001);
    }

    [Fact]
    public void ToMm_NegativeValue_ReturnsCorrectMm()
    {
        var result = Coordinate.ToMm(-3140);
        Assert.Equal(-3.14, result, tolerance: 0.0001);
    }

    // ===== Round-trip =====

    [Fact]
    public void RoundTrip_MmToMicronsToMm_NoLoss()
    {
        var original = 10.0;
        var microns = Coordinate.ToMicrons(original);
        var back = Coordinate.ToMm(microns);
        Assert.Equal(original, back, tolerance: 0.0001);
    }

    [Fact]
    public void RoundTrip_MicronsToMmToMicrons_NoLoss()
    {
        var original = 10500L;
        var mm = Coordinate.ToMm(original);
        var back = Coordinate.ToMicrons(mm);
        Assert.Equal(original, back);
    }

    // ===== SnapToGrid =====

    [Fact]
    public void SnapToGrid_ExactMultiple_ReturnsSame()
    {
        var result = Coordinate.SnapToGrid(10000, 5000);
        Assert.Equal(10000, result);
    }

    [Fact]
    public void SnapToGrid_Halfway_RoundsUp()
    {
        var result = Coordinate.SnapToGrid(7500, 5000);
        Assert.Equal(10000, result);
    }

    [Fact]
    public void SnapToGrid_BelowHalfway_RoundsDown()
    {
        var result = Coordinate.SnapToGrid(7400, 5000);
        Assert.Equal(5000, result);
    }

    [Fact]
    public void SnapToGrid_Zero_ReturnsZero()
    {
        var result = Coordinate.SnapToGrid(0, 5000);
        Assert.Equal(0, result);
    }

    [Fact]
    public void SnapToGrid_Negative_RoundsCorrectly()
    {
        // -2600 + 2500 = -100 / 5000 = 0 → 0
        var result = Coordinate.SnapToGrid(-2600, 5000);
        Assert.Equal(0, result);
        // -7600 + 2500 = -5100 / 5000 = -1 → -5000
        var result2 = Coordinate.SnapToGrid(-7600, 5000);
        Assert.Equal(-5000, result2);
    }

    // ===== FormatMm =====

    [Fact]
    public void FormatMm_WholeNumber_NoTrailingZeros()
    {
        var result = Coordinate.FormatMm(5000);
        Assert.Equal("5", result);
    }

    [Fact]
    public void FormatMm_Fractional_UpToThreeDecimals()
    {
        var result = Coordinate.FormatMm(3140);
        Assert.Equal("3.14", result);
    }

    [Fact]
    public void FormatMm_Zero_ReturnsZero()
    {
        var result = Coordinate.FormatMm(0);
        Assert.Equal("0", result);
    }

    // ===== SerializeMicrons / DeserializeMicrons =====

    [Fact]
    public void SerializeMicrons_Positive_ReturnsString()
    {
        var result = Coordinate.SerializeMicrons(420000);
        Assert.Equal("420000", result);
    }

    [Fact]
    public void SerializeMicrons_Negative_ReturnsString()
    {
        var result = Coordinate.SerializeMicrons(-1000);
        Assert.Equal("-1000", result);
    }

    [Fact]
    public void DeserializeMicrons_ValidString_ReturnsLong()
    {
        var result = Coordinate.DeserializeMicrons("420000");
        Assert.Equal(420000L, result);
    }

    [Fact]
    public void RoundTrip_SerializeDeserialize_NoLoss()
    {
        var original = 297000L;
        var serialized = Coordinate.SerializeMicrons(original);
        var deserialized = Coordinate.DeserializeMicrons(serialized);
        Assert.Equal(original, deserialized);
    }
}
