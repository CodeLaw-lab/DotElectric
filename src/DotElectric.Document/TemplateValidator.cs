using DotElectric.Sheets;

namespace DotElectric.Document;

public sealed class TemplateValidator : ITemplateValidator
{
    private readonly IValidationService _validation;

    public TemplateValidator() : this(HexColorValidation.Default) { }

    public TemplateValidator(IValidationService validation)
    {
        _validation = validation;
    }

    public IEnumerable<ValidationError> Validate(Template template)
    {
        if (template == null)
        {
            yield return new ValidationError("V-000", "Шаблон не может быть null.");
            yield break;
        }

        if (template.Sheet == null)
        {
            yield return new ValidationError("V-006", "Параметры листа не заданы.");
            yield break;
        }

        // Правила уровня шаблона в нынешнем порядке
        foreach (var error in ValidateSheetFormat(template.Sheet))
            yield return error;

        foreach (var error in ValidateMetadataKeys(template.Metadata))
            yield return error;

        foreach (var error in ValidateUniqueIds(template.Objects))
            yield return error;

        // Кросс-объектное правило: дубли ключей требуют все объекты сразу
        foreach (var error in ValidateTextKeys(template.Objects))
            yield return error;

        // Объектные правила — группировка по объекту: дескриптор каталога
        // знает тип (V-003 → V-004 → V-007 → V-005 внутри объекта)
        foreach (var obj in template.Objects)
        {
            if (!ObjectTypeCatalog.TryGet(obj, out var descriptor))
                continue;

            foreach (var error in descriptor.Validate(obj, template.Sheet, _validation))
                yield return error;
        }
    }

    private static IEnumerable<ValidationError> ValidateUniqueIds(IList<TemplateObjectBase> objects)
    {
        var seenIds = new HashSet<string>();
        foreach (var obj in objects)
        {
            if (string.IsNullOrWhiteSpace(obj.Id))
            {
                yield return new ValidationError(
                    "V-001", "Объект имеет пустой ID.",
                    objectId: obj.Id);
            }
            else if (!seenIds.Add(obj.Id))
            {
                yield return new ValidationError(
                    "V-001", $"Дублирующийся ID объекта: '{obj.Id}'.",
                    objectId: obj.Id);
            }
        }
    }

    public static IEnumerable<ValidationError> ValidateMetadataKeys(Metadata? metadata)
    {
        if (metadata == null) yield break;

        if (string.IsNullOrWhiteSpace(metadata.Author))
        {
            yield return new ValidationError(
                "V-002", "Автор шаблона не указан (ключевое поле Metadata.Author).",
                severity: ValidationSeverity.Warning);
        }
    }

    private static IEnumerable<ValidationError> ValidateTextKeys(IList<TemplateObjectBase> objects)
    {
        var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var obj in objects)
        {
            if (obj is not Text text || !text.IsEditable || string.IsNullOrWhiteSpace(text.Key))
                continue;

            if (!seenKeys.Add(text.Key))
            {
                yield return new ValidationError(
                    "V-002",
                    $"Дублирующийся ключ изменяемого поля: '{text.Key}'.",
                    objectId: obj.Id);
            }
        }
    }

    private static IEnumerable<ValidationError> ValidateSheetFormat(Sheet sheet)
    {
        if (sheet == null)
        {
            yield return new ValidationError("V-006", "Параметры листа не заданы.");
            yield break;
        }

        if (string.IsNullOrWhiteSpace(sheet.Format))
        {
            yield return new ValidationError("V-006", "Формат листа не указан.");
            yield break;
        }

        var isCustom = sheet.Format.Equals(Sheet.CustomName, StringComparison.OrdinalIgnoreCase);
        if (!isCustom && !SheetFormatCatalog.Contains(sheet.Format))
        {
            var allowed = string.Join(", ", SheetFormatCatalog.All.Select(f => f.Name)) + $", {Sheet.CustomName}";
            yield return new ValidationError(
                "V-006",
                $"Некорректный формат листа: '{sheet.Format}'. Допустимые: {allowed}.");
        }

        if (isCustom)
        {
            if (sheet.WidthMicrons <= 0)
            {
                yield return new ValidationError(
                    "V-006",
                    "Ширина листа Custom должна быть положительной.");
            }

            if (sheet.HeightMicrons <= 0)
            {
                yield return new ValidationError(
                    "V-006",
                    "Высота листа Custom должна быть положительной.");
            }
        }
    }
}
