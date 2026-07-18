using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Tests.Helpers;

public class ValidationErrorTests
{
    [Fact]
    public void Constructor_Default_SetsDefaults()
    {
        var error = new ValidationError("V-001", "Test message");

        Assert.Equal("V-001", error.RuleId);
        Assert.Equal("Test message", error.Message);
        Assert.Null(error.ObjectId);
        Assert.Equal(ValidationSeverity.Error, error.Severity);
    }

    [Fact]
    public void Constructor_WithObjectId_SetsObjectId()
    {
        var error = new ValidationError("V-003", "Out of bounds", objectId: "obj-123");

        Assert.Equal("obj-123", error.ObjectId);
    }

    [Fact]
    public void Constructor_WithSeverity_SetsSeverity()
    {
        var error = new ValidationError("V-004", "Warning", severity: ValidationSeverity.Warning);

        Assert.Equal(ValidationSeverity.Warning, error.Severity);
    }

    [Fact]
    public void Properties_CanBeRead()
    {
        var error = new ValidationError(
            "V-007",
            "Invalid line type",
            objectId: "line-456",
            severity: ValidationSeverity.Warning);

        Assert.Equal("V-007", error.RuleId);
        Assert.Equal("Invalid line type", error.Message);
        Assert.Equal("line-456", error.ObjectId);
        Assert.Equal(ValidationSeverity.Warning, error.Severity);
    }
}

public class SnapHelperAdditionalTests
{
    [Theory]
    [InlineData(0, 5000, 0)]       // exactly on grid
    [InlineData(1000, 5000, 0)]    // closer to 0
    [InlineData(2500, 5000, 5000)] // exactly middle, rounds up
    [InlineData(4000, 5000, 5000)] // closer to 5000
    [InlineData(4999, 5000, 5000)] // just below 5000
    [InlineData(5000, 5000, 5000)] // exactly 5000
    [InlineData(5001, 5000, 5000)] // just above 5000
    public void SnapToGrid_SnapsToNearest(long value, long step, long expected)
    {
        var point = new PointMicrons(value, 0);
        var snapped = point.SnapToGrid(step);
        Assert.Equal(expected, snapped.MicronsX);
    }

    [Fact]
    public void SnapToGrid_NegativeCoordinates_SnapsCorrectly()
    {
        var point = new PointMicrons(-1000, -3000);
        var snapped = point.SnapToGrid(5000);
        Assert.Equal(0, snapped.MicronsX);
        Assert.True(snapped.MicronsY <= 0);
    }

    [Fact]
    public void SnapHelper_SnapToGrid_ReturnsSnappedPoint()
    {
        var point = new PointMicrons(7500, 12500);
        var snapped = SnapHelper.SnapToGrid(point, 5000);
        Assert.Equal(10000, snapped.MicronsX);
        Assert.True(snapped.MicronsY >= 10000 && snapped.MicronsY <= 15000);
    }

    [Fact]
    public void SnapHelper_SnapToGrid_ThrowsOnZeroStep()
    {
        var point = new PointMicrons(1000, 2000);
        Assert.Throws<ArgumentOutOfRangeException>(() => SnapHelper.SnapToGrid(point, 0));
    }

    [Fact]
    public void SnapHelper_SnapToGrid_ThrowsOnNegativeStep()
    {
        var point = new PointMicrons(1000, 2000);
        Assert.Throws<ArgumentOutOfRangeException>(() => SnapHelper.SnapToGrid(point, -5000));
    }

    [Fact]
    public void SnapHelper_SnapToGrid_ExactGridPoint_ReturnsSame()
    {
        var point = new PointMicrons(10000, 15000);
        var snapped = SnapHelper.SnapToGrid(point, 5000);
        Assert.Equal(10000, snapped.MicronsX);
        Assert.Equal(15000, snapped.MicronsY);
    }

    [Fact]
    public void SnapHelper_SnapToGrid_LargeStep_SnapsCorrectly()
    {
        var point = new PointMicrons(7000, 13000);
        var snapped = SnapHelper.SnapToGrid(point, 10000);
        Assert.Equal(10000, snapped.MicronsX);
        Assert.Equal(10000, snapped.MicronsY);
    }
}
