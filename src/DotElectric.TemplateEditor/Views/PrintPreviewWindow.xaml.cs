using System.Windows;
using System.Windows.Documents;
using DotElectric.TemplateEditor.Services;

namespace DotElectric.TemplateEditor.Views;

public partial class PrintPreviewWindow : Window
{
    public PrintPreviewWindow(FixedDocument document, string title)
    {
        InitializeComponent();
        Title = $"Предпросмотр печати — {title}";
        DocumentViewer.Document = document;
    }

    private void DocumentViewer_OnLoaded(object sender, RoutedEventArgs e)
    {
        System.Windows.Controls.DocumentViewer.FitToWidthCommand.Execute(null, null);
    }
}
