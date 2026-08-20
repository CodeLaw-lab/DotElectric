using Moq;

namespace DotElectric.Document.Tests;

public class TemplateServiceTests
{
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly TemplateService _service;
    private static readonly DateTime FixedDate = new(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    public TemplateServiceTests()
    {
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(FixedDate);
        _service = new TemplateService(dateTimeProvider: _dateTimeProviderMock.Object);
    }

    [Fact]
    public void CreateNew_DefaultFormat_CreatesValidTemplate()
    {
        var template = _service.CreateNew("A4");

        Assert.NotNull(template);
        Assert.Equal("A4", template.Sheet.Format);
        Assert.Equal(SheetOrientation.Portrait, template.Sheet.Orientation);
        Assert.Equal(210000, template.Sheet.WidthMicrons);
        Assert.Equal(297000, template.Sheet.HeightMicrons);
        Assert.NotNull(template.Metadata);
        Assert.Empty(template.Objects);
    }

    [Fact]
    public void CreateNew_A3Format_CreatesCorrectDimensions()
    {
        var template = _service.CreateNew("A3");

        Assert.Equal("A3", template.Sheet.Format);
        Assert.Equal(420000, template.Sheet.WidthMicrons);
        Assert.Equal(297000, template.Sheet.HeightMicrons);
    }

    [Fact]
    public void CreateNew_InvalidFormat_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _service.CreateNew("A5"));
    }

    [Fact]
    public void SaveAndLoad_RoundTrip_PreservesData()
    {
        var template = _service.CreateNew("A4");
        template.Metadata.Name = "Round Trip Test";
        template.Metadata.Author = "Test User";
        template.Objects.Add(new Line(0, 0, 10000, 5000, LineType.Dashed));
        template.Objects.Add(new Rectangle(1000, 1000, 5000, 3000, LineType.DashDot));
        template.Objects.Add(new Text(2000, 2000, "Hello", 3500, "ГОСТ А", TextType.Label, 90));

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_template_{Guid.NewGuid():N}.tdel");

        try
        {
            _service.Save(template, tempFile);
            Assert.True(File.Exists(tempFile));

            var loaded = _service.Load(tempFile);

            Assert.Equal("Round Trip Test", loaded.Metadata.Name);
            Assert.Equal("Test User", loaded.Metadata.Author);
            Assert.Equal(template.Sheet.Format, loaded.Sheet.Format);
            Assert.Equal(3, loaded.Objects.Count);

            var line = (Line)loaded.Objects[0];
            Assert.Equal(0, line.StartMicronsX);
            Assert.Equal(0, line.StartMicronsY);
            Assert.Equal(10000, line.EndMicronsX);
            Assert.Equal(5000, line.EndMicronsY);
            Assert.Equal(LineType.Dashed, line.LineType);

            var rect = (Rectangle)loaded.Objects[1];
            Assert.Equal(1000, rect.MicronsX);
            Assert.Equal(1000, rect.MicronsY);
            Assert.Equal(5000, rect.WidthMicrons);
            Assert.Equal(3000, rect.HeightMicrons);
            Assert.Equal(LineType.DashDot, rect.LineType);

            var text = (Text)loaded.Objects[2];
            Assert.Equal(2000, text.MicronsX);
            Assert.Equal(2000, text.MicronsY);
            Assert.Equal("Hello", text.Content);
            Assert.Equal(3500, text.FontSizeMicrons);
            Assert.Equal("ГОСТ А", text.FontName);
            Assert.Equal(TextType.Label, text.TextType);
            Assert.Equal(90, text.RotationAngle);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void Load_NonExistentFile_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() => _service.Load("non_existent_file.tdel"));
    }

    [Fact]
    public void Save_NullTemplate_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _service.Save(null!, "test.tdel"));
    }

    [Fact]
    public void Save_EmptyFilePath_ThrowsArgumentException()
    {
        var template = _service.CreateNew("A4");
        Assert.Throws<ArgumentException>(() => _service.Save(template, ""));
    }

    [Fact]
    public void Save_WithNullFilePath_ThrowsArgumentException()
    {
        var template = _service.CreateNew("A4");
        Assert.Throws<ArgumentException>(() => _service.Save(template, null!));
    }

    [Fact]
    public void Save_CreatesDirectoryIfNotExists()
    {
        var template = _service.CreateNew("A4");
        var nestedDir = Path.Combine(Path.GetTempPath(), $"test_nested_{Guid.NewGuid():N}");
        var tempFile = Path.Combine(nestedDir, "test.tdel");

        try
        {
            _service.Save(template, tempFile);
            Assert.True(File.Exists(tempFile));
        }
        finally
        {
            if (Directory.Exists(nestedDir))
                Directory.Delete(nestedDir, true);
        }
    }

    [Fact]
    public void Save_UpdatesModifiedDate()
    {
        var template = _service.CreateNew("A4");
        var originalDate = template.Metadata.ModifiedDate;

        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(FixedDate.AddHours(1));

        var tempFile = Path.Combine(Path.GetTempPath(), $"test_save_{Guid.NewGuid():N}.tdel");
        try
        {
            _service.Save(template, tempFile);
            Assert.True(template.Metadata.ModifiedDate > originalDate);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    // === CreateFromSheet ===

    [Fact]
    public void CreateFromSheet_CreatesTemplateWithCorrectSheet()
    {
        var sheet = Sheet.Custom(500, 700);
        var template = _service.CreateFromSheet(sheet);

        Assert.NotNull(template);
        Assert.Equal(500_000, template.Sheet.WidthMicrons);
        Assert.Equal(700_000, template.Sheet.HeightMicrons);
        Assert.Equal("Custom", template.Sheet.Format);
    }

    [Fact]
    public void CreateFromSheet_SetsMetadata()
    {
        var sheet = Sheet.Custom(300, 400);
        var template = _service.CreateFromSheet(sheet);

        Assert.Contains("Custom", template.Metadata.Name);
        Assert.Equal(Environment.UserName, template.Metadata.Author);
        Assert.NotEqual(default, template.Metadata.CreatedDate);
    }

    [Fact]
    public void CreateFromSheet_StandardFormat_PreservesFormat()
    {
        var sheet = Sheet.FromFormat("A2");
        var template = _service.CreateFromSheet(sheet);

        Assert.Equal("A2", template.Sheet.Format);
        Assert.Equal(594_000, template.Sheet.WidthMicrons);
        Assert.Equal(420_000, template.Sheet.HeightMicrons);
    }

    // ===== Перенесено из TemplateServiceExtendedTests (приложение) =====

    [Fact]
    public void SaveAndLoad_PreservesMultipleObjectTypes()
    {
        var template = TestTemplates.CreateA3(FixedDate);
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
        var template = TestTemplates.CreateA3(FixedDate);
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
        var template = TestTemplates.CreateA3(FixedDate);
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
}
