using System.Collections.ObjectModel;
using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Tools;

public interface IEditorContext
{
    // Selection
    ObservableCollection<TemplateObjectBase> SelectedObjects { get; }
    TemplateObjectBase? SingleSelectedObject { get; }
    void SelectSingle(TemplateObjectBase obj);
    void AddToSelection(TemplateObjectBase obj);
    void RemoveFromSelection(TemplateObjectBase obj);
    void ClearSelection();
    bool IsObjectSelected(TemplateObjectBase obj);

    // Hover + Resize state
    TemplateObjectBase? HoveredObject { get; set; }
    ResizeHandle? HoveredHandle { get; set; }
    ResizeHandle? ActiveResizeHandle { get; set; }

    // Preview
    Line? PreviewLine { get; set; }
    Models.Objects.Rectangle? PreviewRectangle { get; set; }
    Text? PreviewText { get; set; }
    void SetSelectionBox(long left, long bottom, long width, long height, Models.SelectionDirection direction);
    void ClearSelectionBox();

    // Inline editing
    void StartInlineEditing(Text textObj);

    // Template + Sheet
    Template Template { get; }
    GridSettings GridSettings { get; }
    double Zoom { get; }
    long ClampX(long x);
    long ClampY(long y);

    // Tool management
    void PushTool(ToolKind kind);
    void PopTool();
    void ActivateTool(ToolKind kind);
    T GetOrCreateTool<T>() where T : class, ITool;

    // Commands / Undo
    CommandHistory CommandHistory { get; }
    void MarkDirty();
    void DeleteSelected();

    // Status
    string StatusMessage { get; set; }
}
