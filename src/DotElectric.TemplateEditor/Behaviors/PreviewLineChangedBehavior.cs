using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.ViewModels;
using DotElectric.TemplateEditor.ViewModels.Managers;

namespace DotElectric.TemplateEditor.Behaviors;

/// <summary>
/// Рендерер предпросмотра: обновляет preview-элементы (линия, прямоугольник, текст) на канвасе.
/// Единственный читатель preview-состояния. Подписан на PreviewManager (смена объекта —
/// показать/скрыть и переподписать) и на INPC текущего preview-объекта (мутации геометрии
/// во время жеста рисования, без ре-ассайна ссылки).
/// Обходит проблему MultiBinding с chain-свойствами (PreviewLine.StartMicronsX).
/// </summary>
public static class PreviewLineChangedBehavior
{
    internal sealed class CachedElements
    {
        public required System.Windows.Shapes.Line PreviewLineElement { get; init; }
        public required System.Windows.Shapes.Rectangle PreviewRectangleElement { get; init; }
        public required System.Windows.Controls.TextBlock PreviewTextElement { get; init; }
    }

    private sealed class ObjectSubscriptions
    {
        public Line? Line;
        public PropertyChangedEventHandler? LineHandler;
        public Rectangle? Rectangle;
        public PropertyChangedEventHandler? RectangleHandler;
        public Text? Text;
        public PropertyChangedEventHandler? TextHandler;
    }

    private static readonly ConditionalWeakTable<EditorViewModel, Canvas> _canvasRefs = new();
    private static readonly ConditionalWeakTable<EditorViewModel, CachedElements> _cachedElements = new();
    private static readonly ConditionalWeakTable<EditorViewModel, PropertyChangedEventHandler> _handlers = new();
    private static readonly ConditionalWeakTable<EditorViewModel, ObjectSubscriptions> _objectSubs = new();

    public static void RegisterCanvas(Canvas canvas, EditorViewModel vm)
    {
        _canvasRefs.AddOrUpdate(vm, canvas);

        var line = canvas.FindName("PreviewLineElement") as System.Windows.Shapes.Line;
        var rect = canvas.FindName("PreviewRectangleElement") as System.Windows.Shapes.Rectangle;
        var text = canvas.FindName("PreviewTextElement") as TextBlock;

        if (line != null && rect != null && text != null)
        {
            _cachedElements.AddOrUpdate(vm, new CachedElements
            {
                PreviewLineElement = line,
                PreviewRectangleElement = rect,
                PreviewTextElement = text
            });
        }

        if (!_objectSubs.TryGetValue(vm, out _))
            _objectSubs.AddOrUpdate(vm, new ObjectSubscriptions());

        PropertyChangedEventHandler handler = (s, e) =>
        {
            if (e.PropertyName == nameof(PreviewManager.PreviewLine))
                SwapLine(vm);
            else if (e.PropertyName == nameof(PreviewManager.PreviewRectangle))
                SwapRectangle(vm);
            else if (e.PropertyName == nameof(PreviewManager.PreviewText))
                SwapText(vm);
        };
        vm.PreviewManager.PropertyChanged += handler;
        _handlers.AddOrUpdate(vm, handler);
    }

    public static void Unregister(EditorViewModel vm)
    {
        if (_handlers.TryGetValue(vm, out var handler))
            vm.PreviewManager.PropertyChanged -= handler;
        if (_objectSubs.TryGetValue(vm, out var subs))
        {
            Unsubscribe(ref subs.Line, ref subs.LineHandler);
            Unsubscribe(ref subs.Rectangle, ref subs.RectangleHandler);
            Unsubscribe(ref subs.Text, ref subs.TextHandler);
        }
        _handlers.Remove(vm);
        _canvasRefs.Remove(vm);
        _cachedElements.Remove(vm);
        _objectSubs.Remove(vm);
    }

    private static void SwapLine(EditorViewModel vm)
    {
        if (!_objectSubs.TryGetValue(vm, out var subs)) return;
        Swap(vm, ref subs.Line, ref subs.LineHandler, () => vm.PreviewManager.PreviewLine, UpdatePreviewLine);
    }

    private static void SwapRectangle(EditorViewModel vm)
    {
        if (!_objectSubs.TryGetValue(vm, out var subs)) return;
        Swap(vm, ref subs.Rectangle, ref subs.RectangleHandler, () => vm.PreviewManager.PreviewRectangle, UpdatePreviewRectangle);
    }

