using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.ViewModels.Managers;

namespace DotElectric.TemplateEditor.Tests.ViewModels.Managers;

public class SelectionManagerTests
{
    private SelectionManager CreateSut() => new(() => { });

    [Fact]
    public void SelectSingle_ClearsPreviousSelection()
    {
        var sut = CreateSut();
        var obj1 = new Line(0, 0, 10000, 10000);
        var obj2 = new Line(0, 0, 20000, 20000);

        sut.SelectSingle(obj1);
        sut.SelectSingle(obj2);

        Assert.Single(sut.SelectedObjects);
        Assert.Equal(obj2, sut.SelectedObjects[0]);
    }

    [Fact]
    public void AddToSelection_DoesNotAddDuplicates()
    {
        var sut = CreateSut();
        var obj = new Line(0, 0, 10000, 10000);

        sut.AddToSelection(obj);
        sut.AddToSelection(obj);

        Assert.Single(sut.SelectedObjects);
    }

    [Fact]
    public void RemoveFromSelection_RemovesObject()
    {
        var sut = CreateSut();
        var obj = new Line(0, 0, 10000, 10000);

        sut.SelectSingle(obj);
        sut.RemoveFromSelection(obj);

        Assert.Empty(sut.SelectedObjects);
    }

    [Fact]
    public void ClearSelection_RemovesAll()
    {
        var sut = CreateSut();
        var obj1 = new Line(0, 0, 10000, 10000);
        var obj2 = new Line(0, 0, 20000, 20000);

        sut.AddToSelection(obj1);
        sut.AddToSelection(obj2);
        sut.ClearSelection();

        Assert.Empty(sut.SelectedObjects);
    }

    [Fact]
    public void SelectAll_SelectsAllObjects()
    {
        var sut = CreateSut();
        var objects = new List<TemplateObjectBase>
        {
            new Line(0, 0, 10000, 10000),
            new Line(0, 0, 20000, 20000)
        };

        sut.SelectAll(objects);

        Assert.Equal(2, sut.SelectedObjects.Count);
    }

    [Fact]
    public void ShowSelectionMarkers_TrueForSingleSelection()
    {
        var sut = CreateSut();
        var obj = new Line(0, 0, 10000, 10000);

        sut.SelectSingle(obj);

        Assert.True(sut.ShowSelectionMarkers);
    }

    [Fact]
    public void ShowSelectionMarkers_TrueForMultipleSelection()
    {
        var sut = CreateSut();
        var obj1 = new Line(0, 0, 10000, 10000);
        var obj2 = new Line(0, 0, 20000, 20000);

        sut.AddToSelection(obj1);
        sut.AddToSelection(obj2);

        Assert.True(sut.ShowSelectionMarkers);
    }

    [Fact]
    public void SingleSelectedObject_ReturnsObjectForSingleSelection()
    {
        var sut = CreateSut();
        var obj = new Line(0, 0, 10000, 10000);

        sut.SelectSingle(obj);

        Assert.Equal(obj, sut.SingleSelectedObject);
    }

    [Fact]
    public void SingleSelectedObject_NullForMultipleSelection()
    {
        var sut = CreateSut();
        var obj1 = new Line(0, 0, 10000, 10000);
        var obj2 = new Line(0, 0, 20000, 20000);

        sut.AddToSelection(obj1);
        sut.AddToSelection(obj2);

        Assert.Null(sut.SingleSelectedObject);
    }
}

public class ClipboardManagerTests
{
    [Fact]
    public void Copy_ClonesObjects()
    {
        var sut = new ClipboardManager();
        var obj = new Line(0, 0, 10000, 10000);

        sut.Copy(new[] { obj });

        Assert.Equal(1, sut.Count);
        var first = sut.GetClipboardContents();
        Assert.NotSame(obj, first[0]);
        Assert.Equal(obj.MicronsX + 10000, ((Line)first[0]).MicronsX);
    }

    [Fact]
    public void Copy_ClearsPreviousClipboard()
    {
        var sut = new ClipboardManager();
        var obj1 = new Line(0, 0, 10000, 10000);
        var obj2 = new Line(0, 0, 20000, 20000);

        sut.Copy(new[] { obj1 });
        sut.Copy(new[] { obj2 });

        var contents = sut.GetClipboardContents();
        Assert.Single(contents);
        Assert.Equal(obj2.MicronsX + 10000, ((Line)contents[0]).MicronsX);
    }

