using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.Tools;

namespace DotElectric.TemplateEditor.Tests.Helpers;

[Collection("FontMetrics")]
public class HitTestHelperTests : IDisposable
{
    public HitTestHelperTests()
    {
        FontMetrics.Default.Reset();
    }

    public void Dispose()
    {
        FontMetrics.Default.Reset();
    }

    // ===== DistanceFromPointToLine =====

    [Fact]
    public void DistanceFromPointToLine_PointOnStart_ReturnsZero()
    {
        var start = new PointMicrons(0, 0);
        var end = new PointMicrons(10000, 0);
        var point = new PointMicrons(0, 0);
        Assert.Equal(0, HitTestHelper.DistanceFromPointToLine(point, start, end));
    }

    [Fact]
    public void DistanceFromPointToLine_PointOnEnd_ReturnsZero()
    {
        var start = new PointMicrons(0, 0);
        var end = new PointMicrons(10000, 0);
        var point = new PointMicrons(10000, 0);
        Assert.Equal(0, HitTestHelper.DistanceFromPointToLine(point, start, end));
    }

    [Fact]
    public void DistanceFromPointToLine_PointOnLine_ReturnsZero()
    {
        var start = new PointMicrons(0, 0);
        var end = new PointMicrons(10000, 0);
        var point = new PointMicrons(5000, 0);
        Assert.Equal(0, HitTestHelper.DistanceFromPointToLine(point, start, end));
    }

    [Fact]
    public void DistanceFromPointToLine_PointAboveLine_ReturnsCorrectDistance()
    {
        var start = new PointMicrons(0, 0);
        var end = new PointMicrons(10000, 0);
        var point = new PointMicrons(5000, 3000);
        Assert.Equal(3000, HitTestHelper.DistanceFromPointToLine(point, start, end));
    }

    [Fact]
    public void DistanceFromPointToLine_PointBeyondStart_ReturnsDistanceToStart()
    {
        var start = new PointMicrons(0, 0);
        var end = new PointMicrons(10000, 0);
        var point = new PointMicrons(-5000, 0);
        Assert.Equal(5000, HitTestHelper.DistanceFromPointToLine(point, start, end));
    }

    [Fact]
    public void DistanceFromPointToLine_PointBeyondEnd_ReturnsDistanceToEnd()
    {
        var start = new PointMicrons(0, 0);
        var end = new PointMicrons(10000, 0);
        var point = new PointMicrons(15000, 0);
        Assert.Equal(5000, HitTestHelper.DistanceFromPointToLine(point, start, end));
    }

    [Fact]
    public void DistanceFromPointToLine_ZeroLengthLine_ReturnsDistanceToStart()
    {
        var start = new PointMicrons(5000, 5000);
        var end = new PointMicrons(5000, 5000);
        var point = new PointMicrons(8000, 9000);
        // sqrt(3000^2 + 4000^2) = 5000
        Assert.Equal(5000, HitTestHelper.DistanceFromPointToLine(point, start, end));
    }

    [Fact]
    public void DistanceFromPointToLine_DiagonalLine_PointOnLine_ReturnsZero()
    {
        var start = new PointMicrons(0, 0);
        var end = new PointMicrons(10000, 10000);
        var point = new PointMicrons(5000, 5000);
        Assert.Equal(0, HitTestHelper.DistanceFromPointToLine(point, start, end));
    }

    // ===== HitTestLine =====

    [Fact]
    public void HitTestLine_PointWithinTolerance_ReturnsTrue()
    {
        var line = new Line(0, 0, 10000, 0);
        var point = new PointMicrons(5000, 4000); // 4мм < 5мм tolerance
        Assert.True(HitTestHelper.HitTestLine(line, point));
    }

    [Fact]
    public void HitTestLine_PointOutsideTolerance_ReturnsFalse()
    {
        var line = new Line(0, 0, 10000, 0);
        var point = new PointMicrons(5000, 6000); // 6мм > 5мм tolerance
        Assert.False(HitTestHelper.HitTestLine(line, point));
    }

    // ===== HitTestRectangle =====

    [Fact]
    public void HitTestRectangle_PointInsideLargeRect_ReturnsFalse()
    {
        var rect = new Rectangle(0, 0, 200000, 100000);
        var point = new PointMicrons(100000, 50000); // center, far from edges
        Assert.False(HitTestHelper.HitTestRectangle(rect, point));
    }

