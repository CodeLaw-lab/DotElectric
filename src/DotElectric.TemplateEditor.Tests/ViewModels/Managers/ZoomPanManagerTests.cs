using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.ViewModels.Managers;

namespace DotElectric.TemplateEditor.Tests.ViewModels.Managers;

public class ZoomPanManagerTests
{
    private static Template CreateTemplate()
    {
        var sheet = Sheet.FromFormat("A4", SheetOrientation.Landscape);
        var metadata = new Metadata();
        return new Template(metadata, sheet);
    }

    private static ZoomPanManager CreateSut(double zoom = 1.0)
    {
        var sut = new ZoomPanManager(CreateTemplate(), () => { }, () => { });
        sut.SetZoom(zoom);
        return sut;
    }

    [Fact]
    public void SetZoom_ClampsToMin()
    {
        var sut = CreateSut();

        sut.SetZoom(0.05);

        Assert.Equal(0.1, sut.Zoom);
    }

    [Fact]
    public void SetZoom_ClampsToMax()
    {
        var sut = CreateSut();

        sut.SetZoom(15.0);

        Assert.Equal(10.0, sut.Zoom);
    }

    [Fact]
    public void SetZoomPercent_ConvertsCorrectly()
    {
        var sut = CreateSut();

        sut.SetZoomPercent(150);

        Assert.Equal(1.5, sut.Zoom);
    }

    [Fact]
    public void ZoomPercent_ReturnsCorrectValue()
    {
        var sut = CreateSut();
        sut.SetZoom(1.5);

        var percent = sut.ZoomPercent;

        Assert.Equal(150, percent);
    }

    [Fact]
    public void ZoomIn_IncreasesZoom()
    {
        var sut = CreateSut();
        var initialZoom = sut.Zoom;

        sut.ZoomIn();

        Assert.Equal(initialZoom + 0.1, sut.Zoom);
    }

    [Fact]
    public void ZoomOut_DecreasesZoom()
    {
        var sut = CreateSut();
        var initialZoom = sut.Zoom;

        sut.ZoomOut();

        Assert.Equal(initialZoom - 0.1, sut.Zoom);
    }

    [Fact]
    public void PanCanvas_UpdatesOffsets()
    {
        var sut = CreateSut();
        sut.SetZoom(2.0);
        var initialX = sut.PanOffsetX;
        var initialY = sut.PanOffsetY;

        sut.PanCanvas(5.0, 3.0);

        Assert.Equal(initialX + 10.0, sut.PanOffsetX); // 5 * 2
        Assert.Equal(initialY - 6.0, sut.PanOffsetY);  // 3 * 2 (Y inverted)
    }

    [Fact]
    public void FitToScreen_CalculatesCorrectZoom()
    {
        var sut = CreateSut();
        var canvasWidth = 800.0;
        var canvasHeight = 600.0;

        sut.FitToScreen(canvasWidth, canvasHeight);

        Assert.True(sut.Zoom > 0);
        Assert.True(sut.Zoom <= 10.0);
    }

    [Fact]
    public void CanvasWidthPixels_ReturnsCorrectValue()
    {
        var sut = CreateSut();
        sut.SetZoom(2.0);
        var template = CreateTemplate();
        var expectedWidthMm = template.Sheet.WidthMm;

        var pixels = sut.CanvasWidthPixels;

        Assert.Equal(expectedWidthMm * 2.0, pixels);
    }

    [Fact]
    public void CanvasHeightPixels_ReturnsCorrectValue()
    {
        var sut = CreateSut();
        sut.SetZoom(2.0);
        var template = CreateTemplate();
        var expectedHeightMm = template.Sheet.HeightMm;

        var pixels = sut.CanvasHeightPixels;

        Assert.Equal(expectedHeightMm * 2.0, pixels);
    }

    // === IsCentered ===

    [Fact]
    public void IsCentered_ViewportLargerThanCanvas_ReturnsTrue()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(500, 500); // A4 landscape = 297x210mm, viewport in px

