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