    [Fact]
    public void GetClipboardContents_ReturnsReadOnlyCopy()
    {
        var sut = new ClipboardManager();
        var obj = new Line(0, 0, 10000, 10000);

        sut.Copy(new[] { obj });
        var contents = sut.GetClipboardContents();

        Assert.NotNull(contents);
        Assert.Single(contents);
    }

    [Fact]
    public void Count_ReturnsZeroForEmptyClipboard()
    {
        var sut = new ClipboardManager();

        Assert.Equal(0, sut.Count);
    }

    [Fact]
    public void Cut_CopiesAndCallsDeleteAction()
    {
        var sut = new ClipboardManager();
        var obj = new Line(0, 0, 10000, 10000);
        var deleted = new List<TemplateObjectBase>();

        sut.Cut(new[] { obj }, objects => deleted.AddRange(objects));

        Assert.Equal(1, sut.Count);
        Assert.Single(deleted);
        Assert.Same(obj, deleted[0]);
    }

    [Fact]
    public void Cut_MultipleObjects_CopiesAllAndCallsDeleteAction()
    {
        var sut = new ClipboardManager();
        var obj1 = new Line(0, 0, 10000, 10000);
        var obj2 = new Line(0, 0, 20000, 20000);
        var deleted = new List<TemplateObjectBase>();

        sut.Cut(new[] { obj1, obj2 }, objects => deleted.AddRange(objects));

        Assert.Equal(2, sut.Count);
        Assert.Equal(2, deleted.Count);
    }

    [Fact]
    public void Cut_EmptyCollection_DoesNotThrow()
    {
        var sut = new ClipboardManager();
        var called = false;

        var ex = Record.Exception(() => sut.Cut(Array.Empty<TemplateObjectBase>(), _ => called = true));

        Assert.Null(ex);
        Assert.Equal(0, sut.Count);
        Assert.True(called);
    }

    [Fact]
    public void Clear_EmptiesClipboard()
    {
        var sut = new ClipboardManager();
        var obj = new Line(0, 0, 10000, 10000);
        sut.Copy(new[] { obj });
        Assert.Equal(1, sut.Count);

        sut.Clear();

        Assert.Equal(0, sut.Count);
    }

    [Fact]
    public void Clear_AlreadyEmpty_DoesNotThrow()
    {
        var sut = new ClipboardManager();

        var ex = Record.Exception(() => sut.Clear());

        Assert.Null(ex);
    }

    [Fact]
    public void GetClipboardContents_ReturnsCloneWithOffset()
    {
        var sut = new ClipboardManager();
        var obj = new Line(0, 0, 10000, 10000);
        sut.Copy(new[] { obj });

        var contents = sut.GetClipboardContents();

        Assert.Single(contents);
        var cloned = (Line)contents[0];
        Assert.NotSame(obj, cloned);
        Assert.Equal(10000, cloned.MicronsX); // 0 + 10000 offset
        Assert.Equal(10000, cloned.MicronsY); // 0 + 10000 offset
    }

    [Fact]
    public void GetClipboardContents_OffsetIncrementsOnEachCall()
    {
        var sut = new ClipboardManager();
        var obj = new Line(0, 0, 10000, 10000);
        sut.Copy(new[] { obj });

        var first = sut.GetClipboardContents();
        var second = sut.GetClipboardContents();

        var firstLine = (Line)first[0];
        var secondLine = (Line)second[0];
        Assert.Equal(10000, firstLine.MicronsX);
        Assert.Equal(20000, secondLine.MicronsX);
    }

    [Fact]
    public void PasteOffset_ResetsOnNewCopy()
    {
        var sut = new ClipboardManager();
        var obj = new Line(0, 0, 10000, 10000);
        sut.Copy(new[] { obj });

        var first = sut.GetClipboardContents(); // offset = 10mm
        var firstLine = (Line)first[0];
        Assert.Equal(10000, firstLine.MicronsX);

        // Second paste without new copy
        var second = sut.GetClipboardContents(); // offset = 20mm
        var secondLine = (Line)second[0];
        Assert.Equal(20000, secondLine.MicronsX);

        // New copy resets offset
        sut.Copy(new[] { obj });
        var third = sut.GetClipboardContents(); // offset = 10mm again
        var thirdLine = (Line)third[0];
        Assert.Equal(10000, thirdLine.MicronsX);
    }
}

