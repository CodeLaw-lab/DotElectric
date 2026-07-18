using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Настройки печати.
/// </summary>
public class PrintSettings
{
    /// <summary>
    /// Количество копий (по умолчанию 1).
    /// </summary>
    public int Copies { get; set; } = 1;

    /// <summary>
    /// Масштабирование: "FitToPage", "ActualSize", "Custom".
    /// </summary>
    public string Scaling { get; set; } = "FitToPage";

    /// <summary>
    /// Пользовательский масштаб (при Scaling = "Custom"), проценты.
    /// </summary>
    public double CustomScalePercent { get; set; } = 100.0;

    /// <summary>
    /// Печатать в цвете (по умолчанию true).
    /// </summary>
    public bool Color { get; set; } = true;

    /// <summary>
    /// Имя принтера (null = принтер по умолчанию).
    /// </summary>
    public string? PrinterName { get; set; }
}

/// <summary>
/// Сервис печати шаблонов.
/// </summary>
public interface IPrintService
{
    /// <summary>
    /// Печать Visual (Canvas/DrawingVisual) через PrintDialog.
    /// Автоматически масштабирует под страницу при Scaling = FitToPage.
    /// </summary>
    /// <param name="visual">Визуальный объект для печати.</param>
    /// <param name="description">Описание задания печати.</param>
    /// <param name="settings">Настройки печати.</param>
    /// <returns>true при успешной печати.</returns>
    bool PrintWithVisual(System.Windows.Media.Visual visual, string description, PrintSettings settings);

    /// <summary>
    /// Показать стандартный диалог печати Windows.
    /// </summary>
    /// <returns>true = пользователь нажал "Печать", false = отмена.</returns>
    bool ShowPrintDialog();
}
