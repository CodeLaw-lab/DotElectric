using System.Windows;
using System.Windows.Input;
using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tools;
using DotElectric.TemplateEditor.ViewModels;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Tools;

public class SelectToolTests
{
    private static EditorViewModel CreateViewModel()
    {
        var template = new Template();
        var mockService = new Mock<ITemplateService>();
        var mockPrintService = new Mock<IPrintService>();
        var vm = new EditorViewModel(template, mockService.Object, printService: mockPrintService.Object);
        // Отключаем привязку к сетке для тестов
        vm.GridSettings.SnapEnabled = false;
        return vm;
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        var vm = CreateViewModel();
        var tool = new SelectTool(vm);
        Assert.Equal("Выделение", tool.Name);
    }

    [Fact]
    public void GetCursor_ReturnsArrow()
    {
        var vm = CreateViewModel();
        var tool = new SelectTool(vm);
        Assert.Equal(ToolCursor.Arrow, tool.GetCursor());
    }

    [Fact]
    public void OnMouseDown_LeftClickOnObject_SelectsIt()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 10000, 10000);
        vm.Template.Objects.Add(line);

        var tool = new SelectTool(vm);
        tool.OnMouseDown(new PointMicrons(500, 500), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Single(vm.SelectedObjects);
        Assert.Same(line, vm.SelectedObjects[0]);
    }

    [Fact]
    public void OnMouseDown_LeftClickOnEmpty_ClearsSelection()
    {
        var vm = CreateViewModel();
        var line = new Line(500, 500, 1500, 1500);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        var tool = new SelectTool(vm);
        tool.OnMouseDown(new PointMicrons(9000, 9000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Empty(vm.SelectedObjects);
    }

    [Fact]
    public void OnMouseDown_ShiftClick_AddsToSelection()
    {
        var vm = CreateViewModel();
        // Координаты достаточно далеко друг от друга (вне tolerance 5мм = 5000 микрон)
        var line1 = new Line(0, 0, 1000, 1000);
        var line2 = new Line(50000, 50000, 51000, 51000);
        vm.Template.Objects.Add(line1);
        vm.Template.Objects.Add(line2);

        var tool = new SelectTool(vm);
        tool.OnMouseDown(new PointMicrons(500, 500), ToolMouseButton.Left, ToolModifiers.None);
        Assert.Single(vm.SelectedObjects); // line1 selected

        tool.OnMouseDown(new PointMicrons(50500, 50500), ToolMouseButton.Left, ToolModifiers.Shift);
        Assert.Equal(2, vm.SelectedObjects.Count);
    }

    [Fact]
    public void OnMouseDown_CtrlClick_RemovesFromSelection()
    {
        var vm = CreateViewModel();
        // Длинная линия — клик в середине далеко от маркеров
        var line = new Line(0, 0, 50000, 50000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        var tool = new SelectTool(vm);
        // Клик в середину — до маркера (0,0) = 35355 > 8000
        tool.OnMouseDown(new PointMicrons(25000, 25000), ToolMouseButton.Left, ToolModifiers.Ctrl);

        Assert.Empty(vm.SelectedObjects);
    }

    [Fact]
    public void OnMouseWheel_DoesNotThrow()
    {
        var vm = CreateViewModel();
        var tool = new SelectTool(vm);
        tool.OnMouseWheel(120, new PointMicrons(0, 0)); // should not throw
    }

    // ============= OnDoubleClick =============

    [Fact]
    public void OnDoubleClick_OnTextObject_StartsInlineEditing()
    {
        var vm = CreateViewModel();
        var text = new Text(1000, 1000, "Hello", 3500, isEditable: true) { RotationAngle = 0 };
        vm.Template.Objects.Add(text);
        vm.SelectSingle(text);

        var tool = new SelectTool(vm);
        tool.OnDoubleClick(new PointMicrons(1000, 1000));

        Assert.NotNull(vm.InlineEditManager.InlineEditingText);
    }

    [Fact]
    public void OnDoubleClick_OnNonEditableText_DoesNotStartInlineEditing()
    {
        var vm = CreateViewModel();
        var text = new Text(1000, 1000, "Hello", 3500, isEditable: false) { RotationAngle = 0 };
        vm.Template.Objects.Add(text);
        vm.SelectSingle(text);

        var tool = new SelectTool(vm);
        tool.OnDoubleClick(new PointMicrons(1000, 1000));

        Assert.Null(vm.InlineEditManager.InlineEditingText);
    }

    [Fact]
    public void OnDoubleClick_OnLineObject_DoesNotStartInlineEditing()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 10000, 10000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        var tool = new SelectTool(vm);
        tool.OnDoubleClick(new PointMicrons(5000, 5000));

        Assert.Null(vm.InlineEditManager.InlineEditingText);
    }

    [Fact]
    public void OnDoubleClick_OnRectangleObject_DoesNotStartInlineEditing()
    {
        var vm = CreateViewModel();
        var rect = new Rectangle(0, 0, 10000, 10000);
        vm.Template.Objects.Add(rect);
        vm.SelectSingle(rect);

        var tool = new SelectTool(vm);
        tool.OnDoubleClick(new PointMicrons(5000, 5000));

        Assert.Null(vm.InlineEditManager.InlineEditingText);
    }

    [Fact]
    public void OnDoubleClick_OnEmptyArea_DoesNothing()
    {
        var vm = CreateViewModel();

        var tool = new SelectTool(vm);
        var ex = Record.Exception(() =>
            tool.OnDoubleClick(new PointMicrons(99999, 99999)));

        Assert.Null(ex);
        Assert.Empty(vm.SelectedObjects);
    }

    // ============= OnKeyDown =============

    [Fact]
    public void OnKeyDown_Delete_DeletesSelectedObject()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 10000, 10000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);
        Assert.Single(vm.Template.Objects);

        var tool = new SelectTool(vm);
        var handled = tool.OnKeyDown(ToolKey.Delete, ToolModifiers.None);

        Assert.True(handled);
        Assert.Empty(vm.Template.Objects);
    }

    [Fact]
    public void OnKeyDown_Delete_DeletesMultipleSelectedObjects()
    {
        var vm = CreateViewModel();
        var line1 = new Line(0, 0, 10000, 10000);
        var line2 = new Line(20000, 20000, 30000, 30000);
        vm.Template.Objects.Add(line1);
        vm.Template.Objects.Add(line2);
        vm.SelectedObjects.Add(line1);
        vm.SelectedObjects.Add(line2);
        Assert.Equal(2, vm.Template.Objects.Count);

        var tool = new SelectTool(vm);
        var handled = tool.OnKeyDown(ToolKey.Delete, ToolModifiers.None);

        Assert.True(handled);
        Assert.Empty(vm.Template.Objects);
    }

    [Fact]
    public void OnKeyDown_Delete_WithNoSelection_DoesNotThrow()
    {
        var vm = CreateViewModel();

        var tool = new SelectTool(vm);
        var ex = Record.Exception(() => tool.OnKeyDown(ToolKey.Delete, ToolModifiers.None));

        Assert.Null(ex);
    }

    [Fact]
    public void OnKeyDown_Delete_CreatesUndoableCommand()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 10000, 10000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        var tool = new SelectTool(vm);
        tool.OnKeyDown(ToolKey.Delete, ToolModifiers.None);

        Assert.True(vm.CommandHistory.CanUndo);

        vm.CommandHistory.Undo();
        Assert.Single(vm.Template.Objects);
    }

    [Fact]
    public void OnKeyDown_Escape_ClearsSelectionAndHover()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 10000, 10000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);
        vm.HoveredObject = line;

        var tool = new SelectTool(vm);
        var handled = tool.OnKeyDown(ToolKey.Escape, ToolModifiers.None);

        Assert.True(handled);
        Assert.Empty(vm.SelectedObjects);
        Assert.Null(vm.HoveredObject);
        Assert.Null(vm.HoveredHandle);
    }

    [Fact]
    public void OnKeyDown_Escape_ResetsSelectionBoxState()
    {
        var vm = CreateViewModel();

        var tool = new SelectTool(vm);
        tool.OnKeyDown(ToolKey.Escape, ToolModifiers.None);

        Assert.Equal(0, vm.SelectionBoxLeft);
        Assert.Equal(0, vm.SelectionBoxBottom);
        Assert.Equal(0, vm.SelectionBoxWidth);
        Assert.Equal(0, vm.SelectionBoxHeight);
        Assert.Equal(SelectionDirection.LeftToRight, vm.SelectionDirection);
    }

    [Fact]
    public void OnKeyDown_UnknownKey_ReturnsFalse()
    {
        var vm = CreateViewModel();
        var tool = new SelectTool(vm);

        var handled = tool.OnKeyDown(ToolKey.Enter, ToolModifiers.None);

        Assert.False(handled);
    }

    // ============= Selection Box =============

    [Fact]
    public void OnMouseDown_OnEmptyArea_StartsSelectionBox()
    {
        var vm = CreateViewModel();
        var tool = new SelectTool(vm);

        tool.OnMouseDown(new PointMicrons(1000, 1000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Empty(vm.SelectedObjects);
    }

    [Fact]
    public void SelectionBox_MouseMoveLessThanThreshold_DoesNotShowBox()
    {
        var vm = CreateViewModel();
        var tool = new SelectTool(vm);

        tool.OnMouseDown(new PointMicrons(1000, 1000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(1100, 1100), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(0, vm.SelectionBoxWidth);
    }

    [Fact]
    public void SelectionBox_MouseMoveExceedsThreshold_ShowsBox()
    {
        var vm = CreateViewModel();
        var tool = new SelectTool(vm);

        tool.OnMouseDown(new PointMicrons(1000, 1000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(10000, 8000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.True(vm.SelectionBoxWidth > 0);
        Assert.True(vm.SelectionBoxHeight > 0);
        Assert.Equal(1000, vm.SelectionBoxLeft);
        Assert.Equal(1000, vm.SelectionBoxBottom);
    }

    [Fact]
    public void SelectionBox_MouseMoveRightToLeft_SetsDirection()
    {
        var vm = CreateViewModel();
        var tool = new SelectTool(vm);

        tool.OnMouseDown(new PointMicrons(10000, 1000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(1000, 8000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(SelectionDirection.RightToLeft, vm.SelectionDirection);
    }

    [Fact]
    public void SelectionBox_MouseUpLessThanThreshold_ClearsBoxWithoutSelecting()
    {
        var vm = CreateViewModel();
        var obj = new Line(0, 0, 10000, 10000);
        vm.Template.Objects.Add(obj);

        var tool = new SelectTool(vm);
        tool.OnMouseDown(new PointMicrons(50000, 50000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(50100, 50100), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(50100, 50100), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(0, vm.SelectionBoxWidth);
        Assert.Equal(0, vm.SelectionBoxHeight);
        Assert.Empty(vm.SelectedObjects);
    }

    [Fact]
    public void SelectionBox_SelectsObjectsInsideBox()
    {
        var vm = CreateViewModel();
        var obj1 = new Line(0, 0, 10000, 10000);
        var obj2 = new Line(50000, 50000, 60000, 60000);
        vm.Template.Objects.Add(obj1);
        vm.Template.Objects.Add(obj2);

        var tool = new SelectTool(vm);
        tool.OnMouseDown(new PointMicrons(-1000, -1000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(20000, 20000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(20000, 20000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Single(vm.SelectedObjects);
        if (vm.SelectedObjects.Count > 0)
            Assert.Same(obj1, vm.SelectedObjects[0]);
    }

    [Fact]
    public void SelectionBox_Finalize_ClearsPreviewBox()
    {
        var vm = CreateViewModel();
        var tool = new SelectTool(vm);

        tool.OnMouseDown(new PointMicrons(1000, 1000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(0, vm.SelectionBoxWidth);
        Assert.Equal(0, vm.SelectionBoxHeight);
        Assert.Equal(SelectionDirection.LeftToRight, vm.SelectionDirection);
    }

    // ============= Reset =============

    [Fact]
    public void Reset_ClearsDragState()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 10000, 10000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        var tool = new SelectTool(vm);
        tool.OnMouseDown(new PointMicrons(5000, 5000), ToolMouseButton.Left, ToolModifiers.None);

        tool.Reset();

        Assert.Equal(0, vm.SelectionBoxWidth);
        Assert.Equal(0, vm.SelectionBoxHeight);
    }

    // ============= Hover state =============

    [Fact]
    public void Cursor_WhenHoveringObject_ReturnsHand()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 10000, 10000);
        vm.Template.Objects.Add(line);

        var tool = new SelectTool(vm);
        vm.HoveredObject = line;

        Assert.Equal(ToolCursor.Hand, tool.GetCursor());
    }

    [Fact]
    public void Cursor_WhenHoveringHandle_ReturnsCross()
    {
        var vm = CreateViewModel();
        var line = new Line(0, 0, 10000, 10000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        var tool = new SelectTool(vm);
        vm.HoveredHandle = ResizeHandle.TopLeft;

        Assert.Equal(ToolCursor.Cross, tool.GetCursor());
    }

    [Fact]
    public void Cursor_Default_ReturnsArrow()
    {
        var vm = CreateViewModel();
        var tool = new SelectTool(vm);

        Assert.Equal(ToolCursor.Arrow, tool.GetCursor());
    }
}

public class DrawingLineToolTests
{
    private static EditorViewModel CreateViewModel()
    {
        var template = new Template();
        var mockService = new Mock<ITemplateService>();
        var mockPrintService = new Mock<IPrintService>();
        var vm = new EditorViewModel(template, mockService.Object, printService: mockPrintService.Object);
        vm.GridSettings.SnapEnabled = false;
        return vm;
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        var vm = CreateViewModel();
        var tool = new DrawingLineTool(vm);
        Assert.Equal("Линия", tool.Name);
    }

    [Fact]
    public void GetCursor_ReturnsCross()
    {
        var vm = CreateViewModel();
        var tool = new DrawingLineTool(vm);
        Assert.Equal(ToolCursor.Cross, tool.GetCursor());
    }

    [Fact]
    public void FirstClick_CreatesPreviewLine()
    {
        var vm = CreateViewModel();
        var tool = new DrawingLineTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.NotNull(vm.PreviewLine);
        Assert.Equal(1000, vm.PreviewLine.StartMicronsX);
        Assert.Equal(2000, vm.PreviewLine.StartMicronsY);
    }

    [Fact]
    public void MouseUp_CreatesLineAndClearsPreview()
    {
        var vm = CreateViewModel();
        var tool = new DrawingLineTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(3000, 4000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Single(vm.Template.Objects);
        Assert.Null(vm.PreviewLine);

        var line = (Line)vm.Template.Objects[0];
        Assert.Equal(1000, line.StartMicronsX);
        Assert.Equal(2000, line.StartMicronsY);
        Assert.Equal(3000, line.EndMicronsX);
        Assert.Equal(4000, line.EndMicronsY);
    }

    [Fact]
    public void MouseMove_UpdatesPreviewLine()
    {
        var vm = CreateViewModel();
        var tool = new DrawingLineTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(5000, 6000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.NotNull(vm.PreviewLine);
        Assert.Equal(5000, vm.PreviewLine.EndMicronsX);
        Assert.Equal(6000, vm.PreviewLine.EndMicronsY);
    }

    [Fact]
    public void DoubleClick_CancelsCurrentLine()
    {
        var vm = CreateViewModel();
        var tool = new DrawingLineTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnDoubleClick(new PointMicrons(5000, 6000));

        Assert.Null(vm.PreviewLine);
        Assert.Empty(vm.Template.Objects);
    }

    [Fact]
    public void ShiftModifier_ConstrainsToHorizontal()
    {
        var vm = CreateViewModel();
        var tool = new DrawingLineTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);
        // Move more vertically but Shift should constrain to horizontal
        // dx=9000, dy=100 → dx > dy+5000 → horizontal
        tool.OnMouseMove(new PointMicrons(10000, 2100), ToolMouseButton.Left, ToolModifiers.Shift);

        Assert.Equal(2000, vm.PreviewLine!.EndMicronsY); // Y stays at start
    }

    [Fact]
    public void ShiftModifier_ConstrainsToVertical()
    {
        var vm = CreateViewModel();
        var tool = new DrawingLineTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);
        // Move more horizontally but Shift should constrain to vertical
        // dx=100, dy=8000 → dy > dx+5000 → vertical
        tool.OnMouseMove(new PointMicrons(1100, 10000), ToolMouseButton.Left, ToolModifiers.Shift);

        Assert.Equal(1000, vm.PreviewLine!.EndMicronsX); // X stays at start
    }

    [Fact]
    public void ZeroLengthLine_MouseUp_DoesNotCreate()
    {
        var vm = CreateViewModel();
        var tool = new DrawingLineTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Empty(vm.Template.Objects);
    }

    [Fact]
    public void MouseUp_WithoutMouseDown_DoesNothing()
    {
        var vm = CreateViewModel();
        var tool = new DrawingLineTool(vm);
        tool.OnMouseUp(new PointMicrons(3000, 4000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Empty(vm.Template.Objects);
        Assert.Null(vm.PreviewLine);
    }

    [Fact]
    public void RightClick_Ignored()
    {
        var vm = CreateViewModel();
        var tool = new DrawingLineTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Right, ToolModifiers.None);

        Assert.Null(vm.PreviewLine);
        Assert.Empty(vm.Template.Objects);
    }

    [Fact]
    public void OnMouseUp_ClampsStartToSheet()
    {
        var vm = CreateViewModel();
        var tool = new DrawingLineTool(vm);
        // Start outside sheet (negative X)
        tool.OnMouseDown(new PointMicrons(-5000, -5000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(10000, 10000), ToolMouseButton.Left, ToolModifiers.None);

        var line = vm.Template.Objects.OfType<Line>().FirstOrDefault();
        Assert.NotNull(line);
        Assert.Equal(0, line.StartMicronsX);
        Assert.Equal(0, line.StartMicronsY);
    }

    [Fact]
    public void OnMouseUp_ClampsEndToSheet()
    {
        var vm = CreateViewModel();
        var sheetW = vm.Template.Sheet.WidthMicrons;
        var sheetH = vm.Template.Sheet.HeightMicrons;
        var tool = new DrawingLineTool(vm);
        // End outside sheet (far beyond bounds)
        tool.OnMouseDown(new PointMicrons(0, 0), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(sheetW + 50000, sheetH + 50000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(sheetW + 50000, sheetH + 50000), ToolMouseButton.Left, ToolModifiers.None);

        var line = vm.Template.Objects.OfType<Line>().FirstOrDefault();
        Assert.NotNull(line);
        Assert.Equal(sheetW, line.EndMicronsX);
        Assert.Equal(sheetH, line.EndMicronsY);
    }

    // ============= Reset =============

    [Fact]
    public void Reset_ClearsPreview_PreviewLineBecomesNull()
    {
        var vm = CreateViewModel();
        var tool = new DrawingLineTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);
        Assert.NotNull(vm.PreviewLine);

        tool.Reset();

        Assert.Null(vm.PreviewLine);
    }

    [Fact]
    public void Reset_AfterMouseDown_ClearsState()
    {
        var vm = CreateViewModel();
        var tool = new DrawingLineTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);

        tool.Reset();

        // After Reset, MouseMove should not update preview
        tool.OnMouseMove(new PointMicrons(5000, 6000), ToolMouseButton.Left, ToolModifiers.None);
        Assert.Null(vm.PreviewLine);

        // After Reset, MouseUp should not create an object
        tool.OnMouseUp(new PointMicrons(5000, 6000), ToolMouseButton.Left, ToolModifiers.None);
        Assert.Empty(vm.Template.Objects);
    }

    [Fact]
    public void Reset_CanBeCalledMultipleTimes_DoesNotThrow()
    {
        var vm = CreateViewModel();
        var tool = new DrawingLineTool(vm);

        var ex = Record.Exception(() =>
        {
            tool.Reset();
            tool.Reset();
            tool.Reset();
        });

        Assert.Null(ex);
    }

    // ============= OnKeyDown =============

    [Fact]
    public void OnKeyDown_Escape_ReturnsTrueAndSwitchesToSelect()
    {
        var vm = CreateViewModel();
        var tool = new DrawingLineTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);
        Assert.NotNull(vm.PreviewLine);

        var result = tool.OnKeyDown(ToolKey.Escape, ToolModifiers.None);

        Assert.True(result);
        Assert.Null(vm.PreviewLine);
        Assert.Equal("Select", vm.ToolManager.ActiveTool);
    }

    [Fact]
    public void OnKeyDown_NonEscapeKey_ReturnsFalse()
    {
        var vm = CreateViewModel();
        var tool = new DrawingLineTool(vm);

        var result = tool.OnKeyDown(ToolKey.Enter, ToolModifiers.None);

        Assert.False(result);
    }

    // ============= OnMouseWheel =============

    [Fact]
    public void OnMouseWheel_ReturnsFalse_DoesNotBlockZoom()
    {
        var vm = CreateViewModel();
        var tool = new DrawingLineTool(vm);

        var result = tool.OnMouseWheel(120, new PointMicrons(0, 0));

        Assert.False(result);
    }
}

public class DrawingRectangleToolTests
{
    private static EditorViewModel CreateViewModel()
    {
        var template = new Template();
        var mockService = new Mock<ITemplateService>();
        var mockPrintService = new Mock<IPrintService>();
        var vm = new EditorViewModel(template, mockService.Object, printService: mockPrintService.Object);
        vm.GridSettings.SnapEnabled = false;
        return vm;
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        var vm = CreateViewModel();
        var tool = new DrawingRectangleTool(vm);
        Assert.Equal("Прямоугольник", tool.Name);
    }

    [Fact]
    public void GetCursor_ReturnsCross()
    {
        var vm = CreateViewModel();
        var tool = new DrawingRectangleTool(vm);
        Assert.Equal(ToolCursor.Cross, tool.GetCursor());
    }

    [Fact]
    public void FirstClick_CreatesPreviewRectangle()
    {
        var vm = CreateViewModel();
        var tool = new DrawingRectangleTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.NotNull(vm.PreviewRectangle);
        Assert.Equal(1000, vm.PreviewRectangle.MicronsX);
        Assert.Equal(2000, vm.PreviewRectangle.MicronsY);
    }

    [Fact]
    public void MouseUp_CreatesRectangleAndClearsPreview()
    {
        var vm = CreateViewModel();
        var tool = new DrawingRectangleTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(5000, 6000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Single(vm.Template.Objects);
        Assert.Null(vm.PreviewRectangle);

        var rect = (Rectangle)vm.Template.Objects[0];
        Assert.Equal(1000, rect.MicronsX);
        Assert.Equal(2000, rect.MicronsY);
        Assert.Equal(4000, rect.WidthMicrons);
        Assert.Equal(4000, rect.HeightMicrons);
    }

    [Fact]
    public void MouseMove_UpdatesPreviewRectangle()
    {
        var vm = CreateViewModel();
        var tool = new DrawingRectangleTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(5000, 6000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.NotNull(vm.PreviewRectangle);
        Assert.Equal(4000, vm.PreviewRectangle.WidthMicrons);
        Assert.Equal(4000, vm.PreviewRectangle.HeightMicrons);
    }

    [Fact]
    public void DoubleClick_CancelsCurrentRectangle()
    {
        var vm = CreateViewModel();
        var tool = new DrawingRectangleTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnDoubleClick(new PointMicrons(5000, 6000));

        Assert.Null(vm.PreviewRectangle);
        Assert.Empty(vm.Template.Objects);
    }

    [Fact]
    public void ZeroSizeRectangle_MouseUp_DoesNotCreate()
    {
        var vm = CreateViewModel();
        var tool = new DrawingRectangleTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Empty(vm.Template.Objects);
    }

    [Fact]
    public void RightClick_Ignored()
    {
        var vm = CreateViewModel();
        var tool = new DrawingRectangleTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Right, ToolModifiers.None);

        Assert.Null(vm.PreviewRectangle);
    }

    [Fact]
    public void ShiftModifier_CreatesSquare()
    {
        var vm = CreateViewModel();
        var tool = new DrawingRectangleTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(5000, 4000), ToolMouseButton.Left, ToolModifiers.Shift);
        tool.OnMouseUp(new PointMicrons(5000, 4000), ToolMouseButton.Left, ToolModifiers.Shift);

        var rect = (Rectangle)vm.Template.Objects[0];
        Assert.Equal(rect.WidthMicrons, rect.HeightMicrons);
    }

    [Fact]
    public void CtrlModifier_CreatesFromCenter()
    {
        var vm = CreateViewModel();
        var tool = new DrawingRectangleTool(vm);
        tool.OnMouseDown(new PointMicrons(5000, 5000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(7000, 6000), ToolMouseButton.Left, ToolModifiers.Ctrl);
        tool.OnMouseUp(new PointMicrons(7000, 6000), ToolMouseButton.Left, ToolModifiers.Ctrl);

        var rect = (Rectangle)vm.Template.Objects[0];
        // Ctrl: от центра — x = start - |dx|, y = start - |dy|, w = 2*|dx|, h = 2*|dy|
        Assert.Equal(3000, rect.MicronsX); // 5000 - |7000-5000| = 3000
        Assert.Equal(4000, rect.MicronsY); // 5000 - |6000-5000| = 4000
        Assert.Equal(4000, rect.WidthMicrons); // 2 * |7000-5000| = 4000
        Assert.Equal(2000, rect.HeightMicrons); // 2 * |6000-5000| = 2000
    }

    [Fact]
    public void CtrlShiftModifier_CreatesSquareFromCenter()
    {
        var vm = CreateViewModel();
        var tool = new DrawingRectangleTool(vm);
        tool.OnMouseDown(new PointMicrons(5000, 5000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(7000, 6000), ToolMouseButton.Left, ToolModifiers.Ctrl | ToolModifiers.Shift);
        tool.OnMouseUp(new PointMicrons(7000, 6000), ToolMouseButton.Left, ToolModifiers.Ctrl | ToolModifiers.Shift);

        var rect = (Rectangle)vm.Template.Objects[0];
        Assert.Equal(rect.WidthMicrons, rect.HeightMicrons); // Square
    }

    [Fact]
    public void OnMouseUp_ClampsToSheetBounds()
    {
        var vm = CreateViewModel();
        var sheetW = vm.Template.Sheet.WidthMicrons;
        var sheetH = vm.Template.Sheet.HeightMicrons;
        var tool = new DrawingRectangleTool(vm);
        // Start inside, end far outside → clamped
        tool.OnMouseDown(new PointMicrons(0, 0), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(sheetW + 50000, sheetH + 50000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(sheetW + 50000, sheetH + 50000), ToolMouseButton.Left, ToolModifiers.None);

        var rect = (Rectangle)vm.Template.Objects[0];
        Assert.Equal(0, rect.MicronsX);
        Assert.Equal(0, rect.MicronsY);
        Assert.Equal(sheetW, rect.MicronsX + rect.WidthMicrons);
        Assert.Equal(sheetH, rect.MicronsY + rect.HeightMicrons);
    }

    [Fact]
    public void OnMouseUp_ClampsNegativeStartToSheet()
    {
        var vm = CreateViewModel();
        var tool = new DrawingRectangleTool(vm);
        // Start outside sheet (negative), end inside
        tool.OnMouseDown(new PointMicrons(-50000, -50000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(5000, 5000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(5000, 5000), ToolMouseButton.Left, ToolModifiers.None);

        var rect = (Rectangle)vm.Template.Objects[0];
        Assert.Equal(0, rect.MicronsX);
        Assert.Equal(0, rect.MicronsY);
        Assert.True(rect.WidthMicrons > 0);
        Assert.True(rect.HeightMicrons > 0);
    }

    // ============= Reset =============

    [Fact]
    public void Reset_ClearsPreview_PreviewRectangleBecomesNull()
    {
        var vm = CreateViewModel();
        var tool = new DrawingRectangleTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);
        Assert.NotNull(vm.PreviewRectangle);

        tool.Reset();

        Assert.Null(vm.PreviewRectangle);
    }

    [Fact]
    public void Reset_AfterMouseDown_ClearsState()
    {
        var vm = CreateViewModel();
        var tool = new DrawingRectangleTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);

        tool.Reset();

        // After Reset, MouseMove should not update preview
        tool.OnMouseMove(new PointMicrons(5000, 6000), ToolMouseButton.Left, ToolModifiers.None);
        Assert.Null(vm.PreviewRectangle);

        // After Reset, MouseUp should not create an object
        tool.OnMouseUp(new PointMicrons(5000, 6000), ToolMouseButton.Left, ToolModifiers.None);
        Assert.Empty(vm.Template.Objects);
    }

    [Fact]
    public void Reset_CanBeCalledMultipleTimes_DoesNotThrow()
    {
        var vm = CreateViewModel();
        var tool = new DrawingRectangleTool(vm);

        var ex = Record.Exception(() =>
        {
            tool.Reset();
            tool.Reset();
            tool.Reset();
        });

        Assert.Null(ex);
    }

    // ============= OnKeyDown =============

    [Fact]
    public void OnKeyDown_Escape_ClearsPreviewAndSwitchesToSelect()
    {
        var vm = CreateViewModel();
        var tool = new DrawingRectangleTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);
        Assert.NotNull(vm.PreviewRectangle);

        var result = tool.OnKeyDown(ToolKey.Escape, ToolModifiers.None);

        Assert.True(result);
        Assert.Null(vm.PreviewRectangle);
        Assert.Equal("Select", vm.ToolManager.ActiveTool);
    }

    [Fact]
    public void OnKeyDown_NonEscapeKey_ReturnsFalse()
    {
        var vm = CreateViewModel();
        var tool = new DrawingRectangleTool(vm);

        var result = tool.OnKeyDown(ToolKey.Enter, ToolModifiers.None);

        Assert.False(result);
    }

    // ============= OnMouseWheel =============

    [Fact]
    public void OnMouseWheel_ReturnsFalse_DoesNotBlockZoom()
    {
        var vm = CreateViewModel();
        var tool = new DrawingRectangleTool(vm);

        var result = tool.OnMouseWheel(120, new PointMicrons(0, 0));

        Assert.False(result);
    }
}

public class TextToolTests
{
    private static EditorViewModel CreateViewModel()
    {
        var template = new Template();
        var mockService = new Mock<ITemplateService>();
        var mockPrintService = new Mock<IPrintService>();
        var vm = new EditorViewModel(template, mockService.Object, printService: mockPrintService.Object);
        vm.GridSettings.SnapEnabled = false;
        return vm;
    }

    [Fact]
    public void Name_ReturnsCorrectValue()
    {
        var vm = CreateViewModel();
        var tool = new TextTool(vm);
        Assert.Equal("Текст", tool.Name);
    }

    [Fact]
    public void GetCursor_ReturnsIBeam()
    {
        var vm = CreateViewModel();
        var tool = new TextTool(vm);
        Assert.Equal(ToolCursor.IBeam, tool.GetCursor());
    }

    [Fact]
    public void LeftClick_CreatesTextObject()
    {
        var vm = CreateViewModel();
        var tool = new TextTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Single(vm.Template.Objects);
        var text = (Text)vm.Template.Objects[0];
        Assert.Equal(1000, text.MicronsX);
        Assert.Equal(2000, text.MicronsY);
        Assert.Equal("Текст", text.Content);
    }

    [Fact]
    public void LeftClick_SelectsNewText()
    {
        var vm = CreateViewModel();
        var tool = new TextTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);

        Assert.Single(vm.SelectedObjects);
        Assert.IsType<Text>(vm.SelectedObjects[0]);
    }

    [Fact]
    public void RightClick_Ignored()
    {
        var vm = CreateViewModel();
        var tool = new TextTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Right, ToolModifiers.None);

        Assert.Empty(vm.Template.Objects);
    }

    [Fact]
    public void SetFontSize_UpdatesFontSize()
    {
        var vm = CreateViewModel();
        var tool = new TextTool(vm);
        tool.SetFontSize(5000);
        tool.OnMouseDown(new PointMicrons(0, 0), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(0, 0), ToolMouseButton.Left, ToolModifiers.None);

        var text = (Text)vm.Template.Objects[0];
        Assert.Equal(5000, text.FontSizeMicrons);
    }

    [Fact]
    public void SetDefaultContent_UpdatesContent()
    {
        var vm = CreateViewModel();
        var tool = new TextTool(vm);
        tool.SetDefaultContent("My Custom Text");
        tool.OnMouseDown(new PointMicrons(0, 0), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(0, 0), ToolMouseButton.Left, ToolModifiers.None);

        var text = (Text)vm.Template.Objects[0];
        Assert.Equal("My Custom Text", text.Content);
    }

    [Fact]
    public void SetTextType_UpdatesType()
    {
        var vm = CreateViewModel();
        var tool = new TextTool(vm);
        tool.SetTextType(TextType.Dimension);
        tool.OnMouseDown(new PointMicrons(0, 0), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(0, 0), ToolMouseButton.Left, ToolModifiers.None);

        var text = (Text)vm.Template.Objects[0];
        Assert.Equal(TextType.Dimension, text.TextType);
    }

    // ============= Reset =============

    [Fact]
    public void Reset_ClearsPreview_PreviewTextBecomesNull()
    {
        var vm = CreateViewModel();
        var tool = new TextTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);
        Assert.NotNull(vm.PreviewText);

        tool.Reset();

        Assert.Null(vm.PreviewText);
    }

    [Fact]
    public void Reset_AfterMouseDown_ClearsState()
    {
        var vm = CreateViewModel();
        var tool = new TextTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);

        tool.Reset();

        // After Reset, MouseMove should not update preview
        tool.OnMouseMove(new PointMicrons(5000, 6000), ToolMouseButton.Left, ToolModifiers.None);
        Assert.Null(vm.PreviewText);

        // After Reset, MouseUp should not create an object
        tool.OnMouseUp(new PointMicrons(5000, 6000), ToolMouseButton.Left, ToolModifiers.None);
        Assert.Empty(vm.Template.Objects);
    }

    [Fact]
    public void Reset_CanBeCalledMultipleTimes_DoesNotThrow()
    {
        var vm = CreateViewModel();
        var tool = new TextTool(vm);

        var ex = Record.Exception(() =>
        {
            tool.Reset();
            tool.Reset();
            tool.Reset();
        });

        Assert.Null(ex);
    }

    // ============= OnKeyDown =============

    [Fact]
    public void OnKeyDown_Escape_ReturnsTrueAndSwitchesToSelect()
    {
        var vm = CreateViewModel();
        var tool = new TextTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);
        Assert.NotNull(vm.PreviewText);

        var result = tool.OnKeyDown(ToolKey.Escape, ToolModifiers.None);

        Assert.True(result);
        Assert.Null(vm.PreviewText);
        Assert.Equal("Select", vm.ToolManager.ActiveTool);
    }

    [Fact]
    public void OnKeyDown_NonEscapeKey_ReturnsFalse()
    {
        var vm = CreateViewModel();
        var tool = new TextTool(vm);

        var result = tool.OnKeyDown(ToolKey.Enter, ToolModifiers.None);

        Assert.False(result);
    }

    // ============= OnDoubleClick =============

    [Fact]
    public void OnDoubleClick_ClearsPreviewAndSwitchesToSelect()
    {
        var vm = CreateViewModel();
        var tool = new TextTool(vm);
        tool.OnMouseDown(new PointMicrons(1000, 2000), ToolMouseButton.Left, ToolModifiers.None);
        Assert.NotNull(vm.PreviewText);

        tool.OnDoubleClick(new PointMicrons(5000, 6000));

        Assert.Null(vm.PreviewText);
        Assert.Equal("Select", vm.ToolManager.ActiveTool);
    }

    // ============= OnMouseWheel =============

    [Fact]
    public void OnMouseWheel_ReturnsFalse_DoesNotBlockZoom()
    {
        var vm = CreateViewModel();
        var tool = new TextTool(vm);

        var result = tool.OnMouseWheel(120, new PointMicrons(0, 0));

        Assert.False(result);
    }
}

public class SelectToolDragTests
{
    private static EditorViewModel CreateViewModel()
    {
        var template = new Template();
        var mockService = new Mock<ITemplateService>();
        var mockPrintService = new Mock<IPrintService>();
        var vm = new EditorViewModel(template, mockService.Object, printService: mockPrintService.Object);
        vm.GridSettings.SnapEnabled = false;
        return vm;
    }

    [Fact]
    public void Drag_SingleSelection_UndoRestoresPosition()
    {
        var vm = CreateViewModel();
        // Используем Line — маркеры только на концах, клик в середину = drag тела
        var line = new Line(10000, 10000, 30000, 30000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        var tool = new SelectTool(vm);

        // MouseDown в середину линии (НЕ на маркере) — маркеры на (10000,10000) и (30000,30000)
        tool.OnMouseDown(new PointMicrons(20000, 20000), ToolMouseButton.Left, ToolModifiers.None);

        // MouseMove — сдвигаем на 5мм вправо и 3мм вверх
        tool.OnMouseMove(new PointMicrons(25000, 23000), ToolMouseButton.Left, ToolModifiers.None);

        // Линия перемещена (начало сдвинулось на 5000, 3000)
        Assert.Equal(15000, line.StartMicronsX);
        Assert.Equal(13000, line.StartMicronsY);

        // MouseUp — фиксируем
        tool.OnMouseUp(new PointMicrons(25000, 23000), ToolMouseButton.Left, ToolModifiers.None);

        // Undo
        vm.CommandHistory.Undo();

        // Позиция восстановлена
        Assert.Equal(10000, line.StartMicronsX);
        Assert.Equal(10000, line.StartMicronsY);
    }

    [Fact]
    public void Drag_MultiSelection_UndoRestoresAllPositions()
    {
        var vm = CreateViewModel();
        var line1 = new Line(10000, 10000, 30000, 30000);
        var line2 = new Line(20000, 20000, 40000, 40000);
        vm.Template.Objects.Add(line1);
        vm.Template.Objects.Add(line2);
        vm.SelectedObjects.Add(line1);
        vm.SelectedObjects.Add(line2);

        var tool = new SelectTool(vm);

        // MouseDown на line1 (в середину, не на маркере)
        tool.OnMouseDown(new PointMicrons(20000, 20000), ToolMouseButton.Left, ToolModifiers.None);

        // MouseMove — сдвигаем на 5мм вправо
        tool.OnMouseMove(new PointMicrons(25000, 20000), ToolMouseButton.Left, ToolModifiers.None);

        // Оба объекта перемещены
        Assert.Equal(15000, line1.StartMicronsX);
        Assert.Equal(25000, line2.StartMicronsX);

        // MouseUp
        tool.OnMouseUp(new PointMicrons(25000, 20000), ToolMouseButton.Left, ToolModifiers.None);

        // Undo
        vm.CommandHistory.Undo();

        // Оба объекта восстановлены
        Assert.Equal(10000, line1.StartMicronsX);
        Assert.Equal(20000, line2.StartMicronsX);
    }

    [Fact]
    public void Drag_WithSnapEnabled_SnapsToGrid()
    {
        var vm = CreateViewModel();
        vm.GridSettings.SnapEnabled = true;
        vm.GridSettings.StepMicrons = 5000; // 5мм шаг

        var line = new Line(10000, 10000, 30000, 30000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        var tool = new SelectTool(vm);

        // MouseDown в середину (не на маркере)
        tool.OnMouseDown(new PointMicrons(20000, 20000), ToolMouseButton.Left, ToolModifiers.None);

        // MouseMove — на 3мм
        tool.OnMouseMove(new PointMicrons(23000, 23000), ToolMouseButton.Left, ToolModifiers.None);

        // С snap: 10000 + 3000 = 13000 → snap до 15000 (ближайший шаг 5мм)
        Assert.Equal(15000, line.StartMicronsX);
        Assert.Equal(15000, line.StartMicronsY);
    }

    [Fact]
    public void Drag_SmallMovement_DoesNotTriggerDrag()
    {
        var vm = CreateViewModel();
        var line = new Line(10000, 10000, 30000, 30000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);

        var tool = new SelectTool(vm);

        // MouseDown в середину
        tool.OnMouseDown(new PointMicrons(20000, 20000), ToolMouseButton.Left, ToolModifiers.None);

        // MouseMove — на 1мм (< threshold 3мм)
        tool.OnMouseMove(new PointMicrons(21000, 21000), ToolMouseButton.Left, ToolModifiers.None);

        // Объект НЕ перемещён (threshold не преодолён)
        Assert.Equal(10000, line.StartMicronsX);
        Assert.Equal(10000, line.StartMicronsY);

        // MouseUp — команда НЕ создана
        tool.OnMouseUp(new PointMicrons(21000, 21000), ToolMouseButton.Left, ToolModifiers.None);
        Assert.False(vm.CommandHistory.CanUndo);
    }

    [Fact]
    public void Drag_SetsIsDirty()
    {
        var vm = CreateViewModel();
        var line = new Line(10000, 10000, 30000, 30000);
        vm.Template.Objects.Add(line);
        vm.SelectSingle(line);
        vm.DirtyStateManager.IsDirty = false;

        var tool = new SelectTool(vm);

        // MouseDown в середину
        tool.OnMouseDown(new PointMicrons(20000, 20000), ToolMouseButton.Left, ToolModifiers.None);

        // MouseMove — сдвигаем на 5мм
        tool.OnMouseMove(new PointMicrons(25000, 20000), ToolMouseButton.Left, ToolModifiers.None);

        // MouseUp
        tool.OnMouseUp(new PointMicrons(25000, 20000), ToolMouseButton.Left, ToolModifiers.None);

        // IsDirty установлен
        Assert.True(vm.DirtyStateManager.IsDirty);
    }
}

public class DrawingTextToolTests
{
    private static EditorViewModel CreateViewModel()
    {
        var template = new Template();
        var mockService = new Mock<ITemplateService>();
        var mockPrintService = new Mock<IPrintService>();
        var vm = new EditorViewModel(template, mockService.Object, printService: mockPrintService.Object);
        vm.GridSettings.SnapEnabled = false;
        return vm;
    }

    [Fact]
    public void OnMouseUp_ClampsPositionToSheetBounds()
    {
        var vm = CreateViewModel();
        var sheetW = vm.Template.Sheet.WidthMicrons;
        var sheetH = vm.Template.Sheet.HeightMicrons;
        var tool = new TextTool(vm);

        tool.OnMouseDown(new PointMicrons(0, 0), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(sheetW + 50000, sheetH + 50000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(sheetW + 50000, sheetH + 50000), ToolMouseButton.Left, ToolModifiers.None);

        var text = vm.Template.Objects.OfType<Text>().FirstOrDefault();
        Assert.NotNull(text);
        Assert.Equal(sheetW, text.MicronsX);
        Assert.Equal(sheetH, text.MicronsY);
    }

    [Fact]
    public void OnMouseUp_ClampsNegativePositionToZero()
    {
        var vm = CreateViewModel();
        var tool = new TextTool(vm);

        tool.OnMouseDown(new PointMicrons(-5000, -5000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(-10000, -10000), ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(-10000, -10000), ToolMouseButton.Left, ToolModifiers.None);

        var text = vm.Template.Objects.OfType<Text>().FirstOrDefault();
        Assert.NotNull(text);
        Assert.Equal(0, text.MicronsX);
        Assert.Equal(0, text.MicronsY);
    }
}
