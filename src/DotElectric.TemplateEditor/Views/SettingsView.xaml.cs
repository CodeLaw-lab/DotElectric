using System.Windows;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Views;

public partial class SettingsView : Window
{
    public SettingsView(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.ConfirmRequested += () =>
        {
            DialogResult = true;
            Close();
        };

        viewModel.CancelRequested += () =>
        {
            DialogResult = false;
            Close();
        };
    }
}
