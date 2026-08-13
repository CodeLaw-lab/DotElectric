using System.Windows;
using DotElectric.TemplateEditor.Behaviors;

namespace DotElectric.TemplateEditor.Tests.Behaviors;

public class CoordinateTransformTests
{
    // ===== ToModelPoint: origin =====

    [Fact]
    public void ToModelPoint_Origin_ReturnsSheetHeightMicrons()
    {
        var result = CoordinateTransform.ToModelPoint(new Point(0, 0), zoom: 1.0, sheetHeightMm: 297);

        Assert.Equal(0, result.MicronsX);
        Assert.Equal(297_000, result.MicronsY);
    }

    [Fact]
    public void ToModelPoint_BottomOfSheet_ReturnsZeroY()
    {
        // WPF Y-down: точка у нижнего края листа (y = высота листа) → модель Y = 0
        var result = CoordinateTransform.ToModelPoint(new Point(0, 297), zoom: 1.0, sheetHeightMm: 297);

        Assert.Equal(0, result.MicronsX);
        Assert.Equal(0, result.MicronsY);
    }

    // ===== ToModelPoint: zoom scaling =====

    [Fact]
    public void ToModelPoint_ZoomTwo_HalvesValues()
    {
        var result = CoordinateTransform.ToModelPoint(new Point(210, 297), zoom: 2.0, sheetHeightMm: 297);

        Assert.Equal(105_000, result.MicronsX);
        Assert.Equal(148_500, result.MicronsY);
    }

    [Fact]
    public void ToModelPoint_ZoomTwo_PointAtHalfHeight_ReturnsQuarterSheetY()
    {
        // 297/2 = 148.5 пикселя при zoom 2 → модель 74.25 мм от низа
        var result = CoordinateTransform.ToModelPoint(new Point(0, 148.5), zoom: 2.0, sheetHeightMm: 297);

        Assert.Equal(0, result.MicronsX);
        Assert.Equal(222_750, result.MicronsY);
    }

    // ===== ToModelPoint: Y-flip =====

    [Fact]
    public void ToModelPoint_YFlip_MapsTopToZeroAndBottomToSheetHeight()
    {
        var top = CoordinateTransform.ToModelPoint(new Point(0, 0), zoom: 1.0, sheetHeightMm: 420);
        var bottom = CoordinateTransform.ToModelPoint(new Point(0, 420), zoom: 1.0, sheetHeightMm: 420);

        Assert.Equal(420_000, top.MicronsY);
        Assert.Equal(0, bottom.MicronsY);
    }

    // ===== ToModelPoint: microns rounding =====

    [Fact]
    public void ToModelPoint_RoundsToNearestMicron()
    {
        // x = 0.0004 мм → 0.4 мкм → округление 0; y = 296.9994 мм → 296999.4 мкм → 296999
        var result = CoordinateTransform.ToModelPoint(new Point(0.0004, 0.0006), zoom: 1.0, sheetHeightMm: 297);

        Assert.Equal(0, result.MicronsX);
        Assert.Equal(296_999, result.MicronsY);
    }

    // ===== ToModelPoint: zero zoom guard =====

    [Fact]
    public void ToModelPoint_ZeroZoom_ReturnsSaturatedValuesWithoutThrowing()
    {
        // Zoom=0 в проде невозможен (SetZoom клиппит), но defensive-поведение детерминировано:
        // деление на 0 даёт ±Infinity, (long)Math.Round(±Infinity) насыщается в long.Min/MaxValue
        // (НЕ бросает OverflowException — проверено на .NET 10).
        var result = CoordinateTransform.ToModelPoint(new Point(10, 10), zoom: 0.0, sheetHeightMm: 297);

        Assert.Equal(long.MaxValue, result.MicronsX);
        Assert.Equal(long.MinValue, result.MicronsY);
    }
}
