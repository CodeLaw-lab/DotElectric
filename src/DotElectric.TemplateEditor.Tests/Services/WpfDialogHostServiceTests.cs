using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.ViewModels;
using DotElectric.TemplateEditor.Views;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Services;

/// <summary>
/// MTA-тесты ResolveWindowDescriptor (чистый switch, без создания окон).
/// </summary>
public class WpfDialogHostServiceTests
{
    [Fact]
    public void ResolveWindowDescriptor_SettingsViewModel_ReturnsSettingsView()
    {
        var settingsServiceMock = new Mock<ISettingsService>();
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
}
