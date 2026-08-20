using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DotElectric.TemplateEditor.Behaviors;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tests.Helpers;
using DotElectric.TemplateEditor.Tools;
using DotElectric.TemplateEditor.ViewModels;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Behaviors;

/// <summary>
/// STA-тесты attached properties EditorCanvasBehavior.State/Editor и жизненного цикла
/// подписок OnEditorChanged (подписка/отписка обработчиков, Unloaded).
/// Паттерн: WpfContext.Execute + real EditorViewModel (как в CanvasInputRouterTests).
/// </summary>
public class EditorCanvasBehaviorTests
{
    private static EditorViewModel CreateEditor()
    {
        var template = new Template();
        return new EditorViewModel(template, printService: new Mock<IPrintService>().Object);
    }

    // ===== ToToolMouseButton (pure) =====

    [Theory]
    [InlineData(MouseButton.Left, ToolMouseButton.Left)]
    [InlineData(MouseButton.Right, ToolMouseButton.Right)]
    [InlineData(MouseButton.Middle, ToolMouseButton.Middle)]
    [InlineData(MouseButton.XButton1, ToolMouseButton.Left)]
    [InlineData(MouseButton.XButton2, ToolMouseButton.Left)]
    public void ToToolMouseButton_MapsCorrectly(MouseButton input, ToolMouseButton expected)
    {
        var result = CanvasInputRouter.ToToolMouseButton(input);
        Assert.Equal(expected, result);
    }

    // ===== ToToolModifiers (pure) =====

    [Fact]
    public void ToToolModifiers_None_ReturnsNone()
    {
        var result = CanvasInputRouter.ToToolModifiers(ModifierKeys.None);
        Assert.Equal(ToolModifiers.None, result);
    }

    [Fact]
    public void ToToolModifiers_Ctrl_ReturnsCtrl()
    {
        var result = CanvasInputRouter.ToToolModifiers(ModifierKeys.Control);
        Assert.Equal(ToolModifiers.Ctrl, result);
    }

    [Fact]
    public void ToToolModifiers_Shift_ReturnsShift()
    {
        var result = CanvasInputRouter.ToToolModifiers(ModifierKeys.Shift);
        Assert.Equal(ToolModifiers.Shift, result);
    }

    [Fact]
    public void ToToolModifiers_Alt_ReturnsAlt()
    {
        var result = CanvasInputRouter.ToToolModifiers(ModifierKeys.Alt);
        Assert.Equal(ToolModifiers.Alt, result);
    }

    [Fact]
    public void ToToolModifiers_CtrlShift_ReturnsCtrlShift()
    {
        var result = CanvasInputRouter.ToToolModifiers(ModifierKeys.Control | ModifierKeys.Shift);
        Assert.Equal(ToolModifiers.Ctrl | ToolModifiers.Shift, result);
    }

    [Fact]
    public void ToToolModifiers_All_ReturnsAll()
    {
        var result = CanvasInputRouter.ToToolModifiers(ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt);
        Assert.Equal(ToolModifiers.Ctrl | ToolModifiers.Shift | ToolModifiers.Alt, result);
    }

    [Fact]
    public void ToToolModifiers_Windows_Ignored()
    {
        var result = CanvasInputRouter.ToToolModifiers(ModifierKeys.Windows);
        Assert.Equal(ToolModifiers.None, result);
    }

    // ===== ToToolKey (pure) =====

    [Theory]
    [InlineData(Key.Escape, ToolKey.Escape)]
    [InlineData(Key.Enter, ToolKey.Enter)]
    [InlineData(Key.Delete, ToolKey.Delete)]
    public void ToToolKey_KnownKey_ReturnsToolKey(Key input, ToolKey expected)
    {
        var result = CanvasInputRouter.ToToolKey(input);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(Key.A)]
    [InlineData(Key.Space)]
    [InlineData(Key.None)]
    public void ToToolKey_UnknownKey_ReturnsNull(Key input)
    {
        var result = CanvasInputRouter.ToToolKey(input);
        Assert.Null(result);
    }

