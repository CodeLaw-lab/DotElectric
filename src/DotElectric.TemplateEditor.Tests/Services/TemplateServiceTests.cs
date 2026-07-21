using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.Services;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Services;

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
    public void Validate_ValidTemplate_ReturnsNoErrors()
    {
        var template = _service.CreateNew("A4");
        var errors = _service.Validate(template).ToList();
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_TemplateWithErrors_ReturnsErrors()
    {
        var template = _service.CreateNew("A4");
        template.Objects.Add(new Line(999000, 0, 1000000, 0)); // за пределами A4 (297мм)

        var errors = _service.Validate(template).ToList();
        Assert.NotEmpty(errors);
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
}
