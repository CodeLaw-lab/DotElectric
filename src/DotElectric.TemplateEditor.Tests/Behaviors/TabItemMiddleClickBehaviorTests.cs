using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Messaging;
using DotElectric.TemplateEditor.Behaviors;
using DotElectric.TemplateEditor.Messages;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Services;
using DotElectric.TemplateEditor.Tests.Helpers;
using DotElectric.TemplateEditor.ViewModels;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Behaviors;

public class TabItemMiddleClickBehaviorTests
{
    // ===== DP get/set (no STA needed) =====

    [Fact]
    public void SetEnableMiddleClickToClose_OnDependencyObject_SetsValue()
    {
        var obj = new DependencyObject();
        TabItemMiddleClickBehavior.SetEnableMiddleClickToClose(obj, true);
        var result = TabItemMiddleClickBehavior.GetEnableMiddleClickToClose(obj);
        Assert.True(result);
    }

    [Fact]
    public void GetEnableMiddleClickToClose_Default_ReturnsFalse()
    {
        var obj = new DependencyObject();
        var result = TabItemMiddleClickBehavior.GetEnableMiddleClickToClose(obj);
        Assert.False(result);
    }

    [Fact]
    public void SetEnableMiddleClickToClose_ToFalse_ClearsValue()
    {
        var obj = new DependencyObject();
        TabItemMiddleClickBehavior.SetEnableMiddleClickToClose(obj, true);
        TabItemMiddleClickBehavior.SetEnableMiddleClickToClose(obj, false);
        Assert.False(TabItemMiddleClickBehavior.GetEnableMiddleClickToClose(obj));
    }

    [Fact]
    public void OnEnableMiddleClickToCloseChanged_NonTabControl_DoesNotThrow()
    {
        var obj = new DependencyObject();
        var args = new DependencyPropertyChangedEventArgs(
            TabItemMiddleClickBehavior.EnableMiddleClickToCloseProperty,
            false, true);

        var exception = Record.Exception(() =>
            TabItemMiddleClickBehavior.OnEnableMiddleClickToCloseChanged(obj, args));

        Assert.Null(exception);
    }

    // ===== OnPreviewMouseUp handler (needs STA) =====

