using System.Windows.Input;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.ViewModels;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Helpers;

public class ShortcutRegistryTests
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
    public void TryHandle_V_WhenInlineEditing_ReturnsFalse()
    {
        var vm = CreateViewModel();
        var text = new Text(1000, 1000, "Hello", 3500);
        vm.Template.Objects.Add(text);
        vm.StartInlineEditing(text);

        var result = ShortcutRegistry.TryHandle(Key.V, ModifierKeys.None, vm);

        Assert.False(result);
    }

    [Fact]
    public void TryHandle_L_WhenInlineEditing_ReturnsFalse()
    {
        var vm = CreateViewModel();
        var text = new Text(1000, 1000, "Hello", 3500);
        vm.Template.Objects.Add(text);
        vm.StartInlineEditing(text);

        var result = ShortcutRegistry.TryHandle(Key.L, ModifierKeys.None, vm);

        Assert.False(result);
    }

    [Fact]
    public void TryHandle_R_WhenInlineEditing_ReturnsFalse()
    {
        var vm = CreateViewModel();
        var text = new Text(1000, 1000, "Hello", 3500);
        vm.Template.Objects.Add(text);
        vm.StartInlineEditing(text);

        var result = ShortcutRegistry.TryHandle(Key.R, ModifierKeys.None, vm);

        Assert.False(result);
    }

    [Fact]
    public void TryHandle_T_WhenInlineEditing_ReturnsFalse()
    {
        var vm = CreateViewModel();
        var text = new Text(1000, 1000, "Hello", 3500);
        vm.Template.Objects.Add(text);
        vm.StartInlineEditing(text);

        var result = ShortcutRegistry.TryHandle(Key.T, ModifierKeys.None, vm);

        Assert.False(result);
    }

    [Fact]
    public void TryHandle_E_WhenInlineEditing_ReturnsFalse()
    {
        var vm = CreateViewModel();
        var text = new Text(1000, 1000, "Hello", 3500);
        vm.Template.Objects.Add(text);
        vm.StartInlineEditing(text);

        var result = ShortcutRegistry.TryHandle(Key.E, ModifierKeys.None, vm);

        Assert.False(result);
    }

    [Fact]
    public void TryHandle_ShiftE_WhenInlineEditing_ReturnsFalse()
    {
        var vm = CreateViewModel();
        var text = new Text(1000, 1000, "Hello", 3500);
        vm.Template.Objects.Add(text);
        vm.StartInlineEditing(text);

        var result = ShortcutRegistry.TryHandle(Key.E, ModifierKeys.Shift, vm);

        Assert.False(result);
    }

    [Fact]
    public void TryHandle_V_WhenNotEditing_ReturnsTrue()
    {
        var vm = CreateViewModel();

        var result = ShortcutRegistry.TryHandle(Key.V, ModifierKeys.None, vm);

        Assert.True(result);
        Assert.Equal("Select", vm.ToolManager.ActiveTool);
    }
}
