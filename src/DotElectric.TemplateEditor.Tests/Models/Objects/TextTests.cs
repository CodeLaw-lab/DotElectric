using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Tests.Models.Objects;

[Collection("FontMetrics")]
public class TextTests : IDisposable
{
    public TextTests()
    {
        FontMetrics.Default.Reset();
    }

    public void Dispose()
    {
        FontMetrics.Default.Reset();
    }

    [Fact]
    public void Constructor_SetsAllProperties()
    {
        var text = new Text(1000, 2000, "������", 3500, "���� �", TextType.Label, 90);

        Assert.NotNull(text.Id);
        Assert.NotEmpty(text.Id);
        Assert.Equal(1000, text.MicronsX);
        Assert.Equal(2000, text.MicronsY);
        Assert.Equal("������", text.Content);
        Assert.Equal(3500, text.FontSizeMicrons);
        Assert.Equal("���� �", text.FontName);
        Assert.Equal(TextType.Label, text.TextType);
        Assert.Equal(90, text.RotationAngle);
    }

    [Fact]
    public void Constructor_DefaultValues()
    {
        var text = new Text(0, 0, "Test", 2500);

        Assert.Equal(EditorSettings.DefaultFontName, text.FontName);
        Assert.Equal(TextType.Text, text.TextType);
        Assert.Equal(0, text.RotationAngle);
    }

    [Fact]
    public void X_ReturnsMmValue()
    {
        var text = new Text(5500, 0, "Test", 2500);
        Assert.Equal(5.5, text.X, tolerance: 0.0001);
    }

    [Fact]
    public void Y_ReturnsMmValue()
    {
        var text = new Text(0, 3140, "Test", 2500);
        Assert.Equal(3.14, text.Y, tolerance: 0.0001);
    }

    [Fact]
    public void RotationAngleValid_ForValidAngles_ReturnsTrue()
    {
        foreach (var angle in new[] { 0, 90, 180, 270 })
        {
            var text = new Text(0, 0, "Test", 2500, rotationAngle: angle);
            Assert.True(text.RotationAngleValid);
        }
    }

    [Fact]
    public void RotationAngle_NormalizesTo0To359()
    {
        var text = new Text(0, 0, "Test", 2500);
        text.RotationAngle = 450; // 450 - 360 = 90
        Assert.Equal(90, text.RotationAngle);

        text.RotationAngle = -90; // -90 + 360 = 270
        Assert.Equal(270, text.RotationAngle);
    }

    [Fact]
    public void Move_UpdatesPosition()
    {
        var text = new Text(1000, 2000, "Test", 2500);

        text.Move(3000, 4000);

        Assert.Equal(3000, text.MicronsX);
        Assert.Equal(4000, text.MicronsY);
        // ��������� �������� �� ��������
        Assert.Equal("Test", text.Content);
        Assert.Equal(2500, text.FontSizeMicrons);
    }

    [Fact]
    public void Clone_CreatesNewObjectWithSameData()
    {
        var text = new Text(1000, 2000, "Hello", 3500, "���� �", TextType.Dimension, 180);
        var clone = text.Clone();

        Assert.NotSame(text, clone);
        Assert.NotEqual(text.Id, clone.Id);
        Assert.Equal(text.MicronsX, ((Text)clone).MicronsX);
        Assert.Equal(text.MicronsY, ((Text)clone).MicronsY);
        Assert.Equal(text.Content, ((Text)clone).Content);
        Assert.Equal(text.FontSizeMicrons, ((Text)clone).FontSizeMicrons);
        Assert.Equal(text.FontName, ((Text)clone).FontName);
        Assert.Equal(text.TextType, ((Text)clone).TextType);
        Assert.Equal(text.RotationAngle, ((Text)clone).RotationAngle);
    }

    [Fact]
    public void Clone_IsInstanceOfText()
    {
        var text = new Text(0, 0, "Test", 2500);
        var clone = text.Clone();
        Assert.IsType<Text>(clone);
    }

    [Fact]
    public void Id_IsUniqueForEachInstance()
    {
        var text1 = new Text(0, 0, "Test", 2500);
        var text2 = new Text(0, 0, "Test", 2500);
        Assert.NotEqual(text1.Id, text2.Id);
    }

    [Fact]
    public void Content_CanBeEmpty()
    {
        var text = new Text(0, 0, "", 2500);
        Assert.Empty(text.Content);
    }

