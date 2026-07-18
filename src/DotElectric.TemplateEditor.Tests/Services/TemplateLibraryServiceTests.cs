using DotElectric.TemplateEditor.Services;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Services;

public class TemplateLibraryServiceTests
{
    [Fact]
    public void LoadTemplateInfos_NoFiles_ReturnsEmptyList()
    {
        var fileServiceMock = new Mock<IFileService>();
        var tempFolder = Path.Combine(Path.GetTempPath(), $"test_lib_empty_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempFolder);
        fileServiceMock.Setup(f => f.GetTemplatesFolder()).Returns(tempFolder);

        var service = new TemplateLibraryService(fileServiceMock.Object);
        var result = service.LoadTemplateInfos();

        Assert.Empty(result);

        // Cleanup
        Directory.Delete(tempFolder, true);
    }

    [Fact]
    public void LoadTemplateInfos_WithFiles_ReturnsSortedList()
    {
        var fileServiceMock = new Mock<IFileService>();
        var tempFolder = Path.Combine(Path.GetTempPath(), $"test_lib_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempFolder);

        // Создаём тестовые файлы
        File.WriteAllText(Path.Combine(tempFolder, "bravo.tdel"), "test");
        File.WriteAllText(Path.Combine(tempFolder, "alpha.tdel"), "test");
        File.WriteAllText(Path.Combine(tempFolder, "charlie.tdel"), "test");
        // Не-tdel файл — должен игнорироваться
        File.WriteAllText(Path.Combine(tempFolder, "other.txt"), "test");

        fileServiceMock.Setup(f => f.GetTemplatesFolder()).Returns(tempFolder);

        var service = new TemplateLibraryService(fileServiceMock.Object);
        var result = service.LoadTemplateInfos();

        Assert.Equal(3, result.Count);
        // Проверка сортировки
        Assert.Equal("alpha", result[0].DisplayName);
        Assert.Equal("bravo", result[1].DisplayName);
        Assert.Equal("charlie", result[2].DisplayName);

        // Cleanup
        Directory.Delete(tempFolder, true);
    }

    [Fact]
    public void TemplateInfo_HasCorrectProperties()
    {
        var info = new TemplateInfo("file.tdel", "My Template", "/path/to/file.tdel");

        Assert.Equal("file.tdel", info.FileName);
        Assert.Equal("My Template", info.DisplayName);
        Assert.Equal("/path/to/file.tdel", info.FullPath);
    }

    [Fact]
    public void TemplatesFolder_ReturnsFolderPath()
    {
        var fileServiceMock = new Mock<IFileService>();
        var tempFolder = Path.Combine(Path.GetTempPath(), $"test_lib_folder_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempFolder);
        fileServiceMock.Setup(f => f.GetTemplatesFolder()).Returns(tempFolder);

        var service = new TemplateLibraryService(fileServiceMock.Object);
        Assert.Equal(tempFolder, service.TemplatesFolder);

        // Cleanup
        Directory.Delete(tempFolder, true);
    }
}
