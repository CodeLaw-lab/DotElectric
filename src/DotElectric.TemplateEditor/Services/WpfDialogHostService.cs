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
    private readonly Func<Window?> _mainWindowProvider;

    public WpfDialogHostService(Func<Window?>? mainWindowProvider = null)
    {
        // Application.Current.MainWindow читается только с потока диспетчера
        // приложения; с чужих потоков владелец не разрешается.
        _mainWindowProvider = mainWindowProvider ?? (() =>
        {
            var application = Application.Current;
            return application != null && application.Dispatcher.CheckAccess()
                ? application.MainWindow
                : null;
        });
    }

    public bool? ShowDialog(object viewModel, object? owner = null)
    {
        var (windowType, dataContext) = ResolveWindowDescriptor(viewModel);
        var window = CreateWindow(windowType, dataContext);

        // Владелец по умолчанию — главное окно приложения: объявленное в XAML
        // центрирование на владельце работает у всех диалогов.
        var resolvedOwner = owner as Window ?? _mainWindowProvider();
        if (resolvedOwner != null)
            window.Owner = resolvedOwner;

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
            PrintPreviewViewModel _ => (typeof(PrintPreviewWindow), viewModel),
            _ => (typeof(CustomSheetDialog), viewModel)
        };
    }

    internal static Window CreateWindow(Type windowType, object? dataContext)
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
