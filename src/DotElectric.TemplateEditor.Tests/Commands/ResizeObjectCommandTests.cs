using System.Reflection;
using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Tests.Commands;

public class ResizeObjectCommandTests
{
    // ===== Rectangle tests =====

    [Fact]
    public void RectangleConstructor_Execute_ChangesWidthAndHeight()
    {
        var rect = new Rectangle(0, 0, 1000, 2000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(rect.MicronsX, rect.MicronsY, rect.WidthMicrons, rect.HeightMicrons),
            s => rect.ApplyResize(s),
            new ResizeState(rect.MicronsX, rect.MicronsY, 5000, 3000),
            "размер");

        cmd.Execute();

        Assert.Equal(5000, rect.WidthMicrons);
        Assert.Equal(3000, rect.HeightMicrons);
    }

    [Fact]
    public void RectangleConstructor_Undo_RestoresOriginalWidthAndHeight()
    {
        var rect = new Rectangle(0, 0, 1000, 2000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(rect.MicronsX, rect.MicronsY, rect.WidthMicrons, rect.HeightMicrons),
            s => rect.ApplyResize(s),
            new ResizeState(rect.MicronsX, rect.MicronsY, 5000, 3000),
            "размер");
        cmd.Execute();

        cmd.Undo();

        Assert.Equal(1000, rect.WidthMicrons);
        Assert.Equal(2000, rect.HeightMicrons);
    }

    [Fact]
    public void RectangleConstructor_Execute_CallsMarkDirty()
    {
        var rect = new Rectangle(0, 0, 1000, 2000);
        bool dirtyCalled = false;
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(rect.MicronsX, rect.MicronsY, rect.WidthMicrons, rect.HeightMicrons),
            s => rect.ApplyResize(s),
            new ResizeState(rect.MicronsX, rect.MicronsY, 5000, 3000),
            "размер",
            () => dirtyCalled = true);

        cmd.Execute();