public class SelectionManagerExtendedTests
{
    [Fact]
    public void SelectAll_ClearsPreviousAndSelectsGiven()
    {
        var sut = new SelectionManager(() => { });
        var obj1 = new Line(0, 0, 10000, 10000);
        var obj2 = new Line(0, 0, 20000, 20000);
        sut.SelectSingle(obj1);

        sut.SelectAll(new[] { obj2 });

        Assert.Single(sut.SelectedObjects);
        Assert.Equal(obj2, sut.SelectedObjects[0]);
    }

    [Fact]
    public void SelectAll_EmptyCollection_ClearsSelection()
    {
        var sut = new SelectionManager(() => { });
        var obj = new Line(0, 0, 10000, 10000);
        sut.SelectSingle(obj);

        sut.SelectAll(Array.Empty<TemplateObjectBase>());

        Assert.Empty(sut.SelectedObjects);
    }

    [Fact]
    public void SelectAll_MultipleObjects_SelectsAll()
    {
        var sut = new SelectionManager(() => { });
        var obj1 = new Line(0, 0, 10000, 10000);
        var obj2 = new Line(0, 0, 20000, 20000);

        sut.SelectAll(new[] { obj1, obj2 });

        Assert.Equal(2, sut.SelectedObjects.Count);
        Assert.Contains(obj1, sut.SelectedObjects);
        Assert.Contains(obj2, sut.SelectedObjects);
    }

    [Fact]
    public void IsObjectSelected_ReturnsTrueForSelectedObject()
    {
        var sut = new SelectionManager(() => { });
        var obj = new Line(0, 0, 10000, 10000);
        sut.SelectSingle(obj);

        Assert.True(sut.IsObjectSelected(obj));
    }

    [Fact]
    public void IsObjectSelected_ReturnsFalseForNonSelectedObject()
    {
        var sut = new SelectionManager(() => { });
        var obj1 = new Line(0, 0, 10000, 10000);
        var obj2 = new Line(0, 0, 20000, 20000);
        sut.SelectSingle(obj1);

        Assert.False(sut.IsObjectSelected(obj2));
    }

    [Fact]
    public void IsObjectSelected_ReturnsFalseForRemovedObject()
    {
        var sut = new SelectionManager(() => { });
        var obj = new Line(0, 0, 10000, 10000);
        sut.SelectSingle(obj);
        sut.RemoveFromSelection(obj);

        Assert.False(sut.IsObjectSelected(obj));
    }

    [Fact]
    public void IsObjectSelected_ReturnsFalseForEmptySelection()
    {
        var sut = new SelectionManager(() => { });
        var obj = new Line(0, 0, 10000, 10000);

        Assert.False(sut.IsObjectSelected(obj));
    }

    [Fact]
    public void Constructor_CallsOnSelectionChangedOnCollectionChange()
    {
        var callCount = 0;
        var sut = new SelectionManager(() => callCount++);

        sut.AddToSelection(new Line(0, 0, 10000, 10000));

        Assert.Equal(1, callCount);
    }

    [Fact]
    public void SelectAll_SelectsAllGivenObjects()
    {
        var sut = new SelectionManager(() => { });
        var objects = new TemplateObjectBase[]
        {
            new Line(0, 0, 10000, 10000),
            new Rectangle(0, 0, 5000, 5000),
            new Text(0, 0, "Test", 2500)
        };

        sut.SelectAll(objects);

        Assert.Equal(3, sut.SelectedObjects.Count);
    }
}

// ==================== PreviewManager ====================

public class PreviewManagerTests
{
    private readonly PreviewManager _manager;

    public PreviewManagerTests()
    {
        _manager = new PreviewManager();
    }

