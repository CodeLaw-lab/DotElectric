using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Tools;

namespace DotElectric.TemplateEditor.Tests.Helpers;

[Collection("FontMetrics")]
public class MarkerLayoutTests : IDisposable
{
    public MarkerLayoutTests()
    {
        FontMetricsProvider.Reset();
    }

    public void Dispose()
    {
        FontMetricsProvider.Reset();
    }

    // ===== Каталог маркеров =====

    [Fact]
    public void MarkersFor_Line_ReturnsEndThenStart()
    {
        var line = new Line(0, 0, 10000, 0);

        // Порядок каталога задаёт приоритет hit-проверки: конец линии первым (он наверху в Z-order)
        Assert.Equal(
            [ResizeHandle.BottomRight, ResizeHandle.TopLeft],
            MarkerLayout.MarkersFor(line));
    }

    [Fact]
    public void MarkersFor_Rectangle_ReturnsAllEight()
    {
        var rect = new Rectangle(0, 0, 10000, 10000);

        Assert.Equal(
            [
                ResizeHandle.TopLeft, ResizeHandle.Top, ResizeHandle.TopRight, ResizeHandle.Right,
                ResizeHandle.BottomRight, ResizeHandle.Bottom, ResizeHandle.BottomLeft, ResizeHandle.Left
            ],
            MarkerLayout.MarkersFor(rect));
    }

    [Fact]
    public void MarkersFor_Text_ReturnsFourCorners()
    {
        var text = new Text(0, 0, "Test", 3500);

        Assert.Equal(
            [ResizeHandle.TopLeft, ResizeHandle.TopRight, ResizeHandle.BottomLeft, ResizeHandle.BottomRight],
            MarkerLayout.MarkersFor(text));
    }

    [Fact]
    public void MarkersFor_UnknownObject_Throws()
    {
        var unknownObj = new UnknownObject(0, 0);

        Assert.Throws<NotSupportedException>(() => MarkerLayout.MarkersFor(unknownObj));
    }

    // ===== Позиции маркеров =====

    [Fact]
    public void GetPosition_Line_Endpoints()
    {
        var line = new Line(1000, 2000, 30000, 40000);

        Assert.Equal(new PointMicrons(1000, 2000), MarkerLayout.GetPosition(line, ResizeHandle.TopLeft));
        Assert.Equal(new PointMicrons(30000, 40000), MarkerLayout.GetPosition(line, ResizeHandle.BottomRight));
    }

    [Theory]
    [InlineData(ResizeHandle.TopLeft, 10000, 60000)]
    [InlineData(ResizeHandle.Top, 25000, 60000)]
    [InlineData(ResizeHandle.TopRight, 40000, 60000)]
    [InlineData(ResizeHandle.Right, 40000, 40000)]
    [InlineData(ResizeHandle.BottomRight, 40000, 20000)]
    [InlineData(ResizeHandle.Bottom, 25000, 20000)]
    [InlineData(ResizeHandle.BottomLeft, 10000, 20000)]
    [InlineData(ResizeHandle.Left, 10000, 40000)]
    public void GetPosition_Rectangle_AllEightMarkers(ResizeHandle handle, long expectedX, long expectedY)
    {
        var rect = new Rectangle(10000, 20000, 30000, 40000);

        Assert.Equal(new PointMicrons(expectedX, expectedY), MarkerLayout.GetPosition(rect, handle));
    }

    [Fact]
    public void GetPosition_Text_NoRotation_MatchesBoxCorners()
    {
        var text = new Text(5000, 6000, "Test", 10000) { RotationAngle = 0 };

        // При 0° LayoutTransform offset нулевой: углы — углы нетрансформированного бокса
        Assert.Equal(new PointMicrons(5000, 6000 + text.HeightMicrons), MarkerLayout.GetPosition(text, ResizeHandle.TopLeft));
        Assert.Equal(new PointMicrons(5000 + text.WidthMicrons, 6000 + text.HeightMicrons), MarkerLayout.GetPosition(text, ResizeHandle.TopRight));
        Assert.Equal(new PointMicrons(5000, 6000), MarkerLayout.GetPosition(text, ResizeHandle.BottomLeft));
        Assert.Equal(new PointMicrons(5000 + text.WidthMicrons, 6000), MarkerLayout.GetPosition(text, ResizeHandle.BottomRight));
    }

