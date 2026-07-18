using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DotElectric.TemplateEditor.Behaviors;
using DotElectric.TemplateEditor.Tests.Helpers;
using Moq;

namespace DotElectric.TemplateEditor.Tests.Behaviors;

public class ComboBoxSelectionChangedCommandBehaviorTests
{
    // ===== DP get/set (no STA needed) =====

    [Fact]
    public void SetSelectionChangedCommand_OnDependencyObject_SetsValue()
    {
        var obj = new DependencyObject();
        var command = Mock.Of<ICommand>();

        ComboBoxSelectionChangedCommandBehavior.SetSelectionChangedCommand(obj, command);
        var result = ComboBoxSelectionChangedCommandBehavior.GetSelectionChangedCommand(obj);

        Assert.Same(command, result);
    }

    [Fact]
    public void GetSelectionChangedCommand_Default_ReturnsNull()
    {
        var obj = new DependencyObject();
        var result = ComboBoxSelectionChangedCommandBehavior.GetSelectionChangedCommand(obj);
        Assert.Null(result);
    }

    [Fact]
    public void SetSelectionChangedCommand_ToNull_ClearsValue()
    {
        var obj = new DependencyObject();
        ComboBoxSelectionChangedCommandBehavior.SetSelectionChangedCommand(obj, Mock.Of<ICommand>());
        ComboBoxSelectionChangedCommandBehavior.SetSelectionChangedCommand(obj, null);
        Assert.Null(ComboBoxSelectionChangedCommandBehavior.GetSelectionChangedCommand(obj));
    }

    // ===== OnSelectionChanged (needs STA for ComboBox) =====

    [Fact]
    public void OnSelectionChanged_ExecutesCommandWithItemContent()
    {
        WpfContext.Execute(() =>
        {
            var comboBox = new ComboBox();
            var item = new ComboBoxItem { Content = "Solid" };
            comboBox.Items.Add(item);
            comboBox.SelectedItem = item;

            var command = new Mock<ICommand>();
            command.Setup(c => c.CanExecute("Solid")).Returns(true);
            ComboBoxSelectionChangedCommandBehavior.SetSelectionChangedCommand(comboBox, command.Object);

            ComboBoxSelectionChangedCommandBehavior.OnSelectionChanged(
                comboBox,
                new SelectionChangedEventArgs(Selector.SelectionChangedEvent, new List<object>(), new List<object> { item }));

            command.Verify(c => c.Execute("Solid"), Times.Once);
        });
    }

    [Fact]
    public void OnSelectionChanged_NullCommand_DoesNothing()
    {
        WpfContext.Execute(() =>
        {
            var comboBox = new ComboBox();
            var item = new ComboBoxItem { Content = "Test" };
            comboBox.Items.Add(item);
            comboBox.SelectedItem = item;

            ComboBoxSelectionChangedCommandBehavior.SetSelectionChangedCommand(comboBox, null);

            ComboBoxSelectionChangedCommandBehavior.OnSelectionChanged(
                comboBox,
                new SelectionChangedEventArgs(Selector.SelectionChangedEvent, new List<object>(), new List<object> { item }));
        });
    }

    [Fact]
    public void OnSelectionChanged_CanExecuteFalse_DoesNotExecute()
    {
        WpfContext.Execute(() =>
        {
            var comboBox = new ComboBox();
            var item = new ComboBoxItem { Content = "No" };
            comboBox.Items.Add(item);
            comboBox.SelectedItem = item;

            var command = new Mock<ICommand>();
            command.Setup(c => c.CanExecute("No")).Returns(false);
            ComboBoxSelectionChangedCommandBehavior.SetSelectionChangedCommand(comboBox, command.Object);

            ComboBoxSelectionChangedCommandBehavior.OnSelectionChanged(
                comboBox,
                new SelectionChangedEventArgs(Selector.SelectionChangedEvent, new List<object>(), new List<object> { item }));

            command.Verify(c => c.Execute(It.IsAny<object?>()), Times.Never);
        });
    }

    [Fact]
    public void OnSelectionChanged_NonComboBoxSender_DoesNothing()
    {
        WpfContext.Execute(() =>
        {
            var commandMock = new Mock<ICommand>(MockBehavior.Loose);
            commandMock.Setup(c => c.CanExecute(It.IsAny<object?>())).Returns(true);

            ComboBoxSelectionChangedCommandBehavior.OnSelectionChanged(
                new TextBox(),
                new SelectionChangedEventArgs(Selector.SelectionChangedEvent, Array.Empty<object>(), Array.Empty<object>()));

            commandMock.Verify(c => c.Execute(It.IsAny<object?>()), Times.Never);
        });
    }

    [Fact]
    public void OnSelectionChanged_NullSelectedItem_ExecutesWithNull()
    {
        WpfContext.Execute(() =>
        {
            var comboBox = new ComboBox();
            comboBox.Items.Add(new ComboBoxItem { Content = "A" });

            var command = new Mock<ICommand>();
            command.Setup(c => c.CanExecute(It.Is<object?>(v => v == null))).Returns(true);
            ComboBoxSelectionChangedCommandBehavior.SetSelectionChangedCommand(comboBox, command.Object);

            ComboBoxSelectionChangedCommandBehavior.OnSelectionChanged(
                comboBox,
                new SelectionChangedEventArgs(Selector.SelectionChangedEvent, new List<object>(), new List<object>()));

            command.Verify(c => c.Execute(null), Times.Once);
        });
    }
}
