using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Tests.Models.Objects;

public class RectangleAdditionalTests
{
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
    public void Clone_CreatesIndependentCopy()
    {
        var rect = new Rectangle(1000, 2000, 5000, 3000, LineType.Dashed);
        var clone = (Rectangle)rect.Clone();

        Assert.Equal(rect.MicronsX, clone.MicronsX);
        Assert.Equal(rect.MicronsY, clone.MicronsY);
        Assert.Equal(rect.WidthMicrons, clone.WidthMicrons);
        Assert.Equal(rect.HeightMicrons, clone.HeightMicrons);
        Assert.Equal(rect.LineType, clone.LineType);
        Assert.NotEqual(rect.Id, clone.Id); // different IDs
    }

    [Fact]
    public void Move_UpdatesPosition()
    {
        var rect = new Rectangle(0, 0, 1000, 1000);
        rect.Move(5000, 6000);

        Assert.Equal(5000, rect.MicronsX);
        Assert.Equal(6000, rect.MicronsY);
    }
}

public class TextAdditionalTests
{
    [Fact]
    public void WidthMicrons_EmptyContent_ReturnsFontSize()
    {
        var text = new Text(0, 0, "", 5000);
        Assert.Equal(5000, text.WidthMicrons);
    }

    [Fact]
    public void WidthMicrons_CalculatesBasedOnLength()
    {
        var text = new Text(0, 0, "Hello", 5000);
        // Default font "ГОСТ А" → factor 0.5 → 5 * 5000 * 0.5 = 12500
        Assert.Equal(12500, text.WidthMicrons);
    }

    [Fact]
    public void RightMicronsX_ReturnsXPlusWidth()
    {
        var text = new Text(1000, 2000, "Test", 5000);
        Assert.Equal(1000 + text.WidthMicrons, text.RightMicronsX);
    }

    [Fact]
    public void BottomMicronsY_ReturnsYPlusFontSize()
    {
        var text = new Text(0, 0, "Test", 5000);
        Assert.Equal(5000, text.BottomMicronsY);
    }

    [Fact]
    public void CenterMicronsX_ReturnsCenterX()
    {
        var text = new Text(0, 0, "Test", 5000);
        Assert.Equal(text.WidthMicrons / 2, text.CenterMicronsX);
    }

    [Fact]
    public void CenterMicronsY_ReturnsCenterY()
    {
        var text = new Text(0, 0, "Test", 5000);
        Assert.Equal(2500, text.CenterMicronsY);
    }

    [Fact]
    public void RotationAngleValid_OnlyAcceptsValidAngles()
    {
        var text = new Text(0, 0, "Test", 5000, rotationAngle: 0);
        Assert.True(text.RotationAngleValid);

        text.RotationAngle = 90;
        Assert.True(text.RotationAngleValid);

        text.RotationAngle = 270;
        Assert.True(text.RotationAngleValid);
    }

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        var text = new Text(1000, 2000, "Hello", 5000, "ГОСТ Б", TextType.Label, 90);
        var clone = (Text)text.Clone();

        Assert.Equal(text.MicronsX, clone.MicronsX);
        Assert.Equal(text.Content, clone.Content);
        Assert.Equal(text.FontSizeMicrons, clone.FontSizeMicrons);
        Assert.Equal(text.FontName, clone.FontName);
        Assert.Equal(text.TextType, clone.TextType);
        Assert.Equal(text.RotationAngle, clone.RotationAngle);
        Assert.NotEqual(text.Id, clone.Id);
    }
}

public class LineAdditionalTests
{
    [Fact]
    public void MicronsX_PropertyMapsToStartMicronsX()
    {
        var line = new Line(1000, 2000, 3000, 4000);
        Assert.Equal(1000, line.MicronsX);
        line.MicronsX = 5000;
        Assert.Equal(5000, line.StartMicronsX);
    }

    [Fact]
    public void MicronsY_PropertyMapsToStartMicronsY()
    {
        var line = new Line(1000, 2000, 3000, 4000);
        Assert.Equal(2000, line.MicronsY);
    }

    [Fact]
    public void Move_UpdatesBothEnds()
    {
        var line = new Line(0, 0, 1000, 1000);
        line.Move(5000, 6000);

        Assert.Equal(5000, line.StartMicronsX);
        Assert.Equal(6000, line.StartMicronsY);
        Assert.Equal(6000, line.EndMicronsX);  // moved by delta
        Assert.Equal(7000, line.EndMicronsY);
    }

    [Fact]
    public void Clone_CreatesIndependentCopy()
    {
        var line = new Line(0, 0, 1000, 1000, LineType.Dashed);
        var clone = (Line)line.Clone();

        Assert.Equal(line.StartMicronsX, clone.StartMicronsX);
        Assert.Equal(line.StartMicronsY, clone.StartMicronsY);
        Assert.Equal(line.EndMicronsX, clone.EndMicronsX);
        Assert.Equal(line.EndMicronsY, clone.EndMicronsY);
        Assert.Equal(line.LineType, clone.LineType);
        Assert.NotEqual(line.Id, clone.Id);
    }
}
