namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Результат диалога несохранённых изменений.
/// </summary>
public enum UnsavedChangesResult
{
    /// <summary>
    /// Сохранить изменения.
    /// </summary>
    Save,

    /// <summary>
    /// Не сохранять изменения.
    /// </summary>
    DontSave,

    /// <summary>
    /// Отменить операцию.
    /// </summary>
    Cancel
}

/// <summary>
/// Сервис для показа UI-диалогов.
/// Абстрагирует System.Windows.MessageBox для тестируемости.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Показать диалог несохранённых изменений.
    /// </summary>
    /// <param name="fileName">Имя файла с несохранёнными изменениями.</param>
    /// <returns>Save, DontSave, или Cancel.</returns>
    Task<UnsavedChangesResult> ShowUnsavedChangesDialogAsync(string fileName);

    /// <summary>
    /// Показать диалог восстановления после сбоя.
    /// </summary>
    /// <returns>true = восстановить, false = начать новую сессию.</returns>
    Task<bool> ShowRecoveryDialogAsync();

    /// <summary>
    /// Показать сообщение об ошибке.
    /// </summary>
    /// <param name="message">Текст ошибки.</param>
    void ShowError(string message);

    /// <summary>
    /// Показать информационное сообщение.
    /// </summary>
    /// <param name="message">Текст сообщения.</param>
    void ShowInfo(string message);

    /// <summary>
    /// Показать критическую ошибку (при запуске).
    /// </summary>
    /// <param name="message">Текст ошибки.</param>
    void ShowFatalError(string message);

    /// <summary>
    /// Показать диалог подтверждения.
    /// </summary>
    /// <param name="message">Текст вопроса.</param>
    /// <param name="title">Заголовок окна.</param>
    /// <returns>true = Да, false = Нет.</returns>
    bool ShowConfirmation(string message, string title = "Подтверждение");
}


