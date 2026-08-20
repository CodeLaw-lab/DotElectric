using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace DotElectric.TemplateEditor.Tests.ViewModels;

public class EditorViewModelFactoryTests
{
    private readonly Mock<IPrintService> _mockPrintService;
    private readonly IServiceProvider _serviceProvider;

    public EditorViewModelFactoryTests()
    {
        _mockPrintService = new Mock<IPrintService>();

        var services = new ServiceCollection();
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

    [Fact]
    public void Create_NoGridSettings_UsesAppSettings()
    {
        var mockSettingsService = new Mock<ISettingsService>();
        mockSettingsService.Setup(s => s.Load()).Returns(new AppSettings
        {
            ShowGrid = false,
            GridStepMm = 1.0,
            GridMaxNodes = 50000,
            GridNodeSize = 3.5,
        });
        var factory = new EditorViewModelFactory(_serviceProvider, settingsService: mockSettingsService.Object);
        var template = new Template();

        var vm = factory.Create(template);

        Assert.NotNull(vm);
        Assert.False(vm.GridSettings.Enabled);
        Assert.Equal(1000, vm.GridSettings.StepMicrons);
        Assert.Equal(50000, vm.GridSettings.MaxGridNodes);
        Assert.Equal(3.5, vm.GridSettings.NodeSize);
    }

    [Fact]
    public void Create_WithExplicitGridSettings_ExplicitWins()
    {
        var mockSettingsService = new Mock<ISettingsService>();
        mockSettingsService.Setup(s => s.Load()).Returns(new AppSettings { GridStepMm = 1.0 });
        var factory = new EditorViewModelFactory(_serviceProvider, settingsService: mockSettingsService.Object);
        var template = new Template();

        var vm = factory.Create(template, new GridSettings { StepMicrons = 10000 });

        Assert.NotNull(vm);
        Assert.Equal(10000, vm.GridSettings.StepMicrons);
        mockSettingsService.Verify(s => s.Load(), Times.Never);
    }

    [Fact]
    public void CreateWithFilePath_WithExplicitGridSettings_ExplicitWinsAndLoadNotCalled()
    {
        // Arrange — settings service вернул бы шаг 1 мм, но явный GridSettings должен победить
        var mockSettingsService = new Mock<ISettingsService>();
        mockSettingsService.Setup(s => s.Load()).Returns(new AppSettings { GridStepMm = 1.0 });
        var factory = new EditorViewModelFactory(_serviceProvider, settingsService: mockSettingsService.Object);
        var template = new Template();

        // Act
        var vm = factory.CreateWithFilePath(template, @"C:\test\t.tdel", new GridSettings { StepMicrons = 10000 });

        // Assert — explicit настройки применены, Load() не вызывался
        Assert.NotNull(vm);
        Assert.Equal(10000, vm.GridSettings.StepMicrons);
        mockSettingsService.Verify(s => s.Load(), Times.Never);
    }

    [Fact]
    public void Create_NoSettingsService_FallsBackToDefaults()
    {
        var factory = new EditorViewModelFactory(_serviceProvider);
        var template = new Template();

        var vm = factory.Create(template);

        Assert.NotNull(vm);
        Assert.True(vm.GridSettings.Enabled);
        Assert.Equal(EditorSettings.DefaultGridStepMicrons, vm.GridSettings.StepMicrons);
        Assert.Equal(EditorSettings.MaxGridNodes, vm.GridSettings.MaxGridNodes);
        Assert.Null(vm.GridSettings.NodeColor);
        Assert.Equal(2.0, vm.GridSettings.NodeSize);
    }

    [Fact]
    public void CreateWithFilePath_NoGridSettings_UsesAppSettings()
    {
        var mockSettingsService = new Mock<ISettingsService>();
        mockSettingsService.Setup(s => s.Load()).Returns(new AppSettings { ShowGrid = false });
        var factory = new EditorViewModelFactory(_serviceProvider, settingsService: mockSettingsService.Object);
        var template = new Template();

        var vm = factory.CreateWithFilePath(template, @"C:\test\template.tdel");

        Assert.NotNull(vm);
        Assert.False(vm.GridSettings.Enabled);
        mockSettingsService.Verify(s => s.Load(), Times.Once);
    }

    [Fact]
    public void Create_WithAppSettings_NodeColorAndSnapApplied()
    {
        var mockSettingsService = new Mock<ISettingsService>();
        mockSettingsService.Setup(s => s.Load()).Returns(new AppSettings
        {
            SnapToGrid = false,
            GridNodeColor = "#00FF00",
        });
        var factory = new EditorViewModelFactory(_serviceProvider, settingsService: mockSettingsService.Object);
        var template = new Template();

        var vm = factory.Create(template);

        Assert.NotNull(vm);
        Assert.False(vm.GridSettings.SnapEnabled);
        Assert.Equal("#00FF00", vm.GridSettings.NodeColor);
    }

    [Fact]
    public void Create_SettingsServiceLoadCalledOnce()
    {
        var mockSettingsService = new Mock<ISettingsService>();
        mockSettingsService.Setup(s => s.Load()).Returns(new AppSettings());
        var factory = new EditorViewModelFactory(_serviceProvider, settingsService: mockSettingsService.Object);
        var template = new Template();

        factory.Create(template);

        mockSettingsService.Verify(s => s.Load(), Times.Once);
    }

    [Fact]
    public void Create_SettingsServiceLoadReturnsNull_FallsBackToDefaults()
    {
        var mockSettingsService = new Mock<ISettingsService>();
        mockSettingsService.Setup(s => s.Load()).Returns((AppSettings)null!);
        var factory = new EditorViewModelFactory(_serviceProvider, settingsService: mockSettingsService.Object);
        var template = new Template();

        var vm = factory.Create(template);

        Assert.NotNull(vm);
        Assert.True(vm.GridSettings.Enabled);
        Assert.Equal(EditorSettings.DefaultGridStepMicrons, vm.GridSettings.StepMicrons);
    }

    [Fact]
    public void Create_WithConstructorThemeService_AppliesTheme()
    {
        var mockThemeService = new Mock<IThemeService>();
        mockThemeService.Setup(t => t.CurrentTheme).Returns("Dark");
        var factory = new EditorViewModelFactory(_serviceProvider, themeService: mockThemeService.Object);
        var template = new Template();

        var vm = factory.Create(template);

        Assert.NotNull(vm);
        Assert.True(vm.IsDarkTheme);
    }

    [Fact]
    public void CreateWithFilePath_WithConstructorGridNodeGenerator_Works()
    {
        var mockGridNodeGenerator = new Mock<IGridNodeGenerator>();
        var factory = new EditorViewModelFactory(_serviceProvider, gridNodeGenerator: mockGridNodeGenerator.Object);
        var template = new Template();

        var vm = factory.CreateWithFilePath(template, @"C:\test\template.tdel");

        Assert.NotNull(vm);
        Assert.Equal(@"C:\test\template.tdel", vm.DirtyStateManager.FilePath);
    }
}
