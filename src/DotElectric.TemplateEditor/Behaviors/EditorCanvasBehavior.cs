using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Behaviors;

public static class EditorCanvasBehavior
{
    public static readonly DependencyProperty StateProperty =
        DependencyProperty.RegisterAttached(
            "State", typeof(EditorCanvasState),
            typeof(EditorCanvasBehavior), new PropertyMetadata(null));

    public static EditorCanvasState GetState(DependencyObject obj) =>
        (EditorCanvasState)obj.GetValue(StateProperty);

    public static void SetState(DependencyObject obj, EditorCanvasState value) =>
        obj.SetValue(StateProperty, value);

    public static readonly DependencyProperty EditorProperty =
        DependencyProperty.RegisterAttached(
            "Editor",
            typeof(EditorViewModel),
            typeof(EditorCanvasBehavior),
            new PropertyMetadata(null, OnEditorChanged));

    public static EditorViewModel GetEditor(DependencyObject obj) =>
        (EditorViewModel)obj.GetValue(EditorProperty);

    public static void SetEditor(DependencyObject obj, EditorViewModel value) =>
        obj.SetValue(EditorProperty, value);

    private static void OnEditorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Canvas canvas) return;

        if (GetState(canvas) is { } oldState)
        {
            canvas.MouseDown -= State_MouseDown;
            canvas.MouseMove -= State_MouseMove;
            canvas.MouseUp -= State_MouseUp;
            canvas.PreviewMouseWheel -= State_MouseWheel;
            SetState(canvas, null!);
        }

        if (e.NewValue is EditorViewModel editor)
        {
            var state = new EditorCanvasState { Editor = editor };
            SetState(canvas, state);

            canvas.MouseDown += State_MouseDown;
            canvas.MouseMove += State_MouseMove;
            canvas.MouseUp += State_MouseUp;
            canvas.PreviewMouseWheel += State_MouseWheel;
            canvas.PreviewKeyDown += State_PreviewKeyDown;
            canvas.KeyDown += State_KeyDown;

            canvas.Unloaded += Canvas_Unloaded;
            canvas.PreviewMouseLeftButtonDown += (s, args) => canvas.Focus();
            canvas.Dispatcher.BeginInvoke(() => canvas.Focus(), System.Windows.Threading.DispatcherPriority.Loaded);
        }
    }

    private static void Canvas_Unloaded(object sender, RoutedEventArgs e)
    {
        if (sender is not Canvas canvas) return;

        canvas.Unloaded -= Canvas_Unloaded;
        canvas.MouseDown -= State_MouseDown;
        canvas.MouseMove -= State_MouseMove;
        canvas.MouseUp -= State_MouseUp;
        canvas.PreviewMouseWheel -= State_MouseWheel;
        canvas.PreviewKeyDown -= State_PreviewKeyDown;
        canvas.KeyDown -= State_KeyDown;

        SetState(canvas, null!);
    }

    private static void State_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Canvas canvas) return;
        if (GetState(canvas) is not { } state) return;
        CanvasInputRouter.RouteMouseDown(canvas, e, state);
    }

    private static void State_MouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not Canvas canvas) return;
        if (GetState(canvas) is not { } state) return;
        CanvasInputRouter.RouteMouseMove(canvas, e, state);
    }

    private static void State_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Canvas canvas) return;
        if (GetState(canvas) is not { } state) return;
        CanvasInputRouter.RouteMouseUp(canvas, e, state);
    }

    private static void State_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not Canvas canvas) return;
        if (GetState(canvas) is not { } state) return;
        CanvasInputRouter.RouteMouseWheel(canvas, e, state);
    }

    private static void State_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not Canvas canvas) return;
        if (GetState(canvas) is not { } state) return;
        CanvasInputRouter.RoutePreviewKeyDown(canvas, e, state);
    }

    private static void State_KeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not Canvas canvas) return;
        if (GetState(canvas) is not { } state) return;
        CanvasInputRouter.RouteKeyDown(canvas, e, state);
    }
}
