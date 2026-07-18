using DotElectric.TemplateEditor.Services;
using Microsoft.Extensions.Logging;

namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Реализация сервиса управления темами оформления.
/// Переключает ResourceDictionary через IThemeDictionaryManager.
/// </summary>
public sealed class ThemeService : IThemeService
{
    private readonly ISettingsService _settingsService;
    private readonly IThemeDictionaryManager _dictionaryManager;
    private readonly ILogger<ThemeService>? _logger;

    private const string LightThemePath = "Resources/Styles/LightTheme.xaml";
    private const string DarkThemePath = "Resources/Styles/DarkTheme.xaml";

    public ThemeService(ISettingsService settingsService, IThemeDictionaryManager dictionaryManager, ILogger<ThemeService>? logger = null)
    {
        _settingsService = settingsService;
        _dictionaryManager = dictionaryManager;
        _logger = logger;
        CurrentTheme = _settingsService.Get("Theme", "Light");
    }

    public string CurrentTheme { get; private set; }

    public void SetTheme(string theme)
    {
        if (string.IsNullOrEmpty(theme))
        {
            _logger?.LogWarning("Попытка установить пустую тему. Используется Light.");
            theme = "Light";
        }

        var themePath = theme == "Dark" ? DarkThemePath : LightThemePath;

        try
        {
            _dictionaryManager.SetThemeDictionary(themePath);

            CurrentTheme = theme;
            _settingsService.Set("Theme", theme);

            _logger?.LogDebug("Тема изменена на: {Theme}", theme);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Ошибка при переключении темы на {Theme}", theme);
            // Fallback: не меняем тему
        }
    }

    public string ToggleTheme()
    {
        var newTheme = CurrentTheme == "Light" ? "Dark" : "Light";
        SetTheme(newTheme);
        return newTheme;
    }
}
