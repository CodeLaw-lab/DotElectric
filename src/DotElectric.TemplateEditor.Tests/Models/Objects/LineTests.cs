using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Tests.Models.Objects;

public class LineTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var line = new Line(0, 0, 10000, 5000, LineType.Dashed);

        Assert.NotNull(line.Id);
        Assert.NotEmpty(line.Id);
        Assert.Equal(0, line.StartMicronsX);
        Assert.Equal(0, line.StartMicronsY);
        Assert.Equal(10000, line.EndMicronsX);
        Assert.Equal(5000, line.EndMicronsY);
        Assert.Equal(LineType.Dashed, line.LineType);
    }

    [Fact]
    public void Constructor_DefaultLineType_IsSolid()
    {
        var line = new Line(0, 0, 1000, 1000);
        Assert.Equal(LineType.Solid, line.LineType);
    }

    [Fact]
    public void MicronsX_ReturnsStartMicronsX()
    {
        var line = new Line(5000, 3000, 10000, 8000);
        Assert.Equal(5000, line.MicronsX);
    }

    [Fact]
    public void MicronsY_ReturnsStartMicronsY()
    {
        var line = new Line(5000, 3000, 10000, 8000);
        Assert.Equal(3000, line.MicronsY);
    }

    [Fact]
    public void X_ReturnsMmValue()
    {
        var line = new Line(5500, 0, 10000, 0);
        Assert.Equal(5.5, line.X, tolerance: 0.0001);
    }

    [Fact]
    public void Y_ReturnsMmValue()
    {
        var line = new Line(0, 3140, 0, 10000);
        Assert.Equal(3.14, line.Y, tolerance: 0.0001);
    }

    [Fact]
    public void Move_UpdatesBothStartAndEnd()
    {
        var line = new Line(0, 0, 10000, 5000);

        line.Move(1000, 2000);

        // Start = новая позиция, End = старый End + дельта
        Assert.Equal(1000, line.StartMicronsX);
        Assert.Equal(2000, line.StartMicronsY);
        Assert.Equal(11000, line.EndMicronsX);  // 10000 + (1000 - 0)
        Assert.Equal(7000, line.EndMicronsY);   // 5000 + (2000 - 0)
    }

    [Fact]
    public void Move_WithoutChange_DeltaIsZero()
    {
        var line = new Line(5000, 3000, 15000, 8000);

        line.Move(5000, 3000);

        Assert.Equal(5000, line.StartMicronsX);
        Assert.Equal(3000, line.StartMicronsY);
        Assert.Equal(15000, line.EndMicronsX);
        Assert.Equal(8000, line.EndMicronsY);
    }

    [Fact]
    public void Clone_CreatesNewObjectWithSameData()
    {
        var line = new Line(0, 0, 10000, 5000, LineType.DashDot);
        var clone = line.Clone();

        Assert.NotSame(line, clone);
        Assert.NotEqual(line.Id, clone.Id); // Новый Id
        Assert.Equal(line.StartMicronsX, ((Line)clone).StartMicronsX);
        Assert.Equal(line.StartMicronsY, ((Line)clone).StartMicronsY);
        Assert.Equal(line.EndMicronsX, ((Line)clone).EndMicronsX);
        Assert.Equal(line.EndMicronsY, ((Line)clone).EndMicronsY);
        Assert.Equal(line.LineType, ((Line)clone).LineType);
    }

    [Fact]
    public void Clone_IsInstanceOfLine()
    {
        var line = new Line(0, 0, 1000, 1000);
        var clone = line.Clone();
        Assert.IsType<Line>(clone);
    }

    [Fact]
    public void Id_IsUniqueForEachInstance()
    {
        var line1 = new Line(0, 0, 1000, 1000);
        var line2 = new Line(0, 0, 1000, 1000);
        Assert.NotEqual(line1.Id, line2.Id);
    }

    [Fact]
    public void LineType_AllValuesWork()
    {
        foreach (LineType type in Enum.GetValues<LineType>())
        {
            var line = new Line(0, 0, 1000, 1000, type);
            Assert.Equal(type, line.LineType);
        }
    }

    // === StrokeColor ===

    [Fact]
    public void StrokeColor_DefaultIsBlack()
    {
        var line = new Line(0, 0, 1000, 1000);
        Assert.Equal("#000000", line.StrokeColor);
    }

    [Fact]
    public void StrokeColor_CanBeSet()
    {
        var line = new Line(0, 0, 1000, 1000);
        line.StrokeColor = "#FF0000";
        Assert.Equal("#FF0000", line.StrokeColor);
    }

    [Fact]
    public void Clone_CopiesStrokeColor()
    {
        var line = new Line(0, 0, 1000, 1000, strokeColor: "#00FF00");
        var clone = (Line)line.Clone();
        Assert.Equal("#00FF00", clone.StrokeColor);
    }

    [Fact]
    public void Constructor_AcceptsStrokeColor()
    {
        var line = new Line(0, 0, 1000, 1000, LineType.Solid, strokeColor: "#123456");
        Assert.Equal("#123456", line.StrokeColor);
    }

    [Fact]
    public void Clone_CopiesAllPublicProperties_ExceptId()
    {
        var original = new Line(100, 200, 3000, 4000, LineType.Dashed, 600, "#FF0000");
        var clone = (Line)original.Clone();

        var props = typeof(Line).GetProperties()
            .Where(p => p.Name != "Id" && p.Name != "X" && p.Name != "Y" && p.CanRead);
        foreach (var prop in props)
        {
            Assert.Equal(prop.GetValue(original), prop.GetValue(clone));
        }
        Assert.NotEqual(original.Id, clone.Id);
    }
}