    // ===== SetState / GetState (DP round-trip) =====

    [Fact]
    public void GetState_Default_ReturnsNull()
    {
        WpfContext.Execute(() =>
        {
            var canvas = new Canvas();
            Assert.Null(EditorCanvasBehavior.GetState(canvas));
        });
    }

    [Fact]
    public void SetState_RoundTrip_ReturnsSameInstance()
    {
        WpfContext.Execute(() =>
        {
            var canvas = new Canvas();
            var editor = CreateEditor();
            var state = new EditorCanvasState { Editor = editor };

            EditorCanvasBehavior.SetState(canvas, state);

            Assert.Same(state, EditorCanvasBehavior.GetState(canvas));
            Assert.Same(editor, EditorCanvasBehavior.GetState(canvas).Editor);
        });
    }

    [Fact]
    public void SetState_ToNull_ClearsValue()
    {
        WpfContext.Execute(() =>
        {
            var canvas = new Canvas();
            var state = new EditorCanvasState { Editor = CreateEditor() };
            EditorCanvasBehavior.SetState(canvas, state);

            EditorCanvasBehavior.SetState(canvas, null!);

            Assert.Null(EditorCanvasBehavior.GetState(canvas));
        });
    }

    [Fact]
    public void SetState_OnDifferentCanvases_AreIndependent()
    {
        WpfContext.Execute(() =>
        {
            var canvas1 = new Canvas();
            var canvas2 = new Canvas();
            var state = new EditorCanvasState { Editor = CreateEditor() };

            EditorCanvasBehavior.SetState(canvas1, state);

            Assert.Same(state, EditorCanvasBehavior.GetState(canvas1));
            Assert.Null(EditorCanvasBehavior.GetState(canvas2));
        });
    }

    // ===== SetEditor: создание state + подписка обработчиков =====

    [Fact]
    public void SetEditor_CreatesStateWithEditor()
    {
        WpfContext.Execute(() =>
        {
            var canvas = new Canvas();
            var editor = CreateEditor();

            EditorCanvasBehavior.SetEditor(canvas, editor);

            Assert.Same(editor, EditorCanvasBehavior.GetEditor(canvas));
            Assert.NotNull(EditorCanvasBehavior.GetState(canvas));
            Assert.Same(editor, EditorCanvasBehavior.GetState(canvas).Editor);
        });
    }

