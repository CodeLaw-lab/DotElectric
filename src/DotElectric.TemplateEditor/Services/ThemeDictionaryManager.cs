using System.Windows;

namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Реальная WPF-реализация IThemeDictionaryManager.
/// Работает с Application.Current.Resources.MergedDictionaries.
/// </summary>
public sealed class ThemeDictionaryManager : IThemeDictionaryManager
{
    private const string LightThemeMarker = "LightTheme.xaml";
    private const string DarkThemeMarker = "DarkTheme.xaml";

    public void SetThemeDictionary(string themePath)
    {
        var dictionaries = Application.Current.Resources.MergedDictionaries;

        // Находим и удаляем текущую тему
        var oldTheme = dictionaries.FirstOrDefault(d =>
            d.Source?.OriginalString.Contains(LightThemeMarker) == true ||
            d.Source?.OriginalString.Contains(DarkThemeMarker) == true);

        if (oldTheme != null)
        {
            dictionaries.Remove(oldTheme);
        }

        // Загружаем новую тему
        var newTheme = new ResourceDictionary
        {
            Source = new Uri(themePath, UriKind.Relative)
        };

        // BeginInit/EndInit чтобы избежать мигания
        newTheme.BeginInit();
        dictionaries.Add(newTheme);
        newTheme.EndInit();
    }

    public string? GetCurrentThemePath()
    {
        var dictionaries = Application.Current.Resources.MergedDictionaries;

        var currentTheme = dictionaries.FirstOrDefault(d =>
            d.Source?.OriginalString.Contains(LightThemeMarker) == true ||
            d.Source?.OriginalString.Contains(DarkThemeMarker) == true);

        return currentTheme?.Source?.OriginalString;
    }
}
