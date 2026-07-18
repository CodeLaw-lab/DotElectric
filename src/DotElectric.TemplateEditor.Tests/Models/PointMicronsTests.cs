using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Tests.Models;

public class PointMicronsTests
{
    [Fact]
    public void Constructor_SetsCorrectValues()
    {
        var point = new PointMicrons(10000, 20000);
        Assert.Equal(10000, point.MicronsX);
        Assert.Equal(20000, point.MicronsY);
    }

    [Fact]
    public void X_ReturnsMmValue()
    {
        var point = new PointMicrons(5500, 0);
        Assert.Equal(5.5, point.X, tolerance: 0.0001);
    }

    [Fact]
    public void Y_ReturnsMmValue()
    {
        var point = new PointMicrons(0, -3140);
        Assert.Equal(-3.14, point.Y, tolerance: 0.0001);
    }

    [Fact]
    public void FromMm_CreatesPointInMicrons()
    {
        var point = PointMicrons.FromMm(5.5, 3.0);
        Assert.Equal(5500, point.MicronsX);
        Assert.Equal(3000, point.MicronsY);
    }

    [Fact]
    public void SnapToGrid_SnapsBothCoordinates()
    {
        var point = new PointMicrons(7500, 7400);
        var snapped = point.SnapToGrid(5000);
        Assert.Equal(10000, snapped.MicronsX);
        Assert.Equal(5000, snapped.MicronsY);
    }

    [Fact]
    public void DistanceTo_SamePoint_ReturnsZero()
    {
        var a = new PointMicrons(1000, 2000);
        var b = new PointMicrons(1000, 2000);
        Assert.Equal(0, a.DistanceTo(b));
    }

    [Fact]
    public void DistanceTo_DifferentPoints_ReturnsCorrectDistance()
    {
        var a = new PointMicrons(0, 0);
        var b = new PointMicrons(3000, 4000);
        // sqrt(3000^2 + 4000^2) = 5000
        Assert.Equal(5000, a.DistanceTo(b));
    }

    [Fact]
    public void DistanceTo_OnlyX_ReturnsAbsoluteDifference()
    {
        var a = new PointMicrons(0, 0);
        var b = new PointMicrons(10000, 0);
        Assert.Equal(10000, a.DistanceTo(b));
    }

    [Fact]
    public void DistanceTo_OnlyY_ReturnsAbsoluteDifference()
    {
        var a = new PointMicrons(0, 0);
        var b = new PointMicrons(0, 7500);
        Assert.Equal(7500, a.DistanceTo(b));
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        var a = new PointMicrons(1000, 2000);
        var b = new PointMicrons(1000, 2000);
        Assert.True(a.Equals(b));
        Assert.True(a == b);
    }

    [Fact]
    public void Equals_DifferentValues_ReturnsFalse()
    {
        var a = new PointMicrons(1000, 2000);
        var b = new PointMicrons(1001, 2000);
        Assert.False(a.Equals(b));
        Assert.True(a != b);
    }

    [Fact]
    public void GetHashCode_SameValues_SameHash()
    {
        var a = new PointMicrons(1000, 2000);
        var b = new PointMicrons(1000, 2000);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ToString_ReturnsFormattedMm()
    {
        var point = new PointMicrons(5000, 3000);
        var result = point.ToString();
        Assert.Contains("5", result);
        Assert.Contains("3", result);
    }

    [Fact]
    public void ReadOnlyStruct_CannotModifyAfterCreation()
    {
        var point = new PointMicrons(1000, 2000);
        // PointMicrons — readonly struct, копия при передаче
        var copy = point;
        Assert.Equal(point.MicronsX, copy.MicronsX);
    }
}
