using System.IO;
using Microsoft.Extensions.Logging;

namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Реализация IFileService.
/// Делегирует диалоги IDialogFileService.
/// </summary>
public sealed class FileService : IFileService
{
    private readonly string _templatesFolder;
    private readonly string _backupFolder;
    private readonly ILogger<FileService>? _logger;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IDialogFileService _dialogService;

    public FileService(ILogger<FileService>? logger = null, IDateTimeProvider? dateTimeProvider = null, IDialogFileService? dialogService = null)
    {
        _logger = logger;
        _dateTimeProvider = dateTimeProvider ?? new DateTimeProvider();
        _dialogService = dialogService ?? new WpfDialogFileService(logger);
        var appDataFolder = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DotElectric");

        _templatesFolder = System.IO.Path.Combine(appDataFolder, "Templates");
        _backupFolder = System.IO.Path.Combine(appDataFolder, "Backups");

        // Создаём папки, если не существуют
        Directory.CreateDirectory(_templatesFolder);
        Directory.CreateDirectory(_backupFolder);
    }

    /// <inheritdoc/>
    public string? OpenFileDialog(string filter) => _dialogService.OpenFileDialog(filter);

    /// <inheritdoc/>
    public string? SaveFileDialog(string filter, string defaultFileName) => _dialogService.SaveFileDialog(filter, defaultFileName);

    /// <inheritdoc/>
    public string GetTemplatesFolder()
    {
        Directory.CreateDirectory(_templatesFolder);
        return _templatesFolder;
    }

    /// <inheritdoc/>
    public string GetBackupFolder()
    {
        Directory.CreateDirectory(_backupFolder);
        return _backupFolder;
    }

    /// <inheritdoc/>
    public void CreateBackup(string sourcePath)
    {
        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new ArgumentException("Путь к файлу не может быть пустым.", nameof(sourcePath));

        if (!File.Exists(sourcePath))
            throw new FileNotFoundException($"Исходный файл не найден: {sourcePath}");

        var fileName = System.IO.Path.GetFileNameWithoutExtension(sourcePath);
        var timestamp = _dateTimeProvider.UtcNow.ToString("yyyyMMdd_HHmmss");
        var backupFileName = $"{fileName}_backup_{timestamp}.tdel";
        var backupPath = System.IO.Path.Combine(_backupFolder, backupFileName);

        File.Copy(sourcePath, backupPath, overwrite: true);
        _logger?.LogInformation("Создана резервная копия: sourcePath={SourcePath}, backupPath={BackupPath}", sourcePath, backupPath);
    }
}
