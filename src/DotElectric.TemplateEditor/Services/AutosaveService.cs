using System.IO;
using System.Text.Json;
using DotElectric.TemplateEditor.Abstractions;
using Microsoft.Extensions.Logging;

namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// РРЅС„РѕСЂРјР°С†РёСЏ РѕР± Р°РІС‚РѕСЃРѕС…СЂР°РЅРµРЅРёРё РѕРґРЅРѕР№ РІРєР»Р°РґРєРё.
/// </summary>
public sealed class AutosaveTabInfo
{
    /// <summary>
    /// РЈРЅРёРєР°Р»СЊРЅС‹Р№ ID РІРєР»Р°РґРєРё (РґР»СЏ СЃРѕРїРѕСЃС‚Р°РІР»РµРЅРёСЏ Р°РІС‚РѕСЃРѕС…СЂР°РЅС‘РЅРЅС‹С… С„Р°Р№Р»РѕРІ).
    /// </summary>
    public string? TabId { get; set; }

    /// <summary>
    /// РћС‚РѕР±СЂР°Р¶Р°РµРјРѕРµ РёРјСЏ РІРєР»Р°РґРєРё.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// РћСЂРёРіРёРЅР°Р»СЊРЅС‹Р№ РїСѓС‚СЊ Рє С„Р°Р№Р»Сѓ (РµСЃР»Рё Р±С‹Р» СЃРѕС…СЂР°РЅС‘РЅ).
    /// </summary>
    public string? OriginalFilePath { get; set; }

    /// <summary>
    /// РРјСЏ Р°РІС‚РѕСЃРѕС…СЂР°РЅС‘РЅРЅРѕРіРѕ С„Р°Р№Р»Р°.
    /// </summary>
    public string? AutosaveFile { get; set; }

    /// <summary>
    /// Р‘С‹Р»Р° Р»Рё РІРєР»Р°РґРєР° В«РіСЂСЏР·РЅРѕР№В» РЅР° РјРѕРјРµРЅС‚ Р°РІС‚РѕСЃРѕС…СЂР°РЅРµРЅРёСЏ.
    /// </summary>
    public bool WasDirty { get; set; }
}

/// <summary>
/// РЎРµСЃСЃРёСЏ Р°РІС‚РѕСЃРѕС…СЂР°РЅРµРЅРёСЏ.
/// </summary>
public sealed class AutosaveSession
{
    /// <summary>
    /// Р’СЂРµРјСЏ РЅР°С‡Р°Р»Р° СЃРµСЃСЃРёРё.
    /// </summary>
    public DateTime SessionStart { get; set; }

    /// <summary>
    /// Р’СЂРµРјСЏ РїРѕСЃР»РµРґРЅРµРіРѕ Р°РІС‚РѕСЃРѕС…СЂР°РЅРµРЅРёСЏ.
    /// </summary>
    public DateTime LastAutosave { get; set; }

    /// <summary>
    /// РРЅС„РѕСЂРјР°С†РёСЏ РѕР± Р°РІС‚РѕСЃРѕС…СЂР°РЅС‘РЅРЅС‹С… РІРєР»Р°РґРєР°С….
    /// </summary>
    public List<AutosaveTabInfo> Tabs { get; set; } = new();
}

