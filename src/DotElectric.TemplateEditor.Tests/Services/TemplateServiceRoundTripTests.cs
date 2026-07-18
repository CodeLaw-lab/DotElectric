using System.IO;
using System.IO.Compression;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.Services;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Services.RoundTrip;

public class TemplateServiceRoundTripTests
{
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock;
    private readonly TemplateService _service;
    private static readonly DateTime FixedDate = new(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    public TemplateServiceRoundTripTests()
    {
        _dateTimeProviderMock = new Mock<IDateTimeProvider>();
        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(FixedDate);
        _service = new TemplateService(dateTimeProvider: _dateTimeProviderMock.Object);
    }

    [Fact]
    public void CreateNew_Default_CreatesA3Template()
    {
        var template = _service.CreateNew();

        Assert.Equal("A3", template.Sheet.Format);
        Assert.Equal(420_000, template.Sheet.WidthMicrons);
        Assert.Equal(297_000, template.Sheet.HeightMicrons);
        Assert.NotNull(template.Metadata);
        Assert.NotEmpty(template.Metadata.Name);
        Assert.Empty(template.Objects);
    }

    [Theory]
    [InlineData("A0", 1189_000, 841_000)]
    [InlineData("A1", 841_000, 594_000)]
    [InlineData("A2", 594_000, 420_000)]
    [InlineData("A3", 420_000, 297_000)]
    [InlineData("A4", 210_000, 297_000)]
    public void CreateNew_DifferentFormats_CreatesCorrectSheet(string format, long expectedW, long expectedH)
    {
        var template = _service.CreateNew(format);

        Assert.Equal(format, template.Sheet.Format);
        Assert.Equal(expectedW, template.Sheet.WidthMicrons);
        Assert.Equal(expectedH, template.Sheet.HeightMicrons);
    }

    [Fact]
    public void SaveAndLoad_RoundTrip_PreservesAllData()
    {
        var template = CreateTestTemplate();
        var filePath = Path.Combine(Path.GetTempPath(), $"test_template_{Guid.NewGuid():N}.tdel");

        try
        {
            _service.Save(template, filePath);
            Assert.True(File.Exists(filePath));

            var loaded = _service.Load(filePath);

            Assert.Equal(template.Sheet.Format, loaded.Sheet.Format);
            Assert.Equal(template.Sheet.WidthMicrons, loaded.Sheet.WidthMicrons);
            Assert.Equal(template.Metadata.Name, loaded.Metadata.Name);
            Assert.Equal(template.Metadata.Author, loaded.Metadata.Author);
            Assert.Equal(template.Objects.Count, loaded.Objects.Count);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void SaveAndLoad_PreservesLineObjects()
    {
        var template = CreateTestTemplate();
        template.Objects.Add(new Line(1000, 2000, 5000, 6000, LineType.Dashed));
        template.Objects.Add(new Line(0, 0, 10000, 10000, LineType.Solid));

        var filePath = Path.Combine(Path.GetTempPath(), $"test_lines_{Guid.NewGuid():N}.tdel");

        try
        {
            _service.Save(template, filePath);
            var loaded = _service.Load(filePath);

            Assert.Equal(2, loaded.Objects.Count);
            var line1 = (Line)loaded.Objects[0];
            Assert.Equal(1000, line1.StartMicronsX);
            Assert.Equal(2000, line1.StartMicronsY);
            Assert.Equal(5000, line1.EndMicronsX);
            Assert.Equal(6000, line1.EndMicronsY);
            Assert.Equal(LineType.Dashed, line1.LineType);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void SaveAndLoad_PreservesRectangleObjects()
    {
        var template = CreateTestTemplate();
        template.Objects.Add(new Rectangle(1000, 2000, 5000, 3000, LineType.DashDot));

        var filePath = Path.Combine(Path.GetTempPath(), $"test_rect_{Guid.NewGuid():N}.tdel");

        try
        {
            _service.Save(template, filePath);
            var loaded = _service.Load(filePath);

            var rect = (Rectangle)loaded.Objects[0];
            Assert.Equal(1000, rect.MicronsX);
            Assert.Equal(2000, rect.MicronsY);
            Assert.Equal(5000, rect.WidthMicrons);
            Assert.Equal(3000, rect.HeightMicrons);
            Assert.Equal(LineType.DashDot, rect.LineType);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void SaveAndLoad_PreservesTextObjects()
    {
        var template = CreateTestTemplate();
        template.Objects.Add(new Text(5000, 3000, "Hello World", 5000, "ГОСТ Б", TextType.Label, 90));

        var filePath = Path.Combine(Path.GetTempPath(), $"test_text_{Guid.NewGuid():N}.tdel");

        try
        {
            _service.Save(template, filePath);
            var loaded = _service.Load(filePath);

            var text = (Text)loaded.Objects[0];
            Assert.Equal(5000, text.MicronsX);
            Assert.Equal(3000, text.MicronsY);
            Assert.Equal("Hello World", text.Content);
            Assert.Equal(5000, text.FontSizeMicrons);
            Assert.Equal("ГОСТ Б", text.FontName);
            Assert.Equal(TextType.Label, text.TextType);
            Assert.Equal(90, text.RotationAngle);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void Save_CreatesDirectoryIfNeeded()
    {
        var template = CreateTestTemplate();
        var dirPath = Path.Combine(Path.GetTempPath(), $"test_dir_{Guid.NewGuid():N}");
        var filePath = Path.Combine(dirPath, "test.tdel");

        try
        {
            _service.Save(template, filePath);
            Assert.True(Directory.Exists(dirPath));
            Assert.True(File.Exists(filePath));
        }
        finally
        {
            if (Directory.Exists(dirPath)) Directory.Delete(dirPath, true);
        }
    }

    [Fact]
    public void Save_UpdatesModifiedDate()
    {
        var template = CreateTestTemplate();
        var oldDate = template.Metadata.ModifiedDate;
        var filePath = Path.Combine(Path.GetTempPath(), $"test_save_{Guid.NewGuid():N}.tdel");

        _dateTimeProviderMock.Setup(p => p.UtcNow).Returns(FixedDate.AddHours(1));

        try
        {
            _service.Save(template, filePath);
            Assert.True(template.Metadata.ModifiedDate > oldDate);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void Save_NullTemplate_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _service.Save(null!, "test.tdel"));
    }

    [Fact]
    public void Save_EmptyPath_ThrowsArgumentException()
    {
        var template = CreateTestTemplate();
        Assert.Throws<ArgumentException>(() => _service.Save(template, ""));
    }

    [Fact]
    public void Load_NonExistentFile_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() => _service.Load("nonexistent.tdel"));
    }

    [Fact]
    public void Load_InvalidFile_ThrowsInvalidDataException()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"invalid_{Guid.NewGuid():N}.tdel");
        File.WriteAllText(filePath, "not a zip file");

        try
        {
            Assert.Throws<InvalidDataException>(() => _service.Load(filePath));
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void Load_ZipWithoutXml_ThrowsInvalidDataException()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"noxml_{Guid.NewGuid():N}.tdel");
        using (var archive = ZipFile.Open(filePath, ZipArchiveMode.Create))
        {
            archive.CreateEntry("other.txt");
        }

        try
        {
            Assert.Throws<InvalidDataException>(() => _service.Load(filePath));
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void Validate_ValidTemplate_ReturnsNoErrors()
    {
        var template = CreateTestTemplate();
        template.Objects.Add(new Line(0, 0, 10000, 10000));

        var errors = _service.Validate(template);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_NullTemplate_ReturnsError()
    {
        var errors = _service.Validate(null!);
        Assert.NotEmpty(errors);
    }

    [Fact(Skip = "Id is init-only — cannot set duplicates with current model")]
    public void Validate_DuplicateIds_ReturnsError()
    {
        // Testing V-001 requires two objects with same Id, which is impossible with current init-only design.
        // When model changes to allow duplicate IDs, unskip and implement:
        //    var errors = _service.Validate(template);
        //    Assert.Contains(errors, e => e.Code == "V-001");
    }

    [Fact]
    public void SaveAndLoad_PreservesLineStrokeColor()
    {
        var template = CreateTestTemplate();
        template.Objects.Add(new Line(0, 0, 1000, 1000, strokeColor: "#FF0000"));

        var filePath = Path.Combine(Path.GetTempPath(), $"test_line_color_{Guid.NewGuid():N}.tdel");

        try
        {
            _service.Save(template, filePath);
            var loaded = _service.Load(filePath);

            var line = (Line)loaded.Objects[0];
            Assert.Equal("#FF0000", line.StrokeColor);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void SaveAndLoad_PreservesRectangleColors()
    {
        var template = CreateTestTemplate();
        template.Objects.Add(new Rectangle(0, 0, 1000, 1000, strokeColor: "#00FF00", fillColor: "#0000FF"));

        var filePath = Path.Combine(Path.GetTempPath(), $"test_rect_colors_{Guid.NewGuid():N}.tdel");

        try
        {
            _service.Save(template, filePath);
            var loaded = _service.Load(filePath);

            var rect = (Rectangle)loaded.Objects[0];
            Assert.Equal("#00FF00", rect.StrokeColor);
            Assert.Equal("#0000FF", rect.FillColor);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void SaveAndLoad_PreservesTextForeground()
    {
        var template = CreateTestTemplate();
        template.Objects.Add(new Text(0, 0, "Test", 2500, foreground: "#FF00FF"));

        var filePath = Path.Combine(Path.GetTempPath(), $"test_text_fg_{Guid.NewGuid():N}.tdel");

        try
        {
            _service.Save(template, filePath);
            var loaded = _service.Load(filePath);

            var text = (Text)loaded.Objects[0];
            Assert.Equal("#FF00FF", text.Foreground);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void SaveAndLoad_PreservesDefaultColors()
    {
        var template = CreateTestTemplate();
        template.Objects.Add(new Line(0, 0, 1000, 1000));
        template.Objects.Add(new Rectangle(0, 0, 1000, 1000));
        template.Objects.Add(new Text(0, 0, "Test", 2500));

        var filePath = Path.Combine(Path.GetTempPath(), $"test_def_colors_{Guid.NewGuid():N}.tdel");

        try
        {
            _service.Save(template, filePath);
            var loaded = _service.Load(filePath);

            Assert.Equal("#000000", ((Line)loaded.Objects[0]).StrokeColor);
            Assert.Equal("#000000", ((Rectangle)loaded.Objects[1]).StrokeColor);
            Assert.Equal("Transparent", ((Rectangle)loaded.Objects[1]).FillColor);
            Assert.Equal("#000000", ((Text)loaded.Objects[2]).Foreground);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void SaveAndLoad_PreservesTextWrapping()
    {
        var template = CreateTestTemplate();
        template.Objects.Add(new Text(0, 0, "Test", 2500, textWrapping: true));

        var filePath = Path.Combine(Path.GetTempPath(), $"test_text_wrap_{Guid.NewGuid():N}.tdel");

        try
        {
            _service.Save(template, filePath);
            var loaded = _service.Load(filePath);

            var text = (Text)loaded.Objects[0];
            Assert.True(text.TextWrapping);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void SaveAndLoad_PreservesTextAlignment()
    {
        var template = CreateTestTemplate();
        template.Objects.Add(new Text(0, 0, "Test", 2500, textAlignment: "Right"));

        var filePath = Path.Combine(Path.GetTempPath(), $"test_text_align_{Guid.NewGuid():N}.tdel");

        try
        {
            _service.Save(template, filePath);
            var loaded = _service.Load(filePath);

            var text = (Text)loaded.Objects[0];
            Assert.Equal("Right", text.TextAlignment);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void SaveAndLoad_PreservesTextWrappingAlignmentDefaults()
    {
        var template = CreateTestTemplate();
        template.Objects.Add(new Text(0, 0, "Test", 2500));

        var filePath = Path.Combine(Path.GetTempPath(), $"test_text_wrap_align_def_{Guid.NewGuid():N}.tdel");

        try
        {
            _service.Save(template, filePath);
            var loaded = _service.Load(filePath);

            var text = (Text)loaded.Objects[0];
            Assert.False(text.TextWrapping);
            Assert.Equal("Left", text.TextAlignment);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void Validate_ObjectOutOfBounds_ReturnsError()
    {
        var template = CreateTestTemplate(); // A3: 420x297mm
        template.Objects.Add(new Line(1000_000, 1000_000, 1001_000, 1001_000)); // way outside

        var errors = _service.Validate(template);
        Assert.NotEmpty(errors);
    }

    private static Template CreateTestTemplate()
    {
        return new Template(
            new Metadata { Name = "Test", Author = "TestUser", CreatedDate = FixedDate, ModifiedDate = FixedDate },
            Sheet.FromFormat("A3"));
    }
}
