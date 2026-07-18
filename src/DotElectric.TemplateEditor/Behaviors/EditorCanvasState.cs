using System.Windows;
using DotElectric.TemplateEditor.Tools;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Behaviors;

public class EditorCanvasState
{
    public required EditorViewModel Editor { get; init; }
    public int LastButtonRaw { get; set; } = -1;
    public ToolMouseButton LastButton => LastButtonRaw switch
    {
        0 => ToolMouseButton.Left,
        1 => ToolMouseButton.Right,
        2 => ToolMouseButton.Middle,
        _ => ToolMouseButton.Middle
    };
    public bool IsPanning { get; set; }
    public Point PanStartWpfPoint { get; set; }
    public Point PanAppliedModelDelta { get; set; }
}
