using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Services;

/// <summary>
/// Unit tests for TabOperationsService (NewTab, OpenFile, Save, SaveAs, CustomTab, PromptAndSave, ParseSheetFormat).
/// </summary>
public class TabOperationsServiceTests
{
    private readonly Mock<ITemplateService> _templateServiceMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly Mock<ISettingsService> _settingsServiceMock;
    private readonly Mock<IPrintService> _printServiceMock;
    private readonly Mock<IEditorViewModelFactory> _factoryMock;
    private readonly Mock<ILogger<TabOperationsService>> _loggerMock;
    private readonly TabOperationsService _service;

    public TabOperationsServiceTests()
    {
        _templateServiceMock = new Mock<ITemplateService>();
        _templateServiceMock.Setup(s => s.Validate(It.IsAny<Template>())).Returns(Enumerable.Empty<string>());
        _fileServiceMock = new Mock<IFileService>();
        _dialogServiceMock = new Mock<IDialogService>();
        _settingsServiceMock = new Mock<ISettingsService>();
        _printServiceMock = new Mock<IPrintService>();
        _factoryMock = new Mock<IEditorViewModelFactory>();
        _loggerMock = new Mock<ILogger<TabOperationsService>>();
        _service = new TabOperationsService(
            _templateServiceMock.Object,
            _fileServiceMock.Object,
            _dialogServiceMock.Object,
            _settingsServiceMock.Object,
            _printServiceMock.Object,
            _factoryMock.Object,
            _loggerMock.Object);
    }

    private static Template CreateTemplate(string format = "A3", SheetOrientation orientation = SheetOrientation.Landscape)
        => new(new Metadata(), Sheet.FromFormat(format, orientation));

    private static EditorViewModel CreateEditor(Template template, string? filePath = null)
    {
        var templateService = new Mock<ITemplateService>().Object;
        var printService = new Mock<IPrintService>().Object;
        return filePath == null
            ? new EditorViewModel(template, templateService, printService: printService)
            : new EditorViewModel(template, filePath, templateService, printService: printService);
    }

    private static void VerifyLogErrorOnce(Mock<ILogger<TabOperationsService>> logger)
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

    // === Constructor ===

