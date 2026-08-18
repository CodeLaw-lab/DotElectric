using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using DotElectric.TemplateEditor.Behaviors;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tests.Helpers;
using DotElectric.TemplateEditor.Tools;
using DotElectric.TemplateEditor.ViewModels;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Behaviors;

public class CanvasInputRouterTests
{
    private static EditorViewModel CreateEditor()
    {
        var template = new Template();
        var mockService = new Mock<ITemplateService>();
        mockService.Setup(s => s.CreateNew(It.IsAny<string>(), It.IsAny<SheetOrientation>()))
            .Returns(() => new Template());
        mockService.Setup(s => s.Validate(It.IsAny<Template>()))
            .Returns(Enumerable.Empty<string>());
        return new EditorViewModel(template, mockService.Object, printService: new Mock<IPrintService>().Object);
    }

    private static EditorCanvasState CreateState(EditorViewModel editor)
        => new() { Editor = editor };

    private static MouseButtonEventArgs CreateMouseButtonArgs(MouseButton button, int clickCount = 1)
    {
        var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, button)
        {
            // Handled setter требует не-null RoutedEvent
            RoutedEvent = UIElement.MouseDownEvent
        };
        // ClickCount имеет internal setter в .NET 10 (backing field _count) — reflection
        if (clickCount != 1)
        {
            var field = typeof(MouseButtonEventArgs).GetField("_count", BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException("MouseButtonEventArgs._count not found");
            field.SetValue(args, clickCount);
        }
        return args;
    }

    private static KeyEventArgs CreateKeyEventArgs(Key key)
        => new(Keyboard.PrimaryDevice, new FakePresentationSource(), 0, key)
        {
            RoutedEvent = Keyboard.KeyDownEvent
        };

    private static MouseWheelEventArgs CreateWheelArgs(int delta)
        => new(Mouse.PrimaryDevice, 0, delta)
        {
            RoutedEvent = Mouse.PreviewMouseWheelEvent
        };

    // ===== GetCurrentTool (pure) =====

    [Theory]
    [InlineData(ToolKind.Select, typeof(SelectTool))]
    [InlineData(ToolKind.Line, typeof(DrawingLineTool))]
    [InlineData(ToolKind.Rectangle, typeof(DrawingRectangleTool))]
    [InlineData(ToolKind.Text, typeof(TextTool))]
    [InlineData(ToolKind.Resize, typeof(ResizeTool))]
    public void GetCurrentTool_ReturnsToolForActiveKind(ToolKind activeKind, Type expectedType)
    {
        var editor = CreateEditor();
        editor.ToolRegistry.ActiveToolKind = activeKind;

        var tool = CanvasInputRouter.GetCurrentTool(editor);

        Assert.IsType(expectedType, tool);
    }

    [Fact]
    public void GetCurrentTool_ReturnsCachedInstance()
    {
        var editor = CreateEditor();
        editor.ToolRegistry.ActiveToolKind = ToolKind.Line;

        var first = CanvasInputRouter.GetCurrentTool(editor);
        var second = CanvasInputRouter.GetCurrentTool(editor);

        Assert.Same(first, second);
        Assert.Same(editor.ToolRegistry.ActiveToolInstance, first);
        Assert.Same(editor.GetOrCreateTool<DrawingLineTool>(), first);
    }
// ===== ToWpfCursor (pure) =====

    [Fact]
    public void ToWpfCursor_AllKnownCursors_MapsToWpfInstances()
    {
        var expected = new Dictionary<ToolCursor, Cursor>
        {
            [ToolCursor.Hand] = Cursors.Hand,
            [ToolCursor.Cross] = Cursors.Cross,
            [ToolCursor.SizeNWSE] = Cursors.SizeNWSE,
            [ToolCursor.SizeNESW] = Cursors.SizeNESW,
            [ToolCursor.SizeNS] = Cursors.SizeNS,
            [ToolCursor.SizeWE] = Cursors.SizeWE,
            [ToolCursor.IBeam] = Cursors.IBeam,
            [ToolCursor.Arrow] = Cursors.Arrow
        };

        foreach (var (cursor, cursorInstance) in expected)
            Assert.Same(cursorInstance, CanvasInputRouter.ToWpfCursor(cursor));

        // Unknown value falls back to Arrow
        Assert.Same(Cursors.Arrow, CanvasInputRouter.ToWpfCursor((ToolCursor)999));
    }

