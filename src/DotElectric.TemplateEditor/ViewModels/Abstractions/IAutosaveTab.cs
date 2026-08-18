using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Abstractions;

public interface IAutosaveTab
{
    string? TabId { get; }
    string? FilePath { get; }
    string DisplayName { get; }
    bool IsDirty { get; }
    Template Template { get; }
}
