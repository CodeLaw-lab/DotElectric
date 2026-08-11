using System.Windows;
using System.Windows.Threading;

namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// WPF-реализация IDispatcherService через Application.Current.Dispatcher.
/// </summary>
public sealed class WpfDispatcherService : IDispatcherService
{
    private readonly Dispatcher _dispatcher;

    public WpfDispatcherService(Dispatcher? dispatcher = null)
    {
        _dispatcher = dispatcher ?? Application.Current.Dispatcher;
    }

    public T Invoke<T>(Func<T> action)
    {
        return _dispatcher.Invoke(action);
    }

    public void Invoke(Action action)
    {
        _dispatcher.Invoke(action);
    }

    public Task InvokeAsync(Func<Task> action)
    {
        return _dispatcher.InvokeAsync(action).Task;
    }
}
