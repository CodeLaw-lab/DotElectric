using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DotElectric.TemplateEditor.Converters;

namespace DotElectric.TemplateEditor.Behaviors;

public static class MarkerPosition
{
    public static readonly DependencyProperty XPropertyPathProperty =
        DependencyProperty.RegisterAttached(
            "XPropertyPath", typeof(string), typeof(MarkerPosition),
            new PropertyMetadata(null, OnPropertyPathChanged));

    public static readonly DependencyProperty YPropertyPathProperty =
        DependencyProperty.RegisterAttached(
            "YPropertyPath", typeof(string), typeof(MarkerPosition),
            new PropertyMetadata(null, OnPropertyPathChanged));

    public static void SetXPropertyPath(DependencyObject obj, string? value) =>
        obj.SetValue(XPropertyPathProperty, value);

    public static string? GetXPropertyPath(DependencyObject obj) =>
        (string?)obj.GetValue(XPropertyPathProperty);

    public static void SetYPropertyPath(DependencyObject obj, string? value) =>
        obj.SetValue(YPropertyPathProperty, value);

    public static string? GetYPropertyPath(DependencyObject obj) =>
        (string?)obj.GetValue(YPropertyPathProperty);

    private static void OnPropertyPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element) return;

        var xPath = GetXPropertyPath(element);
        var yPath = GetYPropertyPath(element);
        if (string.IsNullOrEmpty(xPath) || string.IsNullOrEmpty(yPath)) return;

        var userControlRelative = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(UserControl), 1);

        var leftMultiBinding = new MultiBinding
        {
            Converter = new ModelXToCanvasLeftConverter()
        };
        leftMultiBinding.Bindings.Add(new Binding(xPath));
        leftMultiBinding.Bindings.Add(new Binding("DataContext.ZoomPanManager.Zoom") { RelativeSource = userControlRelative });
        element.SetBinding(Canvas.LeftProperty, leftMultiBinding);

        var topMultiBinding = new MultiBinding
        {
            Converter = new ModelYToCanvasTopConverter()
        };
        topMultiBinding.Bindings.Add(new Binding(yPath));
        topMultiBinding.Bindings.Add(new Binding("DataContext.Template.Sheet.HeightMm") { RelativeSource = userControlRelative });
        topMultiBinding.Bindings.Add(new Binding("DataContext.ZoomPanManager.Zoom") { RelativeSource = userControlRelative });
        element.SetBinding(Canvas.TopProperty, topMultiBinding);
    }
}
