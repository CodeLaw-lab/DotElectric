using DotElectric.Sheets;

namespace DotElectric.Document;

/// <summary>
/// Дескриптор типа объекта формата .tdel — единственная точка знания на тип:
/// строка идентичности, маппинг «запись ↔ модель» и объектные правила проверки
/// документа. Зависимости проверки (лист, сервис проверки цвета) передаются
/// аргументами вызова от валидатора. Новый тип объекта = один дескриптор
/// в <see cref="ObjectTypeCatalog"/>.
/// </summary>
public sealed class ObjectTypeDescriptor
{
    public ObjectTypeDescriptor(
        string typeName,
        Type modelType,
        Func<ObjectDto, TemplateObjectBase> fromDto,
        Action<TemplateObjectBase, ObjectDto> writeToDto,
        Func<TemplateObjectBase, Sheet, IValidationService, IEnumerable<ValidationError>> validate)
    {
        TypeName = typeName;
        ModelType = modelType;
        FromDto = fromDto;
        WriteToDto = writeToDto;
        Validate = validate;
    }

    /// <summary>
    /// Строка идентичности типа в файле (XML-элемент &lt;Type&gt;).
    /// </summary>
    public string TypeName { get; }

    /// <summary>
    /// Тип модели, соответствующий дескриптору.
    /// </summary>
    public Type ModelType { get; }

    /// <summary>
    /// Фабрика «запись → модель». Дефолты отсутствующих полей записи —
    /// из единого источника (<see cref="DocumentDefaults"/>); отсутствующий
    /// или пустой идентификатор — новый.
    /// </summary>
    public Func<ObjectDto, TemplateObjectBase> FromDto { get; }

    /// <summary>
    /// Действие «модель → запись»: пишет строку типа и поля модели в запись.
    /// Идентификатор записи устанавливается вызывающим до вызова.
    /// </summary>
    public Action<TemplateObjectBase, ObjectDto> WriteToDto { get; }

    /// <summary>
    /// Объектные правила проверки документа в порядке V-003 → V-004 → V-007 → V-005.
    /// </summary>
    public Func<TemplateObjectBase, Sheet, IValidationService, IEnumerable<ValidationError>> Validate { get; }
}
