
namespace DotElectric.Document.Tests;

[Collection("FontMetrics")]
public class TextGeometryTests : IDisposable
{
    public TextGeometryTests()
    {
        FontMetricsProvider.Reset();
    }

    public void Dispose()
    {
        FontMetricsProvider.Reset();
    }

    // === LayoutOffset ===

    [Fact]
    public void LayoutOffset_0Deg_IsZero()
    {
        FontMetricsProvider.SetCurrent(new FixedFontMetrics(1.1719, 0.55));
        var text = new Text(10000, 20000, "Hi", 10000, "ГОСТ Б", rotationAngle: 0);

        // At 0°: offset = (0, 0). Corner0 = (MicronsX, MicronsY+H) — unchanged.
        var (offsetX, offsetY) = TextGeometry.LayoutOffset(text);
        Assert.Equal(0, offsetX);
        Assert.Equal(0, offsetY);
        Assert.Equal(new PointMicrons(10000, 20000 + text.HeightMicrons), TextGeometry.Corner(text, 0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(45)]
    [InlineData(90)]
    [InlineData(135)]
    [InlineData(180)]
    [InlineData(270)]
    public void LayoutOffset_MatchesVisualLayoutTransformPosition(int angle)
    {
        FontMetricsProvider.SetCurrent(new FixedFontMetrics(1.1719, 0.55));
        var text = new Text(10000, 20000, "Hi", 10000, "ГОСТ Б", rotationAngle: angle);
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;
        var (minX, minY) = ExpectedOffset(w, h, angle);

        var (offsetX, offsetY) = TextGeometry.LayoutOffset(text);
        Assert.Equal(minX, offsetX);
        Assert.Equal(minY, offsetY);

        // Corner0 = anchor + offset = (MicronsX - minX, MicronsY + H + minY)
        Assert.Equal(new PointMicrons(text.MicronsX - minX, text.MicronsY + h + minY), TextGeometry.Corner(text, 0));
    }

    // === Corner с корректными метриками ===

    [Fact]
    public void Corner_NoRotation_UsesCorrectedHeight()
    {
        FontMetricsProvider.SetCurrent(new FixedFontMetrics(1.1719, 0.55));
        var text = new Text(1000, 2000, "Hi", 10000, "ГОСТ Б");
        var expectedH = (long)(10000 * 1.1719);

        // At 0°: corner 0 = (X, Y+H), corner 1 = (X+W, Y+H), corner 2 = (X, Y), corner 3 = (X+W, Y)
        Assert.Equal(new PointMicrons(text.MicronsX, text.MicronsY + expectedH), TextGeometry.Corner(text, 0));
        Assert.Equal(new PointMicrons(text.MicronsX + text.WidthMicrons, text.MicronsY + expectedH), TextGeometry.Corner(text, 1));
    }

    [Fact]
    public void Corner_90Deg_UsesCorrectedDimensions()
    {
        FontMetricsProvider.SetCurrent(new FixedFontMetrics(1.1719, 0.55));
        var text = new Text(1000, 2000, "Hi", 10000, "ГОСТ Б", rotationAngle: 90);
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;
        var (minX, minY) = ExpectedOffset(w, h, 90);
        var cos90 = Math.Cos(Math.PI / 2);
        var sin90 = Math.Sin(Math.PI / 2);

        // With LayoutTransform offset (-minX, +minY) applied to anchor (X, Y+H):
        // corner0 = (X - minX, Y+H + minY)
        Assert.Equal(new PointMicrons(1000 - minX, 2000 + h + minY), TextGeometry.Corner(text, 0));

        // corner1 = (X + W*cos - minX, Y+H - W*sin + minY)
        Assert.Equal(
            new PointMicrons(1000 + (long)Math.Round(w * cos90) - minX, 2000 + h - (long)Math.Round(w * sin90) + minY),
            TextGeometry.Corner(text, 1));

        // corner2 = (X - H*sin - minX, Y+H - H*cos + minY)
        Assert.Equal(
            new PointMicrons(1000 - (long)Math.Round(h * sin90) - minX, 2000 + h - (long)Math.Round(h * cos90) + minY),
            TextGeometry.Corner(text, 2));

        // corner3 = (X + W*cos - H*sin - minX, Y+H - W*sin - H*cos + minY)
        Assert.Equal(
            new PointMicrons(1000 + (long)Math.Round(w * cos90 - h * sin90) - minX, 2000 + h - (long)Math.Round(w * sin90 + h * cos90) + minY),
            TextGeometry.Corner(text, 3));
    }

    [Fact]
    public void Corner_180Deg_UsesCorrectedDimensions()
    {
        FontMetricsProvider.SetCurrent(new FixedFontMetrics(1.1719, 0.55));
        var text = new Text(1000, 2000, "Hi", 10000, "ГОСТ Б", rotationAngle: 180);
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;
        var (minX, minY) = ExpectedOffset(w, h, 180);
        var cos180 = Math.Cos(Math.PI);
        var sin180 = Math.Sin(Math.PI);

        Assert.Equal(new PointMicrons(1000 - minX, 2000 + h + minY), TextGeometry.Corner(text, 0));

        Assert.Equal(
            new PointMicrons(1000 + (long)Math.Round(w * cos180) - minX, 2000 + h - (long)Math.Round(w * sin180) + minY),
            TextGeometry.Corner(text, 1));

        Assert.Equal(
            new PointMicrons(1000 - (long)Math.Round(h * sin180) - minX, 2000 + h - (long)Math.Round(h * cos180) + minY),
            TextGeometry.Corner(text, 2));
    }

    [Fact]
    public void Corner_270Deg_UsesCorrectedDimensions()
    {
        FontMetricsProvider.SetCurrent(new FixedFontMetrics(1.1719, 0.55));
        var text = new Text(1000, 2000, "Hi", 10000, "ГОСТ Б", rotationAngle: 270);
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;
        var (minX, minY) = ExpectedOffset(w, h, 270);
        var cos270 = Math.Cos(3 * Math.PI / 2);
        var sin270 = Math.Sin(3 * Math.PI / 2);

        Assert.Equal(new PointMicrons(1000 - minX, 2000 + h + minY), TextGeometry.Corner(text, 0));

        // At 270° CW (standard CCW matrix in Y-down = CW): sin270=-1
        Assert.Equal(
            new PointMicrons(1000 + (long)Math.Round(w * cos270) - minX, 2000 + h - (long)Math.Round(w * sin270) + minY),
            TextGeometry.Corner(text, 1));

        Assert.Equal(
            new PointMicrons(1000 - (long)Math.Round(h * sin270) - minX, 2000 + h - (long)Math.Round(h * cos270) + minY),
            TextGeometry.Corner(text, 2));
    }

    [Fact]
    public void Corner_45Deg_MatchesCwRotation()
    {
        FontMetricsProvider.SetCurrent(new FixedFontMetrics(1.1719, 0.55));
        var text = new Text(1000, 2000, "Hi", 10000, "ГОСТ Б", rotationAngle: 45);
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;
        var (minX, minY) = ExpectedOffset(w, h, 45);
        var cos45 = Math.Cos(Math.PI / 4);
        var sin45 = Math.Sin(Math.PI / 4);

        // Corner0 = anchor + offset = (X - minX, Y+H + minY)
        Assert.Equal(new PointMicrons(1000 - minX, 2000 + h + minY), TextGeometry.Corner(text, 0));

        // Corner 1 (local W, 0): X = X + W·cos45 - minX, Y = Y+H - W·sin45 + minY
        Assert.Equal(
            new PointMicrons(1000 + (long)Math.Round(w * cos45) - minX, 2000 + h - (long)Math.Round(w * sin45) + minY),
            TextGeometry.Corner(text, 1));

        // Corner 2 (local 0, H): X = X - H·sin45 - minX, Y = Y+H - H·cos45 + minY
        Assert.Equal(
            new PointMicrons(1000 - (long)Math.Round(h * sin45) - minX, 2000 + h - (long)Math.Round(h * cos45) + minY),
            TextGeometry.Corner(text, 2));

        // Corner 3 (local W, H): X = X + W·cos45 - H·sin45 - minX, Y = Y+H - W·sin45 - H·cos45 + minY
        Assert.Equal(
            new PointMicrons(1000 + (long)Math.Round(w * cos45 - h * sin45) - minX, 2000 + h - (long)Math.Round(w * sin45 + h * cos45) + minY),
            TextGeometry.Corner(text, 3));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void Corner_IndexOutOfRange_Throws(int index)
    {
        var text = new Text(0, 0, "Hi", 10000);

        Assert.Throws<NotSupportedException>(() => TextGeometry.Corner(text, index));
    }

    // === Contains с корректными метриками ===

    [Fact]
    public void Contains_CorrectedMetrics_RotatedText()
    {
        FontMetricsProvider.SetCurrent(new FixedFontMetrics(1.1719, 0.55));
        var text = new Text(0, 0, "Hi", 10000, "ГОСТ Б", rotationAngle: 0);
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;

        // Center of text should hit
        var center = new PointMicrons(w / 2, h / 2);
        Assert.True(TextGeometry.Contains(text, center));

        // Outside should not hit
        var outside = new PointMicrons(w + 1000, h + 1000);
        Assert.False(TextGeometry.Contains(text, outside));
    }

    [Fact]
    public void Contains_Rotated90Deg_HitsVisualCorner()
    {
        FontMetricsProvider.SetCurrent(new FixedFontMetrics(1.1719, 0.55));
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

        Assert.True(TextGeometry.Contains(text, visualCenter),
            $"Visual center {visualCenter} should hit text at 90°");

        // Point clearly outside the rotated AABB must NOT hit
        var outside = new PointMicrons(h + 1000, h + w + 1000);
        Assert.False(TextGeometry.Contains(text, outside));
    }

    [Theory]
    [InlineData(45)]
    [InlineData(90)]
    [InlineData(180)]
    [InlineData(270)]
    public void Contains_Rotated_HitsVisualCenter(int angle)
    {
        FontMetricsProvider.SetCurrent(new FixedFontMetrics(1.1719, 0.55));
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

        Assert.True(TextGeometry.Contains(text, visualCenter),
            $"Visual center {visualCenter} should hit text at {angle}°");
    }

    // === BoundingBox с корректными метриками ===

    [Fact]
    public void BoundingBox_Rotated90Deg_CorrectBounds()
    {
        FontMetricsProvider.SetCurrent(new FixedFontMetrics(1.1719, 0.55));
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

        var bb = TextGeometry.BoundingBox(text);
        // All 4 corners' model X: centerX+0, centerX+0, centerX-H, centerX-H → [centerX-H, centerX]
        // All 4 corners' model Y: centerY-0, centerY-W, centerY-0, centerY-W → [centerY-W, centerY]
        Assert.Equal(centerX - h, bb.Left);
        Assert.Equal(centerY - w, bb.Bottom);
        Assert.Equal(centerX, bb.Right);
        Assert.Equal(centerY, bb.Top);
    }

    [Fact]
    public void BoundingBox_Rotated90Deg_IncludesLayoutTransformOffset()
    {
        FontMetricsProvider.SetCurrent(new FixedFontMetrics(1.1719, 0.55));
        var text = new Text(10000, 20000, "Hi", 10000, "ГОСТ Б", rotationAngle: 90);
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;
        var (minX, minY) = ExpectedOffset(w, h, 90);

        // At 90°: offset = (+H, 0). Center = (10000+H, 20000+H).
        // Corners (local Y-down): (0,0)→(0,0), (W,0)→(0,W), (0,H)→(-H,0), (W,H)→(-H,W)
        // Model: center + rotated_x, center_y - rotated_y
        var centerX = text.MicronsX - minX;  // 10000 + H
        var centerY = text.MicronsY + h + minY;  // 20000 + H

        var bb = TextGeometry.BoundingBox(text);
        // All 4 corners' model X: centerX+0, centerX+0, centerX-H, centerX-H → [centerX-H, centerX]
        // All 4 corners' model Y: centerY-0, centerY-W, centerY-0, centerY-W → [centerY-W, centerY]
        Assert.Equal(centerX - h, bb.Left);
        Assert.Equal(centerY - w, bb.Bottom);
        Assert.Equal(centerX, bb.Right);
        Assert.Equal(centerY, bb.Top);
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
