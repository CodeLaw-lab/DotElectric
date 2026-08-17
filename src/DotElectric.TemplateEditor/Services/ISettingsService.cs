using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Сервис управления настройками приложения.
/// Сохраняет и загружает настройки между запусками.
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Загрузить настройки из файла.
    /// </summary>
    AppSettings Load();

    /// <summary>
    /// Сохранить настройки в файл.
    /// </summary>
    void Save(AppSettings settings);
}
