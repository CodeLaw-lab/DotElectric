using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DotElectric.TemplateEditor.Messages;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Views;
using Microsoft.Extensions.Logging;

namespace DotElectric.TemplateEditor.ViewModels;

/// <summary>
/// Р“Р»Р°РІРЅС‹Р№ ViewModel РїСЂРёР»РѕР¶РµРЅРёСЏ.
/// РЈРїСЂР°РІР»СЏРµС‚ РІРєР»Р°РґРєР°РјРё, РіР»РѕР±Р°Р»СЊРЅС‹РјРё РєРѕРјР°РЅРґР°РјРё, DI.
/// </summary>
public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly ITemplateService _templateService;
    private readonly IFileService _fileService;
    private readonly IDialogService _dialogService;
    private readonly ISettingsService _settingsService;
    private readonly IThemeService _themeService;
    private readonly ITemplateLibraryService _templateLibraryService;
    private readonly IPrintService _printService;
    private readonly IPrintDocumentGenerator _printDocumentGenerator;
    private readonly IDialogHostService _dialogHostService;
    private readonly IApplicationLifecycle _applicationLifecycle;
    private readonly ILogger<MainViewModel> _logger;
    private readonly IEditorViewModelFactory _editorViewModelFactory;
    private readonly AutosaveService _autosaveService;
    private bool _isDisposed;

    private async Task OnAutosaveTickHandler()
    {
        try
        {
            await _autosaveService.AutosaveAllTabsAsync(OpenedTabs);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Autosave failed");
        }
    }

    /// <summary>
    /// РћС‚РєСЂС‹С‚С‹Рµ РІРєР»Р°РґРєРё.
    /// </summary>
    public ObservableCollection<EditorViewModel> OpenedTabs { get; } = new();

    /// <summary>
    /// ViewModel Р±РёР±Р»РёРѕС‚РµРєРё С€Р°Р±Р»РѕРЅРѕРІ.
    /// </summary>
    public TemplateLibraryViewModel TemplateLibraryVm { get; }

    /// <summary>
    /// РђРєС‚РёРІРЅР°СЏ РІРєР»Р°РґРєР°.
    /// </summary>
    [ObservableProperty]
    private EditorViewModel? _selectedTab;

    /// <summary>
    /// РўРµРјР° РѕС„РѕСЂРјР»РµРЅРёСЏ.
    /// </summary>
    [ObservableProperty]
    private string _theme = "Light";

    // === РљРѕРЅСЃС‚СЂСѓРєС‚РѕСЂ ===

    public MainViewModel(
        ITemplateService templateService,
        IFileService fileService,
        IDialogService dialogService,
        ISettingsService settingsService,
        IThemeService themeService,
        ITemplateLibraryService templateLibraryService,
        IPrintService printService,
        IPrintDocumentGenerator printDocumentGenerator,
        IDialogHostService dialogHostService,
        IApplicationLifecycle applicationLifecycle,
        ILogger<MainViewModel> logger,
        IEditorViewModelFactory editorViewModelFactory,
        AutosaveService autosaveService)
    {
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _templateLibraryService = templateLibraryService ?? throw new ArgumentNullException(nameof(templateLibraryService));
        _printService = printService ?? throw new ArgumentNullException(nameof(printService));
        _printDocumentGenerator = printDocumentGenerator ?? throw new ArgumentNullException(nameof(printDocumentGenerator));
        _dialogHostService = dialogHostService ?? throw new ArgumentNullException(nameof(dialogHostService));
        _applicationLifecycle = applicationLifecycle ?? throw new ArgumentNullException(nameof(applicationLifecycle));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _editorViewModelFactory = editorViewModelFactory ?? throw new ArgumentNullException(nameof(editorViewModelFactory));
        _autosaveService = autosaveService ?? throw new ArgumentNullException(nameof(autosaveService));

        TemplateLibraryVm = new TemplateLibraryViewModel(
            templateLibraryService,
            OnTemplateDoubleClicked,
            fileService);

        // РџРѕРґРїРёСЃРєР° РЅР° СЃРѕРѕР±С‰РµРЅРёСЏ Рѕ Р·Р°РєСЂС‹С‚РёРё РІРєР»Р°РґРѕРє (РѕС‚ EditorViewModel)
        WeakReferenceMessenger.Default.Register<CloseTabRequestMessage>(this, (r, m) =>
        {
            _ = CloseTab(m.Tab);
        });
        WeakReferenceMessenger.Default.Register<CloseOtherTabsRequestMessage>(this, (r, m) =>
        {
            _ = CloseOtherTabs(m.Tab);
        });
        WeakReferenceMessenger.Default.Register<CloseAllTabsRequestMessage>(this, (r, m) =>
        {
            _ = CloseAllTabsAsync();
        });

        // РџРѕРґРїРёСЃРєР° РЅР° Р°РІС‚РѕСЃРѕС…СЂР°РЅРµРЅРёРµ
        _autosaveService.AutosaveTick += OnAutosaveTickHandler;
        _autosaveService.Start();

        // Р—Р°РіСЂСѓР·РєР° РЅР°СЃС‚СЂРѕРµРє
        var settings = _settingsService.Load();
        Theme = settings.Theme;
    }

    /// <summary>
    /// РћР±СЂР°Р±РѕС‚С‡РёРє РґРІРѕР№РЅРѕРіРѕ РєР»РёРєР° РїРѕ С€Р°Р±Р»РѕРЅСѓ РІ Р±РёР±Р»РёРѕС‚РµРєРµ.
    /// РћС‚РєСЂС‹РІР°РµС‚ С€Р°Р±Р»РѕРЅ РІ РЅРѕРІРѕР№ РІРєР»Р°РґРєРµ.
    /// </summary>
    private void OnTemplateDoubleClicked(TemplateInfo templateInfo)
    {
        try
        {
            var template = _templateService.Load(templateInfo.FullPath);
            if (template != null)
            {
                var editorVm = _editorViewModelFactory.CreateWithFilePath(template, templateInfo.FullPath, printService: _printService);
                OpenedTabs.Add(editorVm);
                SelectedTab = editorVm;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "РћС€РёР±РєР° Р·Р°РіСЂСѓР·РєРё С€Р°Р±Р»РѕРЅР° РёР· Р±РёР±Р»РёРѕС‚РµРєРё: {FilePath}", templateInfo.FullPath);
            _dialogService.ShowError($"РћС€РёР±РєР° Р·Р°РіСЂСѓР·РєРё С€Р°Р±Р»РѕРЅР°: {ex.Message}");
        }
    }

    // === РљРѕРјР°РЅРґС‹ ===

    /// <summary>
    /// РЎРѕР·РґР°С‚СЊ РЅРѕРІСѓСЋ РІРєР»Р°РґРєСѓ (РїСѓСЃС‚РѕР№ С€Р°Р±Р»РѕРЅ Р·Р°РґР°РЅРЅРѕРіРѕ С„РѕСЂРјР°С‚Р°).
    /// РџРѕРґРґРµСЂР¶РёРІР°РµРјС‹Рµ С„РѕСЂРјР°С‚С‹: "A0", "A1", "A2", "A3", "A4" РёР»Рё СЃ СЃСѓС„С„РёРєСЃРѕРј РѕСЂРёРµРЅС‚Р°С†РёРё: "A4P", "A4L", "A3P", "A3L" Рё С‚.Рґ.
    /// Р•СЃР»Рё format = null, РёСЃРїРѕР»СЊР·СѓРµС‚СЃСЏ РїРѕСЃР»РµРґРЅРёР№ СЃРѕС…СЂР°РЅС‘РЅРЅС‹Р№ С„РѕСЂРјР°С‚.
    /// </summary>
    [RelayCommand]
    private void NewTab(string? format = null)
    {
        var rawFormat = format ?? _settingsService.Get("LastUsedSheetFormat", "A3");

        // РџР°СЂСЃРёРј С„РѕСЂРјР°С‚ Рё РѕСЂРёРµРЅС‚Р°С†РёСЋ РёР· СЃС‚СЂРѕРєРё РІРёРґР° "A4P", "A3L", "A4", "A3"
        var fmt = ParseSheetFormat(rawFormat, out var orientation);

        // Р•СЃР»Рё РѕСЂРёРµРЅС‚Р°С†РёСЏ РЅРµ СѓРєР°Р·Р°РЅР° вЂ” РёСЃРїРѕР»СЊР·СѓРµРј РїРѕСЃР»РµРґРЅСЋСЋ СЃРѕС…СЂР°РЅС‘РЅРЅСѓСЋ РёР»Рё РґРµС„РѕР»С‚РЅСѓСЋ РґР»СЏ С„РѕСЂРјР°С‚Р°
        if (orientation == null)
        {
            var orientStr = _settingsService.Get("LastUsedSheetOrientation", "Landscape");
            orientation = Enum.TryParse<SheetOrientation>(orientStr, true, out var parsed)
                ? parsed
                : Sheet.GetDefaultOrientation(fmt);
        }

        _settingsService.Set("LastUsedSheetFormat", fmt);
        _settingsService.Set("LastUsedSheetOrientation", orientation.Value.ToString());

        var template = _templateService.CreateNew(fmt, orientation.Value);
        var editor = _editorViewModelFactory.Create(template, printService: _printService);
        OpenedTabs.Add(editor);
        SelectedTab = editor;
    }

    /// <summary>
    /// РџР°СЂСЃРёС‚ СЃС‚СЂРѕРєСѓ С„РѕСЂРјР°С‚Р° Рё РІРѕР·РІСЂР°С‰Р°РµС‚ Р±Р°Р·РѕРІС‹Р№ С„РѕСЂРјР°С‚ + РѕСЂРёРµРЅС‚Р°С†РёСЋ (РµСЃР»Рё СѓРєР°Р·Р°РЅР°).
    /// РџСЂРёРјРµСЂС‹: "A4" в†’ ("A4", null), "A4P" в†’ ("A4", Portrait), "A4L" в†’ ("A4", Landscape)
    /// </summary>
    private static string ParseSheetFormat(string rawFormat, out SheetOrientation? orientation)
    {
        orientation = null;

        if (string.IsNullOrEmpty(rawFormat) || rawFormat.Length < 2)
            return rawFormat;

        // РџРѕСЃР»РµРґРЅРёР№ СЃРёРјРІРѕР» вЂ” СЃСѓС„С„РёРєСЃ РѕСЂРёРµРЅС‚Р°С†РёРё
        var suffix = rawFormat[^1].ToString().ToUpperInvariant();
        var baseFormat = rawFormat[..^1];

        if (suffix == "P")
        {
            orientation = SheetOrientation.Portrait;
            return baseFormat;
        }

        if (suffix == "L")
        {
            orientation = SheetOrientation.Landscape;
            return baseFormat;
        }

        // РќРµС‚ СЃСѓС„С„РёРєСЃР° вЂ” РІРµСЂРЅСѓС‚СЊ РєР°Рє РµСЃС‚СЊ
        return rawFormat;
    }

    /// <summary>
    /// РЎРѕР·РґР°С‚СЊ РЅРѕРІСѓСЋ РІРєР»Р°РґРєСѓ СЃ РїРѕСЃР»РµРґРЅРёРј РёСЃРїРѕР»СЊР·РѕРІР°РЅРЅС‹Рј С„РѕСЂРјР°С‚РѕРј (РґР»СЏ Ctrl+N Рё Toolbar).
    /// </summary>
    [RelayCommand]
    private void NewTabWithLastFormat()
    {
        var fmt = _settingsService.Get("LastUsedSheetFormat", "A3");
        NewTab(fmt);
    }

    /// <summary>
    /// РЎРѕР·РґР°С‚СЊ РЅРѕРІСѓСЋ РІРєР»Р°РґРєСѓ СЃ РїРѕР»СЊР·РѕРІР°С‚РµР»СЊСЃРєРёРј С„РѕСЂРјР°С‚РѕРј (РґРёР°Р»РѕРі РІРІРѕРґР° СЂР°Р·РјРµСЂРѕРІ).
    /// </summary>
    [RelayCommand]
    private void OpenSettings()
    {
        var dialogVm = new SettingsViewModel(_settingsService);
        _dialogHostService.ShowDialog(dialogVm);
    }

    [RelayCommand]
    private async Task NewCustomTabAsync()
    {
        var dialogVm = new CustomSheetDialogViewModel();
        var result = _dialogHostService.ShowDialog(dialogVm);

            if (result == true && dialogVm.WidthMm > 0 && dialogVm.HeightMm > 0)
            {
                var sheet = Sheet.Custom(dialogVm.WidthMm, dialogVm.HeightMm);
                var template = _templateService.CreateFromSheet(sheet);
                var editor = _editorViewModelFactory.Create(template, printService: _printService);
                OpenedTabs.Add(editor);
                SelectedTab = editor;
            }
    }

    /// <summary>
    /// РћС‚РєСЂС‹С‚СЊ С„Р°Р№Р» РІ РЅРѕРІРѕР№ РІРєР»Р°РґРєРµ.
    /// </summary>
    [RelayCommand]
    private async Task OpenFileAsync()
    {
        var filePath = _fileService.OpenFileDialog("DotElectric Template|*.tdel");
        if (string.IsNullOrEmpty(filePath)) return;

        try
        {
            var template = _templateService.Load(filePath);
            var editor = _editorViewModelFactory.CreateWithFilePath(template, filePath, printService: _printService);
            OpenedTabs.Add(editor);
            SelectedTab = editor;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "РќРµ СѓРґР°Р»РѕСЃСЊ РѕС‚РєСЂС‹С‚СЊ С„Р°Р№Р»: {FilePath}", filePath);
            _dialogService.ShowError($"РќРµ СѓРґР°Р»РѕСЃСЊ РѕС‚РєСЂС‹С‚СЊ С„Р°Р№Р»: {ex.Message}");
        }
    }

    /// <summary>
    /// РЎРѕС…СЂР°РЅРёС‚СЊ Р°РєС‚РёРІРЅСѓСЋ РІРєР»Р°РґРєСѓ.
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedTab == null) return;
        await SaveTabAsync(SelectedTab);
    }

    /// <summary>
    /// РЎРѕС…СЂР°РЅРёС‚СЊ РІСЃРµ РІРєР»Р°РґРєРё.
    /// </summary>
    [RelayCommand]
    private async Task SaveAllAsync()
    {
        foreach (var tab in OpenedTabs.ToList())
            await SaveTabAsync(tab);
    }

    /// <summary>
    /// РЎРѕС…СЂР°РЅРёС‚СЊ РєРѕРЅРєСЂРµС‚СѓСЋ РІРєР»Р°РґРєСѓ.
    /// </summary>
    private async Task SaveTabAsync(EditorViewModel tab)
    {
        if (tab == null) return;

        var path = tab.DirtyStateManager.FilePath;
        try
        {
            if (string.IsNullOrEmpty(path))
            {
                path = _fileService.SaveFileDialog("DotElectric Template|*.tdel", tab.DirtyStateManager.DisplayName);
                if (string.IsNullOrEmpty(path)) return;
            }

            // РџРµСЂРµРґ СЃРѕС…СЂР°РЅРµРЅРёРµРј вЂ” РІР°Р»РёРґР°С†РёСЏ
            var errors = _templateService.Validate(tab.Template).ToList();
            if (errors.Count > 0)
            {
                var errorText = string.Join("\n", errors);
                var result = await _dialogService.ShowUnsavedChangesDialogAsync(
                    $"Р’ С€Р°Р±Р»РѕРЅРµ РµСЃС‚СЊ РѕС€РёР±РєРё:\n{errorText}\n\nР’СЃС‘ СЂР°РІРЅРѕ СЃРѕС…СЂР°РЅРёС‚СЊ?");
                if (result != UnsavedChangesResult.Save)
                    return;
            }

            // Р‘СЌРєР°Рї РїРµСЂРµРґ СЃРѕС…СЂР°РЅРµРЅРёРµРј (РµСЃР»Рё С„Р°Р№Р» СѓР¶Рµ СЃСѓС‰РµСЃС‚РІСѓРµС‚)
            if (!string.IsNullOrEmpty(tab.DirtyStateManager.FilePath) && System.IO.File.Exists(tab.DirtyStateManager.FilePath))
            {
                _fileService.CreateBackup(tab.DirtyStateManager.FilePath);
            }

            _templateService.Save(tab.Template, path);
            tab.DirtyStateManager.FilePath = path;
            tab.ClearDirty();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "РќРµ СѓРґР°Р»РѕСЃСЊ СЃРѕС…СЂР°РЅРёС‚СЊ С„Р°Р№Р»: {FilePath}", path ?? tab.DirtyStateManager.FilePath);
            _dialogService.ShowError($"РќРµ СѓРґР°Р»РѕСЃСЊ СЃРѕС…СЂР°РЅРёС‚СЊ С„Р°Р№Р»: {ex.Message}");
        }
    }

    /// <summary>
    /// РЎРѕС…СЂР°РЅРёС‚СЊ РєР°Рє.
    /// </summary>
    [RelayCommand]
    private async Task SaveAsAsync()
    {
        if (SelectedTab == null) return;

        var path = _fileService.SaveFileDialog("DotElectric Template|*.tdel", SelectedTab.DirtyStateManager.DisplayName);
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            _templateService.Save(SelectedTab.Template, path);
            SelectedTab.DirtyStateManager.FilePath = path;
            SelectedTab.ClearDirty();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "РќРµ СѓРґР°Р»РѕСЃСЊ СЃРѕС…СЂР°РЅРёС‚СЊ С„Р°Р№Р» (Save As): {FilePath}", path);
            _dialogService.ShowError($"РќРµ СѓРґР°Р»РѕСЃСЊ СЃРѕС…СЂР°РЅРёС‚СЊ С„Р°Р№Р»: {ex.Message}");
        }
    }

    /// <summary>
    /// Р—Р°РєСЂС‹С‚СЊ РІРєР»Р°РґРєСѓ.
    /// </summary>
    [RelayCommand]
    private async Task CloseTab(EditorViewModel tab)
    {
        if (tab == null) return;

        if (tab.DirtyStateManager.IsDirty)
        {
            var result = await _dialogService.ShowUnsavedChangesDialogAsync(tab.DirtyStateManager.DisplayName);
            if (result == UnsavedChangesResult.Cancel) return;
            if (result == UnsavedChangesResult.Save)
                await SaveTabAsync(tab);
        }

        OpenedTabs.Remove(tab);
        tab.Dispose();

        // Р•СЃР»Рё Р·Р°РєСЂС‹Р»Рё Р°РєС‚РёРІРЅСѓСЋ РІРєР»Р°РґРєСѓ вЂ” РІС‹Р±СЂР°С‚СЊ РґСЂСѓРіСѓСЋ
        if (SelectedTab == tab)
            SelectedTab = OpenedTabs.LastOrDefault();
    }

    /// <summary>
    /// Р—Р°РєСЂС‹С‚СЊ РІСЃРµ РІРєР»Р°РґРєРё.
    /// </summary>
    [RelayCommand]
    private async Task CloseAllTabsAsync()
    {
        foreach (var tab in OpenedTabs.ToList())
            await CloseTab(tab);
    }

    /// <summary>
    /// Р—Р°РєСЂС‹С‚СЊ РІСЃРµ РІРєР»Р°РґРєРё, РєСЂРѕРјРµ СѓРєР°Р·Р°РЅРЅРѕР№.
    /// </summary>
    [RelayCommand]
    private async Task CloseOtherTabs(EditorViewModel tab)
    {
        if (tab == null) return;

        var tabsToClose = OpenedTabs.Where(t => t != tab).ToList();
        foreach (var t in tabsToClose)
            await CloseTab(t);
    }

    /// <summary>
    /// РџРµСЂРµРєР»СЋС‡РёС‚СЊ С‚РµРјСѓ РѕС„РѕСЂРјР»РµРЅРёСЏ.
    /// </summary>
    [RelayCommand]
    private void ToggleTheme()
    {
        var newTheme = _themeService.ToggleTheme();
        Theme = newTheme;
    }

    /// <summary>
    /// РџРµС‡Р°С‚СЊ Р°РєС‚РёРІРЅРѕР№ РІРєР»Р°РґРєРё.
    /// </summary>
    [RelayCommand]
    private void Print()
    {
        if (SelectedTab == null) return;
        SelectedTab.PrintCommand.Execute(null);
    }

    /// <summary>
    /// РџСЂРµРґРїСЂРѕСЃРјРѕС‚СЂ РїРµС‡Р°С‚Рё Р°РєС‚РёРІРЅРѕР№ РІРєР»Р°РґРєРё.
    /// </summary>
    [RelayCommand]
    private void PreviewPrint()
    {
        if (SelectedTab == null) return;

        try
        {
            var document = _printDocumentGenerator.Generate(SelectedTab.Template);
            var window = new PrintPreviewWindow(document, SelectedTab.DirtyStateManager.DisplayName);
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.ShowDialog();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "РћС€РёР±РєР° РїСЂРё РіРµРЅРµСЂР°С†РёРё РїСЂРµРґРїСЂРѕСЃРјРѕС‚СЂР° РїРµС‡Р°С‚Рё");
            _dialogService.ShowError($"РћС€РёР±РєР° РїСЂРµРґРїСЂРѕСЃРјРѕС‚СЂР°: {ex.Message}");
        }
    }

    /// <summary>
    /// Р’С‹С…РѕРґ РёР· РїСЂРёР»РѕР¶РµРЅРёСЏ.
    /// </summary>
    [RelayCommand]
    private void Exit()
    {
        _applicationLifecycle.Shutdown();
    }

    // === Р’СЃРїРѕРјРѕРіР°С‚РµР»СЊРЅС‹Рµ ===

    /// <summary>
    /// РњРѕР¶РЅРѕ Р»Рё Р·Р°РєСЂС‹С‚СЊ С…РѕС‚СЏ Р±С‹ РѕРґРЅСѓ РІРєР»Р°РґРєСѓ.
    /// </summary>
    public bool CanCloseAnyTab() => OpenedTabs.Count > 0;

    /// <summary>
    /// РњРѕР¶РЅРѕ Р»Рё СЃРѕС…СЂР°РЅРёС‚СЊ.
    /// </summary>
    public bool CanSave() => SelectedTab != null;

    /// <summary>
    /// РћСЃРІРѕР±РѕР¶РґР°РµС‚ СЂРµСЃСѓСЂСЃС‹ (РѕС‚РїРёСЃРєР° РѕС‚ WeakReferenceMessenger).
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;
        _autosaveService.AutosaveTick -= OnAutosaveTickHandler;
        WeakReferenceMessenger.Default.UnregisterAll(this);
        _isDisposed = true;
    }
}

