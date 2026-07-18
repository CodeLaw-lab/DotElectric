using System.Collections.ObjectModel;
using System.Windows.Input;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tools;
using DotElectric.TemplateEditor.ViewModels;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Tools;

public class ResizeToolTests
{
    private static EditorViewModel CreateEditorWithObject(TemplateObjectBase obj)
    {
        var template = new Template();
        template.Objects.Add(obj);

        var mockService = new Mock<ITemplateService>();
        var mockPrintService = new Mock<IPrintService>();
        return new EditorViewModel(template, mockService.Object, printService: mockPrintService.Object);
    }

    private static EditorViewModel CreateEmptyEditor()
    {
        var template = new Template();
        var mockService = new Mock<ITemplateService>();
        var mockPrintService = new Mock<IPrintService>();
        return new EditorViewModel(template, mockService.Object, printService: mockPrintService.Object);
    }

    // === Constructor ===

    [Fact]
    public void Constructor_SetsName()
    {
        var editor = CreateEditorWithObject(new Rectangle(0, 0, 10000, 10000));
        var tool = new ResizeTool(editor);

        Assert.Equal("Изменение размера", tool.Name);
    }

    [Fact]
    public void Constructor_ThrowsOnNullEditor()
    {
        Assert.Throws<ArgumentNullException>(() => new ResizeTool(null!));
    }

    // === Rectangle Resize ===

    [Fact]
    public void Resize_Rectangle_BottomRight_IncreasesSize()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        var start = new PointMicrons(10000, 10000);
        var end = new PointMicrons(20000, 20000); // +10мм right, +10мм up

        tool.OnMouseDown(start, ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(end, ToolMouseButton.Left, ToolModifiers.None);

        // BottomRight: right edge +10mm, bottom edge +10mm (moves up → shorter)
        Assert.Equal(30000, rect.WidthMicrons);
        Assert.Equal(10000, rect.HeightMicrons);
    }

    [Fact]
    public void Resize_Rectangle_MinSize_Enforced()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.TopRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        var start = new PointMicrons(30000, 30000);
        var end = new PointMicrons(5000, 5000); // Уменьшаем до < 1мм

