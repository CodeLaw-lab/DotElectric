using System.Windows.Input;
using DotElectric.TemplateEditor.Tools;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Helpers;

public static class ShortcutRegistry
{
    private static readonly Dictionary<Key, ToolKind> ToolMap = new()
    {
        [Key.V] = ToolKind.Select,
        [Key.L] = ToolKind.Line,
        [Key.R] = ToolKind.Rectangle,
        [Key.T] = ToolKind.Text,
    };

    public static bool TryHandle(Key key, ModifierKeys modifiers, EditorViewModel editor)
    {
        if (editor.InlineEditManager.IsEditing)
            return false;

        var kind = GetToolForShortcut(key);
        if (kind.HasValue && modifiers == ModifierKeys.None)
        {
            editor.ActivateTool(kind.Value);
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

    public static ToolKind? GetToolForShortcut(Key key) =>
        ToolMap.TryGetValue(key, out var kind) ? kind : null;

    public static bool IsRotate(Key key, ModifierKeys modifiers) =>
        key == Key.E && modifiers == ModifierKeys.None;

    public static bool IsRotateReverse(Key key, ModifierKeys modifiers) =>
        key == Key.E && modifiers == ModifierKeys.Shift;
}
