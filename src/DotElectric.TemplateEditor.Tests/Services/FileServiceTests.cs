using System;
using System.IO;
using DotElectric.TemplateEditor.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotElectric.TemplateEditor.Tests.Services;

/// <summary>
/// Тесты для FileService.
/// </summary>
public class FileServiceTests : IDisposable
{
    private readonly string _testAppDataFolder;
    private readonly string _testTemplatesFolder;
    private readonly string _realBackupFolder;
    private readonly Mock<ILogger<FileService>> _loggerMock;
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly Mock<IDialogFileService> _dialogServiceMock;
    private readonly FileService _fileService;
    private static readonly DateTime FixedDate = new(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);
    private readonly List<string> _createdBackupFiles = new();

    public FileServiceTests()
    {
        // Создаём временную папку для тестов
        _testAppDataFolder = Path.Combine(Path.GetTempPath(), $"DotElectricTests_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testAppDataFolder);
        _testTemplatesFolder = Path.Combine(_testAppDataFolder, "Templates");

        _loggerMock = new Mock<ILogger<FileService>>();
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(FixedDate);
        _dialogServiceMock = new Mock<IDialogFileService>();
        _dialogServiceMock.Setup(d => d.OpenFileDialog(It.IsAny<string>())).Returns((string?)null);
        _dialogServiceMock.Setup(d => d.SaveFileDialog(It.IsAny<string>(), It.IsAny<string>())).Returns((string?)null);
        _fileService = new FileService(_loggerMock.Object, _dateTimeProviderMock.Object, _dialogServiceMock.Object);
        _realBackupFolder = _fileService.GetBackupFolder();
    }

    public void Dispose()
    {
        // Удаляем временные папки
        if (Directory.Exists(_testAppDataFolder))
        {
            Directory.Delete(_testAppDataFolder, true);
        }

        // Очищаем созданные бэкапы из реальной папки
        foreach (var file in _createdBackupFiles)
        {
            if (File.Exists(file))
                File.Delete(file);
        }
    }

    #region GetTemplatesFolder

    [Fact]
    public void GetTemplatesFolder_ReturnsValidPath()
    {
        // Act
        var result = _fileService.GetTemplatesFolder();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(Directory.Exists(result));
    }

    [Fact]
    public void GetTemplatesFolder_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var folder = _fileService.GetTemplatesFolder();
        Directory.Delete(folder, true);

        // Act
        var result = _fileService.GetTemplatesFolder();

        // Assert
        Assert.True(Directory.Exists(result));
    }

    #endregion

    #region GetBackupFolder

    [Fact]
    public void GetBackupFolder_ReturnsValidPath()
    {
        // Act
        var result = _fileService.GetBackupFolder();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(Directory.Exists(result));
    }

    [Fact]
    public void GetBackupFolder_CreatesDirectoryIfNotExists()
    {
        // Arrange
        var folder = _fileService.GetBackupFolder();
        Directory.Delete(folder, true);

        // Act
        var result = _fileService.GetBackupFolder();

        // Assert
        Assert.True(Directory.Exists(result));
    }

    #endregion

    #region CreateBackup

    [Fact]
    public void CreateBackup_ValidFile_CreatesBackup()
    {
        // Arrange
        var testId = Guid.NewGuid().ToString("N")[..8];
        var sourceFile = Path.Combine(_testAppDataFolder, $"test_{testId}.tdel");
        File.WriteAllText(sourceFile, "<Template>Test</Template>");

        // Act
        _fileService.CreateBackup(sourceFile);

        // Assert
        var backupFiles = Directory.GetFiles(_realBackupFolder, $"test_{testId}_backup_*.tdel");
        _createdBackupFiles.AddRange(backupFiles);
        Assert.Single(backupFiles);
    }

