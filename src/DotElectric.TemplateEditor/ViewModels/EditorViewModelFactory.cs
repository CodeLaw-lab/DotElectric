using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotElectric.TemplateEditor.ViewModels;

public sealed class EditorViewModelFactory : IEditorViewModelFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IGridNodeGenerator? _gridNodeGenerator;
    private readonly IThemeService? _themeService;
    private readonly ISettingsService? _settingsService;

    public EditorViewModelFactory(IServiceProvider serviceProvider, IGridNodeGenerator? gridNodeGenerator = null, IThemeService? themeService = null, ISettingsService? settingsService = null)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _gridNodeGenerator = gridNodeGenerator;
        _themeService = themeService;
        _settingsService = settingsService;
    }

    public EditorViewModel Create(
        Template template,
        GridSettings? gridSettings = null,
        IPrintService? printService = null,
        IGridNodeGenerator? gridNodeGenerator = null,
        IThemeService? themeService = null)
    {
        if (template == null) throw new ArgumentNullException(nameof(template));

        var resolvedPrintService = printService ?? _serviceProvider.GetService<IPrintService>();
        var resolvedGridNodeGenerator = gridNodeGenerator ?? _gridNodeGenerator
            ?? _serviceProvider.GetService<IGridNodeGenerator>();
        var resolvedThemeService = themeService ?? _themeService
            ?? _serviceProvider.GetService<IThemeService>();

        return new EditorViewModel(
            template,
            ResolveGridSettings(gridSettings),
            resolvedPrintService,
            resolvedGridNodeGenerator,
            resolvedThemeService);
    }

    public EditorViewModel CreateWithFilePath(
        Template template,
        string filePath,
        GridSettings? gridSettings = null,
        IPrintService? printService = null,
        IGridNodeGenerator? gridNodeGenerator = null,
        IThemeService? themeService = null)
    {
        if (template == null) throw new ArgumentNullException(nameof(template));
        if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        var resolvedPrintService = printService ?? _serviceProvider.GetService<IPrintService>();
        var resolvedGridNodeGenerator = gridNodeGenerator ?? _gridNodeGenerator
            ?? _serviceProvider.GetService<IGridNodeGenerator>();
        var resolvedThemeService = themeService ?? _themeService
            ?? _serviceProvider.GetService<IThemeService>();

        return new EditorViewModel(
            template,
            filePath,
            ResolveGridSettings(gridSettings),
            resolvedPrintService,
            resolvedGridNodeGenerator,
            resolvedThemeService);
    }

    private GridSettings ResolveGridSettings(GridSettings? gridSettings)
    {
        if (gridSettings != null) return gridSettings;
        if (_settingsService != null)
        {
            var app = _settingsService.Load();
            if (app != null)
                return GridSettings.FromAppSettings(app);
        }
        return GridSettings.FromDefaultGrid();
    }
}
