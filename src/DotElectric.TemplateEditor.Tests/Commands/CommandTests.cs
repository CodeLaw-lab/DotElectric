using System.Collections.ObjectModel;
using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Tests.Commands;

public class CommandTests
{
    // ===== AddObjectCommand =====

    [Fact]
    public void AddObjectCommand_Execute_AddsObjectToCollection()
    {
        var collection = new ObservableCollection<TemplateObjectBase>();
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new AddObjectCommand(collection, line);

        cmd.Execute();

        Assert.Single(collection);
        Assert.Same(line, collection[0]);
    }

    [Fact]
    public void AddObjectCommand_Undo_RemovesObjectFromCollection()
    {
        var collection = new ObservableCollection<TemplateObjectBase>();
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new AddObjectCommand(collection, line);
        cmd.Execute();

        cmd.Undo();

        Assert.Empty(collection);
    }

    [Fact]
    public void AddObjectCommand_Name_IsCorrect()
    {
        var collection = new ObservableCollection<TemplateObjectBase>();
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new AddObjectCommand(collection, line);
        Assert.Equal("Добавить объект", cmd.Name);
    }

    // ===== DeleteObjectCommand =====

    [Fact]
    public void DeleteObjectCommand_Execute_RemovesObjectFromCollection()
    {
        var collection = new ObservableCollection<TemplateObjectBase>();
        var line = new Line(0, 0, 1000, 1000);
        collection.Add(line);
        var cmd = new DeleteObjectCommand(collection, line);

        cmd.Execute();

        Assert.Empty(collection);
    }

    [Fact]
    public void DeleteObjectCommand_Undo_RestoresObjectToCollection()
    {
        var collection = new ObservableCollection<TemplateObjectBase>();
        var line = new Line(0, 0, 1000, 1000);
        collection.Add(line);
        var cmd = new DeleteObjectCommand(collection, line);
        cmd.Execute();

        cmd.Undo();

        Assert.Single(collection);
        Assert.Same(line, collection[0]);
    }

    [Fact]
    public void DeleteObjectCommand_Name_IsCorrect()
    {
        var collection = new ObservableCollection<TemplateObjectBase>();
        var line = new Line(0, 0, 1000, 1000);
        collection.Add(line);
        var cmd = new DeleteObjectCommand(collection, line);
        Assert.Equal("Удалить объект", cmd.Name);
    }

    // ===== Move via ChangePropertyCommand<(long,long)> =====

    [Fact]
    public void ChangePropertyCommand_Move_Execute_ChangesPosition()
    {
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<(long X, long Y)>(
            () => (line.MicronsX, line.MicronsY),
            v => line.Move(v.X, v.Y),
            (5000, 3000),
            "Переместить объект");

        cmd.Execute();

        Assert.Equal(5000, line.StartMicronsX);
        Assert.Equal(3000, line.StartMicronsY);
        Assert.Equal(6000, line.EndMicronsX);
        Assert.Equal(4000, line.EndMicronsY);
    }

    [Fact]
    public void ChangePropertyCommand_Move_Undo_RestoresOriginalPosition()
    {
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<(long X, long Y)>(
            () => (line.MicronsX, line.MicronsY),
            v => line.Move(v.X, v.Y),
            (5000, 3000),
            "Переместить объект");
        cmd.Execute();

        cmd.Undo();

        Assert.Equal(0, line.StartMicronsX);
        Assert.Equal(0, line.StartMicronsY);
        Assert.Equal(1000, line.EndMicronsX);
        Assert.Equal(1000, line.EndMicronsY);
    }

    [Fact]
    public void ChangePropertyCommand_Move_Name_IsCorrect()
    {
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<(long X, long Y)>(
            () => (line.MicronsX, line.MicronsY),
            v => line.Move(v.X, v.Y),
            (5000, 3000),
            "Переместить объект");
        Assert.Equal("Изменить Переместить объект", cmd.Name);
    }

    // ===== Resize via ChangePropertyCommand<ResizeState> =====

