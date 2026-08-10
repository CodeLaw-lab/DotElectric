namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Сервис управления темами оформления.
/// Переключает между LightTheme.xaml и DarkTheme.xaml через MergedDictionaries.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Текущая тема: "Light" или "Dark".
    /// </summary>
    string CurrentTheme { get; }

    /// <summary>
    /// Вызывается после успешной смены темы. Аргумент — новая тема ("Light"/"Dark").
    /// </summary>
    event Action<string>? ThemeChanged;

    /// <summary>
    /// Применить тему.
    /// </summary>
    /// <param name="theme">"Light" или "Dark".</param>
    void SetTheme(string theme);

    /// <summary>
    /// Переключить на противоположную тему.
    /// </summary>
    /// <returns>Новое название темы.</returns>
    string ToggleTheme();
}
