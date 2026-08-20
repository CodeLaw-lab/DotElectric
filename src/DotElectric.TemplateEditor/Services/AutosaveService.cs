using System.IO;
using System.Text.Json;
using DotElectric.TemplateEditor.Abstractions;
using DotElectric.TemplateEditor.Constants;
using Microsoft.Extensions.Logging;

namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Информация об автосохранении одной вкладки.
/// </summary>
public sealed class AutosaveTabInfo
{
    /// <summary>
    /// Уникальный ID вкладки (для сопоставления автосохранённых файлов).
    /// </summary>
    public string? TabId { get; set; }

    /// <summary>
    /// Отображаемое имя вкладки.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Оригинальный путь к файлу (если был сохранён).
    /// </summary>
    public string? OriginalFilePath { get; set; }

    /// <summary>
    /// Имя автосохранённого файла.
    /// </summary>
    public string? AutosaveFile { get; set; }

    /// <summary>
    /// Была ли вкладка «грязной» на момент автосохранения.
    /// </summary>
    public bool WasDirty { get; set; }
}

/// <summary>
/// Сессия автосохранения.
/// </summary>
public sealed class AutosaveSession
{
    /// <summary>
    /// Время начала сессии.
    /// </summary>
    public DateTime SessionStart { get; set; }

    /// <summary>
    /// Время последнего автосохранения.
    /// </summary>
    public DateTime LastAutosave { get; set; }

    /// <summary>
    /// Информация об автосохранённых вкладках.
    /// </summary>
    public List<AutosaveTabInfo> Tabs { get; set; } = new();
}

/// <summary>
/// Сервис автосохранения открытых шаблонов.
/// Сохраняет все «грязные» вкладки каждые N минут.
/// Папка: %APPDATA%\DotElectric\autosave\
/// </summary>
public sealed class AutosaveService : IDisposable
{
    private readonly ITemplateService _templateService;
    private readonly ISettingsService _settingsService;
    private readonly ILogger<AutosaveService>? _logger;
    private readonly IDispatcherService? _dispatcherService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private Timer? _timer;
    private readonly string _autosaveFolder;
    private readonly string _sessionFile;
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private bool _isDisposed;

    /// <summary>
    /// Событие тика таймера. Подписывается MainViewModel.
    /// </summary>
    public event Func<Task>? AutosaveTick;

    public AutosaveService(
        ITemplateService templateService,
        ISettingsService settingsService,
        ILogger<AutosaveService>? logger = null,
        IDispatcherService? dispatcherService = null,
        IDateTimeProvider? dateTimeProvider = null,
        string? autosaveFolder = null)
    {
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _logger = logger;
        _dispatcherService = dispatcherService;
        _dateTimeProvider = dateTimeProvider ?? new DateTimeProvider();

        _autosaveFolder = autosaveFolder ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DotElectric", "autosave");

        _sessionFile = Path.Combine(_autosaveFolder, "session.json");

        Directory.CreateDirectory(_autosaveFolder);
    }

    /// <summary>
    /// Запустить таймер автосохранения.
    /// </summary>
    public void Start()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        var intervalMinutes = _settingsService.Load().AutosaveIntervalMinutes;
        intervalMinutes = Math.Clamp(intervalMinutes, 1, 60);

        _timer = new Timer(
            OnAutosaveTick,
            null,
            TimeSpan.FromMinutes(intervalMinutes),
            TimeSpan.FromMinutes(intervalMinutes));

