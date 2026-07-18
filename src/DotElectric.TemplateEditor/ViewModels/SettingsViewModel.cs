using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    private int _autosaveIntervalMinutes;

    [ObservableProperty]
    private string _defaultSheetFormat;

    [ObservableProperty]
    private double _defaultZoom;

    public string Title => "Настройки";

    public string[] ThemeOptions { get; } = ["Light", "Dark"];

    public string[] FormatOptions { get; } =
    [
        "A0", "A1", "A2", "A3", "A4",
        "A4×2", "A3×2", "A2×2", "A1×2", "A0×2"
    ];

    public double[] ZoomOptions { get; } = [0.25, 0.5, 0.75, 1.0, 1.5, 2.0];

    public SettingsViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

        Theme = _settingsService.Get("Theme", "Light");
        ShowGrid = _settingsService.Get("ShowGrid", true);
        SnapToGrid = _settingsService.Get("SnapToGrid", true);
        GridStepMm = _settingsService.Get("GridStepMm", 5.0);
        AutosaveIntervalMinutes = _settingsService.Get("AutosaveIntervalMinutes", 5);
        DefaultSheetFormat = _settingsService.Get("DefaultSheetFormat", "A3");
        DefaultZoom = _settingsService.Get("DefaultZoom", 1.0);
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
        settings.AutosaveIntervalMinutes = AutosaveIntervalMinutes;
        settings.DefaultSheetFormat = DefaultSheetFormat;
        settings.DefaultZoom = DefaultZoom;
        _settingsService.Save(settings);
        ConfirmRequested?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        CancelRequested?.Invoke();
    }
}
