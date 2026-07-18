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

    /// <summary>
    /// Получить значение настройки по ключу.
    /// </summary>
    /// <typeparam name="T">Тип значения.</typeparam>
    /// <param name="key">Ключ настройки.</param>
    /// <param name="defaultValue">Значение по умолчанию.</param>
    /// <returns>Значение настройки или defaultValue, если не найдено.</returns>
    T Get<T>(string key, T defaultValue);

    /// <summary>
    /// Установить значение настройки.
    /// </summary>
    /// <typeparam name="T">Тип значения.</typeparam>
    /// <param name="key">Ключ настройки.</param>
    /// <param name="value">Новое значение.</param>
    void Set<T>(string key, T value);
}
