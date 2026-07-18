using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DotElectric.TemplateEditor.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotElectric.TemplateEditor.ViewModels;

/// <summary>
/// ViewModel библиотеки шаблонов (левая панель).
/// Отображает список доступных шаблонов из %APPDATA%\DotElectric\Templates\.
/// </summary>
public partial class TemplateLibraryViewModel : ObservableObject
{
    private readonly ITemplateLibraryService _templateLibraryService;
    private readonly IFileService? _fileService;
    private readonly Action<TemplateInfo>? _onTemplateDoubleClicked;
    private readonly ILogger<TemplateLibraryViewModel> _logger;

    /// <summary>
    /// Список шаблонов для отображения в ListBox.
    /// </summary>
    public ObservableCollection<TemplateInfo> Templates { get; } = new();

    /// <summary>
    /// Выбранный шаблон.
    /// </summary>
    [ObservableProperty]
    private TemplateInfo? _selectedTemplate;

    /// <summary>
    /// Статус загрузки (для отображения при пустом списке).
    /// </summary>
    [ObservableProperty]
    private string _statusMessage = "Загрузка...";

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="fileService">Сервис файловых диалогов для импорта.</param>
    /// <param name="logger">Логгер для записи ошибок.</param>
    public TemplateLibraryViewModel(
        ITemplateLibraryService templateLibraryService,
        Action<TemplateInfo>? onTemplateDoubleClicked = null,
        IFileService? fileService = null,
        ILogger<TemplateLibraryViewModel>? logger = null)
    {
        _templateLibraryService = templateLibraryService ?? throw new ArgumentNullException(nameof(templateLibraryService));
        _onTemplateDoubleClicked = onTemplateDoubleClicked;
        _fileService = fileService;
        _logger = logger ?? NullLogger<TemplateLibraryViewModel>.Instance;
        LoadTemplates();
    }

    /// <summary>
    /// Загрузить список шаблонов из папки.
    /// </summary>
    public void LoadTemplates()
    {
        try
        {
            Templates.Clear();
            var infos = _templateLibraryService.LoadTemplateInfos();
            foreach (var info in infos)
                Templates.Add(info);

            StatusMessage = Templates.Count == 0
                ? "Нет шаблонов в библиотеке"
                : $"Шаблонов: {Templates.Count}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки списка шаблонов из библиотеки");
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    /// <summary>
    /// Импортировать .tdel файл в библиотеку.
    /// </summary>
    [RelayCommand]
    private void ImportToLibrary()
    {
        var filePath = _fileService?.OpenFileDialog("Файлы шаблонов (*.tdel)|*.tdel|Все файлы (*.*)|*.*");
        if (string.IsNullOrWhiteSpace(filePath))
            return;

        try
        {
            var info = _templateLibraryService.CopyToLibrary(filePath);
            Templates.Add(info);
            StatusMessage = $"Шаблонов: {Templates.Count}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка импорта шаблона из {FilePath}", filePath);
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    /// <summary>
    /// Удалить выбранный шаблон из библиотеки.
    /// </summary>
    [RelayCommand]
    private void RemoveFromLibrary()
    {
        var template = SelectedTemplate;
        if (template == null)
            return;

        try
        {
            _templateLibraryService.RemoveFromLibrary(template);
            Templates.Remove(template);
            SelectedTemplate = null;
            StatusMessage = Templates.Count == 0
                ? "Нет шаблонов в библиотеке"
                : $"Шаблонов: {Templates.Count}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка удаления шаблона {DisplayName}", template.DisplayName);
            StatusMessage = $"Ошибка: {ex.Message}";
        }
    }

    /// <summary>
    /// Двойной клик по шаблону — открыть в новой вкладке.
    /// </summary>
     [RelayCommand]
     private void OpenTemplate(TemplateInfo? template)
     {
         if (template == null) return;
         _onTemplateDoubleClicked?.Invoke(template);
     }

 }
