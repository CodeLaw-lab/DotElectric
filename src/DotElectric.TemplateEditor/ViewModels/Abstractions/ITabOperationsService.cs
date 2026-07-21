using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.ViewModels.Abstractions;

/// <summary>
/// Facade for tab operations (NewTab, OpenFile, Save, SaveAs, Print).
/// Reduces MainViewModel DI dependencies.
/// </summary>
public interface ITabOperationsService
{
    /// <summary>Create a new tab with the specified format.</summary>
    EditorViewModel CreateNewTab(string? format, string? lastUsedFormat, string? lastUsedOrientation);

    /// <summary>Open a file dialog and create a tab.</summary>
    Task<EditorViewModel?> OpenFileAsync();

    /// <summary>Open a template from a file path (used by library double-click).</summary>
    EditorViewModel? OpenFromFilePath(string filePath);

    /// <summary>Save a specific tab.</summary>
    Task SaveTabAsync(EditorViewModel tab);

    /// <summary>Save-as for a specific tab.</summary>
    Task SaveAsAsync(EditorViewModel tab);

    /// <summary>
    /// If tab is dirty, prompt user and optionally save.
    /// Returns true if the tab can be closed, false if cancelled.
    /// </summary>
    Task<bool> PromptAndSaveIfDirtyAsync(EditorViewModel tab);

    /// <summary>Create a new tab with a custom sheet size.</summary>
    EditorViewModel CreateNewCustomTab(double widthMm, double heightMm);
}
