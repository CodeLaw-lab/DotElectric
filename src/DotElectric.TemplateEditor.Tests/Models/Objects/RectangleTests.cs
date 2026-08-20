using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Tests.Models.Objects;

public class RectangleTests
{
    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var rect = new Rectangle(1000, 2000, 5000, 3000, LineType.Dashed);

        Assert.NotNull(rect.Id);
        Assert.NotEmpty(rect.Id);
        Assert.Equal(1000, rect.MicronsX);
        Assert.Equal(2000, rect.MicronsY);
        Assert.Equal(5000, rect.WidthMicrons);
        Assert.Equal(3000, rect.HeightMicrons);
        Assert.Equal(LineType.Dashed, rect.LineType);
    }

    [Fact]
    public void Constructor_DefaultLineType_IsSolid()
    {
        var rect = new Rectangle(0, 0, 1000, 1000);
        Assert.Equal(LineType.Solid, rect.LineType);
    }

    [Fact]
    public void X_ReturnsMmValue()
    {
        var rect = new Rectangle(5500, 0, 1000, 1000);
        Assert.Equal(5.5, rect.X, tolerance: 0.0001);
    }

    [Fact]
    public void Y_ReturnsMmValue()
    {
        var rect = new Rectangle(0, 3140, 1000, 1000);
        Assert.Equal(3.14, rect.Y, tolerance: 0.0001);
    }

    [Fact]
    public void Move_UpdatesPositionOnly()
    {
        var rect = new Rectangle(1000, 2000, 5000, 3000);

        rect.Move(3000, 4000);

        Assert.Equal(3000, rect.MicronsX);
        Assert.Equal(4000, rect.MicronsY);
        // Размеры не меняются
        Assert.Equal(5000, rect.WidthMicrons);
        Assert.Equal(3000, rect.HeightMicrons);
    }

    [Fact]
    public void Clone_CreatesNewObjectWithSameData()
    {
        var rect = new Rectangle(1000, 2000, 5000, 3000, LineType.DashDot);
        var clone = rect.Clone();

        Assert.NotSame(rect, clone);
        Assert.NotEqual(rect.Id, clone.Id);
        Assert.Equal(rect.MicronsX, ((Rectangle)clone).MicronsX);
        Assert.Equal(rect.MicronsY, ((Rectangle)clone).MicronsY);
        Assert.Equal(rect.WidthMicrons, ((Rectangle)clone).WidthMicrons);
        Assert.Equal(rect.HeightMicrons, ((Rectangle)clone).HeightMicrons);
        Assert.Equal(rect.LineType, ((Rectangle)clone).LineType);
    }

    [Fact]
    public void Clone_IsInstanceOfRectangle()
    {
        var rect = new Rectangle(0, 0, 1000, 1000);
        var clone = rect.Clone();
        Assert.IsType<Rectangle>(clone);
    }

    [Fact]
    public void Id_IsUniqueForEachInstance()
    {
        var rect1 = new Rectangle(0, 0, 1000, 1000);
        var rect2 = new Rectangle(0, 0, 1000, 1000);
        Assert.NotEqual(rect1.Id, rect2.Id);
    }

    [Fact]
    public void RightMicronsX_ReturnsXPlusWidth()
    {
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        Assert.Equal(6000, rect.RightMicronsX);
    }

    [Fact]
    public void BottomMicronsY_ReturnsYPlusHeight()
    {
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        Assert.Equal(5000, rect.BottomMicronsY);
    }

    [Fact]
    public void CenterMicronsX_ReturnsXPlusHalfWidth()
    {
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        Assert.Equal(3500, rect.CenterMicronsX);
    }

    [Fact]
    public void CenterMicronsY_ReturnsYPlusHalfHeight()
    {
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        Assert.Equal(3500, rect.CenterMicronsY);
    }

    [Fact]
    public void HalfWidthMicrons_ReturnsHalfOfWidth()
    {
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        Assert.Equal(2500, rect.HalfWidthMicrons);
    }

    [Fact]
    public void HalfHeightMicrons_ReturnsHalfOfHeight()
    {
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        Assert.Equal(1500, rect.HalfHeightMicrons);
    }

    [Fact]
    public void HalfWidthMicrons_OddWidth_Truncates()
    {
        var rect = new Rectangle(0, 0, 5001, 3000);
        Assert.Equal(2500, rect.HalfWidthMicrons); // 5001 / 2 = 2500 (long division)
    }

    [Fact]
    public void HalfHeightMicrons_OddHeight_Truncates()
    {
        var rect = new Rectangle(0, 0, 5000, 3001);
        Assert.Equal(1500, rect.HalfHeightMicrons); // 3001 / 2 = 1500 (long division)
    }

    // === StrokeColor ===

    [Fact]
    public void StrokeColor_DefaultIsBlack()
    {
        var rect = new Rectangle(0, 0, 1000, 1000);
        Assert.Equal("#000000", rect.StrokeColor);
    }

    [Fact]
    public void StrokeColor_CanBeSet()
    {
        var rect = new Rectangle(0, 0, 1000, 1000);
        rect.StrokeColor = "#FF0000";
        Assert.Equal("#FF0000", rect.StrokeColor);
    }

    [Fact]
    public void Clone_CopiesStrokeColor()
    {
        var rect = new Rectangle(0, 0, 1000, 1000, strokeColor: "#00FF00");
        var clone = (Rectangle)rect.Clone();
        Assert.Equal("#00FF00", clone.StrokeColor);
    }

    // === FillColor ===

    [Fact]
    public void FillColor_DefaultIsTransparent()
    {
        var rect = new Rectangle(0, 0, 1000, 1000);
        Assert.Equal("Transparent", rect.FillColor);
    }

    [Fact]
    public void FillColor_CanBeSet()
    {
        var rect = new Rectangle(0, 0, 1000, 1000);
        rect.FillColor = "#FF0000";
        Assert.Equal("#FF0000", rect.FillColor);
    }

    [Fact]
    public void Clone_CopiesFillColor()
    {
        var rect = new Rectangle(0, 0, 1000, 1000, fillColor: "#00FF00");
        var clone = (Rectangle)rect.Clone();
        Assert.Equal("#00FF00", clone.FillColor);
    }

    [Fact]
    public void Constructor_AcceptsBothColors()
    {
        var rect = new Rectangle(0, 0, 1000, 1000, strokeColor: "#111111", fillColor: "#222222");
        Assert.Equal("#111111", rect.StrokeColor);
        Assert.Equal("#222222", rect.FillColor);
    }

    [Fact]
    public void Clone_CopiesAllPublicProperties_ExceptId()
    {
        var original = new Rectangle(100, 200, 3000, 4000, LineType.Dashed, 600, "#FF0000", "#00FF00");
        var clone = (Rectangle)original.Clone();

        var props = typeof(Rectangle).GetProperties()
            .Where(p => p.Name != "Id" && p.Name != "X" && p.Name != "Y" && p.CanRead);
        foreach (var prop in props)
        {
            Assert.Equal(prop.GetValue(original), prop.GetValue(clone));
        }
        Assert.NotEqual(original.Id, clone.Id);
    }
}
