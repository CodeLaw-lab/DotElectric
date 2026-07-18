namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Абстракция для управления жизненным циклом приложения.
/// Позволяет тестировать MainViewModel без зависимости от WPF.
/// </summary>
public interface IApplicationLifecycle
{
    /// <summary>
    /// Завершает работу приложения.
    /// </summary>
    void Shutdown();
}
