using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;

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
        var snapped = point.SnapToGrid(step);

        Assert.Equal(expectedX, snapped.MicronsX);
        Assert.Equal(expectedY, snapped.MicronsY);
    }

    [Fact]
    public void SnapToGrid_ZeroStep_ThrowsException()
    {
        var point = new PointMicrons(1234, 5678);
        Assert.Throws<ArgumentOutOfRangeException>(() => point.SnapToGrid(0));
    }

    [Fact]
    public void SnapToGrid_NegativeStep_ThrowsException()
    {
        var point = new PointMicrons(1234, 5678);
        Assert.Throws<ArgumentOutOfRangeException>(() => point.SnapToGrid(-5000));
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
    public void HitTestObject_OnRectangle_ReturnsTrue()
    {
        var rect = new Rectangle(0, 0, 10000, 10000);
        var point = new PointMicrons(3000, 3000); // within border band (tol=5000)

        Assert.True(HitTestHelper.HitTestObject(rect, point));
    }

    [Fact]
    public void HitTestObject_OutsideRectangle_ReturnsFalse()
    {
        var rect = new Rectangle(0, 0, 10000, 10000);
        var point = new PointMicrons(16000, 16000); // outside expanded bounds (tol=5000)

        Assert.False(HitTestHelper.HitTestObject(rect, point));
    }

    [Fact]
    public void HitTestObject_OnEdge_ReturnsTrue()
    {
        var rect = new Rectangle(0, 0, 10000, 10000);
        var point = new PointMicrons(0, 5000); // left edge

        Assert.True(HitTestHelper.HitTestObject(rect, point));
    }

    [Fact]
    public void HitTestObject_OnCorner_ReturnsTrue()
    {
        var rect = new Rectangle(0, 0, 10000, 10000);
        var point = new PointMicrons(0, 0); // bottom-left corner

        Assert.True(HitTestHelper.HitTestObject(rect, point));
    }

    [Fact]
    public void HitTestLine_OnLine_ReturnsTrue()
    {
        var line = new Line(0, 0, 10000, 0);
        var point = new PointMicrons(5000, 0);

        Assert.True(HitTestHelper.HitTestLine(line, point));
    }

    [Fact]
    public void HitTestLine_NearLine_ReturnsTrue()
    {
        var line = new Line(0, 0, 10000, 0);
        var point = new PointMicrons(5000, 1000); // 1mm from line

        Assert.True(HitTestHelper.HitTestLine(line, point));
    }

    [Fact]
    public void HitTestLine_AtEndpoint_ReturnsTrue()
    {
        var line = new Line(0, 0, 10000, 0);
        var point = new PointMicrons(0, 0);

        Assert.True(HitTestHelper.HitTestLine(line, point));
    }

    [Fact]
    public void HitTestLine_FarFromLine_ReturnsFalse()
    {
        var line = new Line(0, 0, 10000, 0);
        var point = new PointMicrons(5000, 10000); // 10mm from line

        Assert.False(HitTestHelper.HitTestLine(line, point));
    }

    [Fact]
    public void HitTestLine_VerticalLine_ReturnsTrue()
    {
        var line = new Line(5000, 0, 5000, 10000);
        var point = new PointMicrons(5000, 5000);

        Assert.True(HitTestHelper.HitTestLine(line, point));
    }

    [Fact]
    public void HitTestText_InsideBoundingBox_ReturnsTrue()
    {
        var text = new Text(0, 0, "Test", 5000);
        var point = new PointMicrons(2000, 2000);

        Assert.True(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_OutsideBoundingBox_ReturnsFalse()
    {
        var text = new Text(0, 0, "Test", 5000);
        var point = new PointMicrons(100000, 100000);

        Assert.False(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestText_EmptyContent_ReturnsTrueForDefaultSize()
    {
        // Empty content has WidthMicrons = FontSizeMicrons (minimum)
        var text = new Text(0, 0, "", 5000);
        var point = new PointMicrons(100, 100);

        Assert.True(HitTestHelper.HitTestText(text, point));
    }

    [Fact]
    public void HitTestAll_ReturnsAllObjectsUnderPoint()
    {
        var objects = new List<TemplateObjectBase>
        {
            new Rectangle(0, 0, 10000, 10000),
            new Line(0, 0, 10000, 10000),
        };
        var point = new PointMicrons(5000, 2500); // within rect border band and near line

        var hits = HitTestHelper.HitTestAll(point, objects);
        Assert.Equal(2, hits.Count);
    }

    [Fact]
    public void HitTestAll_NoObjectsUnderPoint_ReturnsEmpty()
    {
        var objects = new List<TemplateObjectBase>
        {
            new Rectangle(0, 0, 1000, 1000),
        };
        var point = new PointMicrons(7000, 7000); // outside expanded bounds (tol=5000)

        var hits = HitTestHelper.HitTestAll(point, objects);
        Assert.Empty(hits);
    }

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

    [Fact]
    public void DistanceFromPointToLine_OnLine_ReturnsZero()
    {
        var p1 = new PointMicrons(0, 0);
        var p2 = new PointMicrons(10000, 0);
        var point = new PointMicrons(5000, 0);

        var distance = HitTestHelper.DistanceFromPointToLine(point, p1, p2);
        Assert.Equal(0.0, distance, 0.001);
    }

    [Fact]
    public void DistanceFromPointToLine_Perpendicular_ReturnsCorrectDistance()
    {
        var p1 = new PointMicrons(0, 0);
        var p2 = new PointMicrons(10000, 0);
        var point = new PointMicrons(5000, 3000); // 3mm perpendicular

        var distance = HitTestHelper.DistanceFromPointToLine(point, p1, p2);
        Assert.Equal(3000.0, distance, 0.001);
    }
}