    [Fact]
    public void Constructor_InitializesDefaults()
    {
        Assert.Null(_manager.PreviewLine);
        Assert.Null(_manager.PreviewRectangle);
        Assert.Null(_manager.PreviewText);
        Assert.Equal(0, _manager.SelectionBoxLeft);
        Assert.Equal(0, _manager.SelectionBoxBottom);
        Assert.Equal(0, _manager.SelectionBoxWidth);
        Assert.Equal(0, _manager.SelectionBoxHeight);
        Assert.Equal(SelectionDirection.LeftToRight, _manager.SelectionBoxDirection);
    }

    [Fact]
    public void ClearAll_ClearsAllPreviewElements()
    {
        _manager.PreviewLine = new Line(0, 0, 1000, 1000);
        _manager.PreviewRectangle = new Rectangle(0, 0, 1000, 1000);
        _manager.PreviewText = new Text(0, 0, "Test", 2500);

        _manager.ClearAll();

        Assert.Null(_manager.PreviewLine);
        Assert.Null(_manager.PreviewRectangle);
        Assert.Null(_manager.PreviewText);
    }

    [Fact]
    public void SetSelectionBox_SetsAllProperties()
    {
        _manager.SetSelectionBox(1000, 2000, 3000, 4000, SelectionDirection.RightToLeft);

        Assert.Equal(1000, _manager.SelectionBoxLeft);
        Assert.Equal(2000, _manager.SelectionBoxBottom);
        Assert.Equal(3000, _manager.SelectionBoxWidth);
        Assert.Equal(4000, _manager.SelectionBoxHeight);
        Assert.Equal(SelectionDirection.RightToLeft, _manager.SelectionBoxDirection);
    }

    [Fact]
    public void SelectionBoxTop_CalculatedCorrectly()
    {
        _manager.SetSelectionBox(1000, 2000, 3000, 4000, SelectionDirection.LeftToRight);

        Assert.Equal(6000, _manager.SelectionBoxTop); // 2000 + 4000
    }

    [Fact]
    public void SelectionBoxRight_CalculatedCorrectly()
    {
        _manager.SetSelectionBox(1000, 2000, 3000, 4000, SelectionDirection.LeftToRight);

        Assert.Equal(4000, _manager.SelectionBoxRight); // 1000 + 3000
    }

    [Fact]
    public void ClearSelectionBox_ResetsAllProperties()
    {
        _manager.SetSelectionBox(1000, 2000, 3000, 4000, SelectionDirection.RightToLeft);

        _manager.ClearSelectionBox();

        Assert.Equal(0, _manager.SelectionBoxLeft);
        Assert.Equal(0, _manager.SelectionBoxBottom);
        Assert.Equal(0, _manager.SelectionBoxWidth);
        Assert.Equal(0, _manager.SelectionBoxHeight);
        Assert.Equal(SelectionDirection.LeftToRight, _manager.SelectionBoxDirection);
    }
}

// ==================== InlineEditManager ====================

public class InlineEditManagerTests
{
    private readonly CommandHistory _commandHistory;
    private readonly InlineEditManager _manager;
    private string? _lastStatus;

    public InlineEditManagerTests()
    {
        _commandHistory = new CommandHistory(50);
        _manager = new InlineEditManager(_commandHistory, markDirty: null, onStatusChanged: s => _lastStatus = s);
    }

    [Fact]
    public void Constructor_InitializesDefaults()
    {
        Assert.Null(_manager.InlineEditingText);
        Assert.Equal(string.Empty, _manager.InlineEditText);
        Assert.False(_manager.IsEditing);
    }

    [Fact]
    public void Start_SetsTextAndStatus()
    {
        var text = new Text(0, 0, "Original", 2500);

        _manager.Start(text);

        Assert.Same(text, _manager.InlineEditingText);
        Assert.Equal("Original", _manager.InlineEditText);
        Assert.True(_manager.IsEditing);
        Assert.Equal("Редактирование текста", _lastStatus);
    }

    [Fact]
    public void Commit_WithValidText_PushesCommand()
    {
        var text = new Text(0, 0, "Original", 2500);
        _manager.Start(text);
        _manager.InlineEditText = "Modified";

        _manager.Commit();

        Assert.Null(_manager.InlineEditingText);
        Assert.Equal(string.Empty, _manager.InlineEditText);
        Assert.False(_manager.IsEditing);
        Assert.Equal("Готово", _lastStatus);
        Assert.Equal("Modified", text.Content);
    }