    // ===== RoutePanDown (STA) =====

    [Fact]
    public void RoutePanDown_WithWindowPoint_StartsPanningAndStoresStartPoint()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var state = CreateState(editor);
            var canvas = new Canvas();
            // CaptureMouse требует подключённый PresentationSource — окно показываем
            var window = new Window { Content = canvas };
            var windowPoint = new Point(100, 50);

            try
            {
                window.Show();
                window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);

                CanvasInputRouter.RoutePanDown(canvas, state, windowPoint);

                Assert.True(state.IsPanning);
                Assert.True(canvas.IsMouseCaptured, "Старт пана должен захватывать мышь");
                Assert.Equal(windowPoint, state.PanStartWpfPoint);
                Assert.Equal(new Point(0, 0), state.PanAppliedModelDelta);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void RoutePanDown_WithoutWindowPoint_KeepsDefaultStartPoint()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var state = CreateState(editor);
            var canvas = new Canvas();

            CanvasInputRouter.RoutePanDown(canvas, state, null);

            Assert.True(state.IsPanning);
            Assert.Equal(new Point(0, 0), state.PanStartWpfPoint);
        });
    }

    // ===== IsPanGesture (pure) =====

    [Theory]
    [InlineData(MouseButton.Middle, false, false, false, true)] // средняя кнопка — всегда пан
    [InlineData(MouseButton.Middle, true, false, false, true)]  // модификаторы при средней не важны
    [InlineData(MouseButton.Left, true, false, false, true)]    // Left + Space
    [InlineData(MouseButton.Left, false, true, false, true)]    // Left + LeftAlt
    [InlineData(MouseButton.Left, false, false, true, true)]    // Left + RightAlt
    [InlineData(MouseButton.Left, true, true, true, true)]      // комбинации
    [InlineData(MouseButton.Left, false, false, false, false)]  // Left без модификаторов — не пан
    [InlineData(MouseButton.Right, true, true, true, false)]    // правая кнопка — не пан
    public void IsPanGesture_CanonicalGestureSet_ReturnsExpected(MouseButton button, bool space, bool leftAlt, bool rightAlt, bool expected)
    {
        Assert.Equal(expected, CanvasInputRouter.IsPanGesture(button, space, leftAlt, rightAlt));
    }

    // ===== Пан-жест: полный цикл Left-пути (STA) =====
    // Триггер через RouteMouseDown с зажатым модификатором unit-тестом не покрывается
    // (Keyboard.IsKeyDown не фейчится) — детекция закрыта теорией IsPanGesture выше.

    [Fact]
    public void RoutePanDown_LeftGestureFullCycle_StopsCleanlyAndRefreshesGrid()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var refreshCount = 0;
            editor.GridManager.GridInvalidated = () => refreshCount++;
            var state = CreateState(editor);
            var canvas = new Canvas();
            // Окно НЕ показываем: с PresentationSource GetPosition возвращает реальную
            // позицию курсора, и дельта становится недетерминированной; без него — (0,0).
            // Захват мыши проверяется в RoutePanDown_WithWindowPoint (показанное окно).
            var window = new Window { Content = canvas };

            // Жест Left+Space/Alt: старт → движение → завершение; единственный источник истины — state
            CanvasInputRouter.RoutePanDown(canvas, state, new Point(100, 100));
            Assert.True(state.IsPanning);

            CanvasInputRouter.RouteMouseMove(canvas, CreateMouseButtonArgs(MouseButton.Left), state);

            // GetPosition(window) = (0,0) без PresentationSource → дельта (-100,-100) при zoom 1.0
            Assert.Equal(-100.0, editor.ZoomPanManager.PanOffsetX, 6);
            Assert.Equal(-100.0, editor.ZoomPanManager.PanOffsetY, 6);

            CanvasInputRouter.RouteMouseUp(canvas, CreateMouseButtonArgs(MouseButton.Left), state);

            Assert.False(state.IsPanning);
            Assert.Equal(new Point(0, 0), state.PanAppliedModelDelta);
            Assert.True(refreshCount > 0, "GridManager.RefreshGridNodes должен быть вызван после пана");
        });
    }

    // ===== RouteMouseDown (STA) =====

    [Fact]
    public void RouteMouseDown_LeftClickWithLineTool_CreatesPreviewLine()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            editor.ToolRegistry.ActiveToolKind = ToolKind.Line;
            var state = CreateState(editor);
            var canvas = new Canvas();
            var args = CreateMouseButtonArgs(MouseButton.Left);

            CanvasInputRouter.RouteMouseDown(canvas, args, state);

            Assert.True(args.Handled);
            Assert.Equal(0, state.LastButtonRaw);
            Assert.NotNull(editor.PreviewManager.PreviewLine);
        });
    }

    [Fact]
    public void RouteMouseDown_LeftClickWithSelectTool_RecordsLastButton()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var state = CreateState(editor);
            var canvas = new Canvas();
            var args = CreateMouseButtonArgs(MouseButton.Left);

            CanvasInputRouter.RouteMouseDown(canvas, args, state);

            Assert.True(args.Handled);
            Assert.Equal(0, state.LastButtonRaw);
        });
    }

    [Fact]
    public void RouteMouseDown_DoubleClickWithSelectTool_StartsInlineEditingOnText()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var text = new Text(0, 297_000, "hello", 14_000); // anchor совпадает с модель-точкой (0,297000)
            editor.Template.Objects.Add(text);
            var state = CreateState(editor);
            var canvas = new Canvas();
            var args = CreateMouseButtonArgs(MouseButton.Left, clickCount: 2);

            CanvasInputRouter.RouteMouseDown(canvas, args, state);

            Assert.True(args.Handled);
            Assert.True(editor.InlineEditManager.IsEditing);
        });
    }

    [Fact]
    public void RouteMouseDown_RightClick_SetsHandledAndDoesNotThrow()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var state = CreateState(editor);
            var canvas = new Canvas();
            var args = CreateMouseButtonArgs(MouseButton.Right);

            var exception = Record.Exception(() => CanvasInputRouter.RouteMouseDown(canvas, args, state));

            Assert.Null(exception);
            Assert.True(args.Handled);
        });
    }

    [Fact]
    public void RouteMouseDown_MiddleButton_StartsPanning()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var state = CreateState(editor);
            var canvas = new Canvas();
            var args = CreateMouseButtonArgs(MouseButton.Middle);

            CanvasInputRouter.RouteMouseDown(canvas, args, state);

            Assert.True(args.Handled);
            Assert.True(state.IsPanning, "Средняя кнопка должна запускать панорамирование");
        });
    }

    [Fact]
    public void RouteMouseDown_RightClickInsideUserControl_OpensContextMenu()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var state = CreateState(editor);
            var canvas = new Canvas();
            var menu = new ContextMenu();
            var userControl = new UserControl { Content = canvas, ContextMenu = menu };
            var window = new Window { Width = 200, Height = 150, Content = userControl };
            var args = CreateMouseButtonArgs(MouseButton.Right);

            try
            {
                // ContextMenu.IsOpen работает только при подключённом PresentationSource
                window.Show();
                window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);

                var exception = Record.Exception(() => CanvasInputRouter.RouteMouseDown(canvas, args, state));

                Assert.Null(exception);
                Assert.True(args.Handled);
                Assert.True(menu.IsOpen, "ContextMenu должен открыться при подключённом Window");
            }
            finally
            {
                window.Close();
            }
        });
    }

    // ===== RouteMouseMove (STA) =====

    [Fact]
    public void RouteMouseMove_NotPanning_AppliesToolCursor()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            editor.ToolRegistry.ActiveToolKind = ToolKind.Line;
            var state = CreateState(editor);
            var canvas = new Canvas();
            var args = CreateMouseButtonArgs(MouseButton.Left);

            CanvasInputRouter.RouteMouseMove(canvas, args, state);

            Assert.Equal(Cursors.Cross, canvas.Cursor);
        });
    }

    [Fact]
    public void RouteMouseMove_PanningWithoutWindow_SetsSizeAllCursor()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var state = CreateState(editor);
            state.IsPanning = true;
            var canvas = new Canvas();
            var args = CreateMouseButtonArgs(MouseButton.Middle);

            CanvasInputRouter.RouteMouseMove(canvas, args, state);

            Assert.Equal(Cursors.SizeAll, canvas.Cursor);
        });
    }

    [Fact]
    public void RouteMouseMove_PanningInWindow_AppliesIncrementalPan()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var state = CreateState(editor);
            state.IsPanning = true;
            state.PanStartWpfPoint = new Point(100, 100);
            state.PanAppliedModelDelta = new Point(0, 0);
            var canvas = new Canvas();
            var window = new Window { Content = canvas };
            var args = CreateMouseButtonArgs(MouseButton.Middle);

            CanvasInputRouter.RouteMouseMove(canvas, args, state);

            // GetPosition(window) = (0,0) без PresentationSource → дельта (-100,-100),
            // totalModelDelta = (-100, 100) при zoom 1.0
            Assert.Equal(-100.0, editor.ZoomPanManager.PanOffsetX, 6);
            Assert.Equal(-100.0, editor.ZoomPanManager.PanOffsetY, 6);
            Assert.Equal(new Point(-100, 100), state.PanAppliedModelDelta);
            Assert.Equal(Cursors.SizeAll, canvas.Cursor);
        });
    }

    // ===== ApplyPan (pure) =====

    [Fact]
    public void ApplyPan_FirstMove_AppliesTotalDelta()
    {
        var editor = CreateEditor();
        var state = CreateState(editor);
        state.PanStartWpfPoint = new Point(10, 10);
        state.PanAppliedModelDelta = new Point(0, 0);

        CanvasInputRouter.ApplyPan(state, new Point(110, 60));

        // deltaPx=(100,50) → totalModelDelta=(100,-50) → PanCanvas(100,-50)
        Assert.Equal(100.0, editor.ZoomPanManager.PanOffsetX, 6);
        Assert.Equal(50.0, editor.ZoomPanManager.PanOffsetY, 6);
        Assert.Equal(new Point(100, -50), state.PanAppliedModelDelta);
    }

    [Fact]
    public void ApplyPan_SecondMove_AppliesOnlyIncrementalDelta()
    {
        var editor = CreateEditor();
        var state = CreateState(editor);
        state.PanStartWpfPoint = new Point(10, 10);
        state.PanAppliedModelDelta = new Point(100, -50);

        CanvasInputRouter.ApplyPan(state, new Point(210, 110));

        // total=(200,-100), incremental=(100,-50) — повторное применение той же дельты не должно накапливаться
        Assert.Equal(100.0, editor.ZoomPanManager.PanOffsetX, 6);
        Assert.Equal(50.0, editor.ZoomPanManager.PanOffsetY, 6);
        Assert.Equal(new Point(200, -100), state.PanAppliedModelDelta);
    }

    [Fact]
    public void ApplyPan_ZoomScalesModelDeltaToPixels()
    {
        var editor = CreateEditor();
        editor.SetZoom(2.0);
        var state = CreateState(editor);
        state.PanStartWpfPoint = new Point(0, 0);
        state.PanAppliedModelDelta = new Point(0, 0);

        CanvasInputRouter.ApplyPan(state, new Point(10, 10));

        // total=(5,-5) модель-мм → PanCanvas умножает на zoom=2 → 10 пикселей
        Assert.Equal(10.0, editor.ZoomPanManager.PanOffsetX, 6);
        Assert.Equal(10.0, editor.ZoomPanManager.PanOffsetY, 6);
    }

    // ===== RouteMouseUp (STA) =====

    [Theory]
    [InlineData(MouseButton.Middle)]
    [InlineData(MouseButton.Left)]
    public void RouteMouseUp_PanEnd_StopsPanningAndRefreshesGrid(MouseButton button)
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var refreshCount = 0;
            editor.GridManager.GridInvalidated = () => refreshCount++;
            var state = CreateState(editor);
            state.IsPanning = true;
            state.PanAppliedModelDelta = new Point(10, 10);
            var canvas = new Canvas();
            var args = CreateMouseButtonArgs(button);

            CanvasInputRouter.RouteMouseUp(canvas, args, state);

            Assert.False(state.IsPanning);
            Assert.Equal(new Point(0, 0), state.PanAppliedModelDelta);
            Assert.True(args.Handled);
            Assert.True(refreshCount > 0, "GridManager.RefreshGridNodes должен быть вызван после пана");
        });
    }

    [Fact]
    public void RouteMouseUp_RightButton_ReturnsEarly()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var state = CreateState(editor);
            state.IsPanning = true;
            var canvas = new Canvas();
            var args = CreateMouseButtonArgs(MouseButton.Right);

            CanvasInputRouter.RouteMouseUp(canvas, args, state);

            Assert.True(state.IsPanning, "Панорамирование не должно завершаться по правой кнопке");
            Assert.False(args.Handled);
        });
    }

    [Fact]
    public void RouteMouseUp_NotPanning_RoutesToToolAndResetsLastButton()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var line = new Line(0, 0, 10_000, 10_000);
            editor.Template.Objects.Add(line);
            editor.SelectSingle(line);
            var state = CreateState(editor);
            state.LastButtonRaw = 0;
            var canvas = new Canvas();
            var args = CreateMouseButtonArgs(MouseButton.Left);

            CanvasInputRouter.RouteMouseUp(canvas, args, state);

            Assert.True(args.Handled);
            Assert.Equal(-1, state.LastButtonRaw);
        });
    }

    // ===== RouteMouseWheel (STA) =====

    [Fact]
    public void RouteMouseWheel_PositiveDelta_ZoomsIn()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var state = CreateState(editor);
            var canvas = new Canvas();
            var args = CreateWheelArgs(120);

            CanvasInputRouter.RouteMouseWheel(canvas, args, state);

            Assert.Equal(1.1, editor.Zoom, 6);
            Assert.True(args.Handled);
        });
    }

    [Fact]
    public void RouteMouseWheel_NegativeDelta_ZoomsOut()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var state = CreateState(editor);
            var canvas = new Canvas();
            var args = CreateWheelArgs(-120);

            CanvasInputRouter.RouteMouseWheel(canvas, args, state);

            Assert.Equal(1.0 / 1.1, editor.Zoom, 6);
            Assert.True(args.Handled);
        });
    }

    // NOTE: ветка «tool.OnMouseWheel == true → zoom не применять» (CanvasInputRouter.cs:112)
    // недостижима через публичный API: все 6 реализаций ITool sealed и возвращают false.
    // Инъекция фейка в ToolRegistry._toolCache невозможна — GetOrCreateTool<T>() делает (T)cached cast.
    // Строки RouteMouseWheel полностью покрыты тестами RouteMouseWheel_Positive/NegativeDelta.

    // ===== RoutePreviewKeyDown / RouteKeyDown (STA) =====

    [Fact]
    public void RouteKeyDown_DeleteWithSelectTool_DeletesSelectedObject()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var line = new Line(0, 0, 10_000, 10_000);
            editor.Template.Objects.Add(line);
            editor.SelectSingle(line);
            var state = CreateState(editor);
            var canvas = new Canvas();
            var args = CreateKeyEventArgs(Key.Delete);

            CanvasInputRouter.RouteKeyDown(canvas, args, state);

            Assert.True(args.Handled);
            Assert.Empty(editor.Template.Objects);
        });
    }

    [Fact]
    public void RouteKeyDown_EscapeWithSelectTool_ClearsSelection()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var line = new Line(0, 0, 10_000, 10_000);
            editor.Template.Objects.Add(line);
            editor.SelectSingle(line);
            var state = CreateState(editor);
            var canvas = new Canvas();
            var args = CreateKeyEventArgs(Key.Escape);

            CanvasInputRouter.RouteKeyDown(canvas, args, state);

            Assert.True(args.Handled);
            Assert.Empty(editor.SelectedObjects);
        });
    }

    [Fact]
    public void RouteKeyDown_UnknownKey_NotHandled()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var state = CreateState(editor);
            var canvas = new Canvas();
            var args = CreateKeyEventArgs(Key.A);

            CanvasInputRouter.RouteKeyDown(canvas, args, state);

            Assert.False(args.Handled);
        });
    }

    [Fact]
    public void RouteKeyDown_WhileInlineEditing_NotHandled()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var text = new Text(0, 0, "hello", 14_000);
            editor.StartInlineEditing(text);
            var state = CreateState(editor);
            var canvas = new Canvas();
            var args = CreateKeyEventArgs(Key.Delete);

            CanvasInputRouter.RouteKeyDown(canvas, args, state);

            Assert.False(args.Handled);
        });
    }

    [Fact]
    public void RoutePreviewKeyDown_WhileInlineEditing_NotHandled()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var text = new Text(0, 0, "hello", 14_000);
            editor.StartInlineEditing(text);
            var state = CreateState(editor);
            var canvas = new Canvas();
            var args = CreateKeyEventArgs(Key.Escape);

            CanvasInputRouter.RoutePreviewKeyDown(canvas, args, state);

            Assert.False(args.Handled);
        });
    }

    [Fact]
    public void RoutePreviewKeyDown_EscapeWithSelectTool_Handled()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var line = new Line(0, 0, 10_000, 10_000);
            editor.Template.Objects.Add(line);
            editor.SelectSingle(line);
            var state = CreateState(editor);
            var canvas = new Canvas();
            var args = CreateKeyEventArgs(Key.Escape);

            CanvasInputRouter.RoutePreviewKeyDown(canvas, args, state);

            Assert.True(args.Handled);
        });
    }

    [Fact]
    public void RouteKeyDown_EscapeWithLineTool_ResetsAndSwitchesToSelect()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            editor.ToolRegistry.ActiveToolKind = ToolKind.Line;
            var state = CreateState(editor);
            var canvas = new Canvas();
            var args = CreateKeyEventArgs(Key.Escape);

            CanvasInputRouter.RouteKeyDown(canvas, args, state);

            Assert.True(args.Handled);
            Assert.Equal(ToolKind.Select, editor.ToolRegistry.ActiveToolKind);
        });
    }

    [Fact]
    public void RoutePreviewKeyDown_OtherElementFocused_NotHandled()
    {
        WpfContext.Execute(() =>
        {
            var editor = CreateEditor();
            var state = CreateState(editor);
            var canvas = new Canvas();
            var textBox = new TextBox();
            var window = new Window
            {
                Width = 300,
                Height = 200,
                Content = new StackPanel { Children = { canvas, textBox } }
            };

            try
            {
                window.Show();
                window.Activate();
                window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);

                // GetFocusedElement(canvas) читает FocusedElementProperty НА canvas (не на scope Window).
                // Реальный клавиатурный фокус в headless CI недетерминирован и DP на canvas не ставит —
                // устанавливаем логический фокус scope canvas напрямую (детерминированно).
                FocusManager.SetFocusedElement(canvas, textBox);
                window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
                Assert.Same(textBox, FocusManager.GetFocusedElement(canvas));

                var args = CreateKeyEventArgs(Key.Escape);
                CanvasInputRouter.RoutePreviewKeyDown(canvas, args, state);

                Assert.False(args.Handled, "При фокусе на другом элементе роутер не должен обрабатывать клавишу");
            }
            finally
            {
                window.Close();
            }
        });
    }

}
