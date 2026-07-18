using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Сервис управления шаблонами: создание, загрузка, сохранение, валидация.
/// </summary>
public interface ITemplateService
{
   /// <summary>
   /// Создать новый пустой шаблон заданного формата.
   /// </summary>
   /// <param name="format">Формат листа (A0-A4).</param>
   /// <param name="orientation">Ориентация листа. Если null, используется ориентация по умолчанию для формата.</param>
   /// <returns>Новый шаблон.</returns>
   Template CreateNew(string format = "A3", SheetOrientation? orientation = null);

    /// <summary>
    /// Создать новый шаблон из готового объекта Sheet (для пользовательских форматов).
    /// </summary>
    /// <param name="sheet">Объект листа с размерами.</param>
    /// <returns>Новый шаблон.</returns>
    Template CreateFromSheet(Sheet sheet);

    /// <summary>
    /// Загрузить шаблон из .tdel файла.
    /// </summary>
    /// <param name="filePath">Путь к файлу .tdel.</param>
    /// <returns>Загруженный шаблон.</returns>
    Template Load(string filePath);

    /// <summary>
    /// Сохранить шаблон в .tdel файл (XML в ZIP).
    /// </summary>
    /// <param name="template">Шаблон для сохранения.</param>
    /// <param name="filePath">Путь к файлу .tdel.</param>
    void Save(Template template, string filePath);

    /// <summary>
    /// Валидировать шаблон.
    /// </summary>
    /// <param name="template">Шаблон для проверки.</param>
    /// <returns>Коллекция ошибок валидации. Пустая = всё в порядке.</returns>
    IEnumerable<string> Validate(Template template);
}
