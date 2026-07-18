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

public class SelectionBoxTests
{
    private static EditorViewModel CreateEditorWithObjects(List<TemplateObjectBase> objects)
    {
        var template = new Template();
        foreach (var obj in objects)
            template.Objects.Add(obj);

        var mockService = new Mock<ITemplateService>();
        var mockPrintService = new Mock<IPrintService>();
        return new EditorViewModel(template, mockService.Object, printService: mockPrintService.Object);
    }

    // === Selection Box in SelectTool ===

    [Fact]
    public void SelectTool_MouseDownOnEmptySpace_StartsSelectionBox()
    {
        var editor = CreateEditorWithObjects(new List<TemplateObjectBase>
        {
            new Line(50000, 50000, 60000, 60000)
        });
        var tool = new SelectTool(editor);
        var startPoint = new PointMicrons(100000, 100000);

        tool.OnMouseDown(startPoint, ToolMouseButton.Left, ToolModifiers.None);

        Assert.Empty(editor.SelectedObjects);
        Assert.Equal(0, editor.SelectionBoxWidth); // Preview ещё не обновлён
    }

    [Fact]
    public void SelectTool_MouseMove_UpdatesSelectionBoxPreview()
    {
        var editor = CreateEditorWithObjects(new List<TemplateObjectBase>());
        var tool = new SelectTool(editor);
        var start = new PointMicrons(100000, 100000);
        var end = new PointMicrons(120000, 120000); // +20мм

        tool.OnMouseDown(start, ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(end, ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(20000, editor.SelectionBoxWidth);
        Assert.Equal(20000, editor.SelectionBoxHeight);
        Assert.Equal(SelectionDirection.LeftToRight, editor.SelectionDirection);
    }

    [Fact]
    public void SelectTool_MouseMove_SmallDistance_DoesNotShowBox()
    {
        var editor = CreateEditorWithObjects(new List<TemplateObjectBase>());
        var tool = new SelectTool(editor);
        var start = new PointMicrons(100000, 100000);
        var end = new PointMicrons(101000, 101000); // 1мм (< 3мм threshold)

        tool.OnMouseDown(start, ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(end, ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(0, editor.SelectionBoxWidth);
    }

    [Fact]
    public void SelectTool_MouseUp_SelectsObjects_LeftToRight()
    {
        var objects = new List<TemplateObjectBase>
        {
            new Line(105000, 105000, 110000, 110000), // Полностью внутри
            new Line(200000, 200000, 210000, 210000)  // Снаружи
        };
        var editor = CreateEditorWithObjects(objects);
        var tool = new SelectTool(editor);
        var start = new PointMicrons(100000, 100000);
        var end = new PointMicrons(120000, 120000);

        tool.OnMouseDown(start, ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(end, ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(end, ToolMouseButton.Left, ToolModifiers.None);

        Assert.Single(editor.SelectedObjects);
        Assert.Same(objects[0], editor.SelectedObjects[0]);
    }

    [Fact]
    public void SelectTool_MouseUp_SelectsObjects_RightToLeft()
    {
        var objects = new List<TemplateObjectBase>
        {
            new Line(45000, 45000, 50000, 50000),   // Внутри [40-60]
            new Line(55000, 55000, 65000, 65000)   // Пересекается [55-65] с [40-60]
        };
        var editor = CreateEditorWithObjects(objects);
        var tool = new SelectTool(editor);
        var start = new PointMicrons(70000, 70000);
        var end = new PointMicrons(40000, 40000); // RTL, box = [40000, 70000]

        tool.OnMouseDown(start, ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(end, ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(end, ToolMouseButton.Left, ToolModifiers.None);

        // RTL = intersecting
        Assert.True(editor.SelectedObjects.Count >= 1);
    }

    [Fact]
    public void SelectTool_MouseUp_ClearsPreview()
    {
        var editor = CreateEditorWithObjects(new List<TemplateObjectBase>());
        var tool = new SelectTool(editor);
        var start = new PointMicrons(100000, 100000);
        var end = new PointMicrons(120000, 120000);

        tool.OnMouseDown(start, ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseMove(end, ToolMouseButton.Left, ToolModifiers.None);
        tool.OnMouseUp(end, ToolMouseButton.Left, ToolModifiers.None);

        Assert.Equal(0, editor.SelectionBoxWidth);
        Assert.Equal(0, editor.SelectionBoxHeight);
        Assert.Equal(SelectionDirection.LeftToRight, editor.SelectionDirection);
    }

    // === SelectionBoxHelper Tests ===

    [Fact]
    public void GetDirection_LeftToRight_ReturnsCorrect()
    {
        var start = new PointMicrons(10000, 10000);
        var end = new PointMicrons(20000, 20000);

        var direction = SelectionBoxHelper.GetDirection(start, end);

        Assert.Equal(SelectionDirection.LeftToRight, direction);
    }

    [Fact]
    public void GetDirection_RightToLeft_ReturnsCorrect()
    {
        var start = new PointMicrons(20000, 20000);
        var end = new PointMicrons(10000, 10000);

        var direction = SelectionBoxHelper.GetDirection(start, end);

        Assert.Equal(SelectionDirection.RightToLeft, direction);
    }

    [Fact]
    public void GetSelectedObjects_LeftToRight_OnlyFullyContained()
    {
        var box = new RectMicrons(10000, 10000, 30000, 30000);
        var objects = new List<TemplateObjectBase>
        {
            new Rectangle(12000, 12000, 5000, 5000),  // Полностью внутри
            new Rectangle(25000, 25000, 10000, 10000) // Частично снаружи
        };

        var selected = SelectionBoxHelper.GetSelectedObjects(box, objects, SelectionDirection.LeftToRight);

        Assert.Single(selected);
        Assert.Same(objects[0], selected[0]);
    }

    [Fact]
    public void GetSelectedObjects_RightToLeft_AnyIntersecting()
    {
        var box = new RectMicrons(10000, 10000, 30000, 30000);
        var objects = new List<TemplateObjectBase>
        {
            new Rectangle(12000, 12000, 5000, 5000),  // Полностью внутри
            new Rectangle(25000, 25000, 10000, 10000), // Частично пересекается
            new Rectangle(50000, 50000, 5000, 5000)   // Снаружи
        };

        var selected = SelectionBoxHelper.GetSelectedObjects(box, objects, SelectionDirection.RightToLeft);

        Assert.Equal(2, selected.Count);
        Assert.Contains(objects[0], selected);
        Assert.Contains(objects[1], selected);
    }

    [Fact]
    public void RectMicrons_FromPoints_CorrectBounds()
    {
        var start = new PointMicrons(10000, 20000);
        var end = new PointMicrons(30000, 5000);

        var rect = RectMicrons.FromPoints(start, end);

        Assert.Equal(10000, rect.Left);
        Assert.Equal(5000, rect.Bottom);
        Assert.Equal(30000, rect.Right);
        Assert.Equal(20000, rect.Top);
        Assert.Equal(20000, rect.Width);
        Assert.Equal(15000, rect.Height);
    }

    [Fact]
    public void RectMicrons_Contains_ReturnsCorrect()
    {
        var outer = new RectMicrons(0, 0, 100000, 100000);
        var inner = new RectMicrons(20000, 20000, 80000, 80000);
        var partial = new RectMicrons(50000, 50000, 150000, 150000);

        Assert.True(outer.Contains(inner));
        Assert.False(outer.Contains(partial));
    }

    [Fact]
    public void RectMicrons_Intersects_ReturnsCorrect()
    {
        var box = new RectMicrons(0, 0, 100000, 100000);
        var overlapping = new RectMicrons(50000, 50000, 150000, 150000);
        var outside = new RectMicrons(200000, 200000, 300000, 300000);

        Assert.True(box.Intersects(overlapping));
        Assert.False(box.Intersects(outside));
    }
}
