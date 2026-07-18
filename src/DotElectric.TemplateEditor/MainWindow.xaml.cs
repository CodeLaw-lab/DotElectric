using System.Windows;
using System.Windows.Input;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor;

/// <summary>
/// Главное окно приложения DotElectric Template Editor.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Handled) return;
        if (DataContext is not MainViewModel vm) return;
        if (vm.SelectedTab is not EditorViewModel editor) return;

        if (ShortcutRegistry.TryHandle(e.Key, e.KeyboardDevice.Modifiers, editor))
            e.Handled = true;
    }
}
