namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Информация о шаблоне для библиотеки.
/// </summary>
public record TemplateInfo(string FileName, string DisplayName, string FullPath);

/// <summary>
/// Сервис управления библиотекой пользовательских шаблонов.
/// Сканирует папку %APPDATA%\DotElectric\Templates\ на наличие .tdel файлов.
/// </summary>
public interface ITemplateLibraryService
{
    /// <summary>
    /// Папка шаблонов: %APPDATA%\DotElectric\Templates\
    /// Создаётся при первом вызове, если не существует.
    /// </summary>
    string TemplatesFolder { get; }

    /// <summary>
    /// Сканирует TemplatesFolder на наличие *.tdel файлов.
    /// Возвращает отсортированный список (по DisplayName, без учёта регистра).
    /// </summary>
    /// <returns>Список информации о шаблонах.</returns>
    IReadOnlyList<TemplateInfo> LoadTemplateInfos();

    /// <summary>
    /// Копирует .tdel файл в библиотеку шаблонов.
    /// </summary>
    /// <param name="sourceFilePath">Путь к исходному .tdel файлу.</param>
    /// <param name="newName">Новое имя (без расширения). Если null, используется имя исходного файла.</param>
    /// <returns>Информация о добавленном шаблоне.</returns>
    TemplateInfo CopyToLibrary(string sourceFilePath, string? newName = null);

    /// <summary>
    /// Удаляет шаблон из библиотеки.
    /// </summary>
    void RemoveFromLibrary(TemplateInfo template);
}
