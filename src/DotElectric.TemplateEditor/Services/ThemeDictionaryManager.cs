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
        var oldTheme = FindThemeDictionary(dictionaries);

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

        var currentTheme = FindThemeDictionary(dictionaries);

        return currentTheme?.Source?.OriginalString;
    }

    internal static ResourceDictionary? FindThemeDictionary(IEnumerable<ResourceDictionary> dictionaries)
    {
        return dictionaries.FirstOrDefault(d =>
            d.Source != null &&
            (d.Source.OriginalString.Contains(LightThemeMarker) ||
             d.Source.OriginalString.Contains(DarkThemeMarker)));
    }
}
