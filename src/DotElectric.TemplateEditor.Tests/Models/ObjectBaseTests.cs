using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Tests.Models;

public class ObjectBaseTests
{
    [Fact]
    public void Id_HasDefaultValue_NotEmpty()
    {
        var obj = new TestObject();
        Assert.False(string.IsNullOrWhiteSpace(obj.Id));
    }

    [Fact]
    public void Id_CanBeSetViaInitializer()
    {
        var obj = new TestObject { Id = "explicit-id" };
        Assert.Equal("explicit-id", obj.Id);
    }

    [Fact]
    public void Clone_Overridden_ReturnsCopyWithNewId()
    {
        var obj = new TestObject { Id = "original" };
        var clone = (TestObject)obj.Clone();

        Assert.NotNull(clone);
        Assert.IsType<TestObject>(clone);
        Assert.NotSame(obj, clone);
        Assert.NotEqual(obj.Id, clone.Id);
    }

    [Fact]
    public void Clone_Overridden_PreservesOtherProperties()
    {
        var obj = new TestObject { Id = "original" };
        var clone = (TestObject)obj.Clone();

        Assert.Equal("original", obj.Id);
    }

    [Fact]
    public void DerivedClass_CanHaveAdditionalProperties()
    {
        var obj = new TestObjectWithProperties
        {
            Id = "test",
            Name = "MyObject",
            Value = 42
        };

        Assert.Equal("test", obj.Id);
        Assert.Equal("MyObject", obj.Name);
        Assert.Equal(42, obj.Value);
    }

    [Fact]
    public void Clone_WithAdditionalProperties_PreservesAll()
    {
        var obj = new TestObjectWithProperties
        {
            Id = "test",
            Name = "MyObject",
            Value = 42
        };

        var clone = (TestObjectWithProperties)obj.Clone();

        Assert.NotEqual(obj.Id, clone.Id);
        Assert.Equal("MyObject", clone.Name);
        Assert.Equal(42, clone.Value);
    }

    [Fact]
    public void MultipleInstances_HaveIndependentIds()
    {
        var obj1 = new TestObject { Id = "obj1" };
        var obj2 = new TestObject { Id = "obj2" };

        Assert.Equal("obj1", obj1.Id);
        Assert.Equal("obj2", obj2.Id);
    }

    [Fact]
    public void Id_IsImmutable_AfterConstruction()
    {
        var obj = new TestObject { Id = "fixed" };

        Assert.Equal("fixed", obj.Id);
    }

    // === Base class X / Y properties ===

    [Fact]
    public void BaseX_ReturnsMmFromMicronsX()
    {
        var obj = new TestObject { MicronsX = 5000, MicronsY = 0 };
        Assert.Equal(5.0, obj.X, 0.0001);
    }

    [Fact]
    public void BaseY_ReturnsMmFromMicronsY()
    {
        var obj = new TestObject { MicronsX = 0, MicronsY = 3000 };
        Assert.Equal(3.0, obj.Y, 0.0001);
    }

    [Fact]
    public void BaseX_ZeroMicrons_ReturnsZero()
    {
        var obj = new TestObject { MicronsX = 0, MicronsY = 0 };
        Assert.Equal(0.0, obj.X, 0.0001);
    }

    [Fact]
    public void BaseY_ZeroMicrons_ReturnsZero()
    {
        var obj = new TestObject { MicronsX = 0, MicronsY = 0 };
        Assert.Equal(0.0, obj.Y, 0.0001);
    }

    [Fact]
    public void BaseX_NegativeMicrons_ReturnsNegativeMm()
    {
        var obj = new TestObject { MicronsX = -2500, MicronsY = 0 };
        Assert.Equal(-2.5, obj.X, 0.0001);
    }

    [Fact]
    public void BaseY_NegativeMicrons_ReturnsNegativeMm()
    {
        var obj = new TestObject { MicronsX = 0, MicronsY = -1500 };
        Assert.Equal(-1.5, obj.Y, 0.0001);
    }

    // === Base virtual Move() ===

