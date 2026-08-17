namespace DotElectric.TemplateEditor.Tools;

/// <summary>
/// Идентичность переключаемого инструмента редактора.
/// Пан не входит: он вспомогательный и type-адресуется через GetOrCreateTool&lt;PanTool&gt;().
/// </summary>
public enum ToolKind
{
    Select,
    Line,
    Rectangle,
    Text,
    Resize,
}
