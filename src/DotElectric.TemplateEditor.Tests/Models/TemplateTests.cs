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

    [Fact]
    public void Clone_ReturnsNewInstance_NotSameReference()
    {
        var template = new Template();
        template.Metadata.Name = "Original";
        template.Objects.Add(new Line(0, 0, 1000, 1000));

        var clone = template.Clone();

        Assert.NotSame(template, clone);
        Assert.NotSame(template.Metadata, clone.Metadata);
        Assert.NotSame(template.Sheet, clone.Sheet);
        Assert.NotSame(template.Objects, clone.Objects);
    }

    [Fact]
    public void Clone_CopiesAllObjects_CountMatches()
    {
        var template = new Template();
        template.Objects.Add(new Line(0, 0, 1000, 1000));
        template.Objects.Add(new Rectangle(0, 0, 2000, 2000));
        template.Objects.Add(new Text(5000, 5000, "Hello", 5000));

        var clone = template.Clone();

        Assert.Equal(template.Objects.Count, clone.Objects.Count);
    }

    [Fact]
    public void Clone_CopiesProperties_CorrectValues()
    {
        var template = new Template();
        template.Version = "2.0";
        template.Metadata.Name = "Test";
        template.Metadata.Author = "Author";
        template.Sheet = Sheet.FromFormat("A4");

        var clone = template.Clone();

        Assert.Equal(template.Version, clone.Version);
        Assert.Equal(template.Metadata.Name, clone.Metadata.Name);
        Assert.Equal(template.Metadata.Author, clone.Metadata.Author);
        Assert.Equal(template.Sheet.Format, clone.Sheet.Format);
        Assert.Equal(template.Sheet.WidthMicrons, clone.Sheet.WidthMicrons);
        Assert.Equal(template.Sheet.HeightMicrons, clone.Sheet.HeightMicrons);
    }

    [Fact]
    public void Clone_DeepCopiesObjects_ModificationsIndependent()
    {
        var template = new Template();
        var line = new Line(0, 0, 1000, 1000);
        template.Objects.Add(line);

        var clone = template.Clone();

        // Modify original
        line.Move(5000, 5000);
        template.Objects.Add(new Rectangle(0, 0, 1000, 1000));

        Assert.Single(clone.Objects);
        Assert.Equal(0, clone.Objects[0].MicronsX);
        Assert.Equal(2, template.Objects.Count);
    }

    [Fact]
    public void Clone_CopiesAllPublicProperties_ExceptId()
    {
        // Arrange
        var template = new Template
        {
            Version = "2.0"
        };
        template.Metadata.Name = "RegressionTest";
        template.Metadata.Description = "Check all properties are cloned";
        template.Metadata.Author = "CI";
        template.Metadata.CreatedDate = new DateTime(2026, 1, 1);
        template.Metadata.ModifiedDate = new DateTime(2026, 7, 21);
        template.Sheet = Sheet.FromFormat("A0");
        template.Objects.Add(new Line(100, 200, 3000, 4000, LineType.Dashed, 600, "#FF0000"));
        template.Objects.Add(new Rectangle(500, 600, 2000, 3000, LineType.Solid, 500, "#00FF00", "#0000FF"));
        template.Objects.Add(new Text(1000, 2000, "CloneTest", 3500, "ГОСТ Б", TextType.Dimension, 45,
            "key1", true, "defaultVal", "#FF00FF", true, "Center"));

        // Act
        var clone = template.Clone();

        // Assert
        // Not same references
        Assert.NotSame(template, clone);
        Assert.NotSame(template.Metadata, clone.Metadata);
        Assert.NotSame(template.Sheet, clone.Sheet);
        Assert.NotSame(template.Objects, clone.Objects);

        // Reflection: all public readable properties except DefaultGrid (singleton), Objects (checked separately),
        // Metadata and Sheet (reference types without value equality — checked in detail below)
        var props = typeof(Template).GetProperties()
            .Where(p => p.CanRead && p.Name != "DefaultGrid" && p.Name != "Objects" && p.Name != "Metadata" && p.Name != "Sheet");
        foreach (var prop in props)
        {
            var originalValue = prop.GetValue(template);
            var cloneValue = prop.GetValue(clone);
            Assert.Equal(originalValue, cloneValue);
        }

        // Objects: deep equality, new Ids
        Assert.Equal(template.Objects.Count, clone.Objects.Count);
        for (int i = 0; i < template.Objects.Count; i++)
        {
            Assert.NotSame(template.Objects[i], clone.Objects[i]);
            Assert.NotEqual(template.Objects[i].Id, clone.Objects[i].Id);
        }

        // Metadata deep equality
        Assert.Equal(template.Metadata.Name, clone.Metadata.Name);
        Assert.Equal(template.Metadata.Description, clone.Metadata.Description);
        Assert.Equal(template.Metadata.Author, clone.Metadata.Author);
        Assert.Equal(template.Metadata.CreatedDate, clone.Metadata.CreatedDate);
        Assert.Equal(template.Metadata.ModifiedDate, clone.Metadata.ModifiedDate);

        // Sheet deep equality
        Assert.Equal(template.Sheet.Format, clone.Sheet.Format);
        Assert.Equal(template.Sheet.WidthMicrons, clone.Sheet.WidthMicrons);
        Assert.Equal(template.Sheet.HeightMicrons, clone.Sheet.HeightMicrons);
        Assert.Equal(template.Sheet.Orientation, clone.Sheet.Orientation);

        // DefaultGrid is singleton
        Assert.Same(template.DefaultGrid, clone.DefaultGrid);
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

    [Theory]
    [InlineData("A0", SheetOrientation.Landscape, 1189_000, 841_000)]
    [InlineData("A1", SheetOrientation.Landscape, 841_000, 594_000)]
    [InlineData("A2", SheetOrientation.Landscape, 594_000, 420_000)]
    [InlineData("A3", SheetOrientation.Landscape, 420_000, 297_000)]
    [InlineData("A4", SheetOrientation.Portrait, 210_000, 297_000)]
    [InlineData("A4×2", SheetOrientation.Portrait, 210_000, 594_000)]
    [InlineData("A3×2", SheetOrientation.Portrait, 297_000, 840_000)]
    [InlineData("A2×2", SheetOrientation.Portrait, 420_000, 1_188_000)]
    [InlineData("A1×2", SheetOrientation.Portrait, 594_000, 1_682_000)]
    [InlineData("A0×2", SheetOrientation.Portrait, 841_000, 2_378_000)]
    public void FromFormat_AllStandardFormats_ReturnsCorrectDimensions(
        string format, SheetOrientation expectedOrientation, long expectedWidth, long expectedHeight)
    {
        var sheet = Sheet.FromFormat(format);

        Assert.Equal(format, sheet.Format);
        Assert.Equal(expectedOrientation, sheet.Orientation);
        Assert.Equal(expectedWidth, sheet.WidthMicrons);
        Assert.Equal(expectedHeight, sheet.HeightMicrons);
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

    [Fact]
    public void DefaultConstructor_Defaults_AreCorrect()
    {
        var settings = new GridSettings();

        Assert.True(settings.Enabled);
        Assert.True(settings.SnapEnabled);
        Assert.True(settings.Visible);
        Assert.Equal(5000, settings.StepMicrons);
    }

    [Fact]
    public void Constructor_CustomValues_CanBeSetViaProperties()
    {
        var settings = new GridSettings();

        settings.Enabled = false;
        Assert.False(settings.Enabled);

        settings.SnapEnabled = false;
        Assert.False(settings.SnapEnabled);

        settings.Visible = false;
        Assert.False(settings.Visible);

        settings.StepMicrons = 10000;
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
