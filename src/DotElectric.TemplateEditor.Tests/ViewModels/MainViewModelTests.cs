using System.Collections.ObjectModel;
using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotElectric.TemplateEditor.Tests.ViewModels;

public class MainViewModelTests : IDisposable
{
    private readonly Mock<ITemplateService> _mockTemplateService;
    private readonly Mock<IFileService> _mockFileService;
    private readonly Mock<IDialogService> _mockDialogService;
    private readonly Mock<ISettingsService> _mockSettingsService;
    private readonly Mock<IThemeService> _mockThemeService;
    private readonly Mock<ITemplateLibraryService> _mockTemplateLibraryService;
    private readonly Mock<IPrintService> _mockPrintService;
    private readonly Mock<IPrintDocumentGenerator> _mockPrintDocumentGenerator;
    private readonly Mock<IDialogHostService> _mockDialogHostService;
    private readonly Mock<IApplicationLifecycle> _mockApplicationLifecycle;
    private readonly Mock<ILogger<MainViewModel>> _mockLogger;
    private readonly Mock<IEditorViewModelFactory> _mockEditorFactory;
    private readonly AutosaveService _autosaveService;
    private readonly MainViewModel _viewModel;

    public MainViewModelTests()
    {
        _mockTemplateService = new Mock<ITemplateService>();
        _mockTemplateService.Setup(s => s.CreateNew(It.IsAny<string>(), It.IsAny<SheetOrientation>()))
            .Returns((string fmt, SheetOrientation orient) => new Template());

        _mockFileService = new Mock<IFileService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockSettingsService = new Mock<ISettingsService>();
        _mockSettingsService.Setup(s => s.Get("LastUsedSheetFormat", "A3")).Returns("A3");
        _mockSettingsService.Setup(s => s.Get("LastUsedSheetOrientation", "Landscape")).Returns("Landscape");
        _mockSettingsService.Setup(s => s.Load()).Returns(new AppSettings());

        _mockThemeService = new Mock<IThemeService>();
        _mockThemeService.Setup(t => t.ToggleTheme()).Returns("Dark");

        _mockTemplateLibraryService = new Mock<ITemplateLibraryService>();
        _mockPrintService = new Mock<IPrintService>();
        _mockPrintDocumentGenerator = new Mock<IPrintDocumentGenerator>();
        _mockDialogHostService = new Mock<IDialogHostService>();
        _mockApplicationLifecycle = new Mock<IApplicationLifecycle>();
        _mockLogger = new Mock<ILogger<MainViewModel>>();
        _mockEditorFactory = new Mock<IEditorViewModelFactory>();
        _autosaveService = new AutosaveService(
            _mockTemplateService.Object,
            _mockSettingsService.Object,
            logger: null,
            dispatcherService: null,
            dateTimeProvider: null);

        var mockTemplate = new Template();
        _mockEditorFactory.Setup(f => f.Create(
                It.IsAny<Template>(),
                It.IsAny<GridSettings>(),
                It.IsAny<IPrintService>()))
            .Returns((Template t, GridSettings? gs, IPrintService? ps) =>
            {
                var grid = gs ?? new GridSettings { Enabled = false, StepMicrons = 1000L };
                return new EditorViewModel(
                    t,
                    templateService: _mockTemplateService.Object,
                    gridSettings: grid,
                    printService: ps);
            });

        _mockEditorFactory.Setup(f => f.CreateWithFilePath(
                It.IsAny<Template>(),
                It.IsAny<string>(),
                It.IsAny<GridSettings>(),
                It.IsAny<IPrintService>()))
            .Returns((Template t, string fp, GridSettings? gs, IPrintService? ps) =>
            {
                var grid = gs ?? new GridSettings { Enabled = false, StepMicrons = 1000L };
                return new EditorViewModel(
                    t,
                    filePath: fp,
                    templateService: _mockTemplateService.Object,
                    gridSettings: grid,
                    printService: ps);
            });

        _viewModel = new MainViewModel(
            _mockTemplateService.Object,
            _mockFileService.Object,
            _mockDialogService.Object,
            _mockSettingsService.Object,
            _mockThemeService.Object,
            _mockTemplateLibraryService.Object,
            _mockPrintService.Object,
            _mockPrintDocumentGenerator.Object,
            _mockDialogHostService.Object,
            _mockApplicationLifecycle.Object,
            _mockLogger.Object,
            _mockEditorFactory.Object,
            _autosaveService);
    }

    public void Dispose()
    {
        _viewModel.Dispose();
        _autosaveService.Dispose();
    }

    // ===== Constructor =====

    [Fact]
    public void Constructor_InitializesCollections()
    {
        Assert.NotNull(_viewModel.OpenedTabs);
        Assert.Empty(_viewModel.OpenedTabs);
    }

    [Fact]
    public void Constructor_InitializesTemplateLibraryVm()
    {
        Assert.NotNull(_viewModel.TemplateLibraryVm);
    }