        _logger?.LogInformation(
            "AutosaveService запущен. Интервал: {IntervalMinutes} мин.", intervalMinutes);
    }

    /// <summary>
    /// Остановить таймер.
    /// </summary>
    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
        _logger?.LogInformation("AutosaveService остановлен.");
    }

    private void OnAutosaveTick(object? state)
    {
        try
        {
            if (AutosaveTick != null)
                _dispatcherService?.InvokeAsync(() => AutosaveTick());
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Ошибка в обработчике события AutosaveTick");
        }
    }

    /// <summary>
    /// Сохранить все «грязные» вкладки.
    /// Вызывается из MainViewModel по событию AutosaveTick.
    /// </summary>
    public async Task AutosaveAllTabsAsync(
        IEnumerable<IAutosaveTab> openedTabs,
        CancellationToken ct = default)
    {
        if (_isDisposed) return;

        await _saveLock.WaitAsync(ct);
        try
        {
            var session = new AutosaveSession
            {
                SessionStart = _dateTimeProvider.UtcNow,
                LastAutosave = _dateTimeProvider.UtcNow,
                Tabs = new List<AutosaveTabInfo>()
            };

            foreach (var tab in openedTabs.Where(t => t.IsDirty))
            {
                try
                {
                    AutosaveTab(tab, session);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex,
                        "Autosave не удался для вкладки: {TabName}", tab.DisplayName);
                }
            }

            // Сохраняем session.json
            SaveSession(session);

            // Очистка старых файлов (старше 7 дней)
            CleanupOldAutosaveFiles();
        }
        finally
        {
            _saveLock.Release();
        }
    }

    /// <summary>
    /// Загрузить сессию автосохранения (для восстановления после сбоя).
    /// </summary>
    /// <returns>Сессия или null, если session.json не найден.</returns>
    public AutosaveSession? LoadSession()
    {
        try
        {
            if (!File.Exists(_sessionFile))
                return null;

            var json = File.ReadAllText(_sessionFile);
            var session = JsonSerializer.Deserialize<AutosaveSession>(json);
            return session;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Не удалось загрузить сессию автосохранения");
            return null;
        }
    }

    /// <summary>
    /// Получить полный путь к автосохранённому файлу.
    /// </summary>
    public string GetAutosaveFilePath(string autosaveFileName)
    {
        return Path.Combine(_autosaveFolder, autosaveFileName);
    }

    /// <summary>
    /// Очистить папку автосохранения (после успешного восстановления или по запросу).
    /// </summary>
    public void ClearAutosaveFolder()
    {
        if (!Directory.Exists(_autosaveFolder))
            return;

        foreach (var file in Directory.GetFiles(_autosaveFolder))
        {
            try
            {
                File.Delete(file);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Не удалось удалить файл автосохранения: {File}", file);
            }
        }

        _logger?.LogInformation("Папка автосохранения очищена: {Folder}", _autosaveFolder);
    }

    #region Private Methods

    private void AutosaveTab(IAutosaveTab tab, AutosaveSession session)
    {
        var tabId = GetTabAutosaveId(tab);
        var fileName = $"autosave_{tabId}_{_dateTimeProvider.UtcNow:yyyyMMdd_HHmmss}.tdel";
        var filePath = Path.Combine(_autosaveFolder, fileName);

        // Сохраняем шаблон
        _templateService.Save(tab.Template, filePath);

        // Удаляем предыдущий файл автосохранения для этой вкладки
        CleanupOldAutosaveForTab(tabId);

        session.Tabs.Add(new AutosaveTabInfo
        {
            TabId = tabId,
            DisplayName = tab.DisplayName,
            OriginalFilePath = tab.FilePath,
            AutosaveFile = fileName,
            WasDirty = true
        });

        _logger?.LogDebug(
            "Автосохранение вкладки: {TabName} → {FileName}", tab.DisplayName, fileName);
    }

    private void SaveSession(AutosaveSession session)
    {
        if (session.Tabs.Count > 0)
        {
            var json = JsonSerializer.Serialize(session, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            File.WriteAllText(_sessionFile, json);
        }
        else
        {
            // Нет «грязных» вкладок — удаляем session.json
            if (File.Exists(_sessionFile))
                File.Delete(_sessionFile);
        }
    }

    /// <summary>
    /// Получить уникальный ID автосохранения для вкладки.
    /// </summary>
    private static string GetTabAutosaveId(IAutosaveTab tab)
    {
        return tab.TabId ??
               (string.IsNullOrEmpty(tab.FilePath)
                   ? Guid.NewGuid().ToString("N")[..8]
                   : Path.GetFileNameWithoutExtension(tab.FilePath));
    }

    /// <summary>
    /// Удалить старые автосохранённые файлы для конкретной вкладки.
    /// Оставляем только последний файл.
    /// </summary>
    private void CleanupOldAutosaveForTab(string tabId)
    {
        try
        {
            var pattern = $"autosave_{tabId}_*.tdel";
            var files = Directory.GetFiles(_autosaveFolder, pattern)
                .OrderByDescending(f => f)
                .Skip(1)
                .ToList();

            foreach (var file in files)
            {
                File.Delete(file);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex,
                "Ошибка очистки старых автосохранений для вкладки {TabId}", tabId);
        }
    }

    /// <summary>
    /// Удалить автосохранённые файлы старше 7 дней.
    /// </summary>
    private void CleanupOldAutosaveFiles()
    {
        try
        {
            var cutoffDate = _dateTimeProvider.UtcNow.AddDays(-EditorSettings.AutosaveCleanupDays);
            var files = Directory.GetFiles(_autosaveFolder, "autosave_*.tdel");

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.LastWriteTime < cutoffDate)
                {
                    File.Delete(file);
                    _logger?.LogDebug("Удалён старый автосохранённый файл: {File}", file);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Ошибка очистки старых автосохранённых файлов");
        }
    }

    #endregion

    #region IDisposable

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;

        Stop();
        _saveLock.Dispose();
    }

    #endregion
}


