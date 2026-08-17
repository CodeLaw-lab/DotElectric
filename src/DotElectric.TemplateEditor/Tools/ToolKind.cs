namespace DotElectric.TemplateEditor.Tools;

/// <summary>
/// Идентичность переключаемого инструмента редактора.
/// Пан не входит: это жест роутера, а не инструмент (docs/adr/0001-pan-gesture-not-tool.md).
/// </summary>
public enum ToolKind
{
    Select,
    Line,
    Rectangle,
    Text,
    Resize,
}