    [Fact]
    public void ChangePropertyCommand_Resize_Execute_ChangesSize()
    {
        var rect = new Rectangle(0, 0, 1000, 1000);
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
    public void ChangePropertyCommand_Resize_Undo_RestoresOriginalSize()
    {
        var rect = new Rectangle(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(rect.MicronsX, rect.MicronsY, rect.WidthMicrons, rect.HeightMicrons),
            s => rect.ApplyResize(s),
            new ResizeState(rect.MicronsX, rect.MicronsY, 5000, 3000),
            "размер");
        cmd.Execute();

        cmd.Undo();

        Assert.Equal(1000, rect.WidthMicrons);
        Assert.Equal(1000, rect.HeightMicrons);
    }

    [Fact]
    public void ChangePropertyCommand_Resize_Name_IsCorrect()
    {
        var rect = new Rectangle(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<ResizeState>(
            new ResizeState(rect.MicronsX, rect.MicronsY, rect.WidthMicrons, rect.HeightMicrons),
            s => rect.ApplyResize(s),
            new ResizeState(rect.MicronsX, rect.MicronsY, 5000, 3000),
            "размер");
        Assert.Equal("Изменить размер", cmd.Name);
    }

    // ===== ChangePropertyCommand =====

    [Fact]
    public void ChangePropertyCommand_Execute_ChangesProperty()
    {
        var line = new Line(0, 0, 1000, 1000, LineType.Solid);
        var cmd = new ChangePropertyCommand<LineType>(
            () => line.LineType,
            v => line.LineType = v,
            LineType.Dashed,
            "LineType");

        cmd.Execute();

        Assert.Equal(LineType.Dashed, line.LineType);
    }

    [Fact]
    public void ChangePropertyCommand_Undo_RestoresOriginalValue()
    {
        var line = new Line(0, 0, 1000, 1000, LineType.Solid);
        var cmd = new ChangePropertyCommand<LineType>(
            () => line.LineType,
            v => line.LineType = v,
            LineType.Dashed,
            "LineType");
        cmd.Execute();

        cmd.Undo();

        Assert.Equal(LineType.Solid, line.LineType);
    }

    [Fact]
    public void ChangePropertyCommand_Name_IsCorrect()
    {
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<LineType>(
            () => line.LineType,
            v => line.LineType = v,
            LineType.Dashed,
            "LineType");
        Assert.Equal("Изменить LineType", cmd.Name);
    }

    // ===== Rotate via ChangePropertyCommand<int> =====

    [Fact]
    public void ChangePropertyCommand_Rotate_Execute_Plus90_ChangesAngle()
    {
        var text = new Text(0, 0, "Test", 2500, rotationAngle: 0);
        var cmd = new ChangePropertyCommand<int>(
            () => text.RotationAngle,
            v => text.RotationAngle = v,
            90,
            "Повернуть текст (+90°)");

        cmd.Execute();

        Assert.Equal(90, text.RotationAngle);
    }

    [Fact]
    public void ChangePropertyCommand_Rotate_Execute_Minus90_ChangesAngle()
    {
        var text = new Text(0, 0, "Test", 2500, rotationAngle: 0);
        var cmd = new ChangePropertyCommand<int>(
            () => text.RotationAngle,
            v => text.RotationAngle = v,
            -90,
            "Повернуть текст (-90°)");

        cmd.Execute();

        Assert.Equal(270, text.RotationAngle);
    }

    [Fact]
    public void ChangePropertyCommand_Rotate_Undo_RestoresOriginalAngle()
    {
        var text = new Text(0, 0, "Test", 2500, rotationAngle: 90);
        var cmd = new ChangePropertyCommand<int>(
            () => text.RotationAngle,
            v => text.RotationAngle = v,
            180,
            "Повернуть текст (+90°)");
        cmd.Execute();

        cmd.Undo();

        Assert.Equal(90, text.RotationAngle);
    }

    [Fact]
    public void ChangePropertyCommand_Rotate_Name_IsCorrect()
    {
        var text = new Text(0, 0, "Test", 2500, rotationAngle: 0);
        var cmd = new ChangePropertyCommand<int>(
            () => text.RotationAngle,
            v => text.RotationAngle = v,
            90,
            "Повернуть текст (+90°)");
        Assert.Equal("Изменить Повернуть текст (+90°)", cmd.Name);
    }

    // ===== BatchCommand =====

    [Fact]
    public void BatchCommand_Execute_ExecutesAllSubCommands()
    {
        var collection = new ObservableCollection<TemplateObjectBase>();
        var line1 = new Line(0, 0, 1000, 1000);
        var line2 = new Line(0, 0, 2000, 2000);
        var commands = new List<IUndoCommand>
        {
            new AddObjectCommand(collection, line1),
            new AddObjectCommand(collection, line2)
        };
        var batch = new BatchCommand(commands, "Добавить несколько");

        batch.Execute();

        Assert.Equal(2, collection.Count);
    }

    [Fact]
    public void BatchCommand_Undo_UndoInReverseOrder()
    {
        var collection = new ObservableCollection<TemplateObjectBase>();
        var line1 = new Line(0, 0, 1000, 1000);
        var line2 = new Line(0, 0, 2000, 2000);
        var cmd1 = new AddObjectCommand(collection, line1);
        var cmd2 = new AddObjectCommand(collection, line2);
        var commands = new List<IUndoCommand> { cmd1, cmd2 };
        var batch = new BatchCommand(commands, "Добавить несколько");
        batch.Execute();

        batch.Undo();

        // Undo в обратном порядке: cmd2, затем cmd1
        Assert.Empty(collection);
    }

    [Fact]
    public void BatchCommand_Name_IsCorrect()
    {
        var commands = new List<IUndoCommand> { new TestMockCommand() };
        var batch = new BatchCommand(commands, "Test Batch");
        Assert.Equal("Test Batch", batch.Name);
    }

    [Fact]
    public void BatchCommand_EmptyList_ThrowsArgumentException()
    {
        var commands = new List<IUndoCommand>();
        Assert.Throws<ArgumentException>(() => new BatchCommand(commands, "Empty"));
    }

    [Fact]
    public void BatchCommand_WithMarkDirty_CallsMarkDirtyOnExecute()
    {
        var markDirtyCalled = false;
        var commands = new List<IUndoCommand> { new TestMockCommand() };
        var batch = new BatchCommand(commands, "Test", () => markDirtyCalled = true);

        batch.Execute();

        Assert.True(markDirtyCalled);
    }

    [Fact]
    public void BatchCommand_WithMarkDirty_CallsMarkDirtyOnUndo()
    {
        var markDirtyCalled = false;
        var commands = new List<IUndoCommand> { new TestMockCommand() };
        var batch = new BatchCommand(commands, "Test", () => markDirtyCalled = true);

        batch.Undo();

        Assert.True(markDirtyCalled);
    }

    // ===== Move via ChangePropertyCommand<(long,long)> с явными координатами =====

    [Fact]
    public void ChangePropertyCommand_Move_ExplicitConstructor_ExecuteMovesToNewPosition()
    {
        var rect = new Rectangle(10000, 10000, 5000, 5000);
        var cmd = new ChangePropertyCommand<(long X, long Y)>(
            (10000, 10000),
            v => { rect.MicronsX = v.X; rect.MicronsY = v.Y; },
            (15000, 13000),
            "Переместить объект");

        cmd.Execute();

        Assert.Equal(15000, rect.MicronsX);
        Assert.Equal(13000, rect.MicronsY);
    }

    [Fact]
    public void ChangePropertyCommand_Move_ExplicitConstructor_UndoRestoresOldPosition()
    {
        var rect = new Rectangle(10000, 10000, 5000, 5000);
        var cmd = new ChangePropertyCommand<(long X, long Y)>(
            (10000, 10000),
            v => { rect.MicronsX = v.X; rect.MicronsY = v.Y; },
            (15000, 13000),
            "Переместить объект");
        cmd.Execute();

        cmd.Undo();

        Assert.Equal(10000, rect.MicronsX);
        Assert.Equal(10000, rect.MicronsY);
    }

    [Fact]
    public void ChangePropertyCommand_Move_ExplicitConstructor_CallsMarkDirtyOnExecute()
    {
        var markDirtyCalled = false;
        var rect = new Rectangle(10000, 10000, 5000, 5000);
        var cmd = new ChangePropertyCommand<(long X, long Y)>(
            (10000, 10000),
            v => { rect.MicronsX = v.X; rect.MicronsY = v.Y; },
            (15000, 13000),
            "Переместить объект",
            () => markDirtyCalled = true);

        cmd.Execute();

        Assert.True(markDirtyCalled);
    }

    [Fact]
    public void ChangePropertyCommand_Move_ExplicitConstructor_CallsMarkDirtyOnUndo()
    {
        var markDirtyCalled = false;
        var rect = new Rectangle(10000, 10000, 5000, 5000);
        var cmd = new ChangePropertyCommand<(long X, long Y)>(
            (10000, 10000),
            v => { rect.MicronsX = v.X; rect.MicronsY = v.Y; },
            (15000, 13000),
            "Переместить объект",
            () => markDirtyCalled = true);
        cmd.Execute();
        markDirtyCalled = false;

        cmd.Undo();

        Assert.True(markDirtyCalled);
    }

    // ===== Tests from AdditionalCommandTests =====

    [Fact]
    public void BatchCommand_Execute_RunsAllSubCommands()
    {
        var objects = new ObservableCollection<TemplateObjectBase>();
        var cmd1 = new AddObjectCommand(objects, new Line(0, 0, 1000, 1000));
        var cmd2 = new AddObjectCommand(objects, new Rectangle(0, 0, 2000, 2000));

        var batch = new BatchCommand(new[] { cmd1, cmd2 });
        batch.Execute();

        Assert.Equal(2, objects.Count);
    }

    [Fact]
    public void BatchCommand_EmptyArray_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new BatchCommand(Array.Empty<IUndoCommand>()));
    }

