using System.Windows;
using DotElectric.TemplateEditor.ViewModels;
using DotElectric.TemplateEditor.Views;

namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Реализация IDialogHostService для WPF.
/// Открывает диалоги с заданной ViewModel.
/// </summary>
public sealed class WpfDialogHostService : IDialogHostService
{
    public bool? ShowDialog(object viewModel, object? owner = null)
    {
        Window window = viewModel switch
        {
            SettingsViewModel _ => new SettingsView((SettingsViewModel)viewModel),
            _ => new CustomSheetDialog { DataContext = viewModel }
        };

        if (owner is Window ownerWindow)
            window.Owner = ownerWindow;

        if (viewModel is ICustomSheetDialogVm dialogVm)
        {
            void OnConfirm() { window.DialogResult = true; window.Close(); }
            void OnCancel() { window.DialogResult = false; window.Close(); }

            dialogVm.ConfirmRequested += OnConfirm;
            dialogVm.CancelRequested += OnCancel;

            try
            {
                return window.ShowDialog();
            }
            finally
            {
                dialogVm.ConfirmRequested -= OnConfirm;
                dialogVm.CancelRequested -= OnCancel;
            }
        }

        // SettingsViewModel handles confirm/cancel internally
        return window.ShowDialog();
    }
}
