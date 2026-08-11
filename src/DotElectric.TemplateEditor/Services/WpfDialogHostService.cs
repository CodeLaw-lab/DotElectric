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
        var (windowType, dataContext) = ResolveWindowDescriptor(viewModel);
        var window = CreateWindow(windowType, dataContext);

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

    internal static (Type WindowType, object? DataContext) ResolveWindowDescriptor(object viewModel)
    {
        return viewModel switch
        {
            SettingsViewModel _ => (typeof(SettingsView), viewModel),
            _ => (typeof(CustomSheetDialog), viewModel)
        };
    }

    private static Window CreateWindow(Type windowType, object? dataContext)
    {
        if (windowType.GetConstructor(Type.EmptyTypes) != null)
        {
            var window = (Window)Activator.CreateInstance(windowType)!;
            window.DataContext = dataContext;
            return window;
        }

        return (Window)Activator.CreateInstance(windowType, dataContext)!;
    }
}