    private static void SwapText(EditorViewModel vm)
    {
        if (!_objectSubs.TryGetValue(vm, out var subs)) return;
        Swap(vm, ref subs.Text, ref subs.TextHandler, () => vm.PreviewManager.PreviewText, UpdatePreviewText);
    }

    /// <summary>
    /// Смена preview-объекта: отписка от старого, подписка на новый (INPC — мутации
    /// геометрии без ре-ассайна), перерисовка элемента (показать/скрыть).
    /// </summary>
    private static void Swap<T>(
        EditorViewModel vm,
        ref T? current,
        ref PropertyChangedEventHandler? handler,
        Func<T?> previewOf,
        UpdateElement update)
        where T : class, INotifyPropertyChanged
    {
        Unsubscribe(ref current, ref handler);

        var preview = previewOf();
        current = preview;
        handler = preview == null ? null : (_, _) => Render(vm, update);
        if (preview != null)
            preview.PropertyChanged += handler;

        Render(vm, update);
    }

    private static void Unsubscribe<T>(ref T? current, ref PropertyChangedEventHandler? handler)
        where T : class, INotifyPropertyChanged
    {
        if (current != null && handler != null)
            current.PropertyChanged -= handler;
        current = null;
        handler = null;
    }

    private delegate void UpdateElement(Canvas canvas, EditorViewModel vm, double zoom, double sheetHeightMm);

    private static void Render(EditorViewModel vm, UpdateElement update)
    {
        if (!_canvasRefs.TryGetValue(vm, out var canvas)) return;
        update(canvas, vm, vm.ZoomPanManager.Zoom, SheetHeightMmOf(vm));
    }

    private static double SheetHeightMmOf(EditorViewModel vm) => vm.Template?.Sheet?.HeightMm ?? 0;

    internal static void UpdatePreviewLine(Canvas canvas, EditorViewModel vm, double zoom, double sheetHeightMm)
    {
        if (!_cachedElements.TryGetValue(vm, out var cached)) return;
        var line = cached.PreviewLineElement;

        var preview = vm.PreviewManager.PreviewLine;
        if (preview == null)
        {
            line.Visibility = Visibility.Collapsed;
            return;
        }

        line.Visibility = Visibility.Visible;
        line.X1 = Coordinate.ToMm(preview.StartMicronsX) * zoom;
        line.Y1 = RenderRules.ModelYToTop(preview.StartMicronsY, sheetHeightMm, zoom);
        line.X2 = Coordinate.ToMm(preview.EndMicronsX) * zoom;
        line.Y2 = RenderRules.ModelYToTop(preview.EndMicronsY, sheetHeightMm, zoom);
    }

    internal static void UpdatePreviewRectangle(Canvas canvas, EditorViewModel vm, double zoom, double sheetHeightMm)
    {
        if (!_cachedElements.TryGetValue(vm, out var cached)) return;
        var rect = cached.PreviewRectangleElement;

        var preview = vm.PreviewManager.PreviewRectangle;
        if (preview == null)
        {
            rect.Visibility = Visibility.Collapsed;
            return;
        }

        rect.Visibility = Visibility.Visible;
        Canvas.SetLeft(rect, Coordinate.ToMm(preview.MicronsX) * zoom);
        Canvas.SetTop(rect, RenderRules.ModelYToTop(RenderRules.AnchorTopMicrons(preview), sheetHeightMm, zoom));
        rect.Width = Coordinate.ToMm(preview.WidthMicrons) * zoom;
        rect.Height = Coordinate.ToMm(preview.HeightMicrons) * zoom;
    }

    internal static void UpdatePreviewText(Canvas canvas, EditorViewModel vm, double zoom, double sheetHeightMm)
    {
        if (!_cachedElements.TryGetValue(vm, out var cached)) return;
        var tb = cached.PreviewTextElement;

        var preview = vm.PreviewManager.PreviewText;
        if (preview == null)
        {
            tb.Visibility = Visibility.Collapsed;
            return;
        }

        tb.Visibility = Visibility.Visible;
        tb.Text = preview.Content;
        tb.FontSize = Coordinate.ToMm(preview.FontSizeMicrons) * zoom;
        tb.FontFamily = RenderRules.FontFamilyFor(preview.FontName);
        Canvas.SetLeft(tb, Coordinate.ToMm(RenderRules.AnchorLeftMicrons(preview)) * zoom);
        Canvas.SetTop(tb, RenderRules.ModelYToTop(RenderRules.AnchorTopMicrons(preview), sheetHeightMm, zoom));
    }
}
