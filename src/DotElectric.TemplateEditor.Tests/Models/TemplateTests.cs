using System.IO;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.Services;

namespace DotElectric.TemplateEditor.Tests.Models;

public class TemplateTests
{
    [Fact]
    public void Constructor_Default_CreatesValidTemplate()
    {
        var template = new Template();

        Assert.NotNull(template.Metadata);
        Assert.NotNull(template.Sheet);
        Assert.NotNull(template.Objects);
        Assert.Equal("1.0", template.Version);
        Assert.NotNull(template.DefaultGrid);
    }

    [Fact]
    public void Constructor_WithParameters_SetsProperties()
    {
        var metadata = new Metadata { Name = "Test", Author = "User" };
        var sheet = Sheet.FromFormat("A4");
        var template = new Template(metadata, sheet);

        Assert.Same(metadata, template.Metadata);
        Assert.Same(sheet, template.Sheet);
        Assert.NotNull(template.Objects);
    }

    [Fact]
    public void DefaultGrid_IsSingleton5mm()
    {
        var template = new Template();
        Assert.Equal(5000, template.DefaultGrid.StepMicrons);
    }

    [Fact]
    public void Objects_IsObservableCollection()
    {
        var template = new Template();
        Assert.IsType<System.Collections.ObjectModel.ObservableCollection<TemplateObjectBase>>(template.Objects);
    }

    [Fact]
    public void Objects_CanAddAndRemove()
    {
        var template = new Template();
        var line = new Line(0, 0, 1000, 1000);

        template.Objects.Add(line);
        Assert.Single(template.Objects);

        template.Objects.Remove(line);
        Assert.Empty(template.Objects);
    }

    [Fact]
    public void Constructor_Default_CreatesA3Template()
    {
        var template = new Template();

        Assert.Equal("1.0", template.Version);
        Assert.Equal("A3", template.Sheet.Format);
        Assert.NotNull(template.Metadata);
        Assert.NotNull(template.Objects);
        Assert.NotNull(template.DefaultGrid);
    }

    [Fact]
    public void Constructor_WithParameters_SetsCorrectly()
    {
        var metadata = new Metadata { Name = "My Template", Author = "Author" };
        var sheet = Sheet.FromFormat("A4");
        var template = new Template(metadata, sheet);

        Assert.Same(metadata, template.Metadata);
        Assert.Same(sheet, template.Sheet);
        Assert.Equal("1.0", template.Version);
    }

    [Fact]
    public void DefaultGrid_IsSingleton()
    {
        var t1 = new Template();
        var t2 = new Template();
        Assert.Same(t1.DefaultGrid, t2.DefaultGrid);
    }

    [Fact]
    public void DefaultGrid_Has5mmStep()
    {
        var template = new Template();
        Assert.Equal(5000, template.DefaultGrid.StepMicrons);
    }
}

public class SheetTests
{
    [Theory]
    [InlineData("A0", 1189000, 841000)]
    [InlineData("A1", 841000, 594000)]
    [InlineData("A2", 594000, 420000)]
    [InlineData("A3", 420000, 297000)]
    [InlineData("A4", 210000, 297000)]
    public void FromFormat_ValidFormat_SetsCorrectDimensions(string format, long expectedWidth, long expectedHeight)
    {
        var sheet = Sheet.FromFormat(format);

        Assert.Equal(format, sheet.Format);
        Assert.Equal(expectedWidth, sheet.WidthMicrons);
        Assert.Equal(expectedHeight, sheet.HeightMicrons);
    }

