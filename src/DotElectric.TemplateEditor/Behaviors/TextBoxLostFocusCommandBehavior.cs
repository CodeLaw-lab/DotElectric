using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DotElectric.TemplateEditor.Behaviors;

/// <summary>
/// Attached behavior для TextBox, который вызывает ICommand при LostFocus.
/// Передаёт текст из TextBox.Text как параметр команды.
/// </summary>
public static class TextBoxLostFocusCommandBehavior
{
    public static readonly DependencyProperty LostFocusCommandProperty =
        DependencyProperty.RegisterAttached(
            "LostFocusCommand",
            typeof(ICommand),
            typeof(TextBoxLostFocusCommandBehavior),
            new PropertyMetadata(null, OnLostFocusCommandChanged));

    public static void SetLostFocusCommand(DependencyObject obj, ICommand? value) =>
        obj.SetValue(LostFocusCommandProperty, value);

    public static ICommand GetLostFocusCommand(DependencyObject obj) =>
        (ICommand)obj.GetValue(LostFocusCommandProperty);

    private static void OnLostFocusCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox tb) return;

        if (e.NewValue is ICommand)
        {
            tb.LostFocus += OnLostFocus;
            tb.KeyDown += OnKeyDown;
        }
        else
        {
            tb.LostFocus -= OnLostFocus;
            tb.KeyDown -= OnKeyDown;
        }
    }

    internal static void OnLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox tb) return;
        var command = GetLostFocusCommand(tb);
        if (command == null || !command.CanExecute(tb.Text)) return;

        command.Execute(tb.Text);
    }

    internal static void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        if (sender is not TextBox tb) return;

        var command = GetLostFocusCommand(tb);
        if (command == null || !command.CanExecute(tb.Text)) return;

        command.Execute(tb.Text);
        e.Handled = true;
    }
}
