using System.Collections.ObjectModel;
using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Tests.ViewModels;

public class PropertiesViewModelCommandTests
{
    private static ObservableCollection<TemplateObjectBase> CreateCollection()
    {
        return new ObservableCollection<TemplateObjectBase>();
    }

    private static CommandHistory CreateCommandHistory()
    {
        return new CommandHistory(maxLevels: 50);
    }

    // === ChangeStartXCommand (Line) ===

    [Fact]
    public void ChangeLineStartXCommand_UpdatesLineStartX()
    {
        var collection = CreateCollection();
        var line = new Line(1000, 2000, 3000, 4000);
        collection.Add(line);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.LineVM.ChangeStartXCommand.Execute(5000);

        Assert.Equal(5000, line.StartMicronsX);
        Assert.Equal(1, cmdHistory.UndoCount);
    }

    [Fact]
    public void ChangeLineStartXCommand_Undo_RestoresOriginalValue()
    {
        var collection = CreateCollection();
        var line = new Line(1000, 2000, 3000, 4000);
        collection.Add(line);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.LineVM.ChangeStartXCommand.Execute(5000);
        cmdHistory.Undo();

        Assert.Equal(1000, line.StartMicronsX);
    }

    [Fact]
    public void ChangeLineStartXCommand_DoesNothingWhenNoSelection()
    {
        var collection = CreateCollection();
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.LineVM.ChangeStartXCommand.Execute(5000);

        Assert.Equal(0, cmdHistory.UndoCount);
    }

    [Fact]
    public void ChangeLineStartXCommand_SetsValidationErrorForNegativeCoordinate()
    {
        var collection = CreateCollection();
        var line = new Line(1000, 2000, 3000, 4000);
        collection.Add(line);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.LineVM.ChangeStartXCommand.Execute(-100);

        Assert.NotNull(vm.ValidationError);
        Assert.Equal(0, cmdHistory.UndoCount);
    }

    // === ChangeLineTypeCommand (Line) ===

    [Fact]
    public void ChangeLineTypeCommand_UpdatesLineType()
    {
        var collection = CreateCollection();
        var line = new Line(1000, 2000, 3000, 4000, LineType.Solid);
        collection.Add(line);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.LineVM.ChangeLineTypeCommand.Execute(LineType.Dashed);

        Assert.Equal(LineType.Dashed, line.LineType);
        Assert.Equal(1, cmdHistory.UndoCount);
    }

    [Fact]
    public void ChangeLineTypeCommand_Undo_RestoresOriginalType()
    {
        var collection = CreateCollection();
        var line = new Line(1000, 2000, 3000, 4000, LineType.Solid);
        collection.Add(line);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.LineVM.ChangeLineTypeCommand.Execute(LineType.Dashed);
        cmdHistory.Undo();

        Assert.Equal(LineType.Solid, line.LineType);
    }

    // === ChangeWidthCommand (Rectangle) ===

    [Fact]
    public void ChangeRectWidthCommand_UpdatesWidth()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        collection.Add(rect);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.RectVM.ChangeWidthCommand.Execute(8000);

        Assert.Equal(8000, rect.WidthMicrons);
        Assert.Equal(1, cmdHistory.UndoCount);
    }

    [Fact]
    public void ChangeRectWidthCommand_Undo_RestoresOriginalWidth()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        collection.Add(rect);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.RectVM.ChangeWidthCommand.Execute(8000);
        cmdHistory.Undo();

        Assert.Equal(5000, rect.WidthMicrons);
    }

    [Fact]
    public void ChangeRectWidthCommand_SetsValidationErrorForTooSmallWidth()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        collection.Add(rect);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.RectVM.ChangeWidthCommand.Execute(300);

        Assert.NotNull(vm.ValidationError);
        Assert.Equal(0, cmdHistory.UndoCount);
    }

    // === ChangeContentCommand (Text) ===

    [Fact]
    public void ChangeTextContentCommand_UpdatesContent()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Hello", 3500);
        collection.Add(text);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.TextVM.ChangeContentCommand.Execute("World");

        Assert.Equal("World", text.Content);
        Assert.Equal(1, cmdHistory.UndoCount);
    }

    [Fact]
    public void ChangeTextContentCommand_Undo_RestoresOriginalContent()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Hello", 3500);
        collection.Add(text);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.TextVM.ChangeContentCommand.Execute("World");
        cmdHistory.Undo();

        Assert.Equal("Hello", text.Content);
    }

    [Fact]
    public void ChangeTextContentCommand_SetsValidationErrorForEmptyContent()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Hello", 3500);
        collection.Add(text);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.TextVM.ChangeContentCommand.Execute("");

        Assert.NotNull(vm.ValidationError);
        Assert.Equal(0, cmdHistory.UndoCount);
    }

    // === ChangeRotationCommand (Text) ===

    [Fact]
    public void ChangeTextRotationCommand_UpdatesRotation()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Test", 3500, rotationAngle: 0);
        collection.Add(text);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.TextVM.ChangeRotationCommand.Execute(90);

        Assert.Equal(90, text.RotationAngle);
        Assert.Equal(1, cmdHistory.UndoCount);
    }

    [Fact]
    public void ChangeTextRotationCommand_Undo_RestoresOriginalRotation()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Test", 3500, rotationAngle: 0);
        collection.Add(text);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.TextVM.ChangeRotationCommand.Execute(90);
        cmdHistory.Undo();

        Assert.Equal(0, text.RotationAngle);
    }

    [Fact]
    public void ChangeTextRotationCommand_AcceptsAnyAngle()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Test", 3500, rotationAngle: 0);
        collection.Add(text);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.TextVM.ChangeRotationCommand.Execute(45);

        Assert.Null(vm.ValidationError);
        Assert.Equal(1, cmdHistory.UndoCount);
        Assert.Equal(45, text.RotationAngle);
    }

    // === ChangeTextTypeCommand ===

    [Fact]
    public void ChangeTextTypeCommand_UpdatesTextType()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Test", 3500, textType: TextType.Text);
        collection.Add(text);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.TextVM.ChangeTextTypeCommand.Execute(TextType.Dimension);

        Assert.Equal(TextType.Dimension, text.TextType);
        Assert.Equal(1, cmdHistory.UndoCount);
    }

    [Fact]
    public void ChangeTextTypeCommand_Undo_RestoresOriginalType()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Test", 3500, textType: TextType.Text);
        collection.Add(text);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.TextVM.ChangeTextTypeCommand.Execute(TextType.Dimension);
        cmdHistory.Undo();

        Assert.Equal(TextType.Text, text.TextType);
    }

    // === MarkDirty callback ===

    [Fact]
    public void Commands_CallMarkDirtyCallback()
    {
        var collection = CreateCollection();
        var line = new Line(1000, 2000, 3000, 4000);
        collection.Add(line);
        var cmdHistory = CreateCommandHistory();
        bool dirtyCalled = false;
        var vm = new PropertiesViewModel(collection, cmdHistory, () => dirtyCalled = true);

        vm.LineVM.ChangeStartXCommand.Execute(5000);

        Assert.True(dirtyCalled);
    }

    // === ValidationError cleared on selection change ===

    [Fact]
    public void ValidationError_ClearedOnSelectionChange()
    {
        var collection = CreateCollection();
        var line = new Line(1000, 2000, 3000, 4000);
        collection.Add(line);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.LineVM.ChangeStartXCommand.Execute(-100);
        Assert.NotNull(vm.ValidationError);

        collection.Clear();
        var line2 = new Line(5000, 6000, 7000, 8000);
        collection.Add(line2);
        vm.Refresh();

        Assert.Null(vm.ValidationError);
    }
}
