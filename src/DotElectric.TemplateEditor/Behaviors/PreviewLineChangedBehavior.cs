using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DotElectric.TemplateEditor.Converters;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.ViewModels;
using DotElectric.TemplateEditor.ViewModels.Managers;

namespace DotElectric.TemplateEditor.Behaviors;

/// <summary>
/// Вспомогательный класс для обновления preview-элементов (линия, прямоугольник, текст).
/// Подписывается на PropertyChanged EditorViewModel и обновляет WPF-элементы напрямую.
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

    private static readonly ConditionalWeakTable<EditorViewModel, Canvas> _canvasRefs = new();
    private static readonly ConditionalWeakTable<EditorViewModel, CachedElements> _cachedElements = new();
    private static readonly ConditionalWeakTable<EditorViewModel, PropertyChangedEventHandler> _handlers = new();
    private static readonly ModelXToCanvasLeftConverter _convX = new();
    private static readonly ModelYToCanvasTopConverter _convY = new();
    private static readonly MicronsToPixelConverter _convPx = new();

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

        PropertyChangedEventHandler handler = (s, e) =>
        {
            if (!_canvasRefs.TryGetValue(vm, out var canvas))
                return;

            var zoom = vm.ZoomPanManager.Zoom;
            var sheetHeightMm = vm.Template?.Sheet?.HeightMm ?? 0;

            if (e.PropertyName == nameof(PreviewManager.PreviewLine))
                UpdatePreviewLine(canvas, vm, zoom, sheetHeightMm);
            else if (e.PropertyName == nameof(PreviewManager.PreviewRectangle))
                UpdatePreviewRectangle(canvas, vm, zoom, sheetHeightMm);
            else if (e.PropertyName == nameof(PreviewManager.PreviewText))
                UpdatePreviewText(canvas, vm, zoom, sheetHeightMm);
        };
        vm.PreviewManager.PropertyChanged += handler;
        _handlers.AddOrUpdate(vm, handler);
    }

    public static void Unregister(EditorViewModel vm)
    {
        if (_handlers.TryGetValue(vm, out var handler))
            vm.PreviewManager.PropertyChanged -= handler;
        _handlers.Remove(vm);
        _canvasRefs.Remove(vm);
        _cachedElements.Remove(vm);
    }

    internal static void UpdatePreviewLine(Canvas canvas, EditorViewModel vm, double zoom, double sheetHeightMm)
    {
        if (!_cachedElements.TryGetValue(vm, out var cached)) return;
        var line = cached.PreviewLineElement;

        var preview = vm.PreviewLine;
        if (preview == null)
        {
            line.Visibility = Visibility.Collapsed;
            return;
        }

        line.Visibility = Visibility.Visible;
        line.X1 = (double)_convX.Convert(new object[] { preview.StartMicronsX, zoom }, typeof(double), null, System.Globalization.CultureInfo.CurrentCulture)!;
        line.Y1 = (double)_convY.Convert(new object[] { preview.StartMicronsY, sheetHeightMm, zoom }, typeof(double), null, System.Globalization.CultureInfo.CurrentCulture)!;
        line.X2 = (double)_convX.Convert(new object[] { preview.EndMicronsX, zoom }, typeof(double), null, System.Globalization.CultureInfo.CurrentCulture)!;
        line.Y2 = (double)_convY.Convert(new object[] { preview.EndMicronsY, sheetHeightMm, zoom }, typeof(double), null, System.Globalization.CultureInfo.CurrentCulture)!;
    }

    internal static void UpdatePreviewRectangle(Canvas canvas, EditorViewModel vm, double zoom, double sheetHeightMm)
    {
        if (!_cachedElements.TryGetValue(vm, out var cached)) return;
        var rect = cached.PreviewRectangleElement;

        var preview = vm.PreviewRectangle;
        if (preview == null)
        {
            rect.Visibility = Visibility.Collapsed;
            return;
        }

        rect.Visibility = Visibility.Visible;
        Canvas.SetLeft(rect, (double)_convX.Convert(new object[] { preview.MicronsX, zoom }, typeof(double), null, System.Globalization.CultureInfo.CurrentCulture)!);
        Canvas.SetTop(rect, (double)_convY.Convert(new object[] { preview.MicronsY + preview.HeightMicrons, sheetHeightMm, zoom }, typeof(double), null, System.Globalization.CultureInfo.CurrentCulture)!);
        rect.Width = (double)_convPx.Convert(new object[] { preview.WidthMicrons, zoom }, typeof(double), null, System.Globalization.CultureInfo.CurrentCulture)!;
        rect.Height = (double)_convPx.Convert(new object[] { preview.HeightMicrons, zoom }, typeof(double), null, System.Globalization.CultureInfo.CurrentCulture)!;
    }

    internal static void UpdatePreviewText(Canvas canvas, EditorViewModel vm, double zoom, double sheetHeightMm)
    {
        if (!_cachedElements.TryGetValue(vm, out var cached)) return;
        var tb = cached.PreviewTextElement;

        var preview = vm.PreviewText;
        if (preview == null)
        {
            tb.Visibility = Visibility.Collapsed;
            return;
        }

        tb.Visibility = Visibility.Visible;
        tb.Text = preview.Content;
        var fontPixels = Coordinate.ToMm(preview.FontSizeMicrons) * zoom;
        tb.FontSize = fontPixels;
        tb.FontFamily = preview.FontName switch
        {
            "ГОСТ А" => new FontFamily("pack://application:,,,/Resources/Fonts/#GOST Type AU"),
            "ГОСТ Б" => new FontFamily("pack://application:,,,/Resources/Fonts/#GOST Type BU"),
            _ => new FontFamily("Segoe UI")
        };
        Canvas.SetLeft(tb, (double)_convX.Convert(new object[] { preview.MicronsX, zoom }, typeof(double), null, System.Globalization.CultureInfo.CurrentCulture)!);
        Canvas.SetTop(tb, (double)_convY.Convert(new object[] { preview.MicronsY + preview.FontSizeMicrons, sheetHeightMm, zoom }, typeof(double), null, System.Globalization.CultureInfo.CurrentCulture)!);
    }
}
