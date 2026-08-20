using DotElectric.TemplateEditor.Constants;

namespace DotElectric.TemplateEditor.Models;

/// <summary>
/// Рабочие настройки сетки редактора.
/// Хранятся в EditorViewModel, НЕ сериализуются в .tdel.
/// Пользователь может менять шаг, привязку и видимость.
/// </summary>
public sealed class GridSettings
{
    private const long MinStepMicrons = 1;
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
    /// По умолчанию — 5 мм.
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
    public double NodeSize { get; set; } = EditorSettings.DefaultGridNodeSize;

    /// <summary>
    /// Создать настройки с параметрами по умолчанию.
    /// </summary>
    public GridSettings() { }

    /// <summary>
    /// Создать настройки по умолчанию (шаг 5 мм).
    /// </summary>
    public static GridSettings FromDefaultGrid()
        => new()
        {
            Enabled = true,
            SnapEnabled = true,
            StepMicrons = EditorSettings.DefaultGridStepMicrons,
            Visible = true,
            MaxGridNodes = EditorSettings.MaxGridNodes,
            NodeColor = null,
            NodeSize = EditorSettings.DefaultGridNodeSize,
        };

    /// <summary>
    /// Создать настройки из AppSettings (цепочка настроек приложения → настройки сетки).
    /// </summary>
    public static GridSettings FromAppSettings(AppSettings settings)
    {
        if (settings == null) throw new ArgumentNullException(nameof(settings));

        var stepMicrons = (long)(settings.GridStepMm * Coordinate.MicronsPerMm);
        if (stepMicrons < MinStepMicrons) // < 1 мкм
            stepMicrons = EditorSettings.DefaultGridStepMicrons; // запасной шаг 5 мм

        var maxNodes = settings.GridMaxNodes < 1 ? EditorSettings.MaxGridNodes : settings.GridMaxNodes;

        double nodeSize = settings.GridNodeSize;
        if (double.IsNaN(nodeSize) || double.IsInfinity(nodeSize) || nodeSize <= 0)
            nodeSize = EditorSettings.DefaultGridNodeSize;

        return new GridSettings
        {
            Enabled = settings.ShowGrid,
            SnapEnabled = settings.SnapToGrid,
            StepMicrons = stepMicrons,
            Visible = true,
            MaxGridNodes = maxNodes,
            NodeColor = settings.GridNodeColor,
            NodeSize = nodeSize,
        };
    }
}