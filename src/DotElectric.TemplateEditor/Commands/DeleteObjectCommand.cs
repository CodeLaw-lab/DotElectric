using System.Collections.ObjectModel;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Commands;

/// <summary>
/// Команда удаления объекта из коллекции.
/// </summary>
public sealed class DeleteObjectCommand : IUndoCommand
{
    private readonly ObservableCollection<TemplateObjectBase> _objects;
    private readonly TemplateObjectBase _object;
    private int? _executedIndex;

    /// <summary>
    /// Удалённый объект (доступен для восстановления выделения после Undo).
    /// </summary>
    public TemplateObjectBase Object => _object;

    /// <inheritdoc/>
    public string Name => "Удалить объект";

    /// <summary>
    /// Создаёт команду удаления объекта.
    /// </summary>
    /// <param name="objects">Коллекция объектов шаблона.</param>
    /// <param name="obj">Объект для удаления.</param>
    public DeleteObjectCommand(
        ObservableCollection<TemplateObjectBase> objects,
        TemplateObjectBase obj)
    {
        _objects = objects ?? throw new ArgumentNullException(nameof(objects));
        _object = obj ?? throw new ArgumentNullException(nameof(obj));
    }

    /// <inheritdoc/>
    public void Execute()
    {
        _executedIndex = _objects.IndexOf(_object);
        if (_executedIndex < 0)
        {
            _executedIndex = null;
            return;
        }
        _objects.Remove(_object);
    }

    /// <inheritdoc/>
    public void Undo()
    {
        if (_executedIndex == null)
            return;

        var insertAt = Math.Clamp(_executedIndex.Value, 0, _objects.Count);
        _objects.Insert(insertAt, _object);
    }
}
