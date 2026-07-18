using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Tests.Commands;

public class CustomResizeCommandTests
{
    // ===== Rectangle Tests =====

    [Fact]
    public void Name_IsCorrect()
    {
        var rect = new Rectangle(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(0, 0, 1000, 1000),
            s => rect.ApplyResize(s),
            new ResizeState(0, 0, 2000, 2000),
            "размер");
        Assert.Equal("Изменить размер", cmd.Name);
    }

    [Fact]
    public void Execute_Rectangle_UpdatesAllProperties()
    {
        var rect = new Rectangle(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(0, 0, 1000, 1000),
            s => rect.ApplyResize(s),
            new ResizeState(5000, 3000, 4000, 2000),
            "размер");

        cmd.Execute();

        Assert.Equal(5000, rect.MicronsX);
        Assert.Equal(3000, rect.MicronsY);
        Assert.Equal(4000, rect.WidthMicrons);
        Assert.Equal(2000, rect.HeightMicrons);
    }

    [Fact]
    public void Undo_Rectangle_RestoresAllProperties()
    {
        var rect = new Rectangle(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(0, 0, 1000, 1000),
            s => rect.ApplyResize(s),
            new ResizeState(5000, 3000, 4000, 2000),
            "размер");
        cmd.Execute();

        cmd.Undo();

        Assert.Equal(0, rect.MicronsX);
        Assert.Equal(0, rect.MicronsY);
        Assert.Equal(1000, rect.WidthMicrons);
        Assert.Equal(1000, rect.HeightMicrons);
    }

    [Fact]
    public void Execute_Rectangle_CallsMarkDirty()
    {
        var dirtyCalled = false;
        var rect = new Rectangle(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(0, 0, 1000, 1000),
            s => rect.ApplyResize(s),
            new ResizeState(5000, 3000, 4000, 2000),
            "размер",
            () => dirtyCalled = true);

        cmd.Execute();

        Assert.True(dirtyCalled);
    }

    [Fact]
    public void Execute_Rectangle_NullMarkDirty_DoesNotThrow()
    {
        var rect = new Rectangle(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(0, 0, 1000, 1000),
            s => rect.ApplyResize(s),
            new ResizeState(5000, 3000, 4000, 2000),
            "размер",
            null);

        cmd.Execute();

        Assert.Equal(5000, rect.MicronsX);
    }

    // ===== Text Tests =====

    [Fact]
    public void Execute_Text_UpdatesPositionAndFontSize()
    {
        var text = new Text(0, 0, "Test", 2500);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(0, 0, 0, 2500),
            s => text.ApplyResize(s),
            new ResizeState(5000, 3000, 0, 3500),
            "размер");

        cmd.Execute();

