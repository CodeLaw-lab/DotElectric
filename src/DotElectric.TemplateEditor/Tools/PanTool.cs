using System.Windows;
using System.Windows.Controls;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Tools;

/// <summary>
/// Инструмент панорамирования (перетаскивания холста).
/// Активируется средней кнопкой мыши или Space+LeftDrag.
/// Использует ScrollViewer для перемещения видимой области.
/// </summary>
public sealed class PanTool : ITool
{
    private readonly IEditorContext _context;
    private Point _lastMousePosition;
    private bool _isPanning;

    public string Name => "Панорамирование";

    public PanTool(IEditorContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public void OnMouseDown(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        // Средняя кнопка или Left с зажатым Alt
        if (button != ToolMouseButton.Middle &&
            (button != ToolMouseButton.Left || (modifiers & ToolModifiers.Alt) == 0))
            return;

        _isPanning = true;
        _lastMousePosition = new Point(Coordinate.ToMm(modelPoint.MicronsX), Coordinate.ToMm(modelPoint.MicronsY));
    }

    public void OnMouseMove(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        if (!_isPanning)
            return;

        var currentPosition = new Point(Coordinate.ToMm(modelPoint.MicronsX), Coordinate.ToMm(modelPoint.MicronsY));
        var delta = currentPosition - _lastMousePosition;

        // Панорамирование через ScrollViewer
        // Поскольку у нас нет прямого доступа к ScrollViewer из ViewModel,
        // мы используем Attached Behavior через EditorCanvasBehavior
        _context.PanCanvas(delta.X, delta.Y);

        _lastMousePosition = currentPosition;
    }

    public void OnMouseUp(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        _isPanning = false;
    }

    public void OnDoubleClick(PointMicrons modelPoint)
    {
        // Двойной клик не делает ничего при панорамировании
    }

    public bool OnMouseWheel(int delta, PointMicrons modelPoint) => false;

    public ToolCursor GetCursor()
    {
        return ToolCursor.Hand;
    }

    public void Reset()
    {
        _isPanning = false;
    }

    public bool OnKeyDown(ToolKey key, ToolModifiers modifiers) => false;
}