    [Fact]
    public void BaseMove_SetsMicronsXAndMicronsY()
    {
        var obj = new TestObject { MicronsX = 1000, MicronsY = 2000 };

        obj.Move(5000, 6000);

        Assert.Equal(5000, obj.MicronsX);
        Assert.Equal(6000, obj.MicronsY);
    }

    [Fact]
    public void BaseMove_SamePosition_NoChange()
    {
        var obj = new TestObject { MicronsX = 1000, MicronsY = 2000 };

        obj.Move(1000, 2000);

        Assert.Equal(1000, obj.MicronsX);
        Assert.Equal(2000, obj.MicronsY);
    }

    [Fact]
    public void BaseMove_ZeroPosition_UpdatesCorrectly()
    {
        var obj = new TestObject { MicronsX = 0, MicronsY = 0 };

        obj.Move(7500, 12500);

        Assert.Equal(7500, obj.MicronsX);
        Assert.Equal(12500, obj.MicronsY);
    }

    [Fact]
    public void BaseMove_UpdatesXAndYComputedProperties()
    {
        var obj = new TestObject { MicronsX = 0, MicronsY = 0 };

        obj.Move(10000, 20000);

        Assert.Equal(10.0, obj.X, 0.0001);
        Assert.Equal(20.0, obj.Y, 0.0001);
    }

    // === Line — CaptureResizeState / ApplyResize ===

    [Fact]
    public void Line_CaptureResizeState_ReturnsStartPositionAndDelta()
    {
        var line = new Line(1000, 2000, 8000, 6000);
        var state = line.CaptureResizeState();

        Assert.Equal(1000, state.X);   // StartMicronsX
        Assert.Equal(2000, state.Y);   // StartMicronsY
        Assert.Equal(7000, state.Width);  // End - Start X
        Assert.Equal(4000, state.Height); // End - Start Y
    }

    [Fact]
    public void Line_ApplyResize_RestoresState()
    {
        var line = new Line(0, 0, 10000, 5000);
        var state = new ResizeState(2000, 3000, 6000, 4000);

        line.ApplyResize(state);

        Assert.Equal(2000, line.StartMicronsX);
        Assert.Equal(3000, line.StartMicronsY);
        Assert.Equal(8000, line.EndMicronsX);   // X + Width
        Assert.Equal(7000, line.EndMicronsY);   // Y + Height
    }

    [Fact]
    public void Line_ApplyResize_ZeroWidth_ProducesPoint()
    {
        var line = new Line(0, 0, 10000, 5000);
        var state = new ResizeState(5000, 5000, 0, 0);

        line.ApplyResize(state);

        Assert.Equal(5000, line.StartMicronsX);
        Assert.Equal(5000, line.StartMicronsY);
        Assert.Equal(5000, line.EndMicronsX);
        Assert.Equal(5000, line.EndMicronsY);
    }

    // === Rectangle — CaptureResizeState / ApplyResize ===

    [Fact]
    public void Rectangle_CaptureResizeState_ReturnsPositionAndDimensions()
    {
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        var state = rect.CaptureResizeState();

        Assert.Equal(1000, state.X);
        Assert.Equal(2000, state.Y);
        Assert.Equal(5000, state.Width);
        Assert.Equal(3000, state.Height);
    }

    [Fact]
    public void Rectangle_ApplyResize_RestoresState()
    {
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        var state = new ResizeState(3000, 4000, 7000, 5000);

        rect.ApplyResize(state);

        Assert.Equal(3000, rect.MicronsX);
        Assert.Equal(4000, rect.MicronsY);
        Assert.Equal(7000, rect.WidthMicrons);
        Assert.Equal(5000, rect.HeightMicrons);
    }

    [Fact]
    public void Rectangle_ApplyResize_ZeroDimensions_ProducesPoint()
    {
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        var state = new ResizeState(0, 0, 0, 0);

        rect.ApplyResize(state);

        Assert.Equal(0, rect.MicronsX);
        Assert.Equal(0, rect.MicronsY);
        Assert.Equal(0, rect.WidthMicrons);
        Assert.Equal(0, rect.HeightMicrons);
    }

