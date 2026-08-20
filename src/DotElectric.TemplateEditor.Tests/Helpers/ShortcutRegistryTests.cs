using System.Windows.Input;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tools;
using DotElectric.TemplateEditor.ViewModels;
using Moq;
using DotElectric.TemplateEditor.Helpers;

namespace DotElectric.TemplateEditor.Tests.Helpers;

public sealed class ShortcutRegistryTests
{
    private static EditorViewModel CreateViewModel()
    {
        var template = new Template();
        var mockPrintService = new Mock<IPrintService>();
        var vm = new EditorViewModel(template, printService: mockPrintService.Object);
        vm.GridSettings.SnapEnabled = false;
        return vm;
    }

    // ==================== TryHandle — Inline editing guard ====================

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
        Assert.Equal(ToolKind.Select, vm.ToolRegistry.ActiveToolKind);
    }

    // ==================== TryHandle — Tool switching ====================

    [Fact]
    public void TryHandle_V_None_ReturnsTrue()
    {
        var editor = CreateViewModel();
        editor.ToolRegistry.ActiveToolKind = ToolKind.Line; // ensure different from target

        var result = ShortcutRegistry.TryHandle(Key.V, ModifierKeys.None, editor);

        Assert.True(result);
        Assert.Equal(ToolKind.Select, editor.ToolRegistry.ActiveToolKind);
    }

    [Fact]
    public void TryHandle_L_None_ReturnsTrue()
    {
        var editor = CreateViewModel();

        var result = ShortcutRegistry.TryHandle(Key.L, ModifierKeys.None, editor);

        Assert.True(result);
        Assert.Equal(ToolKind.Line, editor.ToolRegistry.ActiveToolKind);
    }

    [Fact]
    public void TryHandle_R_None_ReturnsTrue()
    {
        var editor = CreateViewModel();

        var result = ShortcutRegistry.TryHandle(Key.R, ModifierKeys.None, editor);

        Assert.True(result);
        Assert.Equal(ToolKind.Rectangle, editor.ToolRegistry.ActiveToolKind);
    }

    [Fact]
    public void TryHandle_T_None_ReturnsTrue()
    {
        var editor = CreateViewModel();

        var result = ShortcutRegistry.TryHandle(Key.T, ModifierKeys.None, editor);

        Assert.True(result);
        Assert.Equal(ToolKind.Text, editor.ToolRegistry.ActiveToolKind);
    }

    // ==================== TryHandle — Rotate ====================

    [Fact]
    public void TryHandle_E_None_RotatesClockwise()
    {
        var editor = CreateViewModel();
        var text = new Text(1000, 1000, "Test", 3500);
        editor.Template.Objects.Add(text);
        editor.SelectSingle(text);
        Assert.Equal(0, text.RotationAngle);

        var result = ShortcutRegistry.TryHandle(Key.E, ModifierKeys.None, editor);

        Assert.True(result);
        Assert.Equal(90, text.RotationAngle);
    }

    [Fact]
    public void TryHandle_E_Shift_RotatesCounterClockwise()
    {
        var editor = CreateViewModel();
        var text = new Text(1000, 1000, "Test", 3500);
        editor.Template.Objects.Add(text);
        editor.SelectSingle(text);
        Assert.Equal(0, text.RotationAngle);

        var result = ShortcutRegistry.TryHandle(Key.E, ModifierKeys.Shift, editor);

        Assert.True(result);
        Assert.Equal(270, text.RotationAngle);
    }

    // ==================== TryHandle — Unrecognized keys ====================

    [Fact]
    public void TryHandle_UnknownKey_ReturnsFalse()
    {
        var editor = CreateViewModel();

        var result = ShortcutRegistry.TryHandle(Key.F12, ModifierKeys.None, editor);

        Assert.False(result);
    }

    [Fact]
    public void TryHandle_NoneKey_ReturnsFalse()
    {
        var editor = CreateViewModel();

        var result = ShortcutRegistry.TryHandle(Key.None, ModifierKeys.None, editor);

        Assert.False(result);
    }

    [Fact]
    public void TryHandle_KeyWithModifier_DoesNotTriggerToolSwitch()
    {
        var editor = CreateViewModel();
        editor.ToolRegistry.ActiveToolKind = ToolKind.Select;

        // V with Ctrl should not switch to Select (it should be handled as Paste elsewhere)
        var result = ShortcutRegistry.TryHandle(Key.V, ModifierKeys.Control, editor);

        Assert.False(result);
        Assert.Equal(ToolKind.Select, editor.ToolRegistry.ActiveToolKind); // unchanged
    }

    [Fact]
    public void TryHandle_ToolKeyWithModifier_ReturnsFalse()
    {
        var editor = CreateViewModel();

        var result = ShortcutRegistry.TryHandle(Key.L, ModifierKeys.Control, editor);

        Assert.False(result);
        Assert.Equal(ToolKind.Select, editor.ToolRegistry.ActiveToolKind); // default, unchanged
    }

    // ==================== GetToolForShortcut — public static ====================

    [Fact]
    public void GetToolForShortcut_V_ReturnsSelect()
    {
        var tool = ShortcutRegistry.GetToolForShortcut(Key.V);
        Assert.Equal(ToolKind.Select, tool);
    }

    [Fact]
    public void GetToolForShortcut_L_ReturnsLine()
    {
        var tool = ShortcutRegistry.GetToolForShortcut(Key.L);
        Assert.Equal(ToolKind.Line, tool);
    }

    [Fact]
    public void GetToolForShortcut_R_ReturnsRectangle()
    {
        var tool = ShortcutRegistry.GetToolForShortcut(Key.R);
        Assert.Equal(ToolKind.Rectangle, tool);
    }

    [Fact]
    public void GetToolForShortcut_T_ReturnsText()
    {
        var tool = ShortcutRegistry.GetToolForShortcut(Key.T);
        Assert.Equal(ToolKind.Text, tool);
    }

    [Fact]
    public void GetToolForShortcut_Unknown_ReturnsNull()
    {
        var tool = ShortcutRegistry.GetToolForShortcut(Key.F12);
        Assert.Null(tool);
    }

    [Fact]
    public void GetToolForShortcut_None_ReturnsNull()
    {
        var tool = ShortcutRegistry.GetToolForShortcut(Key.None);
        Assert.Null(tool);
    }

    // ==================== IsRotate / IsRotateReverse — public static ====================

    [Fact]
    public void IsRotate_E_None_ReturnsTrue()
    {
        Assert.True(ShortcutRegistry.IsRotate(Key.E, ModifierKeys.None));
    }

    [Fact]
    public void IsRotate_E_Shift_ReturnsFalse()
    {
        Assert.False(ShortcutRegistry.IsRotate(Key.E, ModifierKeys.Shift));
    }

    [Fact]
    public void IsRotate_OtherKey_ReturnsFalse()
    {
        Assert.False(ShortcutRegistry.IsRotate(Key.F12, ModifierKeys.None));
    }

    [Fact]
    public void IsRotateReverse_E_Shift_ReturnsTrue()
    {
        Assert.True(ShortcutRegistry.IsRotateReverse(Key.E, ModifierKeys.Shift));
    }

    [Fact]
    public void IsRotateReverse_E_None_ReturnsFalse()
    {
        Assert.False(ShortcutRegistry.IsRotateReverse(Key.E, ModifierKeys.None));
    }

    [Fact]
    public void IsRotateReverse_OtherKey_ReturnsFalse()
    {
        Assert.False(ShortcutRegistry.IsRotateReverse(Key.F12, ModifierKeys.Shift));
    }
}
