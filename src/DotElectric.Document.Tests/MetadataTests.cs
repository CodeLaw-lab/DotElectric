namespace DotElectric.Document.Tests;

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
