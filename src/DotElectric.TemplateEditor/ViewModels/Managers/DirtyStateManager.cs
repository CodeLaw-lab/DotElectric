using CommunityToolkit.Mvvm.ComponentModel;
using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.ViewModels.Managers;

/// <summary>
/// Управляет состоянием "грязности" (IsDirty), путём к файлу и отображаемым именем вкладки.
/// </summary>
public sealed partial class DirtyStateManager : ObservableObject
{
    private readonly Template _template;

    public DirtyStateManager(Template template)
    {
        _template = template ?? throw new ArgumentNullException(nameof(template));
    }

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private string? _filePath;

    [ObservableProperty]
    private string _displayName = string.Empty;

    /// <summary>
    /// Пометить шаблон как изменённый.
    /// </summary>
    public void MarkDirty()
    {
        if (!IsDirty)
        {
            IsDirty = true;
            UpdateDisplayName();
        }
    }

    /// <summary>
    /// Сбросить флаг «грязности» (после сохранения).
    /// </summary>
    public void ClearDirty()
    {
        IsDirty = false;
        UpdateDisplayName();
    }

    /// <summary>
    /// Обновить DisplayName.
    /// </summary>
    public void UpdateDisplayName()
    {
        var orientLabel = _template.Sheet.Orientation == SheetOrientation.Portrait ? "кн." : "алб.";
        var baseName = string.IsNullOrEmpty(FilePath)
            ? $"{_template.Sheet.Format} ({orientLabel}) — Без имени"
            : System.IO.Path.GetFileName(FilePath);

        DisplayName = baseName;
    }
}
