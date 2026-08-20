namespace DotElectric.Document.Tests;

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

        // Reflection: all public readable properties except Objects (checked separately),
        // Metadata and Sheet (reference types without value equality — checked in detail below)
        var props = typeof(Template).GetProperties()
            .Where(p => p.CanRead && p.Name != "Objects" && p.Name != "Metadata" && p.Name != "Sheet");
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
    }
}
