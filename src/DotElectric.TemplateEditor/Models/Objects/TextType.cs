namespace DotElectric.TemplateEditor.Models.Objects;

/// <summary>
/// Тип текста в шаблоне.
/// </summary>
public enum TextType
{
    /// <summary>
    /// Произвольный текст.
    /// </summary>
    Text = 0,

    /// <summary>
    /// Размерная надпись.
    /// </summary>
    Dimension = 1,

    /// <summary>
    /// Допуск.
    /// </summary>
    Tolerance = 2,

    /// <summary>
    /// Примечание.
    /// </summary>
    Note = 3,

    /// <summary>
    /// Обозначение (позиция).
    /// </summary>
    Label = 4
}
