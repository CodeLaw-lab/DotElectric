using System.Windows.Input;
using DotElectric.TemplateEditor.Behaviors;
using DotElectric.TemplateEditor.Tools;

namespace DotElectric.TemplateEditor.Tests.Behaviors;

public class EditorCanvasBehaviorTests
{
    // ===== ToToolMouseButton =====

    [Theory]
    [InlineData(MouseButton.Left, ToolMouseButton.Left)]
    [InlineData(MouseButton.Right, ToolMouseButton.Right)]
    [InlineData(MouseButton.Middle, ToolMouseButton.Middle)]
    [InlineData(MouseButton.XButton1, ToolMouseButton.Left)]
    [InlineData(MouseButton.XButton2, ToolMouseButton.Left)]
    public void ToToolMouseButton_MapsCorrectly(MouseButton input, ToolMouseButton expected)
    {
        var result = CanvasInputRouter.ToToolMouseButton(input);
        Assert.Equal(expected, result);
    }

    // ===== ToToolModifiers =====

    [Fact]
    public void ToToolModifiers_None_ReturnsNone()
    {
        var result = CanvasInputRouter.ToToolModifiers(ModifierKeys.None);
        Assert.Equal(ToolModifiers.None, result);
    }

    [Fact]
    public void ToToolModifiers_Ctrl_ReturnsCtrl()
    {
        var result = CanvasInputRouter.ToToolModifiers(ModifierKeys.Control);
        Assert.Equal(ToolModifiers.Ctrl, result);
    }

    [Fact]
    public void ToToolModifiers_Shift_ReturnsShift()
    {
        var result = CanvasInputRouter.ToToolModifiers(ModifierKeys.Shift);
        Assert.Equal(ToolModifiers.Shift, result);
    }

    [Fact]
    public void ToToolModifiers_Alt_ReturnsAlt()
    {
        var result = CanvasInputRouter.ToToolModifiers(ModifierKeys.Alt);
        Assert.Equal(ToolModifiers.Alt, result);
    }

    [Fact]
    public void ToToolModifiers_CtrlShift_ReturnsCtrlShift()
    {
        var result = CanvasInputRouter.ToToolModifiers(ModifierKeys.Control | ModifierKeys.Shift);
        Assert.Equal(ToolModifiers.Ctrl | ToolModifiers.Shift, result);
    }

    [Fact]
    public void ToToolModifiers_All_ReturnsAll()
    {
        var result = CanvasInputRouter.ToToolModifiers(ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt);
        Assert.Equal(ToolModifiers.Ctrl | ToolModifiers.Shift | ToolModifiers.Alt, result);
    }

    [Fact]
    public void ToToolModifiers_Windows_Ignored()
    {
        var result = CanvasInputRouter.ToToolModifiers(ModifierKeys.Windows);
        Assert.Equal(ToolModifiers.None, result);
    }

    // ===== ToToolKey =====

    [Theory]
    [InlineData(Key.Escape, ToolKey.Escape)]
    [InlineData(Key.Enter, ToolKey.Enter)]
    [InlineData(Key.Delete, ToolKey.Delete)]
    public void ToToolKey_KnownKey_ReturnsToolKey(Key input, ToolKey expected)
    {
        var result = CanvasInputRouter.ToToolKey(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(Key.A)]
    [InlineData(Key.Space)]
    [InlineData(Key.None)]
    public void ToToolKey_UnknownKey_ReturnsNull(Key input)
    {
        var result = CanvasInputRouter.ToToolKey(input);
        Assert.Null(result);
    }
}