    [Fact]
    public void GetPosition_Line_EdgeHandle_Throws()
    {
        var line = new Line(0, 0, 10000, 0);

        Assert.Throws<NotSupportedException>(() => MarkerLayout.GetPosition(line, ResizeHandle.Top));
    }

    [Fact]
    public void GetPosition_Text_EdgeHandle_Throws()
    {
        var text = new Text(0, 0, "Test", 3500);

        Assert.Throws<NotSupportedException>(() => MarkerLayout.GetPosition(text, ResizeHandle.Right));
    }

    [Fact]
    public void GetPosition_UnknownObject_Throws()
    {
        var unknownObj = new UnknownObject(0, 0);

        Assert.Throws<NotSupportedException>(() => MarkerLayout.GetPosition(unknownObj, ResizeHandle.TopLeft));
    }

    // ===== Hit-тест маркеров =====

    [Fact]
    public void HitHandle_Line_StartMarker()
    {
        var line = new Line(0, 0, 20000, 20000);
        var point = new PointMicrons(0, 0); // На начале линии

        var handle = MarkerLayout.HitHandle(point, line);

        Assert.Equal(ResizeHandle.TopLeft, handle);
    }

    [Fact]
    public void HitHandle_Line_EndMarker()
    {
        var line = new Line(0, 0, 20000, 20000);
        var point = new PointMicrons(20000, 20000); // На конце линии

        var handle = MarkerLayout.HitHandle(point, line);

        Assert.Equal(ResizeHandle.BottomRight, handle);
    }

    [Fact]
    public void HitHandle_Line_NoMarker()
    {
        var line = new Line(0, 0, 20000, 20000);
        var point = new PointMicrons(10000, 10000); // В середине линии, не на маркере

        var handle = MarkerLayout.HitHandle(point, line);

        Assert.Null(handle);
    }

    [Fact]
    public void HitHandle_ZeroLengthLine_EndWinsByCatalogOrder()
    {
        // Нулевая линия: обе зоны в одной точке; допуск 0, дистанция 0.
        // Порядок каталога (конец первым) определяет результат.
        var line = new Line(5000, 5000, 5000, 5000);
        var point = new PointMicrons(5000, 5000);

        var handle = MarkerLayout.HitHandle(point, line);

        Assert.Equal(ResizeHandle.BottomRight, handle);
    }

    [Fact]
    public void HitHandle_Rectangle_TopLeftMarker()
    {
        var rect = new Rectangle(0, 0, 20000, 20000);
        var point = new PointMicrons(0, 20000); // Верхний левый угол (BottomMicronsY = 0 + 20000)

        var handle = MarkerLayout.HitHandle(point, rect);

        Assert.Equal(ResizeHandle.TopLeft, handle);
    }

    [Fact]
    public void HitHandle_Rectangle_BottomRightMarker()
    {
        var rect = new Rectangle(0, 0, 20000, 20000);
        var point = new PointMicrons(20000, 0); // Нижний правый угол

        var handle = MarkerLayout.HitHandle(point, rect);

        Assert.Equal(ResizeHandle.BottomRight, handle);
    }

    [Fact]
    public void HitHandle_Rectangle_NoMarker()
    {
        var rect = new Rectangle(0, 0, 20000, 20000);
        var point = new PointMicrons(10000, 10000); // В центре прямоугольника

        var handle = MarkerLayout.HitHandle(point, rect);

        Assert.Null(handle);
    }

    [Fact]
    public void HitHandle_Text_TopLeftMarker()
    {
        var text = new Text(0, 0, "Test", 3500);
        var point = new PointMicrons(0, 3500); // Верхний левый угол (BottomMicronsY)

        var handle = MarkerLayout.HitHandle(point, text);

        Assert.NotNull(handle);
    }

    [Fact]
    public void HitHandle_Text_BottomRightMarker()
    {
        var text = new Text(0, 0, "Test", 3500);
        var textWidth = text.WidthMicrons;
        var point = new PointMicrons(textWidth, 0); // Нижний правый угол

        var handle = MarkerLayout.HitHandle(point, text);

        Assert.NotNull(handle);
    }