    [Fact]
    public void HitTestRectangle_PointNearEdgeLargeRect_ReturnsTrue()
    {
        var rect = new Rectangle(0, 0, 200000, 100000);
        var point = new PointMicrons(100000, 3000); // 3mm from bottom edge, within tolerance
        Assert.True(HitTestHelper.HitTestRectangle(rect, point));
    }

    [Fact]
    public void HitTestRectangle_PointOnEdge_ReturnsTrue()
    {
        var rect = new Rectangle(0, 0, 10000, 5000);
        var point = new PointMicrons(0, 0);
        Assert.True(HitTestHelper.HitTestRectangle(rect, point));
    }

    [Fact]
    public void HitTestRectangle_PointOutside_ReturnsFalse()
    {
        var rect = new Rectangle(0, 0, 10000, 5000);
        var point = new PointMicrons(16000, 11000); // outside expanded bounds (tol=5000)
        Assert.False(HitTestHelper.HitTestRectangle(rect, point));
    }

    [Fact]
    public void HitTestRectangle_PointNegativeCoordinates_ReturnsFalse()
    {
        var rect = new Rectangle(0, 0, 10000, 5000);
        var point = new PointMicrons(-6000, -6000); // outside expanded bounds (tol=5000)
        Assert.False(HitTestHelper.HitTestRectangle(rect, point));
    }

    // ===== HitTestText =====

