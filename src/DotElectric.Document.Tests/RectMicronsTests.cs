namespace DotElectric.Document.Tests;

public class RectMicronsTests
{
    // ===== Перенесено из SelectionBoxHelperTests (приложение) =====

    [Fact]
    public void RectMicrons_NormalizesCoordinates()
    {
        var rect = new RectMicrons(10000, 10000, 0, 0);
        Assert.Equal(0, rect.Left);
        Assert.Equal(0, rect.Bottom);
        Assert.Equal(10000, rect.Right);
        Assert.Equal(10000, rect.Top);
    }

    [Fact]
    public void RectMicrons_WidthHeight_CalculatedCorrectly()
    {
        var rect = new RectMicrons(0, 0, 10000, 5000);
        Assert.Equal(10000, rect.Width);
        Assert.Equal(5000, rect.Height);
    }

    [Fact]
    public void RectMicrons_FromPoints_CreatesCorrectRect()
    {
        var rect = RectMicrons.FromPoints(
            new PointMicrons(0, 0),
            new PointMicrons(10000, 5000));
        Assert.Equal(0, rect.Left);
        Assert.Equal(0, rect.Bottom);
        Assert.Equal(10000, rect.Right);
        Assert.Equal(5000, rect.Top);
    }

    [Fact]
    public void RectMicrons_Contains_FullContainment_ReturnsTrue()
    {
        var outer = new RectMicrons(0, 0, 10000, 10000);
        var inner = new RectMicrons(2000, 2000, 8000, 8000);
        Assert.True(outer.Contains(inner));
    }

    [Fact]
    public void RectMicrons_Contains_PartialOverlap_ReturnsFalse()
    {
        var a = new RectMicrons(0, 0, 5000, 5000);
        var b = new RectMicrons(3000, 3000, 8000, 8000);
        Assert.False(a.Contains(b));
    }

    [Fact]
    public void RectMicrons_Intersects_Overlap_ReturnsTrue()
    {
        var a = new RectMicrons(0, 0, 5000, 5000);
        var b = new RectMicrons(3000, 3000, 8000, 8000);
        Assert.True(a.Intersects(b));
    }

    [Fact]
    public void RectMicrons_Intersects_NoOverlap_ReturnsFalse()
    {
        var a = new RectMicrons(0, 0, 1000, 1000);
        var b = new RectMicrons(5000, 5000, 10000, 10000);
        Assert.False(a.Intersects(b));
    }

    // ===== Перенесено из ExtendedSelectionBoxHelperTests (приложение) =====

    [Fact]
    public void RectMicrons_NormalizesCoordinates_ReversedInput()
    {
        var rect = new RectMicrons(10000, 10000, 0, 0);
        Assert.Equal(0, rect.Left);
        Assert.Equal(0, rect.Bottom);
        Assert.Equal(10000, rect.Right);
        Assert.Equal(10000, rect.Top);
    }

    [Fact]
    public void RectMicrons_CalculatesWidthHeight()
    {
        var rect = new RectMicrons(0, 0, 5000, 3000);
        Assert.Equal(5000, rect.Width);
        Assert.Equal(3000, rect.Height);
    }

    [Fact]
    public void RectMicrons_FromPoints_NonZeroOrigin()
    {
        var start = new PointMicrons(1000, 1000);
        var end = new PointMicrons(5000, 4000);
        var rect = RectMicrons.FromPoints(start, end);

        Assert.Equal(1000, rect.Left);
        Assert.Equal(1000, rect.Bottom);
        Assert.Equal(5000, rect.Right);
        Assert.Equal(4000, rect.Top);
    }

    [Fact]
    public void RectMicrons_FromPoints_ReversedCoordinates()
    {
        var start = new PointMicrons(5000, 4000);
        var end = new PointMicrons(1000, 1000);
        var rect = RectMicrons.FromPoints(start, end);

        // RectMicrons normalizes: Left=min, Bottom=min, Right=max, Top=max
        Assert.Equal(1000, rect.Left);
        Assert.Equal(1000, rect.Bottom);
        Assert.Equal(5000, rect.Right);
        Assert.Equal(4000, rect.Top);
    }

    [Fact]
    public void RectMicrons_Intersects_Overlapping_ReturnsTrue()
    {
        var rect1 = new RectMicrons(0, 0, 5000, 5000);
        var rect2 = new RectMicrons(3000, 3000, 8000, 8000);

        Assert.True(rect1.Intersects(rect2));
    }

    [Fact]
    public void RectMicrons_Intersects_NonOverlapping_ReturnsFalse()
    {
        var rect1 = new RectMicrons(0, 0, 2000, 2000);
        var rect2 = new RectMicrons(5000, 5000, 8000, 8000);

        Assert.False(rect1.Intersects(rect2));
    }

    [Fact]
    public void RectMicrons_Intersects_TouchingEdges_ReturnsFalse()
    {
        var rect1 = new RectMicrons(0, 0, 2000, 2000);
        var rect2 = new RectMicrons(2000, 2000, 4000, 4000);

        Assert.False(rect1.Intersects(rect2)); // edges touching = no intersection
    }

    [Fact]
    public void RectMicrons_Contains_FullyInside_ReturnsTrue()
    {
        var outer = new RectMicrons(0, 0, 10000, 10000);
        var inner = new RectMicrons(2000, 2000, 5000, 5000);

        Assert.True(outer.Contains(inner));
    }

    [Fact]
    public void RectMicrons_Contains_PartiallyOutside_ReturnsFalse()
    {
        var outer = new RectMicrons(0, 0, 5000, 5000);
        var inner = new RectMicrons(3000, 3000, 8000, 8000);

        Assert.False(outer.Contains(inner));
    }
}
