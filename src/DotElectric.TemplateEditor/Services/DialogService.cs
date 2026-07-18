namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Реализация IDialogService через IMessageBoxProvider и IDispatcherService.
/// В проде использует WPF MessageBox и Dispatcher, в тестах — моки.
/// </summary>
public sealed class DialogService : IDialogService
{
    private readonly IMessageBoxProvider _messageBox;
    private readonly IDispatcherService _dispatcher;

    /// <summary>
    /// Создаёт DialogService с реальной WPF инфраструктурой.
    /// </summary>
    public DialogService() : this(new WpfMessageBoxProvider(), new WpfDispatcherService()) { }

    /// <summary>
    /// Создаёт DialogService с заданными зависимостями (для тестирования).
    /// </summary>
    public DialogService(IMessageBoxProvider messageBox, IDispatcherService dispatcher)
    {
        _messageBox = messageBox;
        _dispatcher = dispatcher;
    }

    /// <inheritdoc/>
    public Task<UnsavedChangesResult> ShowUnsavedChangesDialogAsync(string fileName)
    {
        var result = _dispatcher.Invoke(() => _messageBox.Show(
            $"Сохранить изменения в \"{fileName}\"?",
            "Несохранённые изменения",
            MsgrButtons.YesNoCancel,
            MsgrIcon.Question));

        return Task.FromResult(result switch
        {
            MsgrResult.Yes => UnsavedChangesResult.Save,
            MsgrResult.No => UnsavedChangesResult.DontSave,
            _ => UnsavedChangesResult.Cancel
        });
    }

    /// <inheritdoc/>
    public Task<bool> ShowRecoveryDialogAsync()
    {
        var result = _dispatcher.Invoke(() => _messageBox.Show(
            "Обнаружены несохранённые файлы после сбоя.\n" +
            "Восстановить?",
            "Восстановление после сбоя",
            MsgrButtons.YesNo,
            MsgrIcon.Question));

        return Task.FromResult(result == MsgrResult.Yes);
    }

    /// <inheritdoc/>
    public void ShowError(string message)
    {
        _dispatcher.Invoke(() => _messageBox.Show(
            message,
            "Ошибка",
            MsgrButtons.OK,
            MsgrIcon.Error));
    }

    /// <inheritdoc/>
    public void ShowInfo(string message)
    {
        _dispatcher.Invoke(() => _messageBox.Show(
            message,
            "DotElectric",
            MsgrButtons.OK,
            MsgrIcon.Information));
    }

    /// <inheritdoc/>
    public void ShowFatalError(string message)
    {
        _dispatcher.Invoke(() => _messageBox.Show(
            message,
            "Критическая ошибка",
            MsgrButtons.OK,
            MsgrIcon.Error));
    }

    /// <inheritdoc/>
    public bool ShowConfirmation(string message, string title = "Подтверждение")
    {
        var result = _dispatcher.Invoke(() => _messageBox.Show(
            message,
            title,
            MsgrButtons.YesNo,
            MsgrIcon.Question));

        return result == MsgrResult.Yes;
    }
}
