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
    public void IsEditable_Default_IsTrue()
    {
        var text = new Text(0, 0, "Test", 2500);
        Assert.True(text.IsEditable);
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
        var (minX, minY) = ExpectedOffset(w, h, 90);
        var cos90 = Math.Cos(Math.PI / 2);
        var sin90 = Math.Sin(Math.PI / 2);

        // With LayoutTransform offset (-minX, +minY) applied to anchor (X, Y+H):
        // corner0 = (X - minX, Y+H + minY)
        Assert.Equal(1000 - minX, text.RotatedCorner0X);
        Assert.Equal(2000 + h + minY, text.RotatedCorner0Y);

        // corner1 = (X + W*cos - minX, Y+H - W*sin + minY)
        Assert.Equal(1000 + (long)Math.Round(w * cos90) - minX, text.RotatedCorner1X);
        Assert.Equal(2000 + h - (long)Math.Round(w * sin90) + minY, text.RotatedCorner1Y);

        // corner2 = (X - H*sin - minX, Y+H - H*cos + minY)
        Assert.Equal(1000 - (long)Math.Round(h * sin90) - minX, text.RotatedCorner2X);
        Assert.Equal(2000 + h - (long)Math.Round(h * cos90) + minY, text.RotatedCorner2Y);

        // corner3 = (X + W*cos - H*sin - minX, Y+H - W*sin - H*cos + minY)
        Assert.Equal(1000 + (long)Math.Round(w * cos90 - h * sin90) - minX, text.RotatedCorner3X);
        Assert.Equal(2000 + h - (long)Math.Round(w * sin90 + h * cos90) + minY, text.RotatedCorner3Y);
    }

    [Fact]
    public void RotatedCorner_180Deg_UsesCorrectedDimensions()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(1000, 2000, "Hi", 10000, "ГОСТ Б", rotationAngle: 180);
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;
        var (minX, minY) = ExpectedOffset(w, h, 180);
        var cos180 = Math.Cos(Math.PI);
        var sin180 = Math.Sin(Math.PI);

        Assert.Equal(1000 - minX, text.RotatedCorner0X);
        Assert.Equal(2000 + h + minY, text.RotatedCorner0Y);

        Assert.Equal(1000 + (long)Math.Round(w * cos180) - minX, text.RotatedCorner1X);
        Assert.Equal(2000 + h - (long)Math.Round(w * sin180) + minY, text.RotatedCorner1Y);

        Assert.Equal(1000 - (long)Math.Round(h * sin180) - minX, text.RotatedCorner2X);
        Assert.Equal(2000 + h - (long)Math.Round(h * cos180) + minY, text.RotatedCorner2Y);
    }

    [Fact]
    public void RotatedCorner_270Deg_UsesCorrectedDimensions()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(1000, 2000, "Hi", 10000, "ГОСТ Б", rotationAngle: 270);
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;
        var (minX, minY) = ExpectedOffset(w, h, 270);
        var cos270 = Math.Cos(3 * Math.PI / 2);
        var sin270 = Math.Sin(3 * Math.PI / 2);

        Assert.Equal(1000 - minX, text.RotatedCorner0X);
        Assert.Equal(2000 + h + minY, text.RotatedCorner0Y);

        // At 270° CW (standard CCW matrix in Y-down = CW): sin270=-1
        Assert.Equal(1000 + (long)Math.Round(w * cos270) - minX, text.RotatedCorner1X);
        Assert.Equal(2000 + h - (long)Math.Round(w * sin270) + minY, text.RotatedCorner1Y);

        Assert.Equal(1000 - (long)Math.Round(h * sin270) - minX, text.RotatedCorner2X);
        Assert.Equal(2000 + h - (long)Math.Round(h * cos270) + minY, text.RotatedCorner2Y);
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
        var (minX, minY) = ExpectedOffset(w, h, 45);
        var cos45 = Math.Cos(Math.PI / 4);
        var sin45 = Math.Sin(Math.PI / 4);

        // Corner0 = anchor + offset = (X - minX, Y+H + minY)
        Assert.Equal(1000 - minX, text.RotatedCorner0X);
        Assert.Equal(2000 + h + minY, text.RotatedCorner0Y);

        // Corner 1 (local W, 0): X = X + W·cos45 - minX, Y = Y+H - W·sin45 + minY
        Assert.Equal(1000 + (long)Math.Round(w * cos45) - minX, text.RotatedCorner1X);
        Assert.Equal(2000 + h - (long)Math.Round(w * sin45) + minY, text.RotatedCorner1Y);

        // Corner 2 (local 0, H): X = X - H·sin45 - minX, Y = Y+H - H·cos45 + minY
        Assert.Equal(1000 - (long)Math.Round(h * sin45) - minX, text.RotatedCorner2X);
        Assert.Equal(2000 + h - (long)Math.Round(h * cos45) + minY, text.RotatedCorner2Y);

        // Corner 3 (local W, H): X = X + W·cos45 - H·sin45 - minX, Y = Y+H - W·sin45 - H·cos45 + minY
        Assert.Equal(1000 + (long)Math.Round(w * cos45 - h * sin45) - minX, text.RotatedCorner3X);
        Assert.Equal(2000 + h - (long)Math.Round(w * sin45 + h * cos45) + minY, text.RotatedCorner3Y);
    }

    [Fact]
    public void ContainsPoint_Rotated90Deg_HitsVisualCorner()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(0, 0, "Hi", 10000, "ГОСТ Б", rotationAngle: 90);
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;
        var (minX, minY) = ExpectedOffset(w, h, 90);

        // With LayoutTransform offset, actual rotation center is at:
        // centerX = X - minX, centerY = Y + H + minY
        // Visual center: local (W/2, H/2) rotated, then mapped to model space.
        var centerX = 0 - minX;
        var centerY = 0 + h + minY;
        var angleRad = 90 * Math.PI / 180.0;
        var localCx = w / 2.0 * Math.Cos(angleRad) - h / 2.0 * Math.Sin(angleRad);
        var localCy = w / 2.0 * Math.Sin(angleRad) + h / 2.0 * Math.Cos(angleRad);
        var visualCenter = new PointMicrons(
            (long)(centerX + localCx),
            (long)(centerY - localCy));

        Assert.True(text.ContainsPoint(visualCenter),
            $"Visual center {visualCenter} should hit text at 90°");

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
        var (minX, minY) = ExpectedOffset(w, h, 90);

        // With LayoutTransform offset: center = (-minX, H + minY).
        // At 90°: offset = (+H, 0). Center = (H, H).
        // Corners (local Y-down): (0,0)→(0,0), (W,0)→(0,W), (0,H)→(-H,0), (W,H)→(-H,W)
        // Model: center + rotated_x, center_y - rotated_y
        var centerX = 0 - minX;   // = H
        var centerY = 0 + h + minY; // = H

        var bb = text.GetBoundingBox();
        // All 4 corners' model X: centerX+0, centerX+0, centerX-H, centerX-H → [centerX-H, centerX]
        // All 4 corners' model Y: centerY-0, centerY-W, centerY-0, centerY-W → [centerY-W, centerY]
        Assert.Equal(centerX - h, bb.Left);
        Assert.Equal(centerY - w, bb.Bottom);
        Assert.Equal(centerX, bb.Right);
        Assert.Equal(centerY, bb.Top);
    }

    // === New verification tests for LayoutTransform offset fix ===

    [Theory]
    [InlineData(0)]
    [InlineData(45)]
    [InlineData(90)]
    [InlineData(135)]
    [InlineData(180)]
    [InlineData(270)]
    public void RotatedCorner_WithOffset_MatchesVisualLayoutTransformPosition(int angle)
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(10000, 20000, "Hi", 10000, "ГОСТ Б", rotationAngle: angle);
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;
        var (minX, minY) = ExpectedOffset(w, h, angle);

        // Corner0 = anchor + offset = (MicronsX - minX, MicronsY + H + minY)
        Assert.Equal(text.MicronsX - minX, text.RotatedCorner0X);
        Assert.Equal(text.MicronsY + h + minY, text.RotatedCorner0Y);
    }

    [Theory]
    [InlineData(45)]
    [InlineData(90)]
    [InlineData(180)]
    [InlineData(270)]
    public void ContainsPoint_Rotated_HitsVisualCenter(int angle)
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(10000, 20000, "Hi", 10000, "ГОСТ Б", rotationAngle: angle);
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;
        var (minX, minY) = ExpectedOffset(w, h, angle);

        var centerX = text.MicronsX - minX;
        var centerY = text.MicronsY + h + minY;
        var angleRad = angle * Math.PI / 180.0;
        var localCx = w / 2.0 * Math.Cos(angleRad) - h / 2.0 * Math.Sin(angleRad);
        var localCy = w / 2.0 * Math.Sin(angleRad) + h / 2.0 * Math.Cos(angleRad);
        var visualCenter = new PointMicrons(
            (long)(centerX + localCx),
            (long)(centerY - localCy));

        Assert.True(text.ContainsPoint(visualCenter),
            $"Visual center {visualCenter} should hit text at {angle}°");
    }

    [Fact]
    public void GetBoundingBox_Rotated90Deg_IncludesLayoutTransformOffset()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(10000, 20000, "Hi", 10000, "ГОСТ Б", rotationAngle: 90);
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;
        var (minX, minY) = ExpectedOffset(w, h, 90);

        // At 90°: offset = (+H, 0). Center = (10000+H, 20000+H).
        // Corners (local Y-down): (0,0)→(0,0), (W,0)→(0,W), (0,H)→(-H,0), (W,H)→(-H,W)
        // Model: center + rotated_x, center_y - rotated_y
        var centerX = text.MicronsX - minX;  // 10000 + H
        var centerY = text.MicronsY + h + minY;  // 20000 + H

        var bb = text.GetBoundingBox();
        // All 4 corners' model X: centerX+0, centerX+0, centerX-H, centerX-H → [centerX-H, centerX]
        // All 4 corners' model Y: centerY-0, centerY-W, centerY-0, centerY-W → [centerY-W, centerY]
        Assert.Equal(centerX - h, bb.Left);
        Assert.Equal(centerY - w, bb.Bottom);
        Assert.Equal(centerX, bb.Right);
        Assert.Equal(centerY, bb.Top);
    }

    [Fact]
    public void RotatedCorner_0Deg_OffsetIsZero()
    {
        FontMetrics.Default.InitializeWithTestValues(1.1719, 0.55, "ГОСТ Б");
        var text = new Text(10000, 20000, "Hi", 10000, "ГОСТ Б", rotationAngle: 0);

        // At 0°: offset = (0, 0). Corner0 = (MicronsX, MicronsY+H) — unchanged.
        Assert.Equal(10000, text.RotatedCorner0X);
        Assert.Equal(20000 + text.HeightMicrons, text.RotatedCorner0Y);
    }

    // === Helper: compute expected LayoutTransform offset (minX, minY) ===

    private static (long offsetX, long offsetY) ExpectedOffset(long w, long h, int angle)
    {
        var rad = angle * Math.PI / 180.0;
        var cosA = Math.Cos(rad);
        var sinA = Math.Sin(rad);
        var c1x = w * cosA; var c1y = w * sinA;
        var c2x = -h * sinA; var c2y = h * cosA;
        var c3x = w * cosA - h * sinA; var c3y = w * sinA + h * cosA;
        var minX = Math.Min(0, Math.Min(Math.Min(c1x, c2x), c3x));
        var minY = Math.Min(0, Math.Min(Math.Min(c1y, c2y), c3y));
        return ((long)Math.Round(minX), (long)Math.Round(minY));
    }
}
