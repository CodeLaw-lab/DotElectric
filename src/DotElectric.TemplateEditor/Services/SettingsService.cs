using System.IO;
using System.Text.Json;
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

    /// <inheritdoc/>
    public T Get<T>(string key, T defaultValue)
    {
        if (string.IsNullOrWhiteSpace(key))
            return defaultValue;

        var settings = Load();

        // Проверяем известные свойства
        switch (key)
        {
            case "AutosaveIntervalMinutes":
                if (typeof(T) == typeof(int))
                    return (T)(object)settings.AutosaveIntervalMinutes;
                break;
            case "Theme":
                if (typeof(T) == typeof(string))
                    return (T)(object)settings.Theme;
                break;
            case "ShowGrid":
                if (typeof(T) == typeof(bool))
                    return (T)(object)settings.ShowGrid;
                break;
            case "SnapToGrid":
                if (typeof(T) == typeof(bool))
                    return (T)(object)settings.SnapToGrid;
                break;
            case "GridStepMm":
                if (typeof(T) == typeof(double))
                    return (T)(object)settings.GridStepMm;
                break;
            case "DefaultZoom":
                if (typeof(T) == typeof(double))
                    return (T)(object)settings.DefaultZoom;
                break;
            case "DefaultSheetFormat":
                if (typeof(T) == typeof(string))
                    return (T)(object)settings.DefaultSheetFormat;
                break;
            case "LastUsedSheetFormat":
                if (typeof(T) == typeof(string))
                    return (T)(object)settings.LastUsedSheetFormat;
                break;
            case "LastUsedSheetOrientation":
               if (defaultValue is string defStr3)
                  return (T)(object)settings.LastUsedSheetOrientation;
               break;
        }

        // Проверяем CustomSettings
        if (settings.CustomSettings.TryGetValue(key, out var value) && value != null)
        {
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Failed to convert setting '{Key}' to {Type}", key, typeof(T).Name);
            }
        }

        return defaultValue;
    }

    /// <inheritdoc/>
    public void Set<T>(string key, T value)
    {
        if (string.IsNullOrWhiteSpace(key))
            return;

        var settings = Load();

        // Устанавливаем известные свойства
        switch (key)
        {
            case "AutosaveIntervalMinutes":
                if (value is int intVal) settings.AutosaveIntervalMinutes = intVal;
                break;
            case "Theme":
                if (value is string strVal) settings.Theme = strVal;
                break;
            case "ShowGrid":
                if (value is bool boolVal) settings.ShowGrid = boolVal;
                break;
            case "SnapToGrid":
                if (value is bool boolVal2) settings.SnapToGrid = boolVal2;
                break;
            case "GridStepMm":
                if (value is double doubleVal) settings.GridStepMm = doubleVal;
                break;
            case "DefaultZoom":
                if (value is double doubleVal2) settings.DefaultZoom = doubleVal2;
                break;
            case "DefaultSheetFormat":
                if (value is string strVal2) settings.DefaultSheetFormat = strVal2;
                break;
            case "LastUsedSheetFormat":
                if (value is string strVal3) settings.LastUsedSheetFormat = strVal3;
                break;
            case "LastUsedSheetOrientation":
                if (value is string strVal4) settings.LastUsedSheetOrientation = strVal4;
                break;
            default:
                // Сохраняем в CustomSettings
                settings.CustomSettings[key] = value?.ToString() ?? string.Empty;
                break;
        }

        Save(settings);
    }
}
