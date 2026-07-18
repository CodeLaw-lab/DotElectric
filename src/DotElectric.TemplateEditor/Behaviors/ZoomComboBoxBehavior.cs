using System.Windows;
using System.Windows.Controls;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Behaviors;

/// <summary>
/// Attached behavior для ComboBox зума.
/// Обрабатывает DropDownClosed и SelectionChanged для вызова SetZoomPercent.
/// </summary>
public static class ZoomComboBoxBehavior
{
    public static readonly DependencyProperty EditorProperty =
        DependencyProperty.RegisterAttached(
            "Editor",
            typeof(EditorViewModel),
            typeof(ZoomComboBoxBehavior),
            new PropertyMetadata(null, OnEditorChanged));

    public static EditorViewModel? GetEditor(ComboBox comboBox) =>
        (EditorViewModel?)comboBox.GetValue(EditorProperty);

    public static void SetEditor(ComboBox comboBox, EditorViewModel? value) =>
        comboBox.SetValue(EditorProperty, value);

    private static void OnEditorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ComboBox comboBox) return;

        comboBox.SelectionChanged -= OnSelectionChanged;
        comboBox.DropDownClosed -= OnDropDownClosed;
        
        if (e.NewValue is not null)
        {
            comboBox.SelectionChanged += OnSelectionChanged;
            comboBox.DropDownClosed += OnDropDownClosed;
        }
    }

    internal static void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ComboBox comboBox) return;
        ApplyZoom(comboBox);
    }

    internal static void OnDropDownClosed(object? sender, EventArgs e)
    {
        if (sender is not ComboBox comboBox) return;
        ApplyZoom(comboBox);
    }

    internal static void ApplyZoom(ComboBox comboBox)
    {
        if (GetEditor(comboBox) is not { } editor) return;

        var text = comboBox.Text.Replace("%", "").Trim();
        if (int.TryParse(text, out var percent) && percent > 0)
        {
            editor.SetZoomPercent(percent);
        }
    }
}
