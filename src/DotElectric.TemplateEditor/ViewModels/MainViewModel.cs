using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DotElectric.TemplateEditor.Messages;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.ViewModels.Abstractions;
using DotElectric.TemplateEditor.Views;
using Microsoft.Extensions.Logging;

namespace DotElectric.TemplateEditor.ViewModels;

/// <summary>
/// Р“Р»Р°РІРЅС‹Р№ ViewModel РїСЂРёР»РѕР¶РµРЅРёСЏ.
/// РЈРїСЂР°РІР»СЏРµС‚ РІРєР»Р°РґРєР°РјРё, РіР»РѕР±Р°Р»СЊРЅС‹РјРё РєРѕРјР°РЅРґР°РјРё, DI.
/// </summary>
public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly ITabOperationsService _tabOperations;
    private readonly ISettingsService _settingsService;
    private readonly IThemeService _themeService;
    private readonly ITemplateLibraryService _templateLibraryService;
    private readonly IDialogService _dialogService;
    private readonly IDialogHostService _dialogHostService;
    private readonly IApplicationLifecycle _applicationLifecycle;
    private readonly ILogger<MainViewModel> _logger;
    private readonly AutosaveService _autosaveService;
    private readonly IPrintDocumentGenerator _printDocumentGenerator;
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
        ITabOperationsService tabOperations,
        ISettingsService settingsService,
        IThemeService themeService,
        ITemplateLibraryService templateLibraryService,
        IDialogService dialogService,
        IDialogHostService dialogHostService,
        IApplicationLifecycle applicationLifecycle,
        ILogger<MainViewModel> logger,
        AutosaveService autosaveService,
        IPrintDocumentGenerator printDocumentGenerator)
    {
        _tabOperations = tabOperations ?? throw new ArgumentNullException(nameof(tabOperations));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _templateLibraryService = templateLibraryService ?? throw new ArgumentNullException(nameof(templateLibraryService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _dialogHostService = dialogHostService ?? throw new ArgumentNullException(nameof(dialogHostService));
        _applicationLifecycle = applicationLifecycle ?? throw new ArgumentNullException(nameof(applicationLifecycle));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _autosaveService = autosaveService ?? throw new ArgumentNullException(nameof(autosaveService));
        _printDocumentGenerator = printDocumentGenerator ?? throw new ArgumentNullException(nameof(printDocumentGenerator));

        TemplateLibraryVm = new TemplateLibraryViewModel(
            templateLibraryService,
            OnTemplateDoubleClicked);

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
        var editor = _tabOperations.OpenFromFilePath(templateInfo.FullPath);
        if (editor != null)
        {
            OpenedTabs.Add(editor);
            SelectedTab = editor;
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
        var lastOrient = _settingsService.Get("LastUsedSheetOrientation", "Landscape");
        var editor = _tabOperations.CreateNewTab(rawFormat, null, lastOrient);
        OpenedTabs.Add(editor);
        SelectedTab = editor;
    }

    /// <summary>
    /// РЎРѕР·РґР°С‚СЊ РЅРѕРІСѓСЋ РІРєР»Р°РґРєСѓ СЃ РїРѕСЃР»РµРґРЅРёРј РёСЃРїРѕР»СЊР·РѕРІР°РЅРЅС‹Рј С„РѕСЂРјР°С‚РѕРј (РґР»СЏ Ctrl+N Рё Toolbar).
    /// </summary>
    [RelayCommand]
    private void NewTabWithLastFormat()
    {
        var fmt = _settingsService.Get("LastUsedSheetFormat", "A3");
        var orient = _settingsService.Get("LastUsedSheetOrientation", "Landscape");
        var editor = _tabOperations.CreateNewTab(null, fmt, orient);
        OpenedTabs.Add(editor);
        SelectedTab = editor;
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
                var editor = _tabOperations.CreateNewCustomTab(dialogVm.WidthMm, dialogVm.HeightMm);
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
        var editor = await _tabOperations.OpenFileAsync();
        if (editor != null)
        {
            OpenedTabs.Add(editor);
            SelectedTab = editor;
        }
    }

    /// <summary>
    /// РЎРѕС…СЂР°РЅРёС‚СЊ Р°РєС‚РёРІРЅСѓСЋ РІРєР»Р°РґРєСѓ.
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedTab == null) return;
        await _tabOperations.SaveTabAsync(SelectedTab);
    }

    /// <summary>
    /// РЎРѕС…СЂР°РЅРёС‚СЊ РІСЃРµ РІРєР»Р°РґРєРё.
    /// </summary>
    [RelayCommand]
    private async Task SaveAllAsync()
    {
        foreach (var tab in OpenedTabs.ToList())
            await _tabOperations.SaveTabAsync(tab);
    }

    /// <summary>
    /// РЎРѕС…СЂР°РЅРёС‚СЊ РєР°Рє.
    /// </summary>
    [RelayCommand]
    private async Task SaveAsAsync()
    {
        if (SelectedTab == null) return;
        await _tabOperations.SaveAsAsync(SelectedTab);
    }

    /// <summary>
    /// Р—Р°РєСЂС‹С‚СЊ РІРєР»Р°РґРєСѓ.
    /// </summary>
    [RelayCommand]
    private async Task CloseTab(EditorViewModel tab)
    {
        if (tab == null) return;

        if (!await _tabOperations.PromptAndSaveIfDirtyAsync(tab))
            return;

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
            _logger.LogError(ex, "Error generating print preview");
            _dialogService.ShowError($"Print preview error: {ex.Message}");
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

