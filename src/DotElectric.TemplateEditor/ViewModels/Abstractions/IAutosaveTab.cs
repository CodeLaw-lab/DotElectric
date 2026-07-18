using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Abstractions;

public interface IAutosaveTab
{
    string? TabId { get; }
    string? FilePath { get; set; }
    string DisplayName { get; }
    bool IsDirty { get; }
    object Template { get; }
}
