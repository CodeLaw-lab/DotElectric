using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;

namespace DotElectric.TemplateEditor.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;

    [ObservableProperty]
    private string _theme;

    [ObservableProperty]
    private bool _showGrid;

    [ObservableProperty]
    private bool _snapToGrid;

    [ObservableProperty]
    private double _gridStepMm;

    [ObservableProperty]
    private int _gridMaxNodes;

    [ObservableProperty]
    private bool _gridNodeColorAuto;

    [ObservableProperty]
    private string _gridNodeColor = "#C0C0C0";

    [ObservableProperty]
    private double _gridNodeSize;

    [ObservableProperty]
    private int _autosaveIntervalMinutes;

    [ObservableProperty]
    private string _defaultSheetFormat;

    public string Title => "Настройки";

    public string[] ThemeOptions { get; } = ["Light", "Dark"];

    public string[] FormatOptions { get; } =
        SheetFormatCatalog.All.Select(f => f.Name).ToArray();

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

        var settings = _settingsService.Load();
        Theme = settings.Theme;
        ShowGrid = settings.ShowGrid;
        SnapToGrid = settings.SnapToGrid;
        GridStepMm = settings.GridStepMm;
        GridMaxNodes = settings.GridMaxNodes;
        GridNodeColorAuto = settings.GridNodeColor == null;
        GridNodeColor = settings.GridNodeColor ?? "#C0C0C0";
        GridNodeSize = settings.GridNodeSize;
        AutosaveIntervalMinutes = settings.AutosaveIntervalMinutes;
        DefaultSheetFormat = settings.DefaultSheetFormat;
    }

    public event Action? ConfirmRequested;
    public event Action? CancelRequested;

    [RelayCommand]
    private void Confirm()
    {
        var settings = _settingsService.Load();
        settings.Theme = Theme;
        settings.ShowGrid = ShowGrid;
        settings.SnapToGrid = SnapToGrid;
        settings.GridStepMm = GridStepMm;
        settings.GridMaxNodes = GridMaxNodes;
        settings.GridNodeColor = GridNodeColorAuto ? null : GridNodeColor;
        settings.GridNodeSize = GridNodeSize;
        settings.AutosaveIntervalMinutes = AutosaveIntervalMinutes;
        settings.DefaultSheetFormat = DefaultSheetFormat;
        _settingsService.Save(settings);
        ConfirmRequested?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        CancelRequested?.Invoke();
    }
}
