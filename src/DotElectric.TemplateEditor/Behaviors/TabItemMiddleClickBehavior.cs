using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CommunityToolkit.Mvvm.Messaging;
using DotElectric.TemplateEditor.Messages;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Behaviors;

/// <summary>
/// Attached behavior для закрытия вкладок TabControl средней кнопкой мыши.
/// При клике колёсиком на заголовок вкладки — закрывает её.
/// </summary>
public static class TabItemMiddleClickBehavior
{
    public static readonly DependencyProperty EnableMiddleClickToCloseProperty =
        DependencyProperty.RegisterAttached(
            "EnableMiddleClickToClose",
            typeof(bool),
            typeof(TabItemMiddleClickBehavior),
            new PropertyMetadata(false, OnEnableMiddleClickToCloseChanged));

    public static bool GetEnableMiddleClickToClose(DependencyObject obj)
    {
        return (bool)obj.GetValue(EnableMiddleClickToCloseProperty);
    }

    public static void SetEnableMiddleClickToClose(DependencyObject obj, bool value)
    {
        obj.SetValue(EnableMiddleClickToCloseProperty, value);
    }

    internal static void OnEnableMiddleClickToCloseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TabControl tabControl)
        {
            if ((bool)e.NewValue)
            {
                tabControl.PreviewMouseUp += OnPreviewMouseUp;
            }
            else
            {
                tabControl.PreviewMouseUp -= OnPreviewMouseUp;
            }
        }
    }

    internal static void OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not TabControl tabControl) return;
        
        // Обрабатываем ТОЛЬКО среднюю кнопку мыши
        if (e.ChangedButton != MouseButton.Middle) return;
        if (e.MiddleButton != MouseButtonState.Released) return;

        // Ищем TabItem, на который кликнули
        var dependencyObject = e.OriginalSource as DependencyObject;
        while (dependencyObject != null)
        {
            if (dependencyObject is TabItem tabItem && tabItem.DataContext is EditorViewModel editorVm)
            {
                // Нашли TabItem — закрываем через WeakReferenceMessenger
                WeakReferenceMessenger.Default.Send(new CloseTabRequestMessage(editorVm));
                e.Handled = true;
                return;
            }

            dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
        }
    }
}