    [Fact]
    public void BatchCommand_Name_IsGroupOperation()
    {
        var cmd1 = new AddObjectCommand(new ObservableCollection<TemplateObjectBase>(), new Line(0, 0, 1000, 1000));
        var cmd2 = new AddObjectCommand(new ObservableCollection<TemplateObjectBase>(), new Rectangle(0, 0, 2000, 2000));

        var batch = new BatchCommand(new[] { cmd1, cmd2 });
        Assert.Contains("Групповая", batch.Name);
    }

    [Fact]
    public void ChangeProperty_Execute_UpdatesProperty()
    {
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<long>(
            () => line.StartMicronsX,
            v => line.StartMicronsX = v,
            5000,
            "Change StartX");

        cmd.Execute();
        Assert.Equal(5000, line.StartMicronsX);
    }

    [Fact]
    public void ChangeProperty_Undo_RestoresOldValue()
    {
        var line = new Line(1000, 0, 2000, 1000);
        var cmd = new ChangePropertyCommand<long>(
            () => line.StartMicronsX,
            v => line.StartMicronsX = v,
            5000,
            "Change StartX");

        cmd.Execute();
        cmd.Undo();
        Assert.Equal(1000, line.StartMicronsX);
    }

    [Fact]
    public void ChangeProperty_Name_ContainsCustomName()
    {
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<long>(
            () => line.StartMicronsX,
            v => line.StartMicronsX = v,
            5000,
            "Custom Name");

        Assert.Contains("Custom Name", cmd.Name);
    }