    [Fact]
    public void Constructor_NullTemplateService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TabOperationsService(
            null!, _fileServiceMock.Object, _dialogServiceMock.Object, _settingsServiceMock.Object,
            _printServiceMock.Object, _factoryMock.Object, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_NullFileService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TabOperationsService(
            _templateServiceMock.Object, null!, _dialogServiceMock.Object, _settingsServiceMock.Object,
            _printServiceMock.Object, _factoryMock.Object, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_NullDialogService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TabOperationsService(
            _templateServiceMock.Object, _fileServiceMock.Object, null!, _settingsServiceMock.Object,
            _printServiceMock.Object, _factoryMock.Object, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_NullSettingsService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TabOperationsService(
            _templateServiceMock.Object, _fileServiceMock.Object, _dialogServiceMock.Object, null!,
            _printServiceMock.Object, _factoryMock.Object, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_NullPrintService_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TabOperationsService(
            _templateServiceMock.Object, _fileServiceMock.Object, _dialogServiceMock.Object, _settingsServiceMock.Object,
            null!, _factoryMock.Object, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_NullFactory_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TabOperationsService(
            _templateServiceMock.Object, _fileServiceMock.Object, _dialogServiceMock.Object, _settingsServiceMock.Object,
            _printServiceMock.Object, null!, _loggerMock.Object));
    }

    [Fact]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new TabOperationsService(
            _templateServiceMock.Object, _fileServiceMock.Object, _dialogServiceMock.Object, _settingsServiceMock.Object,
            _printServiceMock.Object, _factoryMock.Object, null!));
    }

    // === CreateNewTab ===

    [Fact]
    public void CreateNewTab_FormatNull_UsesLastUsedFormatWithOrientation()
    {
        var template = CreateTemplate("A4", SheetOrientation.Landscape);
        using var editor = CreateEditor(template);
        _templateServiceMock.Setup(s => s.CreateNew("A4", SheetOrientation.Landscape)).Returns(template);
        _factoryMock.Setup(f => f.Create(template, null, _printServiceMock.Object, null, null)).Returns(editor);

        var result = _service.CreateNewTab(null, "A4L", "Landscape");

        Assert.Same(editor, result);
        _templateServiceMock.Verify(s => s.CreateNew("A4", SheetOrientation.Landscape), Times.Once);
        _settingsServiceMock.Verify(s => s.Set("LastUsedSheetFormat", "A4"), Times.Once);
        _settingsServiceMock.Verify(s => s.Set("LastUsedSheetOrientation", "Landscape"), Times.Once);
        _factoryMock.Verify(f => f.Create(template, null, _printServiceMock.Object, null, null), Times.Once);
    }

    [Fact]
    public void CreateNewTab_ExplicitFormat_WinsOverLastUsed()
    {
        _service.CreateNewTab("A3P", "A4L", "Landscape");

        _templateServiceMock.Verify(s => s.CreateNew("A3", SheetOrientation.Portrait), Times.Once);
        _settingsServiceMock.Verify(s => s.Set("LastUsedSheetFormat", "A3"), Times.Once);
        _settingsServiceMock.Verify(s => s.Set("LastUsedSheetOrientation", "Portrait"), Times.Once);
    }

    [Fact]
    public void CreateNewTab_FormatWithoutSuffix_UsesLastUsedOrientation()
    {
        _service.CreateNewTab(null, "A3", "Portrait");

        _templateServiceMock.Verify(s => s.CreateNew("A3", SheetOrientation.Portrait), Times.Once);
    }

    [Fact]
    public void CreateNewTab_InvalidLastUsedOrientation_FallsBackToDefaultOrientation()
    {
        _service.CreateNewTab("A3", null, "Bogus");

        _templateServiceMock.Verify(
            s => s.CreateNew("A3", SheetOrientation.Landscape),
            Times.Once);
    }

    [Fact]
    public void CreateNewTab_FormatWithoutSuffix_LastUsedOrientationNull_UsesLandscapeFallback()
    {
        _service.CreateNewTab("A4", null, null);

        _templateServiceMock.Verify(s => s.CreateNew("A4", SheetOrientation.Landscape), Times.Once);
    }

    [Fact]
    public void CreateNewTab_AllNull_UsesDefaultFormatAndLandscape()
    {
        _service.CreateNewTab(null, null, null);

        _templateServiceMock.Verify(s => s.CreateNew("A3", SheetOrientation.Landscape), Times.Once);
        _settingsServiceMock.Verify(s => s.Set("LastUsedSheetFormat", "A3"), Times.Once);
        _settingsServiceMock.Verify(s => s.Set("LastUsedSheetOrientation", "Landscape"), Times.Once);
    }

    // === OpenFileAsync ===

    [Fact]
    public async Task OpenFileAsync_DialogReturnsPath_LoadsAndCreatesEditor()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"tabops_open_{Guid.NewGuid():N}.tdel");
        var template = CreateTemplate();
        using var editor = CreateEditor(template);
        _fileServiceMock.Setup(f => f.OpenFileDialog("DotElectric Template|*.tdel")).Returns(filePath);
        _templateServiceMock.Setup(s => s.Load(filePath)).Returns(template);
        _factoryMock.Setup(f => f.CreateWithFilePath(template, filePath, null, _printServiceMock.Object, null, null)).Returns(editor);

        var result = await _service.OpenFileAsync();

