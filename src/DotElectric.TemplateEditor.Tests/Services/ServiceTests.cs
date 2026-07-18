using System.IO;
using System.Xml.Serialization;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.Services;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Services;

public class TemplateServiceExtendedTests
{
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly TemplateService _service;
    private static readonly DateTime FixedDate = new(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    public TemplateServiceExtendedTests()
    {
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(FixedDate);
        _service = new TemplateService(dateTimeProvider: _dateTimeProviderMock.Object);
    }

    [Fact]
    public void SaveAndLoad_PreservesMultipleObjectTypes()
    {
        var template = CreateTestTemplate();
        template.Objects.Add(new Line(0, 0, 10000, 10000, LineType.Solid));
        template.Objects.Add(new Rectangle(5000, 5000, 10000, 8000, LineType.Dashed));
        template.Objects.Add(new Text(2000, 3000, "Label", 5000, "ГОСТ А", TextType.Label));
        template.Objects.Add(new Text(20000, 30000, "Note", 3500, "ГОСТ Б", TextType.Note, 180));

        var filePath = Path.Combine(Path.GetTempPath(), $"test_multi_{Guid.NewGuid():N}.tdel");
        try
        {
            _service.Save(template, filePath);
            var loaded = _service.Load(filePath);

            Assert.Equal(4, loaded.Objects.Count);
            Assert.IsType<Line>(loaded.Objects[0]);
            Assert.IsType<Rectangle>(loaded.Objects[1]);
            Assert.IsType<Text>(loaded.Objects[2]);
            Assert.IsType<Text>(loaded.Objects[3]);

            var text2 = (Text)loaded.Objects[3];
            Assert.Equal(180, text2.RotationAngle);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void SaveAndLoad_PreservesMetadata()
    {
        var template = new Template(
            new Metadata
            {
                Name = "My Template",
                Author = "John Doe",
                Description = "A test template",
                CreatedDate = new DateTime(2024, 1, 15),
                ModifiedDate = new DateTime(2024, 6, 20)
            },
            Sheet.FromFormat("A3"));

        var filePath = Path.Combine(Path.GetTempPath(), $"test_meta_{Guid.NewGuid():N}.tdel");
        try
        {
            _service.Save(template, filePath);
            var loaded = _service.Load(filePath);

            Assert.Equal("My Template", loaded.Metadata.Name);
            Assert.Equal("John Doe", loaded.Metadata.Author);
            Assert.Equal("A test template", loaded.Metadata.Description);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void Save_CreatesValidZipArchive()
    {
        var template = CreateTestTemplate();
        var filePath = Path.Combine(Path.GetTempPath(), $"test_zip_{Guid.NewGuid():N}.tdel");
        try
        {
            _service.Save(template, filePath);

            // Verify it's a valid ZIP
            using var archive = System.IO.Compression.ZipFile.OpenRead(filePath);
            Assert.NotEmpty(archive.Entries);
            Assert.Contains("template.xml", archive.Entries.Select(e => e.Name));
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void Save_OverwritesExistingFile()
    {
        var template = CreateTestTemplate();
        var filePath = Path.Combine(Path.GetTempPath(), $"test_overwrite_{Guid.NewGuid():N}.tdel");
        try
        {
            _service.Save(template, filePath);

            template.Metadata.Name = "Updated";
            _service.Save(template, filePath);

            // Verify the saved file contains updated name
            using var archive = System.IO.Compression.ZipFile.OpenRead(filePath);
            var entry = archive.GetEntry("template.xml");
            Assert.NotNull(entry);
            using var reader = new StreamReader(entry.Open());
            var xml = reader.ReadToEnd();
            Assert.Contains("Updated", xml);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    private static Template CreateTestTemplate()
    {
        return new Template(
            new Metadata { Name = "Test", Author = "Test", CreatedDate = FixedDate, ModifiedDate = FixedDate },
            Sheet.FromFormat("A3"));
    }
}

public class FileServiceUnitTests
{
    [Fact]
    public void GetTemplatesFolder_CreatesDirectory()
    {
        var service = new FileService();
        var folder = service.GetTemplatesFolder();

        Assert.True(Directory.Exists(folder));
        Assert.Contains("DotElectric", folder);
        Assert.Contains("Templates", folder);
    }

    [Fact]
    public void GetBackupFolder_CreatesDirectory()
    {
        var service = new FileService();
        var folder = service.GetBackupFolder();

        Assert.True(Directory.Exists(folder));
        Assert.Contains("DotElectric", folder);
        Assert.Contains("Backups", folder);
    }

    [Fact]
    public void CreateBackup_NullPath_ThrowsArgumentException()
    {
        var service = new FileService();
        Assert.Throws<ArgumentException>(() => service.CreateBackup(null!));
    }

    [Fact]
    public void CreateBackup_EmptyPath_ThrowsArgumentException()
    {
        var service = new FileService();
        Assert.Throws<ArgumentException>(() => service.CreateBackup(""));
    }

    [Fact]
    public void CreateBackup_NonExistentFile_ThrowsFileNotFoundException()
    {
        var service = new FileService();
        Assert.Throws<FileNotFoundException>(() => service.CreateBackup("nonexistent.tdel"));
    }

    [Fact]
    public void CreateBackup_CreatesBackupFile()
    {
        var service = new FileService();
        var tempFile = Path.Combine(Path.GetTempPath(), $"test_backup_{Guid.NewGuid():N}.tdel");
        File.WriteAllText(tempFile, "test content");

        try
        {
            service.CreateBackup(tempFile);

            var backupFolder = service.GetBackupFolder();
            var backupFiles = Directory.GetFiles(backupFolder, "test_backup_*.tdel");
            Assert.NotEmpty(backupFiles);

            // Clean up
            foreach (var f in backupFiles) File.Delete(f);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}
