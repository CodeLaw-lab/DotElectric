using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using DotElectric.TemplateEditor.Behaviors;
using DotElectric.TemplateEditor.Tests.Helpers;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Behaviors;

public class TextBoxLostFocusCommandBehaviorTests
{
    // ===== DP get/set (no STA needed) =====

    [Fact]
    public void SetLostFocusCommand_OnDependencyObject_SetsValue()
    {
        var obj = new DependencyObject();
        var command = Mock.Of<ICommand>();

        TextBoxLostFocusCommandBehavior.SetLostFocusCommand(obj, command);
        var result = TextBoxLostFocusCommandBehavior.GetLostFocusCommand(obj);

        Assert.Same(command, result);
    }

    [Fact]
    public void GetLostFocusCommand_Default_ReturnsNull()
    {
        var obj = new DependencyObject();
        var result = TextBoxLostFocusCommandBehavior.GetLostFocusCommand(obj);
        Assert.Null(result);
    }

    [Fact]
    public void SetLostFocusCommand_ToNull_ClearsValue()
    {
        var obj = new DependencyObject();
        TextBoxLostFocusCommandBehavior.SetLostFocusCommand(obj, Mock.Of<ICommand>());
        TextBoxLostFocusCommandBehavior.SetLostFocusCommand(obj, null);
        Assert.Null(TextBoxLostFocusCommandBehavior.GetLostFocusCommand(obj));
    }

    // ===== OnLostFocus (needs STA for TextBox) =====

    [Fact]
    public void OnLostFocus_ExecutesCommandWithTextBoxText()
    {
        WpfContext.Execute(() =>
        {
            var textBox = new TextBox { Text = "42.5" };
            var command = new Mock<ICommand>();
            command.Setup(c => c.CanExecute("42.5")).Returns(true);
            TextBoxLostFocusCommandBehavior.SetLostFocusCommand(textBox, command.Object);

            TextBoxLostFocusCommandBehavior.OnLostFocus(textBox, new RoutedEventArgs(TextBox.LostFocusEvent));

            command.Verify(c => c.Execute("42.5"), Times.Once);
        });
    }

    [Fact]
    public void OnLostFocus_CanExecuteFalse_DoesNotExecute()
    {
        WpfContext.Execute(() =>
        {
            var textBox = new TextBox { Text = "bad" };
            var command = new Mock<ICommand>();
            command.Setup(c => c.CanExecute("bad")).Returns(false);
            TextBoxLostFocusCommandBehavior.SetLostFocusCommand(textBox, command.Object);

            TextBoxLostFocusCommandBehavior.OnLostFocus(textBox, new RoutedEventArgs(TextBox.LostFocusEvent));

            command.Verify(c => c.Execute(It.IsAny<object?>()), Times.Never);
        });
    }

    [Fact]
    public void OnLostFocus_NullCommand_DoesNothing()
    {
        WpfContext.Execute(() =>
        {
            var textBox = new TextBox { Text = "test" };
            TextBoxLostFocusCommandBehavior.SetLostFocusCommand(textBox, null);

            TextBoxLostFocusCommandBehavior.OnLostFocus(textBox, new RoutedEventArgs(TextBox.LostFocusEvent));
        });
    }

    [Fact]
    public void OnLostFocus_NonTextBoxSender_DoesNothing()
    {
        WpfContext.Execute(() =>
        {
            var commandMock = new Mock<ICommand>(MockBehavior.Loose);
            commandMock.Setup(c => c.CanExecute(It.IsAny<object?>())).Returns(true);

            TextBoxLostFocusCommandBehavior.OnLostFocus(new Button(), new RoutedEventArgs(TextBox.LostFocusEvent));

            commandMock.Verify(c => c.Execute(It.IsAny<object?>()), Times.Never);
        });
    }

    // ===== OnKeyDown Enter (needs STA for TextBox) =====

