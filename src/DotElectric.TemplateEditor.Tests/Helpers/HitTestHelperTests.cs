using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Tests.Helpers;

public class HitTestHelperTests
{
    // ===== HitTest (top-level) =====

    [Fact]
    public void HitTest_NoObjects_ReturnsNull()
    {
        var objects = new List<TemplateObjectBase>();
        var point = new PointMicrons(0, 0);
        Assert.Null(HitTestHelper.HitTest(point, objects));
    }

    [Fact]
    public void HitTest_PointNotOnAnyObject_ReturnsNull()
    {
        var objects = new List<TemplateObjectBase>
        {
            new Rectangle(0, 0, 1000, 1000),
            new Line(5000, 5000, 10000, 10000)
        };
        var point = new PointMicrons(20000, 20000);
        Assert.Null(HitTestHelper.HitTest(point, objects));
    }

    [Fact]
    public void HitTest_FindsTopObject()
    {
        var rect = new Rectangle(0, 0, 10000, 10000);
        var line = new Line(5000, 5000, 8000, 8000);
        var objects = new List<TemplateObjectBase> { rect, line };
        var point = new PointMicrons(6000, 6000);

        var result = HitTestHelper.HitTest(point, objects);

        // line добавлен последним = верхний
        Assert.Same(line, result);
    }

    // ===== Extended: HitTest (main method) =====

    [Fact]
    public void HitTest_ReturnsTopmostObject_WhenMultipleOverlap()
    {
        var rect1 = new Rectangle(0, 0, 10000, 10000);
        var rect2 = new Rectangle(0, 0, 5000, 5000);
        var objects = new List<TemplateObjectBase> { rect1, rect2 };
        var point = new PointMicrons(2000, 2000);

        var result = HitTestHelper.HitTest(point, objects);

        Assert.Same(rect2, result);
    }

    [Fact]
    public void HitTest_ReturnsNull_ForEmptyList()
    {
        var objects = new List<TemplateObjectBase>();
        var point = new PointMicrons(0, 0);

        var result = HitTestHelper.HitTest(point, objects);

        Assert.Null(result);
    }

    [Fact]
    public void HitTest_ReturnsNull_WhenNoObjectsUnderPoint()
    {
        var objects = new List<TemplateObjectBase>
        {
            new Rectangle(0, 0, 1000, 1000),
        };
        var point = new PointMicrons(7000, 7000); // outside expanded bounds (tol=5000)

        var result = HitTestHelper.HitTest(point, objects);

        Assert.Null(result);
    }

    [Fact]
    public void HitTest_ReturnsCorrectType_WhenPointIsOnBoundary()
    {
        var rect = new Rectangle(0, 0, 10000, 10000);
        var objects = new List<TemplateObjectBase> { rect };
        var point = new PointMicrons(0, 0);

        var result = HitTestHelper.HitTest(point, objects);

        Assert.Same(rect, result);
    }

    [Fact]
    public void HitTest_ReturnsLine_WhenPointIsOnLine()
    {
        var line = new Line(0, 0, 10000, 0);
        var objects = new List<TemplateObjectBase> { line };
        var point = new PointMicrons(5000, 0);

        var result = HitTestHelper.HitTest(point, objects);

        Assert.Same(line, result);
    }

    [Fact]
    public void HitTest_ReturnsText_WhenPointIsInsideText()
    {
        var text = new Text(0, 0, "Test", 5000);
        var objects = new List<TemplateObjectBase> { text };
        var point = new PointMicrons(3000, 2000);

        var result = HitTestHelper.HitTest(point, objects);

        Assert.Same(text, result);
    }
}