    [Fact]
    public void SetEditor_SubscribesMouseDownHandler()
    {
        WpfContext.Execute(() =>
        {
            var canvas = new Canvas();
            EditorCanvasBehavior.SetEditor(canvas, CreateEditor());

            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseDownEvent
            };
            canvas.RaiseEvent(args);

            Assert.True(args.Handled, "MouseDown должен дойти до RouteMouseDown (state.LastButtonRaw записан, Handled=true)");
        });
    }

    [Fact]
    public void SetEditor_SubscribesMouseMoveHandler()
    {
        WpfContext.Execute(() =>
        {
            var canvas = new Canvas();
            var editor = CreateEditor();
            editor.ToolRegistry.ActiveToolKind = ToolKind.Line; // LineTool.GetCursor() = Cross
            EditorCanvasBehavior.SetEditor(canvas, editor);

            var args = new MouseEventArgs(Mouse.PrimaryDevice, 0)
            {
                RoutedEvent = UIElement.MouseMoveEvent
            };
            canvas.RaiseEvent(args);

            Assert.Equal(Cursors.Cross, canvas.Cursor);
        });
    }

    [Fact]
    public void SetEditor_SubscribesMouseUpHandler()
    {
        WpfContext.Execute(() =>
        {
            var canvas = new Canvas();
            EditorCanvasBehavior.SetEditor(canvas, CreateEditor());

            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseUpEvent
            };
            canvas.RaiseEvent(args);

            Assert.True(args.Handled, "MouseUp должен дойти до RouteMouseUp (Handled=true)");
        });
    }

    [Fact]
    public void SetEditor_SubscribesPreviewMouseWheelHandler()
    {
        WpfContext.Execute(() =>
        {
            var canvas = new Canvas();
            var editor = CreateEditor();
            EditorCanvasBehavior.SetEditor(canvas, editor);

            var args = new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, 120)
            {
                RoutedEvent = UIElement.PreviewMouseWheelEvent
            };
            canvas.RaiseEvent(args);

            Assert.Equal(1.1, editor.Zoom, 6);
            Assert.True(args.Handled);
        });
    }

    [Fact]
    public void SetEditor_SubscribesPreviewKeyDownHandler()
    {
        WpfContext.Execute(() =>
        {
            var canvas = new Canvas();
            var editor = CreateEditor();
            var line = new Line(0, 0, 10_000, 10_000);
            editor.Template.Objects.Add(line);
            editor.SelectSingle(line);
            EditorCanvasBehavior.SetEditor(canvas, editor);

            var args = CreateKeyEventArgs(Key.Delete, UIElement.PreviewKeyDownEvent);
            canvas.RaiseEvent(args);

            Assert.True(args.Handled, "PreviewKeyDown должен дойти до RoutePreviewKeyDown (Delete удаляет объект)");
            Assert.Empty(editor.Template.Objects);
        });
    }

    [Fact]
    public void SetEditor_SubscribesKeyDownHandler()
    {
        WpfContext.Execute(() =>
        {
            var canvas = new Canvas();
            var editor = CreateEditor();
            var line = new Line(0, 0, 10_000, 10_000);
            editor.Template.Objects.Add(line);
            editor.SelectSingle(line);
            EditorCanvasBehavior.SetEditor(canvas, editor);

            var args = CreateKeyEventArgs(Key.Delete, UIElement.KeyDownEvent);
            canvas.RaiseEvent(args);

            Assert.True(args.Handled, "KeyDown должен дойти до RouteKeyDown (Delete удаляет объект)");
            Assert.Empty(editor.Template.Objects);
        });
    }

    // ===== SetEditor(null): отписка обработчиков =====

    [Fact]
    public void SetEditor_Null_UnsubscribesHandlersAndClearsState()
    {
        WpfContext.Execute(() =>
        {
            var canvas = new Canvas();
            var editor = CreateEditor();
            EditorCanvasBehavior.SetEditor(canvas, editor);

            EditorCanvasBehavior.SetEditor(canvas, null!);

            Assert.Null(EditorCanvasBehavior.GetState(canvas));
            Assert.Null(EditorCanvasBehavior.GetEditor(canvas));

            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseDownEvent
            };
            canvas.RaiseEvent(args);
            Assert.False(args.Handled, "После SetEditor(null) MouseDown не должен обрабатываться");
        });
    }

    [Fact]
    public void SetEditor_NewValue_ReplacesState()
    {
        WpfContext.Execute(() =>
        {
            var canvas = new Canvas();
            var editor1 = CreateEditor();
            var editor2 = CreateEditor();
            EditorCanvasBehavior.SetEditor(canvas, editor1);

            EditorCanvasBehavior.SetEditor(canvas, editor2);

            Assert.Same(editor2, EditorCanvasBehavior.GetEditor(canvas));
            Assert.Same(editor2, EditorCanvasBehavior.GetState(canvas).Editor);

            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseDownEvent
            };
            canvas.RaiseEvent(args);
            Assert.True(args.Handled, "После замены editor обработчик должен работать с новым state");
        });
    }

    // ===== Unloaded: отписка обработчиков =====

    [Fact]
    public void UnloadedEvent_UnsubscribesHandlersAndClearsState()
    {
        WpfContext.Execute(() =>
        {
            var canvas = new Canvas();
            var editor = CreateEditor();
            EditorCanvasBehavior.SetEditor(canvas, editor);

            canvas.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));

            Assert.Null(EditorCanvasBehavior.GetState(canvas));

            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseDownEvent
            };
            canvas.RaiseEvent(args);
            Assert.False(args.Handled, "После Unloaded MouseDown не должен обрабатываться");
        });
    }

    [Fact]
    public void UnloadedEvent_ThenSetEditorAgain_Resubscribes()
    {
        WpfContext.Execute(() =>
        {
            var canvas = new Canvas();
            var editor1 = CreateEditor();
            EditorCanvasBehavior.SetEditor(canvas, editor1);
            canvas.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));

            // Повторный SetEditor с ТЕМ ЖЕ экземпляром не меняет значение DP → OnEditorChanged не вызывается.
            // Используем второй инстанс, чтобы спровоцировать PropertyChanged callback.
            var editor2 = CreateEditor();
            EditorCanvasBehavior.SetEditor(canvas, editor2);

            Assert.NotNull(EditorCanvasBehavior.GetState(canvas));
            Assert.Same(editor2, EditorCanvasBehavior.GetState(canvas).Editor);
            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseDownEvent
            };
            canvas.RaiseEvent(args);
            Assert.True(args.Handled, "Повторный SetEditor (новый инстанс) должен заново подписать обработчики");
        });
    }


    // ===== MAJOR-1 regression: handler subscription symmetry =====

    [Fact]
    public void SetEditor_Twice_SubscribesAllHandlersExactlyOnce()
    {
        WpfContext.Execute(() =>
        {
            var canvas = new Canvas();
            var editor1 = CreateEditor();
            var editor2 = CreateEditor();

            EditorCanvasBehavior.SetEditor(canvas, editor1);
            EditorCanvasBehavior.SetEditor(canvas, editor2);

            foreach (var routedEvent in AllSubscribedEvents)
                Assert.Equal(1, CountSubscriptions(canvas, routedEvent));
        });
    }

    [Fact]
    public void SetEditor_TwiceThenNull_UnsubscribesAllHandlers()
    {
        WpfContext.Execute(() =>
        {
            var canvas = new Canvas();
            var editor1 = CreateEditor();
            var editor2 = CreateEditor();

            EditorCanvasBehavior.SetEditor(canvas, editor1);
            EditorCanvasBehavior.SetEditor(canvas, editor2);
            EditorCanvasBehavior.SetEditor(canvas, null!);

            foreach (var routedEvent in AllSubscribedEvents)
                Assert.Equal(0, CountSubscriptions(canvas, routedEvent));
            Assert.Null(EditorCanvasBehavior.GetState(canvas));
        });
    }

    [Fact]
    public void Unloaded_Event_RemovesAllHandlerSubscriptions()
    {
        WpfContext.Execute(() =>
        {
            var canvas = new Canvas();
            var editor = CreateEditor();

            EditorCanvasBehavior.SetEditor(canvas, editor);
            canvas.RaiseEvent(new RoutedEventArgs(FrameworkElement.UnloadedEvent));

            foreach (var routedEvent in AllSubscribedEvents)
                Assert.Equal(0, CountSubscriptions(canvas, routedEvent));
            Assert.Null(EditorCanvasBehavior.GetState(canvas));
        });
    }

    private static RoutedEvent[] AllSubscribedEvents { get; } =
    {
        UIElement.MouseDownEvent,
        UIElement.MouseMoveEvent,
        UIElement.MouseUpEvent,
        UIElement.PreviewMouseWheelEvent,
        UIElement.PreviewKeyDownEvent,
        UIElement.KeyDownEvent,
        FrameworkElement.UnloadedEvent,
        UIElement.PreviewMouseLeftButtonDownEvent
    };

    private static int CountSubscriptions(UIElement element, RoutedEvent routedEvent)
    {
        var store = typeof(UIElement)
            .GetProperty("EventHandlersStore", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(element);
        if (store == null) return 0;
        var getItem = store.GetType()
            .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .First(m => m.Name == "get_Item" && m.GetParameters().Length == 1);
        var list = getItem.Invoke(store, new object[] { routedEvent });
        return list == null ? 0 : (int)list.GetType().GetProperty("Count")!.GetValue(list)!;
    }    // ===== Helpers =====

    private static KeyEventArgs CreateKeyEventArgs(Key key, RoutedEvent routedEvent)
        => new(Keyboard.PrimaryDevice, new FakePresentationSource(), 0, key)
        {
            RoutedEvent = routedEvent
        };

}
