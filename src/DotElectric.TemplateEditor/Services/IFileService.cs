namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Сервис работы с файловой системой: диалоги открытия/сохранения, папки шаблонов и резервных копий.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Показать диалог открытия файла.
    /// </summary>
    /// <param name="filter">Фильтр файлов (например: "DotElectric Template|*.tdel").</param>
    /// <returns>Путь к выбранному файлу или пустая строка при отмене.</returns>
    string? OpenFileDialog(string filter);

    /// <summary>
    /// Показать диалог сохранения файла.
    /// </summary>
    /// <param name="filter">Фильтр файлов.</param>
    /// <param name="defaultFileName">Имя файла по умолчанию.</param>
    /// <returns>Путь к файлу или пустая строка при отмене.</returns>
    string? SaveFileDialog(string filter, string defaultFileName);

    /// <summary>
    /// Папка пользовательских шаблонов: %APPDATA%\DotElectric\Templates
    /// </summary>
    string GetTemplatesFolder();

    /// <summary>
    /// Папка резервных копий: %APPDATA%\DotElectric\Backups
    /// </summary>
    string GetBackupFolder();

    /// <summary>
    /// Создать резервную копию файла.
    /// </summary>
    /// <param name="sourcePath">Путь к исходному файлу.</param>
    void CreateBackup(string sourcePath);
}
