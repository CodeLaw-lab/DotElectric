using CommunityToolkit.Mvvm.ComponentModel;
using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.ViewModels.Managers;

public sealed partial class ZoomPanManager : ObservableObject
{
    private readonly Template _template;
    private readonly Action _onZoomChanged;
    private Action _onGridRefresh;
    private double _viewportWidthPx;
    private double _viewportHeightPx;

    public ZoomPanManager(
        Template template,
        Action onZoomChanged,
        Action onGridRefresh)
    {
        _template = template;
        _onZoomChanged = onZoomChanged;
        _onGridRefresh = onGridRefresh;
    }

    public void SetGridRefreshCallback(Action callback) => _onGridRefresh = callback;

    [ObservableProperty]
    private double _panOffsetX;

    [ObservableProperty]
    private double _panOffsetY;

    [ObservableProperty]
    private double _zoom = 1.0;

    public void SetViewportSize(double widthPx, double heightPx)
    {
        _viewportWidthPx = widthPx;
        _viewportHeightPx = heightPx;
        OnPropertyChanged(nameof(ViewportWidthPixels));
        OnPropertyChanged(nameof(ViewportHeightPixels));
        OnPropertyChanged(nameof(IsCentered));
        OnPropertyChanged(nameof(ScrollXRange));
        OnPropertyChanged(nameof(ScrollYRange));
        OnPropertyChanged(nameof(ScrollXValue));
        OnPropertyChanged(nameof(ScrollYValue));
        OnPropertyChanged(nameof(CanvasOffsetX));
        OnPropertyChanged(nameof(CanvasOffsetY));
        if (widthPx > 0 && heightPx > 0)
            _onGridRefresh();
    }

    public bool IsCentered => _viewportWidthPx > 0 && _viewportHeightPx > 0
        && CanvasWidthPixels <= _viewportWidthPx
        && CanvasHeightPixels <= _viewportHeightPx;

    public double CanvasWidthPixels => Coordinate.ToMm(_template.Sheet.WidthMicrons) * Zoom;

    public double CanvasHeightPixels => Coordinate.ToMm(_template.Sheet.HeightMicrons) * Zoom;

    public double ViewportWidthPixels => _viewportWidthPx;

    public double ViewportHeightPixels => _viewportHeightPx;

    public double ScrollXRange => IsCentered ? 0 : Math.Max(0, CanvasWidthPixels - ViewportWidthPixels);

    public double ScrollYRange => IsCentered ? 0 : Math.Max(0, CanvasHeightPixels - ViewportHeightPixels);

    public double ScrollXValue => IsCentered ? 0 : Math.Max(0, PanOffsetX + (CanvasWidthPixels - ViewportWidthPixels) / 2);

    public double ScrollYValue => IsCentered ? 0 : Math.Max(0, PanOffsetY + (CanvasHeightPixels - ViewportHeightPixels) / 2);

    public void SetScrollX(double value)
    {
        if (!IsCentered)
            PanOffsetX = Math.Max(0, Math.Min(value, ScrollXRange)) - (CanvasWidthPixels - ViewportWidthPixels) / 2;
    }

    public void SetScrollY(double value)
    {
        if (!IsCentered)
            PanOffsetY = Math.Max(0, Math.Min(value, ScrollYRange)) - (CanvasHeightPixels - ViewportHeightPixels) / 2;
    }

    public double CanvasOffsetX => -PanOffsetX;

    public double CanvasOffsetY => -PanOffsetY;

    public void CenterCanvas()
    {
        if (IsCentered)
        {
            PanOffsetX = 0;
            PanOffsetY = 0;
        }
        else
        {
            PanOffsetX = -(CanvasWidthPixels - ViewportWidthPixels) / 2;
            PanOffsetY = -(CanvasHeightPixels - ViewportHeightPixels) / 2;
        }
    }

    public int ZoomPercent
    {
        get => (int)Math.Round(Zoom * 100);
        set => SetZoom(Math.Clamp(value / 100.0, EditorSettings.ZoomMin, EditorSettings.ZoomMax));
    }