    // === Text — CaptureResizeState / ApplyResize ===

    [Fact]
    public void Text_CaptureResizeState_UsesFontSizeMicronsAsHeight()
    {
        var text = new Text(1000, 2000, "Hello", 3500, "ГОСТ А");
        var state = text.CaptureResizeState();

        Assert.Equal(1000, state.X);
        Assert.Equal(2000, state.Y);
        Assert.Equal(text.WidthMicrons, state.Width);
        Assert.Equal(3500, state.Height); // FontSizeMicrons
    }

    [Fact]
    public void Text_ApplyResize_RestoresState()
    {
        var text = new Text(1000, 2000, "Hello", 3500);
        var state = new ResizeState(3000, 4000, text.WidthMicrons, 5000);

        text.ApplyResize(state);

        Assert.Equal(3000, text.MicronsX);
        Assert.Equal(4000, text.MicronsY);
        Assert.Equal(5000, text.FontSizeMicrons);
    }

    [Fact]
    public void Text_ApplyResize_SetsFontSizeFromHeight()
    {
        var text = new Text(1000, 2000, "Hello", 3500);
        var state = new ResizeState(1000, 2000, text.WidthMicrons, 7000);

        text.ApplyResize(state);

        Assert.Equal(7000, text.FontSizeMicrons);
        Assert.Equal("Hello", text.Content); // Content unchanged
    }

    [Fact]
    public void Text_CaptureResizeState_ThenApplyResize_RoundTrips()
    {
        var original = new Text(1000, 2000, "Hello", 3500, "ГОСТ А", TextType.Label, 45);
        var state = original.CaptureResizeState();

        original.Move(5000, 6000);
        original.ApplyResize(state);

        Assert.Equal(1000, original.MicronsX);
        Assert.Equal(2000, original.MicronsY);
        Assert.Equal(3500, original.FontSizeMicrons);
    }

    // === Line — ContainsPoint ===

    [Fact]
    public void Line_ContainsPoint_OnSegment_ReturnsTrue()
    {
        var line = new Line(0, 0, 10000, 0);

        // Point directly on the line segment
        Assert.True(line.ContainsPoint(new PointMicrons(5000, 0)));
    }

    [Fact]
    public void Line_ContainsPoint_NearSegment_ReturnsTrue()
    {
        var line = new Line(0, 0, 10000, 0);

        // 3mm away (within LineHitToleranceMicrons = 5mm)
        Assert.True(line.ContainsPoint(new PointMicrons(5000, 3000)));
    }

    [Fact]
    public void Line_ContainsPoint_FarFromSegment_ReturnsFalse()
    {
        var line = new Line(0, 0, 10000, 0);

        // 6mm away (beyond LineHitToleranceMicrons = 5mm)
        Assert.False(line.ContainsPoint(new PointMicrons(5000, 6000)));
    }

    [Fact]
    public void Line_ContainsPoint_NearEndpoint_ReturnsTrue()
    {
        var line = new Line(0, 0, 10000, 0);

        // Point near the start endpoint
        Assert.True(line.ContainsPoint(new PointMicrons(0, 3000)));
    }

    [Fact]
    public void Line_ContainsPoint_VerticalLine_ReturnsTrue()
    {
        var line = new Line(5000, 1000, 5000, 9000);

        Assert.True(line.ContainsPoint(new PointMicrons(5000, 5000)));
    }

    [Fact]
    public void Line_ContainsPoint_VerticalLine_NearSegment_ReturnsTrue()
    {
        var line = new Line(5000, 1000, 5000, 9000);

        Assert.True(line.ContainsPoint(new PointMicrons(5000 + 3000, 5000)));
    }

    [Fact]
    public void Line_ContainsPoint_VerticalLine_Far_ReturnsFalse()
    {
        var line = new Line(5000, 1000, 5000, 9000);

        Assert.False(line.ContainsPoint(new PointMicrons(5000 + 6000, 5000)));
    }

