using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;

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

    // ===== Перенесено из ExtendedSelectionBoxHelperTests (приложение) =====

    [Theory]
    [InlineData(0, 0, 5000, 5000, SelectionDirection.LeftToRight)]
    [InlineData(5000, 5000, 0, 0, SelectionDirection.RightToLeft)]
    [InlineData(0, 0, 0, 5000, SelectionDirection.LeftToRight)]
    [InlineData(5000, 0, 5000, 5000, SelectionDirection.LeftToRight)]
    public void GetDirection_CorrectDirection(
        int startX, int startY, int endX, int endY,
        SelectionDirection expected)
    {
        var start = new PointMicrons(startX, startY);
        var end = new PointMicrons(endX, endY);
        var direction = SelectionBoxHelper.GetDirection(start, end);
        Assert.Equal(expected, direction);
    }

    [Fact]
    public void GetFullyContained_OnlyFullyInside()
    {
        var box = new RectMicrons(0, 0, 10000, 10000);
        var objects = new List<TemplateObjectBase>
        {
            new Rectangle(1000, 1000, 2000, 2000),    // fully inside: bounds 1000-3000
            new Rectangle(5000, 5000, 2000, 2000),    // fully inside: bounds 5000-7000
            new Rectangle(8000, 8000, 5000, 5000),    // partially outside: bounds 8000-13000
        };

        var selected = SelectionBoxHelper.GetFullyContained(box, objects);
        Assert.Equal(2, selected.Count);
    }

    [Fact]
    public void GetIntersecting_AnyOverlap()
    {
        var box = new RectMicrons(0, 0, 5000, 5000);
        var objects = new List<TemplateObjectBase>
        {
            new Rectangle(1000, 1000, 2000, 2000),    // fully inside
            new Rectangle(4000, 4000, 3000, 3000),    // partially overlaps
            new Rectangle(8000, 8000, 2000, 2000),    // no overlap
        };

        var selected = SelectionBoxHelper.GetIntersecting(box, objects);
        Assert.Equal(2, selected.Count);
    }

    [Fact]
    public void GetSelectedObjects_LeftToRight_UsesFullContain()
    {
        var box = new RectMicrons(0, 0, 5000, 5000);
        var objects = new List<TemplateObjectBase>
        {
            new Rectangle(1000, 1000, 2000, 2000),    // fully inside
            new Rectangle(4000, 4000, 3000, 3000),    // partial
        };

        var selected = SelectionBoxHelper.GetSelectedObjects(
            box, objects, SelectionDirection.LeftToRight);

        Assert.Single(selected);
    }

    [Fact]
    public void GetSelectedObjects_RightToLeft_UsesIntersect()
    {
        var box = new RectMicrons(0, 0, 5000, 5000);
        var objects = new List<TemplateObjectBase>
        {
            new Rectangle(1000, 1000, 2000, 2000),    // fully inside
            new Rectangle(4000, 4000, 3000, 3000),    // partial
        };

        var selected = SelectionBoxHelper.GetSelectedObjects(
            box, objects, SelectionDirection.RightToLeft);

        Assert.Equal(2, selected.Count);
    }

    [Fact]
    public void GetObjectBounds_Line_ReturnsCorrectBounds()
    {
        var line = new Line(0, 0, 10000, 5000);
        var objects = new List<TemplateObjectBase> { line };
        var box = new RectMicrons(0, 0, 10000, 5000);

        var selected = SelectionBoxHelper.GetFullyContained(box, objects);
        Assert.Single(selected);
    }

    [Fact]
    public void GetObjectBounds_Text_CalculatesWidthFromContent()
    {
        var text = new Text(0, 0, "Hello", 5000); // width = 5 * 5000 * 0.6 = 15000
        var objects = new List<TemplateObjectBase> { text };
        var box = new RectMicrons(0, 0, 20000, 10000);

        var selected = SelectionBoxHelper.GetFullyContained(box, objects);
        Assert.Single(selected);
    }

    [Fact]
    public void GetObjectBounds_Text_Rotated90Degrees()
    {
        var text = new Text(0, 0, "Hi", 5000, rotationAngle: 90);
        var objects = new List<TemplateObjectBase> { text };
        // 90°: X stays, Y + width, X + height, Y + width + height
        var box = new RectMicrons(-5000, -6000, 10000, 10000);

        var selected = SelectionBoxHelper.GetIntersecting(box, objects);
        Assert.Single(selected);
    }
}
