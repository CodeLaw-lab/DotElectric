using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Models;

/// <summary>
/// Корневая модель шаблона листов.
/// Содержит метаданные, параметры листа, сетку по умолчанию и коллекцию объектов.
/// Модель создаётся один раз и не заменяется после создания (иммутабельна после инициализации).
/// INPC реализован для <see cref="Sheet"/>, чтобы подписчики (GridManager) могли
/// реагировать на смену формата листа.
/// </summary>
public class Template : ObservableObject
{
    private Sheet _sheet;

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
    public Sheet Sheet
    {
        get => _sheet;
        set => SetProperty(ref _sheet, value);
    }

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
        _sheet = Sheet.FromFormat(SheetFormatCatalog.DefaultName);
        Objects = new ObservableCollection<TemplateObjectBase>();
    }

    /// <summary>
    /// Создать глубокую копию шаблона.
    /// </summary>
    public Template Clone()
    {
        var metadata = new Metadata
        {
            Name = Metadata.Name,
            Description = Metadata.Description,
            Author = Metadata.Author,
            CreatedDate = Metadata.CreatedDate,
            ModifiedDate = Metadata.ModifiedDate
        };

        var sheet = new Sheet
        {
            Format = Sheet.Format,
            WidthMicrons = Sheet.WidthMicrons,
            HeightMicrons = Sheet.HeightMicrons,
            Orientation = Sheet.Orientation
        };

        var clone = new Template(metadata, sheet)
        {
            Version = Version
        };

        foreach (var obj in Objects)
        {
            clone.Objects.Add(obj.Clone());
        }

        return clone;
    }

    /// <summary>
    /// Создать шаблон с заданными параметрами.
    /// </summary>
    /// <param name="metadata">Метаданные.</param>
    /// <param name="sheet">Параметры листа.</param>
    public Template(Metadata metadata, Sheet sheet)
    {
        Metadata = metadata;
        _sheet = sheet;
        Objects = new ObservableCollection<TemplateObjectBase>();
    }
}
