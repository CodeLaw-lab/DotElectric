using System.Windows;
using System.Windows.Media;

namespace DotElectric.TemplateEditor.Views;

/// <summary>
/// Слой отрисовки узлов сетки через DrawingContext.
/// Хранит координаты в микронах (model space), преобразует в пиксели в OnRender.
/// </summary>
public class GridNodesLayer : FrameworkElement
{
    private readonly Brush _nodeBrush;
    private long[] _nodeData = [];
    private int _nodeCount;

    public static readonly DependencyProperty ZoomProperty =
        DependencyProperty.Register(
            nameof(Zoom), typeof(double), typeof(GridNodesLayer),
            new PropertyMetadata(1.0, OnTransformChanged));

    public static readonly DependencyProperty SheetHeightMmProperty =
        DependencyProperty.Register(
            nameof(SheetHeightMm), typeof(double), typeof(GridNodesLayer),
            new PropertyMetadata(0.0, OnTransformChanged));

    public double Zoom
    {
        get => (double)GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    public double SheetHeightMm
    {
        get => (double)GetValue(SheetHeightMmProperty);
        set => SetValue(SheetHeightMmProperty, value);
    }

    private static void OnTransformChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridNodesLayer layer)
            layer.InvalidateVisual();
    }

    public GridNodesLayer()
    {
        var brush = new SolidColorBrush(Color.FromRgb(192, 192, 192));
        brush.Freeze();
        _nodeBrush = brush;

        IsHitTestVisible = false;
    }

    /// <summary>
    /// Updates node data from a flat long[] buffer with alternating X,Y micron coordinates.
    /// The caller must not modify the array after passing it here.
    /// </summary>
    public void SetNodes(long[] data, int count)
    {
        _nodeData = data;
        _nodeCount = count;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        if (_nodeCount <= 0)
            return;

        var zoom = Zoom;
        if (zoom <= 0)
            return;

        var invMicronsPerMm = 1.0 / 1000.0;
        var heightMm = SheetHeightMm;

        for (int i = 0; i < _nodeCount; i++)
        {
            var xPx = _nodeData[i * 2] * invMicronsPerMm * zoom;
            var yPx = (heightMm - _nodeData[i * 2 + 1] * invMicronsPerMm) * zoom;
            dc.DrawEllipse(_nodeBrush, null, new Point(xPx, yPx), 1.0, 1.0);
        }
    }
}
