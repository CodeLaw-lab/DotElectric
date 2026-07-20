using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DotElectric.TemplateEditor.Behaviors;

/// <summary>
/// Attached behavior that auto-focuses a UI element when it becomes visible.
/// For TextBox elements, also selects all text.
/// </summary>
public static class AutoFocusOnVisibleBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled", typeof(bool), typeof(AutoFocusOnVisibleBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static void SetIsEnabled(DependencyObject obj, bool value) =>
        obj.SetValue(IsEnabledProperty, value);

    public static bool GetIsEnabled(DependencyObject obj) =>
        (bool)obj.GetValue(IsEnabledProperty);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
            return;

        var isEnabled = (bool)e.NewValue;
        if (isEnabled)
        {
            element.IsVisibleChanged += OnElementIsVisibleChanged;
        }
        else
        {
            element.IsVisibleChanged -= OnElementIsVisibleChanged;
        }
    }

    private static void OnElementIsVisibleChanged(object? sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not FrameworkElement element || !element.IsVisible)
            return;

        element.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, () =>
        {
            if (!element.IsVisible)
                return;

            element.Focus();

            if (element is TextBox textBox)
            {
                textBox.SelectAll();
            }
        });
    }
}
