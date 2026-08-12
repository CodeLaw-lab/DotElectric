using System.Windows;
using DotElectric.TemplateEditor.Behaviors;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tools;
using DotElectric.TemplateEditor.ViewModels;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Behaviors;

public class EditorCanvasStateTests
{
    private static EditorViewModel CreateEditor()
    {
        var template = new Template();
        var mockService = new Mock<ITemplateService>();
        mockService.Setup(s => s.CreateNew(It.IsAny<string>(), It.IsAny<SheetOrientation>()))
            .Returns(() => new Template());
        mockService.Setup(s => s.Validate(It.IsAny<Template>()))
            .Returns(Enumerable.Empty<string>());
        return new EditorViewModel(template, mockService.Object, printService: new Mock<IPrintService>().Object);
    }

    // ===== Constructor =====

    [Fact]
    public void Constructor_Editor_IsSet()
    {
        var editor = CreateEditor();

        var sut = new EditorCanvasState { Editor = editor };

        Assert.Same(editor, sut.Editor);
    }

    // ===== LastButtonRaw → LastButton mapping =====

    [Theory]
    [InlineData(0, ToolMouseButton.Left)]
    [InlineData(1, ToolMouseButton.Right)]
    [InlineData(2, ToolMouseButton.Middle)]
    public void LastButtonRaw_MapsKnownButtons(int raw, ToolMouseButton expected)
    {
        var sut = new EditorCanvasState { Editor = CreateEditor() };
        sut.LastButtonRaw = raw;

        Assert.Equal(expected, sut.LastButton);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(99)]
    [InlineData(int.MaxValue)]
    public void LastButtonRaw_UnknownValue_FallsBackToMiddle(int raw)
    {
        var sut = new EditorCanvasState { Editor = CreateEditor() };
        sut.LastButtonRaw = raw;

        Assert.Equal(ToolMouseButton.Middle, sut.LastButton);
    }

    [Fact]
    public void LastButtonRaw_Default_IsMinusOne()
    {
        var sut = new EditorCanvasState { Editor = CreateEditor() };

        Assert.Equal(-1, sut.LastButtonRaw);
    }

    // ===== IsPanning =====

    [Fact]
    public void IsPanning_Default_IsFalse()
    {
        var sut = new EditorCanvasState { Editor = CreateEditor() };

        Assert.False(sut.IsPanning);
    }

    [Fact]
    public void IsPanning_SetTrue_ReturnsTrue()
    {
        var sut = new EditorCanvasState { Editor = CreateEditor() };
        sut.IsPanning = true;

        Assert.True(sut.IsPanning);
    }

    // ===== PanStartWpfPoint =====

    [Fact]
    public void PanStartWpfPoint_Default_IsZero()
    {
        var sut = new EditorCanvasState { Editor = CreateEditor() };

        Assert.Equal(new Point(0, 0), sut.PanStartWpfPoint);
    }

    [Fact]
    public void PanStartWpfPoint_Set_ReturnsValue()
    {
        var sut = new EditorCanvasState { Editor = CreateEditor() };
        var point = new Point(123.5, -42.25);
        sut.PanStartWpfPoint = point;

        Assert.Equal(point, sut.PanStartWpfPoint);
    }

    // ===== PanAppliedModelDelta =====

    [Fact]
    public void PanAppliedModelDelta_Default_IsZero()
    {
        var sut = new EditorCanvasState { Editor = CreateEditor() };

        Assert.Equal(new Point(0, 0), sut.PanAppliedModelDelta);
    }

    [Fact]
    public void PanAppliedModelDelta_Set_ReturnsValue()
    {
        var sut = new EditorCanvasState { Editor = CreateEditor() };
        var delta = new Point(10, -20);
        sut.PanAppliedModelDelta = delta;

        Assert.Equal(delta, sut.PanAppliedModelDelta);
    }
}
