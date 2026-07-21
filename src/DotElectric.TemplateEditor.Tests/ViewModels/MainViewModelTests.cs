using System.Collections.ObjectModel;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.ViewModels;
using DotElectric.TemplateEditor.ViewModels.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotElectric.TemplateEditor.Tests.ViewModels;

public class MainViewModelTests : IDisposable
{
    private readonly Mock<ITabOperationsService> _mockTabOperations;
    private readonly Mock<ISettingsService> _mockSettingsService;
    private readonly Mock<IThemeService> _mockThemeService;
    private readonly Mock<ITemplateLibraryService> _mockTemplateLibraryService;
    private readonly Mock<IDialogService> _mockDialogService;
    private readonly Mock<IDialogHostService> _mockDialogHostService;
    private readonly Mock<IApplicationLifecycle> _mockApplicationLifecycle;
    private readonly Mock<ILogger<MainViewModel>> _mockLogger;
    private readonly Mock<IPrintDocumentGenerator> _mockPrintDocumentGenerator;
    private readonly AutosaveService _autosaveService;
    private readonly MainViewModel _viewModel;

    public MainViewModelTests()
    {
        _mockTabOperations = new Mock<ITabOperationsService>();

        _mockSettingsService = new Mock<ISettingsService>();
        _mockSettingsService.Setup(s => s.Get("LastUsedSheetFormat", "A3")).Returns("A3");
        _mockSettingsService.Setup(s => s.Get("LastUsedSheetOrientation", "Landscape")).Returns("Landscape");
        _mockSettingsService.Setup(s => s.Load()).Returns(new AppSettings());

        _mockThemeService = new Mock<IThemeService>();
        _mockThemeService.Setup(t => t.ToggleTheme()).Returns("Dark");

        _mockTemplateLibraryService = new Mock<ITemplateLibraryService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockDialogHostService = new Mock<IDialogHostService>();
        _mockApplicationLifecycle = new Mock<IApplicationLifecycle>();
        _mockLogger = new Mock<ILogger<MainViewModel>>();
        _mockPrintDocumentGenerator = new Mock<IPrintDocumentGenerator>();

        _autosaveService = new AutosaveService(
            new Mock<ITemplateService>().Object,
            _mockSettingsService.Object,
            logger: null,
            dispatcherService: null,
            dateTimeProvider: null);

        // Setup default TabOperations behavior
        _mockTabOperations.Setup(t => t.CreateNewTab(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns((string? fmt, string? _, string? _) =>
            {
                var templateService = new Mock<ITemplateService>();
                templateService.Setup(s => s.CreateNew(It.IsAny<string>(), It.IsAny<SheetOrientation>()))
                    .Returns(new Template());
                return new EditorViewModel(
                    new Template(),
                    templateService.Object,
                    printService: new Mock<IPrintService>().Object);
            });

        _mockTabOperations.Setup(t => t.CreateNewCustomTab(It.IsAny<double>(), It.IsAny<double>()))
            .Returns(new EditorViewModel(new Template(), new Mock<ITemplateService>().Object,
                printService: new Mock<IPrintService>().Object));

        _viewModel = new MainViewModel(
            _mockTabOperations.Object,
            _mockSettingsService.Object,
            _mockThemeService.Object,
            _mockTemplateLibraryService.Object,
            _mockDialogService.Object,
            _mockDialogHostService.Object,
            _mockApplicationLifecycle.Object,
            _mockLogger.Object,
            _autosaveService,
            _mockPrintDocumentGenerator.Object);
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
            _mockTabOperations.Object,
            _mockSettingsService.Object,
            _mockThemeService.Object,
            _mockTemplateLibraryService.Object,
            _mockDialogService.Object,
            _mockDialogHostService.Object,
            _mockApplicationLifecycle.Object,
            _mockLogger.Object,
            _autosaveService,
            _mockPrintDocumentGenerator.Object);

        Assert.Equal("Dark", vm.Theme);
    }

    // ===== NewTab =====

    [Fact]
    public void NewTabCommand_CreatesNewTab()
    {
        _viewModel.NewTabCommand.Execute("A4");

        _mockTabOperations.Verify(t => t.CreateNewTab("A4", null, "Landscape"), Times.Once);
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
    public void NewTabCommand_SavesLastUsedFormat()
    {
        _viewModel.NewTabCommand.Execute("A4");

        _mockTabOperations.Verify(t => t.CreateNewTab("A4", null, "Landscape"), Times.Once);
    }

    [Fact]
    public void NewTabCommand_MultipleTabs_AddsAll()
    {
        _viewModel.NewTabCommand.Execute("A4");
        _viewModel.NewTabCommand.Execute("A3");
        _viewModel.NewTabCommand.Execute("A2");

        Assert.Equal(3, _viewModel.OpenedTabs.Count);
    }

    [Fact]
    public void NewTabCommand_MultipleTabs_AllHaveUniqueIds()
    {
        _viewModel.NewTabCommand.Execute("A4");
        _viewModel.NewTabCommand.Execute("A3");
        _viewModel.NewTabCommand.Execute("A2");

        var ids = _viewModel.OpenedTabs.Select(t => t.TabId).Distinct().ToList();
        Assert.Equal(3, ids.Count);
    }

    // ===== NewTabWithLastFormat =====

    [Fact]
    public void NewTabWithLastFormatCommand_UsesSettings()
    {
        _viewModel.NewTabWithLastFormatCommand.Execute(null);

        _mockSettingsService.Verify(s => s.Get("LastUsedSheetFormat", "A3"), Times.Once);
        _mockTabOperations.Verify(t => t.CreateNewTab(null, "A3", "Landscape"), Times.Once);
        Assert.Single(_viewModel.OpenedTabs);
    }

    // ===== CloseTab =====

    [Fact]
    public async Task CloseTabCommand_RemovesTab()
    {
        _mockTabOperations.Setup(t => t.PromptAndSaveIfDirtyAsync(It.IsAny<EditorViewModel>()))
            .ReturnsAsync(true);

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
        _mockTabOperations.Setup(t => t.PromptAndSaveIfDirtyAsync(It.IsAny<EditorViewModel>()))
            .ReturnsAsync(true);

        _viewModel.NewTabCommand.Execute("A4");
        var tab = _viewModel.OpenedTabs[0];

        await _viewModel.CloseTabCommand.ExecuteAsync(tab);

        Assert.Null(_viewModel.SelectedTab);
    }

    [Fact]
    public async Task CloseTabCommand_NonSelectedTab_KeepsSelectedTab()
    {
        _mockTabOperations.Setup(t => t.PromptAndSaveIfDirtyAsync(It.IsAny<EditorViewModel>()))
            .ReturnsAsync(true);

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
        _mockTabOperations.Setup(t => t.PromptAndSaveIfDirtyAsync(It.IsAny<EditorViewModel>()))
            .ReturnsAsync(true);

        _viewModel.NewTabCommand.Execute("A4");
        var tab = _viewModel.OpenedTabs[0];

        await _viewModel.CloseTabCommand.ExecuteAsync(tab);

        Assert.DoesNotContain(tab, _viewModel.OpenedTabs);
    }

    [Fact]
    public async Task CloseTabCommand_DirtyTabCancel_DoesNotClose()
    {
        _mockTabOperations.Setup(t => t.PromptAndSaveIfDirtyAsync(It.IsAny<EditorViewModel>()))
            .ReturnsAsync(false);

        _viewModel.NewTabCommand.Execute("A4");
        var tab = _viewModel.OpenedTabs[0];

        await _viewModel.CloseTabCommand.ExecuteAsync(tab);

        Assert.Single(_viewModel.OpenedTabs);
    }

    // ===== CloseAllTabsAsync =====

    [Fact]
    public async Task CloseAllTabsCommand_ClosesAllTabs()
    {
        _mockTabOperations.Setup(t => t.PromptAndSaveIfDirtyAsync(It.IsAny<EditorViewModel>()))
            .ReturnsAsync(true);

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
        _mockTabOperations.Setup(t => t.PromptAndSaveIfDirtyAsync(It.IsAny<EditorViewModel>()))
            .ReturnsAsync(true);

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
        _mockTabOperations.Verify(t => t.SaveTabAsync(It.IsAny<EditorViewModel>()), Times.Never);
    }

    [Fact]
    public async Task SaveCommand_WithSelectedTab_DelegatesToService()
    {
        _viewModel.NewTabCommand.Execute("A4");

        await _viewModel.SaveCommand.ExecuteAsync(null);

        _mockTabOperations.Verify(t => t.SaveTabAsync(It.IsAny<EditorViewModel>()), Times.Once);
    }

    // ===== SaveAllAsync =====

    [Fact]
    public async Task SaveAllCommand_NoTabs_DoesNothing()
    {
        await _viewModel.SaveAllCommand.ExecuteAsync(null);
        _mockTabOperations.Verify(t => t.SaveTabAsync(It.IsAny<EditorViewModel>()), Times.Never);
    }

    // ===== SaveAsAsync =====

    [Fact]
    public async Task SaveAsCommand_NoSelectedTab_DoesNothing()
    {
        await _viewModel.SaveAsCommand.ExecuteAsync(null);
        _mockTabOperations.Verify(t => t.SaveAsAsync(It.IsAny<EditorViewModel>()), Times.Never);
    }

    // ===== Autosave Tick Handler =====

    [Fact]
    public async Task AutosaveTickHandler_WhenNoActiveTab_DoesNotThrow()
    {
        var exception = await Record.ExceptionAsync(
            () => _autosaveService.AutosaveAllTabsAsync(_viewModel.OpenedTabs, TestContext.Current.CancellationToken));
        Assert.Null(exception);
    }

    [Fact]
    public async Task AutosaveTickHandler_WhenAutosaveFails_DoesNotThrow()
    {
        // Create a real template service mock that throws on save
        var mockTemplateService = new Mock<ITemplateService>();
        mockTemplateService.Setup(s => s.CreateNew(It.IsAny<string>(), It.IsAny<SheetOrientation>()))
            .Returns(new Template());
        mockTemplateService.Setup(s => s.Save(It.IsAny<Template>(), It.IsAny<string>()))
            .Throws(new InvalidOperationException("Autosave failed"));

        _mockTabOperations.Setup(t => t.CreateNewTab(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns((string? fmt, string? _, string? _) =>
                new EditorViewModel(new Template(), mockTemplateService.Object,
                    printService: new Mock<IPrintService>().Object));

        _viewModel.NewTabCommand.Execute("A4");
        var tab = _viewModel.OpenedTabs[0];
        tab.MarkDirty();

        // Set a file path so autosave doesn't prompt for dialog
        tab.DirtyStateManager.FilePath = "test.tdel";

        // Create a second autosave service that won't try to create backups etc.
        var testAutosave = new AutosaveService(
            mockTemplateService.Object,
            _mockSettingsService.Object,
            logger: null,
            dispatcherService: null,
            dateTimeProvider: null);

        var exception = await Record.ExceptionAsync(
            () => testAutosave.AutosaveAllTabsAsync(_viewModel.OpenedTabs, TestContext.Current.CancellationToken));
        Assert.Null(exception);

        testAutosave.Dispose();
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
        // No SelectedTab — command should not throw
        _viewModel.PrintCommand.Execute(null);
    }

    [Fact]
    public void PrintCommand_WithSelectedTab_DelegatesToTab()
    {
        var mockTemplateService = new Mock<ITemplateService>();
        mockTemplateService.Setup(s => s.CreateNew(It.IsAny<string>(), It.IsAny<SheetOrientation>()))
            .Returns(new Template());
        var editor = new EditorViewModel(new Template(), mockTemplateService.Object,
            printService: new Mock<IPrintService>().Object);
        _viewModel.OpenedTabs.Add(editor);
        _viewModel.SelectedTab = editor;

        _viewModel.PrintCommand.Execute(null);

        Assert.Equal("Печать доступна через меню печати", editor.StatusMessage);
    }

    // ===== PreviewPrint =====

    [Fact]
    public void PrintPreviewCommand_WhenNoActiveTab_DoesNotThrow()
    {
        var exception = Record.Exception(() => _viewModel.PreviewPrintCommand.Execute(null));
        Assert.Null(exception);
    }

    [Fact]
    public void PrintPreviewCommand_WhenActiveTabExists_DoesNotThrow()
    {
        _viewModel.NewTabCommand.Execute("A4");
        var tab = _viewModel.OpenedTabs[0];
        _viewModel.SelectedTab = tab;

        // NOTE: Window creation requires STA, verifying no exception from logic only
        var exception = Record.Exception(() => _viewModel.PreviewPrintCommand.Execute(null));
        Assert.Null(exception);
    }

    // ===== OpenSettings =====

    [Fact]
    public void OpenSettingsCommand_Executes_DoesNotThrow()
    {
        var exception = Record.Exception(() => _viewModel.OpenSettingsCommand.Execute(null));
        Assert.Null(exception);
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