    [Fact]
    public void Constructor_SelectedTabIsNull()
    {
        Assert.Null(_viewModel.SelectedTab);
    }

    [Fact]
    public void Constructor_LoadsThemeFromSettings()
    {
        _mockSettingsService.Setup(s => s.Load()).Returns(new AppSettings { Theme = "Dark" });

        var vm = new MainViewModel(
            _mockTemplateService.Object,
            _mockFileService.Object,
            _mockDialogService.Object,
            _mockSettingsService.Object,
            _mockThemeService.Object,
            _mockTemplateLibraryService.Object,
            _mockPrintService.Object,
            _mockPrintDocumentGenerator.Object,
            _mockDialogHostService.Object,
            _mockApplicationLifecycle.Object,
            _mockLogger.Object,
            _mockEditorFactory.Object,
            _autosaveService);

        Assert.Equal("Dark", vm.Theme);
    }

    // ===== NewTab =====

    [Fact]
    public void NewTabCommand_CreatesNewTab()
    {
        _viewModel.NewTabCommand.Execute("A4");

        Assert.Single(_viewModel.OpenedTabs);
        Assert.NotNull(_viewModel.SelectedTab);
        Assert.Same(_viewModel.OpenedTabs[0], _viewModel.SelectedTab);
    }

    [Fact]
    public void NewTabCommand_WithNullFormat_UsesLastUsed()
    {
        _viewModel.NewTabCommand.Execute(null);

        Assert.Single(_viewModel.OpenedTabs);
        _mockSettingsService.Verify(s => s.Get("LastUsedSheetFormat", "A3"), Times.AtLeastOnce);
    }

    [Fact]
    public void NewTabCommand_ParsesPortraitSuffix()
    {
        _viewModel.NewTabCommand.Execute("A4P");

        Assert.Single(_viewModel.OpenedTabs);
        _mockTemplateService.Verify(s => s.CreateNew("A4", SheetOrientation.Portrait), Times.Once);
    }

    [Fact]
    public void NewTabCommand_ParsesLandscapeSuffix()
    {
        _viewModel.NewTabCommand.Execute("A3L");

        Assert.Single(_viewModel.OpenedTabs);
        _mockTemplateService.Verify(s => s.CreateNew("A3", SheetOrientation.Landscape), Times.Once);
    }

    [Fact]
    public void NewTabCommand_SavesLastUsedFormat()
    {
        _viewModel.NewTabCommand.Execute("A4");

        _mockSettingsService.Verify(s => s.Set("LastUsedSheetFormat", "A4"), Times.Once);
    }

    [Fact]
    public void NewTabCommand_MultipleTabs_AddsAll()
    {
        _viewModel.NewTabCommand.Execute("A4");
        _viewModel.NewTabCommand.Execute("A3");
        _viewModel.NewTabCommand.Execute("A2");

        Assert.Equal(3, _viewModel.OpenedTabs.Count);
    }

    // ===== NewTabWithLastFormat =====

    [Fact]
    public void NewTabWithLastFormatCommand_UsesSettings()
    {
        _viewModel.NewTabWithLastFormatCommand.Execute(null);

        Assert.Single(_viewModel.OpenedTabs);
        _mockSettingsService.Verify(s => s.Get("LastUsedSheetFormat", "A3"), Times.Once);
    }

    // ===== CloseTab =====

    [Fact]
    public async Task CloseTabCommand_RemovesTab()
    {
        _viewModel.NewTabCommand.Execute("A4");
        var tab = _viewModel.OpenedTabs[0];

        await _viewModel.CloseTabCommand.ExecuteAsync(tab);

        Assert.Empty(_viewModel.OpenedTabs);
    }

    [Fact]
    public async Task CloseTabCommand_NullTab_DoesNothing()
    {
        await _viewModel.CloseTabCommand.ExecuteAsync(null!);
        Assert.Empty(_viewModel.OpenedTabs);
    }

    [Fact]
    public async Task CloseTabCommand_LastTab_ClearsSelectedTab()
    {
        _viewModel.NewTabCommand.Execute("A4");
        var tab = _viewModel.OpenedTabs[0];

        await _viewModel.CloseTabCommand.ExecuteAsync(tab);

        Assert.Null(_viewModel.SelectedTab);
    }

    [Fact]
    public async Task CloseTabCommand_NonSelectedTab_KeepsSelectedTab()
    {
        _viewModel.NewTabCommand.Execute("A4");
        _viewModel.NewTabCommand.Execute("A3");
        var firstTab = _viewModel.OpenedTabs[0];

        await _viewModel.CloseTabCommand.ExecuteAsync(firstTab);

        Assert.NotNull(_viewModel.SelectedTab);
        Assert.Single(_viewModel.OpenedTabs);
    }