    [Fact]
    public void OnKeyDown_Enter_ExecutesCommand()
    {
        WpfContext.Execute(() =>
        {
            var textBox = new TextBox { Text = "value" };
            var command = new Mock<ICommand>();
            command.Setup(c => c.CanExecute("value")).Returns(true);
            TextBoxLostFocusCommandBehavior.SetLostFocusCommand(textBox, command.Object);

            var args = CreateKeyEventArgs(Key.Enter);
            TextBoxLostFocusCommandBehavior.OnKeyDown(textBox, args);

            command.Verify(c => c.Execute("value"), Times.Once);
        });
    }

    [Fact]
    public void OnKeyDown_Enter_SetsHandled()
    {
        WpfContext.Execute(() =>
        {
            var textBox = new TextBox { Text = "v" };
            var command = new Mock<ICommand>();
            command.Setup(c => c.CanExecute("v")).Returns(true);
            TextBoxLostFocusCommandBehavior.SetLostFocusCommand(textBox, command.Object);

            var args = CreateKeyEventArgs(Key.Enter);
            TextBoxLostFocusCommandBehavior.OnKeyDown(textBox, args);

            Assert.True(args.Handled);
        });
    }

    [Fact]
    public void OnKeyDown_NonEnterKey_DoesNotExecute()
    {
        WpfContext.Execute(() =>
        {
            var textBox = new TextBox { Text = "value" };
            var command = new Mock<ICommand>();
            command.Setup(c => c.CanExecute("value")).Returns(true);
            TextBoxLostFocusCommandBehavior.SetLostFocusCommand(textBox, command.Object);

            var args = CreateKeyEventArgs(Key.Escape);
            TextBoxLostFocusCommandBehavior.OnKeyDown(textBox, args);

            command.Verify(c => c.Execute(It.IsAny<object?>()), Times.Never);
            Assert.False(args.Handled);
        });
    }

    [Fact]
    public void OnKeyDown_Enter_CanExecuteFalse_DoesNotExecute()
    {
        WpfContext.Execute(() =>
        {
            var textBox = new TextBox { Text = "bad" };
            var command = new Mock<ICommand>();
            command.Setup(c => c.CanExecute("bad")).Returns(false);
            TextBoxLostFocusCommandBehavior.SetLostFocusCommand(textBox, command.Object);

            var args = CreateKeyEventArgs(Key.Enter);
            TextBoxLostFocusCommandBehavior.OnKeyDown(textBox, args);

            command.Verify(c => c.Execute(It.IsAny<object?>()), Times.Never);
            Assert.False(args.Handled);
        });
    }

    [Fact]
    public void OnKeyDown_Enter_NullCommand_DoesNothing()
    {
        WpfContext.Execute(() =>
        {
            var textBox = new TextBox { Text = "v" };
            TextBoxLostFocusCommandBehavior.SetLostFocusCommand(textBox, null);

            var args = CreateKeyEventArgs(Key.Enter);
            TextBoxLostFocusCommandBehavior.OnKeyDown(textBox, args);

            Assert.False(args.Handled);
        });
    }

    [Fact]
    public void OnKeyDown_NonTextBoxSender_DoesNothing()
    {
        WpfContext.Execute(() =>
        {
            var commandMock = new Mock<ICommand>(MockBehavior.Loose);
            commandMock.Setup(c => c.CanExecute(It.IsAny<object?>())).Returns(true);

            var args = CreateKeyEventArgs(Key.Enter);
            TextBoxLostFocusCommandBehavior.OnKeyDown(new Button(), args);

            commandMock.Verify(c => c.Execute(It.IsAny<object?>()), Times.Never);
        });
    }

    private static KeyEventArgs CreateKeyEventArgs(Key key)
    {
        return new KeyEventArgs(
            Keyboard.PrimaryDevice,
            new FakePresentationSource(),
            0,
            key)
        {
            RoutedEvent = Keyboard.KeyDownEvent
        };
    }

    private class FakePresentationSource : PresentationSource
    {
        private Visual? _rootVisual;

        public override bool IsDisposed => false;
        public override Visual? RootVisual
        {
            get => _rootVisual;
            set => _rootVisual = value;
        }

        protected override CompositionTarget? GetCompositionTargetCore() => null;
    }
}
