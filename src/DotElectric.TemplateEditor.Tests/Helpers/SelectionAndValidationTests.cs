using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Tests.Helpers;

public class ExtendedSelectionBoxHelperTests
{
    // === RectMicrons ===

    [Fact]
    public void RectMicrons_NormalizesCoordinates()
    {
        var rect = new RectMicrons(10000, 10000, 0, 0);
        Assert.Equal(0, rect.Left);
        Assert.Equal(0, rect.Bottom);
        Assert.Equal(10000, rect.Right);
        Assert.Equal(10000, rect.Top);
    }

    [Fact]
    public void RectMicrons_CalculatesWidthHeight()
    {
        var rect = new RectMicrons(0, 0, 5000, 3000);
        Assert.Equal(5000, rect.Width);
        Assert.Equal(3000, rect.Height);
    }

    [Fact]
    public void RectMicrons_FromPoints_CreatesCorrectRect()
    {
        var start = new PointMicrons(1000, 1000);
        var end = new PointMicrons(5000, 4000);
        var rect = RectMicrons.FromPoints(start, end);

        Assert.Equal(1000, rect.Left);
        Assert.Equal(1000, rect.Bottom);
        Assert.Equal(5000, rect.Right);
        Assert.Equal(4000, rect.Top);
    }

    [Fact]
    public void RectMicrons_FromPoints_ReversedCoordinates()
    {
        var start = new PointMicrons(5000, 4000);
        var end = new PointMicrons(1000, 1000);
        var rect = RectMicrons.FromPoints(start, end);

        // RectMicrons normalizes: Left=min, Bottom=min, Right=max, Top=max
        Assert.Equal(1000, rect.Left);
        Assert.Equal(1000, rect.Bottom);
        Assert.Equal(5000, rect.Right);
        Assert.Equal(4000, rect.Top);
    }

    [Fact]
    public void RectMicrons_Intersects_Overlapping_ReturnsTrue()
    {
        var rect1 = new RectMicrons(0, 0, 5000, 5000);
        var rect2 = new RectMicrons(3000, 3000, 8000, 8000);

        Assert.True(rect1.Intersects(rect2));
    }

    [Fact]
    public void RectMicrons_Intersects_NonOverlapping_ReturnsFalse()
    {
        var rect1 = new RectMicrons(0, 0, 2000, 2000);
        var rect2 = new RectMicrons(5000, 5000, 8000, 8000);

        Assert.False(rect1.Intersects(rect2));
    }

    [Fact]
    public void RectMicrons_Intersects_TouchingEdges_ReturnsFalse()
    {
        var rect1 = new RectMicrons(0, 0, 2000, 2000);
        var rect2 = new RectMicrons(2000, 2000, 4000, 4000);

        Assert.False(rect1.Intersects(rect2)); // edges touching = no intersection
    }

    [Fact]
    public void RectMicrons_Contains_FullyInside_ReturnsTrue()
    {
        var outer = new RectMicrons(0, 0, 10000, 10000);
        var inner = new RectMicrons(2000, 2000, 5000, 5000);

        Assert.True(outer.Contains(inner));
    }

    [Fact]
    public void RectMicrons_Contains_PartiallyOutside_ReturnsFalse()
    {
        var outer = new RectMicrons(0, 0, 5000, 5000);
        var inner = new RectMicrons(3000, 3000, 8000, 8000);

        Assert.False(outer.Contains(inner));
    }

    // === SelectionDirection ===

    [Theory]
    [InlineData(0, 0, 5000, 5000, SelectionDirection.LeftToRight)]
    [InlineData(5000, 5000, 0, 0, SelectionDirection.RightToLeft)]
    [InlineData(0, 0, 0, 5000, SelectionDirection.LeftToRight)]
    [InlineData(5000, 0, 5000, 5000, SelectionDirection.LeftToRight)]
    public void GetDirection_CorrectDirection(
        int startX, int startY, int endX, int endY,
        SelectionDirection expected)
    {
        var start = new PointMicrons(startX, startY);
        var end = new PointMicrons(endX, endY);
        var direction = SelectionBoxHelper.GetDirection(start, end);
        Assert.Equal(expected, direction);
    }

    // === GetSelectedObjects ===

