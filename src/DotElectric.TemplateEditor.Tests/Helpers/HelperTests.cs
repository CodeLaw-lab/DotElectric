using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Tests.Helpers;

public class AdditionalValidationServiceTests
{
    [Fact]
    public void Validate_DuplicateIds_ReturnsV001Error()
    {
        var template = CreateTestTemplate();
        var line1 = new Line(0, 0, 1000, 1000);
        var line2 = new Line(2000, 2000, 3000, 3000);
        // Force duplicate
        SetId(line2, line1.Id);
        template.Objects.Add(line1);
        template.Objects.Add(line2);

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-001" && e.Severity == ValidationSeverity.Error);
    }

    [Fact]
    public void Validate_DuplicateTextKeys_ReturnsV002Error()
    {
        var template = CreateTestTemplate();
        var text1 = new Text(0, 0, "Field 1", 3500, key: "field_a", isEditable: true);
        var text2 = new Text(1000, 0, "Field 2", 3500, key: "field_a", isEditable: true);
        template.Objects.Add(text1);
        template.Objects.Add(text2);

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-002");
    }

    [Fact]
    public void Validate_UniqueTextKeys_NoV002Error()
    {
        var template = CreateTestTemplate();
        var text1 = new Text(0, 0, "Field 1", 3500, key: "field_a", isEditable: true);
        var text2 = new Text(1000, 0, "Field 2", 3500, key: "field_b", isEditable: true);
        template.Objects.Add(text1);
        template.Objects.Add(text2);

        var errors = new TemplateValidator().Validate(template);
        Assert.DoesNotContain(errors, e => e.RuleId == "V-002");
    }

    [Fact]
    public void Validate_NonEditableTextKeys_IgnoredForV002()
    {
        var template = CreateTestTemplate();
        var text1 = new Text(0, 0, "Field 1", 3500, key: "field_a", isEditable: false);
        var text2 = new Text(1000, 0, "Field 2", 3500, key: "field_a", isEditable: false);
        template.Objects.Add(text1);
        template.Objects.Add(text2);

        var errors = new TemplateValidator().Validate(template);
        Assert.DoesNotContain(errors, e => e.RuleId == "V-002");
    }

    [Fact]
    public void Validate_ObjectOutOfBounds_ReturnsV003Error()
    {
        var template = CreateTestTemplate(); // A3: 420x297mm
        template.Objects.Add(new Line(500_000, 500_000, 501_000, 501_000));

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-003");
    }

    [Fact]
    public void Validate_NegativeDimensions_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Rectangle(0, 0, -1000, -1000));
    }

    [Fact]
    public void Validate_ValidTemplate_ReturnsNoErrors()
    {
        var template = CreateTestTemplate();
        template.Objects.Add(new Line(0, 0, 10000, 10000));
        template.Objects.Add(new Rectangle(0, 0, 5000, 5000));
        template.Objects.Add(new Text(0, 0, "Test", 3500));

        var errors = new TemplateValidator().Validate(template);
        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_NullTemplate_ReturnsError()
    {
        var errors = new TemplateValidator().Validate(null!);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Validate_InvalidHexColor_ReturnsV005Error()
    {
        var template = CreateTestTemplate();
        var line = new Line(0, 0, 1000, 1000);
        line.StrokeColor = "not-a-color";
        template.Objects.Add(line);

        var errors = new TemplateValidator().Validate(template);
        Assert.Contains(errors, e => e.RuleId == "V-005" && e.Severity == ValidationSeverity.Error);
    }

    [Fact]
    public void Validate_ValidHexColors_NoV005Error()
    {
        var template = CreateTestTemplate();
        template.Objects.Add(new Line(0, 0, 1000, 1000, strokeColor: "#000000"));
        template.Objects.Add(new Rectangle(0, 0, 1000, 1000, strokeColor: "#FF0000", fillColor: "Transparent"));
        template.Objects.Add(new Text(0, 0, "Test", 2500, foreground: "#00FF00"));

        var errors = new TemplateValidator().Validate(template);
        Assert.DoesNotContain(errors, e => e.RuleId == "V-005");
    }

    private static Template CreateTestTemplate()
    {
        return new Template(
            new Metadata { Name = "Test", Author = "Test", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow },
            Sheet.FromFormat("A3"));
    }

    private static void SetId(TemplateObjectBase obj, string id)
    {
        // Use reflection to set Id property
        var prop = obj.GetType().GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        prop?.SetValue(obj, id);
    }
}

public class AdditionalSnapHelperTests
{
    [Theory]
    [InlineData(0, 0, 5000, 0, 0)]     // on grid
    [InlineData(1000, 5000, 5000, 0, 5000)] // between grid points, snaps to nearest
    [InlineData(2500, 5000, 5000, 5000, 5000)] // exactly middle, rounds up
    [InlineData(7000, 7000, 5000, 5000, 5000)] // rounds down
    public void SnapToGrid_SnapsToNearestGridPoint(long x, long y, long step, long expectedX, long expectedY)
    {
        var point = new PointMicrons(x, y);
        var snapped = SnapHelper.SnapToGrid(point, step);

        Assert.Equal(expectedX, snapped.MicronsX);
        Assert.Equal(expectedY, snapped.MicronsY);
    }

    [Fact]
    public void SnapToGrid_ZeroStep_ThrowsException()
    {
        var point = new PointMicrons(1234, 5678);
        Assert.Throws<ArgumentOutOfRangeException>(() => SnapHelper.SnapToGrid(point, 0));
    }

    [Fact]
    public void SnapToGrid_NegativeStep_ThrowsException()
    {
        var point = new PointMicrons(1234, 5678);
        Assert.Throws<ArgumentOutOfRangeException>(() => SnapHelper.SnapToGrid(point, -5000));
    }

    [Fact]
    public void SnapHelper_SnapPoint_AlignsToGrid()
    {
        var point = SnapHelper.SnapToGrid(new PointMicrons(12345, 67890), 5000);
        Assert.Equal(10000, point.MicronsX);
        Assert.Equal(70000, point.MicronsY);
    }

    [Fact]
    public void SnapHelper_SnapPoint_ZeroStep_ThrowsException()
    {
        var point = new PointMicrons(12345, 67890);
        Assert.Throws<ArgumentOutOfRangeException>(() => SnapHelper.SnapToGrid(point, 0));
    }
}

public class AdditionalHitTestHelperTests
{
    [Fact]
    public void HitTest_WithEmptyList_ReturnsNull()
    {
        var objects = new List<TemplateObjectBase>();
        var point = new PointMicrons(0, 0);

        var hit = HitTestHelper.HitTest(point, objects);
        Assert.Null(hit);
    }

    [Fact]
    public void HitTest_FindsTopMostObject()
    {
        var objects = new List<TemplateObjectBase>
        {
            new Rectangle(0, 0, 10000, 10000), // bottom
            new Rectangle(0, 0, 5000, 5000),   // top
        };
        var point = new PointMicrons(2000, 2000);

        var hit = HitTestHelper.HitTest(point, objects);
        Assert.IsType<Rectangle>(hit);
        Assert.Equal(5000, ((Rectangle)hit!).WidthMicrons); // smaller one is on top
    }
}
