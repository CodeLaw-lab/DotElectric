using System.Windows.Controls;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Views;

/// <summary>
/// Логика взаимодействия для PropertiesPanelContent.xaml
/// </summary>
public partial class PropertiesPanelContent
{
    public PropertiesPanelContent()
    {
        InitializeComponent();
    }

    private void OnTextIsEditableClick(object sender, System.Windows.RoutedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.DataContext is TextPropertiesViewModel textVm)
        {
            textVm.ChangeIsEditableCommand.Execute(checkBox.IsChecked == true);
        }
    }
}
