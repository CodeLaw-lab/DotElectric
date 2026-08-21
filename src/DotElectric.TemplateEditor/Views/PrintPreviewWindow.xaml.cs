using System.Windows;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Views;

public partial class PrintPreviewWindow : Window
{
    public PrintPreviewWindow(PrintPreviewViewModel viewModel)
    {
        InitializeComponent();
        Title = $"Предпросмотр печати — {viewModel.DisplayName}";
        DocumentViewer.Document = viewModel.Document;
    }

    private void DocumentViewer_OnLoaded(object sender, RoutedEventArgs e)
    {
        System.Windows.Controls.DocumentViewer.FitToWidthCommand.Execute(null, null);
    }
}
