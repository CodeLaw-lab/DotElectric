namespace DotElectric.TemplateEditor.Commands;

/// <summary>
/// Интерфейс команды для системы Undo/Redo.
/// Отличается от System.Windows.Input.ICommand.
/// </summary>
public interface IUndoCommand
{
    /// <summary>
    /// Выполнить команду.
    /// </summary>
    void Execute();

    /// <summary>
    /// Отменить команду (вернуть предыдущее состояние).
    /// </summary>
    void Undo();

    /// <summary>
    /// Человеко-читаемое имя команды (для отображения в UI).
    /// </summary>
    string Name { get; }
}