    [Fact]
    public void FromFormat_InvalidFormat_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Sheet.FromFormat("A5"));
    }

    [Fact]
    public void FromFormat_LowerCase_Works()
    {
        var sheet = Sheet.FromFormat("a3");
        Assert.Equal("A3", sheet.Format);
    }

    [Fact]
    public void Custom_SetsCustomFormatAndDimensions()
    {
        var sheet = Sheet.Custom(500.0, 350.0);

        Assert.Equal("Custom", sheet.Format);
        Assert.Equal(500000, sheet.WidthMicrons);
        Assert.Equal(350000, sheet.HeightMicrons);
    }

    [Fact]
    public void WidthMm_ReturnsCorrectValue()
    {
        var sheet = Sheet.FromFormat("A4");
        Assert.Equal(210.0, sheet.WidthMm, tolerance: 0.001);
    }

    [Fact]
    public void HeightMm_ReturnsCorrectValue()
    {
        var sheet = Sheet.FromFormat("A4");
        Assert.Equal(297.0, sheet.HeightMm, tolerance: 0.001);
    }

    [Fact]
    public void Unit_AlwaysMm()
    {
        var sheet = Sheet.FromFormat("A4");
        Assert.Equal("mm", sheet.Unit);
    }

    [Theory]
    [InlineData("A4×2", 594000, 210000)]
    [InlineData("A3×2", 840000, 297000)]
    [InlineData("A2×2", 1188000, 420000)]
    [InlineData("A1×2", 1682000, 594000)]
    [InlineData("A0×2", 2378000, 841000)]
    public void FromFormat_HalfFormats_CorrectDimensions(string format, long expectedWide, long expectedNarrow)
    {
        var sheet = Sheet.FromFormat(format);
        Assert.Equal(format, sheet.Format);
        Assert.Equal(SheetOrientation.Portrait, sheet.Orientation);
        Assert.Equal(expectedNarrow, sheet.WidthMicrons);
        Assert.Equal(expectedWide, sheet.HeightMicrons);
    }

    [Theory]
    [InlineData("a4×2")]
    [InlineData("A4X2")]
    [InlineData("a4x2")]
    [InlineData("A4x2")]
    public void FromFormat_HalfFormat_CaseInsensitive_NormalizesToUnicode(string input)
    {
        var sheet = Sheet.FromFormat(input);
        Assert.Equal("A4×2", sheet.Format);
    }
}

public class MetadataTests
{
    [Fact]
    public void Constructor_Default_SetsEmptyStrings()
    {
        var metadata = new Metadata();

        Assert.Empty(metadata.Name);
        Assert.Empty(metadata.Description);
        Assert.Empty(metadata.Author);
        Assert.Equal(default, metadata.CreatedDate);
        Assert.Equal(default, metadata.ModifiedDate);
    }

    [Fact]
    public void Properties_CanBeModified()
    {
        var metadata = new Metadata();
        metadata.Name = "Test Template";
        metadata.Author = "John Doe";
        metadata.Description = "A test template";

        Assert.Equal("Test Template", metadata.Name);
        Assert.Equal("John Doe", metadata.Author);
        Assert.Equal("A test template", metadata.Description);
    }
}

public class GridTests
{
    [Fact]
    public void Default_Has5mmStep()
    {
        Assert.Equal(5000, Grid.Default.StepMicrons);
    }

    [Fact]
    public void Default_IsSingleton()
    {
        var instance1 = Grid.Default;
        var instance2 = Grid.Default;
        Assert.Same(instance1, instance2);
    }
}

public class GridSettingsTests
{
    [Fact]
    public void FromDefaultGrid_CopiesStepAndEnablesAll()
    {
        var settings = GridSettings.FromDefaultGrid();

        Assert.True(settings.Enabled);
        Assert.True(settings.SnapEnabled);
        Assert.True(settings.Visible);
        Assert.Equal(5000, settings.StepMicrons);
    }

    [Fact]
    public void Properties_CanBeModified()
    {
        var settings = GridSettings.FromDefaultGrid();

        settings.Enabled = false;
        settings.SnapEnabled = false;
        settings.Visible = false;
        settings.StepMicrons = 10000;

        Assert.False(settings.Enabled);
        Assert.False(settings.SnapEnabled);
        Assert.False(settings.Visible);
        Assert.Equal(10000, settings.StepMicrons);
    }
}

public class MockTemplateFileService : IFileService
{
    public string TemplatesFolder { get; }

    public MockTemplateFileService()
    {
        TemplatesFolder = Path.Combine(Path.GetTempPath(), $"TemplateLibTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(TemplatesFolder);
    }

    public string? OpenFileDialog(string filter) => null;
    public string? SaveFileDialog(string filter, string defaultFileName) => null;
    public string GetTemplatesFolder() => TemplatesFolder;
    public string GetBackupFolder() => Path.Combine(Path.GetTempPath(), "BackupTest");
    public void CreateBackup(string sourcePath) { }
}
