namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Абстракция над операциями со словарями ресурсов WPF для переключения тем.
/// Позволяет тестировать ThemeService без реального WPF Application.
/// </summary>
public interface IThemeDictionaryManager
{
    /// <summary>
    /// Применить словарь ресурсов темы по указанному пути.
    /// </summary>
    /// <param name="themePath">Относительный путь к файлу темы (например, "Resources/Styles/LightTheme.xaml").</param>
    void SetThemeDictionary(string themePath);

    /// <summary>
    /// Получить путь к текущему словарю ресурсов темы.
    /// </summary>
    /// <returns>Путь к текущей теме или null, если тема не установлена.</returns>
    string? GetCurrentThemePath();
}
