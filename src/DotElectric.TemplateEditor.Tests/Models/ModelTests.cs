using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Tests.Models;

public class CoordinateExtendedTests
{
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
    public void ToMicrons_Zero_ReturnsZero()
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

public class PointMicronsExtendedTests
{
    [Fact]
    public void Constructor_SetsCoordinates()
    {
        var point = new PointMicrons(1000, 2000);
        Assert.Equal(1000, point.MicronsX);
        Assert.Equal(2000, point.MicronsY);
    }

    [Fact]
    public void FromMm_CreatesFromMillimeters()
    {
        var point = PointMicrons.FromMm(5.0, 3.5);
        Assert.Equal(5000, point.MicronsX);
        Assert.Equal(3500, point.MicronsY);
    }

    [Fact]
    public void FromMm_Negative_CreatesCorrectly()
    {
        var point = PointMicrons.FromMm(-1.0, -2.5);
        Assert.Equal(-1000, point.MicronsX);
        Assert.Equal(-2500, point.MicronsY);
    }

    [Fact]
    public void DistanceTo_CalculatesCorrectly()
    {
        var p1 = new PointMicrons(0, 0);
        var p2 = new PointMicrons(3000, 4000);

        var distance = p1.DistanceTo(p2);
        Assert.Equal(5000.0, distance, 1); // 3-4-5 triangle
    }

    [Fact]
    public void DistanceTo_SamePoint_ReturnsZero()
    {
        var p1 = new PointMicrons(1000, 2000);
        var p2 = new PointMicrons(1000, 2000);

        Assert.Equal(0.0, p1.DistanceTo(p2), 0.001);
    }

    [Fact]
    public void SnapToGrid_ReturnsSnappedPoint()
    {
        var point = new PointMicrons(7500, 12500);
        var snapped = point.SnapToGrid(5000);

        Assert.Equal(10000, snapped.MicronsX);
        Assert.True(snapped.MicronsY >= 10000 && snapped.MicronsY <= 15000);
    }

    [Fact]
    public void X_ReturnsMmValue()
    {
        var point = new PointMicrons(5000, 0);
        Assert.Equal(5.0, point.X, 0.001);
    }

    [Fact]
    public void Y_ReturnsMmValue()
    {
        var point = new PointMicrons(0, 3500);
        Assert.Equal(3.5, point.Y, 0.001);
    }
}