    [Fact]
    public async Task CloseTabCommand_DisposesTab()
    {
        _viewModel.NewTabCommand.Execute("A4");
        var tab = _viewModel.OpenedTabs[0];

        await _viewModel.CloseTabCommand.ExecuteAsync(tab);

        Assert.DoesNotContain(tab, _viewModel.OpenedTabs);
    }

    // ===== CloseAllTabsAsync =====

    [Fact]
    public async Task CloseAllTabsCommand_ClosesAllTabs()
    {
        _viewModel.NewTabCommand.Execute("A4");
        _viewModel.NewTabCommand.Execute("A3");
        _viewModel.NewTabCommand.Execute("A2");

        await _viewModel.CloseAllTabsCommand.ExecuteAsync(null);

        Assert.Empty(_viewModel.OpenedTabs);
    }

    [Fact]
    public async Task CloseAllTabsCommand_NoTabs_DoesNothing()
    {
        await _viewModel.CloseAllTabsCommand.ExecuteAsync(null);
        Assert.Empty(_viewModel.OpenedTabs);
    }

    // ===== CloseOtherTabs =====

    [Fact]
    public async Task CloseOtherTabsCommand_KeepsSpecifiedTab()
    {
        _viewModel.NewTabCommand.Execute("A4");
        _viewModel.NewTabCommand.Execute("A3");
        _viewModel.NewTabCommand.Execute("A2");
        var keepTab = _viewModel.OpenedTabs[1];

        await _viewModel.CloseOtherTabsCommand.ExecuteAsync(keepTab);

        Assert.Single(_viewModel.OpenedTabs);
        Assert.Same(keepTab, _viewModel.OpenedTabs[0]);
    }

    [Fact]
    public async Task CloseOtherTabsCommand_NullTab_DoesNothing()
    {
        _viewModel.NewTabCommand.Execute("A4");
        await _viewModel.CloseOtherTabsCommand.ExecuteAsync(null!);
        Assert.Single(_viewModel.OpenedTabs);
    }

    // ===== SaveAsync =====

    [Fact]
    public async Task SaveCommand_NoSelectedTab_DoesNothing()
    {
        await _viewModel.SaveCommand.ExecuteAsync(null);
        // No exception
    }

    // ===== SaveAllAsync =====

    [Fact]
    public async Task SaveAllCommand_NoTabs_DoesNothing()
    {
        await _viewModel.SaveAllCommand.ExecuteAsync(null);
        // No exception
    }

    // ===== SaveAsAsync =====

    [Fact]
    public async Task SaveAsCommand_NoSelectedTab_DoesNothing()
    {
        await _viewModel.SaveAsCommand.ExecuteAsync(null);
        // No exception
    }

    // ===== ToggleTheme =====

    [Fact]
    public void ToggleThemeCommand_ChangesTheme()
    {
        _viewModel.ToggleThemeCommand.Execute(null);

        Assert.Equal("Dark", _viewModel.Theme);
        _mockThemeService.Verify(t => t.ToggleTheme(), Times.Once);
    }

    // ===== Print =====

    [Fact]
    public void PrintCommand_NoSelectedTab_DoesNothing()
    {
        // Отсутствует SelectedTab — команда не должна падать
        _viewModel.PrintCommand.Execute(null);
    }

    [Fact]
    public void PrintCommand_WithSelectedTab_DelegatesToTab()
    {
        var template = new Template();
        var editor = new EditorViewModel(template, _mockTemplateService.Object, printService: _mockPrintService.Object);
        _viewModel.OpenedTabs.Add(editor);
        _viewModel.SelectedTab = editor;

        _viewModel.PrintCommand.Execute(null);

        Assert.Equal("Печать доступна через меню печати", editor.StatusMessage);
    }

    // ===== Exit =====

    [Fact]
    public void ExitCommand_CallsShutdown()
    {
        _viewModel.ExitCommand.Execute(null);

        _mockApplicationLifecycle.Verify(a => a.Shutdown(), Times.Once);
    }

    // ===== CanCloseAnyTab =====

    [Fact]
    public void CanCloseAnyTab_NoTabs_ReturnsFalse()
    {
        Assert.False(_viewModel.CanCloseAnyTab());
    }

    [Fact]
    public void CanCloseAnyTab_HasTabs_ReturnsTrue()
    {
        _viewModel.NewTabCommand.Execute("A4");
        Assert.True(_viewModel.CanCloseAnyTab());
    }

    // ===== CanSave =====

    [Fact]
    public void CanSave_NoSelectedTab_ReturnsFalse()
    {
        Assert.False(_viewModel.CanSave());
    }

    [Fact]
    public void CanSave_HasSelectedTab_ReturnsTrue()
    {
        _viewModel.NewTabCommand.Execute("A4");
        Assert.True(_viewModel.CanSave());
    }

    // ===== Dispose =====

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        var ex = Record.Exception(() =>
        {
            _viewModel.Dispose();
            _viewModel.Dispose();
        });
        Assert.Null(ex);
    }
}
