using DotElectric.TemplateEditor.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Services;

public class TemplateLibraryServiceTests
{
    private static string CreateTempFolder()
    {
        var folder = Path.Combine(Path.GetTempPath(), $"tlib_{Guid.NewGuid():N}");
        Directory.CreateDirectory(folder);
        return folder;
    }

    private static Mock<IFileService> CreateFileServiceMock(string templatesFolder)
    {
        var fileServiceMock = new Mock<IFileService>();
        fileServiceMock.Setup(f => f.GetTemplatesFolder()).Returns(templatesFolder);
        return fileServiceMock;
    }

    private static void DeleteFolder(string folder)
    {
        if (Directory.Exists(folder))
            Directory.Delete(folder, true);
    }

    // === Constructor ===

    [Fact]
    public void Ctor_CapturesTemplatesFolderFromFileService()
    {
        var tempFolder = CreateTempFolder();
        try
        {
            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            Assert.Equal(tempFolder, service.TemplatesFolder);
        }
        finally
        {
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void Ctor_NullLogger_DoesNotThrow()
    {
        var tempFolder = CreateTempFolder();
        try
        {
            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            Assert.Equal(tempFolder, service.TemplatesFolder);
        }
        finally
        {
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void TemplateInfo_HasCorrectProperties()
    {
        var info = new TemplateInfo("file.tdel", "My Template", "/path/to/file.tdel");

        Assert.Equal("file.tdel", info.FileName);
        Assert.Equal("My Template", info.DisplayName);
        Assert.Equal("/path/to/file.tdel", info.FullPath);
    }

    // === LoadTemplateInfos ===

    [Fact]
    public void LoadTemplateInfos_FolderDoesNotExist_ReturnsEmptyList()
    {
        var fileServiceMock = new Mock<IFileService>();
        var missingFolder = Path.Combine(Path.GetTempPath(), $"tlib_missing_{Guid.NewGuid():N}");
        fileServiceMock.Setup(f => f.GetTemplatesFolder()).Returns(missingFolder);

        var service = new TemplateLibraryService(fileServiceMock.Object);
        var result = service.LoadTemplateInfos();

        Assert.Empty(result);
    }

    [Fact]
    public void LoadTemplateInfos_NoFiles_ReturnsEmptyList()
    {
        var tempFolder = CreateTempFolder();
        try
        {
            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            var result = service.LoadTemplateInfos();

            Assert.Empty(result);
        }
        finally
        {
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void LoadTemplateInfos_WithFiles_ReturnsSortedList()
    {
        var tempFolder = CreateTempFolder();
        try
        {
            File.WriteAllText(Path.Combine(tempFolder, "bravo.tdel"), "test");
            File.WriteAllText(Path.Combine(tempFolder, "alpha.tdel"), "test");
            File.WriteAllText(Path.Combine(tempFolder, "charlie.tdel"), "test");
            // Не-tdel файл — должен игнорироваться
            File.WriteAllText(Path.Combine(tempFolder, "other.txt"), "test");

            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            var result = service.LoadTemplateInfos();

            Assert.Equal(3, result.Count);
            Assert.Equal("alpha", result[0].DisplayName);
            Assert.Equal("bravo", result[1].DisplayName);
            Assert.Equal("charlie", result[2].DisplayName);
        }
        finally
        {
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void LoadTemplateInfos_FilesHaveCorrectProperties()
    {
        var tempFolder = CreateTempFolder();
        try
        {
            File.WriteAllText(Path.Combine(tempFolder, "frame.tdel"), "test");

            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            var result = service.LoadTemplateInfos();

            var info = Assert.Single(result);
            Assert.Equal("frame.tdel", info.FileName);
            Assert.Equal("frame", info.DisplayName);
            Assert.Equal(Path.Combine(tempFolder, "frame.tdel"), info.FullPath);
        }
        finally
        {
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void LoadTemplateInfos_SortsCaseInsensitive()
    {
        var tempFolder = CreateTempFolder();
        try
        {
            // Discriminating pair: Ordinal would order Beta before alpha ('B' < 'a'),
            // OrdinalIgnoreCase orders alpha before Beta (a < b). Files created scrambled.
            File.WriteAllText(Path.Combine(tempFolder, "gamma.tdel"), "test");
            File.WriteAllText(Path.Combine(tempFolder, "Beta.tdel"), "test");
            File.WriteAllText(Path.Combine(tempFolder, "alpha.tdel"), "test");

            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            var result = service.LoadTemplateInfos();

            Assert.Equal(3, result.Count);
            Assert.Equal("alpha", result[0].DisplayName);
            Assert.Equal("Beta", result[1].DisplayName);
            Assert.Equal("gamma", result[2].DisplayName);
        }
        finally
        {
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void LoadTemplateInfos_OnlyNonTdelFiles_ReturnsEmptyList()
    {
        var tempFolder = CreateTempFolder();
        try
        {
            File.WriteAllText(Path.Combine(tempFolder, "readme.txt"), "test");

            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            var result = service.LoadTemplateInfos();

            Assert.Empty(result);
        }
        finally
        {
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void LoadTemplateInfos_IgnoresSubdirectoryFiles()
    {
        var tempFolder = CreateTempFolder();
        try
        {
            File.WriteAllText(Path.Combine(tempFolder, "root.tdel"), "test");
            var subFolder = Path.Combine(tempFolder, "sub");
            Directory.CreateDirectory(subFolder);
            File.WriteAllText(Path.Combine(subFolder, "nested.tdel"), "test");

            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            var result = service.LoadTemplateInfos();

            var info = Assert.Single(result);
            Assert.Equal("root.tdel", info.FileName);
        }
        finally
        {
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void LoadTemplateInfos_IgnoresFileWithoutExtension()
    {
        var tempFolder = CreateTempFolder();
        try
        {
            File.WriteAllText(Path.Combine(tempFolder, "noext"), "test");

            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            var result = service.LoadTemplateInfos();

            Assert.Empty(result);
        }
        finally
        {
            DeleteFolder(tempFolder);
        }
    }

    // === CopyToLibrary ===

    [Fact]
    public void CopyToLibrary_NullSource_ThrowsArgumentException()
    {
        var tempFolder = CreateTempFolder();
        try
        {
            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            var ex = Assert.Throws<ArgumentException>(() => service.CopyToLibrary(null!));
            Assert.Equal("sourceFilePath", ex.ParamName);
        }
        finally
        {
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void CopyToLibrary_WhitespaceSource_ThrowsArgumentException()
    {
        var tempFolder = CreateTempFolder();
        try
        {
            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            var ex = Assert.Throws<ArgumentException>(() => service.CopyToLibrary("   "));
            Assert.Equal("sourceFilePath", ex.ParamName);
        }
        finally
        {
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void CopyToLibrary_SourceDoesNotExist_ThrowsFileNotFoundException()
    {
        var tempFolder = CreateTempFolder();
        try
        {
            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            var missing = Path.Combine(tempFolder, "missing.tdel");
            Assert.Throws<FileNotFoundException>(() => service.CopyToLibrary(missing));
        }
        finally
        {
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void CopyToLibrary_NonTdelExtension_ThrowsArgumentException()
    {
        var tempFolder = CreateTempFolder();
        var sourceFolder = CreateTempFolder();
        try
        {
            var sourcePath = Path.Combine(sourceFolder, "x.txt");
            File.WriteAllText(sourcePath, "test");

            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            var ex = Assert.Throws<ArgumentException>(() => service.CopyToLibrary(sourcePath));
            Assert.Equal("sourceFilePath", ex.ParamName);
        }
        finally
        {
            DeleteFolder(sourceFolder);
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void CopyToLibrary_HappyPath_CopiesFileAndReturnsInfo()
    {
        var tempFolder = CreateTempFolder();
        var sourceFolder = CreateTempFolder();
        try
        {
            var sourcePath = Path.Combine(sourceFolder, "frame.tdel");
            File.WriteAllText(sourcePath, "test");

            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            var info = service.CopyToLibrary(sourcePath);

            Assert.Equal("frame.tdel", info.FileName);
            Assert.Equal("frame", info.DisplayName);
            Assert.Equal(Path.Combine(tempFolder, "frame.tdel"), info.FullPath);
            Assert.True(File.Exists(info.FullPath));
        }
        finally
        {
            DeleteFolder(sourceFolder);
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void CopyToLibrary_WithNewName_UsesCustomName()
    {
        var tempFolder = CreateTempFolder();
        var sourceFolder = CreateTempFolder();
        try
        {
            var sourcePath = Path.Combine(sourceFolder, "frame.tdel");
            File.WriteAllText(sourcePath, "test");

            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            var info = service.CopyToLibrary(sourcePath, "Custom");

            Assert.Equal("Custom.tdel", info.FileName);
            Assert.Equal("Custom", info.DisplayName);
            Assert.True(File.Exists(Path.Combine(tempFolder, "Custom.tdel")));
        }
        finally
        {
            DeleteFolder(sourceFolder);
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void CopyToLibrary_Collision_AppendsCounter()
    {
        var tempFolder = CreateTempFolder();
        var sourceFolder = CreateTempFolder();
        try
        {
            File.WriteAllText(Path.Combine(tempFolder, "foo.tdel"), "existing");
            var sourcePath = Path.Combine(sourceFolder, "foo.tdel");
            File.WriteAllText(sourcePath, "new");

            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            var info = service.CopyToLibrary(sourcePath);

            Assert.Equal("foo_1.tdel", info.FileName);
            Assert.Equal("foo_1", info.DisplayName);
            Assert.True(File.Exists(Path.Combine(tempFolder, "foo_1.tdel")));
        }
        finally
        {
            DeleteFolder(sourceFolder);
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void CopyToLibrary_CollisionWithCounterTwo_UsesNextFreeCounter()
    {
        var tempFolder = CreateTempFolder();
        var sourceFolder = CreateTempFolder();
        try
        {
            File.WriteAllText(Path.Combine(tempFolder, "foo.tdel"), "existing");
            File.WriteAllText(Path.Combine(tempFolder, "foo_1.tdel"), "existing1");
            var sourcePath = Path.Combine(sourceFolder, "foo.tdel");
            File.WriteAllText(sourcePath, "new");

            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            var info = service.CopyToLibrary(sourcePath);

            Assert.Equal("foo_2.tdel", info.FileName);
            Assert.True(File.Exists(Path.Combine(tempFolder, "foo_2.tdel")));
        }
        finally
        {
            DeleteFolder(sourceFolder);
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void CopyToLibrary_NewNameCollision_UsesCounterWithNewName()
    {
        var tempFolder = CreateTempFolder();
        var sourceFolder = CreateTempFolder();
        try
        {
            File.WriteAllText(Path.Combine(tempFolder, "custom.tdel"), "original");
            var sourcePath = Path.Combine(sourceFolder, "source.tdel");
            File.WriteAllText(sourcePath, "new");

            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            var info = service.CopyToLibrary(sourcePath, "custom");

            Assert.Equal("custom_1.tdel", info.FileName);
            Assert.Equal("custom_1", info.DisplayName);
            Assert.True(File.Exists(Path.Combine(tempFolder, "custom_1.tdel")));
            Assert.True(File.Exists(Path.Combine(tempFolder, "custom.tdel")));
        }
        finally
        {
            DeleteFolder(sourceFolder);
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void CopyToLibrary_CreatesMissingTemplatesFolder()
    {
        var missingFolder = Path.Combine(Path.GetTempPath(), $"tlib_missing_{Guid.NewGuid():N}");
        var sourceFolder = CreateTempFolder();
        try
        {
            var sourcePath = Path.Combine(sourceFolder, "frame.tdel");
            File.WriteAllText(sourcePath, "test");

            var service = new TemplateLibraryService(CreateFileServiceMock(missingFolder).Object);
            var info = service.CopyToLibrary(sourcePath);

            Assert.True(Directory.Exists(missingFolder));
            Assert.True(File.Exists(info.FullPath));
        }
        finally
        {
            DeleteFolder(sourceFolder);
            DeleteFolder(missingFolder);
        }
    }

    [Fact]
    public void CopyToLibrary_LogsInformationOnSuccess()
    {
        var tempFolder = CreateTempFolder();
        var sourceFolder = CreateTempFolder();
        try
        {
            var sourcePath = Path.Combine(sourceFolder, "logme.tdel");
            File.WriteAllText(sourcePath, "test");

            var loggerMock = new Mock<ILogger<TemplateLibraryService>>();
            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object, loggerMock.Object);
            service.CopyToLibrary(sourcePath);

            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            DeleteFolder(sourceFolder);
            DeleteFolder(tempFolder);
        }
    }

    // === RemoveFromLibrary ===

    [Fact]
    public void RemoveFromLibrary_NullTemplate_ThrowsArgumentNullException()
    {
        var tempFolder = CreateTempFolder();
        try
        {
            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            Assert.Throws<ArgumentNullException>(() => service.RemoveFromLibrary(null!));
        }
        finally
        {
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void RemoveFromLibrary_WhitespaceFullPath_ThrowsArgumentException()
    {
        var tempFolder = CreateTempFolder();
        try
        {
            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            var template = new TemplateInfo("a.tdel", "A", "   ");
            var ex = Assert.Throws<ArgumentException>(() => service.RemoveFromLibrary(template));
            Assert.Equal("template", ex.ParamName);
        }
        finally
        {
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void RemoveFromLibrary_MissingFile_ThrowsFileNotFoundException()
    {
        var tempFolder = CreateTempFolder();
        try
        {
            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            var template = new TemplateInfo("missing.tdel", "Missing", Path.Combine(tempFolder, "missing.tdel"));
            Assert.Throws<FileNotFoundException>(() => service.RemoveFromLibrary(template));
        }
        finally
        {
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void RemoveFromLibrary_PathOutsideFolder_ThrowsInvalidOperationException()
    {
        var tempFolder = CreateTempFolder();
        var outsideFolder = CreateTempFolder();
        try
        {
            var outsidePath = Path.Combine(outsideFolder, "outside.tdel");
            File.WriteAllText(outsidePath, "test");

            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            var template = new TemplateInfo("outside.tdel", "Outside", outsidePath);
            Assert.Throws<InvalidOperationException>(() => service.RemoveFromLibrary(template));
        }
        finally
        {
            DeleteFolder(outsideFolder);
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void RemoveFromLibrary_HappyPath_DeletesFileAndLogs()
    {
        var tempFolder = CreateTempFolder();
        try
        {
            var filePath = Path.Combine(tempFolder, "delete_me.tdel");
            File.WriteAllText(filePath, "test");

            var loggerMock = new Mock<ILogger<TemplateLibraryService>>();
            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object, loggerMock.Object);
            var template = new TemplateInfo("delete_me.tdel", "DeleteMe", filePath);
            service.RemoveFromLibrary(template);

            Assert.False(File.Exists(filePath));
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
        finally
        {
            DeleteFolder(tempFolder);
        }
    }

    [Fact]
    public void RemoveFromLibrary_CaseInsensitiveFolderPrefix_RemovesFile()
    {
        var tempFolder = CreateTempFolder();
        try
        {
            var filePath = Path.Combine(tempFolder, "case.tdel");
            File.WriteAllText(filePath, "test");

            var service = new TemplateLibraryService(CreateFileServiceMock(tempFolder).Object);
            var caseInsensitivePath = Path.Combine(tempFolder.ToUpperInvariant(), "case.tdel");
            var template = new TemplateInfo("case.tdel", "Case", caseInsensitivePath);
            service.RemoveFromLibrary(template);

            Assert.False(File.Exists(filePath));
        }
        finally
        {
            DeleteFolder(tempFolder);
        }
    }
}