    [Fact]
    public void Content_CanBeModified()
    {
        var text = new Text(0, 0, "Old", 2500);
        text.Content = "New";
        Assert.Equal("New", text.Content);
    }

    [Fact]
    public void TextType_AllValuesWork()
    {
        foreach (TextType type in Enum.GetValues<TextType>())
        {
            var text = new Text(0, 0, "Test", 2500, textType: type);
            Assert.Equal(type, text.TextType);
        }
    }

    // === ����� ����� ��� WidthMicrons � ��������� ������� ===

    [Fact]
    public void WidthMicrons_EmptyContent_ReturnsFontSizeMicrons()
    {
        var text = new Text(0, 0, "", 3500);
        Assert.Equal(3500, text.WidthMicrons);
    }

    [Fact]
    public void WidthMicrons_WithContent_UsesHeuristic()
    {
        // "Hello" = 5 chars, default font "ГОСТ А" → factor 0.5 → 3500 * 5 * 0.5 = 8750
        var text = new Text(0, 0, "Hello", 3500);
        Assert.Equal(8750, text.WidthMicrons);
    }

    [Fact]
    public void WidthMicrons_ShortContent_ReturnsFontSizeMicrons()
    {
        // "A" = 1 ������, �� ������� ��� 3500 * 1 * 0.6 = 2100 < 3500, ���������� 3500
        var text = new Text(0, 0, "A", 3500);
        Assert.Equal(3500, text.WidthMicrons);
    }

    [Fact]
    public void RightMicronsX_ReturnsXPlusWidth()
    {
        var text = new Text(1000, 2000, "Test", 3500);
        Assert.Equal(1000 + text.WidthMicrons, text.RightMicronsX);
    }

    [Fact]
    public void BottomMicronsY_ReturnsYPlusFontSize()
    {
        var text = new Text(1000, 2000, "Test", 3500);
        Assert.Equal(2000 + 3500, text.BottomMicronsY);
    }

    [Fact]
    public void CenterMicronsX_ReturnsXPlusHalfWidth()
    {
        var text = new Text(1000, 2000, "Test", 3500);
        Assert.Equal(1000 + text.WidthMicrons / 2, text.CenterMicronsX);
    }

    [Fact]
    public void CenterMicronsY_ReturnsYPlusHalfFontSize()
    {
        var text = new Text(1000, 2000, "Test", 3500);
        Assert.Equal(2000 + 3500 / 2, text.CenterMicronsY);
    }

    [Fact]
    public void WidthMicrons_ContentChanged_Recalculates()
    {
        var text = new Text(0, 0, "A", 3500);
        var oldWidth = text.WidthMicrons;

        text.Content = "LongerText";
        Assert.NotEqual(oldWidth, text.WidthMicrons);
    }

    // === Key / IsEditable / DefaultValue ===

    [Fact]
    public void Key_Default_IsNull()
    {
        var text = new Text(0, 0, "Test", 2500);
        Assert.Null(text.Key);
    }

    [Fact]
    public void Key_CanBeSet()
    {
        var text = new Text(0, 0, "Test", 2500);
        text.Key = "field_name";
        Assert.Equal("field_name", text.Key);
    }

    [Fact]
    public void IsEditable_Default_IsFalse()
    {
        var text = new Text(0, 0, "Test", 2500);
        Assert.False(text.IsEditable);
    }

    [Fact]
    public void IsEditable_CanBeSet()
    {
        var text = new Text(0, 0, "Test", 2500);
        text.IsEditable = true;
        Assert.True(text.IsEditable);
    }

    [Fact]
    public void DefaultValue_Default_IsNull()
    {
        var text = new Text(0, 0, "Test", 2500);
        Assert.Null(text.DefaultValue);
    }

    [Fact]
    public void DefaultValue_CanBeSet()
    {
        var text = new Text(0, 0, "Test", 2500);
        text.DefaultValue = "initial value";
        Assert.Equal("initial value", text.DefaultValue);
    }

    [Fact]
    public void Constructor_AcceptsAllNewProperties()
    {
        var text = new Text(1000, 2000, "Content", 3500, "ГОСТ Б", TextType.Dimension, 90,
            "my_key", true, "default_text");

        Assert.Equal("my_key", text.Key);
        Assert.True(text.IsEditable);
        Assert.Equal("default_text", text.DefaultValue);
    }

