using System.Windows;
using System.Windows.Media;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Реализация IPrintService.
/// Использует IPrintDialogFactory для создания диалогов печати,
/// что позволяет мокировать PrintDialog в юнит-тестах.
/// Поддерживает печать Visual (Canvas) с масштабированием FitToPage.
/// </summary>
public sealed class PrintService : IPrintService
{
    private readonly IPrintDialogFactory _dialogFactory;

    public PrintService() : this(new PrintDialogFactory()) { }

    /// <summary>
    /// Создает PrintService с указанной фабрикой диалогов.
    /// Используется для внедрения моков в тестах.
    /// </summary>
    public PrintService(IPrintDialogFactory dialogFactory)
    {
        _dialogFactory = dialogFactory ?? throw new ArgumentNullException(nameof(dialogFactory));
    }

    /// <summary>
    /// Печать Visual (Canvas/DrawingVisual) через PrintDialog.
    /// Автоматически масштабирует под страницу при Scaling = FitToPage.
    /// </summary>
    /// <param name="visual">Визуальный объект для печати.</param>
    /// <param name="description">Описание задания печати.</param>
    /// <param name="settings">Настройки печати.</param>
    /// <returns>true при успешной печати.</returns>
    public bool PrintWithVisual(Visual visual, string description, PrintSettings settings)
    {
        if (visual == null)
            throw new ArgumentNullException(nameof(visual));
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        var dialog = _dialogFactory.Create();

        if (settings.PrinterName != null)
        {
            dialog.PrinterName = settings.PrinterName;
        }

        if (settings.Copies > 1)
        {
            dialog.Copies = settings.Copies;
        }

        // Масштабирование FitToPage
        Transform? originalTransform = null;
        if (settings.Scaling == "FitToPage" && visual is FrameworkElement fe)
        {
            var pageSize = new Size(
                dialog.PrintableAreaWidth,
                dialog.PrintableAreaHeight);

            var elementSize = new Size(fe.ActualWidth, fe.ActualHeight);
            if (elementSize.Width > 0 && elementSize.Height > 0)
            {
                var scale = Math.Min(
                    pageSize.Width / elementSize.Width,
                    pageSize.Height / elementSize.Height);

                originalTransform = fe.RenderTransform;
                fe.RenderTransform = new ScaleTransform(scale, scale);
            }
        }

        try
        {
            var result = dialog.ShowDialog();
            if (result != true) return false;

            dialog.PrintVisual(visual, description);
            return true;
        }
        finally
        {
            // Восстанавливаем оригинальный трансформ
            if (originalTransform != null && visual is FrameworkElement fe2)
            {
                fe2.RenderTransform = originalTransform;
            }
        }
    }

    public bool ShowPrintDialog()
    {
        var dialog = _dialogFactory.Create();
        return dialog.ShowDialog() == true;
    }
}
