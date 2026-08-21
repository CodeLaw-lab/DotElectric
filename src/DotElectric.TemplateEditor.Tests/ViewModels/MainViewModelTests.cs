using System.Collections.ObjectModel;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.ViewModels;
using DotElectric.TemplateEditor.ViewModels.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotElectric.TemplateEditor.Tests.ViewModels;

[Collection("AutosaveSharedState")]
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
    private readonly string _testAutosaveFolder;
    private readonly AutosaveService _autosaveService;
    private readonly MainViewModel _viewModel;

    public MainViewModelTests()
    {
        _mockTabOperations = new Mock<ITabOperationsService>();

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

        _testAutosaveFolder = Path.Combine(Path.GetTempPath(), $"MainViewModelAutosaveTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testAutosaveFolder);

        _autosaveService = new AutosaveService(
            new Mock<ITemplateService>().Object,
            _mockSettingsService.Object,
            logger: null,
            dispatcherService: null,
            dateTimeProvider: null,
            autosaveFolder: _testAutosaveFolder);

        // Setup default TabOperations behavior
        _mockTabOperations.Setup(t => t.CreateNewTab(It.IsAny<string?>(), It.IsAny<SheetOrientation?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns((string? fmt, SheetOrientation? _, string? _, string? _) =>
            {
                return new EditorViewModel(
                    new Template(),
                    printService: new Mock<IPrintService>().Object);
            });

        _mockTabOperations.Setup(t => t.CreateNewCustomTab(It.IsAny<double>(), It.IsAny<double>()))
            .Returns(new EditorViewModel(new Template(),
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
        if (Directory.Exists(_testAutosaveFolder))
            Directory.Delete(_testAutosaveFolder, true);
    }

    // ===== Локальные помощники создания вкладок =====

    private static NewSheetOrientationEntry MenuEntry(string format, SheetOrientation orientation = SheetOrientation.Landscape)
        => new(format, format, orientation);

    private void CreateTab(string format = "A4")
        => _viewModel.NewTabCommand.Execute(MenuEntry(format));

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
        CreateTab("A4");

        _mockTabOperations.Verify(t => t.CreateNewTab("A4", SheetOrientation.Landscape, null, "Landscape"), Times.Once);
        Assert.Single(_viewModel.OpenedTabs);
        Assert.NotNull(_viewModel.SelectedTab);
        Assert.Same(_viewModel.OpenedTabs[0], _viewModel.SelectedTab);
    }

    [Fact]
    public void NewTabCommand_WithNullFormat_UsesLastUsed()
    {
        _viewModel.NewTabCommand.Execute(null);

        Assert.Single(_viewModel.OpenedTabs);
        _mockSettingsService.Verify(s => s.Load(), Times.AtLeastOnce);
    }

    [Fact]
    public void NewTabCommand_SavesLastUsedFormat()
    {
        CreateTab("A4");

        _mockTabOperations.Verify(t => t.CreateNewTab("A4", SheetOrientation.Landscape, null, "Landscape"), Times.Once);
    }

    [Fact]
    public void NewTabCommand_MultipleTabs_AddsAll()
    {
        CreateTab("A4");
        CreateTab("A3");
        CreateTab("A2");

        Assert.Equal(3, _viewModel.OpenedTabs.Count);
    }

    [Fact]
    public void NewTabCommand_MultipleTabs_AllHaveUniqueIds()
    {
        CreateTab("A4");
        CreateTab("A3");
        CreateTab("A2");

        var ids = _viewModel.OpenedTabs.Select(t => t.TabId).Distinct().ToList();
        Assert.Equal(3, ids.Count);
    }

    // ===== NewTabWithLastFormat =====

    [Fact]
    public void NewTabWithLastFormatCommand_UsesSettings()
    {
        _viewModel.NewTabWithLastFormatCommand.Execute(null);

        _mockSettingsService.Verify(s => s.Load(), Times.AtLeastOnce);
        _mockTabOperations.Verify(t => t.CreateNewTab(null, null, "A3", "Landscape"), Times.Once);
        Assert.Single(_viewModel.OpenedTabs);
    }

    // ===== NewSheetMenu (модель меню «Файл > Новый шаблон») =====

    [Fact]
    public void NewSheetMenu_Structure_GroupsSeparatorsCustomInOrder()
    {
        var menu = _viewModel.NewSheetMenu;

        Assert.Equal(13, menu.Count);

        Assert.Equal(
            ["A0", "A1", "A2", "A3", "A4"],
            menu.Take(5).Select(e => e.Header).ToArray());
        Assert.All(menu.Take(5), e => Assert.Equal(NewSheetMenuKind.FormatGroup, e.Kind));

        Assert.Equal(NewSheetMenuKind.Separator, menu[5].Kind);

        Assert.Equal(
            ["A4×2", "A3×2", "A2×2", "A1×2", "A0×2"],
            menu.Skip(6).Take(5).Select(e => e.Header).ToArray());
        Assert.All(menu.Skip(6).Take(5), e => Assert.Equal(NewSheetMenuKind.FormatGroup, e.Kind));

        Assert.Equal(NewSheetMenuKind.Separator, menu[11].Kind);

        Assert.Equal(NewSheetMenuKind.CustomCommand, menu[12].Kind);
        Assert.Equal("Пользовательский...", menu[12].Header);
        Assert.Null(menu[12].Orientations);
    }

    [Fact]
    public void NewSheetMenu_OrientationHeadersAndFormats_ByteForByteWithPreviousMenu()
    {
        var groups = _viewModel.NewSheetMenu
            .Where(e => e.Kind == NewSheetMenuKind.FormatGroup)
            .Select(e => (e.Header, Orientations: e.Orientations!.Select(o => (o.Header, o.Format, o.Orientation)).ToArray()))
            .ToArray();

        var expected = new (string Header, (string Header, string Format, SheetOrientation Orientation)[] Orientations)[]
        {
            ("A0", new[] { ("Альбомная (1189×841)", "A0", SheetOrientation.Landscape), ("Книжная (841×1189)", "A0", SheetOrientation.Portrait) }),
            ("A1", new[] { ("Альбомная (841×594)", "A1", SheetOrientation.Landscape), ("Книжная (594×841)", "A1", SheetOrientation.Portrait) }),
            ("A2", new[] { ("Альбомная (594×420)", "A2", SheetOrientation.Landscape), ("Книжная (420×594)", "A2", SheetOrientation.Portrait) }),
            ("A3", new[] { ("Альбомная (420×297)", "A3", SheetOrientation.Landscape), ("Книжная (297×420)", "A3", SheetOrientation.Portrait) }),
            ("A4", new[] { ("Книжная (210×297)", "A4", SheetOrientation.Portrait), ("Альбомная (297×210)", "A4", SheetOrientation.Landscape) }),
            ("A4×2", new[] { ("Книжная (210×594)", "A4×2", SheetOrientation.Portrait), ("Альбомная (594×210)", "A4×2", SheetOrientation.Landscape) }),
            ("A3×2", new[] { ("Книжная (297×840)", "A3×2", SheetOrientation.Portrait), ("Альбомная (840×297)", "A3×2", SheetOrientation.Landscape) }),
            ("A2×2", new[] { ("Книжная (420×1188)", "A2×2", SheetOrientation.Portrait), ("Альбомная (1188×420)", "A2×2", SheetOrientation.Landscape) }),
            ("A1×2", new[] { ("Книжная (594×1682)", "A1×2", SheetOrientation.Portrait), ("Альбомная (1682×594)", "A1×2", SheetOrientation.Landscape) }),
            ("A0×2", new[] { ("Книжная (841×2378)", "A0×2", SheetOrientation.Portrait), ("Альбомная (2378×841)", "A0×2", SheetOrientation.Landscape) })
        };

        Assert.Equal(expected.Length, groups.Length);
        for (var i = 0; i < expected.Length; i++)
        {
            Assert.Equal(expected[i].Header, groups[i].Header);
            Assert.Equal(expected[i].Orientations, groups[i].Orientations);
        }
    }

    // ===== CloseTab =====

    [Fact]
    public async Task CloseTabCommand_RemovesTab()
    {
        _mockTabOperations.Setup(t => t.PromptAndSaveIfDirtyAsync(It.IsAny<EditorViewModel>()))
            .ReturnsAsync(true);

        CreateTab("A4");
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

        CreateTab("A4");
        var tab = _viewModel.OpenedTabs[0];

        await _viewModel.CloseTabCommand.ExecuteAsync(tab);

        Assert.Null(_viewModel.SelectedTab);
    }

    [Fact]
    public async Task CloseTabCommand_NonSelectedTab_KeepsSelectedTab()
    {
        _mockTabOperations.Setup(t => t.PromptAndSaveIfDirtyAsync(It.IsAny<EditorViewModel>()))
            .ReturnsAsync(true);

        CreateTab("A4");
        CreateTab("A3");
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

        CreateTab("A4");
        var tab = _viewModel.OpenedTabs[0];

        await _viewModel.CloseTabCommand.ExecuteAsync(tab);

        Assert.DoesNotContain(tab, _viewModel.OpenedTabs);
    }

    [Fact]
    public async Task CloseTabCommand_DirtyTabCancel_DoesNotClose()
    {
        _mockTabOperations.Setup(t => t.PromptAndSaveIfDirtyAsync(It.IsAny<EditorViewModel>()))
            .ReturnsAsync(false);

        CreateTab("A4");
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

        CreateTab("A4");
        CreateTab("A3");
        CreateTab("A2");

        await _viewModel.CloseAllTabsCommand.ExecuteAsync(null);

        Assert.Empty(_viewModel.OpenedTabs);
    }

    [Fact]
    public async Task CloseAllTabsCommand_NoTabs_DoesNothing()
    {
        await _viewModel.CloseAllTabsCommand.ExecuteAsync(null);
        Assert.Empty(_viewModel.OpenedTabs);
    }

    [Fact]
    public async Task CloseAllTabsCommand_DirtyTabCancel_KeepsOpenTabs()
    {
        _mockTabOperations.Setup(t => t.PromptAndSaveIfDirtyAsync(It.IsAny<EditorViewModel>()))
            .ReturnsAsync(false);

        CreateTab("A4");
        CreateTab("A3");

        await _viewModel.CloseAllTabsCommand.ExecuteAsync(null);

        Assert.Equal(2, _viewModel.OpenedTabs.Count);
    }

    [Fact]
    public async Task CloseAllTabsCommand_CallsPromptForEachTab()
    {
        _mockTabOperations.Setup(t => t.PromptAndSaveIfDirtyAsync(It.IsAny<EditorViewModel>()))
            .ReturnsAsync(true);

        CreateTab("A4");
        CreateTab("A3");
        CreateTab("A2");

        await _viewModel.CloseAllTabsCommand.ExecuteAsync(null);

        _mockTabOperations.Verify(t => t.PromptAndSaveIfDirtyAsync(It.IsAny<EditorViewModel>()), Times.Exactly(3));
    }

    // ===== CloseOtherTabs =====

    [Fact]
    public async Task CloseOtherTabsCommand_KeepsSpecifiedTab()
    {
        _mockTabOperations.Setup(t => t.PromptAndSaveIfDirtyAsync(It.IsAny<EditorViewModel>()))
            .ReturnsAsync(true);

        CreateTab("A4");
        CreateTab("A3");
        CreateTab("A2");
        var keepTab = _viewModel.OpenedTabs[1];

        await _viewModel.CloseOtherTabsCommand.ExecuteAsync(keepTab);

        Assert.Single(_viewModel.OpenedTabs);
        Assert.Same(keepTab, _viewModel.OpenedTabs[0]);
    }

    [Fact]
    public async Task CloseOtherTabsCommand_NullTab_DoesNothing()
    {
        CreateTab("A4");
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
        CreateTab("A4");

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

    [Fact]
    public async Task SaveAllCommand_WithTabs_SavesAllTabs()
    {
        CreateTab("A4");
        CreateTab("A3");

        await _viewModel.SaveAllCommand.ExecuteAsync(null);

        _mockTabOperations.Verify(t => t.SaveTabAsync(It.IsAny<EditorViewModel>()), Times.Exactly(2));
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
        mockTemplateService.Setup(s => s.Save(It.IsAny<Template>(), It.IsAny<string>()))
            .Throws(new InvalidOperationException("Autosave failed"));

        _mockTabOperations.Setup(t => t.CreateNewTab(It.IsAny<string?>(), It.IsAny<SheetOrientation?>(), It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns((string? fmt, SheetOrientation? _, string? _, string? _) =>
                new EditorViewModel(new Template(),
                    printService: new Mock<IPrintService>().Object));

        CreateTab("A4");
        var tab = _viewModel.OpenedTabs[0];
        tab.MarkDirty();

        // Set a file path so autosave doesn't prompt for dialog
        tab.DirtyStateManager.FilePath = "test.tdel";

        // Create a second autosave service that won't try to create backups etc.
        var autosaveFolder = Path.Combine(Path.GetTempPath(), $"MainViewModelAutosaveTest2_{Guid.NewGuid():N}");
        Directory.CreateDirectory(autosaveFolder);
        var testAutosave = new AutosaveService(
            mockTemplateService.Object,
            _mockSettingsService.Object,
            logger: null,
            dispatcherService: null,
            dateTimeProvider: null,
            autosaveFolder: autosaveFolder);

        var exception = await Record.ExceptionAsync(
            () => testAutosave.AutosaveAllTabsAsync(_viewModel.OpenedTabs, TestContext.Current.CancellationToken));
        Assert.Null(exception);

        testAutosave.Dispose();
        if (Directory.Exists(autosaveFolder))
            Directory.Delete(autosaveFolder, true);
    }

    // ===== ToggleTheme =====

    [Fact]
    public void ToggleThemeCommand_ChangesTheme()
    {
        _viewModel.ToggleThemeCommand.Execute(null);

        Assert.Equal("Dark", _viewModel.Theme);
        _mockThemeService.Verify(t => t.ToggleTheme(), Times.Once);
    }

    [Fact]
    public void ToggleThemeCommand_RaisesPropertyChanged()
    {
        var changed = false;
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.Theme)) changed = true;
        };

        _viewModel.ToggleThemeCommand.Execute(null);

        Assert.True(changed);
    }

    // ===== NewTab с пунктом меню =====

    [Fact]
    public void NewTabCommand_WithMenuEntry_PassesFormatAndOrientation()
    {
        _viewModel.NewTabCommand.Execute(MenuEntry("A4", SheetOrientation.Portrait));

        _mockTabOperations.Verify(t => t.CreateNewTab("A4", SheetOrientation.Portrait, null, "Landscape"), Times.Once);
        Assert.Single(_viewModel.OpenedTabs);
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
        var editor = new EditorViewModel(new Template(),
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
        _mockDialogHostService.Verify(
            d => d.ShowDialog(It.IsAny<object>(), It.IsAny<object?>()), Times.Never);
    }

    [Fact]
    public void PrintPreviewCommand_WhenActiveTabExists_ShowsDialogWithPrintPreviewViewModel()
    {
        CreateTab("A4");
        var tab = _viewModel.OpenedTabs[0];
        _viewModel.SelectedTab = tab;

        _viewModel.PreviewPrintCommand.Execute(null);

        _mockDialogHostService.Verify(
            d => d.ShowDialog(
                It.Is<PrintPreviewViewModel>(vm => vm.DisplayName == tab.DirtyStateManager.DisplayName),
                It.IsAny<object?>()),
            Times.Once);
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
        CreateTab("A4");
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
        CreateTab("A4");
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