    [Fact]
    public void Clone_CopiesKeyIsEditableDefaultValue()
    {
        var text = new Text(1000, 2000, "Content", 3500, key: "clone_key", isEditable: true, defaultValue: "clone_def");
        var clone = (Text)text.Clone();

        Assert.Equal(text.Key, clone.Key);
        Assert.Equal(text.IsEditable, clone.IsEditable);
        Assert.Equal(text.DefaultValue, clone.DefaultValue);
    }

    // === Foreground ===

    [Fact]
    public void Foreground_DefaultIsBlack()
    {
        var text = new Text(0, 0, "Test", 2500);
        Assert.Equal("#000000", text.Foreground);
    }

    [Fact]
    public void Foreground_CanBeSet()
    {
        var text = new Text(0, 0, "Test", 2500);
        text.Foreground = "#FF0000";
        Assert.Equal("#FF0000", text.Foreground);
    }

    [Fact]
    public void Clone_CopiesForeground()
    {
        var text = new Text(0, 0, "Test", 2500, foreground: "#00FF00");
        var clone = (Text)text.Clone();
        Assert.Equal("#00FF00", clone.Foreground);
    }

    [Fact]
    public void Constructor_AcceptsForeground()
    {
        var text = new Text(0, 0, "Test", 2500, foreground: "#123456");
        Assert.Equal("#123456", text.Foreground);
    }

    // === TextWrapping ===

    [Fact]
    public void TextWrapping_DefaultIsFalse()
    {
        var text = new Text(0, 0, "Test", 2500);
        Assert.False(text.TextWrapping);
    }

    [Fact]
    public void TextWrapping_CanBeSet()
    {
        var text = new Text(0, 0, "Test", 2500);
        text.TextWrapping = true;
        Assert.True(text.TextWrapping);
    }

    [Fact]
    public void TextWrapping_CanBeSetViaConstructor()
    {
        var text = new Text(0, 0, "Test", 2500, textWrapping: true);
        Assert.True(text.TextWrapping);
    }

    [Fact]
    public void Clone_CopiesTextWrapping()
    {
        var text = new Text(0, 0, "Test", 2500, textWrapping: true);
        var clone = (Text)text.Clone();
        Assert.True(clone.TextWrapping);
    }

    // === TextAlignment ===

    [Fact]
    public void TextAlignment_DefaultIsLeft()
    {
        var text = new Text(0, 0, "Test", 2500);
        Assert.Equal("Left", text.TextAlignment);
    }

    [Fact]
    public void TextAlignment_CanBeSet()
    {
        var text = new Text(0, 0, "Test", 2500);
        text.TextAlignment = "Center";
        Assert.Equal("Center", text.TextAlignment);
    }

    [Fact]
    public void TextAlignment_CanBeSetViaConstructor()
    {
        var text = new Text(0, 0, "Test", 2500, textAlignment: "Right");
        Assert.Equal("Right", text.TextAlignment);
    }

    [Fact]
    public void Clone_CopiesTextAlignment()
    {
        var text = new Text(0, 0, "Test", 2500, textAlignment: "Center");
        var clone = (Text)text.Clone();
        Assert.Equal("Center", clone.TextAlignment);
    }

    [Fact]
    public void Clone_CopiesAllPublicProperties_ExceptId()
    {
        var original = new Text(100, 200, "Hello", 3000, "ГОСТ Б", TextType.Dimension, 90,
            "mykey", true, "default", "#FF0000", true, "Center");
        var clone = (Text)original.Clone();

        var props = typeof(Text).GetProperties()
            .Where(p => p.Name != "Id" && p.Name != "X" && p.Name != "Y" && p.CanRead);
        foreach (var prop in props)
        {
            Assert.Equal(prop.GetValue(original), prop.GetValue(clone));
        }
        Assert.NotEqual(original.Id, clone.Id);
    }

    // === FontMetrics-corrected geometry ===

    [Fact]
    public void HeightMicrons_CorrectedMetrics_SingleLine()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(0, 0, "Hello", 3500, "ГОСТ Б");

