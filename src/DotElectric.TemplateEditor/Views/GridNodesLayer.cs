using System.Windows;
using System.Windows.Media;
using DotElectric.TemplateEditor.Helpers;

namespace DotElectric.TemplateEditor.Views;

/// <summary>
/// Слой отрисовки узлов сетки через DrawingContext.
/// Узлы хранятся в абсолютных координатах листа (микроны) и НЕ регенерируются при пане —
/// RenderTransform (TranslateTransform) двигает их синхронно с DrawingCanvas.
/// </summary>
public sealed class GridNodesLayer : FrameworkElement
{
    public static readonly DependencyProperty ZoomProperty =
        DependencyProperty.Register(nameof(Zoom), typeof(double), typeof(GridNodesLayer),
            new PropertyMetadata(1.0, OnRenderPropertyChanged));

    public static readonly DependencyProperty SheetHeightMmProperty =
        DependencyProperty.Register(nameof(SheetHeightMm), typeof(double), typeof(GridNodesLayer),
            new PropertyMetadata(0.0, OnRenderPropertyChanged));

    public static readonly DependencyProperty NodeColorProperty =
        DependencyProperty.Register(nameof(NodeColor), typeof(Brush), typeof(GridNodesLayer),
            new PropertyMetadata(null, OnRenderPropertyChanged));

    public static readonly DependencyProperty NodeSizeProperty =
        DependencyProperty.Register(nameof(NodeSize), typeof(double), typeof(GridNodesLayer),
            new PropertyMetadata(2.0, OnRenderPropertyChanged));

    public static readonly DependencyProperty IsDarkThemeProperty =
        DependencyProperty.Register(nameof(IsDarkTheme), typeof(bool), typeof(GridNodesLayer),
            new PropertyMetadata(false, OnIsDarkThemeChanged));

    public double Zoom { get => (double)GetValue(ZoomProperty); set => SetValue(ZoomProperty, value); }
    public double SheetHeightMm { get => (double)GetValue(SheetHeightMmProperty); set => SetValue(SheetHeightMmProperty, value); }

    /// <summary>
    /// Цвет узлов. null = авто (тема): Light → #C0C0C0, Dark → #808080.
    /// </summary>
    public Brush? NodeColor { get => (Brush?)GetValue(NodeColorProperty); set => SetValue(NodeColorProperty, value); }

    /// <summary>
    /// Диаметр узла в пикселях.
    /// </summary>
    public double NodeSize { get => (double)GetValue(NodeSizeProperty); set => SetValue(NodeSizeProperty, value); }

    /// <summary>
    /// Тёмная тема активна — переключает темо-зависимый цвет узлов по умолчанию.
    /// </summary>
    public bool IsDarkTheme { get => (bool)GetValue(IsDarkThemeProperty); set => SetValue(IsDarkThemeProperty, value); }

    private IReadOnlyList<GridNode> _nodes = [];
    private Brush _themeBrush;

    public GridNodesLayer()
    {
        _themeBrush = CreateThemeBrush(isDark: false);
        IsHitTestVisible = false;
    }

    private static Brush CreateThemeBrush(bool isDark)
    {
        var brush = new SolidColorBrush(isDark ? Color.FromRgb(128, 128, 128) : Color.FromRgb(192, 192, 192));
        brush.Freeze();
        return brush;
    }

    /// <summary>
    /// Обновляет темо-зависимый цвет по умолчанию (когда NodeColor == null).
    /// </summary>
    public void UpdateThemeBrush(bool isDark)
    {
        _themeBrush = CreateThemeBrush(isDark);
        InvalidateVisual();
    }

    private static void OnRenderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridNodesLayer layer)
            layer.InvalidateVisual();
    }

    private static void OnIsDarkThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridNodesLayer layer)
            layer.UpdateThemeBrush((bool)e.NewValue);
    }

    /// <summary>
    /// Устанавливает узлы сетки (абсолютные координаты листа в микронах).
    /// </summary>
    public void SetNodes(IReadOnlyList<GridNode>? nodes)
    {
        _nodes = nodes ?? [];
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        if (_nodes.Count <= 0)
            return;

        var zoom = Zoom;
        if (zoom <= 0)
            return;

        var brush = NodeColor ?? _themeBrush;
        var radius = Math.Max(0.5, NodeSize / 2.0);
        var invMicronsPerMm = 1.0 / Coordinate.MicronsPerMm;
        var heightMm = SheetHeightMm;

        foreach (var node in _nodes)
        {
            var xPx = node.XMicrons * invMicronsPerMm * zoom;
            var yPx = RenderRules.ModelYToTop(node.YMicrons, heightMm, zoom);
            dc.DrawEllipse(brush, null, new Point(xPx, yPx), radius, radius);
        }
    }
}
