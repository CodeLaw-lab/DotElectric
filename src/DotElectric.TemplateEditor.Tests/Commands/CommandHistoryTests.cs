using DotElectric.TemplateEditor.Commands;

namespace DotElectric.TemplateEditor.Tests.Commands;

public class CommandHistoryTests
{
    // ===== Execute / Push =====

    [Fact]
    public void ExecuteCommand_AddsToUndoStack()
    {
        var history = new CommandHistory(maxLevels: 50);
        var cmd = new MockCommand();

        history.Push(cmd);

        Assert.True(history.CanUndo);
        Assert.Equal(1, history.UndoCount);
        Assert.False(history.CanRedo);
        Assert.True(cmd.ExecuteCalled);
    }

    [Fact]
    public void ExecuteCommand_ClearsRedoStack()
    {
        var history = new CommandHistory(maxLevels: 50);
        var cmd1 = new MockCommand();
        var cmd2 = new MockCommand();
        history.Push(cmd1);
        history.Undo();
        Assert.True(history.CanRedo);

        history.Push(cmd2);

        Assert.False(history.CanRedo);
        Assert.Equal(0, history.RedoCount);
    }

    [Fact]
    public void ExecuteCommand_CallsMarkDirty()
    {
        bool dirtyCalled = false;
        var history = new CommandHistory(maxLevels: 50, markDirty: () => dirtyCalled = true);
        var cmd = new MockCommand();

        history.Push(cmd);

        Assert.True(dirtyCalled);
    }

    [Fact]
    public void ExecuteCommand_NullCommand_ThrowsArgumentNullException()
    {
        var history = new CommandHistory();
        Assert.Throws<ArgumentNullException>(() => history.Push(null!));
    }

    // ===== Undo =====

    [Fact]
    public void Undo_MovesCommandToRedoStack()
    {
        var history = new CommandHistory(maxLevels: 50);
        var cmd = new MockCommand();
        history.Push(cmd);

        history.Undo();

        Assert.False(history.CanUndo);
        Assert.True(history.CanRedo);
        Assert.True(cmd.UndoCalled);
        Assert.Equal(1, history.RedoCount);
    }

    [Fact]
    public void Undo_EmptyStack_NoOp()
    {
        var history = new CommandHistory();
        // Should not throw
        history.Undo();
        Assert.False(history.CanUndo);
        Assert.False(history.CanRedo);
    }

    [Fact]
    public void Undo_Error_RollbackAndRethrows()
    {
        var history = new CommandHistory();
        var cmd = new MockCommand { UndoThrowsError = true };
        history.Push(cmd);

        var ex = Assert.Throws<InvalidOperationException>(() => history.Undo());
        Assert.Contains("Mock Command", ex.Message);
        // Rollback — команда вернулась в undo-стек
        Assert.True(history.CanUndo);
    }

    // ===== Redo =====

    [Fact]
    public void Redo_MovesCommandBackToUndoStack()
    {
        var history = new CommandHistory(maxLevels: 50);
        var cmd = new MockCommand();
        history.Push(cmd);
        history.Undo();

        history.Redo();

        Assert.True(history.CanUndo);
        Assert.False(history.CanRedo);
        Assert.Equal(2, cmd.ExecuteCalledCount); // Execute вызван дважды
    }

    [Fact]
    public void Redo_EmptyStack_NoOp()
    {
        var history = new CommandHistory();
        // Should not throw
        history.Redo();
        Assert.False(history.CanRedo);
        Assert.False(history.CanUndo);
    }

    [Fact]
    public void Redo_Error_RollbackAndRethrows()
    {
        var history = new CommandHistory();
        var cmd = new MockCommand { ExecuteThrowsErrorOnSecondCall = true };
        history.Push(cmd);
        history.Undo();

        var ex = Assert.Throws<InvalidOperationException>(() => history.Redo());
        Assert.Contains("Mock Command", ex.Message);
        Assert.True(history.CanRedo);
    }

    // ===== Trim =====

    [Fact]
    public void TrimHistory_WhenExceedingMaxLevels()
    {
        var history = new CommandHistory(maxLevels: 3);
        history.Push(new MockCommand());
        history.Push(new MockCommand());
        history.Push(new MockCommand());
        history.Push(new MockCommand());

        Assert.Equal(3, history.UndoCount);
    }