    [Fact]
    public void OnPreviewMouseUp_MiddleButtonOnTabItem_SendsCloseTabMessage()
    {
        WpfContext.Execute(() =>
        {
            // Arrange
            var tabControl = new TabControl();
            var template = new Template();
            var mockService = new Mock<ITemplateService>();
            var mockPrintService = new Mock<IPrintService>();
            var editorVm = new EditorViewModel(template, mockService.Object,
                printService: mockPrintService.Object);
            var tabItem = new TabItem { DataContext = editorVm };
            tabControl.Items.Add(tabItem);

            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Middle)
            {
                RoutedEvent = Mouse.PreviewMouseUpEvent
            };
            SetOriginalSource(args, tabItem);

            CloseTabRequestMessage? receivedMessage = null;
            WeakReferenceMessenger.Default.Register<CloseTabRequestMessage>(this, (r, m) =>
            {
                receivedMessage = m;
            });

            // Act
            TabItemMiddleClickBehavior.OnPreviewMouseUp(tabControl, args);

            // Assert
            Assert.True(args.Handled);
            Assert.NotNull(receivedMessage);
            Assert.Same(editorVm, receivedMessage!.Tab);

            // Cleanup
            WeakReferenceMessenger.Default.Unregister<CloseTabRequestMessage>(this);
        });
    }

    [Fact]
    public void OnPreviewMouseUp_LeftButton_DoesNothing()
    {
        WpfContext.Execute(() =>
        {
            var tabControl = new TabControl();
            var editorVm = CreateEditorViewModel();
            var tabItem = new TabItem { DataContext = editorVm };
            tabControl.Items.Add(tabItem);

            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = Mouse.PreviewMouseUpEvent
            };
            SetOriginalSource(args, tabItem);

            TabItemMiddleClickBehavior.OnPreviewMouseUp(tabControl, args);

            Assert.False(args.Handled);
        });
    }

    [Fact]
    public void OnPreviewMouseUp_RightButton_DoesNothing()
    {
        WpfContext.Execute(() =>
        {
            var tabControl = new TabControl();
            var editorVm = CreateEditorViewModel();
            var tabItem = new TabItem { DataContext = editorVm };
            tabControl.Items.Add(tabItem);

            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right)
            {
                RoutedEvent = Mouse.PreviewMouseUpEvent
            };
            SetOriginalSource(args, tabItem);

            TabItemMiddleClickBehavior.OnPreviewMouseUp(tabControl, args);

            Assert.False(args.Handled);
        });
    }

    [Fact]
    public void OnPreviewMouseUp_NonTabItemOriginalSource_DoesNothing()
    {
        WpfContext.Execute(() =>
        {
            var tabControl = new TabControl();
            var editorVm = CreateEditorViewModel();
            var tabItem = new TabItem { DataContext = editorVm };
            tabControl.Items.Add(tabItem);

            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Middle)
            {
                RoutedEvent = Mouse.PreviewMouseUpEvent
            };
            SetOriginalSource(args, new Button());

            TabItemMiddleClickBehavior.OnPreviewMouseUp(tabControl, args);

            Assert.False(args.Handled);
        });
    }

    [Fact]
    public void OnPreviewMouseUp_TabItemWithoutEditorViewModel_DoesNothing()
    {
        WpfContext.Execute(() =>
        {
            var tabControl = new TabControl();
            var tabItem = new TabItem { DataContext = "not an EditorViewModel" };
            tabControl.Items.Add(tabItem);

            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Middle)
            {
                RoutedEvent = Mouse.PreviewMouseUpEvent
            };
            SetOriginalSource(args, tabItem);

            TabItemMiddleClickBehavior.OnPreviewMouseUp(tabControl, args);

            Assert.False(args.Handled);
        });
    }

    [Fact]
    public void OnPreviewMouseUp_NonTabControlSender_DoesNothing()
    {
        WpfContext.Execute(() =>
        {
            var notTabControl = new Button();
            var args = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Middle)
            {
                RoutedEvent = Mouse.PreviewMouseUpEvent
            };

            TabItemMiddleClickBehavior.OnPreviewMouseUp(notTabControl, args);

            Assert.False(args.Handled);
        });
    }

    // ===== Event subscription tests (needs STA) =====

    [Fact]
    public void OnEnableMiddleClickToCloseChanged_TrueOnTabControl_SubscribesToEvent()
    {
        WpfContext.Execute(() =>
        {
            var tabControl = new TabControl();
            var template = new Template();
            var mockService = new Mock<ITemplateService>();
            var mockPrintService = new Mock<IPrintService>();
            var editorVm = new EditorViewModel(template, mockService.Object,
                printService: mockPrintService.Object);
            var tabItem = new TabItem { DataContext = editorVm };
            tabControl.Items.Add(tabItem);

            // Enable the behavior
            var args = new DependencyPropertyChangedEventArgs(
                TabItemMiddleClickBehavior.EnableMiddleClickToCloseProperty,
                false, true);
            TabItemMiddleClickBehavior.OnEnableMiddleClickToCloseChanged(tabControl, args);

            // Raise the event through the TabControl (simulating middle-click)
            var mouseArgs = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Middle)
            {
                RoutedEvent = Mouse.PreviewMouseUpEvent
            };
            SetOriginalSource(mouseArgs, tabItem);

            CloseTabRequestMessage? receivedMessage = null;
            WeakReferenceMessenger.Default.Register<CloseTabRequestMessage>(this, (r, m) =>
            {
                receivedMessage = m;
            });

            tabControl.RaiseEvent(mouseArgs);

            Assert.NotNull(receivedMessage);
            Assert.Same(editorVm, receivedMessage!.Tab);

            WeakReferenceMessenger.Default.Unregister<CloseTabRequestMessage>(this);
        });
    }

    [Fact]
    public void OnEnableMiddleClickToCloseChanged_FalseOnTabControl_UnsubscribesFromEvent()
    {
        WpfContext.Execute(() =>
        {
            var tabControl = new TabControl();
            var editorVm = CreateEditorViewModel();
            var tabItem = new TabItem { DataContext = editorVm };
            tabControl.Items.Add(tabItem);

            // First enable
            var enableArgs = new DependencyPropertyChangedEventArgs(
                TabItemMiddleClickBehavior.EnableMiddleClickToCloseProperty,
                false, true);
            TabItemMiddleClickBehavior.OnEnableMiddleClickToCloseChanged(tabControl, enableArgs);

            // Then disable
            var disableArgs = new DependencyPropertyChangedEventArgs(
                TabItemMiddleClickBehavior.EnableMiddleClickToCloseProperty,
                true, false);
            TabItemMiddleClickBehavior.OnEnableMiddleClickToCloseChanged(tabControl, disableArgs);

            // Raise event - should NOT trigger since unsubscribed
            var mouseArgs = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Middle)
            {
                RoutedEvent = Mouse.PreviewMouseUpEvent
            };
            SetOriginalSource(mouseArgs, tabItem);

            bool messageReceived = false;
            WeakReferenceMessenger.Default.Register<CloseTabRequestMessage>(this, (r, m) =>
            {
                messageReceived = true;
            });

            tabControl.RaiseEvent(mouseArgs);

            Assert.False(messageReceived);

            WeakReferenceMessenger.Default.Unregister<CloseTabRequestMessage>(this);
        });
    }

    // ===== Helpers =====

    private static void SetOriginalSource(RoutedEventArgs args, object originalSource)
    {
        // OriginalSource has internal set; use reflection in tests
        var prop = typeof(RoutedEventArgs).GetProperty("OriginalSource",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (prop?.CanWrite == true)
            prop.SetValue(args, originalSource);
        else
        {
            var field = typeof(RoutedEventArgs).GetField("_originalSource",
                BindingFlags.Instance | BindingFlags.NonPublic);
            field?.SetValue(args, originalSource);
        }
    }

    private static EditorViewModel CreateEditorViewModel()
    {
        var template = new Template();
        var mockService = new Mock<ITemplateService>();
        var mockPrintService = new Mock<IPrintService>();
        return new EditorViewModel(template, mockService.Object, printService: mockPrintService.Object);
    }
}
