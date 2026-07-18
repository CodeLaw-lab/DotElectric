using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DotElectric.TemplateEditor.Behaviors;

/// <summary>
/// Attached behavior для ComboBox, который вызывает ICommand при SelectionChanged.
/// Передаёт текст выбранного элемента как параметр команды.
/// </summary>
public static class ComboBoxSelectionChangedCommandBehavior
{
    public static readonly DependencyProperty SelectionChangedCommandProperty =
        DependencyProperty.RegisterAttached(
            "SelectionChangedCommand",
            typeof(ICommand),
            typeof(ComboBoxSelectionChangedCommandBehavior),
            new PropertyMetadata(null, OnSelectionChangedCommandChanged));

    public static void SetSelectionChangedCommand(DependencyObject obj, ICommand? value) =>
        obj.SetValue(SelectionChangedCommandProperty, value);

    public static ICommand GetSelectionChangedCommand(DependencyObject obj) =>
        (ICommand)obj.GetValue(SelectionChangedCommandProperty);

    private static void OnSelectionChangedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ComboBox cb) return;

        if (e.NewValue is ICommand)
        {
            cb.SelectionChanged += OnSelectionChanged;
        }
        else
        {
            cb.SelectionChanged -= OnSelectionChanged;
        }
    }

    internal static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox cb) return;
        var command = GetSelectionChangedCommand(cb);
        if (command == null) return;

        var selectedText = (cb.SelectedItem as ComboBoxItem)?.Content?.ToString();
        if (command.CanExecute(selectedText))
        {
            command.Execute(selectedText);
        }
    }
}
