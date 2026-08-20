namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Абстракция для показа MessageBox. Позволяет мокать диалоги в unit-тестах.
/// В тестах можно мокать, в проде — WPF реализация с Dispatcher.
/// </summary>
public interface IMessageBoxProvider
{
    /// <summary>
    /// Показать MessageBox с заданными параметрами.
    /// Вызывается из UI-потока (или Dispatcher.Invoke в WPF реализации).
    /// </summary>
    MsgrResult Show(string message, string caption, MsgrButtons buttons, MsgrIcon icon);
}

/// <summary>
/// Абстракция для IDispatcher. Позволяет тестировать без WPF Application.
/// </summary>
public interface IDispatcherService
{
    /// <summary>
    /// Выполнить действие в UI-потоке.
/// </summary>
    T Invoke<T>(Func<T> action);

    /// <summary>
    /// Выполнить действие в UI-потоке (без возврата результата).
    /// </summary>
    void Invoke(Action action);

    /// <summary>
    /// Выполнить асинхронное действие в UI-потоке.
    /// </summary>
    Task InvokeAsync(Func<Task> action);
}

/// <summary>
/// Результат MessageBox.
/// </summary>
public enum MsgrResult
{
    None = 0,
    OK = 1,
    Cancel = 2,
    Yes = 6,
    No = 7
}

/// <summary>
/// Кнопки MessageBox.
/// </summary>
public enum MsgrButtons
{
    OK = 0,
    OKCancel = 1,
    YesNoCancel = 3,
    YesNo = 4
}

/// <summary>
/// Иконка MessageBox.
/// </summary>
public enum MsgrIcon
{
    None = 0,
    Information = 1,
    Warning = 2,
    Error = 3,
    Question = 4
}
