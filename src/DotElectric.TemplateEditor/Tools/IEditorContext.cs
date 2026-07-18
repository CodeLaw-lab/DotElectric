using System.Collections.ObjectModel;
using System.Windows.Input;
using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.ViewModels.Managers;

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
    long SelectionBoxLeft { get; set; }
    long SelectionBoxBottom { get; set; }
    long SelectionBoxTop { get; }
    long SelectionBoxWidth { get; set; }
    long SelectionBoxHeight { get; set; }
    long SelectionBoxRight { get; }
    Models.SelectionDirection SelectionDirection { get; set; }

    // Inline editing
    void StartInlineEditing(Text textObj);

    // Template + Sheet
    Template Template { get; }
    GridSettings GridSettings { get; }
    double Zoom { get; }
    long ClampX(long x);
    long ClampY(long y);

    // Tool management
    void PushTool(string tool);
    void PopTool();
    T GetOrCreateTool<T>() where T : class, ITool;

    // Commands / Undo
    CommandHistory CommandHistory { get; }
    void MarkDirty();
    void DeleteSelected();

    // Actions
    void PanCanvas(double deltaXMm, double deltaYMm);

    // Commands
    ICommand SetActiveToolCommand { get; }

    // Status
    string StatusMessage { get; set; }
}
