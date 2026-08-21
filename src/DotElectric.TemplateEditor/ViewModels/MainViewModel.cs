using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using DotElectric.TemplateEditor.Messages;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.ViewModels.Abstractions;
using Microsoft.Extensions.Logging;

namespace DotElectric.TemplateEditor.ViewModels;

/// <summary>
/// Главный ViewModel приложения.
/// Управляет вкладками, глобальными командами, DI.
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
    /// Открытые вкладки.
    /// </summary>
    public ObservableCollection<EditorViewModel> OpenedTabs { get; } = new();

    /// <summary>
    /// ViewModel библиотеки шаблонов.
    /// </summary>
    public TemplateLibraryViewModel TemplateLibraryVm { get; }

    /// <summary>
    /// Активная вкладка.
    /// </summary>
    [ObservableProperty]
    private EditorViewModel? _selectedTab;

    /// <summary>
    /// Тема оформления.
    /// </summary>
    [ObservableProperty]
    private string _theme = "Light";

    /// <summary>
    /// Модель меню «Файл > Новый шаблон» — генерируется из каталога
    /// стандартных форматов (единственный источник списка и размеров).
    /// </summary>
    public IReadOnlyList<NewSheetMenuEntry> NewSheetMenu { get; } = BuildNewSheetMenu();

    // === Конструктор ===

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

        // Подписка на сообщения о закрытии вкладок (от EditorViewModel)
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

        // Подписка на автосохранение
        _autosaveService.AutosaveTick += OnAutosaveTickHandler;
        _autosaveService.Start();

        // Загрузка настроек
        var settings = _settingsService.Load();
        Theme = settings.Theme;
    }

    /// <summary>
    /// Обработчик двойного клика по шаблону в библиотеке.
    /// Открывает шаблон в новой вкладке.
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

    // === Команды ===

    /// <summary>
    /// Создать новую вкладку. Пункт меню несёт формат и ориентацию;
    /// пустой пункт — последний использованный формат из настроек,
    /// ориентация — по цепочке запасных значений сервиса вкладок.
    /// </summary>
    [RelayCommand]
    private void NewTab(NewSheetOrientationEntry? entry = null)
    {
        var settings = _settingsService.Load();
        var format = entry?.Format ?? settings.LastUsedSheetFormat;
        var lastOrient = settings.LastUsedSheetOrientation;
        var editor = _tabOperations.CreateNewTab(format, entry?.Orientation, null, lastOrient);
        OpenedTabs.Add(editor);
        SelectedTab = editor;
    }

    /// <summary>
    /// Создать новую вкладку с последним использованным форматом (для Ctrl+N и Toolbar).
    /// </summary>
    [RelayCommand]
    private void NewTabWithLastFormat()
    {
        var settings = _settingsService.Load();
        var fmt = settings.LastUsedSheetFormat;
        var orient = settings.LastUsedSheetOrientation;
        var editor = _tabOperations.CreateNewTab(null, null, fmt, orient);
        OpenedTabs.Add(editor);
        SelectedTab = editor;
    }

    /// <summary>
    /// Создать новую вкладку с пользовательским форматом (диалог ввода размеров).
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
    /// Собирает модель меню «Новый шаблон» из каталога: группы форматов
    /// в порядке каталога, разделитель перед полуформатами и после последней
    /// группы, затем пункт «Пользовательский...».
    /// </summary>
    private static IReadOnlyList<NewSheetMenuEntry> BuildNewSheetMenu()
    {
        var entries = new List<NewSheetMenuEntry>();
        var halfFormatSeparatorAdded = false;

        foreach (var format in SheetFormatCatalog.All)
        {
            if (!halfFormatSeparatorAdded && format.IsHalfFormat)
            {
                entries.Add(new NewSheetMenuEntry(NewSheetMenuKind.Separator, string.Empty));
                halfFormatSeparatorAdded = true;
            }

            entries.Add(new NewSheetMenuEntry(NewSheetMenuKind.FormatGroup, format.Name, BuildOrientations(format)));
        }

        entries.Add(new NewSheetMenuEntry(NewSheetMenuKind.Separator, string.Empty));
        entries.Add(new NewSheetMenuEntry(NewSheetMenuKind.CustomCommand, "Пользовательский..."));
        return entries;
    }

    /// <summary>
    /// Пункты ориентаций группы: ориентация по умолчанию первая.
    /// Заголовки компонуются из размеров каталога («Альбомная (1189×841)»).
    /// </summary>
    private static IReadOnlyList<NewSheetOrientationEntry> BuildOrientations(SheetFormat format)
    {
        var longMm = Coordinate.FormatMm(format.LongSideMicrons);
        var shortMm = Coordinate.FormatMm(format.ShortSideMicrons);
        var landscape = new NewSheetOrientationEntry($"Альбомная ({longMm}×{shortMm})", format.Name, SheetOrientation.Landscape);
        var portrait = new NewSheetOrientationEntry($"Книжная ({shortMm}×{longMm})", format.Name, SheetOrientation.Portrait);

        return format.DefaultOrientation == SheetOrientation.Portrait
            ? [portrait, landscape]
            : [landscape, portrait];
    }

    /// <summary>
    /// Открыть файл в новой вкладке.
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
    /// Сохранить активную вкладку.
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        if (SelectedTab == null) return;
        await _tabOperations.SaveTabAsync(SelectedTab);
    }

    /// <summary>
    /// Сохранить все вкладки.
    /// </summary>
    [RelayCommand]
    private async Task SaveAllAsync()
    {
        foreach (var tab in OpenedTabs.ToList())
            await _tabOperations.SaveTabAsync(tab);
    }

    /// <summary>
    /// Сохранить как.
    /// </summary>
    [RelayCommand]
    private async Task SaveAsAsync()
    {
        if (SelectedTab == null) return;
        await _tabOperations.SaveAsAsync(SelectedTab);
    }

    /// <summary>
    /// Закрыть вкладку.
    /// </summary>
    [RelayCommand]
    private async Task CloseTab(EditorViewModel tab)
    {
        if (tab == null) return;

        if (!await _tabOperations.PromptAndSaveIfDirtyAsync(tab))
            return;

        OpenedTabs.Remove(tab);
        tab.Dispose();

        // Если закрыли активную вкладку — выбрать другую
        if (SelectedTab == tab)
            SelectedTab = OpenedTabs.LastOrDefault();
    }

    /// <summary>
    /// Закрыть все вкладки.
    /// </summary>
    [RelayCommand]
    private async Task CloseAllTabsAsync()
    {
        foreach (var tab in OpenedTabs.ToList())
            await CloseTab(tab);
    }

    /// <summary>
    /// Закрыть все вкладки, кроме указанной.
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
    /// Переключить тему оформления.
    /// </summary>
    [RelayCommand]
    private void ToggleTheme()
    {
        var newTheme = _themeService.ToggleTheme();
        Theme = newTheme;
    }

    /// <summary>
    /// Печать активной вкладки.
    /// </summary>
    [RelayCommand]
    private void Print()
    {
        if (SelectedTab == null) return;
        SelectedTab.PrintCommand.Execute(null);
    }

    /// <summary>
    /// Предпросмотр печати активной вкладки.
    /// </summary>
    [RelayCommand]
    private void PreviewPrint()
    {
        if (SelectedTab == null) return;
        try
        {
            var document = _printDocumentGenerator.Generate(SelectedTab.Template);
            var previewViewModel = new PrintPreviewViewModel(document, SelectedTab.DirtyStateManager.DisplayName);
            _dialogHostService.ShowDialog(previewViewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating print preview");
            _dialogService.ShowError($"Print preview error: {ex.Message}");
        }
    }

    /// <summary>
    /// Выход из приложения.
    /// </summary>
    [RelayCommand]
    private void Exit()
    {
        _applicationLifecycle.Shutdown();
    }

    // === Вспомогательные ===

    /// <summary>
    /// Можно ли закрыть хотя бы одну вкладку.
    /// </summary>
    public bool CanCloseAnyTab() => OpenedTabs.Count > 0;

    /// <summary>
    /// Можно ли сохранить.
    /// </summary>
    public bool CanSave() => SelectedTab != null;

    /// <summary>
    /// Освобождает ресурсы (отписка от WeakReferenceMessenger).
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;
        _autosaveService.AutosaveTick -= OnAutosaveTickHandler;
        WeakReferenceMessenger.Default.UnregisterAll(this);
        _isDisposed = true;
    }
}