        Assert.Equal(5000, text.MicronsX);
        Assert.Equal(3000, text.MicronsY);
        Assert.Equal(3500, text.FontSizeMicrons);
    }

    [Fact]
    public void Undo_Text_RestoresPositionAndFontSize()
    {
        var text = new Text(0, 0, "Test", 2500);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(0, 0, 0, 2500),
            s => text.ApplyResize(s),
            new ResizeState(5000, 3000, 0, 3500),
            "размер");
        cmd.Execute();

        cmd.Undo();

        Assert.Equal(0, text.MicronsX);
        Assert.Equal(0, text.MicronsY);
        Assert.Equal(2500, text.FontSizeMicrons);
    }

    [Fact]
    public void Execute_Text_CallsMarkDirty()
    {
        var dirtyCalled = false;
        var text = new Text(0, 0, "Test", 2500);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(0, 0, 0, 2500),
            s => text.ApplyResize(s),
            new ResizeState(5000, 3000, 0, 3500),
            "размер",
            () => dirtyCalled = true);

        cmd.Execute();

        Assert.True(dirtyCalled);
    }

    // ===== Line Tests =====

    [Fact]
    public void Execute_Line_UpdatesStartAndEndPoints()
    {
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(0, 0, 1000, 1000),
            s => line.ApplyResize(s),
            new ResizeState(5000, 3000, 4000, 2000),
            "размер");

        cmd.Execute();

        Assert.Equal(5000, line.StartMicronsX);
        Assert.Equal(3000, line.StartMicronsY);
        Assert.Equal(9000, line.EndMicronsX);
        Assert.Equal(5000, line.EndMicronsY);
    }

    [Fact]
    public void Undo_Line_RestoresStartAndEndPoints()
    {
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(0, 0, 1000, 1000),
            s => line.ApplyResize(s),
            new ResizeState(5000, 3000, 4000, 2000),
            "размер");
        cmd.Execute();

        cmd.Undo();

        Assert.Equal(0, line.StartMicronsX);
        Assert.Equal(0, line.StartMicronsY);
        Assert.Equal(1000, line.EndMicronsX);
        Assert.Equal(1000, line.EndMicronsY);
    }

    [Fact]
    public void Execute_Line_CallsMarkDirty()
    {
        var dirtyCalled = false;
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(0, 0, 1000, 1000),
            s => line.ApplyResize(s),
            new ResizeState(5000, 3000, 4000, 2000),
            "размер",
            () => dirtyCalled = true);

        cmd.Execute();

        Assert.True(dirtyCalled);
    }

    // ===== Execute/Undo Round-trip =====

    [Fact]
    public void ExecuteUndoExecute_Rectangle_SameResult()
    {
        var rect = new Rectangle(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(0, 0, 1000, 1000),
            s => rect.ApplyResize(s),
            new ResizeState(5000, 3000, 4000, 2000),
            "размер");

        cmd.Execute();
        cmd.Undo();
        cmd.Execute();

        Assert.Equal(5000, rect.MicronsX);
        Assert.Equal(3000, rect.MicronsY);
        Assert.Equal(4000, rect.WidthMicrons);
        Assert.Equal(2000, rect.HeightMicrons);
    }

    [Fact]
    public void ExecuteUndoExecute_Text_SameResult()
    {
        var text = new Text(0, 0, "Test", 2500);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(0, 0, 0, 2500),
            s => text.ApplyResize(s),
            new ResizeState(5000, 3000, 0, 3500),
            "размер");

        cmd.Execute();
        cmd.Undo();
        cmd.Execute();

        Assert.Equal(5000, text.MicronsX);
        Assert.Equal(3000, text.MicronsY);
        Assert.Equal(3500, text.FontSizeMicrons);
    }

    [Fact]
    public void ExecuteUndoExecute_Line_SameResult()
    {
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(0, 0, 1000, 1000),
            s => line.ApplyResize(s),
            new ResizeState(5000, 3000, 4000, 2000),
            "размер");

        cmd.Execute();
        cmd.Undo();
        cmd.Execute();

        Assert.Equal(5000, line.StartMicronsX);
        Assert.Equal(3000, line.StartMicronsY);
        Assert.Equal(9000, line.EndMicronsX);
        Assert.Equal(5000, line.EndMicronsY);
    }

    // ===== Edge Cases =====

    [Fact]
    public void Execute_ZeroValues_Works()
    {
        var rect = new Rectangle(1000, 1000, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(1000, 1000, 1000, 1000),
            s => rect.ApplyResize(s),
            new ResizeState(0, 0, 0, 0),
            "размер");

        cmd.Execute();

        Assert.Equal(0, rect.MicronsX);
        Assert.Equal(0, rect.MicronsY);
        Assert.Equal(0, rect.WidthMicrons);
        Assert.Equal(0, rect.HeightMicrons);
    }

    [Fact]
    public void Execute_NegativeValues_Works()
    {
        var rect = new Rectangle(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(0, 0, 1000, 1000),
            s => rect.ApplyResize(s),
            new ResizeState(-5000, -3000, 4000, 2000),
            "размер");

        cmd.Execute();

        Assert.Equal(-5000, rect.MicronsX);
        Assert.Equal(-3000, rect.MicronsY);
    }

    [Fact]
    public void Execute_LargeValues_Works()
    {
        var rect = new Rectangle(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(0, 0, 1000, 1000),
            s => rect.ApplyResize(s),
            new ResizeState(1_000_000, 500_000, 200_000, 100_000),
            "размер");

        cmd.Execute();

        Assert.Equal(1_000_000, rect.MicronsX);
        Assert.Equal(500_000, rect.MicronsY);
        Assert.Equal(200_000, rect.WidthMicrons);
        Assert.Equal(100_000, rect.HeightMicrons);
    }
}