        Assert.True(dirtyCalled);
    }

    [Fact]
    public void RectangleConstructor_Undo_CallsMarkDirty()
    {
        var rect = new Rectangle(0, 0, 1000, 2000);
        bool dirtyCalled = false;
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(rect.MicronsX, rect.MicronsY, rect.WidthMicrons, rect.HeightMicrons),
            s => rect.ApplyResize(s),
            new ResizeState(rect.MicronsX, rect.MicronsY, 5000, 3000),
            "размер",
            () => dirtyCalled = true);
        cmd.Execute();
        dirtyCalled = false;

        cmd.Undo();

        Assert.True(dirtyCalled);
    }

    [Fact]
    public void RectangleConstructor_Name_ReturnsCorrectName()
    {
        var rect = new Rectangle(0, 0, 1000, 2000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(rect.MicronsX, rect.MicronsY, rect.WidthMicrons, rect.HeightMicrons),
            s => rect.ApplyResize(s),
            new ResizeState(rect.MicronsX, rect.MicronsY, 5000, 3000),
            "размер");

        Assert.Equal("Изменить размер", cmd.Name);
    }

    [Fact]
    public void RectangleConstructor_ExecuteZeroSize_SetsZeroDimensions()
    {
        var rect = new Rectangle(0, 0, 1000, 2000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(rect.MicronsX, rect.MicronsY, rect.WidthMicrons, rect.HeightMicrons),
            s => rect.ApplyResize(s),
            new ResizeState(rect.MicronsX, rect.MicronsY, 0, 0),
            "размер");

        cmd.Execute();

        Assert.Equal(0, rect.WidthMicrons);
        Assert.Equal(0, rect.HeightMicrons);
    }

    [Fact]
    public void RectangleConstructor_ExecuteNegativeSize_SetsNegativeDimensions()
    {
        var rect = new Rectangle(0, 0, 1000, 2000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(rect.MicronsX, rect.MicronsY, rect.WidthMicrons, rect.HeightMicrons),
            s => rect.ApplyResize(s),
            new ResizeState(rect.MicronsX, rect.MicronsY, -500, -300),
            "размер");

        cmd.Execute();

        Assert.Equal(-500, rect.WidthMicrons);
        Assert.Equal(-300, rect.HeightMicrons);
    }

    [Fact]
    public void RectangleConstructor_ExecuteVeryLargeValues_SetsLargeDimensions()
    {
        var rect = new Rectangle(0, 0, 1000, 2000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(rect.MicronsX, rect.MicronsY, rect.WidthMicrons, rect.HeightMicrons),
            s => rect.ApplyResize(s),
            new ResizeState(rect.MicronsX, rect.MicronsY, long.MaxValue / 2, long.MaxValue / 2),
            "размер");

        cmd.Execute();

        Assert.Equal(long.MaxValue / 2, rect.WidthMicrons);
        Assert.Equal(long.MaxValue / 2, rect.HeightMicrons);
    }

    [Fact]
    public void RectangleConstructor_Undo_RestoresCorrectlyAfterMultipleExecuteUndo()
    {
        var rect = new Rectangle(0, 0, 1000, 2000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(rect.MicronsX, rect.MicronsY, rect.WidthMicrons, rect.HeightMicrons),
            s => rect.ApplyResize(s),
            new ResizeState(rect.MicronsX, rect.MicronsY, 5000, 3000),
            "размер");

        cmd.Execute();
        cmd.Undo();
        cmd.Execute();
        cmd.Undo();

        Assert.Equal(1000, rect.WidthMicrons);
        Assert.Equal(2000, rect.HeightMicrons);
    }

    [Fact]
    public void RectangleConstructor_NullMarkDirty_DoesNotThrow()
    {
        var rect = new Rectangle(0, 0, 1000, 2000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(rect.MicronsX, rect.MicronsY, rect.WidthMicrons, rect.HeightMicrons),
            s => rect.ApplyResize(s),
            new ResizeState(rect.MicronsX, rect.MicronsY, 5000, 3000),
            "размер",
            null);

        var ex = Record.Exception(() => cmd.Execute());
        Assert.Null(ex);
    }

    // ===== Text tests =====

    [Fact]
    public void TextConstructor_Execute_ChangesPositionAndFontSize()
    {
        var text = new Text(0, 0, "Test", 2500);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(text.MicronsX, text.MicronsY, 0, text.FontSizeMicrons),
            s => text.ApplyResize(s),
            new ResizeState(5000, 6000, 0, 3500),
            "размер");

        cmd.Execute();

        Assert.Equal(5000, text.MicronsX);
        Assert.Equal(6000, text.MicronsY);
        Assert.Equal(3500, text.FontSizeMicrons);
    }

    [Fact]
    public void TextConstructor_Undo_RestoresOriginalPositionAndFontSize()
    {
        var text = new Text(1000, 2000, "Test", 2500);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(text.MicronsX, text.MicronsY, 0, text.FontSizeMicrons),
            s => text.ApplyResize(s),
            new ResizeState(5000, 6000, 0, 3500),
            "размер");
        cmd.Execute();

        cmd.Undo();

        Assert.Equal(1000, text.MicronsX);
        Assert.Equal(2000, text.MicronsY);
        Assert.Equal(2500, text.FontSizeMicrons);
    }

    [Fact]
    public void TextConstructor_Execute_CallsMarkDirty()
    {
        var text = new Text(0, 0, "Test", 2500);
        bool dirtyCalled = false;
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(text.MicronsX, text.MicronsY, 0, text.FontSizeMicrons),
            s => text.ApplyResize(s),
            new ResizeState(5000, 6000, 0, 3500),
            "размер",
            () => dirtyCalled = true);

        cmd.Execute();

        Assert.True(dirtyCalled);
    }

    [Fact]
    public void TextConstructor_Undo_CallsMarkDirty()
    {
        var text = new Text(0, 0, "Test", 2500);
        bool dirtyCalled = false;
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(text.MicronsX, text.MicronsY, 0, text.FontSizeMicrons),
            s => text.ApplyResize(s),
            new ResizeState(5000, 6000, 0, 3500),
            "размер",
            () => dirtyCalled = true);
        cmd.Execute();
        dirtyCalled = false;

        cmd.Undo();

        Assert.True(dirtyCalled);
    }

    [Fact]
    public void TextConstructor_Name_ReturnsCorrectName()
    {
        var text = new Text(0, 0, "Test", 2500);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(text.MicronsX, text.MicronsY, 0, text.FontSizeMicrons),
            s => text.ApplyResize(s),
            new ResizeState(5000, 6000, 0, 3500),
            "размер");

        Assert.Equal("Изменить размер", cmd.Name);
    }

    [Fact]
    public void TextConstructor_ExecuteZeroFontSize_SetsZeroFontSize()
    {
        var text = new Text(1000, 2000, "Test", 2500);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(text.MicronsX, text.MicronsY, 0, text.FontSizeMicrons),
            s => text.ApplyResize(s),
            new ResizeState(0, 0, 0, 0),
            "размер");

        cmd.Execute();

        Assert.Equal(0, text.MicronsX);
        Assert.Equal(0, text.MicronsY);
        Assert.Equal(0, text.FontSizeMicrons);
    }

    [Fact]
    public void TextConstructor_ExecuteNegativeFontSize_SetsNegativeFontSize()
    {
        var text = new Text(1000, 2000, "Test", 2500);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(text.MicronsX, text.MicronsY, 0, text.FontSizeMicrons),
            s => text.ApplyResize(s),
            new ResizeState(-1000, -2000, 0, -500),
            "размер");

        cmd.Execute();

        Assert.Equal(-1000, text.MicronsX);
        Assert.Equal(-2000, text.MicronsY);
        Assert.Equal(-500, text.FontSizeMicrons);
    }

    [Fact]
    public void TextConstructor_ExecuteVeryLargeValues_SetsLargeFontSize()
    {
        var text = new Text(1000, 2000, "Test", 2500);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(text.MicronsX, text.MicronsY, 0, text.FontSizeMicrons),
            s => text.ApplyResize(s),
            new ResizeState(1_000_000, 2_000_000, 0, 50_000),
            "размер");

        cmd.Execute();

        Assert.Equal(1_000_000, text.MicronsX);
        Assert.Equal(2_000_000, text.MicronsY);
        Assert.Equal(50_000, text.FontSizeMicrons);
    }

    [Fact]
    public void TextConstructor_ExecuteWithSamePosition_OnlyChangesFontSize()
    {
        var text = new Text(1000, 2000, "Test", 2500);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(text.MicronsX, text.MicronsY, 0, text.FontSizeMicrons),
            s => text.ApplyResize(s),
            new ResizeState(1000, 2000, 0, 3500),
            "размер");

        cmd.Execute();

        Assert.Equal(1000, text.MicronsX);
        Assert.Equal(2000, text.MicronsY);
        Assert.Equal(3500, text.FontSizeMicrons);
    }

    // ===== Line tests =====

    [Fact]
    public void LineConstructor_Execute_ChangesStartAndEndCoordinates()
    {
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(line.StartMicronsX, line.StartMicronsY,
                line.EndMicronsX - line.StartMicronsX, line.EndMicronsY - line.StartMicronsY),
            s => line.ApplyResize(s),
            new ResizeState(5000, 6000, 15000 - 5000, 16000 - 6000),
            "размер");

        cmd.Execute();

        Assert.Equal(5000, line.StartMicronsX);
        Assert.Equal(6000, line.StartMicronsY);
        Assert.Equal(15000, line.EndMicronsX);
        Assert.Equal(16000, line.EndMicronsY);
    }

    [Fact]
    public void LineConstructor_Undo_RestoresOriginalCoordinates()
    {
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(line.StartMicronsX, line.StartMicronsY,
                line.EndMicronsX - line.StartMicronsX, line.EndMicronsY - line.StartMicronsY),
            s => line.ApplyResize(s),
            new ResizeState(5000, 6000, 15000 - 5000, 16000 - 6000),
            "размер");
        cmd.Execute();

        cmd.Undo();

        Assert.Equal(0, line.StartMicronsX);
        Assert.Equal(0, line.StartMicronsY);
        Assert.Equal(1000, line.EndMicronsX);
        Assert.Equal(1000, line.EndMicronsY);
    }

    [Fact]
    public void LineConstructor_Execute_CallsMarkDirty()
    {
        var line = new Line(0, 0, 1000, 1000);
        bool dirtyCalled = false;
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(line.StartMicronsX, line.StartMicronsY,
                line.EndMicronsX - line.StartMicronsX, line.EndMicronsY - line.StartMicronsY),
            s => line.ApplyResize(s),
            new ResizeState(5000, 6000, 15000 - 5000, 16000 - 6000),
            "размер",
            () => dirtyCalled = true);

        cmd.Execute();

        Assert.True(dirtyCalled);
    }

    [Fact]
    public void LineConstructor_Undo_CallsMarkDirty()
    {
        var line = new Line(0, 0, 1000, 1000);
        bool dirtyCalled = false;
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(line.StartMicronsX, line.StartMicronsY,
                line.EndMicronsX - line.StartMicronsX, line.EndMicronsY - line.StartMicronsY),
            s => line.ApplyResize(s),
            new ResizeState(5000, 6000, 15000 - 5000, 16000 - 6000),
            "размер",
            () => dirtyCalled = true);
        cmd.Execute();
        dirtyCalled = false;

        cmd.Undo();

        Assert.True(dirtyCalled);
    }

    [Fact]
    public void LineConstructor_Name_ReturnsCorrectName()
    {
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(line.StartMicronsX, line.StartMicronsY,
                line.EndMicronsX - line.StartMicronsX, line.EndMicronsY - line.StartMicronsY),
            s => line.ApplyResize(s),
            new ResizeState(5000, 6000, 15000 - 5000, 16000 - 6000),
            "размер");

        Assert.Equal("Изменить размер", cmd.Name);
    }

    [Fact]
    public void LineConstructor_SameStartAndEndPoint_ExecuteSetsZeroLengthLine()
    {
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(line.StartMicronsX, line.StartMicronsY,
                line.EndMicronsX - line.StartMicronsX, line.EndMicronsY - line.StartMicronsY),
            s => line.ApplyResize(s),
            new ResizeState(5000, 5000, 0, 0),
            "размер");

        cmd.Execute();

        Assert.Equal(5000, line.StartMicronsX);
        Assert.Equal(5000, line.StartMicronsY);
        Assert.Equal(5000, line.EndMicronsX);
        Assert.Equal(5000, line.EndMicronsY);
    }

    [Fact]
    public void LineConstructor_EndBeforeStart_NegativeWidthAndHeight()
    {
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(line.StartMicronsX, line.StartMicronsY,
                line.EndMicronsX - line.StartMicronsX, line.EndMicronsY - line.StartMicronsY),
            s => line.ApplyResize(s),
            new ResizeState(10000, 10000, 5000 - 10000, 5000 - 10000),
            "размер");

        cmd.Execute();

        Assert.Equal(10000, line.StartMicronsX);
        Assert.Equal(10000, line.StartMicronsY);
        Assert.Equal(5000, line.EndMicronsX);
        Assert.Equal(5000, line.EndMicronsY);
    }

    [Fact]
    public void LineConstructor_Undo_RestoresCorrectlyAfterMultipleExecuteUndo()
    {
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(line.StartMicronsX, line.StartMicronsY,
                line.EndMicronsX - line.StartMicronsX, line.EndMicronsY - line.StartMicronsY),
            s => line.ApplyResize(s),
            new ResizeState(5000, 6000, 15000 - 5000, 16000 - 6000),
            "размер");

        cmd.Execute();
        cmd.Undo();
        cmd.Execute();
        cmd.Undo();

        Assert.Equal(0, line.StartMicronsX);
        Assert.Equal(0, line.StartMicronsY);
        Assert.Equal(1000, line.EndMicronsX);
        Assert.Equal(1000, line.EndMicronsY);
    }

    [Fact]
    public void LineConstructor_ExecuteZeroLengthLine_SetsSameStartEnd()
    {
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(line.StartMicronsX, line.StartMicronsY,
                line.EndMicronsX - line.StartMicronsX, line.EndMicronsY - line.StartMicronsY),
            s => line.ApplyResize(s),
            new ResizeState(3000, 3000, 0, 0),
            "размер");

        cmd.Execute();

        Assert.Equal(3000, line.StartMicronsX);
        Assert.Equal(3000, line.StartMicronsY);
        Assert.Equal(3000, line.EndMicronsX);
        Assert.Equal(3000, line.EndMicronsY);
    }

    [Fact]
    public void LineConstructor_ExecuteVeryLargeValues_SetsLargeCoordinates()
    {
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(line.StartMicronsX, line.StartMicronsY,
                line.EndMicronsX - line.StartMicronsX, line.EndMicronsY - line.StartMicronsY),
            s => line.ApplyResize(s),
            new ResizeState(1_000_000, 2_000_000, 5_000_000 - 1_000_000, 6_000_000 - 2_000_000),
            "размер");

        cmd.Execute();

        Assert.Equal(1_000_000, line.StartMicronsX);
        Assert.Equal(2_000_000, line.StartMicronsY);
        Assert.Equal(5_000_000, line.EndMicronsX);
        Assert.Equal(6_000_000, line.EndMicronsY);
    }

    [Fact]
    public void LineConstructor_NegativeCoordinates_SetsNegativeValues()
    {
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(line.StartMicronsX, line.StartMicronsY,
                line.EndMicronsX - line.StartMicronsX, line.EndMicronsY - line.StartMicronsY),
            s => line.ApplyResize(s),
            new ResizeState(-5000, -6000, -15000 + 5000, -16000 + 6000),
            "размер");

        cmd.Execute();

        Assert.Equal(-5000, line.StartMicronsX);
        Assert.Equal(-6000, line.StartMicronsY);
        Assert.Equal(-15000, line.EndMicronsX);
        Assert.Equal(-16000, line.EndMicronsY);
    }

    [Fact]
    public void LineConstructor_HorizontalLine_ExecuteSetsCorrectCoordinates()
    {
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(line.StartMicronsX, line.StartMicronsY,
                line.EndMicronsX - line.StartMicronsX, line.EndMicronsY - line.StartMicronsY),
            s => line.ApplyResize(s),
            new ResizeState(0, 5000, 10000 - 0, 5000 - 5000),
            "размер");

        cmd.Execute();

        Assert.Equal(0, line.StartMicronsX);
        Assert.Equal(5000, line.StartMicronsY);
        Assert.Equal(10000, line.EndMicronsX);
        Assert.Equal(5000, line.EndMicronsY);
    }

    [Fact]
    public void LineConstructor_VerticalLine_ExecuteSetsCorrectCoordinates()
    {
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(line.StartMicronsX, line.StartMicronsY,
                line.EndMicronsX - line.StartMicronsX, line.EndMicronsY - line.StartMicronsY),
            s => line.ApplyResize(s),
            new ResizeState(5000, 0, 5000 - 5000, 10000 - 0),
            "размер");

        cmd.Execute();

        Assert.Equal(5000, line.StartMicronsX);
        Assert.Equal(0, line.StartMicronsY);
        Assert.Equal(5000, line.EndMicronsX);
        Assert.Equal(10000, line.EndMicronsY);
    }
}
