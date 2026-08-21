namespace DotElectric.Document.Tests;

public class ObjectTypeCatalogTests
{
    [Fact]
    public void All_ContainsExactlyThreeDescriptors()
    {
        Assert.Equal(3, ObjectTypeCatalog.All.Count);
    }

    [Fact]
    public void All_TypeNames_AreLineRectangleText()
    {
        Assert.Equal(
            new[] { "Line", "Rectangle", "Text" },
            ObjectTypeCatalog.All.Select(d => d.TypeName).ToArray());
    }

    [Theory]
    [InlineData("Line", typeof(Line))]
    [InlineData("Rectangle", typeof(Rectangle))]
    [InlineData("Text", typeof(Text))]
    public void TryGet_ByTypeName_ReturnsDescriptorWithMatchingModelType(string typeName, Type modelType)
    {
        Assert.True(ObjectTypeCatalog.TryGet(typeName, out var descriptor));
        Assert.Equal(typeName, descriptor.TypeName);
        Assert.Equal(modelType, descriptor.ModelType);
    }

    [Theory]
    [InlineData("Unknown")]
    [InlineData("line")] // идентичность точная: регистр значим
    [InlineData("LINE")]
    [InlineData("")]
    [InlineData(null)]
    public void TryGet_UnknownTypeName_ReturnsFalse(string? typeName)
    {
        Assert.False(ObjectTypeCatalog.TryGet(typeName, out _));
    }

    [Fact]
    public void TryGet_KnownModelObjects_ReturnsMatchingDescriptors()
    {
        Assert.True(ObjectTypeCatalog.TryGet(new Line(0, 0, 1000, 1000), out var lineDescriptor));
        Assert.Equal("Line", lineDescriptor.TypeName);

        Assert.True(ObjectTypeCatalog.TryGet(new Rectangle(0, 0, 1000, 1000), out var rectDescriptor));
        Assert.Equal("Rectangle", rectDescriptor.TypeName);

        Assert.True(ObjectTypeCatalog.TryGet(new Text(0, 0, "t", 1000), out var textDescriptor));
        Assert.Equal("Text", textDescriptor.TypeName);
    }

    [Fact]
    public void TryGet_UnknownModelSubtype_ReturnsFalse()
    {
        Assert.False(ObjectTypeCatalog.TryGet(new TestTemplateObject("x", 0, 0), out _));
    }
}