        tool.OnMouseDown(start, ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(end, ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(1000, rect.WidthMicrons); // Минимум 1мм
        Assert.Equal(1000, rect.HeightMicrons);
    }

    [Fact]
    public void Resize_Rectangle_Left_MovesXAndReducesWidth()
    {
        var rect = new Rectangle(20000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.Left;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        var start = new PointMicrons(20000, 10000);
        var end = new PointMicrons(25000, 10000); // Сдвигаем вправо на 5мм

        tool.OnMouseDown(start, ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(end, ToolMouseButton.Left, ToolModifiers.None);

        // Width уменьшается, X сдвигается
        Assert.True(rect.WidthMicrons < 20000);
        Assert.True(rect.MicronsX > 20000);
    }

    // === GetCursor ===

    [Fact]
    public void GetCursor_DefaultReturnsArrow()
    {
        var editor = CreateEditorWithObject(new Rectangle(0, 0, 10000, 10000));
        var tool = new ResizeTool(editor);

        Assert.Equal(ToolCursor.Arrow, tool.GetCursor());
    }

    [Fact]
    public void GetCursor_AfterMouseDown_ReturnsCorrectCursor()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(30000, 30000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(ToolCursor.SizeNWSE, tool.GetCursor());
    }

    // === OnMouseUp pushes command ===

    [Fact]
    public void OnMouseUp_PushesResizeCommand()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(20000, 20000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(20000, 20000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(1, editor.CommandHistory.UndoCount);
        Assert.Equal("Изменить размер", editor.CommandHistory.LastUndoName);
    }

    // === Line Resize ===

    [Fact]
    public void Resize_Line_BottomRight_ChangesEndPoint()
    {
        var line = new Line(10000, 10000, 30000, 30000);
        var editor = CreateEditorWithObject(line);
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(line);

        var tool = new ResizeTool(editor);
        var start = new PointMicrons(30000, 30000);
        var end = new PointMicrons(40000, 40000); // +10мм

        tool.OnMouseDown(start, ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(end, ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(40000, line.EndMicronsX);
        Assert.Equal(40000, line.EndMicronsY);
    }

    // === Text Resize ===

    [Fact]
    public void Resize_Text_TopRight_ChangesFontSize()
    {
        var text = new Text(10000, 10000, "Test", 3500);
        var editor = CreateEditorWithObject(text);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.TopRight;
        editor.SelectSingle(text);

        var tool = new ResizeTool(editor);
        var start = new PointMicrons(10000, 10000);
        var end = new PointMicrons(10000, 15000); // +5мм вниз

        tool.OnMouseDown(start, ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(end, ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(8500, text.FontSizeMicrons); // 3500 + 5000
    }

    // === OnMouseWheel/OnDoubleClick do nothing ===

    [Fact]
    public void OnMouseWheel_DoesNothing()
    {
        var editor = CreateEditorWithObject(new Rectangle(0, 0, 10000, 10000));
        var tool = new ResizeTool(editor);

        tool.OnMouseWheel(120, new PointMicrons(0, 0));
        // No exceptions
    }

    [Fact]
    public void OnDoubleClick_DoesNothing()
    {
        var editor = CreateEditorWithObject(new Rectangle(0, 0, 10000, 10000));
        var tool = new ResizeTool(editor);

        tool.OnDoubleClick(new PointMicrons(0, 0));
        // No exceptions
    }

    // ============================================================
    // Constructor tests (from Extended)
    // ============================================================

    [Fact]
    public void Constructor_NullEditor_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ResizeTool(null!));
    }

    // ============================================================
    // OnMouseDown tests
    // ============================================================

    [Fact]
    public void OnMouseDown_LeftButtonOnRectangleHandle_StartsResizing()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(ToolCursor.SizeNWSE, tool.GetCursor());
    }

    [Fact]
    public void OnMouseDown_LeftButtonOnTextHandle_StartsResizing()
    {
        var text = new Text(10000, 10000, "Test", 3500);
        var editor = CreateEditorWithObject(text);
        editor.ActiveResizeHandle = ResizeHandle.TopRight;
        editor.SelectSingle(text);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(ToolCursor.SizeNESW, tool.GetCursor());
    }

    [Fact]
    public void OnMouseDown_LeftButtonOnLineHandle_StartsResizing()
    {
        var line = new Line(10000, 10000, 30000, 30000);
        var editor = CreateEditorWithObject(line);
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(line);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(ToolCursor.Cross, tool.GetCursor());
    }

    [Fact]
    public void OnMouseDown_NonLeftButton_DoesNotStartResizing()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Right, ToolModifiers.None);

        Assert.Equal(ToolCursor.Arrow, tool.GetCursor());
    }

    [Fact]
    public void OnMouseDown_NullActiveResizeHandle_DoesNotStartResizing()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(ToolCursor.Arrow, tool.GetCursor());
    }

    [Fact]
    public void OnMouseDown_NullSingleSelectedObject_DoesNotStartResizing()
    {
        var editor = CreateEmptyEditor();
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(ToolCursor.SizeNWSE, tool.GetCursor());

        tool.OnMouseMove(new PointMicrons(20000, 20000), ToolMouseButton.Left, ToolModifiers.None);
    }

    // ============================================================
    // OnMouseMove tests — without MouseDown
    // ============================================================

    [Fact]
    public void OnMouseMove_WithoutMouseDown_DoesNothing()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseMove(new PointMicrons(20000, 20000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(20000, rect.WidthMicrons);
        Assert.Equal(20000, rect.HeightMicrons);
    }

    // ============================================================
    // OnMouseMove tests — Rectangle resize
    // ============================================================

    [Fact]
    public void Resize_Rectangle_BottomRight_IncreasesWidthAndHeight()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(30000, 30000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(40000, 45000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(30000, rect.WidthMicrons);
        Assert.Equal(5000, rect.HeightMicrons);
    }

    [Fact]
    public void Resize_Rectangle_TopLeft_ChangesPositionAndSize()
    {
        var rect = new Rectangle(20000, 20000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.TopLeft;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(20000, 20000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(25000, 25000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(25000, rect.MicronsX);
        Assert.Equal(20000, rect.MicronsY);
        Assert.Equal(15000, rect.WidthMicrons);
        Assert.Equal(25000, rect.HeightMicrons);
    }

    [Fact]
    public void Resize_Rectangle_Shift_MaintainsAspectRatio()
    {
        var rect = new Rectangle(10000, 10000, 20000, 10000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(30000, 20000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(40000, 25000), ToolMouseButton.Left, ToolModifiers.Shift);

        Assert.Equal(30000, rect.WidthMicrons);
        Assert.Equal(15000, rect.HeightMicrons);
    }

    [Fact]
    public void Resize_Rectangle_Ctrl_ChangesFromCenter()
    {
        var rect = new Rectangle(20000, 20000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(30000, 30000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(35000, 35000), ToolMouseButton.Left, ToolModifiers.Ctrl);

        Assert.Equal(30000, rect.WidthMicrons);
        Assert.Equal(30000, rect.HeightMicrons);
    }

    [Fact]
    public void Resize_Rectangle_ShiftPlusCtrl_BothModifiers()
    {
        var rect = new Rectangle(20000, 20000, 20000, 10000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(30000, 25000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(
            new PointMicrons(35000, 30000),
            ToolMouseButton.Left,
            ToolModifiers.Shift | ToolModifiers.Ctrl);

        Assert.Equal(30000, rect.WidthMicrons);
        Assert.Equal(15000, rect.HeightMicrons);
    }

    [Fact]
    public void Resize_Rectangle_MinimumSize_Enforced()
    {
        var rect = new Rectangle(10000, 10000, 5000, 5000);
        var editor = CreateEditorWithObject(rect);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(15000, 15000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(10100, 10100), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(1000, rect.WidthMicrons);
        Assert.Equal(9900, rect.HeightMicrons);
    }

    [Fact]
    public void Resize_Rectangle_TopRight_ChangesWidthAndTop()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.TopRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(30000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(35000, 8000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(25000, rect.WidthMicrons);
        Assert.Equal(10000, rect.MicronsY);
        Assert.Equal(18000, rect.HeightMicrons);
    }

    [Fact]
    public void Resize_Rectangle_BottomLeft_ChangesXAndHeight()
    {
        var rect = new Rectangle(20000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.BottomLeft;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(20000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(25000, 20000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.True(rect.MicronsX > 20000);
        Assert.True(rect.WidthMicrons < 20000);
        Assert.Equal(10000, rect.HeightMicrons);
    }

    [Fact]
    public void Resize_Rectangle_Top_OnlyHeightChanges()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.Top;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(10000, 15000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(20000, rect.WidthMicrons);
        Assert.Equal(25000, rect.HeightMicrons);
        Assert.Equal(10000, rect.MicronsY);
    }

    [Fact]
    public void Resize_Rectangle_Right_OnlyWidthChanges()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.Right;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(30000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(35000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(25000, rect.WidthMicrons);
        Assert.Equal(20000, rect.HeightMicrons);
        Assert.Equal(10000, rect.MicronsX);
    }

    [Fact]
    public void Resize_Rectangle_Bottom_OnlyHeightChanges()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.Bottom;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 30000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(10000, 35000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(20000, rect.WidthMicrons);
        Assert.Equal(15000, rect.HeightMicrons);
    }

    [Fact]
    public void Resize_Rectangle_Left_PositionAndWidthChange()
    {
        var rect = new Rectangle(20000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.Left;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(20000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(25000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.True(rect.MicronsX >= 20000);
        Assert.True(rect.WidthMicrons <= 20000);
        Assert.Equal(10000, rect.MicronsY);
        Assert.Equal(20000, rect.HeightMicrons);
    }

    // ============================================================
    // OnMouseMove tests — Text resize
    // ============================================================

    [Fact]
    public void Resize_Text_CornerHandle_ResizesFont()
    {
        var text = new Text(10000, 10000, "Test", 3500);
        var editor = CreateEditorWithObject(text);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.BottomLeft;
        editor.SelectSingle(text);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(15000, 12000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(6000, text.FontSizeMicrons);
        Assert.Equal(10000, text.MicronsY);
    }

    [Fact]
    public void Resize_Text_MinimumFontSize_Enforced()
    {
        var text = new Text(10000, 10000, "Test", 3500);
        var editor = CreateEditorWithObject(text);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.TopRight;
        editor.SelectSingle(text);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(10000, 5000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(1000, text.FontSizeMicrons);
    }

    // ============================================================
    // OnMouseMove tests — Line resize
    // ============================================================

    [Fact]
    public void Resize_Line_TopLeft_ChangesStartPoint()
    {
        var line = new Line(10000, 10000, 30000, 30000);
        var editor = CreateEditorWithObject(line);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.TopLeft;
        editor.SelectSingle(line);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(15000, 12000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(15000, line.StartMicronsX);
        Assert.Equal(12000, line.StartMicronsY);
        Assert.Equal(30000, line.EndMicronsX);
        Assert.Equal(30000, line.EndMicronsY);
    }

    // ============================================================
    // OnMouseUp tests
    // ============================================================

    [Fact]
    public void OnMouseUp_PushesResizeCommand_Rectangle()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(20000, 20000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(20000, 20000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(1, editor.CommandHistory.UndoCount);
        Assert.Equal("Изменить размер", editor.CommandHistory.LastUndoName);
    }

    [Fact]
    public void OnMouseUp_PushesResizeCommand_Text()
    {
        var text = new Text(10000, 10000, "Test", 3500);
        var editor = CreateEditorWithObject(text);
        editor.ActiveResizeHandle = ResizeHandle.TopRight;
        editor.SelectSingle(text);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(10000, 15000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(10000, 15000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(1, editor.CommandHistory.UndoCount);
        Assert.Equal("Изменить размер", editor.CommandHistory.LastUndoName);
    }

    [Fact]
    public void OnMouseUp_PushesResizeCommand_Line()
    {
        var line = new Line(10000, 10000, 30000, 30000);
        var editor = CreateEditorWithObject(line);
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(line);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(30000, 30000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(40000, 40000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(40000, 40000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(1, editor.CommandHistory.UndoCount);
        Assert.Equal("Изменить размер", editor.CommandHistory.LastUndoName);
    }

    [Fact]
    public void OnMouseUp_CallsMarkDirty()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(20000, 20000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.False(editor.DirtyStateManager.IsDirty);
        tool.OnMouseUp(new PointMicrons(20000, 20000), ToolMouseButton.Left, ToolModifiers.None);
        Assert.True(editor.DirtyStateManager.IsDirty);
    }

    [Fact]
    public void OnMouseUp_ClearsResizedObject()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(20000, 20000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(20000, 20000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(ToolCursor.Arrow, tool.GetCursor());
    }

    [Fact]
    public void OnMouseUp_WithoutResizing_DoesNothing()
    {
        var editor = CreateEmptyEditor();
        var tool = new ResizeTool(editor);

        tool.OnMouseUp(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(0, editor.CommandHistory.UndoCount);
        Assert.False(editor.DirtyStateManager.IsDirty);
    }

    // ============================================================
    // OnDoubleClick / OnMouseWheel tests
    // ============================================================

    // (duplicates skipped: OnDoubleClick_DoesNothing, OnMouseWheel_DoesNothing)

    // ============================================================
    // GetCursor tests
    // ============================================================

    [Fact]
    public void GetCursor_WhenNotResizing_ReturnsArrow()
    {
        var editor = CreateEmptyEditor();
        var tool = new ResizeTool(editor);

        Assert.Equal(ToolCursor.Arrow, tool.GetCursor());
    }

    [Fact]
    public void GetCursor_TopLeftHandle_ReturnsSizeNWSE()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.TopLeft;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(ToolCursor.SizeNWSE, tool.GetCursor());
    }

    [Fact]
    public void GetCursor_BottomRightHandle_ReturnsSizeNWSE()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(ToolCursor.SizeNWSE, tool.GetCursor());
    }

    [Fact]
    public void GetCursor_TopRightHandle_ReturnsSizeNESW()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.TopRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(ToolCursor.SizeNESW, tool.GetCursor());
    }

    [Fact]
    public void GetCursor_BottomLeftHandle_ReturnsSizeNESW()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.BottomLeft;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(ToolCursor.SizeNESW, tool.GetCursor());
    }

    [Fact]
    public void GetCursor_TopHandle_ReturnsSizeNS()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.Top;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(ToolCursor.SizeNS, tool.GetCursor());
    }

    [Fact]
    public void GetCursor_BottomHandle_ReturnsSizeNS()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.Bottom;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(ToolCursor.SizeNS, tool.GetCursor());
    }

    [Fact]
    public void GetCursor_LeftHandle_ReturnsSizeWE()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.Left;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(ToolCursor.SizeWE, tool.GetCursor());
    }

    [Fact]
    public void GetCursor_RightHandle_ReturnsSizeWE()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.Right;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(ToolCursor.SizeWE, tool.GetCursor());
    }

    // ============================================================
    // VisualCursorForHandle tests (rotation-aware cursor)
    // ============================================================

    [Theory]
    [InlineData(ResizeHandle.TopLeft, 0, ToolCursor.SizeNWSE)]
    [InlineData(ResizeHandle.TopRight, 0, ToolCursor.SizeNESW)]
    [InlineData(ResizeHandle.BottomLeft, 0, ToolCursor.SizeNESW)]
    [InlineData(ResizeHandle.BottomRight, 0, ToolCursor.SizeNWSE)]
    [InlineData(ResizeHandle.TopLeft, 90, ToolCursor.SizeNESW)]
    [InlineData(ResizeHandle.TopRight, 90, ToolCursor.SizeNWSE)]
    [InlineData(ResizeHandle.BottomLeft, 90, ToolCursor.SizeNWSE)]
    [InlineData(ResizeHandle.BottomRight, 90, ToolCursor.SizeNESW)]
    [InlineData(ResizeHandle.TopLeft, 180, ToolCursor.SizeNWSE)]
    [InlineData(ResizeHandle.TopRight, 180, ToolCursor.SizeNESW)]
    [InlineData(ResizeHandle.BottomLeft, 180, ToolCursor.SizeNESW)]
    [InlineData(ResizeHandle.BottomRight, 180, ToolCursor.SizeNWSE)]
    [InlineData(ResizeHandle.TopLeft, 270, ToolCursor.SizeNESW)]
    [InlineData(ResizeHandle.TopRight, 270, ToolCursor.SizeNWSE)]
    [InlineData(ResizeHandle.BottomLeft, 270, ToolCursor.SizeNWSE)]
    [InlineData(ResizeHandle.BottomRight, 270, ToolCursor.SizeNESW)]
    public void VisualCursorForHandle_ReturnsCorrectCursorForRotation(
        ResizeHandle handle, int rotationAngle, ToolCursor expected)
    {
        var text = new Text(10000, 10000, "Test", 3500, rotationAngle: rotationAngle);
        var editor = CreateEditorWithObject(text);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = handle;
        editor.SelectSingle(text);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        var cursor = tool.GetCursor();
        Assert.Equal(expected, cursor);
    }

    [Theory]
    [InlineData(ResizeHandle.Left, 90, ToolCursor.SizeWE)]
    [InlineData(ResizeHandle.Right, 90, ToolCursor.SizeWE)]
    [InlineData(ResizeHandle.Top, 90, ToolCursor.SizeNS)]
    [InlineData(ResizeHandle.Bottom, 90, ToolCursor.SizeNS)]
    public void VisualCursorForHandle_EdgeHandles_UnchangedByRotation(
        ResizeHandle handle, int rotationAngle, ToolCursor expected)
    {
        var text = new Text(10000, 10000, "Test", 3500, rotationAngle: rotationAngle);
        var editor = CreateEditorWithObject(text);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = handle;
        editor.SelectSingle(text);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        var cursor = tool.GetCursor();
        Assert.Equal(expected, cursor);
    }

    // ============================================================
    // Edge case tests
    // ============================================================

    [Fact]
    public void Resize_ToMinimumSize_ThenFurther_StaysAtMinimum()
    {
        var rect = new Rectangle(10000, 10000, 5000, 5000);
        var editor = CreateEditorWithObject(rect);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(15000, 15000), ToolMouseButton.Left, ToolModifiers.None);

        tool.OnMouseMove(new PointMicrons(9000, 9000), ToolMouseButton.Left, ToolModifiers.None);
        Assert.Equal(1000, rect.WidthMicrons);
        Assert.Equal(11000, rect.HeightMicrons);

        tool.OnMouseMove(new PointMicrons(5000, 5000), ToolMouseButton.Left, ToolModifiers.None);
        Assert.Equal(1000, rect.WidthMicrons);
        Assert.Equal(15000, rect.HeightMicrons);
    }

    [Fact]
    public void Resize_LargeDragDistance_HandlesCorrectly()
    {
        var rect = new Rectangle(0, 0, 1000, 1000);
        var editor = CreateEditorWithObject(rect);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(1000, 1000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(1000000, 1000000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(420000, rect.WidthMicrons);
        Assert.Equal(1000, rect.HeightMicrons);
    }

    [Fact]
    public void Resize_NegativeDy_HandlesCorrectly()
    {
        var rect = new Rectangle(10000, 20000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(30000, 40000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(35000, 35000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(25000, rect.WidthMicrons);
        Assert.Equal(25000, rect.HeightMicrons);
    }

    [Fact]
    public void Resize_NegativeDx_HandlesCorrectly()
    {
        var rect = new Rectangle(20000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.Left;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(20000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(15000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(15000, rect.MicronsX);
        Assert.Equal(25000, rect.WidthMicrons);
    }

    [Fact]
    public void MultipleResizeCycles_WorksCorrectly()
    {
        var rect = new Rectangle(10000, 10000, 10000, 10000);
        var editor = CreateEditorWithObject(rect);
        editor.GridSettings.SnapEnabled = false;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);

        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        tool.OnMouseDown(new PointMicrons(20000, 20000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(25000, 25000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(25000, 25000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(15000, rect.WidthMicrons);
        Assert.Equal(5000, rect.HeightMicrons);

        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        tool.OnMouseDown(new PointMicrons(25000, 25000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(30000, 30000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(30000, 30000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(20000, rect.WidthMicrons);
        Assert.Equal(1000, rect.HeightMicrons);
        Assert.Equal(2, editor.CommandHistory.UndoCount);
    }

    [Fact]
    public void Resize_Rectangle_TopLeft_MinimumSizeBoundary()
    {
        var rect = new Rectangle(10000, 10000, 5000, 5000);
        var editor = CreateEditorWithObject(rect);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.TopLeft;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(16000, 16000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.True(rect.MicronsX <= 14000);
        Assert.True(rect.WidthMicrons >= 1000);
        Assert.True(rect.MicronsY <= 14000);
        Assert.True(rect.HeightMicrons >= 1000);
    }

    [Fact]
    public void Resize_Rectangle_ShiftPreservesExactRatio()
    {
        var rect = new Rectangle(0, 0, 10000, 10000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(23456, 30000), ToolMouseButton.Left, ToolModifiers.Shift);

        Assert.Equal(rect.WidthMicrons, rect.HeightMicrons);
    }

    [Fact]
    public void OnMouseUp_AfterOnlyMouseDown_ClearsState()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(1, editor.CommandHistory.UndoCount);
        Assert.True(editor.DirtyStateManager.IsDirty);
        Assert.Equal(ToolCursor.Arrow, tool.GetCursor());
    }

    // === Undo / Redo tests ===

    [Fact]
    public void Undo_Rectangle_RestoresInitialState()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(30000, 30000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(40000, 45000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(40000, 45000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(1, editor.CommandHistory.UndoCount);

        editor.CommandHistory.Undo();

        Assert.Equal(10000, rect.MicronsX);
        Assert.Equal(10000, rect.MicronsY);
        Assert.Equal(20000, rect.WidthMicrons);
        Assert.Equal(20000, rect.HeightMicrons);

        editor.CommandHistory.Redo();

        Assert.Equal(10000, rect.MicronsX);
        Assert.Equal(30000, rect.WidthMicrons);
        Assert.Equal(5000, rect.HeightMicrons);
    }

    [Fact]
    public void Undo_Line_RestoresInitialState()
    {
        var line = new Line(10000, 10000, 30000, 30000);
        var editor = CreateEditorWithObject(line);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(line);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(30000, 30000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(40000, 40000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(40000, 40000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(1, editor.CommandHistory.UndoCount);

        editor.CommandHistory.Undo();

        Assert.Equal(10000, line.StartMicronsX);
        Assert.Equal(10000, line.StartMicronsY);
        Assert.Equal(30000, line.EndMicronsX);
        Assert.Equal(30000, line.EndMicronsY);

        editor.CommandHistory.Redo();

        Assert.Equal(10000, line.StartMicronsX);
        Assert.Equal(10000, line.StartMicronsY);
        Assert.Equal(40000, line.EndMicronsX);
        Assert.Equal(40000, line.EndMicronsY);
    }

    [Fact]
    public void Undo_Text_RestoresInitialState()
    {
        var text = new Text(10000, 10000, "Test", 5000);
        var editor = CreateEditorWithObject(text);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.TopRight;
        editor.SelectSingle(text);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(10000, 15000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(10000, 15000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(1, editor.CommandHistory.UndoCount);

        editor.CommandHistory.Undo();

        Assert.Equal(10000, text.MicronsX);
        Assert.Equal(10000, text.MicronsY);
        Assert.Equal(5000, text.FontSizeMicrons);

        editor.CommandHistory.Redo();

        Assert.Equal(10000, text.MicronsX);
        Assert.Equal(5000, text.MicronsY);
        Assert.Equal(10000, text.FontSizeMicrons);
    }

    [Fact]
    public void Undo_DoesNotThrow_NullReferenceException()
    {
        var rect = new Rectangle(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(rect);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(30000, 30000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(40000, 45000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(40000, 45000), ToolMouseButton.Left, ToolModifiers.None);

        var exception = Record.Exception(() => editor.CommandHistory.Undo());

        Assert.Null(exception);
    }

    // === Sheet bounds clamping ===

    [Fact]
    public void ResizeRectangle_ClampsToSheetRight()
    {
        var rect = new Rectangle(0, 0, 10000, 10000);
        var editor = CreateEditorWithObject(rect);
        var sheetW = editor.Template.Sheet.WidthMicrons;
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(sheetW + 50000, 20000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(sheetW, rect.MicronsX + rect.WidthMicrons);
    }

    [Fact]
    public void ResizeRectangle_ClampsToSheetLeft()
    {
        var rect = new Rectangle(10000, 0, 10000, 10000);
        var editor = CreateEditorWithObject(rect);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.Left;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 5000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(-50000, 5000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(0, rect.MicronsX);
    }

    [Fact]
    public void ResizeRectangle_ClampsToSheetTop()
    {
        var rect = new Rectangle(0, 0, 10000, 10000);
        var editor = CreateEditorWithObject(rect);
        var sheetH = editor.Template.Sheet.HeightMicrons;
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.TopRight;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(20000, sheetH + 50000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(sheetH, rect.MicronsY + rect.HeightMicrons);
    }

    [Fact]
    public void ResizeRectangle_ClampsToSheetBottom()
    {
        var rect = new Rectangle(0, 10000, 10000, 10000);
        var editor = CreateEditorWithObject(rect);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.BottomLeft;
        editor.SelectSingle(rect);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(0, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(-50000, -50000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(0, rect.MicronsY);
    }

    [Fact]
    public void ResizeText_MiddleHandle_ClampsToSheetBounds()
    {
        var text = new Text(10000, 10000, "Test", 5000);
        var editor = CreateEditorWithObject(text);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.Right;
        editor.SelectSingle(text);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(15000, 12500), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(500000, 12500), ToolMouseButton.Left, ToolModifiers.None);

        Assert.True(text.MicronsX <= editor.Template.Sheet.WidthMicrons);
    }

    [Fact]
    public void ResizeText_CornerHandle_ClampsToSheetBounds()
    {
        var text = new Text(10000, 10000, "Test", 5000);
        var editor = CreateEditorWithObject(text);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.TopRight;
        editor.SelectSingle(text);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(15000, 15000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(500000, 500000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.True(text.MicronsX <= editor.Template.Sheet.WidthMicrons);
        Assert.True(text.MicronsY <= editor.Template.Sheet.HeightMicrons);
    }

    [Fact]
    public void ResizeLine_EndHandle_ClampsToSheetBounds()
    {
        var line = new Line(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(line);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.BottomRight;
        editor.SelectSingle(line);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(20000, 20000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(500000, 500000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.True(line.EndMicronsX <= editor.Template.Sheet.WidthMicrons);
        Assert.True(line.EndMicronsY <= editor.Template.Sheet.HeightMicrons);
    }

    [Fact]
    public void ResizeLine_StartHandle_ClampsToSheetBounds()
    {
        var line = new Line(10000, 10000, 20000, 20000);
        var editor = CreateEditorWithObject(line);
        editor.GridSettings.SnapEnabled = false;
        editor.ActiveResizeHandle = ResizeHandle.TopLeft;
        editor.SelectSingle(line);

        var tool = new ResizeTool(editor);
        tool.OnMouseDown(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(-50000, -50000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(0, line.StartMicronsX);
        Assert.Equal(0, line.StartMicronsY);
    }
}
