namespace DotElectric.TemplateEditor.Models;

/// <summary>
/// Метаданные шаблона (автор, даты, описание).
/// </summary>
public class Metadata
{
    /// <summary>
    /// Наименование шаблона.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Описание шаблона.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Автор шаблона.
    /// </summary>
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// Дата создания.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Дата последнего изменения.
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    public Metadata()
    {
    }
}