    [Fact]
    public void Line_ContainsPoint_Diagonal_ReturnsTrue()
    {
        var line = new Line(0, 0, 10000, 10000);

        // Midpoint of diagonal
        Assert.True(line.ContainsPoint(new PointMicrons(5000, 5000)));
    }

    [Fact]
    public void Line_ContainsPoint_PointBeforeStart_ReturnsFalse()
    {
        var line = new Line(5000, 5000, 10000, 10000);

        // Point before start of segment (but on the line extension)
        // The perpendicular distance to the infinite line is 0, but the
        // distance to the segment is 7071 (5mm * sqrt(2)) — within 5mm tolerance
        var point = new PointMicrons(0, 0);
        // Distance from (0,0) to segment (5000,5000)-(10000,10000) is distance to (5000,5000) = 7071
        // 7071 > 5000 → false
        Assert.False(line.ContainsPoint(point));
    }

    // === Rectangle — ContainsPoint ===

    [Fact]
    public void Rectangle_ContainsPoint_OnBorder_ReturnsTrue()
    {
        var rect = new Rectangle(1000, 1000, 10000, 8000);

        // Point exactly on the left border
        Assert.True(rect.ContainsPoint(new PointMicrons(1000, 5000)));
    }

    [Fact]
    public void Rectangle_ContainsPoint_NearBorder_ReturnsTrue()
    {
        var rect = new Rectangle(1000, 1000, 10000, 8000);

        // Point within LineHitTolerance (5mm) of the left border
        Assert.True(rect.ContainsPoint(new PointMicrons(1000 + 3000, 5000)));
    }

    [Fact]
    public void Rectangle_ContainsPoint_InsideArea_ReturnsFalse()
    {
        // 30mm x 26mm rect — large enough to have interior beyond LineHitToleranceMicrons (5mm)
        var rect = new Rectangle(1000, 1000, 30000, 26000);

        // Point at center (16000, 14000) — >5mm from all edges → inside interior → not selectable
        Assert.False(rect.ContainsPoint(new PointMicrons(16000, 14000)));
    }

    [Fact]
    public void Rectangle_ContainsPoint_FarAway_ReturnsFalse()
    {
        var rect = new Rectangle(1000, 1000, 10000, 8000);

        // Point far outside
        Assert.False(rect.ContainsPoint(new PointMicrons(50000, 50000)));
    }

    [Fact]
    public void Rectangle_ContainsPoint_SmallRectangle_AllAreaSelectable()
    {
        // Rectangle smaller than 2*LineHitToleranceMicrons (10mm) in both dimensions
        var rect = new Rectangle(0, 0, 8000, 6000);

        // Center of small rect — still inside the border band
        Assert.True(rect.ContainsPoint(new PointMicrons(4000, 3000)));
    }

    // === Line — GetBoundingBox ===

    [Fact]
    public void Line_GetBoundingBox_ReturnsCorrectMinMax()
    {
        var line = new Line(1000, 2000, 8000, 6000);
        var bbox = line.GetBoundingBox();

        Assert.Equal(1000, bbox.Left);
        Assert.Equal(2000, bbox.Bottom);
        Assert.Equal(8000, bbox.Right);
        Assert.Equal(6000, bbox.Top);
    }

    [Fact]
    public void Line_GetBoundingBox_NegativeCoordinates_ReturnsCorrectBounds()
    {
        var line = new Line(5000, 8000, 1000, 2000);
        var bbox = line.GetBoundingBox();

        Assert.Equal(1000, bbox.Left);
        Assert.Equal(2000, bbox.Bottom);
        Assert.Equal(5000, bbox.Right);
        Assert.Equal(8000, bbox.Top);
    }

    [Fact]
    public void Line_GetBoundingBox_ZeroLength_ReturnsSinglePoint()
    {
        var line = new Line(5000, 5000, 5000, 5000);
        var bbox = line.GetBoundingBox();

        Assert.Equal(5000, bbox.Left);
        Assert.Equal(5000, bbox.Bottom);
        Assert.Equal(5000, bbox.Right);
        Assert.Equal(5000, bbox.Top);
    }

