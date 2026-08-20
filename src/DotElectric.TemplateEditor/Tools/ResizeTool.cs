using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Tools;

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

    private ResizeState? _initialState;
    private bool _isResizing;

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

        // Единственный снапшот начальной геометрии — модельный (он же идёт в undo-команду)
        _resizedObject = _context.SingleSelectedObject;
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
            "размер");
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
            return MarkerLayout.VisualCursorForHandle(_activeHandle, text.RotationAngle);
        return MarkerLayout.CursorForHandle(_activeHandle, _isResizing, _context.SingleSelectedObject is Line);
    }

    // === Resize Logic ===

    private void ResizeRectangle(Rectangle rect, double dx, double dy, bool shiftPressed, bool ctrlPressed, bool snapEnabled, long stepMicrons)
    {
        var (newX, newY, newWidth, newHeight) = ResizeMath.ComputeRectangleResize(
            _initialState!.X, _initialState.Y, _initialState.Width, _initialState.Height,
            dx, dy,
            _activeHandle,
            shiftPressed, ctrlPressed,
            snapEnabled, stepMicrons,
            _context.Template.Sheet.WidthMicrons,
            _context.Template.Sheet.HeightMicrons,
            EditorSettings.MinResizeSizeMicrons);

        rect.MicronsX = newX;
        rect.MicronsY = newY;
        rect.WidthMicrons = newWidth;
        rect.HeightMicrons = newHeight;
    }

    private void ResizeText(Text text, double dx, double dy, bool shiftPressed, bool ctrlPressed, bool snapEnabled, long stepMicrons)
    {
        // В ResizeState текста Height — FontSizeMicrons (Width вычисляемая)
        var (newX, newY, newFontSize) = ResizeMath.ComputeTextResize(
            _initialState!.X, _initialState.Y, _initialState.Width, _initialState.Height,
            dx, dy,
            _activeHandle,
            ctrlPressed,
            snapEnabled, stepMicrons,
            _context.Template.Sheet.WidthMicrons,
            _context.Template.Sheet.HeightMicrons,
            EditorSettings.MinFontSizeMicrons,
            text.RotationAngle);

        text.MicronsX = newX;
        text.MicronsY = newY;
        text.FontSizeMicrons = newFontSize;
    }

    private void ResizeLine(Line line, double dx, double dy, bool snapEnabled, long stepMicrons)
    {
        // В ResizeState линии Width/Height — дельты конца относительно начала
        var (newX, newY) = ResizeMath.ComputeLineEndpoint(
            dx, dy,
            _activeHandle,
            _initialState!.X, _initialState.Y,
            _initialState.X + _initialState.Width, _initialState.Y + _initialState.Height,
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
