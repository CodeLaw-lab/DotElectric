using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.ViewModels;
using DotElectric.TemplateEditor.ViewModels.Abstractions;
using Microsoft.Extensions.Logging;

namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Facade for tab operations. Encapsulates NewTab, OpenFile, Save, SaveAs, PreviewPrint.
/// </summary>
public sealed class TabOperationsService : ITabOperationsService
{
    private readonly ITemplateService _templateService;
    private readonly IFileService _fileService;
    private readonly IDialogService _dialogService;
    private readonly ISettingsService _settingsService;
    private readonly IPrintService _printService;
    private readonly IEditorViewModelFactory _editorViewModelFactory;
    private readonly ILogger<TabOperationsService> _logger;

    public TabOperationsService(
        ITemplateService templateService,
        IFileService fileService,
        IDialogService dialogService,
        ISettingsService settingsService,
        IPrintService printService,
        IEditorViewModelFactory editorViewModelFactory,
        ILogger<TabOperationsService> logger)
    {
        _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _printService = printService ?? throw new ArgumentNullException(nameof(printService));
        _editorViewModelFactory = editorViewModelFactory ?? throw new ArgumentNullException(nameof(editorViewModelFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public EditorViewModel CreateNewTab(string? format, string? lastUsedFormat, string? lastUsedOrientation)
    {
        var rawFormat = format ?? lastUsedFormat ?? "A3";

        // Parse format and orientation from string like "A4P", "A3L", "A4", "A3"
        var fmt = ParseSheetFormat(rawFormat, out var orientation);

        // If orientation not specified — use last saved or default for format
        if (orientation == null)
        {
            var orientStr = lastUsedOrientation ?? "Landscape";
            orientation = Enum.TryParse<SheetOrientation>(orientStr, true, out var parsed)
                ? parsed
                : Sheet.GetDefaultOrientation(fmt);
        }

        _settingsService.Set("LastUsedSheetFormat", fmt);
        _settingsService.Set("LastUsedSheetOrientation", orientation.Value.ToString());

        var template = _templateService.CreateNew(fmt, orientation.Value);
        return _editorViewModelFactory.Create(template, printService: _printService);
    }

    public async Task<EditorViewModel?> OpenFileAsync()
    {
        var filePath = _fileService.OpenFileDialog("DotElectric Template|*.tdel");
        if (string.IsNullOrEmpty(filePath)) return null;

        try
        {
            var template = _templateService.Load(filePath);
            return _editorViewModelFactory.CreateWithFilePath(template, filePath, printService: _printService);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open file: {FilePath}", filePath);
            _dialogService.ShowError($"Failed to open file: {ex.Message}");
            return null;
        }
    }

    public EditorViewModel? OpenFromFilePath(string filePath)
    {
        try
        {
            var template = _templateService.Load(filePath);
            if (template != null)
            {
                return _editorViewModelFactory.CreateWithFilePath(template, filePath, printService: _printService);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading template from library: {FilePath}", filePath);
            _dialogService.ShowError($"Error loading template: {ex.Message}");
        }

        return null;
    }

    public async Task SaveTabAsync(EditorViewModel tab)
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

            // Validate before save
            var errors = _templateService.Validate(tab.Template).ToList();
            if (errors.Count > 0)
            {
                var errorText = string.Join("\n", errors);
                var result = await _dialogService.ShowUnsavedChangesDialogAsync(
                    $"Template has errors:\n{errorText}\n\nSave anyway?");
                if (result != UnsavedChangesResult.Save)
                    return;
            }

            // Backup before saving (if file already exists)
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
            _logger.LogError(ex, "Failed to save file: {FilePath}", path ?? tab.DirtyStateManager.FilePath);
            _dialogService.ShowError($"Failed to save file: {ex.Message}");
        }
    }

    public async Task SaveAsAsync(EditorViewModel tab)
    {
        if (tab == null) return;

        var path = _fileService.SaveFileDialog("DotElectric Template|*.tdel", tab.DirtyStateManager.DisplayName);
        if (string.IsNullOrEmpty(path)) return;

        try
        {
            _templateService.Save(tab.Template, path);
            tab.DirtyStateManager.FilePath = path;
            tab.ClearDirty();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file (Save As): {FilePath}", path);
            _dialogService.ShowError($"Failed to save file: {ex.Message}");
        }
    }

    public EditorViewModel CreateNewCustomTab(double widthMm, double heightMm)
    {
        var sheet = Sheet.Custom(widthMm, heightMm);
        var template = _templateService.CreateFromSheet(sheet);
        return _editorViewModelFactory.Create(template, printService: _printService);
    }

    public async Task<bool> PromptAndSaveIfDirtyAsync(EditorViewModel tab)
    {
        if (tab == null) return true;

        if (tab.DirtyStateManager.IsDirty)
        {
            var result = await _dialogService.ShowUnsavedChangesDialogAsync(tab.DirtyStateManager.DisplayName);
            if (result == UnsavedChangesResult.Cancel) return false;
            if (result == UnsavedChangesResult.Save)
                await SaveTabAsync(tab);
        }

        return true;
    }

    internal static string ParseSheetFormat(string rawFormat, out SheetOrientation? orientation)
    {
        orientation = null;

        if (string.IsNullOrEmpty(rawFormat) || rawFormat.Length < 2)
            return rawFormat;

        // Last character is the orientation suffix
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

        // No suffix — return as-is
        return rawFormat;
    }
}
