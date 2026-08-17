using System.Collections.ObjectModel;
using System.Windows.Input;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tools;
using DotElectric.TemplateEditor.ViewModels;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Tools;

public class PanToolTests
{
    private static EditorViewModel CreateEditorViewModel()
    {
        var template = new Template();
        var mockService = new Mock<ITemplateService>();
        var mockPrintService = new Mock<IPrintService>();
        return new EditorViewModel(template, mockService.Object, printService: mockPrintService.Object);
    }

    // === Constructor ===

    [Fact]
    public void Constructor_ThrowsOnNullEditor()
    {
        Assert.Throws<ArgumentNullException>(() => new PanTool(null!));
    }

    // === GetCursor ===

    [Fact]
    public void GetCursor_ReturnsHand()
    {
        var editor = CreateEditorViewModel();
        var tool = new PanTool(editor);

        Assert.Equal(ToolCursor.Hand, tool.GetCursor());
    }

    // === OnMouseDown ===

    [Fact]
    public void OnMouseDown_MiddleButton_StartsPanning()
    {
        var editor = CreateEditorViewModel();
        var tool = new PanTool(editor);
        var point = new PointMicrons(100000, 100000);

        tool.OnMouseDown(point, ToolMouseButton.Middle, ToolModifiers.None);

        // Проверяем через последующий OnMouseMove — PanOffset обновляется
        tool.OnMouseMove(new PointMicrons(101000, 101000), ToolMouseButton.Middle, ToolModifiers.None);
        Assert.True(editor.ZoomPanManager.PanOffsetX != 0 || editor.ZoomPanManager.PanOffsetY != 0);
    }

    [Fact]
    public void OnMouseDown_LeftButtonWithAlt_StartsPanning()
    {
        var editor = CreateEditorViewModel();
        var tool = new PanTool(editor);
        var point = new PointMicrons(100000, 100000);

        tool.OnMouseDown(point, ToolMouseButton.Left, ToolModifiers.Alt);

        // Проверяем через последующий OnMouseMove — PanOffset обновляется
        tool.OnMouseMove(new PointMicrons(101000, 101000), ToolMouseButton.Left, ToolModifiers.Alt);
        Assert.True(editor.ZoomPanManager.PanOffsetX != 0 || editor.ZoomPanManager.PanOffsetY != 0);
    }

    [Fact]
    public void OnMouseDown_LeftButtonWithoutAlt_DoesNothing()
    {
        var editor = CreateEditorViewModel();
        var tool = new PanTool(editor);
        var point = new PointMicrons(100000, 100000);

        tool.OnMouseDown(point, ToolMouseButton.Left, ToolModifiers.None);

        var prevX = editor.ZoomPanManager.PanOffsetX;
        var prevY = editor.ZoomPanManager.PanOffsetY;
        tool.OnMouseMove(new PointMicrons(101000, 101000), ToolMouseButton.Left, ToolModifiers.None);
        Assert.Equal(prevX, editor.ZoomPanManager.PanOffsetX);
        Assert.Equal(prevY, editor.ZoomPanManager.PanOffsetY);
    }

    // === OnMouseMove ===

    [Fact]
    public void OnMouseMove_WithoutMouseDown_DoesNothing()
    {
        var editor = CreateEditorViewModel();
        var tool = new PanTool(editor);

        var prevX = editor.ZoomPanManager.PanOffsetX;
        var prevY = editor.ZoomPanManager.PanOffsetY;
        tool.OnMouseMove(new PointMicrons(101000, 101000), ToolMouseButton.Middle, ToolModifiers.None);

        Assert.Equal(prevX, editor.ZoomPanManager.PanOffsetX);
        Assert.Equal(prevY, editor.ZoomPanManager.PanOffsetY);
    }

    [Fact]
    public void OnMouseMove_WithPanning_UpdatesPanOffset()
    {
        var editor = CreateEditorViewModel();
        var tool = new PanTool(editor);
        var start = new PointMicrons(100000, 100000);
        var end = new PointMicrons(110000, 110000); // +10мм

        tool.OnMouseDown(start, ToolMouseButton.Middle, ToolModifiers.None);
        tool.OnMouseMove(end, ToolMouseButton.Middle, ToolModifiers.None);

        Assert.True(editor.ZoomPanManager.PanOffsetX != 0 || editor.ZoomPanManager.PanOffsetY != 0);
    }

