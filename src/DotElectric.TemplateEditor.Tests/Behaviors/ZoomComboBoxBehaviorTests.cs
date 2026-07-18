using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DotElectric.TemplateEditor.Behaviors;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tests.Helpers;
using DotElectric.TemplateEditor.ViewModels;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Behaviors;

public class ZoomComboBoxBehaviorTests
{
    private static EditorViewModel CreateEditorViewModel()
    {
        var template = new Template();
        var mockService = new Mock<ITemplateService>();
        var mockPrintService = new Mock<IPrintService>();
        return new EditorViewModel(template, mockService.Object, printService: mockPrintService.Object);
    }

    // ===== DP get/set — pure DependencyObject (no STA needed) =====

    [Fact]
    public void GetSetEditor_OnDependencyObject_StoresValue()
    {
        var obj = new DependencyObject();
        var editor = CreateEditorViewModel();

        obj.SetValue(ZoomComboBoxBehavior.EditorProperty, editor);
        var result = (EditorViewModel?)obj.GetValue(ZoomComboBoxBehavior.EditorProperty);

        Assert.Same(editor, result);
    }

    [Fact]
    public void GetEditor_Default_ReturnsNull()
    {
        var obj = new DependencyObject();
        var result = (EditorViewModel?)obj.GetValue(ZoomComboBoxBehavior.EditorProperty);
        Assert.Null(result);
    }

    [Fact]
    public void SetEditor_ToNull_ClearsValue()
    {
        var obj = new DependencyObject();
        var editor = CreateEditorViewModel();
        obj.SetValue(ZoomComboBoxBehavior.EditorProperty, editor);
        obj.SetValue(ZoomComboBoxBehavior.EditorProperty, null);
        Assert.Null(obj.GetValue(ZoomComboBoxBehavior.EditorProperty));
    }

    // ===== ComboBox creation (needs STA) =====

    [Fact]
    public void SetEditor_OnComboBox_SetsValue()
    {
        WpfContext.Execute(() =>
        {
            var comboBox = new ComboBox();
            var editor = CreateEditorViewModel();

            ZoomComboBoxBehavior.SetEditor(comboBox, editor);
            var result = ZoomComboBoxBehavior.GetEditor(comboBox);

            Assert.Same(editor, result);
        });
    }

    // ===== ApplyZoom (needs STA for ComboBox) =====

    [Fact]
    public void ApplyZoom_PercentText_CallsSetZoomPercent()
    {
        WpfContext.Execute(() =>
        {
            var comboBox = new ComboBox { Text = "150%" };
            var editor = CreateEditorViewModel();
            ZoomComboBoxBehavior.SetEditor(comboBox, editor);

            ZoomComboBoxBehavior.ApplyZoom(comboBox);

            Assert.Equal(1.5, editor.Zoom, 4);
        });
    }

    [Fact]
    public void ApplyZoom_PlainNumber_CallsSetZoomPercent()
    {
        WpfContext.Execute(() =>
        {
            var comboBox = new ComboBox { Text = "75" };
            var editor = CreateEditorViewModel();
            ZoomComboBoxBehavior.SetEditor(comboBox, editor);

            ZoomComboBoxBehavior.ApplyZoom(comboBox);

            Assert.Equal(0.75, editor.Zoom, 4);
        });
    }

    [Fact]
    public void ApplyZoom_InvalidText_DoesNothing()
    {
        WpfContext.Execute(() =>
        {
            var comboBox = new ComboBox { Text = "abc" };
            var editor = CreateEditorViewModel();
            ZoomComboBoxBehavior.SetEditor(comboBox, editor);
            var initialZoom = editor.Zoom;

            ZoomComboBoxBehavior.ApplyZoom(comboBox);

            Assert.Equal(initialZoom, editor.Zoom);
        });
    }

    [Fact]
    public void ApplyZoom_ZeroOrNegative_DoesNothing()
    {
        WpfContext.Execute(() =>
        {
            var comboBox = new ComboBox { Text = "0" };
            var editor = CreateEditorViewModel();
            ZoomComboBoxBehavior.SetEditor(comboBox, editor);
            var initialZoom = editor.Zoom;

            ZoomComboBoxBehavior.ApplyZoom(comboBox);

            Assert.Equal(initialZoom, editor.Zoom);
        });
    }

    [Fact]
    public void ApplyZoom_NoEditor_DoesNothing()
    {
        WpfContext.Execute(() =>
        {
            var comboBox = new ComboBox { Text = "100%" };
            ZoomComboBoxBehavior.SetEditor(comboBox, null);

            ZoomComboBoxBehavior.ApplyZoom(comboBox);
        });
    }

    [Fact]
    public void ApplyZoom_TextWithSpaces_ParsesCorrectly()
    {
        WpfContext.Execute(() =>
        {
            var comboBox = new ComboBox { Text = "  200 % " };
            var editor = CreateEditorViewModel();
            ZoomComboBoxBehavior.SetEditor(comboBox, editor);

            ZoomComboBoxBehavior.ApplyZoom(comboBox);

            Assert.Equal(2.0, editor.Zoom, 4);
        });
    }

    // ===== OnSelectionChanged =====

    [Fact]
    public void OnSelectionChanged_CallsApplyZoom()
    {
        WpfContext.Execute(() =>
        {
            var comboBox = new ComboBox { Text = "100%" };
            var editor = CreateEditorViewModel();
            ZoomComboBoxBehavior.SetEditor(comboBox, editor);

            ZoomComboBoxBehavior.OnSelectionChanged(
                comboBox,
                new SelectionChangedEventArgs(Selector.SelectionChangedEvent, Array.Empty<object>(), Array.Empty<object>()));

            Assert.Equal(1.0, editor.Zoom, 4);
        });
    }

    // ===== OnDropDownClosed =====

    [Fact]
    public void OnDropDownClosed_CallsApplyZoom()
    {
        WpfContext.Execute(() =>
        {
            var comboBox = new ComboBox { Text = "50%" };
            var editor = CreateEditorViewModel();
            ZoomComboBoxBehavior.SetEditor(comboBox, editor);

            ZoomComboBoxBehavior.OnDropDownClosed(comboBox, EventArgs.Empty);

            Assert.Equal(0.5, editor.Zoom, 4);
        });
    }
}
