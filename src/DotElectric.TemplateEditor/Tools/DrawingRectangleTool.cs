using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.ViewModels;


namespace DotElectric.TemplateEditor.Tools;

/// <summary>
/// Инструмент рисования прямоугольников.
/// Shift — квадрат (равные стороны), Ctrl — от центра.
/// </summary>
public sealed class DrawingRectangleTool : ITool
{
    private readonly IEditorContext _context;
    private PointMicrons? _startPoint;
    private Rectangle? _previewRect;
    private LineType _lineType = LineType.Solid;

    public string Name => "Прямоугольник";

    public DrawingRectangleTool(IEditorContext context)
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
        _previewRect = new Rectangle(
            _startPoint.Value.MicronsX, _startPoint.Value.MicronsY, 0, 0, _lineType,
            strokeColor: EditorSettings.DefaultStrokeColor,
            fillColor: EditorSettings.DefaultFillColor);
        _context.PreviewRectangle = _previewRect;
    }

    public void OnMouseMove(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        if (_startPoint == null || _previewRect == null)
            return;

        var rect = CalculateRectangle(SnapHelper.SnapIfEnabled(modelPoint, _context.GridSettings), modifiers, _startPoint.Value, _lineType);

        _previewRect.MicronsX = rect.MicronsX;
        _previewRect.MicronsY = rect.MicronsY;
        _previewRect.WidthMicrons = rect.WidthMicrons;
        _previewRect.HeightMicrons = rect.HeightMicrons;

        _context.PreviewRectangle = _previewRect;
    }

    public void OnMouseUp(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        if (_startPoint == null || _previewRect == null)
            return;

        var rect = CalculateRectangle(SnapHelper.SnapIfEnabled(modelPoint, _context.GridSettings), modifiers, _startPoint.Value, _lineType);

        if (rect.WidthMicrons > 0 && rect.HeightMicrons > 0)
        {
            var sheetW = _context.Template.Sheet.WidthMicrons;
            var sheetH = _context.Template.Sheet.HeightMicrons;
            var x = Math.Clamp(rect.MicronsX, 0, sheetW);
            var y = Math.Clamp(rect.MicronsY, 0, sheetH);
            var w = Math.Max(PhysicalConstants.MinResizeSizeMicrons, Math.Min(rect.WidthMicrons, sheetW - x));
            var h = Math.Max(PhysicalConstants.MinResizeSizeMicrons, Math.Min(rect.HeightMicrons, sheetH - y));

            // Если minSize вытолкнул правый/верхний край за границу — сдвинуть позицию
            if (x + w > sheetW) x = sheetW - w;
            if (y + h > sheetH) y = sheetH - h;

            rect.MicronsX = x;
            rect.MicronsY = y;
            rect.WidthMicrons = w;
            rect.HeightMicrons = h;

            var cmd = new Commands.AddObjectCommand(_context.Template.Objects, rect);
            _context.CommandHistory.Push(cmd);
        }

        _startPoint = null;
        _previewRect = null;
        _context.PreviewRectangle = null;
    }

    public void OnDoubleClick(PointMicrons modelPoint)
    {
        // Двойной клик — отменить текущий прямоугольник
        _startPoint = null;
        _previewRect = null;
        _context.PreviewRectangle = null;
    }

    public bool OnMouseWheel(int delta, PointMicrons modelPoint) => false;

    public ToolCursor GetCursor()
    {
        return ToolCursor.Cross;
    }

    public void Reset()
    {
        _startPoint = null;
        _previewRect = null;
        _context.PreviewRectangle = null;
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
    /// Рассчитать прямоугольник с учётом модификаторов.
    /// </summary>
    private static Rectangle CalculateRectangle(
        PointMicrons modelPoint,
        ToolModifiers modifiers,
        PointMicrons startPoint,
        LineType lineType)
    {
        long x, y, w, h;

        if ((modifiers & ToolModifiers.Ctrl) != 0 && (modifiers & ToolModifiers.Shift) != 0)
        {
            // Ctrl+Shift: квадрат от центра
            var dx = modelPoint.MicronsX - startPoint.MicronsX;
            var dy = modelPoint.MicronsY - startPoint.MicronsY;
            var size = Math.Max(Math.Abs(dx), Math.Abs(dy)) * 2;
            x = startPoint.MicronsX - size / 2;
            y = startPoint.MicronsY - size / 2;
            w = size;
            h = size;
        }
        else if ((modifiers & ToolModifiers.Shift) != 0)
        {
            // Shift: квадрат — всегда от меньшего угла
            var dx = Math.Abs(modelPoint.MicronsX - startPoint.MicronsX);
            var dy = Math.Abs(modelPoint.MicronsY - startPoint.MicronsY);
            var size = Math.Max(dx, dy);
            x = Math.Min(startPoint.MicronsX, modelPoint.MicronsX);
            y = Math.Min(startPoint.MicronsY, modelPoint.MicronsY);
            w = size;
            h = size;
        }
        else if ((modifiers & ToolModifiers.Ctrl) != 0)
        {
            // Ctrl: от центра
            var dx = modelPoint.MicronsX - startPoint.MicronsX;
            var dy = modelPoint.MicronsY - startPoint.MicronsY;
            x = startPoint.MicronsX - Math.Abs(dx);
            y = startPoint.MicronsY - Math.Abs(dy);
            w = Math.Abs(dx) * 2;
            h = Math.Abs(dy) * 2;
        }
        else
        {
            // Без модификаторов: от угла к углу
            x = Math.Min(startPoint.MicronsX, modelPoint.MicronsX);
            y = Math.Min(startPoint.MicronsY, modelPoint.MicronsY);
            w = Math.Abs(modelPoint.MicronsX - startPoint.MicronsX);
            h = Math.Abs(modelPoint.MicronsY - startPoint.MicronsY);
        }

        return new Rectangle(x, y, w, h, lineType,
            strokeColor: EditorSettings.DefaultStrokeColor,
            fillColor: EditorSettings.DefaultFillColor);
    }
}