    [Fact]
    public void HitHandle_Text_NoMarker()
    {
        var text = new Text(0, 0, "Test", 3500);
        var point = new PointMicrons(50000, 50000); // Далеко от текста

        var handle = MarkerLayout.HitHandle(point, text);

        Assert.Null(handle);
    }

    [Fact]
    public void HitHandle_UnknownObject_Throws()
    {
        var unknownObj = new UnknownObject(0, 0);
        var point = new PointMicrons(0, 0);

        Assert.Throws<NotSupportedException>(() => MarkerLayout.HitHandle(point, unknownObj));
    }

    // ===== Допуск маркеров (#82) =====

    [Fact]
    public void GetTolerance_LargeObject_ReturnsFullConstant()
    {
        var rect = new Rectangle(0, 0, 50000, 50000); // minDim/3 = 16.7мм > 8мм

        Assert.Equal(PhysicalConstants.HandleHitToleranceMicrons, MarkerLayout.GetTolerance(rect));
    }

    [Fact]
    public void GetTolerance_SmallText_CappedByThirdOfMinDimension()
    {
        var text = new Text(0, 0, "Текст", 2500, "ГОСТ А");
        var expected = Math.Min(text.WidthMicrons, text.HeightMicrons) / 3;

        Assert.True(expected < PhysicalConstants.HandleHitToleranceMicrons);
        Assert.Equal(expected, MarkerLayout.GetTolerance(text));
    }

    [Fact]
    public void GetTolerance_ShortLine_CappedByThirdOfLength()
    {
        var line = new Line(0, 0, 9000, 0); // длина 9мм → 3мм < 8мм

        Assert.Equal(3000, MarkerLayout.GetTolerance(line));
    }

    [Fact]
    public void GetTolerance_SmallRectangle_CappedByThirdOfMinSide()
    {
        var rect = new Rectangle(0, 0, 6000, 15000); // min-сторона 6мм → 2мм

        Assert.Equal(2000, MarkerLayout.GetTolerance(rect));
    }

    [Fact]
    public void GetTolerance_UnknownObject_Throws()
    {
        var unknownObj = new UnknownObject(0, 0);

        Assert.Throws<NotSupportedException>(() => MarkerLayout.GetTolerance(unknownObj));
    }

    // ===== Hit по маркерам повёрнутого текста (скорректированные метрики шрифта) =====

    [Fact]
    public void HitHandle_Text_CorrectedMetrics_0Deg_HitsCorner()
    {
        FontMetricsProvider.SetCurrent(new DotElectric.TemplateEditor.Tests.Helpers.FixedFontMetrics(1.1719, 0.55));
        var text = new Text(0, 0, "Hi", 10000, "ГОСТ Б", rotationAngle: 0);

        // Bottom-left corner (RotatedCorner0) = (0, 0+HeightMicrons)
        var corner = new PointMicrons(0, text.HeightMicrons);
        var handle = MarkerLayout.HitHandle(corner, text);
        Assert.NotNull(handle);
    }

    [Fact]
    public void HitHandle_Text_CorrectedMetrics_90Deg_HitsCorner()
    {
        FontMetricsProvider.SetCurrent(new DotElectric.TemplateEditor.Tests.Helpers.FixedFontMetrics(1.1719, 0.55));
        var text = new Text(0, 0, "Hi", 10000, "ГОСТ Б", rotationAngle: 90);

        // RotatedCorner0 for 90° = (X, Y+H)
        var corner = new PointMicrons(text.RotatedCorner0X, text.RotatedCorner0Y);
        var handle = MarkerLayout.HitHandle(corner, text);
        Assert.NotNull(handle);
    }

    [Fact]
    public void HitHandle_Text_CorrectedMetrics_180Deg_HitsCorner()
    {
        FontMetricsProvider.SetCurrent(new DotElectric.TemplateEditor.Tests.Helpers.FixedFontMetrics(1.1719, 0.55));
        var text = new Text(0, 0, "Hi", 10000, "ГОСТ Б", rotationAngle: 180);

        var corner = new PointMicrons(text.RotatedCorner0X, text.RotatedCorner0Y);
        var handle = MarkerLayout.HitHandle(corner, text);
        Assert.NotNull(handle);
    }

