using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tools;
using DotElectric.TemplateEditor.ViewModels;
using DotElectric.TemplateEditor.ViewModels.Managers;
using Moq;

namespace DotElectric.TemplateEditor.Tests.ViewModels.Managers;

public class ToolManagerTests
{
    private static EditorViewModel CreateVm()
    {
        var template = new Template();
        var mockService = new Mock<ITemplateService>();
        var mockPrintService = new Mock<IPrintService>();
        return new EditorViewModel(template, mockService.Object, printService: mockPrintService.Object);
    }

    [Fact]
    public void Constructor_ActiveToolIsSelect()
    {
        var vm = CreateVm();
        var sut = new ToolManager(vm);

        Assert.Equal("Select", sut.ActiveTool);
    }

    [Theory]
    [InlineData(typeof(SelectTool), "Select")]
    [InlineData(typeof(DrawingLineTool), "Line")]
    [InlineData(typeof(DrawingRectangleTool), "Rectangle")]
    [InlineData(typeof(TextTool), "Text")]
    [InlineData(typeof(PanTool), "Pan")]
    [InlineData(typeof(ResizeTool), "Resize")]
    public void GetOrCreateTool_CreatesTool(Type expectedType, string _)
    {
        var vm = CreateVm();
        var sut = new ToolManager(vm);

        var method = typeof(ToolManager).GetMethod("GetOrCreateTool")!.MakeGenericMethod(expectedType);
        var tool = method.Invoke(sut, null);

        Assert.IsType(expectedType, tool);
        Assert.NotNull(tool);
    }

    [Fact]
    public void GetOrCreateTool_CachesTool()
    {
        var vm = CreateVm();
        var sut = new ToolManager(vm);

        var t1 = sut.GetOrCreateTool<SelectTool>();
        var t2 = sut.GetOrCreateTool<SelectTool>();

        Assert.Same(t1, t2);
    }

    [Fact]
    public void GetOrCreateTool_UnknownType_Throws()
    {
        var vm = CreateVm();
        var sut = new ToolManager(vm);

        var ex = Assert.Throws<ArgumentException>(() => sut.GetOrCreateTool<InvalidTool>());
        Assert.Contains("Unknown tool type", ex.Message);
    }

    private sealed class InvalidTool : ITool
    {
        public string Name => "Invalid";
        public void OnMouseDown(PointMicrons p, ToolMouseButton b, ToolModifiers m) { }
        public void OnMouseMove(PointMicrons p, ToolMouseButton b, ToolModifiers m) { }
        public void OnMouseUp(PointMicrons p, ToolMouseButton b, ToolModifiers m) { }
        public void OnDoubleClick(PointMicrons p) { }
        public bool OnMouseWheel(int d, PointMicrons p) => false;
        public bool OnKeyDown(ToolKey k, ToolModifiers m) => false;
        public ToolCursor GetCursor() => ToolCursor.Arrow;
        public void Reset() { }
    }

    [Fact]
    public void ActiveTool_Set_RaisesPropertyChanged()
    {
        var vm = CreateVm();
        var sut = new ToolManager(vm);
        var changed = false;
        sut.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ToolManager.ActiveTool))
                changed = true;
        };

        sut.ActiveTool = "Line";

        Assert.True(changed);
        Assert.Equal("Line", sut.ActiveTool);
    }

    [Fact]
    public void PushTool_SavesPreviousTool()
    {
        var vm = CreateVm();
        var sut = new ToolManager(vm);

        sut.PushTool("Line");

        Assert.Equal("Line", sut.ActiveTool);
    }

    [Fact]
    public void PopTool_RestoresPreviousTool()
    {
        var vm = CreateVm();
        var sut = new ToolManager(vm);

        sut.PushTool("Line");
        sut.PopTool();

        Assert.Equal("Select", sut.ActiveTool);
    }

    [Fact]
    public void PopTool_EmptyStack_DoesNotChange()
    {
        var vm = CreateVm();
        var sut = new ToolManager(vm);

        sut.ActiveTool = "Line";
        sut.PopTool(); // Empty stack

        Assert.Equal("Line", sut.ActiveTool);
    }

    [Fact]
    public void PopTool_MultiplePushes_RestoresCorrectOrder()
    {
        var vm = CreateVm();
        var sut = new ToolManager(vm);

        sut.PushTool("Line");
        sut.PushTool("Rectangle");
        sut.PopTool();
        Assert.Equal("Line", sut.ActiveTool);

        sut.PopTool();
        Assert.Equal("Select", sut.ActiveTool);
    }

    [Fact]
    public void ResetTool_ResetsExistingTool()
    {
        var vm = CreateVm();
        var sut = new ToolManager(vm);
        var tool = sut.GetOrCreateTool<DrawingLineTool>();

        // Simulate partial state by starting line drawing
        tool.OnMouseDown(new PointMicrons(0, 0), ToolMouseButton.Left, ToolModifiers.None);
        Assert.NotNull(vm.PreviewLine);

        sut.ResetTool("Line");

        Assert.Null(vm.PreviewLine);
    }

    [Fact]
    public void ResetTool_UnknownName_DoesNotThrow()
    {
        var vm = CreateVm();
        var sut = new ToolManager(vm);

        var ex = Record.Exception(() => sut.ResetTool("NonExistent"));
        Assert.Null(ex);
    }

    [Fact]
    public void ResetTool_NotCached_DoesNotThrow()
    {
        var vm = CreateVm();
        var sut = new ToolManager(vm);

        var ex = Record.Exception(() => sut.ResetTool("Line"));
        Assert.Null(ex);
    }
}
