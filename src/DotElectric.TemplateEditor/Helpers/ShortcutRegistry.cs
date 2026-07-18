using System.Windows.Input;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Helpers;

public static class ShortcutRegistry
{
    private static readonly Dictionary<Key, string> ToolMap = new()
    {
        [Key.V] = "Select",
        [Key.L] = "Line",
        [Key.R] = "Rectangle",
        [Key.T] = "Text",
    };

    public static bool TryHandle(Key key, ModifierKeys modifiers, EditorViewModel editor)
    {
        var tool = GetToolForShortcut(key);
        if (tool != null && modifiers == ModifierKeys.None)
        {
            editor.SetActiveToolCommand.Execute(tool);
            return true;
        }

        if (IsRotate(key, modifiers))
        {
            editor.RotateSelectedClockwiseCommand.Execute(null);
            return true;
        }

        if (IsRotateReverse(key, modifiers))
        {
            editor.RotateSelectedCounterClockwiseCommand.Execute(null);
            return true;
        }

        return false;
    }

    public static string? GetToolForShortcut(Key key) =>
        ToolMap.TryGetValue(key, out var tool) ? tool : null;

    public static bool IsRotate(Key key, ModifierKeys modifiers) =>
        key == Key.E && modifiers == ModifierKeys.None;

    public static bool IsRotateReverse(Key key, ModifierKeys modifiers) =>
        key == Key.E && modifiers == ModifierKeys.Shift;
}
