using System.Collections.ObjectModel;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Models;

/// <summary>
/// Корневая модель шаблона листов.
/// Содержит метаданные, параметры листа, сетку по умолчанию и коллекцию объектов.
/// Модель создаётся один раз и не заменяется после создания (иммутабельна после инициализации).
/// </summary>
public class Template
{
    /// <summary>
    /// Версия формата файла.
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Метаданные шаблона.
    /// </summary>
    public Metadata Metadata { get; set; }

    /// <summary>
    /// Параметры листа.
    /// </summary>
    public Sheet Sheet { get; set; }

    /// <summary>
    /// Сетка по умолчанию (фиксированная, 5 мм, readonly).
    /// НЕ настраивается и НЕ сериализуется — это константа шаблона.
    /// </summary>
    public Grid DefaultGrid { get; } = Grid.Default;

    /// <summary>
    /// Коллекция объектов шаблона (линии, прямоугольники, текст).
    /// </summary>
    public ObservableCollection<TemplateObjectBase> Objects { get; }

    /// <summary>
    /// Создать пустой шаблон.
    /// </summary>
    public Template()
    {
        Metadata = new Metadata();
        Sheet = Sheet.FromFormat("A3");
        Objects = new ObservableCollection<TemplateObjectBase>();
    }

    /// <summary>
    /// Создать шаблон с заданными параметрами.
    /// </summary>
    /// <param name="metadata">Метаданные.</param>
    /// <param name="sheet">Параметры листа.</param>
    public Template(Metadata metadata, Sheet sheet)
    {
        Metadata = metadata;
        Sheet = sheet;
        Objects = new ObservableCollection<TemplateObjectBase>();
    }
}
