namespace DotElectric.TemplateEditor.Models;

/// <summary>
/// Настройки приложения (текущая сессция).
/// Сериализуется в JSON для сохранения между запусками.
/// </summary>
public sealed class AppSettings
{
    /// <summary>
    /// Интервал автосохранения в минутах (по умолчанию 5).
    /// </summary>
    public int AutosaveIntervalMinutes { get; set; } = 5;

    /// <summary>
    /// Тема оформления: "Light" или "Dark" (по умолчанию "Light").
    /// </summary>
    public string Theme { get; set; } = "Light";

    /// <summary>
    /// Показывать сетку по умолчанию.
    /// </summary>
    public bool ShowGrid { get; set; } = true;

    /// <summary>
    /// Привязка к сетке по умолчанию.
    /// </summary>
    public bool SnapToGrid { get; set; } = true;

    /// <summary>
    /// Шаг сетки в мм (по умолчанию 5).
    /// </summary>
    public double GridStepMm { get; set; } = 5.0;

    /// <summary>
    /// Максимальное количество узлов сетки (бюджет).
    /// </summary>
    public int GridMaxNodes { get; set; } = 250000;

    /// <summary>
    /// Цвет узлов сетки (HEX). null = авто (по теме).
    /// </summary>
    public string? GridNodeColor { get; set; } = null;

    /// <summary>
    /// Размер узлов сетки в пикселях.
    /// </summary>
    public double GridNodeSize { get; set; } = 2.0;

    /// <summary>
    /// Формат листа по умолчанию (A3).
    /// </summary>
    public string DefaultSheetFormat { get; set; } = SheetFormatCatalog.DefaultName;

    /// <summary>
    /// Последний использованный формат листа (для Ctrl+N и кнопки «Новый»).
    /// </summary>
    public string LastUsedSheetFormat { get; set; } = SheetFormatCatalog.DefaultName;

    /// <summary>
    /// Последняя используемая орентация листа.
    /// </summary>
    public string LastUsedSheetOrientation { get; set; } = "Landscape";
}
