using System.IO;
using System.Text.Json;
using DotElectric.TemplateEditor.Models;
using Microsoft.Extensions.Logging;

namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Реализация ISettingsService.
/// Хранит настройки в JSON-файле: %APPDATA%\DotElectric\settings.json
/// </summary>
public sealed class SettingsService : ISettingsService
{
    private readonly string _settingsFile;
    private AppSettings? _cachedSettings;
    private readonly object _lock = new();
    private readonly ILogger<SettingsService>? _logger;

    public SettingsService(ILogger<SettingsService>? logger = null, string? settingsFilePath = null)
    {
        _logger = logger;

        if (settingsFilePath != null)
        {
            var dir = Path.GetDirectoryName(settingsFilePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            _settingsFile = settingsFilePath;
        }
        else
        {
            var appDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "DotElectric");

            Directory.CreateDirectory(appDataFolder);

            _settingsFile = Path.Combine(appDataFolder, "settings.json");
        }
    }

/// <inheritdoc/>
    public AppSettings Load()
    {
        lock (_lock)
        {
            if (_cachedSettings != null)
                return _cachedSettings;

            try
            {
                if (File.Exists(_settingsFile))
                {
                    var json = File.ReadAllText(_settingsFile);
                    var settings = JsonSerializer.Deserialize<AppSettings>(json);
                    if (settings != null)
                    {
                        _cachedSettings = settings;
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Файл настроек повреждён. Используются настройки по умолчанию.");
            }

            var defaults = new AppSettings();
            _cachedSettings = defaults;
            return defaults;
        }
    }

/// <inheritdoc/>
    public void Save(AppSettings settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        lock (_lock)
        {
            try
            {
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
                File.WriteAllText(_settingsFile, json);
                _cachedSettings = settings;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Не удалось сохранить настройки в файл {FilePath}", _settingsFile);
            }
        }
    }
}