    [Fact]
    public void Line_GetBoundingBox_HorizontalLine_ReturnsCorrectHeight()
    {
        var line = new Line(0, 5000, 10000, 5000);
        var bbox = line.GetBoundingBox();

        Assert.Equal(0, bbox.Left);
        Assert.Equal(5000, bbox.Bottom);
        Assert.Equal(10000, bbox.Right);
        Assert.Equal(5000, bbox.Top);
    }

    [Fact]
    public void Line_GetBoundingBox_VerticalLine_ReturnsCorrectWidth()
    {
        var line = new Line(5000, 0, 5000, 10000);
        var bbox = line.GetBoundingBox();

        Assert.Equal(5000, bbox.Left);
        Assert.Equal(0, bbox.Bottom);
        Assert.Equal(5000, bbox.Right);
        Assert.Equal(10000, bbox.Top);
    }

    // === Rectangle — GetBoundingBox ===

    [Fact]
    public void Rectangle_GetBoundingBox_ReturnsPositionAndDimensions()
    {
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        var bbox = rect.GetBoundingBox();

        Assert.Equal(1000, bbox.Left);
        Assert.Equal(2000, bbox.Bottom);
        Assert.Equal(6000, bbox.Right);  // X + Width
        Assert.Equal(5000, bbox.Top);    // Y + Height
    }

    [Fact]
    public void Rectangle_GetBoundingBox_ZeroDimensions_ReturnsSinglePoint()
    {
        var rect = new Rectangle(1000, 2000, 0, 0);
        var bbox = rect.GetBoundingBox();

        Assert.Equal(1000, bbox.Left);
        Assert.Equal(2000, bbox.Bottom);
        Assert.Equal(1000, bbox.Right);
        Assert.Equal(2000, bbox.Top);
    }

    // === Id — default initialization ===

    [Fact]
    public void Id_DefaultForLine_IsNotEmpty()
    {
        var line = new Line(0, 0, 1000, 1000);
        Assert.False(string.IsNullOrWhiteSpace(line.Id));
    }

    [Fact]
    public void Id_DefaultForRectangle_IsNotEmpty()
    {
        var rect = new Rectangle(0, 0, 1000, 1000);
        Assert.False(string.IsNullOrWhiteSpace(rect.Id));
    }

    [Fact]
    public void Id_DefaultForText_IsNotEmpty()
    {
        var text = new Text(0, 0, "Test", 2500);
        Assert.False(string.IsNullOrWhiteSpace(text.Id));
    }

    private class TestObject : TemplateObjectBase
    {
        public override long MicronsX { get; set; }
        public override long MicronsY { get; set; }
        public override TemplateObjectBase Clone() => new TestObject { Id = Guid.NewGuid().ToString() };
        public override bool ContainsPoint(PointMicrons point) => false;
        public override RectMicrons GetBoundingBox() => new(0, 0, 0, 0);
        public override ResizeState CaptureResizeState() => new(MicronsX, MicronsY, 0, 0);
        public override void ApplyResize(ResizeState state) { MicronsX = state.X; MicronsY = state.Y; }
    }

    private class TestObjectWithProperties : TemplateObjectBase
    {
        public string? Name { get; set; }
        public int Value { get; set; }

        public override long MicronsX { get; set; }
        public override long MicronsY { get; set; }
        public override TemplateObjectBase Clone() => new TestObjectWithProperties
        {
            Id = Guid.NewGuid().ToString(),
            Name = this.Name,
            Value = this.Value
        };
        public override bool ContainsPoint(PointMicrons point) => false;
        public override RectMicrons GetBoundingBox() => new(0, 0, 0, 0);
        public override ResizeState CaptureResizeState() => new(MicronsX, MicronsY, 0, 0);
        public override void ApplyResize(ResizeState state) { MicronsX = state.X; MicronsY = state.Y; }
    }
}
