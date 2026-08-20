using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Tests.Helpers;

public class SnapHelperTests
{
    [Fact]
    public void SnapToGrid_Point_ExactMultiple_ReturnsSame()
    {
        var point = new PointMicrons(10000, 15000);
        var snapped = SnapHelper.SnapToGrid(point, 5000);
        Assert.Equal(10000, snapped.MicronsX);
        Assert.Equal(15000, snapped.MicronsY);
    }

    [Fact]
    public void SnapToGrid_Point_SnapsBothCoordinates()
    {
        var point = new PointMicrons(7500, 7400);
        var snapped = SnapHelper.SnapToGrid(point, 5000);
        Assert.Equal(10000, snapped.MicronsX);
        Assert.Equal(5000, snapped.MicronsY);
    }

    [Fact]
    public void SnapX_SnapsCorrectly()
    {
        Assert.Equal(10000, SnapHelper.SnapX(7500, 5000));
        Assert.Equal(5000, SnapHelper.SnapX(7400, 5000));
    }

    [Fact]
    public void SnapY_SnapsCorrectly()
    {
        Assert.Equal(10000, SnapHelper.SnapY(7500, 5000));
        Assert.Equal(5000, SnapHelper.SnapY(7400, 5000));
    }

    [Fact]
    public void SnapSize_PositiveValue_SnapsCorrectly()
    {
        Assert.Equal(10000, SnapHelper.SnapSize(7500, 5000));
        Assert.Equal(5000, SnapHelper.SnapSize(2600, 5000));
    }

    [Fact]
    public void SnapSize_NegativeValue_ReturnsZero()
    {
        Assert.Equal(0, SnapHelper.SnapSize(-1000, 5000));
    }

    [Fact]
    public void SnapSize_Zero_ReturnsZero()
    {
        Assert.Equal(0, SnapHelper.SnapSize(0, 5000));
    }

    [Fact]
    public void SnapObject_Line_MovesStartToEnd()
    {
        var line = new Line(7500, 7400, 12000, 12000);
        SnapHelper.SnapObject(line, 5000);
        Assert.Equal(10000, line.StartMicronsX);
        Assert.Equal(5000, line.StartMicronsY);
        // End = старый End + дельта: 12000 + (10000-7500) = 14500, 12000 + (5000-7400) = 9600
        Assert.Equal(14500, line.EndMicronsX);
        Assert.Equal(9600, line.EndMicronsY);
    }

    [Fact]
    public void SnapObject_Rectangle_MovesPosition()
    {
        var rect = new Rectangle(7500, 7400, 1000, 1000);
        SnapHelper.SnapObject(rect, 5000);
        Assert.Equal(10000, rect.MicronsX);
        Assert.Equal(5000, rect.MicronsY);
    }

    // ===== Формула привязки (перенесено из тестов документной библиотеки) =====

    [Fact]
    public void SnapX_ExactMultiple_ReturnsSame()
    {
        var result = SnapHelper.SnapX(10000, 5000);
        Assert.Equal(10000, result);
    }

    [Fact]
    public void SnapX_Halfway_RoundsUp()
    {
        var result = SnapHelper.SnapX(7500, 5000);
        Assert.Equal(10000, result);
    }

    [Fact]
    public void SnapX_BelowHalfway_RoundsDown()
    {
        var result = SnapHelper.SnapX(7400, 5000);
        Assert.Equal(5000, result);
    }

    [Fact]
    public void SnapX_Zero_ReturnsZero()
    {
        var result = SnapHelper.SnapX(0, 5000);
        Assert.Equal(0, result);
    }

    [Fact]
    public void SnapX_Negative_RoundsCorrectly()
    {
        // -2600 + 2500 = -100 / 5000 = 0 → 0
        var result = SnapHelper.SnapX(-2600, 5000);
        Assert.Equal(0, result);
        // -7600 + 2500 = -5100 / 5000 = -1 → -5000
        var result2 = SnapHelper.SnapX(-7600, 5000);
        Assert.Equal(-5000, result2);
    }

    [Fact]
    public void SnapToGrid_SnapsBothCoordinates()
    {
        var point = new PointMicrons(7500, 7400);
        var snapped = SnapHelper.SnapToGrid(point, 5000);
        Assert.Equal(10000, snapped.MicronsX);
        Assert.Equal(5000, snapped.MicronsY);
    }

    // ===== Перенесено из PointMicronsExtendedTests (привязка к сетке — понятие редактора) =====

    [Fact]
    public void SnapToGrid_ReturnsSnappedPoint()
    {
        var point = new PointMicrons(7500, 12500);
        var snapped = SnapHelper.SnapToGrid(point, 5000);

        Assert.Equal(10000, snapped.MicronsX);
        Assert.True(snapped.MicronsY >= 10000 && snapped.MicronsY <= 15000);
    }

    // ===== Перенесено из AdditionalSnapHelperTests (приложение) =====

    [Theory]
    [InlineData(0, 0, 5000, 0, 0)]     // on grid
    [InlineData(1000, 5000, 5000, 0, 5000)] // between grid points, snaps to nearest
    [InlineData(2500, 5000, 5000, 5000, 5000)] // exactly middle, rounds up
    [InlineData(7000, 7000, 5000, 5000, 5000)] // rounds down
    public void SnapToGrid_SnapsToNearestGridPoint(long x, long y, long step, long expectedX, long expectedY)
    {
        var point = new PointMicrons(x, y);
        var snapped = SnapHelper.SnapToGrid(point, step);

        Assert.Equal(expectedX, snapped.MicronsX);
        Assert.Equal(expectedY, snapped.MicronsY);
    }

    [Fact]
    public void SnapToGrid_ZeroStep_ThrowsException()
    {
        var point = new PointMicrons(1234, 5678);
        Assert.Throws<ArgumentOutOfRangeException>(() => SnapHelper.SnapToGrid(point, 0));
    }

    [Fact]
    public void SnapToGrid_NegativeStep_ThrowsException()
    {
        var point = new PointMicrons(1234, 5678);
        Assert.Throws<ArgumentOutOfRangeException>(() => SnapHelper.SnapToGrid(point, -5000));
    }

    [Fact]
    public void SnapHelper_SnapPoint_AlignsToGrid()
    {
        var point = SnapHelper.SnapToGrid(new PointMicrons(12345, 67890), 5000);
        Assert.Equal(10000, point.MicronsX);
        Assert.Equal(70000, point.MicronsY);
    }

    [Fact]
    public void SnapHelper_SnapPoint_ZeroStep_ThrowsException()
    {
        var point = new PointMicrons(12345, 67890);
        Assert.Throws<ArgumentOutOfRangeException>(() => SnapHelper.SnapToGrid(point, 0));
    }
}
