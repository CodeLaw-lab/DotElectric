using System.Windows;

namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// WPF-реализация IApplicationLifecycle.
/// Вызывает Application.Current.Shutdown() для завершения работы.
/// </summary>
public sealed class WpfApplicationLifecycle : IApplicationLifecycle
{
    /// <inheritdoc />
    public void Shutdown()
    {
        Application.Current.Shutdown();
    }
}
