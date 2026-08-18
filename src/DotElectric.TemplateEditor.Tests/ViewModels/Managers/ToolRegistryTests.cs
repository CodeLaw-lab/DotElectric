using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tools;
using DotElectric.TemplateEditor.ViewModels;
using DotElectric.TemplateEditor.ViewModels.Managers;
using Moq;

namespace DotElectric.TemplateEditor.Tests.ViewModels.Managers;

public class ToolRegistryTests
{
    private static EditorViewModel CreateVm()
    {
        var template = new Template();
        var mockService = new Mock<ITemplateService>();
        var mockPrintService = new Mock<IPrintService>();
        return new EditorViewModel(template, mockService.Object, printService: mockPrintService.Object);
    }

    [Fact]
    public void Constructor_ActiveToolKind_IsSelect()
    {
        var vm = CreateVm();
        var sut = new ToolRegistry(vm);

        Assert.Equal(ToolKind.Select, sut.ActiveToolKind);
    }

    [Fact]
    public void ActiveToolKind_Set_RaisesKindAndInstanceNotifications()
    {
        var vm = CreateVm();
        var sut = new ToolRegistry(vm);
        var changed = new List<string?>();
        sut.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        sut.ActiveToolKind = ToolKind.Line;

        Assert.Equal(ToolKind.Line, sut.ActiveToolKind);
        Assert.Contains(nameof(ToolRegistry.ActiveToolKind), changed);
        Assert.Contains(nameof(ToolRegistry.ActiveToolInstance), changed);
    }

    [Theory]
    [InlineData(typeof(SelectTool), ToolKind.Select)]
    [InlineData(typeof(DrawingLineTool), ToolKind.Line)]
    [InlineData(typeof(DrawingRectangleTool), ToolKind.Rectangle)]
    [InlineData(typeof(TextTool), ToolKind.Text)]
    [InlineData(typeof(ResizeTool), ToolKind.Resize)]
    public void GetOrCreateTool_CreatesTool(Type expectedType, ToolKind? _)
    {
        var vm = CreateVm();
        var sut = new ToolRegistry(vm);

        var method = typeof(ToolRegistry).GetMethod("GetOrCreateTool")!.MakeGenericMethod(expectedType);
        var tool = method.Invoke(sut, null);

        Assert.IsType(expectedType, tool);
        Assert.NotNull(tool);
    }

    [Fact]
    public void GetOrCreateTool_CachesTool()
    {
        var vm = CreateVm();
        var sut = new ToolRegistry(vm);

        var t1 = sut.GetOrCreateTool<SelectTool>();
        var t2 = sut.GetOrCreateTool<SelectTool>();

        Assert.Same(t1, t2);
    }

    [Fact]
    public void GetOrCreateTool_UnknownType_Throws()
    {
        var vm = CreateVm();
        var sut = new ToolRegistry(vm);

        var ex = Assert.Throws<ArgumentException>(() => sut.GetOrCreateTool<InvalidTool>());
        Assert.Contains("Unknown tool type", ex.Message);
    }

    private sealed class InvalidTool : ITool
    {
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
    public void SwitchTo_ResetsPreviousTool()
    {
        var vm = CreateVm();
        var sut = new ToolRegistry(vm);
        var tool = sut.GetOrCreateTool<DrawingLineTool>();
        sut.ActiveToolKind = ToolKind.Line;
        tool.OnMouseDown(new PointMicrons(0, 0), ToolMouseButton.Left, ToolModifiers.None);
        Assert.NotNull(vm.PreviewManager.PreviewLine);

        sut.SwitchTo(ToolKind.Select);

        Assert.Null(vm.PreviewManager.PreviewLine);
        Assert.Equal(ToolKind.Select, sut.ActiveToolKind);
    }

    [Fact]
    public void SwitchTo_SameKind_DoesNotReset()
    {
        var vm = CreateVm();
        var sut = new ToolRegistry(vm);
        var tool = sut.GetOrCreateTool<DrawingLineTool>();
        sut.ActiveToolKind = ToolKind.Line;
        tool.OnMouseDown(new PointMicrons(0, 0), ToolMouseButton.Left, ToolModifiers.None);
        Assert.NotNull(vm.PreviewManager.PreviewLine);

        sut.SwitchTo(ToolKind.Line);

        Assert.NotNull(vm.PreviewManager.PreviewLine);
        Assert.Equal(ToolKind.Line, sut.ActiveToolKind);
    }

    [Fact]
    public void SwitchTo_UncachedTool_DoesNotThrow()
    {
        var vm = CreateVm();
        var sut = new ToolRegistry(vm);

        var ex = Record.Exception(() => sut.SwitchTo(ToolKind.Resize));
        Assert.Null(ex);
        Assert.Equal(ToolKind.Resize, sut.ActiveToolKind);
    }

    [Fact]
    public void ActiveToolInstance_ReturnsInstanceOfActiveKind()
    {
        var vm = CreateVm();
        var sut = new ToolRegistry(vm);
        sut.ActiveToolKind = ToolKind.Line;

        Assert.IsType<DrawingLineTool>(sut.ActiveToolInstance);
    }

    [Fact]
    public void ActiveToolInstance_ReturnsCachedInstance()
    {
        var vm = CreateVm();
        var sut = new ToolRegistry(vm);

        Assert.Same(sut.GetOrCreateTool<SelectTool>(), sut.ActiveToolInstance);
    }

    [Fact]
    public void ActiveToolKind_Change_RaisesActiveToolInstanceChanged()
    {
        var vm = CreateVm();
        var sut = new ToolRegistry(vm);
        var changed = false;
        sut.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ToolRegistry.ActiveToolInstance))
                changed = true;
        };

        sut.ActiveToolKind = ToolKind.Text;

        Assert.True(changed);
    }

    [Fact]
    public void PushTool_Kind_PopRestoresPrevious()
    {
        var vm = CreateVm();
        var sut = new ToolRegistry(vm);

        sut.PushTool(ToolKind.Resize);
        Assert.Equal(ToolKind.Resize, sut.ActiveToolKind);

        sut.PopTool();
        Assert.Equal(ToolKind.Select, sut.ActiveToolKind);
    }

    [Fact]
    public void PopTool_EmptyStack_DoesNotChange()
    {
        var vm = CreateVm();
        var sut = new ToolRegistry(vm);

        sut.ActiveToolKind = ToolKind.Line;
        sut.PopTool(); // Empty stack

        Assert.Equal(ToolKind.Line, sut.ActiveToolKind);
    }

    [Fact]
    public void PopTool_MultiplePushes_RestoresCorrectOrder()
    {
        var vm = CreateVm();
        var sut = new ToolRegistry(vm);

        sut.PushTool(ToolKind.Line);
        sut.PushTool(ToolKind.Rectangle);
        sut.PopTool();
        Assert.Equal(ToolKind.Line, sut.ActiveToolKind);

        sut.PopTool();
        Assert.Equal(ToolKind.Select, sut.ActiveToolKind);
    }
}
