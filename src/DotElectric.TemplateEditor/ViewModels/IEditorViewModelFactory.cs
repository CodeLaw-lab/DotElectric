using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;

namespace DotElectric.TemplateEditor.ViewModels;

/// <summary>
/// Factory for creating EditorViewModel instances with DI-managed dependencies.
/// Supports all constructor overloads used in the application.
/// </summary>
public interface IEditorViewModelFactory
{
    /// <summary>
    /// Creates an EditorViewModel for a new template (no file path).
    /// </summary>
    EditorViewModel Create(Template template, GridSettings? gridSettings = null, IPrintService? printService = null);

    /// <summary>
    /// Creates an EditorViewModel for a template loaded from a file.
    /// </summary>
    EditorViewModel CreateWithFilePath(Template template, string filePath, GridSettings? gridSettings = null, IPrintService? printService = null);
}
