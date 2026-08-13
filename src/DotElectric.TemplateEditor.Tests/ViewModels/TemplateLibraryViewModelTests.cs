using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotElectric.TemplateEditor.Tests.ViewModels;

public class TemplateLibraryViewModelTests
{
    private static Mock<ITemplateLibraryService> CreateServiceMock(IReadOnlyList<TemplateInfo>? templates = null)
    {
        var mockService = new Mock<ITemplateLibraryService>();
        mockService.Setup(s => s.LoadTemplateInfos()).Returns(templates ?? new List<TemplateInfo>());
        return mockService;
    }

    private static void VerifyLogErrorOnce(Mock<ILogger<TemplateLibraryViewModel>> logger)
    {
        logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    // === Constructor / LoadTemplates ===

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
    public void Constructor_CallsLoadTemplateInfosOnce()
    {
        var mockService = CreateServiceMock();

        _ = new TemplateLibraryViewModel(mockService.Object);

        mockService.Verify(s => s.LoadTemplateInfos(), Times.Once);
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
    public void Constructor_TwoItems_SetsStatusMessage()
    {
        var mockService = CreateServiceMock(new List<TemplateInfo>
        {
            new("a.tdel", "A", @"C:\a.tdel"),
            new("b.tdel", "B", @"C:\b.tdel")
        });

        var vm = new TemplateLibraryViewModel(mockService.Object);

        Assert.Equal("Шаблонов: 2", vm.StatusMessage);
        Assert.Equal(2, vm.Templates.Count);
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
    public void LoadTemplates_ServiceThrows_LogsError()
    {
        var mockService = new Mock<ITemplateLibraryService>();
        mockService.Setup(s => s.LoadTemplateInfos()).Throws(new Exception("Access denied"));
        var loggerMock = new Mock<ILogger<TemplateLibraryViewModel>>();

        var vm = new TemplateLibraryViewModel(mockService.Object, null, null, loggerMock.Object);

        Assert.StartsWith("Ошибка:", vm.StatusMessage);
        VerifyLogErrorOnce(loggerMock);
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
    public void LoadTemplates_RepeatedReload_NoDuplicates()
    {
        var mockService = CreateServiceMock(new List<TemplateInfo>
        {
            new("a.tdel", "A", @"C:\a.tdel")
        });

        var vm = new TemplateLibraryViewModel(mockService.Object);
        vm.LoadTemplates();

        Assert.Single(vm.Templates);
        Assert.Equal("Шаблонов: 1", vm.StatusMessage);
    }

    // === ImportToLibraryCommand ===

    [Fact]
    public void ImportToLibrary_NoFileService_NoOp()
    {
        var mockService = CreateServiceMock();

        var vm = new TemplateLibraryViewModel(mockService.Object);
        vm.ImportToLibraryCommand.Execute(null);

        mockService.Verify(s => s.CopyToLibrary(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
        Assert.Empty(vm.Templates);
    }

    [Fact]
    public void ImportToLibrary_DialogReturnsNull_NoOp()
    {
        var mockService = CreateServiceMock();
        var fileServiceMock = new Mock<IFileService>();
        fileServiceMock.Setup(f => f.OpenFileDialog(It.IsAny<string>())).Returns((string)null!);

        var vm = new TemplateLibraryViewModel(mockService.Object, null, fileServiceMock.Object);
        vm.ImportToLibraryCommand.Execute(null);

        mockService.Verify(s => s.CopyToLibrary(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public void ImportToLibrary_DialogReturnsWhitespace_NoOp()
    {
        var mockService = CreateServiceMock();
        var fileServiceMock = new Mock<IFileService>();
        fileServiceMock.Setup(f => f.OpenFileDialog(It.IsAny<string>())).Returns("   ");

        var vm = new TemplateLibraryViewModel(mockService.Object, null, fileServiceMock.Object);
        vm.ImportToLibraryCommand.Execute(null);

        mockService.Verify(s => s.CopyToLibrary(It.IsAny<string>(), It.IsAny<string?>()), Times.Never);
    }

    [Fact]
    public void ImportToLibrary_HappyPath_AddsTemplateAndUpdatesStatus()
    {
        var mockService = CreateServiceMock();
        var fileServiceMock = new Mock<IFileService>();
        fileServiceMock.Setup(f => f.OpenFileDialog(It.IsAny<string>())).Returns(@"C:\Templates\new.tdel");
        var info = new TemplateInfo("new.tdel", "new", @"C:\Templates\new.tdel");
        mockService.Setup(s => s.CopyToLibrary(@"C:\Templates\new.tdel", It.IsAny<string?>())).Returns(info);

        var vm = new TemplateLibraryViewModel(mockService.Object, null, fileServiceMock.Object);
        vm.ImportToLibraryCommand.Execute(null);

        Assert.Single(vm.Templates);
        Assert.Same(info, vm.Templates[0]);
        Assert.Equal("Шаблонов: 1", vm.StatusMessage);
    }

    [Fact]
    public void ImportToLibrary_CopyThrows_LogsErrorAndSetsStatus()
    {
        var mockService = CreateServiceMock();
        var fileServiceMock = new Mock<IFileService>();
        fileServiceMock.Setup(f => f.OpenFileDialog(It.IsAny<string>())).Returns(@"C:\Templates\broken.tdel");
        mockService.Setup(s => s.CopyToLibrary(It.IsAny<string>(), It.IsAny<string?>()))
            .Throws(new FileNotFoundException("Исходный файл не найден."));
        var loggerMock = new Mock<ILogger<TemplateLibraryViewModel>>();

        var vm = new TemplateLibraryViewModel(mockService.Object, null, fileServiceMock.Object, loggerMock.Object);
        vm.ImportToLibraryCommand.Execute(null);

        Assert.StartsWith("Ошибка:", vm.StatusMessage);
        Assert.Empty(vm.Templates);
        VerifyLogErrorOnce(loggerMock);
    }

    [Fact]
    public void ImportToLibrary_UsesTdelFilter()
    {
        var mockService = CreateServiceMock();
        var fileServiceMock = new Mock<IFileService>();
        fileServiceMock.Setup(f => f.OpenFileDialog(It.IsAny<string>())).Returns(@"C:\Templates\new.tdel");
        mockService.Setup(s => s.CopyToLibrary(It.IsAny<string>(), It.IsAny<string?>()))
            .Returns(new TemplateInfo("new.tdel", "new", @"C:\Templates\new.tdel"));

        var vm = new TemplateLibraryViewModel(mockService.Object, null, fileServiceMock.Object);
        vm.ImportToLibraryCommand.Execute(null);

        fileServiceMock.Verify(
            f => f.OpenFileDialog("Файлы шаблонов (*.tdel)|*.tdel|Все файлы (*.*)|*.*"),
            Times.Once);
    }

    // === RemoveFromLibraryCommand ===

    [Fact]
    public void RemoveFromLibrary_NoSelection_NoOp()
    {
        var mockService = CreateServiceMock();

        var vm = new TemplateLibraryViewModel(mockService.Object);
        vm.RemoveFromLibraryCommand.Execute(null);

        mockService.Verify(s => s.RemoveFromLibrary(It.IsAny<TemplateInfo>()), Times.Never);
    }

    [Fact]
    public void RemoveFromLibrary_HappyPath_RemovesAndClearsSelection()
    {
        var mockService = CreateServiceMock(new List<TemplateInfo>
        {
            new("a.tdel", "A", @"C:\a.tdel"),
            new("b.tdel", "B", @"C:\b.tdel")
        });

        var vm = new TemplateLibraryViewModel(mockService.Object);
        var template = vm.Templates[0];
        vm.SelectedTemplate = template;

        vm.RemoveFromLibraryCommand.Execute(null);

        Assert.DoesNotContain(template, vm.Templates);
        Assert.Null(vm.SelectedTemplate);
        Assert.Equal("Шаблонов: 1", vm.StatusMessage);
    }

    [Fact]
    public void RemoveFromLibrary_RemovingLastItem_SetsEmptyMessage()
    {
        var mockService = CreateServiceMock(new List<TemplateInfo>
        {
            new("a.tdel", "A", @"C:\a.tdel")
        });

        var vm = new TemplateLibraryViewModel(mockService.Object);
        vm.SelectedTemplate = vm.Templates[0];

        vm.RemoveFromLibraryCommand.Execute(null);

        Assert.Empty(vm.Templates);
        Assert.Equal("Нет шаблонов в библиотеке", vm.StatusMessage);
    }

    [Fact]
    public void RemoveFromLibrary_ServiceThrows_TemplateStays()
    {
        var mockService = CreateServiceMock(new List<TemplateInfo>
        {
            new("a.tdel", "A", @"C:\a.tdel")
        });
        mockService.Setup(s => s.RemoveFromLibrary(It.IsAny<TemplateInfo>()))
            .Throws(new FileNotFoundException("Файл шаблона не найден."));
        var loggerMock = new Mock<ILogger<TemplateLibraryViewModel>>();

        var vm = new TemplateLibraryViewModel(mockService.Object, null, null, loggerMock.Object);
        var template = vm.Templates[0];
        vm.SelectedTemplate = template;

        vm.RemoveFromLibraryCommand.Execute(null);

        Assert.Contains(template, vm.Templates);
        Assert.Same(template, vm.SelectedTemplate);
        Assert.StartsWith("Ошибка:", vm.StatusMessage);
        VerifyLogErrorOnce(loggerMock);
    }

    // === OpenTemplateCommand ===

    [Fact]
    public void OpenTemplate_InvokesCallback()
    {
        TemplateInfo? captured = null;
        var mockService = CreateServiceMock();

        var vm = new TemplateLibraryViewModel(mockService.Object, t => captured = t);
        var template = new TemplateInfo("test.tdel", "Test", @"C:\test.tdel");

        vm.OpenTemplateCommand.Execute(template);

        Assert.Same(template, captured);
    }

    [Fact]
    public void OpenTemplate_Null_DoesNotInvokeCallback()
    {
        var invoked = false;
        var mockService = CreateServiceMock();

        var vm = new TemplateLibraryViewModel(mockService.Object, _ => invoked = true);

        vm.OpenTemplateCommand.Execute(null);

        Assert.False(invoked);
    }

    [Fact]
    public void OpenTemplate_NullCallback_DoesNotThrow()
    {
        var mockService = CreateServiceMock();

        var vm = new TemplateLibraryViewModel(mockService.Object);
        var template = new TemplateInfo("test.tdel", "Test", @"C:\test.tdel");

        vm.OpenTemplateCommand.Execute(template);
    }

    // === PropertyChanged ===

    [Fact]
    public void SelectedTemplate_Set_FiresPropertyChanged()
    {
        var mockService = CreateServiceMock();
        var vm = new TemplateLibraryViewModel(mockService.Object);

        string? changedProperty = null;
        vm.PropertyChanged += (_, e) => changedProperty = e.PropertyName;

        vm.SelectedTemplate = new TemplateInfo("a.tdel", "A", @"C:\a.tdel");

        Assert.Equal(nameof(TemplateLibraryViewModel.SelectedTemplate), changedProperty);
    }

    [Fact]
    public void StatusMessage_Change_FiresPropertyChanged()
    {
        var mockService = CreateServiceMock();
        var vm = new TemplateLibraryViewModel(mockService.Object);

        string? changedProperty = null;
        vm.PropertyChanged += (_, e) => changedProperty = e.PropertyName;

        vm.StatusMessage = "Новый статус";

        Assert.Equal(nameof(TemplateLibraryViewModel.StatusMessage), changedProperty);
    }

    [Fact]
    public void Constructor_ThrowsOnNullService()
    {
        Assert.Throws<ArgumentNullException>(() => new TemplateLibraryViewModel(null!));
    }
}