    [Fact]
    public void Commit_WithEmptyText_DoesNotPushCommand()
    {
        var text = new Text(0, 0, "Original", 2500);
        _manager.Start(text);
        _manager.InlineEditText = "   ";

        _manager.Commit();

        Assert.Equal("Original", text.Content); // Unchanged
    }

    [Fact]
    public void Commit_WithNullText_DoesNotThrow()
    {
        _manager.InlineEditingText = null;
        _manager.InlineEditText = "Modified";

        var ex = Record.Exception(() => _manager.Commit());
        Assert.Null(ex);
    }

    [Fact]
    public void Cancel_ClearsEditingState()
    {
        var text = new Text(0, 0, "Original", 2500);
        _manager.Start(text);
        _manager.InlineEditText = "Modified";

        _manager.Cancel();

        Assert.Null(_manager.InlineEditingText);
        Assert.Equal(string.Empty, _manager.InlineEditText);
        Assert.False(_manager.IsEditing);
        Assert.Equal("Готово", _lastStatus);
        Assert.Equal("Original", text.Content); // Unchanged
    }

    [Fact]
    public void Commit_PushesUndoableCommand()
    {
        var text = new Text(0, 0, "Original", 2500);
        _manager.Start(text);
        _manager.InlineEditText = "Modified";

        _manager.Commit();

        Assert.True(_commandHistory.CanUndo);
        _commandHistory.Undo();
        Assert.Equal("Original", text.Content);
    }
}

// ==================== StatusBarManager ====================

public class StatusBarManagerTests
{
    private readonly Template _template;
    private bool _gridEnabled;
    private double _gridStepMm;
    private bool _snapEnabled;
    private readonly StatusBarManager _manager;
    private int _gridRefreshCount;

    public StatusBarManagerTests()
    {
        _template = new Template();
        _gridEnabled = true;
        _gridStepMm = 1.0;
        _snapEnabled = true;
        _gridRefreshCount = 0;
        _manager = new StatusBarManager(
            _template,
            () => _gridEnabled, v => { _gridEnabled = v; },
            () => _gridStepMm, v => _gridStepMm = v,
            () => _snapEnabled, v => _snapEnabled = v,
            () => _gridRefreshCount++);
    }

    [Fact]
    public void Constructor_InitializesDefaults()
    {
        Assert.Equal("Готово", _manager.StatusMessage);
        Assert.True(_manager.GridEnabled);
        Assert.True(_manager.SnapEnabled);
        Assert.Equal(1.0, _manager.GridStepMm);
    }

    [Fact]
    public void SheetFormat_ReturnsCorrectString()
    {
        var result = _manager.SheetFormat;
        Assert.Contains(_template.Sheet.Format, result);
        Assert.Contains("мм", result);
    }

    [Fact]
    public void GridEnabled_SetFalse_DisablesGridAndRefreshes()
    {
        _manager.GridEnabled = false;

        Assert.False(_gridEnabled);
        Assert.Equal(1, _gridRefreshCount);
    }

    [Fact]
    public void GridEnabled_SetTrue_EnablesGridAndRefreshes()
    {
        _gridEnabled = false;
        _gridRefreshCount = 0;

        _manager.GridEnabled = true;

        Assert.True(_gridEnabled);
        Assert.Equal(1, _gridRefreshCount);
    }

    [Fact]
    public void GridStepMm_Set_UpdatesStepMicrons()
    {
        _manager.GridStepMm = 5.0;

        Assert.Equal(5.0, _gridStepMm);
        Assert.Equal(1, _gridRefreshCount);
    }

    [Fact]
    public void GridStepMm_Get_ReturnsCorrectValue()
    {
        _gridStepMm = 5.0;

        Assert.Equal(5.0, _manager.GridStepMm);
    }

    [Fact]
    public void SnapEnabled_Set_UpdatesSnapEnabled()
    {
        _manager.SnapEnabled = false;

        Assert.False(_snapEnabled);
        Assert.Equal(0, _gridRefreshCount); // No grid refresh for snap
    }

    [Fact]
    public void StatusMessage_Set_UpdatesStatus()
    {
        _manager.StatusMessage = "Test Status";

        Assert.Equal("Test Status", _manager.StatusMessage);
    }
}