    [Fact]
    public void TrimHistory_RemovesOldestFirst()
    {
        var history = new CommandHistory(maxLevels: 2);
        var cmd1 = new MockCommand { Name = "First" };
        var cmd2 = new MockCommand { Name = "Second" };
        var cmd3 = new MockCommand { Name = "Third" };

        history.Push(cmd1);
        history.Push(cmd2);
        history.Push(cmd3);

        // cmd1 должна быть удалена
        Assert.Equal(2, history.UndoCount);
        // Undo дважды — должны получить cmd3, затем cmd2
        history.Undo();
        Assert.Equal("Third", history.LastRedoName);
    }

    // ===== Clear =====

    [Fact]
    public void ClearHistory_ResetsStacks()
    {
        var history = new CommandHistory(maxLevels: 50);
        history.Push(new MockCommand());
        history.Push(new MockCommand());
        history.Undo();

        history.Clear();

        Assert.False(history.CanUndo);
        Assert.False(history.CanRedo);
        Assert.Equal(0, history.UndoCount);
        Assert.Equal(0, history.RedoCount);
    }

    // ===== LastUndoName / LastRedoName =====

    [Fact]
    public void LastUndoName_ReturnsCommandName()
    {
        var history = new CommandHistory();
        var cmd = new MockCommand { Name = "Test Command" };
        history.Push(cmd);

        Assert.Equal("Test Command", history.LastUndoName);
    }

    [Fact]
    public void LastRedoName_ReturnsCommandNameAfterUndo()
    {
        var history = new CommandHistory();
        var cmd = new MockCommand { Name = "Test Command" };
        history.Push(cmd);
        history.Undo();

        Assert.Equal("Test Command", history.LastRedoName);
    }

    [Fact]
    public void LastUndoName_NullWhenEmpty()
    {
        var history = new CommandHistory();
        Assert.Null(history.LastUndoName);
    }

    [Fact]
    public void LastRedoName_NullWhenEmpty()
    {
        var history = new CommandHistory();
        Assert.Null(history.LastRedoName);
    }

    // ===== Constructor =====

