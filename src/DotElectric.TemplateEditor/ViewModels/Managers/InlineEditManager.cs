using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.ViewModels.Managers;

/// <summary>
/// Управляет inline-редактированием текстовых объектов.
/// </summary>
public sealed partial class InlineEditManager : ObservableObject
{
    private readonly CommandHistory _commandHistory;
    private readonly Action? _markDirty;
    private readonly Action<string>? _onStatusChanged;

    /// <summary>
    /// Объект текста, который редактируется inline.
    /// </summary>
    [ObservableProperty]
    private Text? _inlineEditingText;

    /// <summary>
    /// Текущий текст в inline-редакторе.
    /// </summary>
    [ObservableProperty]
    private string _inlineEditText = string.Empty;

    /// <summary>
    /// True, если сейчас активно inline-редактирование.
    /// </summary>
    public bool IsEditing => InlineEditingText != null;

    public InlineEditManager(
        CommandHistory commandHistory,
        Action? markDirty = null,
        Action<string>? onStatusChanged = null)
    {
        _commandHistory = commandHistory;
        _markDirty = markDirty;
        _onStatusChanged = onStatusChanged;
    }

    /// <summary>
    /// Начать inline-редактирование текстового объекта.
    /// </summary>
    public void Start(Text textObj)
    {
        if (!textObj.IsEditable) return; // defense-in-depth
        InlineEditingText = textObj;
        InlineEditText = textObj.Content;
        _onStatusChanged?.Invoke("Редактирование текста");
    }

    /// <summary>
    /// Завершить inline-редактирование (сохранить).
    /// </summary>
    public void Commit()
    {
        if (InlineEditingText == null) return;

        var textObj = InlineEditingText;
        var newText = InlineEditText;

        if (!string.IsNullOrWhiteSpace(newText))
        {
            // Не создаём команду, если текст не изменился
            if (!string.Equals(textObj.Content, newText, StringComparison.Ordinal))
            {
                var cmd = new ChangePropertyCommand<string?>(
                    () => textObj.Content,
                    v => textObj.Content = v ?? string.Empty,
                    newText, "Содержимое текста", _markDirty);
                _commandHistory.Push(cmd);
            }
        }

        InlineEditingText = null;
        InlineEditText = string.Empty;
        _onStatusChanged?.Invoke("Готово");
    }

    /// <summary>
    /// Отменить inline-редактирование.
    /// </summary>
    public void Cancel()
    {
        InlineEditingText = null;
        InlineEditText = string.Empty;
        _onStatusChanged?.Invoke("Готово");
    }
}
