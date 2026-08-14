using DotElectric.TemplateEditor.Tools;

namespace DotElectric.TemplateEditor.Tests.Tools;

public class ResizeMathTests
{
    private const long SheetW = 210000; // A4 portrait width (microns)
    private const long SheetH = 297000; // A4 portrait height (microns)
    private const long MinSize = 1000;
    private const long SnapStep = 1000;

    // ==================== ComputeRectangleResize ====================

    [Fact]
    public void ComputeRectangleResize_BottomRight_MovesRightAndBottom()
    {
        var (x, y, w, h) = ResizeMath.ComputeRectangleResize(
            10000, 10000, 20000, 20000,
            dx: 5000, dy: 3000,
            ResizeHandle.BottomRight, shiftPressed: false, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        Assert.Equal((10000, 13000, 25000, 17000), (x, y, w, h));
    }

    [Fact]
    public void ComputeRectangleResize_Right_OnlyWidthChanges()
    {
        var (x, y, w, h) = ResizeMath.ComputeRectangleResize(
            10000, 10000, 20000, 20000,
            dx: 4000, dy: 0,
            ResizeHandle.Right, shiftPressed: false, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        Assert.Equal((10000, 10000, 24000, 20000), (x, y, w, h));
    }

    [Fact]
    public void ComputeRectangleResize_Left_MovesXAndReducesWidth()
    {
        var (x, y, w, h) = ResizeMath.ComputeRectangleResize(
            20000, 10000, 20000, 20000,
            dx: 5000, dy: 0,
            ResizeHandle.Left, shiftPressed: false, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        Assert.Equal((25000, 10000, 15000, 20000), (x, y, w, h));
    }

    [Fact]
    public void ComputeRectangleResize_Top_OnlyHeightChanges()
    {
        var (x, y, w, h) = ResizeMath.ComputeRectangleResize(
            10000, 10000, 20000, 20000,
            dx: 0, dy: 3000,
            ResizeHandle.Top, shiftPressed: false, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        Assert.Equal((10000, 10000, 20000, 23000), (x, y, w, h));
    }

    [Fact]
    public void ComputeRectangleResize_Bottom_MovesBottomEdge()
    {
        var (x, y, w, h) = ResizeMath.ComputeRectangleResize(
            10000, 10000, 20000, 20000,
            dx: 0, dy: 3000,
            ResizeHandle.Bottom, shiftPressed: false, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        Assert.Equal((10000, 13000, 20000, 17000), (x, y, w, h));
    }

    [Fact]
    public void ComputeRectangleResize_TopLeft_MovesXAndTop()
    {
        var (x, y, w, h) = ResizeMath.ComputeRectangleResize(
            20000, 20000, 20000, 20000,
            dx: 5000, dy: 3000,
            ResizeHandle.TopLeft, shiftPressed: false, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

// newTop = startTop + dy = 43000; height = 43000 - 20000 = 23000
        Assert.Equal((25000, 20000, 15000, 23000), (x, y, w, h));
    }

    [Fact]
    public void ComputeRectangleResize_TopRight_MovesRightAndTop()
    {
        var (x, y, w, h) = ResizeMath.ComputeRectangleResize(
            10000, 10000, 20000, 20000,
            dx: 5000, dy: 3000,
            ResizeHandle.TopRight, shiftPressed: false, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

// newTop = startTop + dy = 33000; height = 33000 - 10000 = 23000
        Assert.Equal((10000, 10000, 25000, 23000), (x, y, w, h));
    }

    [Fact]
    public void ComputeRectangleResize_BottomLeft_MovesXAndBottom()
    {
        var (x, y, w, h) = ResizeMath.ComputeRectangleResize(
            20000, 10000, 20000, 20000,
            dx: 5000, dy: 3000,
            ResizeHandle.BottomLeft, shiftPressed: false, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        Assert.Equal((25000, 13000, 15000, 17000), (x, y, w, h));
    }

    [Fact]
    public void ComputeRectangleResize_Ctrl_ExpandsFromCenter_Horizontal()
    {
        var (x, y, w, h) = ResizeMath.ComputeRectangleResize(
            10000, 10000, 20000, 20000,
            dx: 3000, dy: 0,
            ResizeHandle.Left, shiftPressed: false, ctrlPressed: true,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        Assert.Equal((7000, 10000, 26000, 20000), (x, y, w, h));
    }

    [Fact]
    public void ComputeRectangleResize_Ctrl_ExpandsFromCenter_Vertical()
    {
        var (x, y, w, h) = ResizeMath.ComputeRectangleResize(
            10000, 10000, 20000, 20000,
            dx: 0, dy: 2000,
            ResizeHandle.Bottom, shiftPressed: false, ctrlPressed: true,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        Assert.Equal((10000, 8000, 20000, 24000), (x, y, w, h));
    }

    [Fact]
    public void ComputeRectangleResize_Shift_BottomRight_HeightFromWidth()
    {
        var (x, y, w, h) = ResizeMath.ComputeRectangleResize(
            10000, 10000, 20000, 10000,
            dx: 10000, dy: 5000,
            ResizeHandle.BottomRight, shiftPressed: true, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        // aspect = 2.0; |dx| >= |dy| -> newHeight = width / aspect = 15000
        Assert.Equal((10000, 5000, 30000, 15000), (x, y, w, h));
    }

    [Fact]
    public void ComputeRectangleResize_Shift_TopLeft_WidthFromHeight()
    {
        var (x, y, w, h) = ResizeMath.ComputeRectangleResize(
            10000, 10000, 20000, 10000,
            dx: 2000, dy: 6000,
            ResizeHandle.TopLeft, shiftPressed: true, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

// aspect = 2.0; newHeight = 16000 (26000 - 10000); |dy| > |dx| ->
        // newWidth = height * aspect = 32000; newX = startRight - newWidth = -2000 -> clamped to 0
        Assert.Equal((0, 10000, 32000, 16000), (x, y, w, h));
    }

    [Fact]
    public void ComputeRectangleResize_Shift_TopRight_HeightFromWidth()
    {
        var (x, y, w, h) = ResizeMath.ComputeRectangleResize(
            10000, 10000, 20000, 10000,
            dx: 10000, dy: 5000,
            ResizeHandle.TopRight, shiftPressed: true, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        // newWidth = 30000, newHeight = 15000; anchor bottom-left (startX, startY)
        Assert.Equal((10000, 10000, 30000, 15000), (x, y, w, h));
    }

    [Fact]
    public void ComputeRectangleResize_Shift_BottomLeft_WidthFromHeight()
    {
        var (x, y, w, h) = ResizeMath.ComputeRectangleResize(
            10000, 10000, 20000, 10000,
            dx: 2000, dy: 6000,
            ResizeHandle.BottomLeft, shiftPressed: true, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        // newWidth = 8000 (height 4000 * aspect 2.0); anchor right-top (startRight, startTop)
        Assert.Equal((22000, 16000, 8000, 4000), (x, y, w, h));
    }

    [Fact]
    public void ComputeRectangleResize_SnapEnabled_SnapsToStep()
    {
        var (x, y, w, h) = ResizeMath.ComputeRectangleResize(
            10000, 10000, 20000, 20000,
            dx: 5432, dy: 3333,
            ResizeHandle.BottomRight, shiftPressed: false, ctrlPressed: false,
            snapEnabled: true, SnapStep, SheetW, SheetH, MinSize);

        // 13333 -> 13000; 25432 -> 25000; 16667 -> 17000
        Assert.Equal((10000, 13000, 25000, 17000), (x, y, w, h));
    }

    [Fact]
    public void ComputeRectangleResize_ClampMinimumSize_LeftOnly()
    {
        var (x, y, w, h) = ResizeMath.ComputeRectangleResize(
            10000, 10000, 10000, 10000,
            dx: 11000, dy: 0,
            ResizeHandle.Left, shiftPressed: false, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        // newX pushed beyond right - minSize -> clamped to startRight - minSize = 19000
        Assert.Equal((19000, 10000, 1000, 10000), (x, y, w, h));
    }

    [Fact]
    public void ComputeRectangleResize_ClampMinimumSize_RightOnly()
    {
        var (x, y, w, h) = ResizeMath.ComputeRectangleResize(
            10000, 10000, 10000, 10000,
            dx: -20000, dy: 0,
            ResizeHandle.Right, shiftPressed: false, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        // right edge crosses left -> clamped to x + minSize = 11000
        Assert.Equal((10000, 10000, 1000, 10000), (x, y, w, h));
    }

    [Fact]
    public void ComputeRectangleResize_ClampMinimumSize_SymmetricCollapse()
    {
        var (x, y, w, h) = ResizeMath.ComputeRectangleResize(
            10000, 10000, 10000, 10000,
            dx: -8000, dy: 0,
            ResizeHandle.Left, shiftPressed: false, ctrlPressed: true,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        // newX = 18000, newRight = 12000 -> collapse around mid 15000 -> width = 1000
        Assert.Equal((14500, 10000, 1000, 10000), (x, y, w, h));
    }

    [Fact]
    public void ComputeRectangleResize_SheetClamp_X()
    {
        var (x, y, w, h) = ResizeMath.ComputeRectangleResize(
            5000, 5000, 10000, 10000,
            dx: -10000, dy: 0,
            ResizeHandle.Left, shiftPressed: false, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        Assert.Equal((0, 5000, 20000, 10000), (x, y, w, h));
    }

    [Fact]
    public void ComputeRectangleResize_SheetClamp_Y()
    {
        var (x, y, w, h) = ResizeMath.ComputeRectangleResize(
            5000, 5000, 10000, 10000,
            dx: 0, dy: -10000,
            ResizeHandle.Bottom, shiftPressed: false, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        Assert.Equal((5000, 0, 10000, 20000), (x, y, w, h));
    }

    [Fact]
    public void ComputeRectangleResize_MinSize_FloorsWidth()
    {
        var (x, y, w, h) = ResizeMath.ComputeRectangleResize(
            10000, 10000, 20000, 20000,
            dx: -25000, dy: 0,
            ResizeHandle.BottomRight, shiftPressed: false, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        Assert.Equal((10000, 10000, 1000, 20000), (x, y, w, h));
    }

    // ==================== ComputeTextResize ====================

    [Fact]
    public void ComputeTextResize_NonCornerHandle_MovesAndKeepsFontSize()
    {
        var (x, y, fontSize) = ResizeMath.ComputeTextResize(
            10000, 10000, 20000, 5000,
            dx: 3000, dy: 2000,
            ResizeHandle.Bottom, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        Assert.Equal((13000, 12000, 5000), (x, y, fontSize));
    }

    [Fact]
    public void ComputeTextResize_BottomRight_HeightBased()
    {
        var (x, y, fontSize) = ResizeMath.ComputeTextResize(
            10000, 10000, 20000, 5000,
            dx: 1000, dy: 4000,
            ResizeHandle.BottomRight, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        // |dyLocal| >= |dxLocal| -> scale = (5000 + 4000) / 5000 = 1.8
        Assert.Equal((10000, 10000, 9000), (x, y, fontSize));
    }

    [Fact]
    public void ComputeTextResize_BottomRight_WidthBased()
    {
        var (x, y, fontSize) = ResizeMath.ComputeTextResize(
            10000, 10000, 20000, 5000,
            dx: 6000, dy: 1000,
            ResizeHandle.BottomRight, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        // |dyLocal| < |dxLocal| -> scale = (20000 + 6000) / 20000 = 1.3
        Assert.Equal((10000, 10000, 6500), (x, y, fontSize));
    }

    [Fact]
    public void ComputeTextResize_TopRight_ShiftsYByDeltaHeight()
    {
        var (x, y, fontSize) = ResizeMath.ComputeTextResize(
            10000, 10000, 20000, 5000,
            dx: 1000, dy: 4000,
            ResizeHandle.TopRight, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        Assert.Equal((10000, 6000, 9000), (x, y, fontSize));
    }

    [Fact]
    public void ComputeTextResize_BottomLeft_ShiftsXByDeltaWidth()
    {
        var (x, y, fontSize) = ResizeMath.ComputeTextResize(
            10000, 10000, 20000, 5000,
            dx: 6000, dy: 1000,
            ResizeHandle.BottomLeft, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        Assert.Equal((4000, 10000, 6500), (x, y, fontSize));
    }

    [Fact]
    public void ComputeTextResize_TopLeft_ShiftsXAndY()
    {
        var (x, y, fontSize) = ResizeMath.ComputeTextResize(
            10000, 10000, 20000, 5000,
            dx: 6000, dy: 4000,
            ResizeHandle.TopLeft, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        // width-based scale 1.3 -> fontSize 6500; deltaW = 6000, deltaH = 1500
        Assert.Equal((4000, 8500, 6500), (x, y, fontSize));
    }

    [Fact]
    public void ComputeTextResize_Ctrl_TinyDelta_ScaleOne()
    {
        var (x, y, fontSize) = ResizeMath.ComputeTextResize(
            10000, 10000, 20000, 5000,
            dx: 0.4, dy: 0.2,
            ResizeHandle.BottomRight, ctrlPressed: true,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        Assert.Equal((10000, 10000, 5000), (x, y, fontSize));
    }

    [Fact]
    public void ComputeTextResize_Ctrl_PositiveSign_GrowsFont()
    {
        var (x, y, fontSize) = ResizeMath.ComputeTextResize(
            10000, 10000, 20000, 5000,
            dx: 3000, dy: 1000,
            ResizeHandle.BottomRight, ctrlPressed: true,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        // maxDelta = 3000, sign +1 -> scale = (5000 + 3000) / 5000 = 1.6
        Assert.Equal((10000, 10000, 8000), (x, y, fontSize));
    }

    [Fact]
    public void ComputeTextResize_Ctrl_NegativeSign_ShrinksFont()
    {
        var (x, y, fontSize) = ResizeMath.ComputeTextResize(
            10000, 10000, 20000, 5000,
            dx: -3000, dy: 1000,
            ResizeHandle.BottomRight, ctrlPressed: true,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        // sign -1 -> scale = (5000 - 3000) / 5000 = 0.4 -> fontSize 2000
        Assert.Equal((10000, 10000, 2000), (x, y, fontSize));
    }

    [Fact]
    public void ComputeTextResize_Rotation90_ProjectsDeltas()
    {
        var (x, y, fontSize) = ResizeMath.ComputeTextResize(
            10000, 10000, 20000, 5000,
            dx: 0, dy: 4000,
            ResizeHandle.BottomRight, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize,
            rotationAngle: 90);

        // 90°: dxLocal = dy = 4000, dyLocal = -dx = 0 -> width-based scale 1.2
        Assert.Equal((10000, 10000, 6000), (x, y, fontSize));
    }

    [Fact]
    public void ComputeTextResize_Rotation45_ProjectsDeltas()
    {
        var (x, y, fontSize) = ResizeMath.ComputeTextResize(
            10000, 10000, 20000, 5000,
            dx: 1000, dy: 1000,
            ResizeHandle.BottomRight, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize,
            rotationAngle: 45);

        // dxLocal = 1414.21, dyLocal = 0 -> width-based scale 1.07071 -> 5354
        Assert.Equal((10000, 10000, 5354), (x, y, fontSize));
    }

    [Fact]
    public void ComputeTextResize_SnapEnabled_SnapsFontSize()
    {
        var (x, y, fontSize) = ResizeMath.ComputeTextResize(
            10000, 10000, 20000, 5000,
            dx: 0, dy: 4321,
            ResizeHandle.BottomRight, ctrlPressed: false,
            snapEnabled: true, SnapStep, SheetW, SheetH, MinSize);

        // 9321 -> snap 9000
        Assert.Equal((10000, 10000, 9000), (x, y, fontSize));
    }

    [Fact]
    public void ComputeTextResize_MinFontSize_Clamp()
    {
        var (x, y, fontSize) = ResizeMath.ComputeTextResize(
            10000, 10000, 20000, 5000,
            dx: 0, dy: -10000,
            ResizeHandle.BottomRight, ctrlPressed: false,
            snapEnabled: false, 0, SheetW, SheetH, MinSize);

        // scale clamped to 1000/5000 = 0.2 -> fontSize 1000
        Assert.Equal((10000, 10000, 1000), (x, y, fontSize));
    }

    // ==================== ComputeLineEndpoint ====================

    [Fact]
    public void ComputeLineEndpoint_BottomRight_WithoutSnap()
    {
        var (x, y) = ResizeMath.ComputeLineEndpoint(
            dx: 5000, dy: 3000,
            ResizeHandle.BottomRight,
            lineStartX: 10000, lineStartY: 10000,
            lineEndX: 30000, lineEndY: 30000,
            snapEnabled: false, 0, SheetW, SheetH);

        Assert.Equal((35000, 33000), (x, y));
    }

    [Fact]
    public void ComputeLineEndpoint_BottomRight_WithSnap()
    {
        var (x, y) = ResizeMath.ComputeLineEndpoint(
            dx: 5432, dy: 3333,
            ResizeHandle.BottomRight,
            lineStartX: 10000, lineStartY: 10000,
            lineEndX: 30000, lineEndY: 30000,
            snapEnabled: true, SnapStep, SheetW, SheetH);

        Assert.Equal((35000, 33000), (x, y));
    }

    [Fact]
    public void ComputeLineEndpoint_BottomRight_ClampsToSheet()
    {
        var (x, y) = ResizeMath.ComputeLineEndpoint(
            dx: 10000, dy: 10000,
            ResizeHandle.BottomRight,
            lineStartX: 10000, lineStartY: 10000,
            lineEndX: 205000, lineEndY: 295000,
            snapEnabled: false, 0, SheetW, SheetH);

        Assert.Equal((SheetW, SheetH), (x, y));
    }

    [Fact]
    public void ComputeLineEndpoint_TopLeft_WithoutSnap()
    {
        var (x, y) = ResizeMath.ComputeLineEndpoint(
            dx: -5000, dy: -3000,
            ResizeHandle.TopLeft,
            lineStartX: 10000, lineStartY: 10000,
            lineEndX: 30000, lineEndY: 30000,
            snapEnabled: false, 0, SheetW, SheetH);

        Assert.Equal((5000, 7000), (x, y));
    }

    [Fact]
    public void ComputeLineEndpoint_TopLeft_WithSnapAndClamp()
    {
        var (x, y) = ResizeMath.ComputeLineEndpoint(
            dx: -10000, dy: -10000,
            ResizeHandle.TopLeft,
            lineStartX: 3000, lineStartY: 2000,
            lineEndX: 30000, lineEndY: 30000,
            snapEnabled: true, SnapStep, SheetW, SheetH);

        Assert.Equal((0, 0), (x, y));
    }

    [Fact]
    public void ComputeLineEndpoint_NonEndpointHandle_ReturnsZero()
    {
        var (x, y) = ResizeMath.ComputeLineEndpoint(
            dx: 5000, dy: 5000,
            ResizeHandle.Left,
            lineStartX: 10000, lineStartY: 10000,
            lineEndX: 30000, lineEndY: 30000,
            snapEnabled: false, 0, SheetW, SheetH);

        Assert.Equal((0, 0), (x, y));
    }

    // ==================== CursorForHandle ====================

    [Theory]
    [InlineData(ResizeHandle.TopLeft, true)]
    [InlineData(ResizeHandle.BottomRight, true)]
    [InlineData(ResizeHandle.Left, true)]
    [InlineData((ResizeHandle)999, true)]
    public void CursorForHandle_IsLine_ReturnsCross(ResizeHandle handle, bool isLine)
    {
        Assert.Equal(ToolCursor.Cross, ResizeMath.CursorForHandle(handle, isResizing: true, isLine));
    }

    [Fact]
    public void CursorForHandle_NotResizing_ReturnsArrow()
    {
        Assert.Equal(ToolCursor.Arrow, ResizeMath.CursorForHandle(ResizeHandle.TopRight, isResizing: false, isLine: false));
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
    [InlineData((ResizeHandle)999, ToolCursor.Arrow)]
    public void CursorForHandle_ResizingRectangle_ReturnsHandleCursor(ResizeHandle handle, ToolCursor expected)
    {
        Assert.Equal(expected, ResizeMath.CursorForHandle(handle, isResizing: true, isLine: false));
    }

    // ==================== VisualCursorForHandle ====================

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
        Assert.Equal(expected, ResizeMath.VisualCursorForHandle(handle, angle));
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
        Assert.Equal(expected, ResizeMath.VisualCursorForHandle(handle, angle));
    }

    [Theory]
    [InlineData(ResizeHandle.Top, 90, ToolCursor.SizeNS)]
    [InlineData(ResizeHandle.Bottom, 90, ToolCursor.SizeNS)]
    [InlineData(ResizeHandle.Left, 90, ToolCursor.SizeWE)]
    [InlineData(ResizeHandle.Right, 90, ToolCursor.SizeWE)]
    public void VisualCursorForHandle_EdgeHandles_UnchangedByRotation(ResizeHandle handle, int angle, ToolCursor expected)
    {
        Assert.Equal(expected, ResizeMath.VisualCursorForHandle(handle, angle));
    }
}