    [Fact]
    public void HitHandle_Text_CorrectedMetrics_270Deg_HitsCorner()
    {
        FontMetricsProvider.SetCurrent(new DotElectric.TemplateEditor.Tests.Helpers.FixedFontMetrics(1.1719, 0.55));
        var text = new Text(0, 0, "Hi", 10000, "ГОСТ Б", rotationAngle: 270);

        var corner = new PointMicrons(text.RotatedCorner0X, text.RotatedCorner0Y);
        var handle = MarkerLayout.HitHandle(corner, text);
        Assert.NotNull(handle);
    }

    [Fact]
    public void HitHandle_Text_CorrectedMetrics_90Deg_HitsAllFourCorners()
    {
        FontMetricsProvider.SetCurrent(new DotElectric.TemplateEditor.Tests.Helpers.FixedFontMetrics(1.1719, 0.55));
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
            var handle = MarkerLayout.HitHandle(corner, text);
            Assert.NotNull(handle);
        }
    }

    [Fact]
    public void HitHandle_Text_CorrectedMetrics_180Deg_HitsAllFourCorners()
    {
        FontMetricsProvider.SetCurrent(new DotElectric.TemplateEditor.Tests.Helpers.FixedFontMetrics(1.1719, 0.55));
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
            var handle = MarkerLayout.HitHandle(corner, text);
            Assert.NotNull(handle);
        }
    }

    // ===== Классификация маркеров =====

    [Theory]
    //             handle                    left   right  top    bottom corner
    [InlineData(ResizeHandle.TopLeft,       true,  false, true,  false, true)]
    [InlineData(ResizeHandle.Top,           false, false, true,  false, false)]
    [InlineData(ResizeHandle.TopRight,      false, true,  true,  false, true)]
    [InlineData(ResizeHandle.Right,         false, true,  false, false, false)]
    [InlineData(ResizeHandle.BottomRight,   false, true,  false, true,  true)]
    [InlineData(ResizeHandle.Bottom,        false, false, false, true,  false)]
    [InlineData(ResizeHandle.BottomLeft,    true,  false, false, true,  true)]
    [InlineData(ResizeHandle.Left,          true,  false, false, false, false)]
    public void Classification_AllHandles(
        ResizeHandle handle,
        bool expectedLeft, bool expectedRight, bool expectedTop, bool expectedBottom, bool expectedCorner)
    {
        Assert.Equal(expectedLeft, MarkerLayout.TouchesLeft(handle));
        Assert.Equal(expectedRight, MarkerLayout.TouchesRight(handle));
        Assert.Equal(expectedTop, MarkerLayout.TouchesTop(handle));
        Assert.Equal(expectedBottom, MarkerLayout.TouchesBottom(handle));
        Assert.Equal(expectedCorner, MarkerLayout.IsCorner(handle));
    }

    // ===== Курсорная политика =====

    [Theory]
    [InlineData(ResizeHandle.TopLeft)]
    [InlineData(ResizeHandle.Top)]
    [InlineData(ResizeHandle.TopRight)]
    [InlineData(ResizeHandle.Right)]
    [InlineData(ResizeHandle.BottomRight)]
    [InlineData(ResizeHandle.Bottom)]
    [InlineData(ResizeHandle.BottomLeft)]
    [InlineData(ResizeHandle.Left)]
    public void CursorForHandle_IsLine_ReturnsCross(ResizeHandle handle)
    {
        Assert.Equal(ToolCursor.Cross, MarkerLayout.CursorForHandle(handle, isResizing: true, isLine: true));
    }

    [Fact]
    public void CursorForHandle_NotResizing_ReturnsArrow()
    {
        Assert.Equal(ToolCursor.Arrow, MarkerLayout.CursorForHandle(ResizeHandle.TopRight, isResizing: false, isLine: false));
    }

    [Theory]
    [InlineData(ResizeHandle.TopLeft, ToolCursor.SizeNWSE)]
    [InlineData(ResizeHandle.BottomRight, ToolCursor.SizeNWSE)]
    [InlineData(ResizeHandle.TopRight, ToolCursor.SizeNESW)]
    [InlineData(ResizeHandle.BottomLeft, ToolCursor.SizeNESW)]
    [InlineData(ResizeHandle.Top, ToolCursor.SizeNS)]
    [InlineData(ResizeHandle.Bottom, ToolCursor.SizeNS)]
    [InlineData(ResizeHandle.Left, ToolCursor.SizeWE)]
    [InlineData(ResizeHandle.Right, ToolCursor.SizeWE)]
    public void CursorForHandle_ResizingRectangle_ReturnsHandleCursor(ResizeHandle handle, ToolCursor expected)
    {
        Assert.Equal(expected, MarkerLayout.CursorForHandle(handle, isResizing: true, isLine: false));
    }

    [Fact]
    public void CursorForHandle_InvalidHandle_Throws()
    {
        Assert.Throws<NotSupportedException>(
            () => MarkerLayout.CursorForHandle((ResizeHandle)999, isResizing: true, isLine: false));
    }

    [Theory]
    [InlineData(ResizeHandle.TopLeft, 90, ToolCursor.SizeNESW)]
    [InlineData(ResizeHandle.BottomRight, 90, ToolCursor.SizeNESW)]
    [InlineData(ResizeHandle.TopRight, 90, ToolCursor.SizeNWSE)]
    [InlineData(ResizeHandle.BottomLeft, 90, ToolCursor.SizeNWSE)]
    [InlineData(ResizeHandle.TopLeft, 270, ToolCursor.SizeNESW)]
    [InlineData(ResizeHandle.BottomRight, 270, ToolCursor.SizeNESW)]
    [InlineData(ResizeHandle.TopRight, 270, ToolCursor.SizeNWSE)]
    [InlineData(ResizeHandle.BottomLeft, 270, ToolCursor.SizeNWSE)]
    [InlineData(ResizeHandle.TopLeft, -90, ToolCursor.SizeNESW)]
    [InlineData(ResizeHandle.BottomRight, -90, ToolCursor.SizeNESW)]
    [InlineData(ResizeHandle.TopRight, -270, ToolCursor.SizeNWSE)]
    [InlineData(ResizeHandle.BottomLeft, -270, ToolCursor.SizeNWSE)]
    [InlineData(ResizeHandle.TopLeft, 450, ToolCursor.SizeNESW)]
    [InlineData(ResizeHandle.BottomRight, 450, ToolCursor.SizeNESW)]
    public void VisualCursorForHandle_QuarterTurns_SwapDiagonalCursors(ResizeHandle handle, int angle, ToolCursor expected)
    {
        Assert.Equal(expected, MarkerLayout.VisualCursorForHandle(handle, angle));
    }

    [Theory]
    [InlineData(ResizeHandle.TopLeft, 0, ToolCursor.SizeNWSE)]
    [InlineData(ResizeHandle.TopRight, 0, ToolCursor.SizeNESW)]
    [InlineData(ResizeHandle.BottomRight, 180, ToolCursor.SizeNWSE)]
    [InlineData(ResizeHandle.BottomLeft, 180, ToolCursor.SizeNESW)]
    [InlineData(ResizeHandle.TopRight, 540, ToolCursor.SizeNESW)]
    [InlineData(ResizeHandle.TopLeft, 540, ToolCursor.SizeNWSE)]
    public void VisualCursorForHandle_StraightAngles_StandardCursors(ResizeHandle handle, int angle, ToolCursor expected)
    {
        Assert.Equal(expected, MarkerLayout.VisualCursorForHandle(handle, angle));
    }

    [Theory]
    [InlineData(ResizeHandle.Top, 90, ToolCursor.SizeNS)]
    [InlineData(ResizeHandle.Bottom, 90, ToolCursor.SizeNS)]
    [InlineData(ResizeHandle.Left, 90, ToolCursor.SizeWE)]
    [InlineData(ResizeHandle.Right, 90, ToolCursor.SizeWE)]
    public void VisualCursorForHandle_EdgeHandles_UnchangedByRotation(ResizeHandle handle, int angle, ToolCursor expected)
    {
        Assert.Equal(expected, MarkerLayout.VisualCursorForHandle(handle, angle));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(90)]
    public void VisualCursorForHandle_InvalidHandle_Throws(int angle)
    {
        Assert.Throws<NotSupportedException>(
            () => MarkerLayout.VisualCursorForHandle((ResizeHandle)999, angle));
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