    partial void OnZoomChanged(double value)
    {
        OnPropertyChanged(nameof(ZoomPercent));
        OnPropertyChanged(nameof(CanvasWidthPixels));
        OnPropertyChanged(nameof(CanvasHeightPixels));
        OnPropertyChanged(nameof(IsCentered));
        OnPropertyChanged(nameof(ScrollXRange));
        OnPropertyChanged(nameof(ScrollYRange));
        OnPropertyChanged(nameof(ScrollXValue));
        OnPropertyChanged(nameof(ScrollYValue));
        OnPropertyChanged(nameof(CanvasOffsetX));
        OnPropertyChanged(nameof(CanvasOffsetY));
        if (IsCentered)
            CenterCanvas();
        _onZoomChanged();
        _onGridRefresh();
    }

    partial void OnPanOffsetXChanged(double value)
    {
        OnPropertyChanged(nameof(ScrollXValue));
        OnPropertyChanged(nameof(CanvasOffsetX));
    }

    partial void OnPanOffsetYChanged(double value)
    {
        OnPropertyChanged(nameof(ScrollYValue));
        OnPropertyChanged(nameof(CanvasOffsetY));
    }

    public void SetZoom(double newZoom)
    {
        Zoom = Math.Clamp(newZoom, EditorSettings.ZoomMin, EditorSettings.ZoomMax);
    }

    public void SetZoomPercent(int percent)
    {
        SetZoom(Math.Clamp(percent / 100.0, EditorSettings.ZoomMin, EditorSettings.ZoomMax));
    }

    public void ZoomIn()
    {
        Zoom = Math.Min(Zoom + EditorSettings.ZoomIncrement, EditorSettings.ZoomMax);
    }

    public void ZoomOut()
    {
        Zoom = Math.Max(Zoom - EditorSettings.ZoomIncrement, EditorSettings.ZoomMin);
    }

    /// <summary>
    /// Returns the visible viewport area in model microns, with optional margin.
    /// </summary>
    /// <param name="margin">Expand viewport by this factor (1.0 = exact viewport, 2.0 = one screen margin on each side).</param>
    /// <returns>(LeftMicrons, BottomMicrons, WidthMicrons, HeightMicrons) in model coordinates (Y-up).</returns>
    public (long LeftMicrons, long BottomMicrons, long WidthMicrons, long HeightMicrons) GetViewportMicrons(double margin = 1.0)
    {
        if (_viewportWidthPx <= 0 || _viewportHeightPx <= 0 || Zoom <= 0)
            return (0, 0, 0, 0);

        // Expanded viewport in canvas-local WPF pixels (Y-down)
        var expLeftPx = PanOffsetX - _viewportWidthPx * (margin - 1) / 2;
        var expTopPx = PanOffsetY - _viewportHeightPx * (margin - 1) / 2;
        var expWidthPx = _viewportWidthPx * margin;
        var expHeightPx = _viewportHeightPx * margin;

        var sheetHeightMm = Coordinate.ToMm(_template.Sheet.HeightMicrons);
        var zoomInv = 1.0 / Zoom;

        // Convert canvas-local (WPF Y-down) to model microns (cartesian Y-up)
        var leftMm = expLeftPx * zoomInv;
        var bottomMm = sheetHeightMm - (expTopPx + expHeightPx) * zoomInv;
        var widthMm = expWidthPx * zoomInv;
        var heightMm = expHeightPx * zoomInv;

        const long micronsPerMm = Coordinate.MicronsPerMm;

        return (
            (long)(leftMm * micronsPerMm),
            (long)(bottomMm * micronsPerMm),
            (long)(widthMm * micronsPerMm),
            (long)(heightMm * micronsPerMm)
        );
    }

    public void PanCanvas(double deltaXMm, double deltaYMm)
    {
        PanOffsetX += deltaXMm * Zoom;
        PanOffsetY -= deltaYMm * Zoom;
    }

    public void FitToScreen(double canvasWidth, double canvasHeight)
    {
        var sheetWidthMm = _template.Sheet.WidthMm;
        var sheetHeightMm = _template.Sheet.HeightMm;

        var zoomW = (canvasWidth * EditorSettings.FitToScreenPadding) / sheetWidthMm;
        var zoomH = (canvasHeight * EditorSettings.FitToScreenPadding) / sheetHeightMm;
        Zoom = Math.Min(Math.Min(zoomW, zoomH), EditorSettings.ZoomMax);

        CenterCanvas();
    }
}
