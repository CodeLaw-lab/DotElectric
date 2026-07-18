using System.Windows;

namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// WPF-реализация IDispatcherService через Application.Current.Dispatcher.
/// </summary>
public sealed class WpfDispatcherService : IDispatcherService
{
    public T Invoke<T>(Func<T> action)
    {
        return Application.Current.Dispatcher.Invoke(action);
    }

    public void Invoke(Action action)
    {
        Application.Current.Dispatcher.Invoke(action);
    }

    public Task InvokeAsync(Func<Task> action)
    {
        return Application.Current.Dispatcher.InvokeAsync(action).Task;
    }
}
