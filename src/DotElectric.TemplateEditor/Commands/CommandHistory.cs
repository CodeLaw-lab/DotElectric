using DotElectric.TemplateEditor.Constants;

namespace DotElectric.TemplateEditor.Commands;

/// <summary>
/// История команд с поддержкой Undo/Redo.
/// Хранит отдельные стеки для undo и redo, автоматически обрезает oldest при превышении лимита.
/// </summary>
public sealed class CommandHistory
{
    /// <summary>
    /// Максимальное количество уровней Undo (по умолчанию 50).
    /// </summary>
    public int MaxLevels { get; }

    private readonly List<IUndoCommand> _undoStack = new();
    private readonly List<IUndoCommand> _redoStack = new();
    private readonly Action? _markDirty;

    /// <summary>
    /// Количество команд в undo-стеке.
    /// </summary>
    public int UndoCount => _undoStack.Count;

    /// <summary>
    /// Количество команд в redo-стеке.
    /// </summary>
    public int RedoCount => _redoStack.Count;

    /// <summary>
    /// Есть ли команды для отмены.
    /// </summary>
    public bool CanUndo => _undoStack.Count > 0;

    /// <summary>
    /// Есть ли команды для повтора.
    /// </summary>
    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>
    /// Создаёт историю команд с заданным лимитом уровней.
    /// </summary>
    /// <param name="maxLevels">Максимальное количество уровней Undo (по умолчанию 50).</param>
    /// <param name="markDirty">Callback для пометки шаблона как изменённого (опционально).</param>
    public CommandHistory(int maxLevels = EditorSettings.CommandHistoryMaxLevels, Action? markDirty = null)
    {
        if (maxLevels <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxLevels), "Максимальное количество уровней должно быть положительным.");

        MaxLevels = maxLevels;
        _markDirty = markDirty;
    }

    /// <summary>
    /// Выполнить команду и добавить в undo-стек.
    /// Redo-стек очищается.
    /// </summary>
    /// <param name="command">Команда для выполнения.</param>
    public void Push(IUndoCommand command)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));

        command.Execute();
        _undoStack.Add(command);
        _redoStack.Clear();
        TrimUndoStack();
        _markDirty?.Invoke();
    }

    /// <summary>
    /// Отменить последнюю команду.
    /// Команда перемещается из undo-стека в redo-стек.
    /// </summary>
    public void Undo()
    {
        if (!CanUndo) return;

        var command = _undoStack[_undoStack.Count - 1];
        _undoStack.RemoveAt(_undoStack.Count - 1);

        try
        {
            command.Undo();
        }
        catch (Exception ex)
        {
            // Rollback: возвращаем команду в undo-стек
            _undoStack.Add(command);
            throw new InvalidOperationException($"Ошибка при отмене команды '{command.Name}': {ex.Message}", ex);
        }

        _redoStack.Add(command);
    }

    /// <summary>
    /// Повторить последнюю отменённую команду.
    /// Команда перемещается из redo-стека в undo-стек.
    /// </summary>
    public void Redo()
    {
        if (!CanRedo) return;

        var command = _redoStack[_redoStack.Count - 1];
        _redoStack.RemoveAt(_redoStack.Count - 1);

        try
        {
            command.Execute();
        }
        catch (Exception ex)
        {
            // Rollback: возвращаем команду в redo-стек
            _redoStack.Add(command);
            throw new InvalidOperationException($"Ошибка при повторе команды '{command.Name}': {ex.Message}", ex);
        }

        _undoStack.Add(command);
        TrimUndoStack();
    }

    /// <summary>
    /// Очистить историю (undo и redo стеки).
    /// </summary>
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }

    /// <summary>
    /// Обрезать undo-стек до MaxLevels, удаляя oldest команды.
    /// </summary>
    private void TrimUndoStack()
    {
        while (_undoStack.Count > MaxLevels)
            _undoStack.RemoveAt(0);
    }

    /// <summary>
    /// Получить имя последней undo-команды (для отображения в UI).
    /// </summary>
    public string? LastUndoName => CanUndo ? _undoStack[_undoStack.Count - 1].Name : null;

    /// <summary>
    /// Посмотреть последнюю undo-команду без извлечения.
    /// </summary>
    public IUndoCommand? PeekUndo() => CanUndo ? _undoStack[_undoStack.Count - 1] : null;

    /// <summary>
    /// Получить имя последней redo-команды (для отображения в UI).
    /// </summary>
    public string? LastRedoName => CanRedo ? _redoStack[_redoStack.Count - 1].Name : null;
}