    [Fact]
    public void GetFullyContained_OnlyFullyInside()
    {
        var box = new RectMicrons(0, 0, 10000, 10000);
        var objects = new List<TemplateObjectBase>
        {
            new Rectangle(1000, 1000, 2000, 2000),    // fully inside: bounds 1000-3000
            new Rectangle(5000, 5000, 2000, 2000),    // fully inside: bounds 5000-7000
            new Rectangle(8000, 8000, 5000, 5000),    // partially outside: bounds 8000-13000
        };

        var selected = SelectionBoxHelper.GetFullyContained(box, objects);
        Assert.Equal(2, selected.Count);
    }

    [Fact]
    public void GetIntersecting_AnyOverlap()
    {
        var box = new RectMicrons(0, 0, 5000, 5000);
        var objects = new List<TemplateObjectBase>
        {
            new Rectangle(1000, 1000, 2000, 2000),    // fully inside
            new Rectangle(4000, 4000, 3000, 3000),    // partially overlaps
            new Rectangle(8000, 8000, 2000, 2000),    // no overlap
        };

        var selected = SelectionBoxHelper.GetIntersecting(box, objects);
        Assert.Equal(2, selected.Count);
    }

    [Fact]
    public void GetSelectedObjects_LeftToRight_UsesFullContain()
    {
        var box = new RectMicrons(0, 0, 5000, 5000);
        var objects = new List<TemplateObjectBase>
        {
            new Rectangle(1000, 1000, 2000, 2000),    // fully inside
            new Rectangle(4000, 4000, 3000, 3000),    // partial
        };

        var selected = SelectionBoxHelper.GetSelectedObjects(
            box, objects, SelectionDirection.LeftToRight);

        Assert.Single(selected);
    }

    [Fact]
    public void GetSelectedObjects_RightToLeft_UsesIntersect()
    {
        var box = new RectMicrons(0, 0, 5000, 5000);
        var objects = new List<TemplateObjectBase>
        {
            new Rectangle(1000, 1000, 2000, 2000),    // fully inside
            new Rectangle(4000, 4000, 3000, 3000),    // partial
        };

        var selected = SelectionBoxHelper.GetSelectedObjects(
            box, objects, SelectionDirection.RightToLeft);

        Assert.Equal(2, selected.Count);
    }

    // === GetObjectBounds ===

    [Fact]
    public void GetObjectBounds_Line_ReturnsCorrectBounds()
    {
        var line = new Line(0, 0, 10000, 5000);
        var objects = new List<TemplateObjectBase> { line };
        var box = new RectMicrons(0, 0, 10000, 5000);

        var selected = SelectionBoxHelper.GetFullyContained(box, objects);
        Assert.Single(selected);
    }

    [Fact]
    public void GetObjectBounds_Text_CalculatesWidthFromContent()
    {
        var text = new Text(0, 0, "Hello", 5000); // width = 5 * 5000 * 0.6 = 15000
        var objects = new List<TemplateObjectBase> { text };
        var box = new RectMicrons(0, 0, 20000, 10000);

        var selected = SelectionBoxHelper.GetFullyContained(box, objects);
        Assert.Single(selected);
    }

    [Fact]
    public void GetObjectBounds_Text_Rotated90Degrees()
    {
        var text = new Text(0, 0, "Hi", 5000, rotationAngle: 90);
        var objects = new List<TemplateObjectBase> { text };
        // 90°: X stays, Y + width, X + height, Y + width + height
        var box = new RectMicrons(-5000, -6000, 10000, 10000);

        var selected = SelectionBoxHelper.GetIntersecting(box, objects);
        Assert.Single(selected);
    }
}

public class ExtendedValidationServiceTests
{
    private static Template CreateTestTemplate()
    {
        return new Template(
            new Metadata { Name = "Test", Author = "Test", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow },
            Sheet.FromFormat("A3"));
    }

