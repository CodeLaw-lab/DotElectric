using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.ViewModels;
using Moq;

namespace DotElectric.TemplateEditor.Tests.ViewModels;

public class TemplateLibraryViewModelTests
{
    [Fact]
    public void Constructor_LoadsTemplates()
    {
        var mockService = new Mock<ITemplateLibraryService>();
        var templates = new List<TemplateInfo>
        {
            new("frame1.tdel", "Рамка А3", @"C:\Templates\frame1.tdel"),
            new("frame2.tdel", "Рамка А4", @"C:\Templates\frame2.tdel")
        };
        mockService.Setup(s => s.LoadTemplateInfos()).Returns(templates);

        var vm = new TemplateLibraryViewModel(mockService.Object);

        Assert.Equal(2, vm.Templates.Count);
        Assert.Equal("Рамка А3", vm.Templates[0].DisplayName);
    }

    [Fact]
    public void Constructor_EmptyList_SetsStatusMessage()
    {
        var mockService = new Mock<ITemplateLibraryService>();
        mockService.Setup(s => s.LoadTemplateInfos()).Returns(new List<TemplateInfo>());

        var vm = new TemplateLibraryViewModel(mockService.Object);

        Assert.Equal("Нет шаблонов в библиотеке", vm.StatusMessage);
    }

    [Fact]
    public void Constructor_ServiceThrows_SetsErrorMessage()
    {
        var mockService = new Mock<ITemplateLibraryService>();
        mockService.Setup(s => s.LoadTemplateInfos()).Throws(new Exception("Access denied"));

        var vm = new TemplateLibraryViewModel(mockService.Object);

        Assert.Contains("Ошибка", vm.StatusMessage);
        Assert.Empty(vm.Templates);
    }

    [Fact]
    public void LoadTemplates_RefreshesList()
    {
        var mockService = new Mock<ITemplateLibraryService>();
        mockService.Setup(s => s.LoadTemplateInfos())
            .Returns(new List<TemplateInfo>
            {
                new("a.tdel", "A", @"C:\a.tdel")
            });

        var vm = new TemplateLibraryViewModel(mockService.Object);
        Assert.Single(vm.Templates);

        // Обновляем моки
        mockService.Setup(s => s.LoadTemplateInfos())
            .Returns(new List<TemplateInfo>
            {
                new("a.tdel", "A", @"C:\a.tdel"),
                new("b.tdel", "B", @"C:\b.tdel")
            });

        vm.LoadTemplates();

        Assert.Equal(2, vm.Templates.Count);
    }

    [Fact]
    public void OpenTemplate_InvokesCallback()
    {
        TemplateInfo? captured = null;
        var mockService = new Mock<ITemplateLibraryService>();
        mockService.Setup(s => s.LoadTemplateInfos()).Returns(new List<TemplateInfo>());

        var vm = new TemplateLibraryViewModel(mockService.Object, t => captured = t);
        var template = new TemplateInfo("test.tdel", "Test", @"C:\test.tdel");

        vm.OpenTemplateCommand.Execute(template);

        Assert.Same(template, captured);
    }

    [Fact]
    public void OpenTemplate_Null_DoesNotInvokeCallback()
    {
        var invoked = false;
        var mockService = new Mock<ITemplateLibraryService>();
        mockService.Setup(s => s.LoadTemplateInfos()).Returns(new List<TemplateInfo>());

        var vm = new TemplateLibraryViewModel(mockService.Object, _ => invoked = true);

        vm.OpenTemplateCommand.Execute(null);

        Assert.False(invoked);
    }

    [Fact]
    public void Constructor_ThrowsOnNullService()
    {
        Assert.Throws<ArgumentNullException>(() => new TemplateLibraryViewModel(null!));
    }
}
