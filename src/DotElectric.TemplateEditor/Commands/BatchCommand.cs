using System.Collections.Generic;
using System.Linq;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Commands;

/// <summary>
/// Составная команда — группирует несколько команд в одну.
/// Undo выполняется в обратном (LIFO) порядке.
/// </summary>
public sealed class BatchCommand : IUndoCommand
{
    private readonly List<IUndoCommand> _commands;
    private readonly string _name;
    private readonly Action? _markDirty;

    /// <inheritdoc/>
    public string Name => _name;

    /// <summary>
    /// Создаёт составную команду.
    /// </summary>
    /// <param name="commands">Список команд для выполнения.</param>
    /// <param name="name">Имя составной команды (для отображения в UI).</param>
    public BatchCommand(IEnumerable<IUndoCommand> commands, string name = "Групповая операция")
    {
        if (commands == null) throw new ArgumentNullException(nameof(commands));
        _commands = commands.ToList();
        if (_commands.Count == 0)
            throw new ArgumentException("Список команд не может быть пустым.", nameof(commands));
        _name = name;
    }

    /// <summary>
    /// Создаёт составную команду с callback для markDirty.
    /// </summary>
    /// <param name="commands">Список команд для выполнения.</param>
    /// <param name="name">Имя составной команды (для отображения в UI).</param>
    /// <param name="markDirty">Callback для пометки шаблона как изменённого.</param>
    public BatchCommand(IEnumerable<IUndoCommand> commands, string name, Action? markDirty)
        : this(commands, name)
    {
        _markDirty = markDirty;
    }

    /// <summary>
    /// Получить объекты, которые будут восстановлены внутренними DeleteObjectCommand.
    /// </summary>
    public IReadOnlyList<TemplateObjectBase> GetRestoredObjects()
    {
        var result = new List<TemplateObjectBase>();
        foreach (var cmd in _commands)
        {
            if (cmd is DeleteObjectCommand deleteCmd)
                result.Add(deleteCmd.Object);
        }
        return result.AsReadOnly();
    }

    /// <inheritdoc/>
    public void Execute()
    {
        foreach (var command in _commands)
            command.Execute();
        _markDirty?.Invoke();
    }

    /// <inheritdoc/>
    public void Undo()
    {
        // Undo в обратном (LIFO) порядке
        for (var i = _commands.Count - 1; i >= 0; i--)
            _commands[i].Undo();
        _markDirty?.Invoke();
    }
}