    [Fact]
    public void ResizeObject_Execute_UpdatesSize()
    {
        var rect = new Rectangle(0, 0, 1000, 1000);
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
    public void ResizeObject_Undo_RestoresOldSize()
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
    public void PasteObject_Execute_AddsObject()
    {
        var objects = new ObservableCollection<TemplateObjectBase>();
        var line = new Line(0, 0, 1000, 1000);

        var cmd = new AddObjectCommand(objects, line, nameOverride: "Вставить объект");
        cmd.Execute();

        Assert.Single(objects);
        Assert.Same(line, objects[0]);
    }

    [Fact]
    public void PasteObject_Undo_RemovesPasted()
    {
        var objects = new ObservableCollection<TemplateObjectBase>();
        var line = new Line(0, 0, 1000, 1000);

        var cmd = new AddObjectCommand(objects, line, nameOverride: "Вставить объект");
        cmd.Execute();
        cmd.Undo();

        Assert.Empty(objects);
    }

    [Fact]
    public void RotateObject_Execute_UpdatesAngle()
    {
        var text = new Text(0, 0, "Test", 3500, rotationAngle: 0);
        var cmd = new ChangePropertyCommand<int>(
            () => text.RotationAngle,
            v => text.RotationAngle = v,
            90,
            "Повернуть текст (+90°)");

        cmd.Execute();
        Assert.Equal(90, text.RotationAngle);
    }

    [Fact]
    public void RotateObject_Undo_RestoresOldAngle()
    {
        var text = new Text(0, 0, "Test", 3500, rotationAngle: 90);
        var cmd = new ChangePropertyCommand<int>(
            () => text.RotationAngle,
            v => text.RotationAngle = v,
            180,
            "Повернуть текст (+90°)");

        cmd.Execute();
        cmd.Undo();
        Assert.Equal(90, text.RotationAngle);
    }

    [Fact]
    public void RotateObject_AnyAngle_Accepts()
    {
        var text = new Text(0, 0, "Test", 3500, rotationAngle: 0);
        var cmd = new ChangePropertyCommand<int>(
            () => text.RotationAngle,
            v => text.RotationAngle = v,
            45,
            "Повернуть текст (+45°)");
        cmd.Execute();
        Assert.Equal(45, text.RotationAngle);
    }

    [Fact]
    public void RotateObject_NormalizesAngle()
    {
        var text = new Text(0, 0, "Test", 3500, rotationAngle: 270);
        var cmd = new ChangePropertyCommand<int>(
            () => text.RotationAngle,
            v => text.RotationAngle = v,
            90,
            "Повернуть текст (+90°)");

        cmd.Execute();
        Assert.Equal(90, text.RotationAngle);
    }

    [Fact]
    public void MoveObject_Execute_UpdatesPosition()
    {
        var line = new Line(0, 0, 1000, 1000);
        var cmd = new ChangePropertyCommand<(long X, long Y)>(
            () => (line.MicronsX, line.MicronsY),
            v => line.Move(v.X, v.Y),
            (5000, 6000),
            "Переместить объект");

        cmd.Execute();
        Assert.Equal(5000, line.StartMicronsX);
        Assert.Equal(6000, line.StartMicronsY);
        Assert.Equal(6000, line.EndMicronsX);
        Assert.Equal(7000, line.EndMicronsY);
    }

    [Fact]
    public void MoveObject_Undo_RestoresOldPosition()
    {
        var line = new Line(1000, 2000, 3000, 4000);
        var cmd = new ChangePropertyCommand<(long X, long Y)>(
            () => (line.MicronsX, line.MicronsY),
            v => line.Move(v.X, v.Y),
            (5000, 6000),
            "Переместить объект");

        cmd.Execute();
        cmd.Undo();
        Assert.Equal(1000, line.StartMicronsX);
        Assert.Equal(2000, line.StartMicronsY);
        Assert.Equal(3000, line.EndMicronsX);
        Assert.Equal(4000, line.EndMicronsY);
    }

    [Fact]
    public void DeleteObject_Execute_RemovesObject()
    {
        var objects = new ObservableCollection<TemplateObjectBase>();
        var line = new Line(0, 0, 1000, 1000);
        objects.Add(line);

        var cmd = new DeleteObjectCommand(objects, line);
        cmd.Execute();
        Assert.Empty(objects);
    }

    [Fact]
    public void DeleteObject_Undo_RestoresObject()
    {
        var objects = new ObservableCollection<TemplateObjectBase>();
        var line = new Line(0, 0, 1000, 1000);
        objects.Add(line);

        var cmd = new DeleteObjectCommand(objects, line);
        cmd.Execute();
        cmd.Undo();
        Assert.Single(objects);
        Assert.Same(line, objects[0]);
    }

    // ===== AddObjectCommand null guards =====

    [Fact]
    public void AddObjectCommand_Constructor_NullCollection_ThrowsArgumentNullException()
    {
        var line = new Line(0, 0, 1000, 1000);
        Assert.Throws<ArgumentNullException>(() => new AddObjectCommand(null!, line));
    }

    [Fact]
    public void AddObjectCommand_Constructor_NullObject_ThrowsArgumentNullException()
    {
        var collection = new ObservableCollection<TemplateObjectBase>();
        Assert.Throws<ArgumentNullException>(() => new AddObjectCommand(collection, null!));
    }

    // ===== DeleteObjectCommand edge cases =====

    [Fact]
    public void DeleteObjectCommand_Constructor_NullCollection_ThrowsArgumentNullException()
    {
        var line = new Line(0, 0, 1000, 1000);
        Assert.Throws<ArgumentNullException>(() => new DeleteObjectCommand(null!, line));
    }

    [Fact]
    public void DeleteObjectCommand_Constructor_NullObject_ThrowsArgumentNullException()
    {
        var collection = new ObservableCollection<TemplateObjectBase>();
        Assert.Throws<ArgumentNullException>(() => new DeleteObjectCommand(collection, null!));
    }

    [Fact]
    public void DeleteObjectCommand_Undo_BeforeExecute_DoesNotThrow()
    {
        var collection = new ObservableCollection<TemplateObjectBase>();
        var line = new Line(0, 0, 1000, 1000);
        collection.Add(line);
        var cmd = new DeleteObjectCommand(collection, line);

        var exception = Record.Exception(() => cmd.Undo());
        Assert.Null(exception);
    }

    [Fact]
    public void DeleteObjectCommand_Execute_ObjectNotInCollection_DoesNotThrow()
    {
        var collection = new ObservableCollection<TemplateObjectBase>();
        var line = new Line(0, 0, 1000, 1000);
        // line is intentionally NOT added to collection
        var cmd = new DeleteObjectCommand(collection, line);

        var exception = Record.Exception(() => cmd.Execute());
        Assert.Null(exception);
    }

    // ===== ChangePropertyCommand<T> null guards =====

    [Fact]
    public void ChangePropertyCommand_Constructor_NullGetter_ThrowsArgumentNullException()
    {
        var line = new Line(0, 0, 1000, 1000);
        Assert.Throws<ArgumentNullException>(() =>
            new ChangePropertyCommand<long>(
                null!,
                v => line.StartMicronsX = v,
                5000,
                "Test"));
    }

    [Fact]
    public void ChangePropertyCommand_Constructor_NullSetter_ThrowsArgumentNullException()
    {
        var line = new Line(0, 0, 1000, 1000);
        Assert.Throws<ArgumentNullException>(() =>
            new ChangePropertyCommand<long>(
                () => line.StartMicronsX,
                null!,
                5000,
                "Test"));
    }

    [Fact]
    public void ChangePropertyCommand_Constructor_NullPropertyName_ThrowsArgumentNullException()
    {
        var line = new Line(0, 0, 1000, 1000);
        Assert.Throws<ArgumentNullException>(() =>
            new ChangePropertyCommand<long>(
                () => line.StartMicronsX,
                v => line.StartMicronsX = v,
                5000,
                null!));
    }

    [Fact]
    public void ChangePropertyCommand_SecondConstructor_NullSetter_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ChangePropertyCommand<long>(
                1000L,
                null!,
                5000L,
                "Test"));
    }

    [Fact]
    public void ChangePropertyCommand_SecondConstructor_NullPropertyName_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ChangePropertyCommand<long>(
                1000L,
                v => { },
                5000L,
                null!));
    }

    // ===== BatchCommand edge cases =====

    [Fact]
    public void BatchCommand_Constructor_NullCommands_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new BatchCommand(null!));
    }

    [Fact]
    public void BatchCommand_GetRestoredObjects_ReturnsDeleteObjectCommandObjects()
    {
        var collection = new ObservableCollection<TemplateObjectBase>();
        var line = new Line(0, 0, 1000, 1000);
        collection.Add(line);
        var deleteCmd = new DeleteObjectCommand(collection, line);
        var batch = new BatchCommand(new[] { deleteCmd }, "Test");

        var restored = batch.GetRestoredObjects();

        Assert.Single(restored);
        Assert.Same(line, restored[0]);
    }

    [Fact]
    public void BatchCommand_GetRestoredObjects_NoDeleteCommands_ReturnsEmpty()
    {
        var collection = new ObservableCollection<TemplateObjectBase>();
        var line = new Line(0, 0, 1000, 1000);
        var addCmd = new AddObjectCommand(collection, line);
        var batch = new BatchCommand(new[] { addCmd }, "Test");

        var restored = batch.GetRestoredObjects();

        Assert.Empty(restored);
    }

    [Fact]
    public void BatchCommand_GetRestoredObjects_MixedCommands_ReturnsOnlyDeleteCommands()
    {
        var collection = new ObservableCollection<TemplateObjectBase>();
        var line1 = new Line(0, 0, 1000, 1000);
        var line2 = new Line(0, 0, 2000, 2000);
        collection.Add(line1);
        collection.Add(line2);
        var deleteCmd = new DeleteObjectCommand(collection, line1);
        var addCmd = new AddObjectCommand(collection, new Rectangle(0, 0, 500, 500));
        var batch = new BatchCommand(new IUndoCommand[] { deleteCmd, addCmd }, "Test");

        var restored = batch.GetRestoredObjects();

        Assert.Single(restored);
        Assert.Same(line1, restored[0]);
    }

    [Fact]
    public void BatchCommand_Undo_BeforeExecute_DoesNotThrow()
    {
        var collection = new ObservableCollection<TemplateObjectBase>();
        var line = new Line(0, 0, 1000, 1000);
        var addCmd = new AddObjectCommand(collection, line);
        var batch = new BatchCommand(new[] { addCmd }, "Test");

        var exception = Record.Exception(() => batch.Undo());
        Assert.Null(exception);
        Assert.Empty(collection);
    }

    private class TestMockCommand : IUndoCommand
    {
        public string Name => "Mock Command";
        public void Execute() { }
        public void Undo() { }
    }
}
