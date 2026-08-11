using System.Windows;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tests.Helpers;

namespace DotElectric.TemplateEditor.Tests.Services;

/// <summary>
/// STA-тесты ThemeDictionaryManager.FindThemeDictionary.
/// Словари загружаются через pack URI реальных тем главной сборки.
/// Pack-инфраструктура WPF инициализируется созданием Application.
/// </summary>
[Collection("ThemeDictionaryManager")]
public class ThemeDictionaryManagerTests
{
    private const string LightThemePath =
        "pack://application:,,,/DotElectric.TemplateEditor;component/Resources/Styles/LightTheme.xaml";

    private const string DarkThemePath =
        "pack://application:,,,/DotElectric.TemplateEditor;component/Resources/Styles/DarkTheme.xaml";

    [Fact]
    public void FindThemeDictionary_LightTheme_ReturnsDictionary()
    {
        WpfContext.Execute(() =>
        {
            EnsureApplication();
            var dictionaries = new List<ResourceDictionary>
            {
                new() { Source = new Uri(LightThemePath) }
            };

            var found = ThemeDictionaryManager.FindThemeDictionary(dictionaries);

            Assert.NotNull(found);
            Assert.Contains("LightTheme.xaml", found.Source.OriginalString);
        });
    }

    [Fact]
    public void FindThemeDictionary_DarkTheme_ReturnsDictionary()
    {
        WpfContext.Execute(() =>
        {
            EnsureApplication();
            var dictionaries = new List<ResourceDictionary>
            {
                new() { Source = new Uri(DarkThemePath) }
            };

            var found = ThemeDictionaryManager.FindThemeDictionary(dictionaries);

            Assert.NotNull(found);
            Assert.Contains("DarkTheme.xaml", found.Source.OriginalString);
        });
    }

    [Fact]
    public void FindThemeDictionary_NoTheme_ReturnsNull()
    {
        WpfContext.Execute(() =>
        {
            var dictionaries = new List<ResourceDictionary>
            {
                new()
            };

            var found = ThemeDictionaryManager.FindThemeDictionary(dictionaries);

            Assert.Null(found);
        });
    }

    [Fact]
    public void FindThemeDictionary_NullSourceElement_Skipped()
    {
        WpfContext.Execute(() =>
        {
            EnsureApplication();
            var dictionaries = new List<ResourceDictionary>
            {
                new(),
                new() { Source = new Uri(LightThemePath) }
            };

            var found = ThemeDictionaryManager.FindThemeDictionary(dictionaries);

            Assert.NotNull(found);
            Assert.Contains("LightTheme.xaml", found.Source.OriginalString);
        });
    }

    private static void EnsureApplication()
    {
        if (Application.Current == null)
        {
            _ = new Application();
        }
    }
}
