using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
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
    public void VisibleTextBox_BecomesFocused()
    {
        WpfContext.Execute(() =>
        {
            var window = new Window { Width = 200, Height = 100 };
            var textBox = new TextBox();
            window.Content = textBox;
            AutoFocusOnVisibleBehavior.SetIsEnabled(textBox, true);

            try
            {
                window.Show();
                window.Activate();
                window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);

                // Headless CI: focus может не успеть установиться за один pump —
                // повторяем Activate + pump один раз, затем проверяем.
                if (!textBox.IsFocused)
                {
                    window.Activate();
                    window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
                }

                Assert.True(textBox.IsFocused);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void VisibleTextBox_SelectsAllText()
    {
        WpfContext.Execute(() =>
        {
            var window = new Window { Width = 200, Height = 100 };
            var textBox = new TextBox { Text = "Some text" };
            window.Content = textBox;
            AutoFocusOnVisibleBehavior.SetIsEnabled(textBox, true);

            try
            {
                window.Show();
                window.Activate();
                window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);

                Assert.Equal(9, textBox.SelectionLength);
                Assert.Equal("Some text", textBox.SelectedText);
            }
            finally
            {
                window.Close();
            }
        });
    }
}
