using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Tests.Helpers;

public class SelectionBoxHelperTests
{
    // ===== GetDirection =====

    [Fact]
    public void GetDirection_LeftToRight_StartXLessThanEndX()
    {
        var start = new PointMicrons(0, 0);
        var end = new PointMicrons(10000, 10000);
        Assert.Equal(SelectionDirection.LeftToRight, SelectionBoxHelper.GetDirection(start, end));
    }

    [Fact]
    public void GetDirection_RightToLeft_StartXGreaterThanEndX()
    {
        var start = new PointMicrons(10000, 0);
        var end = new PointMicrons(0, 10000);
        Assert.Equal(SelectionDirection.RightToLeft, SelectionBoxHelper.GetDirection(start, end));
    }

    [Fact]
    public void GetDirection_EqualX_ReturnsLeftToRight()
    {
        var start = new PointMicrons(5000, 0);
        var end = new PointMicrons(5000, 10000);
        Assert.Equal(SelectionDirection.LeftToRight, SelectionBoxHelper.GetDirection(start, end));
    }

    // ===== RectMicrons =====

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

    // ===== GetFullyContained (LeftToRight) =====

    [Fact]
    public void GetFullyContained_AllInside_ReturnsAll()
    {
        var box = new RectMicrons(0, 0, 20000, 20000);
        var objects = new List<TemplateObjectBase>
        {
            new Rectangle(1000, 1000, 5000, 5000),
            new Rectangle(10000, 10000, 5000, 5000)
        };

        var result = SelectionBoxHelper.GetFullyContained(box, objects);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetFullyContained_PartiallyOutside_ReturnsOnlyContained()
    {
        var box = new RectMicrons(0, 0, 10000, 10000);
        var inside = new Rectangle(1000, 1000, 5000, 5000);
        var outside = new Rectangle(8000, 8000, 5000, 5000); // выходит за box
        var objects = new List<TemplateObjectBase> { inside, outside };

        var result = SelectionBoxHelper.GetFullyContained(box, objects);

        Assert.Single(result);
        Assert.Same(inside, result[0]);
    }

    [Fact]
    public void GetFullyContained_EmptyBox_ReturnsEmpty()
    {
        var box = new RectMicrons(0, 0, 1, 1);
        var objects = new List<TemplateObjectBase> { new Rectangle(5000, 5000, 1000, 1000) };
        var result = SelectionBoxHelper.GetFullyContained(box, objects);
        Assert.Empty(result);
    }

    // ===== GetIntersecting (RightToLeft) =====

    [Fact]
    public void GetIntersecting_AnyOverlap_ReturnsAll()
    {
        var box = new RectMicrons(0, 0, 10000, 10000);
        var inside = new Rectangle(1000, 1000, 5000, 5000);
        var overlapping = new Rectangle(8000, 8000, 5000, 5000);
        var objects = new List<TemplateObjectBase> { inside, overlapping };

        var result = SelectionBoxHelper.GetIntersecting(box, objects);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void GetIntersecting_NoOverlap_ReturnsEmpty()
    {
        var box = new RectMicrons(0, 0, 1000, 1000);
        var objects = new List<TemplateObjectBase> { new Rectangle(5000, 5000, 1000, 1000) };
        var result = SelectionBoxHelper.GetIntersecting(box, objects);
        Assert.Empty(result);
    }

    // ===== GetSelectedObjects =====

    [Fact]
    public void GetSelectedObjects_LeftToRight_UsesFullContainment()
    {
        var box = new RectMicrons(0, 0, 10000, 10000);
        var inside = new Rectangle(1000, 1000, 5000, 5000);
        var partial = new Rectangle(8000, 8000, 5000, 5000);
        var objects = new List<TemplateObjectBase> { inside, partial };

        var result = SelectionBoxHelper.GetSelectedObjects(box, objects, SelectionDirection.LeftToRight);

        Assert.Single(result);
        Assert.Same(inside, result[0]);
    }

    [Fact]
    public void GetSelectedObjects_RightToLeft_UsesIntersection()
    {
        var box = new RectMicrons(0, 0, 10000, 10000);
        var inside = new Rectangle(1000, 1000, 5000, 5000);
        var partial = new Rectangle(8000, 8000, 5000, 5000);
        var objects = new List<TemplateObjectBase> { inside, partial };

        var result = SelectionBoxHelper.GetSelectedObjects(box, objects, SelectionDirection.RightToLeft);

        Assert.Equal(2, result.Count);
    }

    // ===== Line bounding box =====

    [Fact]
    public void GetSelectedObjects_LineFullyContained_ReturnsLine()
    {
        var box = new RectMicrons(0, 0, 10000, 10000);
        var line = new Line(1000, 1000, 8000, 8000);
        var objects = new List<TemplateObjectBase> { line };

        var result = SelectionBoxHelper.GetFullyContained(box, objects);

        Assert.Single(result);
        Assert.Same(line, result[0]);
    }
}
