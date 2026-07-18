using System.Collections.ObjectModel;
using System.ComponentModel;
using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Tests.ViewModels;

public class PropertiesViewModelTests
{
    private static ObservableCollection<TemplateObjectBase> CreateCollection()
    {
        return new ObservableCollection<TemplateObjectBase>();
    }

    private static CommandHistory CreateCommandHistory()
    {
        return new CommandHistory(maxLevels: 50, markDirty: null);
    }

    // === Constructor ===

    [Fact]
    public void Constructor_EmptyCollection_SetsDefaultValues()
    {
        var collection = CreateCollection();
        var vm = new PropertiesViewModel(collection);

        Assert.Equal(0, vm.SelectionCount);
        Assert.False(vm.IsSingleSelection);
        Assert.Null(vm.SelectedObject);
    }

    [Fact]
    public void Constructor_SingleSelection_SetsCorrectProperties()
    {
        var collection = CreateCollection();
        var line = new Line(1000, 2000, 3000, 4000);
        collection.Add(line);

        var vm = new PropertiesViewModel(collection);

        Assert.Equal(1, vm.SelectionCount);
        Assert.True(vm.IsSingleSelection);
        Assert.Same(line, vm.SelectedObject);
    }

    [Fact]
    public void Constructor_MultiSelection_SetsCorrectProperties()
    {
        var collection = CreateCollection();
        collection.Add(new Line(0, 0, 1000, 1000));
        collection.Add(new Line(2000, 2000, 3000, 3000));

        var vm = new PropertiesViewModel(collection);

        Assert.Equal(2, vm.SelectionCount);
        Assert.False(vm.IsSingleSelection);
        Assert.Null(vm.SelectedObject);
    }

    // === Type detection ===

    [Fact]
    public void IsLineSelected_TrueForLine()
    {
        var collection = CreateCollection();
        collection.Add(new Line(0, 0, 1000, 1000));

        var vm = new PropertiesViewModel(collection);
        Assert.True(vm.IsLineSelected);
        Assert.False(vm.IsRectangleSelected);
        Assert.False(vm.IsTextSelected);
    }

    [Fact]
    public void IsRectangleSelected_TrueForRectangle()
    {
        var collection = CreateCollection();
        collection.Add(new Rectangle(0, 0, 1000, 1000));

        var vm = new PropertiesViewModel(collection);
        Assert.False(vm.IsLineSelected);
        Assert.True(vm.IsRectangleSelected);
        Assert.False(vm.IsTextSelected);
    }

    [Fact]
    public void IsTextSelected_TrueForText()
    {
        var collection = CreateCollection();
        collection.Add(new Text(0, 0, "Test", 3500));

        var vm = new PropertiesViewModel(collection);
        Assert.False(vm.IsLineSelected);
        Assert.False(vm.IsRectangleSelected);
        Assert.True(vm.IsTextSelected);
    }

    // === Line properties (sub-VM) ===

    [Fact]
    public void LineProperties_ReturnCorrectValues()
    {
        var collection = CreateCollection();
        var line = new Line(1000, 2000, 3000, 4000, LineType.Dashed);
        collection.Add(line);

        var vm = new PropertiesViewModel(collection);

        Assert.Equal(1000, vm.LineVM.StartX);
        Assert.Equal(2000, vm.LineVM.StartY);
        Assert.Equal(3000, vm.LineVM.EndX);
        Assert.Equal(4000, vm.LineVM.EndY);
        Assert.Equal(LineType.Dashed, vm.LineVM.LineTypeValue);
    }

    [Fact]
    public void LineProperties_NullForNonLine()
    {
        var collection = CreateCollection();
        collection.Add(new Rectangle(0, 0, 1000, 1000));

        var vm = new PropertiesViewModel(collection);

        Assert.Null(vm.LineVM.StartX);
        Assert.Null(vm.LineVM.StartY);
        Assert.Null(vm.LineVM.EndX);
        Assert.Null(vm.LineVM.EndY);
        Assert.Null(vm.LineVM.LineTypeValue);
    }

    // === Rectangle properties (sub-VM) ===

    [Fact]
    public void RectangleProperties_ReturnCorrectValues()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(1000, 2000, 5000, 3000, LineType.DashDot);
        collection.Add(rect);

        var vm = new PropertiesViewModel(collection);

