using System.Collections.ObjectModel;

namespace DotElectric.TemplateEditor.Commands;

/// <summary>
/// Команда добавления объекта в коллекцию.
/// </summary>
public sealed class AddObjectCommand : IUndoCommand
{
    private readonly ObservableCollection<TemplateObjectBase> _objects;
    private readonly TemplateObjectBase _object;
    private readonly string _name;

    /// <inheritdoc/>
    public string Name => _name;

    /// <summary>
    /// Создаёт команду добавления объекта.
    /// </summary>
    public AddObjectCommand(
        ObservableCollection<TemplateObjectBase> objects,
        TemplateObjectBase obj,
        string? nameOverride = null)
    {
        _objects = objects ?? throw new ArgumentNullException(nameof(objects));
        _object = obj ?? throw new ArgumentNullException(nameof(obj));
        _name = nameOverride ?? "Добавить объект";
    }

    /// <inheritdoc/>
    public void Execute()
    {
        _objects.Add(_object);
    }

    /// <inheritdoc/>
    public void Undo()
    {
        _objects.Remove(_object);
    }
}
