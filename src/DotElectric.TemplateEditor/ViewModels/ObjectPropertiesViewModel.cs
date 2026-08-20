using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DotElectric.TemplateEditor.Commands;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.ViewModels;

/// <summary>
/// Базовая ViewModel панели свойств объекта шаблона.
/// Забирает общую механику панелей: жизненный цикл подписки на модель
/// (отписка → присвоение → подписка → notify-all), изменение свойства
/// с валидацией и undo-командой, парсинг значений из миллиметров.
/// Наследник добавляет только именованные свойства (контракт XAML),
/// команды изменения и декларативную карту пересылки
/// «свойство модели → свойство VM».
/// </summary>
/// <typeparam name="TObject">Тип объекта шаблона.</typeparam>
public abstract class ObjectPropertiesViewModel<TObject> : ObservableObject, IDisposable
    where TObject : TemplateObjectBase
{
    private readonly CommandHistory? _commandHistory;
    private readonly Action<string?> _setValidationError;

    private TObject? _currentObject;

    protected ObjectPropertiesViewModel(
        CommandHistory? commandHistory,
        Action<string?> setValidationError)
    {
        _commandHistory = commandHistory;
        _setValidationError = setValidationError;
    }

    /// <summary>
    /// Текущий объект панели, либо null, если выделение пустое или другого типа.
    /// </summary>
    protected TObject? CurrentObject => _currentObject;

    /// <summary>
    /// Карта пересылки: имя свойства модели → имя свойства VM (только nameof).
    /// Используется и для диспетчеризации PropertyChanged модели,
    /// и для notify-all в <see cref="UpdateObject"/>.
    /// </summary>
    protected abstract IReadOnlyDictionary<string, string> PropertyMap { get; }

    /// <summary>
    /// Отписывается от текущего объекта и очищает ссылку на него.
    /// </summary>
    public void Dispose()
    {
        if (_currentObject is not null)
            _currentObject.PropertyChanged -= OnModelPropertyChanged;
        _currentObject = null;
    }

    /// <summary>
    /// Переключает панель на другой объект: отписка от старого,
    /// присвоение нового, подписка, уведомление всех свойств из карты.
    /// </summary>
    public void UpdateObject(TObject? modelObject)
    {
        if (_currentObject is not null)
            _currentObject.PropertyChanged -= OnModelPropertyChanged;

        _currentObject = modelObject;

        if (_currentObject is not null)
            _currentObject.PropertyChanged += OnModelPropertyChanged;

        foreach (var vmProperty in PropertyMap.Values)
            OnPropertyChanged(vmProperty);
    }

    /// <summary>
    /// Изменяет свойство модели через undo-команду.
    /// При ошибке валидации значение не попадает в историю и уведомление не отправляется.
    /// </summary>
    protected void SetProperty<T>(T value, Func<T> getter, Action<T> setter,
        Func<T, string?>? validator, string propertyName, string commandName, Action? afterSet = null)
    {
        if (validator != null)
        {
            var error = validator(value);
            if (error != null) { _setValidationError(error); return; }
        }
        var cmd = new ChangePropertyCommand<T>(getter, setter, value, commandName);
        _commandHistory?.Push(cmd);
        OnPropertyChanged(propertyName);
        afterSet?.Invoke();
    }

    /// <summary>
    /// Парсит значение в миллиметрах (InvariantCulture) и передаёт его в setter в микронах.
    /// Непарсируемые строки игнорируются.
    /// </summary>
    protected void ChangeFromMmString(string? value, Action<long> setter)
    {
        if (double.TryParse(value, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var mm))
            setter(Coordinate.ToMicrons(mm));
    }

    /// <summary>
    /// Парсит русское имя типа линии; неизвестные значения → Solid.
    /// </summary>
    protected static LineType ParseLineType(string? value)
    {
        return value switch
        {
            "Сплошная" => LineType.Solid,
            "Штриховая" => LineType.Dashed,
            "Штрихпунктирная" => LineType.DashDot,
            "Штрихпунктирная с двумя штрихами" => LineType.DashDotDot,
            _ => LineType.Solid
        };
    }

    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not null && PropertyMap.TryGetValue(e.PropertyName, out var vmProperty))
            OnPropertyChanged(vmProperty);
    }
}