        var expected = (long)(3500 * 1.1719);
        Assert.Equal(expected, text.HeightMicrons);
    }

    [Fact]
    public void HeightMicrons_CorrectedMetrics_MultiLine()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(0, 0, "Hello\nWorld", 3500, "ГОСТ Б");

        var expected = (long)(3500 * 1.1719 * (1 + 1 * 1.3));
        Assert.Equal(expected, text.HeightMicrons);
    }

    [Fact]
    public void WidthMicrons_CorrectedMetrics()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(0, 0, "Hello", 10000, "ГОСТ Б");

        var expected = (long)(10000 * 5 * 0.55);
        Assert.Equal(expected, text.WidthMicrons);
    }

    [Fact]
    public void WidthMicrons_CorrectedMetrics_UsesMaxLine()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(0, 0, "A\nLongLine!", 10000, "ГОСТ Б");

        var expected = (long)(10000 * 9 * 0.55); // max line = "LongLine!" (9 chars)
        Assert.Equal(expected, text.WidthMicrons);
    }

    [Fact]
    public void HeightMicrons_Fallback_MatchesPreviousBehavior()
    {
        var text = new Text(0, 0, "Hello", 3500);
        Assert.Equal(3500, text.HeightMicrons);
    }

    [Fact]
    public void WidthMicrons_Fallback_GostA_MatchesPreviousBehavior()
    {
        var text = new Text(0, 0, "Hello", 3500);
        Assert.Equal(8750, text.WidthMicrons); // 3500 * 5 * 0.5
    }

    // === RotatedCorner* with corrected metrics ===

    [Fact]
    public void RotatedCorner_NoRotation_UsesCorrectedHeight()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(1000, 2000, "Hi", 10000, "ГОСТ Б");
        var expectedH = (long)(10000 * 1.1719);

        // At 0°: corner 0 = (X, Y+H), corner 1 = (X+W, Y+H), corner 2 = (X, Y), corner 3 = (X+W, Y)
        Assert.Equal(text.MicronsX, text.RotatedCorner0X);
        Assert.Equal(text.MicronsY + expectedH, text.RotatedCorner0Y);
        Assert.Equal(text.MicronsX + text.WidthMicrons, text.RotatedCorner1X);
        Assert.Equal(text.MicronsY + expectedH, text.RotatedCorner1Y);
    }

    [Fact]
    public void RotatedCorner_90Deg_UsesCorrectedDimensions()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(1000, 2000, "Hi", 10000, "ГОСТ Б", rotationAngle: 90);
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;

        // At 90° CW (standard CCW matrix in Y-down = CW):
        // corner0 = (X, Y+H)                     = pivot
        // corner1 = (X + W*cos90, Y+H - W*sin90) = (X, Y+H - W)
        // corner2 = (X - H*sin90, Y+H - H*cos90) = (X - H, Y+H)
        // corner3 = (X + W*cos90 - H*sin90, Y+H - W*sin90 - H*cos90) = (X - H, Y+H - W)
        var cos90 = Math.Cos(Math.PI / 2);
        var sin90 = Math.Sin(Math.PI / 2);

        Assert.Equal(1000, text.RotatedCorner0X);
        Assert.Equal(2000 + h, text.RotatedCorner0Y);

        Assert.Equal(1000 + (long)Math.Round(w * cos90), text.RotatedCorner1X);
        Assert.Equal(2000 + h - (long)Math.Round(w * sin90), text.RotatedCorner1Y);

        Assert.Equal(1000 - (long)Math.Round(h * sin90), text.RotatedCorner2X);
        Assert.Equal(2000 + h - (long)Math.Round(h * cos90), text.RotatedCorner2Y);

        Assert.Equal(1000 + (long)Math.Round(w * cos90 - h * sin90), text.RotatedCorner3X);
        Assert.Equal(2000 + h - (long)Math.Round(w * sin90 + h * cos90), text.RotatedCorner3Y);
    }

    [Fact]
    public void RotatedCorner_180Deg_UsesCorrectedDimensions()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(1000, 2000, "Hi", 10000, "ГОСТ Б", rotationAngle: 180);
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;
        var cos180 = Math.Cos(Math.PI);
        var sin180 = Math.Sin(Math.PI);

        Assert.Equal(1000, text.RotatedCorner0X);
        Assert.Equal(2000 + h, text.RotatedCorner0Y);

        Assert.Equal(1000 + (long)Math.Round(w * cos180), text.RotatedCorner1X);
        Assert.Equal(2000 + h - (long)Math.Round(w * sin180), text.RotatedCorner1Y);

        Assert.Equal(1000 - (long)Math.Round(h * sin180), text.RotatedCorner2X);
        Assert.Equal(2000 + h - (long)Math.Round(h * cos180), text.RotatedCorner2Y);
    }

    [Fact]
    public void RotatedCorner_270Deg_UsesCorrectedDimensions()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(1000, 2000, "Hi", 10000, "ГОСТ Б", rotationAngle: 270);
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;
        var cos270 = Math.Cos(3 * Math.PI / 2);
        var sin270 = Math.Sin(3 * Math.PI / 2);

        Assert.Equal(1000, text.RotatedCorner0X);
        Assert.Equal(2000 + h, text.RotatedCorner0Y);

        // At 270° CW (standard CCW matrix in Y-down = CW): sin270=-1
        // corner1: (X, Y+H - W*(-1)) = (X, Y+H+W)
        Assert.Equal(1000 + (long)Math.Round(w * cos270), text.RotatedCorner1X);
        Assert.Equal(2000 + h - (long)Math.Round(w * sin270), text.RotatedCorner1Y);

        // corner2: (X - H*(-1), Y+H - H*0) = (X+H, Y+H)
        Assert.Equal(1000 - (long)Math.Round(h * sin270), text.RotatedCorner2X);
        Assert.Equal(2000 + h - (long)Math.Round(h * cos270), text.RotatedCorner2Y);
    }

    // === ContainsPoint with corrected metrics ===

    [Fact]
    public void ContainsPoint_CorrectedMetrics_RotatedText()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(0, 0, "Hi", 10000, "ГОСТ Б", rotationAngle: 0);
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;

        // Center of text should hit
        var center = new PointMicrons(w / 2, h / 2);
        Assert.True(text.ContainsPoint(center));

        // Outside should not hit
        var outside = new PointMicrons(w + 1000, h + 1000);
        Assert.False(text.ContainsPoint(outside));
    }

    [Fact]
    public void RotatedCorner_45Deg_MatchesCwRotation()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(1000, 2000, "Hi", 10000, "ГОСТ Б", rotationAngle: 45);
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;
        var cos45 = Math.Cos(Math.PI / 4);
        var sin45 = Math.Sin(Math.PI / 4);

        Assert.Equal(1000, text.RotatedCorner0X);
        Assert.Equal(2000 + h, text.RotatedCorner0Y);

        // Corner 1 (local W, 0): X = X + W·cos45, Y = Y+H - W·sin45
        Assert.Equal(1000 + (long)Math.Round(w * cos45), text.RotatedCorner1X);
        Assert.Equal(2000 + h - (long)Math.Round(w * sin45), text.RotatedCorner1Y);

        // Corner 2 (local 0, H): X = X - H·sin45, Y = Y+H - H·cos45
        Assert.Equal(1000 - (long)Math.Round(h * sin45), text.RotatedCorner2X);
        Assert.Equal(2000 + h - (long)Math.Round(h * cos45), text.RotatedCorner2Y);

        // Corner 3 (local W, H): X = X + W·cos45 - H·sin45, Y = Y+H - W·sin45 - H·cos45
        Assert.Equal(1000 + (long)Math.Round(w * cos45 - h * sin45), text.RotatedCorner3X);
        Assert.Equal(2000 + h - (long)Math.Round(w * sin45 + h * cos45), text.RotatedCorner3Y);
    }

    [Fact]
    public void ContainsPoint_Rotated90Deg_HitsVisualCorner()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(0, 0, "Hi", 10000, "ГОСТ Б", rotationAngle: 90);
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;

        // With CW 90° rotation (standard CCW matrix), corners are at:
        // (0, H), (0, H-W), (-H, H), (-H, H-W)
        // AABB center: (-H/2, H - W/2) — must hit with inverse transform

        var center = new PointMicrons(-h / 2, h - w / 2);
        Assert.True(text.ContainsPoint(center));

        // Point clearly outside the rotated AABB must NOT hit
        var outside = new PointMicrons(h + 1000, h + w + 1000);
        Assert.False(text.ContainsPoint(outside));
    }

    [Fact]
    public void GetBoundingBox_Rotated90Deg_CorrectBounds()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(0, 0, "Hi", 10000, "ГОСТ Б", rotationAngle: 90);
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;

        // With CW 90° rotation (standard CCW matrix in Y-down), corners are at:
        // (0, H), (0, H-W), (-H, H), (-H, H-W)
        // Bounding box: X∈[-H, 0], Y∈[H-W, H] (model Y↑)
        var bb = text.GetBoundingBox();

        Assert.Equal(-h, bb.Left);
        Assert.Equal(h - w, bb.Bottom);
        Assert.Equal(0, bb.Right);
        Assert.Equal(h, bb.Top);
    }
}
