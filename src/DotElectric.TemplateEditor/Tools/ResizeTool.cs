using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Tools;

/// <summary>
/// Маркер изменения размера.
/// </summary>
public enum ResizeHandle
{
    TopLeft,
    Top,
    TopRight,
    Right,
    BottomRight,
    Bottom,
    BottomLeft,
    Left
}

/// <summary>
/// Инструмент изменения размера объектов.
/// Активируется при перетаскивании маркера выделения.
/// Shift = сохранение пропорций, Ctrl = от центра, мин. размер 1мм.
/// </summary>
public sealed class ResizeTool : ITool
{
    private readonly IEditorContext _context;
    private TemplateObjectBase? _resizedObject;
    private ResizeHandle _activeHandle;
    private PointMicrons _startPoint;

    // Сохранённые начальные параметры объекта
    private long _startX;
    private long _startY;
    private long _startWidth;
    private long _startHeight;

    // Явные поля для Line (начальные и конечные точки)
    private long _lineStartX;
    private long _lineStartY;
    private long _lineEndX;
    private long _lineEndY;

    private ResizeState? _initialState;
    private bool _isResizing;

    public string Name => "Изменение размера";

    public ResizeTool(IEditorContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public void OnMouseDown(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        if (button != ToolMouseButton.Left) return;

        // Определяем какой маркер был нажат через EditorViewModel
        if (_context.ActiveResizeHandle == null) return;

        _activeHandle = _context.ActiveResizeHandle.Value;
        _startPoint = modelPoint;
        _isResizing = true;

        // Сохраняем начальные параметры
        if (_context.SingleSelectedObject is Rectangle rect)
        {
            _resizedObject = rect;
            _startX = rect.MicronsX;
            _startY = rect.MicronsY;
            _startWidth = rect.WidthMicrons;
            _startHeight = rect.HeightMicrons;
        }
        else if (_context.SingleSelectedObject is Text text)
        {
            // Для текста меняем только позицию (перемещение базовой линии)
            _resizedObject = text;
            _startX = text.MicronsX;
            _startY = text.MicronsY;
            _startWidth = text.WidthMicrons;
            _startHeight = text.FontSizeMicrons;
        }
        else if (_context.SingleSelectedObject is Line line)
        {
            // Для линии — меняем конечную точку
            _resizedObject = line;
            _lineStartX = line.StartMicronsX;
            _lineStartY = line.StartMicronsY;
            _lineEndX = line.EndMicronsX;
            _lineEndY = line.EndMicronsY;
        }

        _initialState = _resizedObject?.CaptureResizeState();
    }

    public void OnMouseMove(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        if (!_isResizing || _resizedObject == null) return;

        var dx = modelPoint.MicronsX - _startPoint.MicronsX;
        var dy = modelPoint.MicronsY - _startPoint.MicronsY;

        bool shiftPressed = (modifiers & ToolModifiers.Shift) != 0;
        bool ctrlPressed = (modifiers & ToolModifiers.Ctrl) != 0;
        bool snapEnabled = _context.GridSettings.SnapEnabled;
        long stepMicrons = _context.GridSettings.StepMicrons;

        if (_resizedObject is Rectangle rect)
        {
            ResizeRectangle(rect, dx, dy, shiftPressed, ctrlPressed, snapEnabled, stepMicrons);
        }
        else if (_resizedObject is Text text)
        {
            ResizeText(text, dx, dy, shiftPressed, ctrlPressed, snapEnabled, stepMicrons);
        }
        else if (_resizedObject is Line line)
        {
            ResizeLine(line, dx, dy, snapEnabled, stepMicrons);
        }
    }

    public void OnMouseUp(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        if (!_isResizing || _resizedObject == null) return;

        _isResizing = false;

        var finalState = _resizedObject.CaptureResizeState();
        var captured = _resizedObject;

        var cmd = new ChangePropertyCommand<ResizeState>(
            _initialState!,
            s => captured.ApplyResize(s),
            finalState,
            "размер",
            _context.MarkDirty);
        _context.CommandHistory.Push(cmd);

        _context.ActiveResizeHandle = null;
        _resizedObject = null;

        _context.PopTool();
    }

    public void OnDoubleClick(PointMicrons modelPoint)
    {
        // Не используется
    }

    public bool OnMouseWheel(int delta, PointMicrons modelPoint) => false;

    public ToolCursor GetCursor()
    {
        if (_context.SingleSelectedObject is Text text)
            return ResizeMath.VisualCursorForHandle(_activeHandle, text.RotationAngle);
        return ResizeMath.CursorForHandle(_activeHandle, _isResizing, _context.SingleSelectedObject is Line);
    }

    // === Resize Logic ===

    private void ResizeRectangle(Rectangle rect, double dx, double dy, bool shiftPressed, bool ctrlPressed, bool snapEnabled, long stepMicrons)
    {
        var (newX, newY, newWidth, newHeight) = ResizeMath.ComputeRectangleResize(
            _startX, _startY, _startWidth, _startHeight,
            dx, dy,
            _activeHandle,
            shiftPressed, ctrlPressed,
            snapEnabled, stepMicrons,
            _context.Template.Sheet.WidthMicrons,
            _context.Template.Sheet.HeightMicrons,
            PhysicalConstants.MinResizeSizeMicrons);

        rect.MicronsX = newX;
        rect.MicronsY = newY;
        rect.WidthMicrons = newWidth;
        rect.HeightMicrons = newHeight;
    }

    private void ResizeText(Text text, double dx, double dy, bool shiftPressed, bool ctrlPressed, bool snapEnabled, long stepMicrons)
    {
        var (newX, newY, newFontSize) = ResizeMath.ComputeTextResize(
            _startX, _startY, _startWidth, _startHeight,
            dx, dy,
            _activeHandle,
            ctrlPressed,
            snapEnabled, stepMicrons,
            _context.Template.Sheet.WidthMicrons,
            _context.Template.Sheet.HeightMicrons,
            PhysicalConstants.MinFontSizeMicrons,
            text.RotationAngle);

        text.MicronsX = newX;
        text.MicronsY = newY;
        text.FontSizeMicrons = newFontSize;
    }

    private void ResizeLine(Line line, double dx, double dy, bool snapEnabled, long stepMicrons)
    {
        var (newX, newY) = ResizeMath.ComputeLineEndpoint(
            dx, dy,
            _activeHandle,
            _lineStartX, _lineStartY,
            _lineEndX, _lineEndY,
            snapEnabled, stepMicrons,
            _context.Template.Sheet.WidthMicrons,
            _context.Template.Sheet.HeightMicrons);

        if (_activeHandle == ResizeHandle.BottomRight)
        {
            line.EndMicronsX = newX;
            line.EndMicronsY = newY;
        }
        else if (_activeHandle == ResizeHandle.TopLeft)
        {
            line.StartMicronsX = newX;
            line.StartMicronsY = newY;
        }
    }

    public void Reset()
    {
        _isResizing = false;
        _resizedObject = null;
        _initialState = null;
        _context.ActiveResizeHandle = null;
    }

    public bool OnKeyDown(ToolKey key, ToolModifiers modifiers)
    {
        if (key == ToolKey.Escape)
        {
            _context.PopTool();
            Reset();
            return true;
        }
        return false;
    }
}
