using System.Reflection;
using CommunityToolkit.Mvvm.Messaging;
using DotElectric.TemplateEditor.Messages;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.ViewModels;
using DotElectric.TemplateEditor.ViewModels.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotElectric.TemplateEditor.Tests.ViewModels;

[Collection("AutosaveSharedState")]
public class MainViewModelAsyncFlowTests : IDisposable
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
    private readonly string _testAutosaveFolder;
    private readonly AutosaveService _autosaveService;
    private readonly MainViewModel _viewModel;

    public MainViewModelAsyncFlowTests()
    {
        _mockTabOperations = new Mock<ITabOperationsService>();
        _mockTabOperations.Setup(t => t.CreateNewTab(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns(CreateEditor);
        _mockTabOperations.Setup(t => t.CreateNewCustomTab(It.IsAny<double>(), It.IsAny<double>()))
            .Returns(() => CreateEditor());
        _mockTabOperations.Setup(t => t.PromptAndSaveIfDirtyAsync(It.IsAny<EditorViewModel>()))
            .ReturnsAsync(true);

        _mockSettingsService = new Mock<ISettingsService>();
        _mockSettingsService.Setup(s => s.Load()).Returns(new AppSettings());

        _mockThemeService = new Mock<IThemeService>();
        _mockThemeService.Setup(t => t.ToggleTheme()).Returns("Dark");

        _mockTemplateLibraryService = new Mock<ITemplateLibraryService>();
        _mockDialogService = new Mock<IDialogService>();
        _mockDialogHostService = new Mock<IDialogHostService>();
        _mockApplicationLifecycle = new Mock<IApplicationLifecycle>();
        _mockLogger = new Mock<ILogger<MainViewModel>>();
        _mockPrintDocumentGenerator = new Mock<IPrintDocumentGenerator>();

        _testAutosaveFolder = Path.Combine(Path.GetTempPath(), $"MainViewModelAsyncTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testAutosaveFolder);

        _autosaveService = new AutosaveService(
            new Mock<ITemplateService>().Object,
            _mockSettingsService.Object,
            logger: null,
            dispatcherService: null,
            dateTimeProvider: null,
            autosaveFolder: _testAutosaveFolder);

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
        if (Directory.Exists(_testAutosaveFolder))
            Directory.Delete(_testAutosaveFolder, true);
    }

    // === Helpers ===

    private static EditorViewModel CreateEditor()
    {
        return new EditorViewModel(new Template(),
            printService: new Mock<IPrintService>().Object);
    }

    private MainViewModel CreateViewModel(AutosaveService autosaveService) => new(
        _mockTabOperations.Object,
        _mockSettingsService.Object,
        _mockThemeService.Object,
        _mockTemplateLibraryService.Object,
        _mockDialogService.Object,
        _mockDialogHostService.Object,
        _mockApplicationLifecycle.Object,
        _mockLogger.Object,
        autosaveService,
        _mockPrintDocumentGenerator.Object);

    private void ConfigureDialog(Action<CustomSheetDialogViewModel>? configure, bool? result)
    {
        _mockDialogHostService.Setup(d => d.ShowDialog(It.IsAny<object>(), It.IsAny<object?>()))
            .Callback((object vm, object? _) =>
            {
                if (vm is CustomSheetDialogViewModel custom)
                    configure?.Invoke(custom);
            })
            .Returns(result);
    }

    private static bool IsDisposed(EditorViewModel tab)
    {
        var field = typeof(EditorViewModel).GetField("_isDisposed", BindingFlags.NonPublic | BindingFlags.Instance);
        return (bool)field!.GetValue(tab)!;
    }

    /// <summary>
    /// Invokes the AutosaveTick event from outside AutosaveService (field-like event — invoke
    /// allowed only inside the declaring class). The backing delegate is named after the event.
    /// Returns the Task of the last (only) subscriber — MainViewModel.OnAutosaveTickHandler.
    /// </summary>
    private static Task? FireAutosaveTick(AutosaveService autosaveService)
    {
        var field = typeof(AutosaveService).GetField("AutosaveTick", BindingFlags.NonPublic | BindingFlags.Instance);
        var handler = field!.GetValue(autosaveService) as Func<Task>;
        return handler?.Invoke();
    }

    private static void VerifyLogErrorOnce(Mock<ILogger<MainViewModel>> logger)
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

    // ===== Constructor null-guards =====

    [Fact]
    public void Ctor_NullTabOperations_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(
            null!, _mockSettingsService.Object, _mockThemeService.Object, _mockTemplateLibraryService.Object,
            _mockDialogService.Object, _mockDialogHostService.Object, _mockApplicationLifecycle.Object,
            _mockLogger.Object, _autosaveService, _mockPrintDocumentGenerator.Object));
    }

    [Fact]
    public void Ctor_NullSettingsService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(
            _mockTabOperations.Object, null!, _mockThemeService.Object, _mockTemplateLibraryService.Object,
            _mockDialogService.Object, _mockDialogHostService.Object, _mockApplicationLifecycle.Object,
            _mockLogger.Object, _autosaveService, _mockPrintDocumentGenerator.Object));
    }

    [Fact]
    public void Ctor_NullThemeService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(
            _mockTabOperations.Object, _mockSettingsService.Object, null!, _mockTemplateLibraryService.Object,
            _mockDialogService.Object, _mockDialogHostService.Object, _mockApplicationLifecycle.Object,
            _mockLogger.Object, _autosaveService, _mockPrintDocumentGenerator.Object));
    }

    [Fact]
    public void Ctor_NullTemplateLibraryService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(
            _mockTabOperations.Object, _mockSettingsService.Object, _mockThemeService.Object, null!,
            _mockDialogService.Object, _mockDialogHostService.Object, _mockApplicationLifecycle.Object,
            _mockLogger.Object, _autosaveService, _mockPrintDocumentGenerator.Object));
    }

    [Fact]
    public void Ctor_NullDialogService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(
            _mockTabOperations.Object, _mockSettingsService.Object, _mockThemeService.Object, _mockTemplateLibraryService.Object,
            null!, _mockDialogHostService.Object, _mockApplicationLifecycle.Object,
            _mockLogger.Object, _autosaveService, _mockPrintDocumentGenerator.Object));
    }

    [Fact]
    public void Ctor_NullDialogHostService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(
            _mockTabOperations.Object, _mockSettingsService.Object, _mockThemeService.Object, _mockTemplateLibraryService.Object,
            _mockDialogService.Object, null!, _mockApplicationLifecycle.Object,
            _mockLogger.Object, _autosaveService, _mockPrintDocumentGenerator.Object));
    }

    [Fact]
    public void Ctor_NullApplicationLifecycle_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(
            _mockTabOperations.Object, _mockSettingsService.Object, _mockThemeService.Object, _mockTemplateLibraryService.Object,
            _mockDialogService.Object, _mockDialogHostService.Object, null!,
            _mockLogger.Object, _autosaveService, _mockPrintDocumentGenerator.Object));
    }

    [Fact]
    public void Ctor_NullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(
            _mockTabOperations.Object, _mockSettingsService.Object, _mockThemeService.Object, _mockTemplateLibraryService.Object,
            _mockDialogService.Object, _mockDialogHostService.Object, _mockApplicationLifecycle.Object,
            null!, _autosaveService, _mockPrintDocumentGenerator.Object));
    }

    [Fact]
    public void Ctor_NullAutosaveService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(
            _mockTabOperations.Object, _mockSettingsService.Object, _mockThemeService.Object, _mockTemplateLibraryService.Object,
            _mockDialogService.Object, _mockDialogHostService.Object, _mockApplicationLifecycle.Object,
            _mockLogger.Object, null!, _mockPrintDocumentGenerator.Object));
    }

    [Fact]
    public void Ctor_NullPrintDocumentGenerator_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MainViewModel(
            _mockTabOperations.Object, _mockSettingsService.Object, _mockThemeService.Object, _mockTemplateLibraryService.Object,
            _mockDialogService.Object, _mockDialogHostService.Object, _mockApplicationLifecycle.Object,
            _mockLogger.Object, _autosaveService, null!));
    }

    // ===== NewCustomTabAsync =====

    [Fact]
    public async Task NewCustomTabCommand_DialogOk_ValidDims_CreatesAndSelectsTab()
    {
        ConfigureDialog(c => { c.WidthMm = 500; c.HeightMm = 700; }, true);

        await _viewModel.NewCustomTabCommand.ExecuteAsync(null);

        _mockTabOperations.Verify(t => t.CreateNewCustomTab(500, 700), Times.Once);
        Assert.Single(_viewModel.OpenedTabs);
        Assert.Same(_viewModel.OpenedTabs[0], _viewModel.SelectedTab);
    }

    [Fact]
    public async Task NewCustomTabCommand_DialogOk_ZeroWidth_NoTabCreated()
    {
        ConfigureDialog(c => { c.WidthMm = 0; c.HeightMm = 700; }, true);

        await _viewModel.NewCustomTabCommand.ExecuteAsync(null);

        _mockTabOperations.Verify(t => t.CreateNewCustomTab(It.IsAny<double>(), It.IsAny<double>()), Times.Never);
        Assert.Empty(_viewModel.OpenedTabs);
    }

    [Fact]
    public async Task NewCustomTabCommand_DialogOk_NonPositiveHeight_NoTabCreated()
    {
        ConfigureDialog(c => { c.WidthMm = 500; c.HeightMm = -1; }, true);

        await _viewModel.NewCustomTabCommand.ExecuteAsync(null);

        _mockTabOperations.Verify(t => t.CreateNewCustomTab(It.IsAny<double>(), It.IsAny<double>()), Times.Never);
        Assert.Empty(_viewModel.OpenedTabs);
    }

    [Fact]
    public async Task NewCustomTabCommand_DialogCancel_NoTabCreated()
    {
        ConfigureDialog(null, false);

        await _viewModel.NewCustomTabCommand.ExecuteAsync(null);

        _mockTabOperations.Verify(t => t.CreateNewCustomTab(It.IsAny<double>(), It.IsAny<double>()), Times.Never);
        Assert.Empty(_viewModel.OpenedTabs);
    }

    [Fact]
    public async Task NewCustomTabCommand_DialogNull_NoTabCreated()
    {
        ConfigureDialog(null, null);

        await _viewModel.NewCustomTabCommand.ExecuteAsync(null);

        _mockTabOperations.Verify(t => t.CreateNewCustomTab(It.IsAny<double>(), It.IsAny<double>()), Times.Never);
        Assert.Empty(_viewModel.OpenedTabs);
    }

    // ===== OpenFileAsync =====

    [Fact]
    public async Task OpenFileCommand_ReturnsEditor_AddsAndSelectsTab()
    {
        var editor = CreateEditor();
        _mockTabOperations.Setup(t => t.OpenFileAsync()).ReturnsAsync(editor);

        await _viewModel.OpenFileCommand.ExecuteAsync(null);

        Assert.Single(_viewModel.OpenedTabs);
        Assert.Same(editor, _viewModel.SelectedTab);
    }

    [Fact]
    public async Task OpenFileCommand_ReturnsNull_AddsNothing()
    {
        _mockTabOperations.Setup(t => t.OpenFileAsync()).ReturnsAsync((EditorViewModel)null!);

        await _viewModel.OpenFileCommand.ExecuteAsync(null);

        Assert.Empty(_viewModel.OpenedTabs);
        Assert.Null(_viewModel.SelectedTab);
    }

    // ===== SaveAsync / SaveAsAsync =====

    [Fact]
    public async Task SaveAsCommand_WithSelectedTab_DelegatesToService()
    {
        _viewModel.NewTabCommand.Execute("A4");
        var tab = _viewModel.OpenedTabs[0];

        await _viewModel.SaveAsCommand.ExecuteAsync(null);

        _mockTabOperations.Verify(t => t.SaveAsAsync(tab), Times.Once);
    }

    [Fact]
    public async Task SaveCommand_WithSelectedTab_DelegatesWithExactTab()
    {
        _viewModel.NewTabCommand.Execute("A4");
        var tab = _viewModel.OpenedTabs[0];

        await _viewModel.SaveCommand.ExecuteAsync(null);

        _mockTabOperations.Verify(t => t.SaveTabAsync(tab), Times.Once);
    }

    // ===== OnAutosaveTickHandler =====

    [Fact]
    public async Task AutosaveTick_InvokesAutosaveAllTabsWithOpenedTabs()
    {
        var autosaveTemplateService = new Mock<ITemplateService>();
        var folder = Path.Combine(Path.GetTempPath(), $"MainViewModelAutosaveTick_{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        var autosave = new AutosaveService(
            autosaveTemplateService.Object,
            _mockSettingsService.Object,
            logger: null,
            dispatcherService: null,
            dateTimeProvider: null,
            autosaveFolder: folder);
        var vm = CreateViewModel(autosave);

        try
        {
            vm.NewTabCommand.Execute("A4");
            var tab = vm.OpenedTabs[0];
            tab.MarkDirty();
            tab.DirtyStateManager.FilePath = "test.tdel";

            var tickTask = FireAutosaveTick(autosave);
            if (tickTask != null)
                await tickTask;

            autosaveTemplateService.Verify(
                s => s.Save(It.IsAny<Template>(), It.Is<string>(p => p.StartsWith(folder, StringComparison.OrdinalIgnoreCase))),
                Times.Once);
        }
        finally
        {
            vm.Dispose();
            autosave.Dispose();
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }
    }

    [Fact]
    public async Task AutosaveTick_AutosaveThrows_LogsError()
    {
        // Remove the autosave folder so SaveSession (File.WriteAllText) throws mid-flight
        Directory.Delete(_testAutosaveFolder, true);

        _viewModel.NewTabCommand.Execute("A4");
        var tab = _viewModel.OpenedTabs[0];
        tab.MarkDirty();
        tab.DirtyStateManager.FilePath = "test.tdel";

        var tickTask = FireAutosaveTick(_autosaveService);
        if (tickTask != null)
            await tickTask;

        VerifyLogErrorOnce(_mockLogger);
    }

    // ===== CloseTab =====

    [Fact]
    public async Task CloseTabCommand_SelectedTabWithOthers_SwitchesToLastRemaining()
    {
        _viewModel.NewTabCommand.Execute("A4");
        _viewModel.NewTabCommand.Execute("A3");
        _viewModel.NewTabCommand.Execute("A2");
        var first = _viewModel.OpenedTabs[0];
        _viewModel.SelectedTab = first;

        await _viewModel.CloseTabCommand.ExecuteAsync(first);

        Assert.Equal(2, _viewModel.OpenedTabs.Count);
        Assert.Same(_viewModel.OpenedTabs[_viewModel.OpenedTabs.Count - 1], _viewModel.SelectedTab);
    }

    [Fact]
    public async Task CloseTabCommand_Cancel_DoesNotDisposeTab()
    {
        _mockTabOperations.Setup(t => t.PromptAndSaveIfDirtyAsync(It.IsAny<EditorViewModel>()))
            .ReturnsAsync(false);
        _viewModel.NewTabCommand.Execute("A4");
        var tab = _viewModel.OpenedTabs[0];

        await _viewModel.CloseTabCommand.ExecuteAsync(tab);

        Assert.Single(_viewModel.OpenedTabs);
        Assert.False(IsDisposed(tab));
    }

    [Fact]
    public async Task CloseTabCommand_Confirm_DisposesTab()
    {
        _viewModel.NewTabCommand.Execute("A4");
        var tab = _viewModel.OpenedTabs[0];

        await _viewModel.CloseTabCommand.ExecuteAsync(tab);

        Assert.Empty(_viewModel.OpenedTabs);
        Assert.True(IsDisposed(tab));
    }

    // ===== CloseAllTabsAsync =====

    [Fact]
    public async Task CloseAllTabsCommand_ClosesAllTabs_DisposesEach()
    {
        _viewModel.NewTabCommand.Execute("A4");
        _viewModel.NewTabCommand.Execute("A3");
        var tabs = _viewModel.OpenedTabs.ToList();

        await _viewModel.CloseAllTabsCommand.ExecuteAsync(null);

        Assert.Empty(_viewModel.OpenedTabs);
        Assert.True(IsDisposed(tabs[0]));
        Assert.True(IsDisposed(tabs[1]));
    }

    // ===== CloseOtherTabs =====

    [Fact]
    public async Task CloseOtherTabsCommand_ClosesOthersInOrder_KeepsTarget()
    {
        var closed = new List<EditorViewModel>();
        _mockTabOperations.Setup(t => t.PromptAndSaveIfDirtyAsync(It.IsAny<EditorViewModel>()))
            .ReturnsAsync(true)
            .Callback((EditorViewModel tab) => closed.Add(tab));

        _viewModel.NewTabCommand.Execute("A4");
        _viewModel.NewTabCommand.Execute("A3");
        _viewModel.NewTabCommand.Execute("A2");
        var t0 = _viewModel.OpenedTabs[0];
        var keep = _viewModel.OpenedTabs[1];
        var t2 = _viewModel.OpenedTabs[2];

        await _viewModel.CloseOtherTabsCommand.ExecuteAsync(keep);

        Assert.Equal(new[] { t0, t2 }, closed);
        Assert.Single(_viewModel.OpenedTabs);
        Assert.Same(keep, _viewModel.OpenedTabs[0]);
        _mockTabOperations.Verify(t => t.PromptAndSaveIfDirtyAsync(keep), Times.Never);
    }

    // ===== OnTemplateDoubleClicked =====

    [Fact]
    public void OnTemplateDoubleClicked_OpenReturnsEditor_AddsAndSelectsTab()
    {
        var info = new TemplateInfo("a.tdel", "A", @"C:\templates\a.tdel");
        var editor = CreateEditor();
        _mockTabOperations.Setup(t => t.OpenFromFilePath(info.FullPath)).Returns(editor);

        _viewModel.TemplateLibraryVm.OpenTemplateCommand.Execute(info);

        Assert.Single(_viewModel.OpenedTabs);
        Assert.Same(editor, _viewModel.SelectedTab);
    }

    [Fact]
    public void OnTemplateDoubleClicked_OpenReturnsNull_AddsNothing()
    {
        var info = new TemplateInfo("a.tdel", "A", @"C:\templates\a.tdel");
        _mockTabOperations.Setup(t => t.OpenFromFilePath(It.IsAny<string>())).Returns((EditorViewModel)null!);

        _viewModel.TemplateLibraryVm.OpenTemplateCommand.Execute(info);

        Assert.Empty(_viewModel.OpenedTabs);
        Assert.Null(_viewModel.SelectedTab);
    }

    // ===== OpenSettings =====

    [Fact]
    public void OpenSettingsCommand_ShowsSettingsDialog()
    {
        _viewModel.OpenSettingsCommand.Execute(null);

        _mockDialogHostService.Verify(
            d => d.ShowDialog(It.IsAny<SettingsViewModel>(), It.IsAny<object?>()), Times.Once);
    }

    // ===== PreviewPrint =====

    [Fact]
    public void PreviewPrintCommand_GenerateThrows_LogsErrorAndShowsError()
    {
        _viewModel.NewTabCommand.Execute("A4");
        _viewModel.SelectedTab = _viewModel.OpenedTabs[0];
        _mockPrintDocumentGenerator.Setup(g => g.Generate(It.IsAny<Template>()))
            .Throws(new InvalidOperationException("boom"));

        var exception = Record.Exception(() => _viewModel.PreviewPrintCommand.Execute(null));

        Assert.Null(exception);
        VerifyLogErrorOnce(_mockLogger);
        _mockDialogService.Verify(d => d.ShowError(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void PreviewPrintCommand_NoSelectedTab_DoesNotGenerate()
    {
        _viewModel.PreviewPrintCommand.Execute(null);

        _mockPrintDocumentGenerator.Verify(g => g.Generate(It.IsAny<Template>()), Times.Never);
    }

    // ===== Messenger-driven close handlers =====

    [Fact]
    public async Task CloseOtherTabsRequestMessage_ClosesOtherTabs_KeepsTarget()
    {
        _viewModel.NewTabCommand.Execute("A4");
        _viewModel.NewTabCommand.Execute("A3");
        _viewModel.NewTabCommand.Execute("A2");
        var keep = _viewModel.OpenedTabs[1];

        WeakReferenceMessenger.Default.Send(new CloseOtherTabsRequestMessage(keep));
        await Task.Yield();

        Assert.Single(_viewModel.OpenedTabs);
        Assert.Same(keep, _viewModel.OpenedTabs[0]);
    }

    [Fact]
    public async Task CloseAllTabsRequestMessage_ClosesAllTabs()
    {
        _viewModel.NewTabCommand.Execute("A4");
        _viewModel.NewTabCommand.Execute("A3");

        WeakReferenceMessenger.Default.Send(new CloseAllTabsRequestMessage());
        await Task.Yield();

        Assert.Empty(_viewModel.OpenedTabs);
    }

    // ===== Dispose =====

    [Fact]
    public void Dispose_AfterDispose_MessengerCloseMessage_DoesNothing()
    {
        _viewModel.NewTabCommand.Execute("A4");
        var tab = _viewModel.OpenedTabs[0];
        _viewModel.Dispose();

        var exception = Record.Exception(() => WeakReferenceMessenger.Default.Send(new CloseTabRequestMessage(tab)));

        Assert.Null(exception);
        Assert.Single(_viewModel.OpenedTabs);
        _mockTabOperations.Verify(t => t.PromptAndSaveIfDirtyAsync(It.IsAny<EditorViewModel>()), Times.Never);
    }

    [Fact]
    public async Task Dispose_AfterDispose_AutosaveTick_NotHandled()
    {
        var autosaveTemplateService = new Mock<ITemplateService>();
        var folder = Path.Combine(Path.GetTempPath(), $"MainViewModelAutosaveDispose_{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        var autosave = new AutosaveService(
            autosaveTemplateService.Object,
            _mockSettingsService.Object,
            logger: null,
            dispatcherService: null,
            dateTimeProvider: null,
            autosaveFolder: folder);
        var vm = CreateViewModel(autosave);

        try
        {
            vm.NewTabCommand.Execute("A4");
            var tab = vm.OpenedTabs[0];
            tab.MarkDirty();
            vm.Dispose();

            var tickTask = FireAutosaveTick(autosave);

            Assert.Null(tickTask);
            autosaveTemplateService.Verify(s => s.Save(It.IsAny<Template>(), It.IsAny<string>()), Times.Never);
        }
        finally
        {
            vm.Dispose();
            autosave.Dispose();
            if (Directory.Exists(folder))
                Directory.Delete(folder, true);
        }
    }
}
