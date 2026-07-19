using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DotElectric.TemplateEditor.Behaviors;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Views;

/// <summary>
/// EditorCanvas — холст редактора.
/// Вся логика в XAML (attached behaviors, DataTemplates, bindings).
/// Code-behind содержит только регистрацию поведения и обновление слоя сетки.
/// </summary>
public partial class EditorCanvas : UserControl
{
    public EditorCanvas()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    private void OnHorizontalScrollBarChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (DataContext is EditorViewModel vm)
        {
            vm.ZoomPanManager.SetScrollX(e.NewValue);
        }
    }

    private void OnVerticalScrollBarChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (DataContext is EditorViewModel vm)
        {
            vm.ZoomPanManager.SetScrollY(e.NewValue);
        }
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (DataContext is EditorViewModel vm)
        {
            var vpWidth = Math.Max(0, e.NewSize.Width - SystemParameters.ScrollWidth);
            var vpHeight = Math.Max(0, e.NewSize.Height - SystemParameters.ScrollHeight);
            vm.ZoomPanManager.SetViewportSize(vpWidth, vpHeight);
        }
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is EditorViewModel oldVm)
        {
            oldVm.GridManager.GridInvalidated = null;
            oldVm.PrintVisualProvider = null;
        }

        if (e.NewValue is EditorViewModel newVm)
        {
            PreviewLineChangedBehavior.RegisterCanvas(DrawingCanvas, newVm);

            // Подписываемся на уведомление о необходимости перерисовки сетки
            // Все вызовы GridInvalidated происходят из UI-потока (binding), поэтому
            // Dispatcher.InvokeAsync не нужен — вызываем InvalidateVisual напрямую.
            newVm.GridManager.GridInvalidated = () =>
            {
                GridNodesLayer.SetNodes(newVm.GridManager.RawNodeData, newVm.GridManager.RawNodeCount);
            };

            RefreshGridNodesLayer();
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is EditorViewModel vm)
        {
            PreviewLineChangedBehavior.RegisterCanvas(DrawingCanvas, vm);
            vm.PrintVisualProvider = new CanvasPrintVisualProvider(DrawingCanvas);
        }

        HorizontalScrollBar.ValueChanged += OnHorizontalScrollBarChanged;
        VerticalScrollBar.ValueChanged += OnVerticalScrollBarChanged;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        HorizontalScrollBar.ValueChanged -= OnHorizontalScrollBarChanged;
        VerticalScrollBar.ValueChanged -= OnVerticalScrollBarChanged;

        if (DataContext is EditorViewModel vm)
        {
            PreviewLineChangedBehavior.Unregister(vm);
        }
    }

    private void RefreshGridNodesLayer()
    {
        if (DataContext is EditorViewModel vm)
        {
            GridNodesLayer.SetNodes(vm.GridManager.RawNodeData, vm.GridManager.RawNodeCount);
            GridNodesLayer.InvalidateVisual();
        }
    }

    private void InlineTextEditor_LostFocus(object sender, RoutedEventArgs e)
    {
        if (DataContext is EditorViewModel vm && vm.InlineEditManager.IsEditing)
        {
            vm.CommitInlineEditingCommand.Execute(null);
        }
    }

    private sealed class CanvasPrintVisualProvider : IPrintVisualProvider
    {
        private readonly Canvas _canvas;
        public CanvasPrintVisualProvider(Canvas canvas) => _canvas = canvas;
        public Visual? GetPrintVisual() => _canvas;
    }
}
