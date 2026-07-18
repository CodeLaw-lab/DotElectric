namespace DotElectric.TemplateEditor.Commands;

/// <summary>
/// Generic-команда изменения любого свойства объекта.
/// </summary>
/// <typeparam name="T">Тип свойства.</typeparam>
public sealed class ChangePropertyCommand<T> : IUndoCommand
{
    private readonly Action<T> _setter;
    private readonly T _oldValue;
    private readonly T _newValue;
    private readonly string _propertyName;
    private readonly Action? _markDirty;

    /// <inheritdoc/>
    public string Name => $"Изменить {_propertyName}";

    /// <summary>
    /// Создаёт команду изменения свойства, захватывая текущее значение через getter.
    /// </summary>
    public ChangePropertyCommand(
        Func<T> getter,
        Action<T> setter,
        T newValue,
        string propertyName,
        Action? markDirty = null)
    {
        if (getter == null) throw new ArgumentNullException(nameof(getter));
        _setter = setter ?? throw new ArgumentNullException(nameof(setter));
        _oldValue = getter();
        _newValue = newValue;
        _propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        _markDirty = markDirty;
    }

    /// <summary>
    /// Создаёт команду изменения свойства с явно заданным старым значением.
    /// Используется когда объект уже изменён (например, при drag в SelectTool).
    /// </summary>
    public ChangePropertyCommand(
        T oldValue,
        Action<T> setter,
        T newValue,
        string propertyName,
        Action? markDirty = null)
    {
        _setter = setter ?? throw new ArgumentNullException(nameof(setter));
        _oldValue = oldValue;
        _newValue = newValue;
        _propertyName = propertyName ?? throw new ArgumentNullException(nameof(propertyName));
        _markDirty = markDirty;
    }

    /// <inheritdoc/>
    public void Execute()
    {
        _setter(_newValue);
        _markDirty?.Invoke();
    }

    /// <inheritdoc/>
    public void Undo()
    {
        _setter(_oldValue);
        _markDirty?.Invoke();
    }
}