    [Fact]
    public void Constructor_InvalidMaxLevels_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CommandHistory(maxLevels: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CommandHistory(maxLevels: -1));
    }

    // ===== Tests from CommandHistoryExtendedTests =====

    [Fact]
    public void Push_ExceedsMaxLevels_TrimsOldest()
    {
        var history = new CommandHistory(maxLevels: 3, markDirty: () => { });
        var cmd1 = new MockCommand();
        var cmd2 = new MockCommand();
        var cmd3 = new MockCommand();
        var cmd4 = new MockCommand();

        history.Push(cmd1);
        history.Push(cmd2);
        history.Push(cmd3);
        history.Push(cmd4);

        Assert.Equal(3, history.UndoCount);
        history.Undo();
        history.Undo();
        history.Undo();
        Assert.Equal(0, history.UndoCount);
    }

    [Fact]
    public void Push_WithMarkDirty_CallsCallback()
    {
        int callCount = 0;
        var history = new CommandHistory(maxLevels: 50, markDirty: () => callCount++);
        var cmd = new MockCommand();

        history.Push(cmd);
        Assert.Equal(1, callCount);
    }

    [Fact]
    public void Undo_CanUndoChanges()
    {
        var history = new CommandHistory(maxLevels: 50);
        var cmd = new MockCommand();
        history.Push(cmd);

        history.Undo();
        Assert.True(cmd.UndoCalled);
    }

    [Fact]
    public void Redo_CanRedoChanges()
    {
        var history = new CommandHistory(maxLevels: 50);
        var cmd = new MockCommand();
        history.Push(cmd);
        history.Undo();
        cmd.ExecuteCalled = false;

        history.Redo();
        Assert.True(cmd.ExecuteCalled);
    }

    [Fact]
    public void CanUndo_AfterTrim_ReturnsCorrectValue()
    {
        var history = new CommandHistory(maxLevels: 2);
        history.Push(new MockCommand());
        history.Push(new MockCommand());
        history.Push(new MockCommand());

        Assert.True(history.CanUndo);
        history.Undo();
        history.Undo();
        Assert.False(history.CanUndo);
    }

    [Fact]
    public void CanRedo_AfterNewPush_ReturnsFalse()
    {
        var history = new CommandHistory(maxLevels: 50);
        history.Push(new MockCommand());
        history.Undo();
        Assert.True(history.CanRedo);

        history.Push(new MockCommand());
        Assert.False(history.CanRedo);
    }

    [Fact]
    public void LastRedoName_ReturnsCommandName()
    {
        var history = new CommandHistory(maxLevels: 50);
        var cmd = new MockCommand { Name = "Test Command" };
        history.Push(cmd);
        history.Undo();

        Assert.Equal("Test Command", history.LastRedoName);
    }

    [Fact]
    public void LastUndoName_EmptyStack_ReturnsNull()
    {
        var history = new CommandHistory(maxLevels: 50);
        Assert.Null(history.LastUndoName);
    }

    [Fact]
    public void LastRedoName_EmptyStack_ReturnsNull()
    {
        var history = new CommandHistory(maxLevels: 50);
        Assert.Null(history.LastRedoName);
    }

    [Fact]
    public void UndoCount_ReturnsCorrectCount()
    {
        var history = new CommandHistory(maxLevels: 50);
        history.Push(new MockCommand());
        history.Push(new MockCommand());
        history.Push(new MockCommand());

        Assert.Equal(3, history.UndoCount);
    }

    [Fact]
    public void RedoCount_ReturnsCorrectCount()
    {
        var history = new CommandHistory(maxLevels: 50);
        history.Push(new MockCommand());
        history.Push(new MockCommand());
        history.Undo();
        history.Undo();

        Assert.Equal(2, history.RedoCount);
    }

    [Fact]
    public void Clear_ClearsAllStacks()
    {
        var history = new CommandHistory(maxLevels: 50);
        history.Push(new MockCommand());
        history.Push(new MockCommand());
        history.Undo();

        history.Clear();

        Assert.False(history.CanUndo);
        Assert.False(history.CanRedo);
        Assert.Equal(0, history.UndoCount);
        Assert.Equal(0, history.RedoCount);
    }

    [Fact]
    public void MaxLevels_CannotBeLessThan1()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CommandHistory(maxLevels: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CommandHistory(maxLevels: -1));
    }

    [Fact]
    public void Push_WithNullCommand_Throws()
    {
        var history = new CommandHistory(maxLevels: 50);
        Assert.Throws<ArgumentNullException>(() => history.Push(null!));
    }

    [Fact]
    public void Undo_ThenPush_ClearsRedoStack()
    {
        var history = new CommandHistory(maxLevels: 50);
        history.Push(new MockCommand());
        history.Push(new MockCommand());
        history.Undo();
        Assert.True(history.CanRedo);

        history.Push(new MockCommand());
        Assert.False(history.CanRedo);
        Assert.Equal(0, history.RedoCount);
    }

    [Fact]
    public void MultipleUndoRedo_CorrectOrder()
    {
        var history = new CommandHistory(maxLevels: 50);
        var cmd1 = new MockCommand { Name = "Cmd1" };
        var cmd2 = new MockCommand { Name = "Cmd2" };
        var cmd3 = new MockCommand { Name = "Cmd3" };

        history.Push(cmd1);
        history.Push(cmd2);
        history.Push(cmd3);

        history.Undo();
        history.Undo();
        history.Undo();
        Assert.False(history.CanUndo);
        Assert.Equal(3, history.RedoCount);

        history.Redo();
        history.Redo();
        history.Redo();
        Assert.Equal(3, history.UndoCount);
        Assert.False(history.CanRedo);
    }

    [Fact]
    public void MaxLevels_1_Pushes2Commands_TrimsOldest()
    {
        var history = new CommandHistory(maxLevels: 1);
        history.Push(new MockCommand());
        history.Push(new MockCommand());

        Assert.Equal(1, history.UndoCount);
    }

    [Fact]
    public void Push_WithNullMarkDirty_DoesNotThrow()
    {
        var history = new CommandHistory(maxLevels: 50, markDirty: null);
        history.Push(new MockCommand());
        Assert.True(history.CanUndo);
    }

    // ===== MockCommand =====

    private class MockCommand : IUndoCommand
    {
        public string Name { get; set; } = "Mock Command";
        public bool ExecuteCalled;
        public int ExecuteCalledCount;
        public bool UndoCalled;
        public bool UndoThrowsError;
        public bool ExecuteThrowsErrorOnSecondCall;

        public void Execute()
        {
            if (ExecuteThrowsErrorOnSecondCall && ExecuteCalledCount > 0)
                throw new InvalidOperationException("Execute error");
            ExecuteCalled = true;
            ExecuteCalledCount++;
        }

        public void Undo()
        {
            if (UndoThrowsError)
                throw new InvalidOperationException("Undo error");
            UndoCalled = true;
        }
    }
}
