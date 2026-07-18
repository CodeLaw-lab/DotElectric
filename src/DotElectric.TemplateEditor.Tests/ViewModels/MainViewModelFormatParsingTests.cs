using System.Reflection;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Tests.ViewModels;

/// <summary>
/// Тесты для парсинга формата и ориентации в MainViewModel.
/// </summary>
public class MainViewModelFormatParsingTests
{
    // === ParseSheetFormat ===

    [Theory]
    [InlineData("A4P", "A4", SheetOrientation.Portrait)]
    [InlineData("A4L", "A4", SheetOrientation.Landscape)]
    [InlineData("A3P", "A3", SheetOrientation.Portrait)]
    [InlineData("A3L", "A3", SheetOrientation.Landscape)]
    [InlineData("A0P", "A0", SheetOrientation.Portrait)]
    [InlineData("A0L", "A0", SheetOrientation.Landscape)]
    [InlineData("A4×2P", "A4×2", SheetOrientation.Portrait)]
    [InlineData("A4×2L", "A4×2", SheetOrientation.Landscape)]
    [InlineData("A3×2P", "A3×2", SheetOrientation.Portrait)]
    [InlineData("A3×2L", "A3×2", SheetOrientation.Landscape)]
    [InlineData("A0×2P", "A0×2", SheetOrientation.Portrait)]
    [InlineData("A0×2L", "A0×2", SheetOrientation.Landscape)]
    [InlineData("A4X2P", "A4X2", SheetOrientation.Portrait)]
    [InlineData("A4X2L", "A4X2", SheetOrientation.Landscape)]
    public void ParseSheetFormat_WithSuffix_ReturnsBaseFormatAndOrientation(
        string input,
        string expectedFormat,
        SheetOrientation expectedOrientation)
    {
        // ParseSheetFormat — private static метод, вызываем через反射
        var method = typeof(MainViewModel).GetMethod("ParseSheetFormat", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var parameters = new object?[] { input, null };
        var result = method!.Invoke(null, parameters);

        Assert.Equal(expectedFormat, result);
        Assert.Equal(expectedOrientation, parameters[1]);
    }

    [Theory]
    [InlineData("A4", "A4")]
    [InlineData("A3", "A3")]
    [InlineData("A0", "A0")]
    [InlineData("A4×2", "A4×2")]
    [InlineData("A3×2", "A3×2")]
    [InlineData("A0×2", "A0×2")]
    [InlineData("A4X2", "A4X2")]
    public void ParseSheetFormat_WithoutSuffix_ReturnsFormatAndNullOrientation(string input, string expectedFormat)
    {
        var method = typeof(MainViewModel).GetMethod("ParseSheetFormat", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var parameters = new object?[] { input, null };
        var result = method!.Invoke(null, parameters);

        Assert.Equal(expectedFormat, result);
        Assert.Null(parameters[1] as SheetOrientation?);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    public void ParseSheetFormat_ShortInput_ReturnsAsIs(string input)
    {
        var method = typeof(MainViewModel).GetMethod("ParseSheetFormat", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var parameters = new object?[] { input, null };
        var result = method!.Invoke(null, parameters);

        Assert.Equal(input, result);
        Assert.Null(parameters[1] as SheetOrientation?);
    }

    [Fact]
    public void ParseSheetFormat_NullInput_ReturnsNull()
    {
        var method = typeof(MainViewModel).GetMethod("ParseSheetFormat", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.NotNull(method);

        var parameters = new object?[] { null, null };
        var result = method!.Invoke(null, parameters);

        Assert.Null(result);
        Assert.Null(parameters[1] as SheetOrientation?);
    }
}