/// <summary>
/// РЎРµСЂРІРёСЃ Р°РІС‚РѕСЃРѕС…СЂР°РЅРµРЅРёСЏ РѕС‚РєСЂС‹С‚С‹С… С€Р°Р±Р»РѕРЅРѕРІ.
/// РЎРѕС…СЂР°РЅСЏРµС‚ РІСЃРµ В«РіСЂСЏР·РЅС‹РµВ» РІРєР»Р°РґРєРё РєР°Р¶РґС‹Рµ N РјРёРЅСѓС‚.
/// РџР°РїРєР°: %APPDATA%\DotElectric\autosave\
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
    /// РЎРѕР±С‹С‚РёРµ С‚РёРєР° С‚Р°Р№РјРµСЂР°. РџРѕРґРїРёСЃС‹РІР°РµС‚СЃСЏ MainViewModel.
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
    /// Р—Р°РїСѓСЃС‚РёС‚СЊ С‚Р°Р№РјРµСЂ Р°РІС‚РѕСЃРѕС…СЂР°РЅРµРЅРёСЏ.
    /// </summary>
    public void Start()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        var intervalMinutes = _settingsService.Get("AutosaveIntervalMinutes", 5);
        intervalMinutes = Math.Clamp(intervalMinutes, 1, 60);

        _timer = new Timer(
            OnAutosaveTick,
            null,
            TimeSpan.FromMinutes(intervalMinutes),
            TimeSpan.FromMinutes(intervalMinutes));

        _logger?.LogInformation(
            "AutosaveService Р·Р°РїСѓС‰РµРЅ. РРЅС‚РµСЂРІР°Р»: {IntervalMinutes} РјРёРЅ.", intervalMinutes);
    }

    /// <summary>
    /// РћСЃС‚Р°РЅРѕРІРёС‚СЊ С‚Р°Р№РјРµСЂ.
    /// </summary>
    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
        _logger?.LogInformation("AutosaveService РѕСЃС‚Р°РЅРѕРІР»РµРЅ.");
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
            _logger?.LogError(ex, "РћС€РёР±РєР° РІ РѕР±СЂР°Р±РѕС‚С‡РёРєРµ СЃРѕР±С‹С‚РёСЏ AutosaveTick");
        }
    }

    /// <summary>
    /// РЎРѕС…СЂР°РЅРёС‚СЊ РІСЃРµ В«РіСЂСЏР·РЅС‹РµВ» РІРєР»Р°РґРєРё.
    /// Р’С‹Р·С‹РІР°РµС‚СЃСЏ РёР· MainViewModel РїРѕ СЃРѕР±С‹С‚РёСЋ AutosaveTick.
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
                        "Autosave РЅРµ СѓРґР°Р»СЃСЏ РґР»СЏ РІРєР»Р°РґРєРё: {TabName}", tab.DisplayName);
                }
            }

            // РЎРѕС…СЂР°РЅСЏРµРј session.json
            SaveSession(session);

            // РћС‡РёСЃС‚РєР° СЃС‚Р°СЂС‹С… С„Р°Р№Р»РѕРІ (СЃС‚Р°СЂС€Рµ 7 РґРЅРµР№)
            CleanupOldAutosaveFiles();
        }
        finally
        {
            _saveLock.Release();
        }
    }

    /// <summary>
    /// Р—Р°РіСЂСѓР·РёС‚СЊ СЃРµСЃСЃРёСЋ Р°РІС‚РѕСЃРѕС…СЂР°РЅРµРЅРёСЏ (РґР»СЏ РІРѕСЃСЃС‚Р°РЅРѕРІР»РµРЅРёСЏ РїРѕСЃР»Рµ СЃР±РѕСЏ).
    /// </summary>
    /// <returns>РЎРµСЃСЃРёСЏ РёР»Рё null, РµСЃР»Рё session.json РЅРµ РЅР°Р№РґРµРЅ.</returns>
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
            _logger?.LogWarning(ex, "РќРµ СѓРґР°Р»РѕСЃСЊ Р·Р°РіСЂСѓР·РёС‚СЊ СЃРµСЃСЃРёСЋ Р°РІС‚РѕСЃРѕС…СЂР°РЅРµРЅРёСЏ");
            return null;
        }
    }

    /// <summary>
    /// РџРѕР»СѓС‡РёС‚СЊ РїРѕР»РЅС‹Р№ РїСѓС‚СЊ Рє Р°РІС‚РѕСЃРѕС…СЂР°РЅС‘РЅРЅРѕРјСѓ С„Р°Р№Р»Сѓ.
    /// </summary>
    public string GetAutosaveFilePath(string autosaveFileName)
    {
        return Path.Combine(_autosaveFolder, autosaveFileName);
    }

    /// <summary>
    /// РћС‡РёСЃС‚РёС‚СЊ РїР°РїРєСѓ Р°РІС‚РѕСЃРѕС…СЂР°РЅРµРЅРёСЏ (РїРѕСЃР»Рµ СѓСЃРїРµС€РЅРѕРіРѕ РІРѕСЃСЃС‚Р°РЅРѕРІР»РµРЅРёСЏ РёР»Рё РїРѕ Р·Р°РїСЂРѕСЃСѓ).
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
                _logger?.LogWarning(ex, "РќРµ СѓРґР°Р»РѕСЃСЊ СѓРґР°Р»РёС‚СЊ С„Р°Р№Р» Р°РІС‚РѕСЃРѕС…СЂР°РЅРµРЅРёСЏ: {File}", file);
            }
        }

        _logger?.LogInformation("РџР°РїРєР° Р°РІС‚РѕСЃРѕС…СЂР°РЅРµРЅРёСЏ РѕС‡РёС‰РµРЅР°: {Folder}", _autosaveFolder);
    }

    #region Private Methods

    private void AutosaveTab(IAutosaveTab tab, AutosaveSession session)
    {
        var tabId = GetTabAutosaveId(tab);
        var fileName = $"autosave_{tabId}_{_dateTimeProvider.UtcNow:yyyyMMdd_HHmmss}.tdel";
        var filePath = Path.Combine(_autosaveFolder, fileName);

        // РЎРѕС…СЂР°РЅСЏРµРј С€Р°Р±Р»РѕРЅ
        // Template вЂ” СЌС‚Рѕ РѕР±СЉРµРєС‚, РїСЂРёРІРѕРґРёРј Рє С‚РёРїСѓ Template
        if (tab.Template is Models.Template template)
        {
            _templateService.Save(template, filePath);
        }

        // РЈРґР°Р»СЏРµРј РїСЂРµРґС‹РґСѓС‰РёР№ С„Р°Р№Р» Р°РІС‚РѕСЃРѕС…СЂР°РЅРµРЅРёСЏ РґР»СЏ СЌС‚РѕР№ РІРєР»Р°РґРєРё
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
            "РђРІС‚РѕСЃРѕС…СЂР°РЅРµРЅРёРµ РІРєР»Р°РґРєРё: {TabName} в†’ {FileName}", tab.DisplayName, fileName);
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
            // РќРµС‚ В«РіСЂСЏР·РЅС‹С…В» РІРєР»Р°РґРѕРє вЂ” СѓРґР°Р»СЏРµРј session.json
            if (File.Exists(_sessionFile))
                File.Delete(_sessionFile);
        }
    }

    /// <summary>
    /// РџРѕР»СѓС‡РёС‚СЊ СѓРЅРёРєР°Р»СЊРЅС‹Р№ ID Р°РІС‚РѕСЃРѕС…СЂР°РЅРµРЅРёСЏ РґР»СЏ РІРєР»Р°РґРєРё.
    /// </summary>
    private static string GetTabAutosaveId(IAutosaveTab tab)
    {
        return tab.TabId ??
               (string.IsNullOrEmpty(tab.FilePath)
                   ? Guid.NewGuid().ToString("N")[..8]
                   : Path.GetFileNameWithoutExtension(tab.FilePath));
    }

    /// <summary>
    /// РЈРґР°Р»РёС‚СЊ СЃС‚Р°СЂС‹Рµ Р°РІС‚РѕСЃРѕС…СЂР°РЅС‘РЅРЅС‹Рµ С„Р°Р№Р»С‹ РґР»СЏ РєРѕРЅРєСЂРµС‚РЅРѕР№ РІРєР»Р°РґРєРё.
    /// РћСЃС‚Р°РІР»СЏРµРј С‚РѕР»СЊРєРѕ РїРѕСЃР»РµРґРЅРёР№ С„Р°Р№Р».
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
                "РћС€РёР±РєР° РѕС‡РёСЃС‚РєРё СЃС‚Р°СЂС‹С… Р°РІС‚РѕСЃРѕС…СЂР°РЅРµРЅРёР№ РґР»СЏ РІРєР»Р°РґРєРё {TabId}", tabId);
        }
    }

    /// <summary>
    /// РЈРґР°Р»РёС‚СЊ Р°РІС‚РѕСЃРѕС…СЂР°РЅС‘РЅРЅС‹Рµ С„Р°Р№Р»С‹ СЃС‚Р°СЂС€Рµ 7 РґРЅРµР№.
    /// </summary>
    private void CleanupOldAutosaveFiles()
    {
        try
        {
            var cutoffDate = _dateTimeProvider.UtcNow.AddDays(-7);
            var files = Directory.GetFiles(_autosaveFolder, "autosave_*.tdel");

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.LastWriteTime < cutoffDate)
                {
                    File.Delete(file);
                    _logger?.LogDebug("РЈРґР°Р»С‘РЅ СЃС‚Р°СЂС‹Р№ Р°РІС‚РѕСЃРѕС…СЂР°РЅС‘РЅРЅС‹Р№ С„Р°Р№Р»: {File}", file);
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "РћС€РёР±РєР° РѕС‡РёСЃС‚РєРё СЃС‚Р°СЂС‹С… Р°РІС‚РѕСЃРѕС…СЂР°РЅС‘РЅРЅС‹С… С„Р°Р№Р»РѕРІ");
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


