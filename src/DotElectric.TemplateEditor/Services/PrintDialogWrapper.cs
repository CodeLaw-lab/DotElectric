using System.Windows.Controls;

namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Реальная обёртка над System.Windows.Controls.PrintDialog.
/// Используется в production-коде.
/// </summary>
public sealed class PrintDialogWrapper : IPrintDialogWrapper
{
    private readonly PrintDialog _dialog;

    public PrintDialogWrapper()
    {
        _dialog = new PrintDialog();
    }

    public bool? ShowDialog()
    {
        return _dialog.ShowDialog();
    }

    public void PrintVisual(System.Windows.Media.Visual visual, string description)
    {
        _dialog.PrintVisual(visual, description);
    }

    public double PrintableAreaWidth => _dialog.PrintableAreaWidth;

    public double PrintableAreaHeight => _dialog.PrintableAreaHeight;

    public string? PrinterName
    {
        get => _dialog.PrintQueue?.FullName;
        set
        {
            if (value != null)
            {
                _dialog.PrintQueue = new System.Printing.PrintQueue(
                    new System.Printing.PrintServer(), value);
            }
        }
    }

    public int Copies
    {
        get => (int)(_dialog.PrintTicket.CopyCount ?? 1);
        set => _dialog.PrintTicket.CopyCount = value;
    }
}