    [Fact]
    public void OnMouseMove_MultipleMoves_UpdatesPanOffsetEachTime()
    {
        var editor = CreateEditorViewModel();
        var tool = new PanTool(editor);

        tool.OnMouseDown(new PointMicrons(100000, 100000), ToolMouseButton.Middle, ToolModifiers.None);

        var panX1 = editor.ZoomPanManager.PanOffsetX;
        tool.OnMouseMove(new PointMicrons(101000, 101000), ToolMouseButton.Middle, ToolModifiers.None);
        var panX2 = editor.ZoomPanManager.PanOffsetX;
        tool.OnMouseMove(new PointMicrons(102000, 102000), ToolMouseButton.Middle, ToolModifiers.None);
        var panX3 = editor.ZoomPanManager.PanOffsetX;
        tool.OnMouseMove(new PointMicrons(103000, 103000), ToolMouseButton.Middle, ToolModifiers.None);
        var panX4 = editor.ZoomPanManager.PanOffsetX;

        // Каждый вызов должен накапливать
        Assert.True(panX2 != panX1);
        Assert.True(panX3 != panX2);
        Assert.True(panX4 != panX3);
    }

    // === OnMouseUp ===

    [Fact]
    public void OnMouseUp_StopsPanning()
    {
        var editor = CreateEditorViewModel();
        var tool = new PanTool(editor);

        tool.OnMouseDown(new PointMicrons(100000, 100000), ToolMouseButton.Middle, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(101000, 101000), ToolMouseButton.Middle, ToolModifiers.None);

        var prevX = editor.ZoomPanManager.PanOffsetX;
        var prevY = editor.ZoomPanManager.PanOffsetY;
        tool.OnMouseMove(new PointMicrons(102000, 102000), ToolMouseButton.Middle, ToolModifiers.None);

        Assert.Equal(prevX, editor.ZoomPanManager.PanOffsetX);
        Assert.Equal(prevY, editor.ZoomPanManager.PanOffsetY);
    }

    // === OnDoubleClick ===

    [Fact]
    public void OnDoubleClick_DoesNothing()
    {
        var editor = CreateEditorViewModel();
        var tool = new PanTool(editor);
        var point = new PointMicrons(100000, 100000);

        // Не должно вызывать исключений
        tool.OnDoubleClick(point);
    }

    // === OnMouseWheel ===

    [Fact]
    public void OnMouseWheel_DoesNothing()
    {
        var editor = CreateEditorViewModel();
        var tool = new PanTool(editor);
        var point = new PointMicrons(100000, 100000);

        // Не должно вызывать исключений
        tool.OnMouseWheel(120, point);
    }

    // === Integration ===

    [Fact]
    public void FullPanSequence_TracksCorrectly()
    {
        var editor = CreateEditorViewModel();
        var tool = new PanTool(editor);
        var start = new PointMicrons(100000, 100000);

        // Начало панорамирования
        tool.OnMouseDown(start, ToolMouseButton.Middle, ToolModifiers.None);
        var pan0 = editor.ZoomPanManager.PanOffsetX;

        // Первое перемещение
        tool.OnMouseMove(new PointMicrons(105000, 105000), ToolMouseButton.Middle, ToolModifiers.None);
        var pan1 = editor.ZoomPanManager.PanOffsetX;
        Assert.True(pan1 != pan0);

        // Второе перемещение
        tool.OnMouseMove(new PointMicrons(110000, 110000), ToolMouseButton.Middle, ToolModifiers.None);
        var pan2 = editor.ZoomPanManager.PanOffsetX;
        Assert.True(pan2 != pan1);

        // Конец панорамирования
        tool.OnMouseUp(new PointMicrons(110000, 110000), ToolMouseButton.Middle, ToolModifiers.None);

        // Ещё одно перемещение — не должно изменить PanOffset
        tool.OnMouseMove(new PointMicrons(115000, 115000), ToolMouseButton.Middle, ToolModifiers.None);
        Assert.Equal(pan2, editor.ZoomPanManager.PanOffsetX);
    }

    // === Edge cases (weak zones coverage) ===

    [Fact]
    public void OnMouseDown_RightButton_DoesNothing()
    {
        var editor = CreateEditorViewModel();
        var tool = new PanTool(editor);

        var prevX = editor.ZoomPanManager.PanOffsetX;
        var prevY = editor.ZoomPanManager.PanOffsetY;
        tool.OnMouseDown(new PointMicrons(100000, 100000), ToolMouseButton.Right, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(110000, 110000), ToolMouseButton.Right, ToolModifiers.None);

        Assert.Equal(prevX, editor.ZoomPanManager.PanOffsetX);
        Assert.Equal(prevY, editor.ZoomPanManager.PanOffsetY);
    }