    [Fact]
    public void HitTestText_PointInside_ReturnsTrue()
    {
        var text = new Text(0, 0, "Test", 3500);
        var point = new PointMicrons(3000, 1000); // внутри bounding box
        Assert.True(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_PointOutside_ReturnsFalse()
    {
        var text = new Text(0, 0, "Test", 3500);
        var point = new PointMicrons(20000, 20000);
        Assert.False(HitTestHelper.HitTestText(text, point));
    }

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

    [Fact]
    public void HitTestAll_FindsAllObjectsAtPoint()
    {
        var rect = new Rectangle(0, 0, 10000, 10000);
        var line = new Line(5000, 5000, 8000, 8000);
        var objects = new List<TemplateObjectBase> { rect, line };
        var point = new PointMicrons(6000, 6000);

        var results = HitTestHelper.HitTestAll(point, objects);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void HitTestObject_Line_ReturnsTrue()
    {
        var line = new Line(0, 0, 10000, 0);
        var point = new PointMicrons(5000, 1000);
        Assert.True(HitTestHelper.HitTestObject(line, point));
    }

    [Fact]
    public void HitTestObject_Rectangle_ReturnsTrue()
    {
        var rect = new Rectangle(0, 0, 10000, 5000);
        var point = new PointMicrons(5000, 2500);
        Assert.True(HitTestHelper.HitTestObject(rect, point));
    }

    [Fact]
    public void HitTestObject_Text_ReturnsTrue()
    {
        var text = new Text(0, 0, "Test", 3500);
        var point = new PointMicrons(3000, 1000);
        Assert.True(HitTestHelper.HitTestObject(text, point));
    }

    // ===== HitTestText — расширенные тесты =====

    [Fact]
    public void HitTestText_NoRotation_PointInside_ReturnsTrue()
    {
        var text = new Text(0, 0, "Test", 3500) { RotationAngle = 0 };
        // Ширина ≈ 4 * 3500 * 0.6 = 8400 микрон
        var point = new PointMicrons(4000, 1000);
        Assert.True(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_NoRotation_PointOutside_ReturnsFalse()
    {
        var text = new Text(0, 0, "Test", 3500) { RotationAngle = 0 };
        var point = new PointMicrons(10000, 1000); // За пределами текста
        Assert.False(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_NoRotation_PointOnEdge_ReturnsTrue()
    {
        var text = new Text(0, 0, "Test", 3500) { RotationAngle = 0 };
        var point = new PointMicrons(0, 0); // Левый нижний угол
        Assert.True(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_Rotation90_PointInside_ReturnsTrue()
    {
        var text = new Text(0, 0, "Test", 3500) { RotationAngle = 90 };
        // With LayoutTransform offset at 90°: center = (-minX, H+minY) = (H, H)
        // Visual center maps to (H/2, 0) in model space (W=7000, H=3500 → (1750, 0))
        var point = new PointMicrons(1750, 0);
        Assert.True(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_Rotation90_PointOutside_ReturnsFalse()
    {
        var text = new Text(0, 0, "Test", 3500) { RotationAngle = 90 };
        var point = new PointMicrons(5000, 1000); // За пределами
        Assert.False(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_Rotation180_PointInside_ReturnsTrue()
    {
        var text = new Text(10000, 10000, "Test", 3500) { RotationAngle = 180 };
        // With LayoutTransform offset at 180°: center = (X-W, Y) for W=7000, H=3500
        // Visual center maps to (X-W/2, Y+H/2) = (17000-3500, 10000+1750) = (13500, 11750)
        var point = new PointMicrons(13500, 11750);
        Assert.True(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_Rotation180_PointOutside_ReturnsFalse()
    {
        var text = new Text(10000, 10000, "Test", 3500) { RotationAngle = 180 };
        var point = new PointMicrons(15000, 9000); // Справа от опорной точки
        Assert.False(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_Rotation270_PointInside_ReturnsTrue()
    {
        var text = new Text(10000, 10000, "Test", 3500) { RotationAngle = 270 };
        // With LayoutTransform offset at 270°: center = (X, Y+H-W) for W=7000, H=3500
        // = (10000, 6500). Visual center maps to (X+H/2, Y+H/2) = (11750, 10000)
        var point = new PointMicrons(11750, 10000);
        Assert.True(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_Rotation270_PointOutside_ReturnsFalse()
    {
        var text = new Text(10000, 10000, "Test", 3500) { RotationAngle = 270 };
        var point = new PointMicrons(15000, 15000); // За пределами
        Assert.False(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_EmptyContent_ZeroWidth()
    {
        var text = new Text(0, 0, "", 3500) { RotationAngle = 0 };
        var point = new PointMicrons(0, 0);
        // Пустой текст имеет нулевую ширину
        Assert.True(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_LongText_LargerWidth()
    {
        var text = new Text(0, 0, "Very Long Text", 3500) { RotationAngle = 0 };
        // Ширина ≈ 14 * 3500 * 0.6 = 29400 микрон
        var pointInside = new PointMicrons(15000, 1000);
        var pointOutside = new PointMicrons(30000, 1000);
        
        Assert.True(HitTestHelper.HitTestText(text, pointInside));
        Assert.False(HitTestHelper.HitTestText(text, pointOutside));
    }

    [Fact]
    public void HitTestText_LargeFontSize_LargerBox()
    {
        var text = new Text(0, 0, "Test", 7000) { RotationAngle = 0 };
        // Высота = 7000 микрон
        var pointInside = new PointMicrons(5000, 3000);
        var pointOutside = new PointMicrons(5000, 8000); // Выше текста
        
        Assert.True(HitTestHelper.HitTestText(text, pointInside));
        Assert.False(HitTestHelper.HitTestText(text, pointOutside));
    }

    [Fact]
    public void HitTestText_BoundaryConditions_EdgeHits()
    {
        var text = new Text(5000, 5000, "Test", 3500) { RotationAngle = 0 };
        
        // "Test"=4 chars, font "ГОСТ А" → factor 0.5 → Width=4*3500*0.5=7000
        var topLeft = new PointMicrons(5000, 5000 + 3500);
        var bottomRight = new PointMicrons(5000 + 7000, 5000);
        
        Assert.True(HitTestHelper.HitTestText(text, topLeft));
        Assert.True(HitTestHelper.HitTestText(text, bottomRight));
    }

    // ===== GetHitHandle Tests =====

    [Fact]
    public void GetHitHandle_Line_StartHandle()
    {
        var line = new Line(0, 0, 20000, 20000);
        var point = new PointMicrons(0, 0); // На начале линии

        var handle = HitTestHelper.GetHitHandle(point, line);

        Assert.Equal(ResizeHandle.TopLeft, handle);
    }

    [Fact]
    public void GetHitHandle_Line_EndHandle()
    {
        var line = new Line(0, 0, 20000, 20000);
        var point = new PointMicrons(20000, 20000); // На конце линии

        var handle = HitTestHelper.GetHitHandle(point, line);

        Assert.Equal(ResizeHandle.BottomRight, handle);
    }

    [Fact]
    public void GetHitHandle_Line_NoHandle()
    {
        var line = new Line(0, 0, 20000, 20000);
        var point = new PointMicrons(10000, 10000); // В середине линии, не на маркере

        var handle = HitTestHelper.GetHitHandle(point, line);

        Assert.Null(handle);
    }

    [Fact]
    public void GetHitHandle_Rectangle_TopLeftHandle()
    {
        var rect = new Rectangle(0, 0, 20000, 20000);
        var point = new PointMicrons(0, 20000); // Верхний левый угол (BottomMicronsY = 0 + 20000)

        var handle = HitTestHelper.GetHitHandle(point, rect);

        Assert.Equal(ResizeHandle.TopLeft, handle);
    }

    [Fact]
    public void GetHitHandle_Rectangle_BottomRightHandle()
    {
        var rect = new Rectangle(0, 0, 20000, 20000);
        var point = new PointMicrons(20000, 0); // Нижний правый угол

        var handle = HitTestHelper.GetHitHandle(point, rect);

        Assert.Equal(ResizeHandle.BottomRight, handle);
    }

    [Fact]
    public void GetHitHandle_Rectangle_NoHandle()
    {
        var rect = new Rectangle(0, 0, 20000, 20000);
        var point = new PointMicrons(10000, 10000); // В центре прямоугольника

        var handle = HitTestHelper.GetHitHandle(point, rect);

        Assert.Null(handle);
    }

    [Fact]
    public void GetHitHandle_Text_TopLeftHandle()
    {
        var text = new Text(0, 0, "Test", 3500);
        var point = new PointMicrons(0, 3500); // Верхний левый угол (BottomMicronsY)

        var handle = HitTestHelper.GetHitHandle(point, text);

        Assert.NotNull(handle);
    }

    [Fact]
    public void GetHitHandle_Text_BottomRightHandle()
    {
        var text = new Text(0, 0, "Test", 3500);
        var textWidth = text.WidthMicrons;
        var point = new PointMicrons(textWidth, 0); // Нижний правый угол

        var handle = HitTestHelper.GetHitHandle(point, text);

        Assert.NotNull(handle);
    }

    [Fact]
    public void GetHitHandle_Text_NoHandle()
    {
        var text = new Text(0, 0, "Test", 3500);
        var point = new PointMicrons(50000, 50000); // Далеко от текста

        var handle = HitTestHelper.GetHitHandle(point, text);

        Assert.Null(handle);
    }

    [Fact]
    public void GetHitHandle_UnknownObject_ReturnsNull()
    {
        var unknownObj = new UnknownObject(0, 0);
        var point = new PointMicrons(0, 0);

        var handle = HitTestHelper.GetHitHandle(point, unknownObj);

        Assert.Null(handle);
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

    // ===== Extended: HitTestAll =====

    [Fact]
    public void HitTestAll_ReturnsAllObjectsUnderPoint()
    {
        var rect = new Rectangle(0, 0, 10000, 10000);
        var line = new Line(0, 0, 10000, 10000);
        var objects = new List<TemplateObjectBase> { rect, line };
        var point = new PointMicrons(5000, 2500); // within rect border band and near line

        var results = HitTestHelper.HitTestAll(point, objects);

        Assert.Equal(2, results.Count);
        Assert.Contains(rect, results);
        Assert.Contains(line, results);
    }

    [Fact]
    public void HitTestAll_ReturnsEmptyList_WhenNoObjects()
    {
        var objects = new List<TemplateObjectBase>();
        var point = new PointMicrons(0, 0);

        var results = HitTestHelper.HitTestAll(point, objects);

        Assert.Empty(results);
    }

    [Fact]
    public void HitTestAll_ReturnsEmptyList_WhenNoObjectsUnderPoint()
    {
        var objects = new List<TemplateObjectBase>
        {
            new Rectangle(0, 0, 1000, 1000),
        };
        var point = new PointMicrons(7000, 7000); // outside expanded bounds (tol=5000)

        var results = HitTestHelper.HitTestAll(point, objects);

        Assert.Empty(results);
    }

    [Fact]
    public void HitTestAll_ReturnsObjectsInZOrder_TopmostFirst()
    {
        var bottom = new Rectangle(0, 0, 10000, 10000);
        var middle = new Rectangle(0, 0, 6000, 6000);
        var top = new Rectangle(0, 0, 3000, 3000);
        var objects = new List<TemplateObjectBase> { bottom, middle, top };
        var point = new PointMicrons(1000, 1000);

        var results = HitTestHelper.HitTestAll(point, objects);

        Assert.Equal(3, results.Count);
        Assert.Same(top, results[0]);
        Assert.Same(middle, results[1]);
        Assert.Same(bottom, results[2]);
    }

    // ===== Extended: HitTestLine =====

    [Fact]
    public void HitTestLine_PointOnLine_ReturnsTrue()
    {
        var line = new Line(0, 0, 10000, 0);
        var point = new PointMicrons(5000, 0);

        Assert.True(HitTestHelper.HitTestLine(line, point));
    }

    [Fact]
    public void HitTestLine_PointNearEndpointWithinTolerance_ReturnsTrue()
    {
        var line = new Line(0, 0, 10000, 0);
        var point = new PointMicrons(0, 3000); // 3mm from start endpoint

        Assert.True(HitTestHelper.HitTestLine(line, point));
    }

    [Fact]
    public void HitTestLine_HorizontalLine_ReturnsTrue()
    {
        var line = new Line(0, 5000, 10000, 5000);
        var point = new PointMicrons(5000, 5000);

        Assert.True(HitTestHelper.HitTestLine(line, point));
    }

    [Fact]
    public void HitTestLine_VerticalLine_ReturnsTrue()
    {
        var line = new Line(5000, 0, 5000, 10000);
        var point = new PointMicrons(5000, 5000);

        Assert.True(HitTestHelper.HitTestLine(line, point));
    }

    [Fact]
    public void HitTestLine_ZeroLengthLine_PointOnIt_ReturnsTrue()
    {
        var line = new Line(5000, 5000, 5000, 5000);
        var point = new PointMicrons(5000, 5000);

        Assert.True(HitTestHelper.HitTestLine(line, point));
    }

    [Fact]
    public void HitTestLine_ZeroLengthLine_PointWithinTolerance_ReturnsTrue()
    {
        var line = new Line(5000, 5000, 5000, 5000);
        var point = new PointMicrons(8000, 5000); // 3mm away

        Assert.True(HitTestHelper.HitTestLine(line, point));
    }

    [Fact]
    public void HitTestLine_ZeroLengthLine_PointOutsideTolerance_ReturnsFalse()
    {
        var line = new Line(5000, 5000, 5000, 5000);
        var point = new PointMicrons(11000, 5000); // 6mm away

        Assert.False(HitTestHelper.HitTestLine(line, point));
    }

    [Fact]
    public void HitTestLine_DiagonalLine_PointOnLine_ReturnsTrue()
    {
        var line = new Line(0, 0, 10000, 10000);
        var point = new PointMicrons(5000, 5000);

        Assert.True(HitTestHelper.HitTestLine(line, point));
    }

    [Fact]
    public void HitTestLine_DiagonalLine_PointWithinTolerance_ReturnsTrue()
    {
        var line = new Line(0, 0, 10000, 10000);
        var point = new PointMicrons(5000, 8535); // ~5mm perpendicular distance

        Assert.True(HitTestHelper.HitTestLine(line, point));
    }

    // ===== Extended: HitTestRectangle =====

    [Fact]
    public void HitTestRectangle_PointAtCorner_ReturnsTrue()
    {
        var rect = new Rectangle(0, 0, 10000, 5000);
        var point = new PointMicrons(10000, 5000); // top-right corner

        Assert.True(HitTestHelper.HitTestRectangle(rect, point));
    }

    [Fact]
    public void HitTestRectangle_PointOnLeftEdge_ReturnsTrue()
    {
        var rect = new Rectangle(0, 0, 10000, 5000);
        var point = new PointMicrons(0, 2500);

        Assert.True(HitTestHelper.HitTestRectangle(rect, point));
    }

    [Fact]
    public void HitTestRectangle_PointOnRightEdge_ReturnsTrue()
    {
        var rect = new Rectangle(0, 0, 10000, 5000);
        var point = new PointMicrons(10000, 2500);

        Assert.True(HitTestHelper.HitTestRectangle(rect, point));
    }

    [Fact]
    public void HitTestRectangle_PointOnTopEdge_ReturnsTrue()
    {
        var rect = new Rectangle(0, 0, 10000, 5000);
        var point = new PointMicrons(5000, 5000);

        Assert.True(HitTestHelper.HitTestRectangle(rect, point));
    }

    [Fact]
    public void HitTestRectangle_PointOnBottomEdge_ReturnsTrue()
    {
        var rect = new Rectangle(0, 0, 10000, 5000);
        var point = new PointMicrons(5000, 0);

        Assert.True(HitTestHelper.HitTestRectangle(rect, point));
    }

    [Fact]
    public void HitTestRectangle_PointJustOutside_ReturnsFalse()
    {
        var rect = new Rectangle(0, 0, 10000, 5000);
        var point = new PointMicrons(16000, 2500); // outside expanded bounds (tol=5000)

        Assert.False(HitTestHelper.HitTestRectangle(rect, point));
    }

    [Fact]
    public void HitTestRectangle_NegativeCoordinates_ReturnsFalse()
    {
        var rect = new Rectangle(0, 0, 10000, 5000);
        var point = new PointMicrons(-6000, -6000); // outside expanded bounds (tol=5000)

        Assert.False(HitTestHelper.HitTestRectangle(rect, point));
    }

    // ===== Extended: HitTestText =====

    [Fact]
    public void HitTestText_PointInside_ReturnsTrue_0DegreeRotation()
    {
        var text = new Text(0, 0, "Test", 5000);
        var point = new PointMicrons(3000, 2000);

        Assert.True(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_PointOutside_ReturnsFalse_0DegreeRotation()
    {
        var text = new Text(0, 0, "Test", 5000);
        var point = new PointMicrons(100000, 100000);

        Assert.False(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_90DegreeRotation_PointInside_ReturnsTrue()
    {
        var text = new Text(0, 0, "Test", 5000, rotationAngle: 90);
        // W=10000, H=5000. LayoutTransform offset at 90°: minX=-H=-5000, minY=0.
        // Rotation center = (X-minX, Y+H+minY) = (5000, 5000).
        // Visual center = center + rotated (W/2, H/2) = (5000-2500, 5000-5000) = (2500, 0).
        var point = new PointMicrons(2500, 0);

        Assert.True(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_180DegreeRotation_PointInside_ReturnsTrue()
    {
        var text = new Text(10000, 10000, "Test", 5000, rotationAngle: 180);
        // W=10000, H=5000. LayoutTransform offset at 180°: minX=-W=-10000, minY=-H=-5000.
        // Rotation center = (X-minX, Y+H+minY) = (20000, 10000).
        // Visual center = center + rotated (W/2, H/2) = (20000-5000, 10000+2500) = (15000, 12500).
        var point = new PointMicrons(15000, 12500);

        Assert.True(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_270DegreeRotation_PointInside_ReturnsTrue()
    {
        var text = new Text(10000, 10000, "Test", 5000, rotationAngle: 270);
        // W=10000, H=5000. LayoutTransform offset at 270°: minX=0, minY=-W=-10000.
        // Rotation center = (X-minX, Y+H+minY) = (10000, 5000).
        // Visual center = center + rotated (W/2, H/2) = (10000+2500, 5000+5000) = (12500, 10000).
        var point = new PointMicrons(12500, 10000);

        Assert.True(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_EmptyContent_ReturnsTrue_OnlyOnXOrigin()
    {
        var text = new Text(0, 0, "", 5000);
        var point = new PointMicrons(0, 1000);

        Assert.True(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_EmptyContent_OutsideReturnsFalse()
    {
        var text = new Text(0, 0, "", 5000);
        var point = new PointMicrons(6000, 6000); // outside 5000x5000 box

        Assert.False(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_0DegreeRotation_OnOrigin_ReturnsTrue()
    {
        var text = new Text(0, 0, "Test", 5000);
        var point = new PointMicrons(0, 0);

        Assert.True(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_90DegreeRotation_OutsideReturnsFalse()
    {
        var text = new Text(0, 0, "Test", 5000, rotationAngle: 90);
        var point = new PointMicrons(6000, 6000);

        Assert.False(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_180DegreeRotation_OutsideReturnsFalse()
    {
        var text = new Text(10000, 10000, "Test", 5000, rotationAngle: 180);
        // Bounding box after offset: X∈[10000, 20000], Y∈[10000, 15000].
        // Point (5000, 5000) is clearly below-left of the shifted box.
        var point = new PointMicrons(5000, 5000);

        Assert.False(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_270DegreeRotation_OutsideReturnsFalse()
    {
        var text = new Text(10000, 10000, "Test", 5000, rotationAngle: 270);
        // Bounding box after offset: X∈[10000, 15000], Y∈[5000, 15000].
        // Point (5000, 20000) is clearly above-left of the shifted box.
        var point = new PointMicrons(5000, 20000);

        Assert.False(HitTestHelper.HitTestText(text, point));
    }

    // ===== Extended: DistanceFromPointToLine =====

    [Fact]
    public void DistanceFromPointToLine_PointPerpendicularToMidpoint()
    {
        var start = new PointMicrons(0, 0);
        var end = new PointMicrons(10000, 0);
        var point = new PointMicrons(5000, 3000);

        var distance = HitTestHelper.DistanceFromPointToLine(point, start, end);

        Assert.Equal(3000, distance);
    }

    [Fact]
    public void DistanceFromPointToLine_PointNearStart_ReturnsDistanceToStart()
    {
        var start = new PointMicrons(0, 0);
        var end = new PointMicrons(10000, 0);
        var point = new PointMicrons(-4000, 0);

        var distance = HitTestHelper.DistanceFromPointToLine(point, start, end);

        Assert.Equal(4000, distance);
    }

    [Fact]
    public void DistanceFromPointToLine_PointNearEnd_ReturnsDistanceToEnd()
    {
        var start = new PointMicrons(0, 0);
        var end = new PointMicrons(10000, 0);
        var point = new PointMicrons(14000, 0);

        var distance = HitTestHelper.DistanceFromPointToLine(point, start, end);

        Assert.Equal(4000, distance);
    }

    [Fact]
    public void DistanceFromPointToLine_ZeroLengthLine_ReturnsDistanceToPoint()
    {
        var start = new PointMicrons(5000, 5000);
        var end = new PointMicrons(5000, 5000);
        var point = new PointMicrons(8000, 9000);

        var distance = HitTestHelper.DistanceFromPointToLine(point, start, end);

        // sqrt(3000^2 + 4000^2) = 5000
        Assert.Equal(5000, distance);
    }

    [Fact]
    public void DistanceFromPointToLine_VeryLongLine_ReturnsCorrectDistance()
    {
        var start = new PointMicrons(0, 0);
        var end = new PointMicrons(1_000_000, 0);
        var point = new PointMicrons(500_000, 7000);

        var distance = HitTestHelper.DistanceFromPointToLine(point, start, end);

        Assert.Equal(7000, distance);
    }

    [Fact]
    public void DistanceFromPointToLine_DiagonalLine_PointOffLine()
    {
        var start = new PointMicrons(0, 0);
        var end = new PointMicrons(10000, 10000);
        var point = new PointMicrons(0, 10000);

        var distance = HitTestHelper.DistanceFromPointToLine(point, start, end);

        // Perpendicular distance from (0,10000) to line y=x = 10000/sqrt(2) ≈ 7071
        Assert.Equal(7071, distance);
    }

    [Fact]
    public void DistanceFromPointToLine_VerticalLine()
    {
        var start = new PointMicrons(5000, 0);
        var end = new PointMicrons(5000, 10000);
        var point = new PointMicrons(8000, 5000);

        var distance = HitTestHelper.DistanceFromPointToLine(point, start, end);

        Assert.Equal(3000, distance);
    }

    [Fact]
    public void DistanceFromPointToLine_PointBeyondStartOnExtension_ReturnsDistanceToStart()
    {
        var start = new PointMicrons(0, 0);
        var end = new PointMicrons(0, 10000);
        var point = new PointMicrons(0, -3000);

        var distance = HitTestHelper.DistanceFromPointToLine(point, start, end);

        Assert.Equal(3000, distance);
    }

    [Fact]
    public void DistanceFromPointToLine_PointBeyondEndOnExtension_ReturnsDistanceToEnd()
    {
        var start = new PointMicrons(0, 0);
        var end = new PointMicrons(0, 10000);
        var point = new PointMicrons(0, 13000);

        var distance = HitTestHelper.DistanceFromPointToLine(point, start, end);

        Assert.Equal(3000, distance);
    }

    [Fact]
    public void DistanceFromPointToLine_PointExactlyOnStart_ReturnsZero()
    {
        var start = new PointMicrons(1000, 2000);
        var end = new PointMicrons(10000, 12000);
        var point = new PointMicrons(1000, 2000);

        var distance = HitTestHelper.DistanceFromPointToLine(point, start, end);

        Assert.Equal(0, distance);
    }

    [Fact]
    public void DistanceFromPointToLine_PointExactlyOnEnd_ReturnsZero()
    {
        var start = new PointMicrons(1000, 2000);
        var end = new PointMicrons(10000, 12000);
        var point = new PointMicrons(10000, 12000);

        var distance = HitTestHelper.DistanceFromPointToLine(point, start, end);

        Assert.Equal(0, distance);
    }

    // === GetTextHandle with corrected metrics ===

    [Fact]
    public void GetTextHandle_CorrectedMetrics_0Deg_HitsCorner()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(0, 0, "Hi", 10000, "ГОСТ Б", rotationAngle: 0);

        // Bottom-left corner (RotatedCorner0) = (0, 0+HeightMicrons)
        var corner = new PointMicrons(0, text.HeightMicrons);
        var handle = HitTestHelper.GetHitHandle(corner, text);
        Assert.NotNull(handle);
    }

    [Fact]
    public void GetTextHandle_CorrectedMetrics_90Deg_HitsCorner()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(0, 0, "Hi", 10000, "ГОСТ Б", rotationAngle: 90);

        // RotatedCorner0 for 90° = (X, Y+H)
        var corner = new PointMicrons(text.RotatedCorner0X, text.RotatedCorner0Y);
        var handle = HitTestHelper.GetHitHandle(corner, text);
        Assert.NotNull(handle);
    }

    [Fact]
    public void GetTextHandle_CorrectedMetrics_180Deg_HitsCorner()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(0, 0, "Hi", 10000, "ГОСТ Б", rotationAngle: 180);

        var corner = new PointMicrons(text.RotatedCorner0X, text.RotatedCorner0Y);
        var handle = HitTestHelper.GetHitHandle(corner, text);
        Assert.NotNull(handle);
    }

    [Fact]
    public void GetTextHandle_CorrectedMetrics_270Deg_HitsCorner()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(0, 0, "Hi", 10000, "ГОСТ Б", rotationAngle: 270);

        var corner = new PointMicrons(text.RotatedCorner0X, text.RotatedCorner0Y);
        var handle = HitTestHelper.GetHitHandle(corner, text);
        Assert.NotNull(handle);
    }

    [Fact]
    public void GetTextHandle_CorrectedMetrics_90Deg_HitsAllFourCorners()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(0, 0, "Hi", 10000, "ГОСТ Б", rotationAngle: 90);

        var corners = new[]
        {
            new PointMicrons(text.RotatedCorner0X, text.RotatedCorner0Y),
            new PointMicrons(text.RotatedCorner1X, text.RotatedCorner1Y),
            new PointMicrons(text.RotatedCorner2X, text.RotatedCorner2Y),
            new PointMicrons(text.RotatedCorner3X, text.RotatedCorner3Y),
        };

        foreach (var corner in corners)
        {
            var handle = HitTestHelper.GetHitHandle(corner, text);
            Assert.NotNull(handle);
        }
    }

    [Fact]
    public void GetTextHandle_CorrectedMetrics_180Deg_HitsAllFourCorners()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(0, 0, "Hi", 10000, "ГОСТ Б", rotationAngle: 180);

        var corners = new[]
        {
            new PointMicrons(text.RotatedCorner0X, text.RotatedCorner0Y),
            new PointMicrons(text.RotatedCorner1X, text.RotatedCorner1Y),
            new PointMicrons(text.RotatedCorner2X, text.RotatedCorner2Y),
            new PointMicrons(text.RotatedCorner3X, text.RotatedCorner3Y),
        };

        foreach (var corner in corners)
        {
            var handle = HitTestHelper.GetHitHandle(corner, text);
            Assert.NotNull(handle);
        }
    }

    private class UnknownObject : TemplateObjectBase
    {
        public UnknownObject(long x, long y) { MicronsX = x; MicronsY = y; }
        public override long MicronsX { get; set; }
        public override long MicronsY { get; set; }
        public override TemplateObjectBase Clone() => new UnknownObject(MicronsX, MicronsY);
        public override bool ContainsPoint(PointMicrons point) => false;
        public override RectMicrons GetBoundingBox() => new RectMicrons(0, 0, 0, 0);
        public override ResizeState CaptureResizeState() => new(MicronsX, MicronsY, 0, 0);
        public override void ApplyResize(ResizeState state) { MicronsX = state.X; MicronsY = state.Y; }
    }
}
