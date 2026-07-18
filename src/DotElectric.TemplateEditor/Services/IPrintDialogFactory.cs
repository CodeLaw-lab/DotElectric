namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Фабрика для создания обёрток над PrintDialog.
/// Позволяет мокировать диалог печати в юнит-тестах.
/// </summary>
public interface IPrintDialogFactory
{
    /// <summary>
    /// Создать обёртку над PrintDialog.
    /// </summary>
    IPrintDialogWrapper Create();
}

/// <summary>
/// Обёртка над System.Windows.Controls.PrintDialog для тестируемости.
/// </summary>
public interface IPrintDialogWrapper
{
    /// <summary>
    /// Показать стандартный диалог печати Windows.
    /// </summary>
    bool? ShowDialog();

    /// <summary>
    /// Напечатать визуальный объект.
    /// </summary>
    void PrintVisual(System.Windows.Media.Visual visual, string description);

    /// <summary>
    /// Ширина печатной области в единицах WPF (1/96 дюйма).
    /// </summary>
    double PrintableAreaWidth { get; }

    /// <summary>
    /// Высота печатной области в единицах WPF (1/96 дюйма).
    /// </summary>
    double PrintableAreaHeight { get; }

    /// <summary>
    /// Имя принтера (null = принтер по умолчанию).
    /// </summary>
    string? PrinterName { get; set; }

    /// <summary>
    /// Количество копий.
    /// </summary>
    int Copies { get; set; }
}
