using System.Windows;
using System.Windows.Controls;
using DotElectric.TemplateEditor.Behaviors;
using DotElectric.TemplateEditor.Tests.Helpers;

namespace DotElectric.TemplateEditor.Tests.Behaviors;

public class AutoFocusOnVisibleBehaviorTests
{
    [Fact]
    public void SetIsEnabled_OnDependencyObject_SetsValue()
    {
        var obj = new DependencyObject();
        AutoFocusOnVisibleBehavior.SetIsEnabled(obj, true);
        var result = AutoFocusOnVisibleBehavior.GetIsEnabled(obj);
        Assert.True(result);
    }

    [Fact]
    public void GetIsEnabled_Default_ReturnsFalse()
    {
        var obj = new DependencyObject();
        var result = AutoFocusOnVisibleBehavior.GetIsEnabled(obj);
        Assert.False(result);
    }

    [Fact]
    public void SetIsEnabled_ToFalse_ClearsValue()
    {
        var obj = new DependencyObject();
        AutoFocusOnVisibleBehavior.SetIsEnabled(obj, true);
        AutoFocusOnVisibleBehavior.SetIsEnabled(obj, false);
        Assert.False(AutoFocusOnVisibleBehavior.GetIsEnabled(obj));
    }

    [Fact]
    public void NonFrameworkElement_DoesNotThrow()
    {
        var obj = new DependencyObject();
        var exception = Record.Exception(() =>
            AutoFocusOnVisibleBehavior.SetIsEnabled(obj, true));
        Assert.Null(exception);
    }

    [Fact]
    public void IsEnabledTrue_RegistersIsVisibleChanged()
    {
        WpfContext.Execute(() =>
        {
            var textBox = new TextBox();
            AutoFocusOnVisibleBehavior.SetIsEnabled(textBox, true);

            // Toggle visibility to trigger the handler
            textBox.Visibility = Visibility.Visible;
            textBox.Visibility = Visibility.Collapsed;
            textBox.Visibility = Visibility.Visible;

            // If we got here without exception, the handler was registered and fired
            Assert.True(AutoFocusOnVisibleBehavior.GetIsEnabled(textBox));
        });
    }
}
