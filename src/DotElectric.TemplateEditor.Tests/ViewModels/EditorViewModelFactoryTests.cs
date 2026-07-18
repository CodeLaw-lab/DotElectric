using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace DotElectric.TemplateEditor.Tests.ViewModels;

public class EditorViewModelFactoryTests
{
    private readonly Mock<ITemplateService> _mockTemplateService;
    private readonly Mock<IPrintService> _mockPrintService;
    private readonly IServiceProvider _serviceProvider;

    public EditorViewModelFactoryTests()
    {
        _mockTemplateService = new Mock<ITemplateService>();
        _mockPrintService = new Mock<IPrintService>();

        var services = new ServiceCollection();
        services.AddSingleton(_mockTemplateService.Object);
        services.AddSingleton(_mockPrintService.Object);
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void Constructor_NullServiceProvider_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new EditorViewModelFactory(null!));
    }

    [Fact]
    public void Create_ValidTemplate_ReturnsEditorViewModel()
    {
        var factory = new EditorViewModelFactory(_serviceProvider);
        var template = new Template();

        var vm = factory.Create(template);

        Assert.NotNull(vm);
        Assert.Null(vm.DirtyStateManager.FilePath);
        Assert.Equal(template, vm.Template);
    }

    [Fact]
    public void Create_NullTemplate_ThrowsArgumentNullException()
    {
        var factory = new EditorViewModelFactory(_serviceProvider);
        Assert.Throws<ArgumentNullException>(() => factory.Create(null!));
    }

    [Fact]
    public void Create_WithGridSettings_UsesProvidedSettings()
    {
        var factory = new EditorViewModelFactory(_serviceProvider);
        var template = new Template();
        var gridSettings = new GridSettings { StepMicrons = 10000 };

        var vm = factory.Create(template, gridSettings);

        Assert.NotNull(vm);
        Assert.Equal(10000, vm.GridSettings.StepMicrons);
    }

    [Fact]
    public void Create_WithPrintService_UsesProvidedService()
    {
        var factory = new EditorViewModelFactory(_serviceProvider);
        var template = new Template();
        var customPrintService = new Mock<IPrintService>();

        var vm = factory.Create(template, printService: customPrintService.Object);

        Assert.NotNull(vm);
    }

    [Fact]
    public void CreateWithFilePath_ValidTemplate_ReturnsEditorViewModelWithPath()
    {
        var factory = new EditorViewModelFactory(_serviceProvider);
        var template = new Template();
        var filePath = @"C:\test\template.tdel";

        var vm = factory.CreateWithFilePath(template, filePath);

        Assert.NotNull(vm);
        Assert.Equal(filePath, vm.DirtyStateManager.FilePath);
        Assert.Equal(template, vm.Template);
    }

    [Fact]
    public void CreateWithFilePath_NullTemplate_ThrowsArgumentNullException()
    {
        var factory = new EditorViewModelFactory(_serviceProvider);
        Assert.Throws<ArgumentNullException>(() => factory.CreateWithFilePath(null!, "path.tdel"));
    }

    [Fact]
    public void CreateWithFilePath_EmptyFilePath_ThrowsArgumentException()
    {
        var factory = new EditorViewModelFactory(_serviceProvider);
        var template = new Template();
        Assert.Throws<ArgumentException>(() => factory.CreateWithFilePath(template, ""));
    }

    [Fact]
    public void CreateWithFilePath_WhiteSpaceFilePath_ThrowsArgumentException()
    {
        var factory = new EditorViewModelFactory(_serviceProvider);
        var template = new Template();
        Assert.Throws<ArgumentException>(() => factory.CreateWithFilePath(template, "   "));
    }

    [Fact]
    public void CreateWithFilePath_NullFilePath_ThrowsArgumentException()
    {
        var factory = new EditorViewModelFactory(_serviceProvider);
        var template = new Template();
        Assert.Throws<ArgumentException>(() => factory.CreateWithFilePath(template, null!));
    }
}