        Assert.Equal(1000, vm.RectVM.X);
        Assert.Equal(2000, vm.RectVM.Y);
        Assert.Equal(5000, vm.RectVM.Width);
        Assert.Equal(3000, vm.RectVM.Height);
        Assert.Equal(LineType.DashDot, vm.RectVM.LineTypeValue);
    }

    [Fact]
    public void RectangleProperties_NullForNonRectangle()
    {
        var collection = CreateCollection();
        collection.Add(new Line(0, 0, 1000, 1000));

        var vm = new PropertiesViewModel(collection);

        Assert.Null(vm.RectVM.X);
        Assert.Null(vm.RectVM.Y);
        Assert.Null(vm.RectVM.Width);
        Assert.Null(vm.RectVM.Height);
        Assert.Null(vm.RectVM.LineTypeValue);
    }

    // === Text properties (sub-VM) ===

    [Fact]
    public void TextProperties_ReturnCorrectValues()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Hello", 5000, "ГОСТ А", TextType.Label, 90);
        collection.Add(text);

        var vm = new PropertiesViewModel(collection);

        Assert.Equal(1000, vm.TextVM.X);
        Assert.Equal(2000, vm.TextVM.Y);
        Assert.Equal("Hello", vm.TextVM.Content);
        Assert.Equal(5000, vm.TextVM.FontSize);
        Assert.Equal("ГОСТ А", vm.TextVM.FontName);
        Assert.Equal(TextType.Label, vm.TextVM.TextTypeValue);
        Assert.Equal(90, vm.TextVM.Rotation);
    }

    [Fact]
    public void TextProperties_NullForNonText()
    {
        var collection = CreateCollection();
        collection.Add(new Line(0, 0, 1000, 1000));

        var vm = new PropertiesViewModel(collection);

        Assert.Null(vm.TextVM.X);
        Assert.Null(vm.TextVM.Y);
        Assert.Null(vm.TextVM.Content);
        Assert.Null(vm.TextVM.FontSize);
        Assert.Null(vm.TextVM.FontName);
        Assert.Null(vm.TextVM.TextTypeValue);
        Assert.Null(vm.TextVM.Rotation);
    }

    // === Common properties ===

    [Fact]
    public void ObjectId_ReturnsCorrectId()
    {
        var collection = CreateCollection();
        var line = new Line(0, 0, 1000, 1000);
        collection.Add(line);

        var vm = new PropertiesViewModel(collection);
        Assert.Equal(line.Id, vm.ObjectId);
    }

    [Fact]
    public void ObjectId_NullForNoSelection()
    {
        var collection = CreateCollection();
        var vm = new PropertiesViewModel(collection);
        Assert.Null(vm.ObjectId);
    }

    [Fact]
    public void ObjectTypeName_ReturnsCorrectName()
    {
        var collection = CreateCollection();
        collection.Add(new Line(0, 0, 1000, 1000));
        var vmLine = new PropertiesViewModel(collection);
        Assert.Equal("Линия", vmLine.ObjectTypeName);

        collection.Clear();
        collection.Add(new Rectangle(0, 0, 1000, 1000));
        var vmRect = new PropertiesViewModel(collection);
        Assert.Equal("Прямоугольник", vmRect.ObjectTypeName);

        collection.Clear();
        collection.Add(new Text(0, 0, "Test", 3500));
        var vmText = new PropertiesViewModel(collection);
        Assert.Equal("Текст", vmText.ObjectTypeName);
    }

    [Fact]
    public void ObjectTypeName_NullForEmptySelection()
    {
        var collection = CreateCollection();
        var vm = new PropertiesViewModel(collection);
        Assert.Null(vm.ObjectTypeName);
    }

    // === Refresh ===

    [Fact]
    public void Refresh_UpdatesState()
    {
        var collection = CreateCollection();
        var vm = new PropertiesViewModel(collection);
        Assert.Equal(0, vm.SelectionCount);

        collection.Add(new Line(0, 0, 1000, 1000));
        vm.Refresh();

        Assert.Equal(1, vm.SelectionCount);
        Assert.True(vm.IsSingleSelection);
    }

    // === IDisposable Tests ===

    [Fact]
    public void Dispose_UnsubscribesFromCollectionChanged()
    {
        var collection = CreateCollection();
        var vm = new PropertiesViewModel(collection);

        vm.Dispose();

        collection.Add(new Line(0, 0, 1000, 1000));
        Assert.Null(vm.SelectedObject);
    }

    [Fact]
    public void Dispose_DoubleDispose_DoesNotThrow()
    {
        var collection = CreateCollection();
        var vm = new PropertiesViewModel(collection);

        vm.Dispose();
        vm.Dispose();
    }

    // === String Parsing Commands (sub-VM) ===

    [Fact]
    public void ChangeLineStartXFromString_ValidValue_UpdatesCoordinate()
    {
        var collection = CreateCollection();
        var line = new Line(0, 0, 10000, 10000);
        collection.Add(line);
        var history = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, history, null);

        vm.LineVM.ChangeStartXFromStringCommand.Execute("10.5");

        Assert.Equal(10500, line.StartMicronsX);
    }

    [Fact]
    public void ChangeLineStartXFromString_InvalidValue_DoesNothing()
    {
        var collection = CreateCollection();
        var line = new Line(0, 0, 10000, 10000);
        collection.Add(line);
        var history = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, history, null);

        vm.LineVM.ChangeStartXFromStringCommand.Execute("invalid");

        Assert.Equal(0, line.StartMicronsX);
    }

    [Fact]
    public void ChangeLineStartXFromString_NullValue_DoesNothing()
    {
        var collection = CreateCollection();
        var line = new Line(0, 0, 10000, 10000);
        collection.Add(line);
        var history = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, history, null);

        vm.LineVM.ChangeStartXFromStringCommand.Execute(null);

        Assert.Equal(0, line.StartMicronsX);
    }

    [Fact]
    public void ChangeLineEndXFromString_ValidValue_UpdatesCoordinate()
    {
        var collection = CreateCollection();
        var line = new Line(0, 0, 10000, 10000);
        collection.Add(line);
        var history = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, history, null);

        vm.LineVM.ChangeEndXFromStringCommand.Execute("20.0");

        Assert.Equal(20000, line.EndMicronsX);
    }

    [Fact]
    public void ChangeLineEndYFromString_ValidValue_UpdatesCoordinate()
    {
        var collection = CreateCollection();
        var line = new Line(0, 0, 10000, 10000);
        collection.Add(line);
        var history = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, history, null);

        vm.LineVM.ChangeEndYFromStringCommand.Execute("15.5");

        Assert.Equal(15500, line.EndMicronsY);
    }

    [Fact]
    public void ChangeRectXFromString_ValidValue_UpdatesCoordinate()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(0, 0, 10000, 10000);
        collection.Add(rect);
        var history = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, history, null);

        vm.RectVM.ChangeXFromStringCommand.Execute("5.0");

        Assert.Equal(5000, rect.MicronsX);
    }

    [Fact]
    public void ChangeRectYFromString_ValidValue_UpdatesCoordinate()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(0, 0, 10000, 10000);
        collection.Add(rect);
        var history = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, history, null);

        vm.RectVM.ChangeYFromStringCommand.Execute("7.5");

        Assert.Equal(7500, rect.MicronsY);
    }

    [Fact]
    public void ChangeRectWidthFromString_ValidValue_UpdatesWidth()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(0, 0, 10000, 10000);
        collection.Add(rect);
        var history = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, history, null);

        vm.RectVM.ChangeWidthFromStringCommand.Execute("25.0");

        Assert.Equal(25000, rect.WidthMicrons);
    }

    [Fact]
    public void ChangeRectHeightFromString_ValidValue_UpdatesHeight()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(0, 0, 10000, 10000);
        collection.Add(rect);
        var history = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, history, null);

        vm.RectVM.ChangeHeightFromStringCommand.Execute("30.0");

        Assert.Equal(30000, rect.HeightMicrons);
    }

    [Fact]
    public void ChangeTextXFromString_ValidValue_UpdatesCoordinate()
    {
        var collection = CreateCollection();
        var text = new Text(0, 0, "Test", 2500);
        collection.Add(text);
        var history = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, history, null);

        vm.TextVM.ChangeXFromStringCommand.Execute("12.0");

        Assert.Equal(12000, text.MicronsX);
    }

    [Fact]
    public void ChangeTextYFromString_ValidValue_UpdatesCoordinate()
    {
        var collection = CreateCollection();
        var text = new Text(0, 0, "Test", 2500);
        collection.Add(text);
        var history = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, history, null);

        vm.TextVM.ChangeYFromStringCommand.Execute("8.5");

        Assert.Equal(8500, text.MicronsY);
    }

    [Fact]
    public void ChangeTextFontSizeFromString_ValidValue_UpdatesFontSize()
    {
        var collection = CreateCollection();
        var text = new Text(0, 0, "Test", 2500);
        collection.Add(text);
        var history = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, history, null);

        vm.TextVM.ChangeFontSizeFromStringCommand.Execute("5.0");

        Assert.Equal(5000, text.FontSizeMicrons);
    }

    [Fact]
    public void ChangeTextRotationFromString_ValidValue_UpdatesRotation()
    {
        var collection = CreateCollection();
        var text = new Text(0, 0, "Test", 2500);
        collection.Add(text);
        var history = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, history, null);

        vm.TextVM.ChangeRotationFromStringCommand.Execute("90");

        Assert.Equal(90, text.RotationAngle);
    }

    [Fact]
    public void ChangeLineTypeFromString_ValidValue_UpdatesLineType()
    {
        var collection = CreateCollection();
        var line = new Line(0, 0, 10000, 10000);
        collection.Add(line);
        var history = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, history, null);

        vm.LineVM.ChangeLineTypeFromStringCommand.Execute("Штриховая");

        Assert.Equal(LineType.Dashed, line.LineType);
    }

    [Fact]
    public void ChangeLineTypeFromString_InvalidValue_DefaultsToSolid()
    {
        var collection = CreateCollection();
        var line = new Line(0, 0, 10000, 10000);
        collection.Add(line);
        var history = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, history, null);

        vm.LineVM.ChangeLineTypeFromStringCommand.Execute("Invalid");

        Assert.Equal(LineType.Solid, line.LineType);
    }

    [Fact]
    public void ChangeRectLineTypeFromString_ValidValue_UpdatesLineType()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(0, 0, 10000, 10000);
        collection.Add(rect);
        var history = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, history, null);

        vm.RectVM.ChangeLineTypeFromStringCommand.Execute("Штрихпунктирная");

        Assert.Equal(LineType.DashDot, rect.LineType);
    }

    [Fact]
    public void ChangeTextTypeFromString_ValidValue_UpdatesTextType()
    {
        var collection = CreateCollection();
        var text = new Text(0, 0, "Test", 2500);
        collection.Add(text);
        var history = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, history, null);

        vm.TextVM.ChangeTextTypeFromStringCommand.Execute("Размер");

        Assert.Equal(TextType.Dimension, text.TextType);
    }

    [Fact]
    public void ChangeTextTypeFromString_InvalidValue_DefaultsToText()
    {
        var collection = CreateCollection();
        var text = new Text(0, 0, "Test", 2500);
        collection.Add(text);
        var history = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, history, null);

        vm.TextVM.ChangeTextTypeFromStringCommand.Execute("Invalid");

        Assert.Equal(TextType.Text, text.TextType);
    }

    // ==== Extended tests ====

    [Fact]
    public void Constructor_NullCollection_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PropertiesViewModel(null!));
    }

    [Fact]
    public void SelectedObjectsChanged_EmptyCollection_ClearsProperties()
    {
        var collection = CreateCollection();
        collection.Add(new Line(1000, 2000, 3000, 4000));
        var vm = new PropertiesViewModel(collection);

        collection.Clear();
        vm.Refresh();

        Assert.Equal(0, vm.SelectionCount);
        Assert.False(vm.IsSingleSelection);
        Assert.Null(vm.SelectedObject);
        Assert.Null(vm.LineVM.StartX);
        Assert.Null(vm.RectVM.X);
        Assert.Null(vm.TextVM.X);
    }

    [Fact]
    public void SelectedObjectsChanged_MixedTypes_SetsMultiSelection()
    {
        var collection = CreateCollection();
        collection.Add(new Line(0, 0, 1000, 1000));
        collection.Add(new Rectangle(0, 0, 1000, 1000));
        var vm = new PropertiesViewModel(collection);

        Assert.Equal(2, vm.SelectionCount);
        Assert.False(vm.IsSingleSelection);
        Assert.Null(vm.SelectedObject);
        Assert.False(vm.IsLineSelected);
        Assert.False(vm.IsRectangleSelected);
        Assert.False(vm.IsTextSelected);
    }

    [Fact]
    public void SelectedObjectsChanged_SameTypeObjects_UpdatesSelectionCount()
    {
        var collection = CreateCollection();
        collection.Add(new Line(0, 0, 1000, 1000));
        collection.Add(new Line(2000, 2000, 3000, 3000));
        var vm = new PropertiesViewModel(collection);

        Assert.Equal(2, vm.SelectionCount);
        Assert.False(vm.IsSingleSelection);
        Assert.Null(vm.SelectedObject);
    }

    [Fact]
    public void ChangeLineEndXCommand_PushesToCommandHistory()
    {
        var collection = CreateCollection();
        var line = new Line(1000, 2000, 3000, 4000);
        collection.Add(line);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.LineVM.ChangeEndXCommand.Execute(7000);

        Assert.Equal(1, cmdHistory.UndoCount);
        Assert.Equal(7000, line.EndMicronsX);
    }

    [Fact]
    public void ChangeLineEndYCommand_PushesToCommandHistory()
    {
        var collection = CreateCollection();
        var line = new Line(1000, 2000, 3000, 4000);
        collection.Add(line);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.LineVM.ChangeEndYCommand.Execute(8000);

        Assert.Equal(1, cmdHistory.UndoCount);
        Assert.Equal(8000, line.EndMicronsY);
    }

    [Fact]
    public void ChangeLineStartYCommand_PushesToCommandHistory()
    {
        var collection = CreateCollection();
        var line = new Line(1000, 2000, 3000, 4000);
        collection.Add(line);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.LineVM.ChangeStartYCommand.Execute(5000);

        Assert.Equal(1, cmdHistory.UndoCount);
        Assert.Equal(5000, line.StartMicronsY);
    }

    [Fact]
    public void ChangeLineEndXCommand_CallsMarkDirty()
    {
        var collection = CreateCollection();
        var line = new Line(1000, 2000, 3000, 4000);
        collection.Add(line);
        var cmdHistory = CreateCommandHistory();
        bool dirtyCalled = false;
        var vm = new PropertiesViewModel(collection, cmdHistory, () => dirtyCalled = true);

        vm.LineVM.ChangeEndXCommand.Execute(7000);

        Assert.True(dirtyCalled);
    }

    [Fact]
    public void ChangeRectXCommand_CallsMarkDirty()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        collection.Add(rect);
        var cmdHistory = CreateCommandHistory();
        bool dirtyCalled = false;
        var vm = new PropertiesViewModel(collection, cmdHistory, () => dirtyCalled = true);

        vm.RectVM.ChangeXCommand.Execute(2000);

        Assert.True(dirtyCalled);
    }

    [Fact]
    public void ChangeTextYCommand_CallsMarkDirty()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Test", 3500);
        collection.Add(text);
        var cmdHistory = CreateCommandHistory();
        bool dirtyCalled = false;
        var vm = new PropertiesViewModel(collection, cmdHistory, () => dirtyCalled = true);

        vm.TextVM.ChangeYCommand.Execute(3000);

        Assert.True(dirtyCalled);
    }

    [Fact]
    public void ChangeLineStartXCommand_UpdatesLineStartXProperty()
    {
        var collection = CreateCollection();
        var line = new Line(1000, 2000, 3000, 4000);
        collection.Add(line);
        var vm = new PropertiesViewModel(collection, CreateCommandHistory(), null);

        Assert.Equal(1000, vm.LineVM.StartX);
        vm.LineVM.ChangeStartXCommand.Execute(5000);
        Assert.Equal(5000, vm.LineVM.StartX);
    }

    [Fact]
    public void ChangeLineEndXCommand_UpdatesLineEndXProperty()
    {
        var collection = CreateCollection();
        var line = new Line(1000, 2000, 3000, 4000);
        collection.Add(line);
        var vm = new PropertiesViewModel(collection, CreateCommandHistory(), null);

        Assert.Equal(3000, vm.LineVM.EndX);
        vm.LineVM.ChangeEndXCommand.Execute(7000);
        Assert.Equal(7000, vm.LineVM.EndX);
    }

    [Fact]
    public void ChangeRectXCommand_UpdatesRectXProperty()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        collection.Add(rect);
        var vm = new PropertiesViewModel(collection, CreateCommandHistory(), null);

        Assert.Equal(1000, vm.RectVM.X);
        vm.RectVM.ChangeXCommand.Execute(2000);
        Assert.Equal(2000, vm.RectVM.X);
    }

    [Fact]
    public void ChangeRectYCommand_UpdatesRectYProperty()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        collection.Add(rect);
        var vm = new PropertiesViewModel(collection, CreateCommandHistory(), null);

        Assert.Equal(2000, vm.RectVM.Y);
        vm.RectVM.ChangeYCommand.Execute(3000);
        Assert.Equal(3000, vm.RectVM.Y);
    }

    [Fact]
    public void ChangeRectHeightCommand_UpdatesRectHeightProperty()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        collection.Add(rect);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        Assert.Equal(3000, vm.RectVM.Height);
        vm.RectVM.ChangeHeightCommand.Execute(5000);
        Assert.Equal(5000, vm.RectVM.Height);
        Assert.Equal(1, cmdHistory.UndoCount);
    }

    [Fact]
    public void ChangeRectHeightCommand_Undo_RestoresOriginalHeight()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        collection.Add(rect);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.RectVM.ChangeHeightCommand.Execute(5000);
        cmdHistory.Undo();

        Assert.Equal(3000, rect.HeightMicrons);
    }

    [Fact]
    public void ChangeRectLineTypeCommand_UpdatesRectLineType()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(1000, 2000, 5000, 3000, LineType.Solid);
        collection.Add(rect);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.RectVM.ChangeLineTypeCommand.Execute(LineType.Dashed);

        Assert.Equal(LineType.Dashed, rect.LineType);
        Assert.Equal(LineType.Dashed, vm.RectVM.LineTypeValue);
        Assert.Equal(1, cmdHistory.UndoCount);
    }

    [Fact]
    public void ChangeTextXCommand_UpdatesTextXProperty()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Test", 3500);
        collection.Add(text);
        var vm = new PropertiesViewModel(collection, CreateCommandHistory(), null);

        Assert.Equal(1000, vm.TextVM.X);
        vm.TextVM.ChangeXCommand.Execute(2000);
        Assert.Equal(2000, vm.TextVM.X);
    }

    [Fact]
    public void ChangeTextYCommand_UpdatesTextYProperty()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Test", 3500);
        collection.Add(text);
        var vm = new PropertiesViewModel(collection, CreateCommandHistory(), null);

        Assert.Equal(2000, vm.TextVM.Y);
        vm.TextVM.ChangeYCommand.Execute(3000);
        Assert.Equal(3000, vm.TextVM.Y);
    }

    [Fact]
    public void ChangeTextFontSizeCommand_UpdatesTextFontSize()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Test", 3500);
        collection.Add(text);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.TextVM.ChangeFontSizeCommand.Execute(5000);

        Assert.Equal(5000, text.FontSizeMicrons);
        Assert.Equal(5000, vm.TextVM.FontSize);
        Assert.Equal(1, cmdHistory.UndoCount);
    }

    [Fact]
    public void ChangeTextFontSizeCommand_Undo_RestoresOriginalFontSize()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Test", 3500);
        collection.Add(text);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.TextVM.ChangeFontSizeCommand.Execute(5000);
        cmdHistory.Undo();

        Assert.Equal(3500, text.FontSizeMicrons);
    }

    [Fact]
    public void ChangeTextFontSizeCommand_SetsValidationErrorForTooSmall()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Test", 3500);
        collection.Add(text);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.TextVM.ChangeFontSizeCommand.Execute(500);

        Assert.NotNull(vm.ValidationError);
        Assert.Equal(0, cmdHistory.UndoCount);
    }

    [Fact]
    public void ChangeRectLineTypeCommand_PushesToCommandHistory()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(1000, 2000, 5000, 3000, LineType.Solid);
        collection.Add(rect);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.RectVM.ChangeLineTypeCommand.Execute(LineType.DashDot);

        Assert.Equal(1, cmdHistory.UndoCount);
    }

    [Fact]
    public void ChangeRectLineTypeCommand_Undo_RestoresOriginalType()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(1000, 2000, 5000, 3000, LineType.Solid);
        collection.Add(rect);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.RectVM.ChangeLineTypeCommand.Execute(LineType.DashDot);
        cmdHistory.Undo();

        Assert.Equal(LineType.Solid, rect.LineType);
    }

    [Fact]
    public void ChangeTextRotationCommand_UpdatesRotationProperty()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Test", 3500, rotationAngle: 0);
        collection.Add(text);
        var vm = new PropertiesViewModel(collection, CreateCommandHistory(), null);

        Assert.Equal(0, vm.TextVM.Rotation);
        vm.TextVM.ChangeRotationCommand.Execute(90);
        Assert.Equal(90, vm.TextVM.Rotation);
    }

    [Fact]
    public void ChangeTextRotationCommand_PushesToCommandHistory()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Test", 3500, rotationAngle: 0);
        collection.Add(text);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.TextVM.ChangeRotationCommand.Execute(180);

        Assert.Equal(1, cmdHistory.UndoCount);
    }

    [Fact]
    public void ChangeTextRotationCommand_Undo_RestoresOriginalRotation()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Test", 3500, rotationAngle: 90);
        collection.Add(text);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.TextVM.ChangeRotationCommand.Execute(180);
        cmdHistory.Undo();

        Assert.Equal(90, text.RotationAngle);
        Assert.Equal(90, vm.TextVM.Rotation);
    }

    [Fact]
    public void ChangeRectWidthCommand_RejectsNegativeWidth()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        collection.Add(rect);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.RectVM.ChangeWidthCommand.Execute(-1000);

        Assert.NotNull(vm.ValidationError);
        Assert.Equal(5000, rect.WidthMicrons);
        Assert.Equal(0, cmdHistory.UndoCount);
    }

    [Fact]
    public void ChangeRectHeightCommand_RejectsNegativeHeight()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        collection.Add(rect);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.RectVM.ChangeHeightCommand.Execute(-500);

        Assert.NotNull(vm.ValidationError);
        Assert.Equal(3000, rect.HeightMicrons);
        Assert.Equal(0, cmdHistory.UndoCount);
    }

    [Fact]
    public void ChangeRectXCommand_RejectsNegativeCoordinate()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        collection.Add(rect);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.RectVM.ChangeXCommand.Execute(-100);

        Assert.NotNull(vm.ValidationError);
        Assert.Equal(1000, rect.MicronsX);
        Assert.Equal(0, cmdHistory.UndoCount);
    }

    [Fact]
    public void ChangeTextXCommand_RejectsNegativeCoordinate()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Test", 3500);
        collection.Add(text);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.TextVM.ChangeXCommand.Execute(-500);

        Assert.NotNull(vm.ValidationError);
        Assert.Equal(1000, text.MicronsX);
        Assert.Equal(0, cmdHistory.UndoCount);
    }

    [Fact]
    public void ChangeRectXCommand_DoesNothingWhenLineSelected()
    {
        var collection = CreateCollection();
        collection.Add(new Line(1000, 2000, 3000, 4000));
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.RectVM.ChangeXCommand.Execute(5000);

        Assert.Equal(0, cmdHistory.UndoCount);
    }

    [Fact]
    public void ChangeTextContentCommand_DoesNothingWhenLineSelected()
    {
        var collection = CreateCollection();
        collection.Add(new Line(1000, 2000, 3000, 4000));
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.TextVM.ChangeContentCommand.Execute("New text");

        Assert.Equal(0, cmdHistory.UndoCount);
    }

    [Fact]
    public void ChangeLineTypeCommand_DoesNothingWhenRectSelected()
    {
        var collection = CreateCollection();
        collection.Add(new Rectangle(1000, 2000, 5000, 3000));
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.LineVM.ChangeLineTypeCommand.Execute(LineType.Dashed);

        Assert.Equal(0, cmdHistory.UndoCount);
    }

    [Fact]
    public void ChangeLineStartXCommand_WithoutCommandHistory_DoesNotExecute()
    {
        var collection = CreateCollection();
        var line = new Line(1000, 2000, 3000, 4000);
        collection.Add(line);
        var vm = new PropertiesViewModel(collection);

        vm.LineVM.ChangeStartXCommand.Execute(5000);

        Assert.Equal(1000, line.StartMicronsX);
    }

    [Fact]
    public void ChangeRectWidthCommand_WithoutCommandHistory_DoesNotExecute()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        collection.Add(rect);
        var vm = new PropertiesViewModel(collection);

        vm.RectVM.ChangeWidthCommand.Execute(8000);

        Assert.Equal(5000, rect.WidthMicrons);
    }

    [Fact]
    public void CollectionChanged_AddingItem_UpdatesSelection()
    {
        var collection = CreateCollection();
        var vm = new PropertiesViewModel(collection);
        Assert.Equal(0, vm.SelectionCount);

        collection.Add(new Line(0, 0, 1000, 1000));

        Assert.Equal(1, vm.SelectionCount);
        Assert.True(vm.IsSingleSelection);
        Assert.True(vm.IsLineSelected);
    }

    [Fact]
    public void CollectionChanged_RemovingItem_UpdatesSelection()
    {
        var collection = CreateCollection();
        collection.Add(new Line(0, 0, 1000, 1000));
        var vm = new PropertiesViewModel(collection);
        Assert.Equal(1, vm.SelectionCount);

        collection.RemoveAt(0);

        Assert.Equal(0, vm.SelectionCount);
        Assert.False(vm.IsSingleSelection);
        Assert.Null(vm.SelectedObject);
    }

    [Fact]
    public void CollectionChanged_ReplacingItem_UpdatesSelection()
    {
        var collection = CreateCollection();
        collection.Add(new Line(0, 0, 1000, 1000));
        var vm = new PropertiesViewModel(collection);
        var oldLine = vm.SelectedObject;

        collection[0] = new Rectangle(0, 0, 1000, 1000);

        Assert.Equal(1, vm.SelectionCount);
        Assert.True(vm.IsSingleSelection);
        Assert.True(vm.IsRectangleSelected);
        Assert.NotSame(oldLine, vm.SelectedObject);
    }

    [Fact]
    public void PropertyChanged_LineProperties_RaiseEvents()
    {
        var collection = CreateCollection();
        var line = new Line(1000, 2000, 3000, 4000);
        collection.Add(line);
        var vm = new PropertiesViewModel(collection);

        var raisedProperties = new List<string?>();
        ((INotifyPropertyChanged)vm.LineVM).PropertyChanged += (s, e) => raisedProperties.Add(e.PropertyName);

        vm.LineVM.ChangeStartXCommand.Execute(5000);

        Assert.Contains("StartX", raisedProperties);
    }

    [Fact]
    public void PropertyChanged_RectProperties_RaiseEvents()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        collection.Add(rect);
        var vm = new PropertiesViewModel(collection);

        var raisedProperties = new List<string?>();
        ((INotifyPropertyChanged)vm.RectVM).PropertyChanged += (s, e) => raisedProperties.Add(e.PropertyName);

        vm.RectVM.ChangeWidthCommand.Execute(8000);

        Assert.Contains("Width", raisedProperties);
    }

    [Fact]
    public void PropertyChanged_TextProperties_RaiseEvents()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Test", 3500);
        collection.Add(text);
        var vm = new PropertiesViewModel(collection);

        var raisedProperties = new List<string?>();
        ((INotifyPropertyChanged)vm.TextVM).PropertyChanged += (s, e) => raisedProperties.Add(e.PropertyName);

        vm.TextVM.ChangeContentCommand.Execute("Updated");

        Assert.Contains("Content", raisedProperties);
    }

    [Fact]
    public void PropertyChanged_SelectionChange_RaisesTypeEvents()
    {
        var collection = CreateCollection();
        var vm = new PropertiesViewModel(collection);

        var raisedProperties = new List<string?>();
        ((INotifyPropertyChanged)vm).PropertyChanged += (s, e) => raisedProperties.Add(e.PropertyName);

        collection.Add(new Line(0, 0, 1000, 1000));

        Assert.Contains("IsLineSelected", raisedProperties);
        Assert.Contains("IsRectangleSelected", raisedProperties);
        Assert.Contains("IsTextSelected", raisedProperties);
        Assert.Contains("SelectionCount", raisedProperties);
        Assert.Contains("IsSingleSelection", raisedProperties);
        Assert.Contains("SelectedObject", raisedProperties);
    }

    [Fact]
    public void ChangeTextContentCommand_PushesToCommandHistory()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Hello", 3500);
        collection.Add(text);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.TextVM.ChangeContentCommand.Execute("World");

        Assert.Equal(1, cmdHistory.UndoCount);
    }

    [Fact]
    public void ChangeTextContentCommand_UpdatesTextContentProperty()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Hello", 3500);
        collection.Add(text);
        var vm = new PropertiesViewModel(collection, CreateCommandHistory(), null);

        Assert.Equal("Hello", vm.TextVM.Content);
        vm.TextVM.ChangeContentCommand.Execute("World");
        Assert.Equal("World", vm.TextVM.Content);
    }

    [Fact]
    public void ChangeTextContentCommand_CallsMarkDirty()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Hello", 3500);
        collection.Add(text);
        var cmdHistory = CreateCommandHistory();
        bool dirtyCalled = false;
        var vm = new PropertiesViewModel(collection, cmdHistory, () => dirtyCalled = true);

        vm.TextVM.ChangeContentCommand.Execute("World");

        Assert.True(dirtyCalled);
    }

    [Fact]
    public void ChangeTextTypeCommand_PushesToCommandHistory()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Test", 3500, textType: TextType.Text);
        collection.Add(text);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.TextVM.ChangeTextTypeCommand.Execute(TextType.Dimension);

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

        vm.TextVM.ChangeTextTypeCommand.Execute(TextType.Note);
        cmdHistory.Undo();

        Assert.Equal(TextType.Text, text.TextType);
        Assert.Equal(TextType.Text, vm.TextVM.TextTypeValue);
    }

    [Fact]
    public void ChangeTextTypeCommand_CallsMarkDirty()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Test", 3500);
        collection.Add(text);
        var cmdHistory = CreateCommandHistory();
        bool dirtyCalled = false;
        var vm = new PropertiesViewModel(collection, cmdHistory, () => dirtyCalled = true);

        vm.TextVM.ChangeTextTypeCommand.Execute(TextType.Label);

        Assert.True(dirtyCalled);
    }

    [Fact]
    public void UpdateSelection_RaisesPropertyChangedForSelectedObjectType()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(1000, 2000, 5000, 3000);
        collection.Add(rect);
        var vm = new PropertiesViewModel(collection);

        var raisedProperties = new List<string?>();
        ((INotifyPropertyChanged)vm).PropertyChanged += (s, e) => raisedProperties.Add(e.PropertyName);

        collection.Clear();
        collection.Add(rect);

        Assert.Contains(nameof(PropertiesViewModel.ObjectId), raisedProperties);
        Assert.Contains(nameof(PropertiesViewModel.ObjectTypeName), raisedProperties);
    }

    [Fact]
    public void UpdateSelection_ChangesObject_UpdatesPropertyValues()
    {
        var collection = CreateCollection();
        var rect1 = new Rectangle(1000, 2000, 5000, 3000);
        var rect2 = new Rectangle(2000, 3000, 6000, 4000);

        collection.Add(rect1);
        var vm = new PropertiesViewModel(collection);

        Assert.Equal(1000, vm.RectVM.X);
        Assert.Equal(2000, vm.RectVM.Y);
        Assert.Equal(5000, vm.RectVM.Width);
        Assert.Equal(3000, vm.RectVM.Height);

        collection.Clear();
        collection.Add(rect2);

        Assert.Equal(2000, vm.RectVM.X);
        Assert.Equal(3000, vm.RectVM.Y);
        Assert.Equal(6000, vm.RectVM.Width);
        Assert.Equal(4000, vm.RectVM.Height);
    }

    // ============================================================
    // Undo after deselection (regression: NRE on field capture)
    // ============================================================

    [Fact]
    public void LineProperties_Undo_AfterDeselect_DoesNotThrow()
    {
        var collection = CreateCollection();
        var line = new Line(1000, 2000, 3000, 4000) { LineType = LineType.Solid };
        collection.Add(line);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.LineVM.ChangeLineTypeCommand.Execute(LineType.Dashed);

        Assert.Equal(LineType.Dashed, line.LineType);

        collection.Clear();

        var exception = Record.Exception(() => cmdHistory.Undo());

        Assert.Null(exception);
        Assert.Equal(LineType.Solid, line.LineType);

        cmdHistory.Redo();
        Assert.Equal(LineType.Dashed, line.LineType);
    }

    [Fact]
    public void RectangleProperties_Undo_AfterDeselect_DoesNotThrow()
    {
        var collection = CreateCollection();
        var rect = new Rectangle(1000, 2000, 5000, 3000) { LineType = LineType.Solid };
        collection.Add(rect);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.RectVM.ChangeLineTypeCommand.Execute(LineType.DashDot);

        Assert.Equal(LineType.DashDot, rect.LineType);

        collection.Clear();

        var exception = Record.Exception(() => cmdHistory.Undo());

        Assert.Null(exception);
        Assert.Equal(LineType.Solid, rect.LineType);

        cmdHistory.Redo();
        Assert.Equal(LineType.DashDot, rect.LineType);
    }

    [Fact]
    public void TextProperties_Undo_AfterDeselect_DoesNotThrow()
    {
        var collection = CreateCollection();
        var text = new Text(1000, 2000, "Hello", 5000);
        collection.Add(text);
        var cmdHistory = CreateCommandHistory();
        var vm = new PropertiesViewModel(collection, cmdHistory, null);

        vm.TextVM.ChangeFontSizeCommand.Execute(8000);

        Assert.Equal(8000, text.FontSizeMicrons);

        collection.Clear();

        var exception = Record.Exception(() => cmdHistory.Undo());

        Assert.Null(exception);
        Assert.Equal(5000, text.FontSizeMicrons);

        cmdHistory.Redo();
        Assert.Equal(8000, text.FontSizeMicrons);
    }
}
