using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tests.Helpers;
using DotElectric.TemplateEditor.ViewModels;
using DotElectric.TemplateEditor.Views;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Services;

/// <summary>
/// Тесты WpfDialogHostService: MTA — ResolveWindowDescriptor,
/// STA — ShowDialog (обе ветки) и CreateWindow (оба конструктора).
/// </summary>
public class WpfDialogHostServiceTests
{
    [Fact]
    public void ResolveWindowDescriptor_SettingsViewModel_ReturnsSettingsView()
    {
        var settingsServiceMock = new Mock<ISettingsService>();
        settingsServiceMock.Setup(s => s.Load()).Returns(new AppSettings());
        var viewModel = new SettingsViewModel(settingsServiceMock.Object);

        var (windowType, dataContext) = WpfDialogHostService.ResolveWindowDescriptor(viewModel);

        Assert.Equal(typeof(SettingsView), windowType);
        Assert.Same(viewModel, dataContext);
    }

    [Fact]
    public void ResolveWindowDescriptor_UnknownViewModel_ReturnsCustomSheetDialog()
    {
        var viewModel = new object();

        var (windowType, dataContext) = WpfDialogHostService.ResolveWindowDescriptor(viewModel);

        Assert.Equal(typeof(CustomSheetDialog), windowType);
        Assert.Same(viewModel, dataContext);
    }

    [Fact]
    public void ResolveWindowDescriptor_PrintPreviewViewModel_ReturnsPrintPreviewWindow()
    {
        WpfContext.Execute(() =>
        {
            var viewModel = new PrintPreviewViewModel(new System.Windows.Documents.FixedDocument(), "Вкладка");

            var (windowType, dataContext) = WpfDialogHostService.ResolveWindowDescriptor(viewModel);

            Assert.Equal(typeof(PrintPreviewWindow), windowType);
            Assert.Same(viewModel, dataContext);
        });
    }

    [Fact]
    public void CreateWindow_PrintPreviewWindow_SetsTitleAndDocumentFromViewModel()
    {
        WpfContext.Execute(() =>
        {
            var document = new System.Windows.Documents.FixedDocument();
            var viewModel = new PrintPreviewViewModel(document, "A3 (алб.) — Без имени");

            var window = WpfDialogHostService.CreateWindow(typeof(PrintPreviewWindow), viewModel);
            try
            {
                var previewWindow = Assert.IsType<PrintPreviewWindow>(window);
                Assert.Equal("Предпросмотр печати — A3 (алб.) — Без имени", previewWindow.Title);
                Assert.Same(document, previewWindow.DocumentViewer.Document);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void ShowDialog_CustomSheetDialogVm_Cancel_ReturnsFalse()
    {
        WpfContext.Execute(() =>
        {
            var vm = new CustomSheetDialogViewModel();
            var host = new WpfDialogHostService();

            // Закрытие планируется ДО ShowDialog — BeginInvoke выполняется в модальном цикле.
            Dispatcher.CurrentDispatcher.BeginInvoke(() => vm.CancelCommand.Execute(null));

            var result = host.ShowDialog(vm);

            Assert.False(result);
        });
    }

    [Fact]
    public void ShowDialog_CustomSheetDialogVm_Confirm_ReturnsTrue()
    {
        WpfContext.Execute(() =>
        {
            var vm = new CustomSheetDialogViewModel();
            var host = new WpfDialogHostService();

            Dispatcher.CurrentDispatcher.BeginInvoke(() => vm.ConfirmCommand.Execute(null));

            var result = host.ShowDialog(vm);

            Assert.True(result);
        });
    }

    [Fact]
    public void ShowDialog_WithOwnerWindow_AssignsOwner()
    {
        WpfContext.Execute(() =>
        {
            var owner = new Window { Width = 100, Height = 100 };
            var vm = new CustomSheetDialogViewModel();
            var host = new WpfDialogHostService();

            owner.Show(); // WPF: Owner можно задать только показанному окну

            try
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(() => vm.CancelCommand.Execute(null));

                var result = host.ShowDialog(vm, owner);

                Assert.False(result);
            }
            finally
            {
                owner.Close();
            }
        });
    }

    [Fact]
    public void ShowDialog_NoExplicitOwner_ResolvesOwnerFromMainWindowProvider()
    {
        WpfContext.Execute(() =>
        {
            var main = new Window();
            main.Show();

            var hasOwnedDialog = false;
            try
            {
                var vm = new CustomSheetDialogViewModel();
                var host = new WpfDialogHostService(() => main);

                Dispatcher.CurrentDispatcher.BeginInvoke(() =>
                {
                    hasOwnedDialog = main.OwnedWindows.Count > 0;
                    vm.CancelCommand.Execute(null);
                });

                host.ShowDialog(vm);

                Assert.True(hasOwnedDialog);
            }
            finally
            {
                main.Close();
            }
        });
    }

    [Fact]
    public void ShowDialog_CustomSheetDialogVm_UnsubscribesHandlersInFinally()
    {
        WpfContext.Execute(() =>
        {
            var vm = new CustomSheetDialogViewModel();
            var host = new WpfDialogHostService();

            Dispatcher.CurrentDispatcher.BeginInvoke(() => vm.CancelCommand.Execute(null));
            host.ShowDialog(vm);

            // Backing-поля field-like событий: после finally-отписки должны быть null.
            var confirmField = typeof(CustomSheetDialogViewModel).GetField(
                "ConfirmRequested", BindingFlags.Instance | BindingFlags.NonPublic);
            var cancelField = typeof(CustomSheetDialogViewModel).GetField(
                "CancelRequested", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.Null(confirmField!.GetValue(vm));
            Assert.Null(cancelField!.GetValue(vm));
        });
    }

    [Fact]
    public void ShowDialog_SettingsViewModel_Cancel_ReturnsFalse()
    {
        WpfContext.Execute(() =>
        {
            var settingsServiceMock = new Mock<ISettingsService>();
            settingsServiceMock.Setup(s => s.Load()).Returns(new AppSettings());
            var vm = new SettingsViewModel(settingsServiceMock.Object);
            var host = new WpfDialogHostService();

            // SettingsView подписывается в своём ctor и закрывает окно сам.
            Dispatcher.CurrentDispatcher.BeginInvoke(() => vm.CancelCommand.Execute(null));

            var result = host.ShowDialog(vm);

            Assert.False(result);
        });
    }

    [Fact]
    public void CreateWindow_ParameterlessCtor_SetsDataContext()
    {
        WpfContext.Execute(() =>
        {
            var window = WpfDialogHostService.CreateWindow(typeof(CustomSheetDialog), "ctx");
            try
            {
                Assert.IsType<CustomSheetDialog>(window);
                Assert.Equal("ctx", window.DataContext);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void CreateWindow_ParameterizedCtor_PassesDataContext()
    {
        WpfContext.Execute(() =>
        {
            var window = WpfDialogHostService.CreateWindow(typeof(TestParamWindow), "ctx");
            try
            {
                Assert.IsType<TestParamWindow>(window);
                Assert.Equal("ctx", window.DataContext);
            }
            finally
            {
                window.Close();
            }
        });
    }
}

/// <summary>
/// Тестовое окно с параметризованным конструктором — детерминированная
/// проверка ветки CreateWindow без headless-зависимостей реального XAML.
/// </summary>
internal sealed class TestParamWindow : Window
{
    public TestParamWindow(object dataContext)
    {
        DataContext = dataContext;
    }
}