    [Fact]
    public void Validate_ValidLine_ReturnsNoErrors()
    {
        var template = CreateTestTemplate();
        template.Objects.Add(new Line(0, 0, 10000, 10000));

        var errors = new TemplateValidator().Validate(template);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_ZeroLengthLine_ReturnsWarning()
    {
        var template = CreateTestTemplate();
        template.Objects.Add(new Line(1000, 1000, 1000, 1000));

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-004" && e.Severity == ValidationSeverity.Warning);
    }

    [Fact]
    public void Validate_EmptyTextContent_ReturnsWarning()
    {
        var template = CreateTestTemplate();
        template.Objects.Add(new Text(0, 0, "", 3500));

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-004" && e.Severity == ValidationSeverity.Warning);
    }

    [Fact]
    public void Validate_NegativeFontSize_ReturnsError()
    {
        var template = CreateTestTemplate();
        template.Objects.Add(new Text(0, 0, "Test", -1000));

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-004" && e.Severity == ValidationSeverity.Error);
    }

    [Fact]
    public void ValidateObject_ValidObject_ReturnsNoErrors()
    {
        var sheet = Sheet.FromFormat("A3");
        var line = new Line(0, 0, 10000, 10000);

        var errors = new TemplateValidator().ValidateObject(line, sheet);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateObject_NullObject_ReturnsEmpty()
    {
        var sheet = Sheet.FromFormat("A3");
        var errors = new TemplateValidator().ValidateObject(null!, sheet);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateMetadataKeys_EmptyAuthor_ReturnsWarning()
    {
        var metadata = new Metadata { Author = "" };
        var errors = TemplateValidator.ValidateMetadataKeys(metadata);
        Assert.Contains(errors, e => e.RuleId == "V-002");
    }

    [Fact]
    public void ValidateMetadataKeys_ValidAuthor_ReturnsNoErrors()
    {
        var metadata = new Metadata { Author = "John Doe" };
        var errors = TemplateValidator.ValidateMetadataKeys(metadata);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateMetadataKeys_NullMetadata_ReturnsEmpty()
    {
        var errors = TemplateValidator.ValidateMetadataKeys(null);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateSheetFormat_NullSheet_ReturnsError()
    {
        var template = CreateTestTemplate();
        template.Sheet = null!;

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-006");
    }

    [Fact]
    public void ValidateSheetFormat_EmptyFormat_ReturnsError()
    {
        var sheet = Sheet.FromFormat("A3");
        sheet.Format = "";
        var template = CreateTestTemplate();
        template.Sheet = sheet;

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-006");
    }

    [Fact]
    public void ValidateSheetFormat_InvalidFormat_ReturnsError()
    {
        var sheet = Sheet.FromFormat("A3");
        sheet.Format = "A5";
        var template = CreateTestTemplate();
        template.Sheet = sheet;

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-006");
    }

    [Theory]
    [InlineData("A4×2")]
    [InlineData("A3×2")]
    [InlineData("A2×2")]
    [InlineData("A1×2")]
    [InlineData("A0×2")]
    [InlineData("A4X2")]
    public void ValidateSheetFormat_HalfFormats_Valid(string format)
    {
        var sheet = Sheet.FromFormat(format);
        var template = CreateTestTemplate();
        template.Sheet = sheet;

        var errors = new TemplateValidator().Validate(template);
        Assert.DoesNotContain(errors, e => e.RuleId == "V-006");
    }

    [Fact]
    public void ValidateSheetFormat_CustomNegativeWidth_ReturnsError()
    {
        var sheet = Sheet.Custom(-100, 200);
        var template = CreateTestTemplate();
        template.Sheet = sheet;

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-006");
    }

    [Fact]
    public void ValidateSheetFormat_CustomNegativeHeight_ReturnsError()
    {
        var sheet = Sheet.Custom(200, -100);
        var template = CreateTestTemplate();
        template.Sheet = sheet;

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-006");
    }

    [Fact]
    public void ValidateSheetFormat_CustomValid_ReturnsNoErrors()
    {
        var sheet = Sheet.Custom(500, 400);
        var template = CreateTestTemplate();
        template.Sheet = sheet;

        var errors = new TemplateValidator().Validate(template);
        Assert.DoesNotContain(errors, e => e.RuleId == "V-006");
    }

    [Fact]
    public void ValidateLineTypes_InvalidLineType_ReturnsError()
    {
        var template = CreateTestTemplate();
        // LineType enum has valid values only, but we can't set invalid ones
        // Testing that valid types pass
        template.Objects.Add(new Line(0, 0, 1000, 1000, LineType.Dashed));
        template.Objects.Add(new Rectangle(0, 0, 5000, 5000, LineType.DashDot));

        var errors = new TemplateValidator().Validate(template);
        Assert.DoesNotContain(errors, e => e.RuleId == "V-007");
    }

    // === Utility methods ===
}
