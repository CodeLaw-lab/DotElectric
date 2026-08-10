using DotElectric.TemplateEditor.Constants;

namespace DotElectric.TemplateEditor.Models;

/// <summary>
/// Рабочие настройки сетки редактора.
/// Хранятся в EditorViewModel, НЕ сериализуются в .tdel.
/// Пользователь может менять шаг, привязку и видимость.
/// </summary>
public class GridSettings
{
    /// <summary>
    /// Сетка включена (отображается на холсте).
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Привязка к сетке включена.
    /// </summary>
    public bool SnapEnabled { get; set; } = true;

    /// <summary>
    /// Шаг сетки в микронах (500, 1000, 5000, 10000).
    /// По умолчанию — 5 мм (совпадает с DefaultGrid шаблона).
    /// </summary>
    public long StepMicrons { get; set; } = EditorSettings.DefaultGridStepMicrons;

    /// <summary>
    /// Отображение сетки на холсте.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Максимальное количество узлов сетки (бюджет). Настраивается пользователем.
    /// </summary>
    public int MaxGridNodes { get; set; } = EditorSettings.MaxGridNodes;

    /// <summary>
    /// Цвет узлов сетки (HEX). null = авто (следовать теме).
    /// </summary>
    public string? NodeColor { get; set; } = null;

    /// <summary>
    /// Размер узла сетки в пикселях (диаметр).
    /// </summary>
    public double NodeSize { get; set; } = 2.0;

    /// <summary>
    /// Создать настройки с параметрами по умолчанию.
    /// </summary>
    public GridSettings() { }

    /// <summary>
    /// Создать настройки из DefaultGrid шаблона.
    /// </summary>
    public static GridSettings FromDefaultGrid()
        => new()
        {
            Enabled = true,
            SnapEnabled = true,
            StepMicrons = Grid.Default.StepMicrons,
            Visible = true,
            MaxGridNodes = EditorSettings.MaxGridNodes,
            NodeColor = null,
            NodeSize = 2.0,
        };
}