    [Fact]
    public void OnMouseDown_MiddleWithCtrl_StartsPanning()
    {
        var editor = CreateEditorViewModel();
        var tool = new PanTool(editor);

        tool.OnMouseDown(new PointMicrons(100000, 100000), ToolMouseButton.Middle, ToolModifiers.Ctrl);

        var prevX = editor.ZoomPanManager.PanOffsetX;
        var prevY = editor.ZoomPanManager.PanOffsetY;
        tool.OnMouseMove(new PointMicrons(101000, 101000), ToolMouseButton.Middle, ToolModifiers.Ctrl);
        Assert.True(editor.ZoomPanManager.PanOffsetX != prevX || editor.ZoomPanManager.PanOffsetY != prevY);
    }

    [Fact]
    public void OnMouseUp_WithoutMouseDown_NoThrow()
    {
        var editor = CreateEditorViewModel();
        var tool = new PanTool(editor);

        var exception = Record.Exception(() =>
            tool.OnMouseUp(new PointMicrons(100000, 100000), ToolMouseButton.Middle, ToolModifiers.None));

        Assert.Null(exception);
    }

    [Fact]
    public void Reset_AfterPanning_StopsPanning()
    {
        var editor = CreateEditorViewModel();
        var tool = new PanTool(editor);

        tool.OnMouseDown(new PointMicrons(100000, 100000), ToolMouseButton.Middle, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(110000, 110000), ToolMouseButton.Middle, ToolModifiers.None);
        var panX = editor.ZoomPanManager.PanOffsetX;
        var panY = editor.ZoomPanManager.PanOffsetY;

        tool.Reset();

        tool.OnMouseMove(new PointMicrons(120000, 120000), ToolMouseButton.Middle, ToolModifiers.None);
        Assert.Equal(panX, editor.ZoomPanManager.PanOffsetX);
        Assert.Equal(panY, editor.ZoomPanManager.PanOffsetY);
    }

    [Fact]
    public void OnKeyDown_ReturnsFalse()
    {
        var editor = CreateEditorViewModel();
        var tool = new PanTool(editor);

        Assert.False(tool.OnKeyDown(ToolKey.Escape, ToolModifiers.None));
        Assert.False(tool.OnKeyDown(ToolKey.Delete, ToolModifiers.Ctrl));
    }

    [Fact]
    public void OnMouseWheel_ReturnsFalse()
    {
        var editor = CreateEditorViewModel();
        var tool = new PanTool(editor);

        Assert.False(tool.OnMouseWheel(120, new PointMicrons(100000, 100000)));
        Assert.False(tool.OnMouseWheel(-120, new PointMicrons(100000, 100000)));
    }

    [Fact]
    public void OnMouseMove_PanDelta_ExactMmValues()
    {
        var editor = CreateEditorViewModel();
        editor.ZoomPanManager.Zoom = 1.0;
        var tool = new PanTool(editor);

        var prevX = editor.ZoomPanManager.PanOffsetX;
        var prevY = editor.ZoomPanManager.PanOffsetY;

        tool.OnMouseDown(new PointMicrons(100000, 100000), ToolMouseButton.Middle, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(110000, 105000), ToolMouseButton.Middle, ToolModifiers.None);

        // delta = (10, 5) мм; PanCanvas: X += dx*Zoom, Y -= dy*Zoom
        Assert.Equal(prevX + 10.0, editor.ZoomPanManager.PanOffsetX);
        Assert.Equal(prevY - 5.0, editor.ZoomPanManager.PanOffsetY);
    }

    [Fact]
    public void FullSequence_ResetBetweenDrags_Works()
    {
        var editor = CreateEditorViewModel();
        var tool = new PanTool(editor);

        tool.OnMouseDown(new PointMicrons(100000, 100000), ToolMouseButton.Middle, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(105000, 105000), ToolMouseButton.Middle, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(105000, 105000), ToolMouseButton.Middle, ToolModifiers.None);
        var panAfterFirst = editor.ZoomPanManager.PanOffsetX;
        Assert.NotEqual(0.0, panAfterFirst);

        tool.Reset();

        tool.OnMouseDown(new PointMicrons(200000, 200000), ToolMouseButton.Middle, ToolModifiers.None);
        tool.OnMouseMove(new PointMicrons(205000, 205000), ToolMouseButton.Middle, ToolModifiers.None);
        tool.OnMouseUp(new PointMicrons(205000, 205000), ToolMouseButton.Middle, ToolModifiers.None);

        var panAfterSecond = editor.ZoomPanManager.PanOffsetX;
        Assert.NotEqual(panAfterFirst, panAfterSecond);
    }
}
