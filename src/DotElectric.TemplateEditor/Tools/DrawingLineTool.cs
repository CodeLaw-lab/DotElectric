using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Tools;

/// <summary>
/// Инструмент рисования линий.
/// Shift — ограничение горизонталью/вертикалью.
/// </summary>
public sealed class DrawingLineTool : ITool
{
    private readonly IEditorContext _context;
    private PointMicrons? _startPoint;
    private Line? _previewLine;
    private LineType _lineType = LineType.Solid;

    public string Name => "Линия";

    public DrawingLineTool(IEditorContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public void OnMouseDown(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        if (button != ToolMouseButton.Left)
            return;

        if (_startPoint != null)
            return;

        _startPoint = SnapHelper.SnapIfEnabled(modelPoint, _context.GridSettings);
        _previewLine = new Line(
            _startPoint.Value.MicronsX, _startPoint.Value.MicronsY,
            _startPoint.Value.MicronsX, _startPoint.Value.MicronsY,
            _lineType, strokeColor: EditorSettings.DefaultStrokeColor);
        _context.PreviewLine = _previewLine;
    }

    public void OnMouseMove(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        if (_startPoint == null || _previewLine == null)
            return;

        var end = ApplyConstraint(SnapHelper.SnapIfEnabled(modelPoint, _context.GridSettings), modifiers, _startPoint.Value);

        _previewLine.StartMicronsX = _startPoint.Value.MicronsX;
        _previewLine.StartMicronsY = _startPoint.Value.MicronsY;
        _previewLine.EndMicronsX = end.MicronsX;
        _previewLine.EndMicronsY = end.MicronsY;

        _context.PreviewLine = _previewLine;
    }

    public void OnMouseUp(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        if (_startPoint == null || _previewLine == null)
            return;

        var end = ApplyConstraint(SnapHelper.SnapIfEnabled(modelPoint, _context.GridSettings), modifiers, _startPoint.Value);

        if (end.MicronsX != _startPoint.Value.MicronsX ||
            end.MicronsY != _startPoint.Value.MicronsY)
        {
            var startX = _context.ClampX(_startPoint.Value.MicronsX);
            var startY = _context.ClampY(_startPoint.Value.MicronsY);
            var endX = _context.ClampX(end.MicronsX);
            var endY = _context.ClampY(end.MicronsY);

            var line = new Line(startX, startY, endX, endY, _lineType, strokeColor: EditorSettings.DefaultStrokeColor);

            var cmd = new Commands.AddObjectCommand(_context.Template.Objects, line);
            _context.CommandHistory.Push(cmd);
        }

        _startPoint = null;
        _previewLine = null;
        _context.PreviewLine = null;
    }

    public void OnDoubleClick(PointMicrons modelPoint)
    {
        // Двойной клик — отменить текущую линию и переключиться на Select
        _startPoint = null;
        _previewLine = null;
        _context.PreviewLine = null;
        _context.SetActiveToolCommand.Execute("Select");
    }

    public bool OnMouseWheel(int delta, PointMicrons modelPoint) => false;

    public ToolCursor GetCursor()
    {
        return ToolCursor.Cross;
    }

    public void Reset()
    {
        _startPoint = null;
        _previewLine = null;
        _context.PreviewLine = null;
    }

    public bool OnKeyDown(ToolKey key, ToolModifiers modifiers)
    {
        if (key == ToolKey.Escape)
        {
            Reset();
            _context.SetActiveToolCommand.Execute("Select");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Применить ограничение (Shift = горизонталь/вертикаль).
    /// </summary>
    private static PointMicrons ApplyConstraint(
        PointMicrons modelPoint,
        ToolModifiers modifiers,
        PointMicrons startPoint)
    {
        if ((modifiers & ToolModifiers.Shift) == 0)
            return modelPoint;

        var dx = Math.Abs(modelPoint.MicronsX - startPoint.MicronsX);
        var dy = Math.Abs(modelPoint.MicronsY - startPoint.MicronsY);

        const long diagonalTolerance = 5000; // микрон (5 мм)
        var isHorizontal = dx > dy + diagonalTolerance;
        var isVertical = dy > dx + diagonalTolerance;

        if (isHorizontal)
            return new PointMicrons(modelPoint.MicronsX, startPoint.MicronsY); // горизонталь
        if (isVertical)
            return new PointMicrons(startPoint.MicronsX, modelPoint.MicronsY); // вертикаль

        // 45° диагональ: dx ≈ dy
        var diagonalLength = Math.Max(dx, dy);
        var signX = modelPoint.MicronsX > startPoint.MicronsX ? 1 : -1;
        var signY = modelPoint.MicronsY > startPoint.MicronsY ? 1 : -1;
        return new PointMicrons(
            startPoint.MicronsX + signX * diagonalLength,
            startPoint.MicronsY + signY * diagonalLength);
    }
}
