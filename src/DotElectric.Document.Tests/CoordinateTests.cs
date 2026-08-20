
namespace DotElectric.Document.Tests;

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

    // ===== ParseMm — edge cases =====

    [Fact]
    public void ParseMm_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentException>(() => Coordinate.ParseMm(null!));
    }

    [Fact]
    public void ParseMm_EmptyInput_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Coordinate.ParseMm(string.Empty));
    }

    [Fact]
    public void ParseMm_WhitespaceInput_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Coordinate.ParseMm("   "));
    }

    [Fact]
    public void ParseMm_InvalidFormat_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => Coordinate.ParseMm("abc"));
    }

    [Fact]
    public void ParseMm_ValidInput_ReturnsCorrectMicrons()
    {
        var result = Coordinate.ParseMm("42.5");
        Assert.Equal(42500, result);
    }

    [Fact]
    public void ParseMm_IntegerString_ReturnsCorrectMicrons()
    {
        var result = Coordinate.ParseMm("10");
        Assert.Equal(10000, result);
    }

    [Fact]
    public void ParseMm_NegativeString_ReturnsCorrectMicrons()
    {
        var result = Coordinate.ParseMm("-3.14");
        Assert.Equal(-3140, result);
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

    // ===== Перенесено из CoordinateExtendedTests (приложение) =====

    [Theory]
    [InlineData(0L)]
    [InlineData(1000L)]
    [InlineData(5500L)]
    [InlineData(-2500L)]
    public void FormatMm_FormatsValue(long microns)
    {
        var result = Coordinate.FormatMm(microns);
        Assert.NotEmpty(result);
        // Should be parseable back
        var parsed = Coordinate.ParseMm(result);
        Assert.Equal(microns, parsed);
    }

    [Theory]
    [InlineData("0.0", 0L)]
    [InlineData("1.0", 1000L)]
    [InlineData("5.5", 5500L)]
    [InlineData("-2.5", -2500L)]
    public void ParseMm_ParsesCorrectly(string input, long expected)
    {
        var result = Coordinate.ParseMm(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1.234, 1234L)]
    [InlineData(10.5, 10500L)]
    [InlineData(-0.5, -500L)]
    public void ToMicrons_FromMmValue_ReturnsMicrons(double mmValue, long expected)
    {
        var result = Coordinate.ToMicrons(mmValue);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToMicrons_ZeroDouble_ReturnsZero()
    {
        var result = Coordinate.ToMicrons(0.0);
        Assert.Equal(0L, result);
    }

    [Fact]
    public void SerializeMicrons_ReturnsString()
    {
        var result = Coordinate.SerializeMicrons(5000);
        Assert.Equal("5000", result);
    }

    [Fact]
    public void DeserializeMicrons_ParsesLong()
    {
        var result = Coordinate.DeserializeMicrons("5000");
        Assert.Equal(5000L, result);
    }

    [Fact]
    public void ToMm_ConvertsCorrectly()
    {
        Assert.Equal(1.0, Coordinate.ToMm(1000), 10);
        Assert.Equal(5.5, Coordinate.ToMm(5500), 10);
        Assert.Equal(0.001, Coordinate.ToMm(1), 10);
    }
}
