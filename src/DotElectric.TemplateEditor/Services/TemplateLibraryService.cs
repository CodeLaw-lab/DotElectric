using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Реализация ITemplateLibraryService.
/// Сканирует папку шаблонов и возвращает отсортированный список .tdel файлов.
/// </summary>
public sealed class TemplateLibraryService : ITemplateLibraryService
{
    private readonly string _templatesFolder;
    private readonly ILogger<TemplateLibraryService> _logger;

    public string TemplatesFolder => _templatesFolder;

    public TemplateLibraryService(IFileService fileService, ILogger<TemplateLibraryService>? logger = null)
    {
        _templatesFolder = fileService.GetTemplatesFolder();
        _logger = logger ?? NullLogger<TemplateLibraryService>.Instance;
    }

    /// <inheritdoc/>
    public IReadOnlyList<TemplateInfo> LoadTemplateInfos()
    {
        if (!Directory.Exists(_templatesFolder))
            return Array.Empty<TemplateInfo>();

        var tdelFiles = Directory.GetFiles(_templatesFolder, "*.tdel", SearchOption.TopDirectoryOnly);
        var templateInfos = new List<TemplateInfo>();

        foreach (var filePath in tdelFiles)
        {
            var fileName = Path.GetFileName(filePath);
            var displayName = Path.GetFileNameWithoutExtension(filePath);

            templateInfos.Add(new TemplateInfo(fileName, displayName, filePath));
        }

        // Сортировка по DisplayName (без учёта регистра)
        templateInfos.Sort((a, b) =>
            string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));

        return templateInfos.AsReadOnly();
    }

    /// <inheritdoc/>
    public TemplateInfo CopyToLibrary(string sourceFilePath, string? newName = null)
    {
        if (string.IsNullOrWhiteSpace(sourceFilePath))
            throw new ArgumentException("Путь к файлу не может быть пустым.", nameof(sourceFilePath));

        if (!File.Exists(sourceFilePath))
            throw new FileNotFoundException("Исходный файл не найден.", sourceFilePath);

        var extension = Path.GetExtension(sourceFilePath);
        if (!string.Equals(extension, ".tdel", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Файл должен иметь расширение .tdel.", nameof(sourceFilePath));

        Directory.CreateDirectory(_templatesFolder);

        var destFileName = (newName ?? Path.GetFileNameWithoutExtension(sourceFilePath)) + ".tdel";
        var destPath = Path.Combine(_templatesFolder, destFileName);

        // Если файл уже существует — генерируем уникальное имя
        var counter = 1;
        while (File.Exists(destPath))
        {
            destFileName = $"{newName ?? Path.GetFileNameWithoutExtension(sourceFilePath)}_{counter}.tdel";
            destPath = Path.Combine(_templatesFolder, destFileName);
            counter++;
        }

        File.Copy(sourceFilePath, destPath, overwrite: false);

        var fileName = Path.GetFileName(destPath);
        var displayName = Path.GetFileNameWithoutExtension(destPath);
        var info = new TemplateInfo(fileName, displayName, destPath);

        _logger.LogInformation("Шаблон добавлен в библиотеку: {DestPath}", destPath);
        return info;
    }

    /// <inheritdoc/>
    public void RemoveFromLibrary(TemplateInfo template)
    {
        ArgumentNullException.ThrowIfNull(template);

        if (string.IsNullOrWhiteSpace(template.FullPath))
            throw new ArgumentException("FullPath шаблона не может быть пустым.", nameof(template));

        if (!File.Exists(template.FullPath))
            throw new FileNotFoundException("Файл шаблона не найден.", template.FullPath);

        if (!template.FullPath.StartsWith(_templatesFolder, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Нельзя удалить файл вне папки библиотеки.");

        File.Delete(template.FullPath);
        _logger.LogInformation("Шаблон удалён из библиотеки: {FullPath}", template.FullPath);
    }
}