    [Fact]
    public void CreateBackup_EmptyPath_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _fileService.CreateBackup(""));
    }

    [Fact]
    public void CreateBackup_NullPath_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _fileService.CreateBackup(null!));
    }

    [Fact]
    public void CreateBackup_WhiteSpacePath_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _fileService.CreateBackup("   "));
    }

    [Fact]
    public void CreateBackup_NonExistentFile_ThrowsFileNotFoundException()
    {
        // Act & Assert
        Assert.Throws<FileNotFoundException>(() => _fileService.CreateBackup("nonexistent.tdel"));
    }

    [Fact]
    public void CreateBackup_LogsInformation()
    {
        // Arrange
        var testId = Guid.NewGuid().ToString("N")[..8];
        var sourceFile = Path.Combine(_testAppDataFolder, $"test_{testId}.tdel");
        File.WriteAllText(sourceFile, "<Template>Test</Template>");

        // Act
        _fileService.CreateBackup(sourceFile);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        // Cleanup
        var backupFiles = Directory.GetFiles(_realBackupFolder, $"test_{testId}_backup_*.tdel");
        _createdBackupFiles.AddRange(backupFiles);
    }

    [Fact]
    public void CreateBackup_MultipleBackups_CreatesUniqueFiles()
    {
        // Arrange
        var testId = Guid.NewGuid().ToString("N")[..8];
        var sourceFile = Path.Combine(_testAppDataFolder, $"test_{testId}.tdel");
        File.WriteAllText(sourceFile, "<Template>Test</Template>");
        var fixedDate = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        _dateTimeProviderMock.SetupSequence(p => p.UtcNow)
            .Returns(fixedDate)
            .Returns(fixedDate.AddSeconds(2));

        // Act
        _fileService.CreateBackup(sourceFile);
        _fileService.CreateBackup(sourceFile);

        // Assert
        var backupFiles = Directory.GetFiles(_realBackupFolder, $"test_{testId}_backup_*.tdel");
        _createdBackupFiles.AddRange(backupFiles);
        Assert.Equal(2, backupFiles.Length);
    }

    #endregion

    #region OpenFileDialog

    [Fact]
    public void OpenFileDialog_NullFilter_PassesThrough()
    {
        // Arrange
        _dialogServiceMock.Setup(d => d.OpenFileDialog(It.IsAny<string>())).Returns((string? filter) => filter);

        // Act
        var result = _fileService.OpenFileDialog(null!);

        // Assert
        _dialogServiceMock.Verify(d => d.OpenFileDialog(null!), Times.Once);
    }

    [Fact]
    public void OpenFileDialog_CustomFilter_DelegatesToDialogService()
    {
        // Arrange
        const string expectedFilter = "Text Files|*.txt";
        _dialogServiceMock.Setup(d => d.OpenFileDialog(expectedFilter)).Returns("/mocked/path.tdel");

        // Act
        var result = _fileService.OpenFileDialog(expectedFilter);

        // Assert
        Assert.Equal("/mocked/path.tdel", result);
        _dialogServiceMock.Verify(d => d.OpenFileDialog(expectedFilter), Times.Once);
    }

    #endregion

    #region SaveFileDialog

    [Fact]
    public void SaveFileDialog_NullFilter_PassesThrough()
    {
        // Arrange
        _dialogServiceMock.Setup(d => d.SaveFileDialog(It.IsAny<string>(), It.IsAny<string>())).Returns((string? f, string n) => f);

        // Act
        var result = _fileService.SaveFileDialog(null!, "default.tdel");

        // Assert
        _dialogServiceMock.Verify(d => d.SaveFileDialog(null!, "default.tdel"), Times.Once);
    }

    [Fact]
    public void SaveFileDialog_CustomFilter_DelegatesToDialogService()
    {
        // Arrange
        const string expectedFilter = "Text Files|*.txt";
        const string expectedFileName = "default.txt";
        _dialogServiceMock.Setup(d => d.SaveFileDialog(expectedFilter, expectedFileName)).Returns("/mocked/save.tdel");

        // Act
        var result = _fileService.SaveFileDialog(expectedFilter, expectedFileName);

        // Assert
        Assert.Equal("/mocked/save.tdel", result);
        _dialogServiceMock.Verify(d => d.SaveFileDialog(expectedFilter, expectedFileName), Times.Once);
    }

    [Fact]
    public void SaveFileDialog_DefaultFileName_DelegatesToDialogService()
    {
        // Arrange
        const string expectedFileName = "my_template.tdel";
        _dialogServiceMock.Setup(d => d.SaveFileDialog(null!, expectedFileName)).Returns("/mocked/save2.tdel");

        // Act
        var result = _fileService.SaveFileDialog(null!, expectedFileName);

        // Assert
        Assert.Equal("/mocked/save2.tdel", result);
        _dialogServiceMock.Verify(d => d.SaveFileDialog(null!, expectedFileName), Times.Once);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void GetTemplatesFolder_And_GetBackupFolder_ReturnDifferentPaths()
    {
        // Act
        var templates = _fileService.GetTemplatesFolder();
        var backups = _fileService.GetBackupFolder();

        // Assert
        Assert.NotEqual(templates, backups);
        Assert.DoesNotContain(templates, backups);
    }

    [Fact]
    public void CreateBackup_OverwritesExistingBackup()
    {
        // Arrange
        var testId = Guid.NewGuid().ToString("N")[..8];
        var sourceFile = Path.Combine(_testAppDataFolder, $"test_{testId}.tdel");
        File.WriteAllText(sourceFile, "<Template>Test</Template>");
        var fixedDate = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        _dateTimeProviderMock.SetupSequence(p => p.UtcNow)
            .Returns(fixedDate)
            .Returns(fixedDate);

        // Act - создаём первую резервную копию
        _fileService.CreateBackup(sourceFile);

        // Изменяем исходный файл
        File.WriteAllText(sourceFile, "<Template>Modified</Template>");

        // Act - создаём вторую резервную копию (тот же timestamp → перезапись)
        _fileService.CreateBackup(sourceFile);

        // Assert
        var backupFiles = Directory.GetFiles(_realBackupFolder, $"test_{testId}_backup_*.tdel");
        _createdBackupFiles.AddRange(backupFiles);
        Assert.Single(backupFiles);
        var content = File.ReadAllText(backupFiles[0]);
        Assert.Contains("Modified", content);
    }

    [Fact]
    public void CreateBackup_InvalidFileNameChars_HandledCorrectly()
    {
        // Arrange
        var testId = Guid.NewGuid().ToString("N")[..8];
        var sourceFile = Path.Combine(_testAppDataFolder, $"test-file-{testId}.tdel");
        File.WriteAllText(sourceFile, "<Template>Test</Template>");

        // Act
        _fileService.CreateBackup(sourceFile);

        // Assert
        var backupFiles = Directory.GetFiles(_realBackupFolder, $"test-file-{testId}_backup_*.tdel");
        _createdBackupFiles.AddRange(backupFiles);
        Assert.Single(backupFiles);
    }

    #endregion
}