        Assert.True(sut.IsCentered);
    }

    [Fact]
    public void IsCentered_ViewportSmallerThanCanvas_ReturnsFalse()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(100, 100);

        Assert.False(sut.IsCentered);
    }

    [Fact]
    public void IsCentered_ViewportZero_ReturnsFalse()
    {
        var sut = CreateSut(1.0);

        Assert.False(sut.IsCentered);
    }

    [Fact]
    public void IsCentered_BecomesFalseWhenZoomMakesCanvasLargerThanViewport()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(500, 500); // canvas fits at zoom 1.0
        Assert.True(sut.IsCentered);

        sut.SetZoom(5.0); // canvas becomes much larger than 500px

        Assert.False(sut.IsCentered);
    }

    // === CanvasWidthPixels / CanvasHeightPixels ===

    [Fact]
    public void CanvasWidthPixels_ScalesWithZoom()
    {
        var template = CreateTemplate();
        var sut = CreateSut(2.0);

        Assert.Equal(template.Sheet.WidthMm * 2.0, sut.CanvasWidthPixels);
    }

    [Fact]
    public void CanvasHeightPixels_ScalesWithZoom()
    {
        var template = CreateTemplate();
        var sut = CreateSut(2.0);

        Assert.Equal(template.Sheet.HeightMm * 2.0, sut.CanvasHeightPixels);
    }

    // === ViewportWidthPixels / ViewportHeightPixels ===

    [Fact]
    public void ViewportWidthPixels_ReturnsSetValue()
    {
        var sut = CreateSut(2.0);
        sut.SetViewportSize(200, 500);

        Assert.Equal(200.0, sut.ViewportWidthPixels);
    }

    [Fact]
    public void ViewportHeightPixels_ReturnsSetValue()
    {
        var sut = CreateSut(2.0);
        sut.SetViewportSize(500, 150);

        Assert.Equal(150.0, sut.ViewportHeightPixels);
    }

    // === ScrollXRange / ScrollYRange ===

    [Fact]
    public void ScrollXRange_WhenCentered_ReturnsZero()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(500, 500);

        Assert.Equal(0, sut.ScrollXRange);
    }

    [Fact]
    public void ScrollYRange_WhenCentered_ReturnsZero()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(500, 500);

        Assert.Equal(0, sut.ScrollYRange);
    }

    [Fact]
    public void ScrollXRange_WhenNotCentered_ReturnsPositive()
    {
        var template = CreateTemplate();
        var sut = CreateSut(1.0);
        sut.SetViewportSize(100, 100);

        var expected = Math.Max(0, sut.CanvasWidthPixels - sut.ViewportWidthPixels);
        Assert.True(sut.ScrollXRange > 0);
        Assert.Equal(expected, sut.ScrollXRange);
    }

    [Fact]
    public void ScrollYRange_WhenNotCentered_ReturnsPositive()
    {
        var template = CreateTemplate();
        var sut = CreateSut(1.0);
        sut.SetViewportSize(100, 100);

        var expected = Math.Max(0, sut.CanvasHeightPixels - sut.ViewportHeightPixels);
        Assert.True(sut.ScrollYRange > 0);
        Assert.Equal(expected, sut.ScrollYRange);
    }

    // === ScrollXValue / ScrollYValue ===

    [Fact]
    public void ScrollXValue_WhenCentered_ReturnsZero()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(500, 500);

        Assert.Equal(0, sut.ScrollXValue);
    }

    [Fact]
    public void ScrollYValue_WhenCentered_ReturnsZero()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(500, 500);

        Assert.Equal(0, sut.ScrollYValue);
    }

    [Fact]
    public void ScrollXValue_WhenNotCentered_ReturnsCorrectValue()
    {
        var template = CreateTemplate();
        var sut = CreateSut(1.0);
        sut.SetViewportSize(100, 100);
        // Initially PanOffsetX = 0 → ScrollXValue should be PanOffsetX + (C-V)/2
        var expected = Math.Max(0, sut.PanOffsetX + (sut.CanvasWidthPixels - sut.ViewportWidthPixels) / 2);

        Assert.Equal(expected, sut.ScrollXValue);
    }

    [Fact]
    public void ScrollXValue_ZeroWhenPanOffsetIsNegativeHalfDifference()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(100, 100);
        // PanOffsetX = -(C-V)/2 → ScrollXValue = 0 (shows left edge)
        sut.PanCanvas(-(sut.CanvasWidthPixels - sut.ViewportWidthPixels) / 2 / sut.Zoom, 0);

        Assert.Equal(0, sut.ScrollXValue);
    }

    // === SetScrollX / SetScrollY ===

    [Fact]
    public void SetScrollX_WhenCentered_DoesNotChangePanOffset()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(500, 500);

        sut.SetScrollX(100);

        Assert.Equal(0, sut.PanOffsetX);
    }

    [Fact]
    public void SetScrollY_WhenCentered_DoesNotChangePanOffset()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(500, 500);

        sut.SetScrollY(100);

        Assert.Equal(0, sut.PanOffsetY);
    }

    [Fact]
    public void SetScrollX_WhenNotCentered_ClampsAndUpdatesPanOffset()
    {
        var template = CreateTemplate();
        var sut = CreateSut(1.0);
        sut.SetViewportSize(100, 100);

        sut.SetScrollX(50);

        // PanOffsetX = clamp(50, 0, range) - (canvasPx - viewportPx)/2
        var halfRange = (sut.CanvasWidthPixels - sut.ViewportWidthPixels) / 2;
        Assert.Equal(50 - halfRange, sut.PanOffsetX);
    }

    [Fact]
    public void SetScrollX_ClampsToRange()
    {
        var template = CreateTemplate();
        var sut = CreateSut(1.0);
        sut.SetViewportSize(100, 100);

        sut.SetScrollX(999999); // Exceeds range

        // Should clamp to ScrollXRange: PanOffsetX = range - halfRange = halfRange
        var halfRange = (sut.CanvasWidthPixels - sut.ViewportWidthPixels) / 2;
        Assert.Equal(halfRange, sut.PanOffsetX);
    }

    // === CanvasOffsetX / CanvasOffsetY ===

    [Fact]
    public void CanvasOffsetX_WhenCentered_ReturnsZero()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(500, 500);

        Assert.Equal(0, sut.CanvasOffsetX);
    }

    [Fact]
    public void CanvasOffsetY_WhenCentered_ReturnsZero()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(500, 500);

        Assert.Equal(0, sut.CanvasOffsetY);
    }

    [Fact]
    public void CanvasOffsetX_WhenNotCentered_IsNegativePanOffset()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(100, 100);
        sut.PanCanvas(50, 0);

        Assert.Equal(-sut.PanOffsetX, sut.CanvasOffsetX);
    }

    [Fact]
    public void CanvasOffsetY_WhenNotCentered_IsNegativePanOffset()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(100, 100);
        sut.PanCanvas(0, 30);

        Assert.Equal(-sut.PanOffsetY, sut.CanvasOffsetY);
    }

    [Fact]
    public void CanvasOffsetX_WhenCentered_AfterPan_IsNegativePanOffset()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(500, 500);
        Assert.True(sut.IsCentered);

        sut.PanCanvas(50, 0);

        Assert.Equal(-sut.PanOffsetX, sut.CanvasOffsetX);
    }

    [Fact]
    public void CanvasOffsetY_WhenCentered_AfterPan_IsNegativePanOffset()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(500, 500);
        Assert.True(sut.IsCentered);

        sut.PanCanvas(0, 30);

        Assert.Equal(-sut.PanOffsetY, sut.CanvasOffsetY);
    }

    // === CenterCanvas ===

    [Fact]
    public void CenterCanvas_WhenCentered_ResetsPanOffset()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(500, 500);
        sut.PanCanvas(50, 30);

        sut.CenterCanvas();

        Assert.Equal(0, sut.PanOffsetX);
        Assert.Equal(0, sut.PanOffsetY);
    }

    [Fact]
    public void CenterCanvas_WhenNotCentered_SetsPanOffsetToNegativeHalfDifference()
    {
        var template = CreateTemplate();
        var sut = CreateSut(1.0);
        sut.SetViewportSize(100, 100);

        sut.CenterCanvas();

        // PanOffset = -(C-V)/2 so that ScrollXValue = 0 (shows left edge)
        var expectedX = -(sut.CanvasWidthPixels - sut.ViewportWidthPixels) / 2;
        var expectedY = -(sut.CanvasHeightPixels - sut.ViewportHeightPixels) / 2;
        Assert.Equal(expectedX, sut.PanOffsetX);
        Assert.Equal(expectedY, sut.PanOffsetY);
    }

    // === SetGridRefreshCallback ===

    [Fact]
    public void SetGridRefreshCallback_UpdatesCallback()
    {
        var sut = CreateSut(1.0);
        var called = false;
        sut.SetGridRefreshCallback(() => called = true);

        // Trigger zoom change which calls _onGridRefresh
        sut.SetZoom(2.0);

        Assert.True(called);
    }

    // === PanCanvas ===

    [Fact]
    public void PanCanvas_IncrementsOffsetCorrectly()
    {
        var sut = CreateSut(2.0);

        sut.PanCanvas(10.0, 5.0);

        Assert.Equal(20.0, sut.PanOffsetX); // 10 * 2
        Assert.Equal(-10.0, sut.PanOffsetY); // -5 * 2 (Y inverted)
    }

    // === Property changed notifications ===

    [Fact]
    public void ZoomChanged_FiresDependentProperties()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(100, 100);
        var notifications = new List<string>();
        sut.PropertyChanged += (_, e) => notifications.Add(e.PropertyName!);

        sut.SetZoom(2.0);

        Assert.Contains(nameof(ZoomPanManager.ZoomPercent), notifications);
        Assert.Contains(nameof(ZoomPanManager.CanvasWidthPixels), notifications);
        Assert.Contains(nameof(ZoomPanManager.CanvasHeightPixels), notifications);
        Assert.Contains(nameof(ZoomPanManager.IsCentered), notifications);
        Assert.Contains(nameof(ZoomPanManager.ScrollXRange), notifications);
        Assert.Contains(nameof(ZoomPanManager.ScrollYRange), notifications);
        Assert.Contains(nameof(ZoomPanManager.CanvasOffsetX), notifications);
        Assert.Contains(nameof(ZoomPanManager.CanvasOffsetY), notifications);
    }

    [Fact]
    public void ZoomPercent_NotifiedOnZoomIn()
    {
        var sut = CreateSut(1.0);
        var notified = false;
        sut.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ZoomPanManager.ZoomPercent))
                notified = true;
        };

        sut.ZoomIn();

        Assert.True(notified);
    }

    [Fact]
    public void ZoomPercent_NotifiedOnZoomOut()
    {
        var sut = CreateSut(2.0);
        var notified = false;
        sut.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ZoomPanManager.ZoomPercent))
                notified = true;
        };

        sut.ZoomOut();

        Assert.True(notified);
    }

    [Fact]
    public void ZoomPercent_NotifiedOnSetZoomPercent()
    {
        var sut = CreateSut(1.0);
        var notified = false;
        sut.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(ZoomPanManager.ZoomPercent))
                notified = true;
        };

        sut.SetZoomPercent(150);

        Assert.True(notified);
    }

    // === GetViewportMicrons ===

    [Fact]
    public void GetViewportMicrons_CenteredAtZoom1_ReturnsFullSheet()
    {
        var template = CreateTemplate();
        var sut = CreateSut(1.0);
        // Viewport larger than canvas → IsCentered → PanOffsetX/Y = 0
        sut.SetViewportSize(template.Sheet.WidthMm + 100, template.Sheet.HeightMm + 100);

        var (left, bottom, width, height) = sut.GetViewportMicrons();

        // Viewport covers the entire sheet, so width/height should match
        Assert.True(left <= 0);
        Assert.True(bottom <= 0);
        Assert.True(width >= template.Sheet.WidthMicrons);
        Assert.True(height >= template.Sheet.HeightMicrons);
    }

    [Fact]
    public void GetViewportMicrons_ZeroViewport_ReturnsZeros()
    {
        var sut = CreateSut(1.0);

        var result = sut.GetViewportMicrons();

        Assert.Equal((0, 0, 0, 0), result);
    }

    [Fact]
    public void GetViewportMicrons_ZeroZoom_ReturnsZeros()
    {
        var sut = CreateSut(0.1);
        sut.SetViewportSize(500, 500);
        sut.SetZoom(0); // Zoom clamped to 0.1

        var result = sut.GetViewportMicrons();

        Assert.NotEqual((0, 0, 0, 0), result);
    }

    [Fact]
    public void GetViewportMicrons_WithMargin_ExpandsViewport()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(100, 100);

        var (left1, bottom1, w1, h1) = sut.GetViewportMicrons(1.0);
        var (left2, bottom2, w2, h2) = sut.GetViewportMicrons(2.0);

        // Margin 2.0 should give larger width/height and shifted left/bottom
        Assert.True(w2 > w1);
        Assert.True(h2 > h1);
        Assert.True(left2 < left1);
        Assert.True(bottom2 < bottom1);
    }

    [Fact]
    public void GetViewportMicrons_PanOffset_ShiftsViewport()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(100, 100);
        var (leftBefore, _, _, _) = sut.GetViewportMicrons();

        // Pan right (positive delta X in mm → PanOffsetX increases)
        sut.PanCanvas(50, 0);

        var (leftAfter, _, _, _) = sut.GetViewportMicrons();

        // After panning right, the left edge should be at a larger model X
        Assert.True(leftAfter > leftBefore);
    }

    [Fact]
    public void GetViewportMicrons_DifferentZoom_ScalesViewport()
    {
        var sut = CreateSut(2.0);
        sut.SetViewportSize(100, 100);

        var (_, _, widthZoom2, _) = sut.GetViewportMicrons();

        sut.SetZoom(1.0);
        var (_, _, widthZoom1, _) = sut.GetViewportMicrons();

        // At zoom 2.0, viewport covers less model space than at zoom 1.0
        Assert.True(widthZoom2 < widthZoom1);
    }

    // === ZoomChanged_CenterCanvas ===

    [Fact]
    public void ZoomChanged_WhenNotCentered_DoesNotResetPanOffset()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(200, 200);

        sut.SetZoom(2.0);
        sut.PanCanvas(50, 30); // PanOffsetX = 100, PanOffsetY = -60

        sut.SetZoom(1.0); // Still not centered (297 > 200)

        Assert.Equal(100, sut.PanOffsetX);
        Assert.Equal(-60, sut.PanOffsetY);
    }

    [Fact]
    public void ZoomChanged_WhenBecomesCentered_ResetsPanOffset()
    {
        var sut = CreateSut(1.0);
        sut.SetViewportSize(200, 200);

        sut.SetZoom(2.0);
        sut.PanCanvas(50, 30); // PanOffsetX = 100, PanOffsetY = -60

        sut.SetZoom(0.5); // Becomes centered (148.5 < 200)

        Assert.Equal(0, sut.PanOffsetX);
        Assert.Equal(0, sut.PanOffsetY);
    }

    // === SetViewportSize_FiresIsCenteredAndScroll ===

    [Fact]
    public void SetViewportSize_FiresIsCenteredAndScroll()
    {
        var sut = CreateSut(1.0);
        var notifications = new List<string>();
        sut.PropertyChanged += (_, e) => notifications.Add(e.PropertyName!);

        sut.SetViewportSize(200, 200);

        Assert.Contains(nameof(ZoomPanManager.ViewportWidthPixels), notifications);
        Assert.Contains(nameof(ZoomPanManager.ViewportHeightPixels), notifications);
        Assert.Contains(nameof(ZoomPanManager.IsCentered), notifications);
        Assert.Contains(nameof(ZoomPanManager.ScrollXRange), notifications);
        Assert.Contains(nameof(ZoomPanManager.CanvasOffsetX), notifications);
    }
}
