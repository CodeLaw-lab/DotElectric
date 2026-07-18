namespace DotElectric.TemplateEditor.Models;

/// <summary>
/// Сетка по умолчанию в шаблоне.
/// Фиксированный шаг 5 мм (5000 микрон), не настраивается, не сериализуется.
/// Рабочие настройки сетки хранятся в GridSettings (EditorViewModel).
/// </summary>
public sealed class Grid
{
    /// <summary>
    /// Шаг сетки в микронах (всегда 5000 = 5 мм).
    /// </summary>
    public long StepMicrons { get; } = 5000;

    /// <summary>
    /// Экземпляр сетки по умолчанию (5 мм).
    /// </summary>
    public static Grid Default { get; } = new();

    private Grid() { }
}
