using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Tools;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Behaviors;

public static class CanvasInputRouter
{
    public static void RouteMouseDown(Canvas canvas, MouseButtonEventArgs e, EditorCanvasState state)
    {
        if (e.ChangedButton == MouseButton.Right)
        {
            var parent = VisualTreeHelper.GetParent(canvas);
            while (parent != null && parent is not UserControl)
                parent = VisualTreeHelper.GetParent(parent);
            if (parent is UserControl userControl && userControl.ContextMenu is { } menu)
            {
                menu.PlacementTarget = canvas;
                menu.IsOpen = true;
            }
            e.Handled = true;
            return;
        }

        if (e.ChangedButton == MouseButton.Middle)
        {
            var window = Window.GetWindow(canvas);
            RoutePanDown(canvas, state, window != null ? e.GetPosition(window) : null, e.GetPosition(canvas), ToolMouseButton.Middle);
            e.Handled = true;
            return;
        }

        if (e.ChangedButton == MouseButton.Left &&
            (Keyboard.IsKeyDown(Key.Space) || Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
        {
            var window = Window.GetWindow(canvas);
            RoutePanDown(canvas, state, window != null ? e.GetPosition(window) : null, e.GetPosition(canvas), ToolMouseButton.Left);
            e.Handled = true;
            return;
        }

        var modelPt = ToModelPoint(canvas, state, e.GetPosition(canvas));
        var tool = GetCurrentTool(state.Editor);

        if (e.ClickCount >= 2 && e.ChangedButton == MouseButton.Left)
        {
            tool.OnDoubleClick(modelPt);
        }
        else
        {
            tool.OnMouseDown(modelPt, ToToolMouseButton(e.ChangedButton), ToToolModifiers(Keyboard.Modifiers));
        }

        state.LastButtonRaw = (int)e.ChangedButton;
        e.Handled = true;
    }

    public static void RouteMouseMove(Canvas canvas, MouseEventArgs e, EditorCanvasState state)
    {
        if (state.IsPanning)
        {
            var window = Window.GetWindow(canvas);
            if (window != null)
                ApplyPan(state, e.GetPosition(window));

            canvas.Cursor = Cursors.SizeAll;
            return;
        }

        var modelPt = ToModelPoint(canvas, state, e.GetPosition(canvas));
        var tool = GetCurrentTool(state.Editor);
        tool.OnMouseMove(modelPoint: modelPt, state.LastButton, ToToolModifiers(Keyboard.Modifiers));

        canvas.Cursor = ToWpfCursor(tool.GetCursor());
    }

    public static void RouteMouseUp(Canvas canvas, MouseButtonEventArgs e, EditorCanvasState state)
    {
        if (e.ChangedButton == MouseButton.Right)
            return;

        if (state.IsPanning && (e.ChangedButton == MouseButton.Middle || e.ChangedButton == MouseButton.Left))
        {
            var panTool = state.Editor.GetOrCreateTool<PanTool>();
            panTool.OnMouseUp(default, ToToolMouseButton(e.ChangedButton), ToToolModifiers(Keyboard.Modifiers));
            state.IsPanning = false;
            state.PanAppliedModelDelta = default;
            canvas.ReleaseMouseCapture();
            state.Editor?.GridManager.RefreshGridNodes();
            e.Handled = true;
            return;
        }

        var tool = GetCurrentTool(state.Editor);
        tool.OnMouseUp(ToModelPoint(canvas, state, e.GetPosition(canvas)), ToToolMouseButton(e.ChangedButton), ToToolModifiers(Keyboard.Modifiers));

        state.LastButtonRaw = -1;
        e.Handled = true;
    }

    public static void RouteMouseWheel(Canvas canvas, MouseWheelEventArgs e, EditorCanvasState state)
    {
        var wpfPoint = e.GetPosition(canvas);
        var modelPoint = ToModelPoint(canvas, state, wpfPoint);

        var tool = GetCurrentTool(state.Editor);
        if (!tool.OnMouseWheel(e.Delta, modelPoint))
        {
            var zoomFactor = e.Delta > 0 ? EditorSettings.MouseWheelZoomFactor : 1.0 / EditorSettings.MouseWheelZoomFactor;
            state.Editor.SetZoom(state.Editor.Zoom * zoomFactor);
        }

        e.Handled = true;
    }

    public static void RoutePreviewKeyDown(Canvas canvas, KeyEventArgs e, EditorCanvasState state)
    {
        if (state.Editor.InlineEditManager.IsEditing)
            return;

        if (FocusManager.GetFocusedElement(canvas) is UIElement focused && focused != canvas)
            return;

        var tool = GetCurrentTool(state.Editor);
        var toolKey = ToToolKey(e.Key);
        var modifiers = ToToolModifiers(Keyboard.Modifiers);

        if (toolKey.HasValue && tool.OnKeyDown(toolKey.Value, modifiers))
        {
            UpdateCursor(canvas, state.Editor);
            e.Handled = true;
        }
    }

    public static void RouteKeyDown(Canvas canvas, KeyEventArgs e, EditorCanvasState state)
    {
        if (state.Editor.InlineEditManager.IsEditing)
            return;

        var tool = GetCurrentTool(state.Editor);
        var toolKey = ToToolKey(e.Key);
        var modifiers = ToToolModifiers(Keyboard.Modifiers);

        if (toolKey.HasValue && tool.OnKeyDown(toolKey.Value, modifiers))
        {
            UpdateCursor(canvas, state.Editor);
            e.Handled = true;
        }
    }

    private static void UpdateCursor(Canvas canvas, EditorViewModel editor)
    {
        var tool = GetCurrentTool(editor);
        canvas.Cursor = ToWpfCursor(tool.GetCursor());
    }

    /// <summary>
    /// Общая логика начала панорамирования (средняя кнопка или Space/Alt+Left).
    /// </summary>
    internal static void RoutePanDown(Canvas canvas, EditorCanvasState state, Point? windowPoint, Point canvasPoint, ToolMouseButton button)
    {
        if (windowPoint.HasValue)
            state.PanStartWpfPoint = windowPoint.Value;
        state.PanAppliedModelDelta = default;

        var modelPoint = ToModelPoint(canvas, state, canvasPoint);
        var panTool = state.Editor.GetOrCreateTool<PanTool>();
        panTool.OnMouseDown(modelPoint, button, ToToolModifiers(Keyboard.Modifiers));
        state.IsPanning = true;
        canvas.CaptureMouse();
    }

    /// <summary>
    /// Инкрементальное применение пана по движению мыши в Window-координатах.
    /// </summary>
    internal static void ApplyPan(EditorCanvasState state, Point currentWpfPoint)
    {
        var deltaPx = currentWpfPoint - state.PanStartWpfPoint;
        var zoom = state.Editor.Zoom;

        var totalModelDelta = new Point(deltaPx.X / zoom, -deltaPx.Y / zoom);
        var incrementalDelta = new Point(
            totalModelDelta.X - state.PanAppliedModelDelta.X,
            totalModelDelta.Y - state.PanAppliedModelDelta.Y);

        state.Editor.PanCanvas(incrementalDelta.X, incrementalDelta.Y);
        state.PanAppliedModelDelta = totalModelDelta;
    }

    internal static Cursor ToWpfCursor(ToolCursor cursor) => cursor switch
    {
        ToolCursor.Hand => Cursors.Hand,
        ToolCursor.Cross => Cursors.Cross,
        ToolCursor.SizeNWSE => Cursors.SizeNWSE,
        ToolCursor.SizeNESW => Cursors.SizeNESW,
        ToolCursor.SizeNS => Cursors.SizeNS,
        ToolCursor.SizeWE => Cursors.SizeWE,
        ToolCursor.IBeam => Cursors.IBeam,
        _ => Cursors.Arrow
    };

    private static PointMicrons ToModelPoint(Canvas canvas, EditorCanvasState state, Point wpfPoint)
    {
        var editor = state.Editor;
        return CoordinateTransform.ToModelPoint(wpfPoint, editor.ZoomPanManager.Zoom, editor.Template.Sheet.HeightMm);
    }

    internal static ITool GetCurrentTool(EditorViewModel editor)
    {
        return editor.ToolRegistry.ActiveToolInstance;
    }

    internal static ToolMouseButton ToToolMouseButton(MouseButton button) => button switch
    {
        MouseButton.Left => ToolMouseButton.Left,
        MouseButton.Right => ToolMouseButton.Right,
        MouseButton.Middle => ToolMouseButton.Middle,
        _ => ToolMouseButton.Left
    };

    internal static ToolModifiers ToToolModifiers(ModifierKeys modifiers)
    {
        var result = ToolModifiers.None;
        if ((modifiers & ModifierKeys.Control) != 0) result |= ToolModifiers.Ctrl;
        if ((modifiers & ModifierKeys.Shift) != 0) result |= ToolModifiers.Shift;
        if ((modifiers & ModifierKeys.Alt) != 0) result |= ToolModifiers.Alt;
        return result;
    }

    internal static ToolKey? ToToolKey(Key key) => key switch
    {
        Key.Escape => ToolKey.Escape,
        Key.Enter => ToolKey.Enter,
        Key.Delete => ToolKey.Delete,
        _ => null
    };
}
