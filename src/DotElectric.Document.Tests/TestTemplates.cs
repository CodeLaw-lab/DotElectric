namespace DotElectric.Document.Tests;

/// <summary>
/// Общая фикстура создания шаблонов для тестов документной библиотеки.
/// </summary>
internal static class TestTemplates
{
    public static Template CreateValidA4()
        => new(new Metadata { Name = "Test", Author = "User" }, Sheet.FromFormat("A4"));

    public static Template CreateA3()
        => new(
            new Metadata { Name = "Test", Author = "Test", CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow },
            Sheet.FromFormat("A3"));

    public static Template CreateA3(DateTime fixedDate)
        => new(
            new Metadata { Name = "Test", Author = "Test", CreatedDate = fixedDate, ModifiedDate = fixedDate },
            Sheet.FromFormat("A3"));

    public static void SetId(TemplateObjectBase obj, string id)
    {
        var prop = obj.GetType().GetProperty("Id", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        prop?.SetValue(obj, id);
    }
}

/// <summary>
/// Минимальная конкретная реализация <see cref="TemplateObjectBase"/> для тестов валидатора.
/// </summary>
internal sealed class TestTemplateObject : TemplateObjectBase
{
    public override long MicronsX { get; set; }
    public override long MicronsY { get; set; }
    public override double X => Coordinate.ToMm(MicronsX);
    public override double Y => Coordinate.ToMm(MicronsY);

    public TestTemplateObject(string id, long x, long y)
    {
        Id = id;
        MicronsX = x;
        MicronsY = y;
    }

    public override void Move(long micronsX, long micronsY) { }
    public override TemplateObjectBase Clone() => new TestTemplateObject(Id!, MicronsX, MicronsY);
    public override bool ContainsPoint(PointMicrons point) => false;
    public override RectMicrons GetBoundingBox() => new RectMicrons(0, 0, 0, 0);
    public override ResizeState CaptureResizeState() => new(MicronsX, MicronsY, 0, 0);
    public override void ApplyResize(ResizeState state) { MicronsX = state.X; MicronsY = state.Y; }
}