        Assert.Same(editor, result);
        _templateServiceMock.Verify(s => s.Load(filePath), Times.Once);
        _factoryMock.Verify(f => f.CreateWithFilePath(template, filePath, null, _printServiceMock.Object, null, null), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task OpenFileAsync_DialogEmpty_ReturnsNullWithoutLoading(string? dialogResult)
    {
        _fileServiceMock.Setup(f => f.OpenFileDialog(It.IsAny<string>())).Returns(dialogResult);

        var result = await _service.OpenFileAsync();

        Assert.Null(result);
        _templateServiceMock.Verify(s => s.Load(It.IsAny<string>()), Times.Never);
        _factoryMock.Verify(f => f.CreateWithFilePath(It.IsAny<Template>(), It.IsAny<string>(), null, It.IsAny<IPrintService?>(), null, null), Times.Never);
    }

    [Fact]
    public async Task OpenFileAsync_LoadThrows_ShowsErrorAndLogs()
    {
        _fileServiceMock.Setup(f => f.OpenFileDialog(It.IsAny<string>())).Returns("C:\\missing\\file.tdel");
        _templateServiceMock.Setup(s => s.Load(It.IsAny<string>())).Throws(new InvalidOperationException("open boom"));

        var result = await _service.OpenFileAsync();

        Assert.Null(result);
        _dialogServiceMock.Verify(d => d.ShowError(It.Is<string>(m => m.Contains("open boom"))), Times.Once);
        VerifyLogErrorOnce(_loggerMock);
    }

    [Fact]
    public async Task OpenFileAsync_FactoryReturnsNull_PassesThrough()
    {
        var template = CreateTemplate();
        _fileServiceMock.Setup(f => f.OpenFileDialog(It.IsAny<string>())).Returns("C:\\file.tdel");
        _templateServiceMock.Setup(s => s.Load("C:\\file.tdel")).Returns(template);
        _factoryMock.Setup(f => f.CreateWithFilePath(template, "C:\\file.tdel", null, _printServiceMock.Object, null, null)).Returns((EditorViewModel)null!);

        var result = await _service.OpenFileAsync();

        Assert.Null(result);
        _templateServiceMock.Verify(s => s.Load("C:\\file.tdel"), Times.Once);
    }

    // === OpenFromFilePath ===

    [Fact]
    public void OpenFromFilePath_LoadsAndCreatesEditor()
    {
        var filePath = "C:\\templates\\library.tdel";
        var template = CreateTemplate();
        using var editor = CreateEditor(template);
        _templateServiceMock.Setup(s => s.Load(filePath)).Returns(template);
        _factoryMock.Setup(f => f.CreateWithFilePath(template, filePath, null, _printServiceMock.Object, null, null)).Returns(editor);

        var result = _service.OpenFromFilePath(filePath);

        Assert.Same(editor, result);
        _templateServiceMock.Verify(s => s.Load(filePath), Times.Once);
        _factoryMock.Verify(f => f.CreateWithFilePath(template, filePath, null, _printServiceMock.Object, null, null), Times.Once);
    }

    [Fact]
    public void OpenFromFilePath_LoadReturnsNull_ReturnsNull()
    {
        _templateServiceMock.Setup(s => s.Load(It.IsAny<string>())).Returns((Template)null!);

        var result = _service.OpenFromFilePath("C:\\file.tdel");

        Assert.Null(result);
        _dialogServiceMock.Verify(d => d.ShowError(It.IsAny<string>()), Times.Never);
        _factoryMock.Verify(f => f.CreateWithFilePath(It.IsAny<Template>(), It.IsAny<string>(), null, It.IsAny<IPrintService?>(), null, null), Times.Never);
    }

    [Fact]
    public void OpenFromFilePath_LoadThrows_ShowsErrorAndLogs()
    {
        _templateServiceMock.Setup(s => s.Load(It.IsAny<string>())).Throws(new InvalidOperationException("lib boom"));

        var result = _service.OpenFromFilePath("C:\\file.tdel");

        Assert.Null(result);
        _dialogServiceMock.Verify(d => d.ShowError(It.Is<string>(m => m.Contains("lib boom"))), Times.Once);
        VerifyLogErrorOnce(_loggerMock);
    }

    [Fact]
    public void OpenFromFilePath_FactoryReturnsNull_PassesThrough()
    {
        var template = CreateTemplate();
        _templateServiceMock.Setup(s => s.Load("C:\\file.tdel")).Returns(template);
        _factoryMock.Setup(f => f.CreateWithFilePath(template, "C:\\file.tdel", null, _printServiceMock.Object, null, null)).Returns((EditorViewModel)null!);

        var result = _service.OpenFromFilePath("C:\\file.tdel");

        Assert.Null(result);
    }

    // === SaveTabAsync ===

    [Fact]
    public async Task SaveTabAsync_NullTab_NoOp()
    {
        await _service.SaveTabAsync(null!);

        _fileServiceMock.Verify(f => f.SaveFileDialog(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _templateServiceMock.Verify(s => s.Save(It.IsAny<Template>(), It.IsAny<string>()), Times.Never);
        _dialogServiceMock.Verify(d => d.ShowUnsavedChangesDialogAsync(It.IsAny<string>()), Times.Never);
        _dialogServiceMock.Verify(d => d.ShowError(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SaveTabAsync_HasFilePath_FileDoesNotExist_SavesWithoutBackup()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"tabops_save_{Guid.NewGuid():N}.tdel");
        try
        {
            Assert.False(File.Exists(filePath));
            var template = CreateTemplate();
            using var editor = CreateEditor(template, filePath);
            editor.MarkDirty();
            Assert.True(editor.DirtyStateManager.IsDirty);

            await _service.SaveTabAsync(editor);

            _templateServiceMock.Verify(s => s.Save(template, filePath), Times.Once);
            _fileServiceMock.Verify(f => f.CreateBackup(It.IsAny<string>()), Times.Never);
            _fileServiceMock.Verify(f => f.SaveFileDialog(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.Equal(filePath, editor.DirtyStateManager.FilePath);
            Assert.False(editor.DirtyStateManager.IsDirty);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task SaveTabAsync_HasFilePath_FileExists_CreatesBackupBeforeSave()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"tabops_backup_{Guid.NewGuid():N}.tdel");
        File.WriteAllText(filePath, "existing");
        try
        {
            var template = CreateTemplate();
            using var editor = CreateEditor(template, filePath);
            var calls = new List<string>();
            _fileServiceMock.Setup(f => f.CreateBackup(filePath)).Callback(() => calls.Add("backup"));
            _templateServiceMock.Setup(s => s.Save(template, filePath)).Callback(() => calls.Add("save"));

            await _service.SaveTabAsync(editor);

            _fileServiceMock.Verify(f => f.CreateBackup(filePath), Times.Once);
            Assert.Equal(new[] { "backup", "save" }, calls);
            Assert.Equal(filePath, editor.DirtyStateManager.FilePath);
            Assert.False(editor.DirtyStateManager.IsDirty);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task SaveTabAsync_NoFilePath_DialogReturnsPath_Saves()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"tabops_sa_dlg_{Guid.NewGuid():N}.tdel");
        try
        {
            var template = CreateTemplate();
            using var editor = CreateEditor(template);
            editor.MarkDirty();
            _fileServiceMock.Setup(f => f.SaveFileDialog("DotElectric Template|*.tdel", It.IsAny<string>())).Returns(filePath);

            await _service.SaveTabAsync(editor);

            _templateServiceMock.Verify(s => s.Save(template, filePath), Times.Once);
            _fileServiceMock.Verify(f => f.SaveFileDialog("DotElectric Template|*.tdel", It.IsAny<string>()), Times.Once);
            Assert.Equal(filePath, editor.DirtyStateManager.FilePath);
            Assert.False(editor.DirtyStateManager.IsDirty);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task SaveTabAsync_NoFilePath_DialogCancelled_DoesNotSave()
    {
        using var editor = CreateEditor(CreateTemplate());
        _fileServiceMock.Setup(f => f.SaveFileDialog(It.IsAny<string>(), It.IsAny<string>())).Returns((string?)null);

        await _service.SaveTabAsync(editor);

        _templateServiceMock.Verify(s => s.Save(It.IsAny<Template>(), It.IsAny<string>()), Times.Never);
        _fileServiceMock.Verify(f => f.CreateBackup(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SaveTabAsync_ValidationErrors_DialogSave_SavesAnyway()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"tabops_val_{Guid.NewGuid():N}.tdel");
        try
        {
            var template = CreateTemplate();
            using var editor = CreateEditor(template, filePath);
            _templateServiceMock.Setup(s => s.Validate(template)).Returns(new[] { "error 1", "error 2" });
            _dialogServiceMock.Setup(d => d.ShowUnsavedChangesDialogAsync(It.IsAny<string>()))
                .ReturnsAsync(UnsavedChangesResult.Save);

            await _service.SaveTabAsync(editor);

            _dialogServiceMock.Verify(d => d.ShowUnsavedChangesDialogAsync(It.Is<string>(m => m.Contains("error 1"))), Times.Once);
            _templateServiceMock.Verify(s => s.Save(template, filePath), Times.Once);
            Assert.False(editor.DirtyStateManager.IsDirty);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task SaveTabAsync_ValidationErrors_DialogDontSave_DoesNotSave()
    {
        var template = CreateTemplate();
        using var editor = CreateEditor(template, "C:\\file.tdel");
        _templateServiceMock.Setup(s => s.Validate(template)).Returns(new[] { "error" });
        _dialogServiceMock.Setup(d => d.ShowUnsavedChangesDialogAsync(It.IsAny<string>()))
            .ReturnsAsync(UnsavedChangesResult.DontSave);

        await _service.SaveTabAsync(editor);

        _templateServiceMock.Verify(s => s.Save(It.IsAny<Template>(), It.IsAny<string>()), Times.Never);
        _dialogServiceMock.Verify(d => d.ShowUnsavedChangesDialogAsync(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task SaveTabAsync_ValidationErrors_DialogCancel_DoesNotSave()
    {
        var template = CreateTemplate();
        using var editor = CreateEditor(template, "C:\\file.tdel");
        _templateServiceMock.Setup(s => s.Validate(template)).Returns(new[] { "error" });
        _dialogServiceMock.Setup(d => d.ShowUnsavedChangesDialogAsync(It.IsAny<string>()))
            .ReturnsAsync(UnsavedChangesResult.Cancel);

        await _service.SaveTabAsync(editor);

        _templateServiceMock.Verify(s => s.Save(It.IsAny<Template>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SaveTabAsync_SaveThrows_ShowsErrorAndLogs_KeepsDirty()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"tabops_thr_{Guid.NewGuid():N}.tdel");
        try
        {
            var template = CreateTemplate();
            using var editor = CreateEditor(template, filePath);
            editor.MarkDirty();
            _templateServiceMock.Setup(s => s.Save(template, filePath)).Throws(new InvalidOperationException("save boom"));

            await _service.SaveTabAsync(editor);

            _dialogServiceMock.Verify(d => d.ShowError(It.Is<string>(m => m.Contains("save boom"))), Times.Once);
            VerifyLogErrorOnce(_loggerMock);
            Assert.True(editor.DirtyStateManager.IsDirty);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    // === SaveAsAsync ===

    [Fact]
    public async Task SaveAsAsync_NullTab_NoOp()
    {
        await _service.SaveAsAsync(null!);

        _fileServiceMock.Verify(f => f.SaveFileDialog(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _templateServiceMock.Verify(s => s.Save(It.IsAny<Template>(), It.IsAny<string>()), Times.Never);
        _dialogServiceMock.Verify(d => d.ShowError(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SaveAsAsync_DialogReturnsPath_SavesAndSetsFilePath()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"tabops_as_{Guid.NewGuid():N}.tdel");
        try
        {
            var template = CreateTemplate();
            using var editor = CreateEditor(template);
            editor.MarkDirty();
            _fileServiceMock.Setup(f => f.SaveFileDialog("DotElectric Template|*.tdel", It.IsAny<string>())).Returns(filePath);

            await _service.SaveAsAsync(editor);

            _templateServiceMock.Verify(s => s.Save(template, filePath), Times.Once);
            Assert.Equal(filePath, editor.DirtyStateManager.FilePath);
            Assert.False(editor.DirtyStateManager.IsDirty);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [Fact]
    public async Task SaveAsAsync_DialogCancelled_DoesNotSave()
    {
        using var editor = CreateEditor(CreateTemplate());
        _fileServiceMock.Setup(f => f.SaveFileDialog(It.IsAny<string>(), It.IsAny<string>())).Returns((string?)null);

        await _service.SaveAsAsync(editor);

        _templateServiceMock.Verify(s => s.Save(It.IsAny<Template>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SaveAsAsync_SaveThrows_ShowsErrorAndLogs()
    {
        var template = CreateTemplate();
        using var editor = CreateEditor(template);
        _fileServiceMock.Setup(f => f.SaveFileDialog(It.IsAny<string>(), It.IsAny<string>())).Returns("C:\\file.tdel");
        _templateServiceMock.Setup(s => s.Save(template, "C:\\file.tdel")).Throws(new InvalidOperationException("as boom"));

        await _service.SaveAsAsync(editor);

        _dialogServiceMock.Verify(d => d.ShowError(It.Is<string>(m => m.Contains("as boom"))), Times.Once);
        VerifyLogErrorOnce(_loggerMock);
    }

    // === CreateNewCustomTab ===

    [Fact]
    public void CreateNewCustomTab_WidthHeight_CreatesCustomSheetAndEditor()
    {
        var template = CreateTemplate();
        using var editor = CreateEditor(template);
        _templateServiceMock.Setup(s => s.CreateFromSheet(It.IsAny<Sheet>())).Returns(template);
        _factoryMock.Setup(f => f.Create(template, null, _printServiceMock.Object, null, null)).Returns(editor);

        var result = _service.CreateNewCustomTab(210, 297);

        Assert.Same(editor, result);
        _templateServiceMock.Verify(s => s.CreateFromSheet(It.Is<Sheet>(sh =>
            sh.Format == "Custom" &&
            sh.WidthMicrons == 210000 &&
            sh.HeightMicrons == 297000 &&
            sh.Orientation == SheetOrientation.Portrait)), Times.Once);
        _factoryMock.Verify(f => f.Create(template, null, _printServiceMock.Object, null, null), Times.Once);
    }

    [Fact]
    public void CreateNewCustomTab_WiderThanTall_UsesLandscapeOrientation()
    {
        _service.CreateNewCustomTab(400, 200);

        _templateServiceMock.Verify(s => s.CreateFromSheet(It.Is<Sheet>(sh =>
            sh.WidthMicrons == 400000 &&
            sh.HeightMicrons == 200000 &&
            sh.Orientation == SheetOrientation.Landscape)), Times.Once);
    }

    // === PromptAndSaveIfDirtyAsync ===

    [Fact]
    public async Task PromptAndSaveIfDirtyAsync_NullTab_ReturnsTrue()
    {
        var result = await _service.PromptAndSaveIfDirtyAsync(null!);

        Assert.True(result);
        _dialogServiceMock.Verify(d => d.ShowUnsavedChangesDialogAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PromptAndSaveIfDirtyAsync_NotDirty_ReturnsTrueWithoutDialog()
    {
        using var editor = CreateEditor(CreateTemplate());

        var result = await _service.PromptAndSaveIfDirtyAsync(editor);

        Assert.True(result);
        _dialogServiceMock.Verify(d => d.ShowUnsavedChangesDialogAsync(It.IsAny<string>()), Times.Never);
        _templateServiceMock.Verify(s => s.Save(It.IsAny<Template>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PromptAndSaveIfDirtyAsync_Dirty_DialogCancel_ReturnsFalseWithoutSave()
    {
        using var editor = CreateEditor(CreateTemplate());
        editor.MarkDirty();
        _dialogServiceMock.Setup(d => d.ShowUnsavedChangesDialogAsync(It.IsAny<string>()))
            .ReturnsAsync(UnsavedChangesResult.Cancel);

        var result = await _service.PromptAndSaveIfDirtyAsync(editor);

        Assert.False(result);
        _templateServiceMock.Verify(s => s.Save(It.IsAny<Template>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PromptAndSaveIfDirtyAsync_Dirty_DialogDontSave_ReturnsTrueWithoutSave()
    {
        using var editor = CreateEditor(CreateTemplate());
        editor.MarkDirty();
        _dialogServiceMock.Setup(d => d.ShowUnsavedChangesDialogAsync(It.IsAny<string>()))
            .ReturnsAsync(UnsavedChangesResult.DontSave);

        var result = await _service.PromptAndSaveIfDirtyAsync(editor);

        Assert.True(result);
        _templateServiceMock.Verify(s => s.Save(It.IsAny<Template>(), It.IsAny<string>()), Times.Never);
        Assert.True(editor.DirtyStateManager.IsDirty);
    }

    [Fact]
    public async Task PromptAndSaveIfDirtyAsync_Dirty_DialogSave_SavesTabAndReturnsTrue()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"tabops_prompt_{Guid.NewGuid():N}.tdel");
        try
        {
            var template = CreateTemplate();
            using var editor = CreateEditor(template, filePath);
            editor.MarkDirty();
            _dialogServiceMock.Setup(d => d.ShowUnsavedChangesDialogAsync(It.IsAny<string>()))
                .ReturnsAsync(UnsavedChangesResult.Save);

            var result = await _service.PromptAndSaveIfDirtyAsync(editor);

            Assert.True(result);
            _templateServiceMock.Verify(s => s.Save(template, filePath), Times.Once);
            Assert.Equal(filePath, editor.DirtyStateManager.FilePath);
            Assert.False(editor.DirtyStateManager.IsDirty);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    // === ParseSheetFormat ===

    [Fact]
    public void ParseSheetFormat_Null_ReturnsNull()
    {
        var result = TabOperationsService.ParseSheetFormat(null!, out var orientation);

        Assert.Null(result);
        Assert.Null(orientation);
    }

    [Fact]
    public void ParseSheetFormat_Empty_ReturnsEmpty()
    {
        var result = TabOperationsService.ParseSheetFormat("", out var orientation);

        Assert.Equal("", result);
        Assert.Null(orientation);
    }

    [Fact]
    public void ParseSheetFormat_SingleChar_ReturnsAsIs()
    {
        var result = TabOperationsService.ParseSheetFormat("A", out var orientation);

        Assert.Equal("A", result);
        Assert.Null(orientation);
    }

    [Theory]
    [InlineData("A4P", "A4", SheetOrientation.Portrait)]
    [InlineData("A3L", "A3", SheetOrientation.Landscape)]
    [InlineData("A4×2L", "A4×2", SheetOrientation.Landscape)]
    public void ParseSheetFormat_WithSuffix_ReturnsBaseFormatAndOrientation(string input, string expectedFormat, SheetOrientation expectedOrientation)
    {
        var result = TabOperationsService.ParseSheetFormat(input, out var orientation);

        Assert.Equal(expectedFormat, result);
        Assert.Equal(expectedOrientation, orientation);
    }

    [Fact]
    public void ParseSheetFormat_LowercaseSuffix_IsUppercasedAndDetected()
    {
        var result = TabOperationsService.ParseSheetFormat("a4p", out var orientation);

        Assert.Equal("a4", result);
        Assert.Equal(SheetOrientation.Portrait, orientation);
    }

    [Fact]
    public void ParseSheetFormat_NoSuffix_ReturnsFormatAndNullOrientation()
    {
        var result = TabOperationsService.ParseSheetFormat("A4", out var orientation);

        Assert.Equal("A4", result);
        Assert.Null(orientation);
    }

    [Fact]
    public void ParseSheetFormat_UnknownSuffix_ReturnsFormatAsIs()
    {
        var result = TabOperationsService.ParseSheetFormat("A4X", out var orientation);

        Assert.Equal("A4X", result);
        Assert.Null(orientation);
    }
}