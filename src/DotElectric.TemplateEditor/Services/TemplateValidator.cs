using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Services;

public sealed class TemplateValidator : ITemplateValidator
{
    private readonly IValidationService _validation;

    public TemplateValidator() : this(ValidationService.Default) { }

    public TemplateValidator(IValidationService validation)
    {
        _validation = validation;
    }

    private static readonly HashSet<string> ValidFormats = new(
        StringComparer.OrdinalIgnoreCase)
    {
        "A0", "A1", "A2", "A3", "A4", "Custom",
        "A4×2", "A4X2", "A3×2", "A3X2",
        "A2×2", "A2X2", "A1×2", "A1X2",
        "A0×2", "A0X2"
    };

    public IEnumerable<ValidationError> Validate(Template template)
    {
        if (template == null)
        {
            yield return new ValidationError("V-000", "Шаблон не может быть null.");
            yield break;
        }

        foreach (var error in ValidateSheetFormat(template.Sheet))
            yield return error;

        foreach (var error in ValidateMetadataKeys(template.Metadata))
            yield return error;

        foreach (var error in ValidateUniqueIds(template.Objects))
            yield return error;

        foreach (var error in ValidateCoordinates(template.Objects, template.Sheet))
            yield return error;

        foreach (var error in ValidatePositiveSizes(template.Objects))
            yield return error;

        foreach (var error in ValidateTextKeys(template.Objects))
            yield return error;

        foreach (var error in ValidateLineTypes(template.Objects))
            yield return error;

        foreach (var error in ValidateColors(template.Objects))
            yield return error;
    }

    public IEnumerable<ValidationError> ValidateObject(TemplateObjectBase obj, Sheet sheet)
    {
        if (obj == null) yield break;

        foreach (var error in ValidateObjectCoordinates(obj, sheet))
            yield return error;

        foreach (var error in ValidateObjectPositiveSizes(obj))
            yield return error;

        foreach (var error in ValidateObjectLineType(obj))
            yield return error;
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

    private static IEnumerable<ValidationError> ValidateCoordinates(
        IList<TemplateObjectBase> objects, Sheet sheet)
    {
        foreach (var obj in objects)
        {
            foreach (var error in ValidateObjectCoordinates(obj, sheet))
                yield return error;
        }
    }

    private static IEnumerable<ValidationError> ValidateObjectCoordinates(
        TemplateObjectBase obj, Sheet sheet)
    {
        if (obj is Line line)
        {
            if (line.StartMicronsX < 0 || line.StartMicronsX > sheet.WidthMicrons ||
                line.StartMicronsY < 0 || line.StartMicronsY > sheet.HeightMicrons)
            {
                yield return new ValidationError(
                    "V-003",
                    $"Начальная точка линии '{obj.Id}' выходит за пределы листа " +
                    $"({Coordinate.FormatMm(line.StartMicronsX)}, {Coordinate.FormatMm(line.StartMicronsY)}).",
                    objectId: obj.Id);
            }

            if (line.EndMicronsX < 0 || line.EndMicronsX > sheet.WidthMicrons ||
                line.EndMicronsY < 0 || line.EndMicronsY > sheet.HeightMicrons)
            {
                yield return new ValidationError(
                    "V-003",
                    $"Конечная точка линии '{obj.Id}' выходит за пределы листа " +
                    $"({Coordinate.FormatMm(line.EndMicronsX)}, {Coordinate.FormatMm(line.EndMicronsY)}).",
                    objectId: obj.Id);
            }
        }
        else if (obj is Rectangle rect)
        {
            if (rect.MicronsX < 0 || rect.MicronsX > sheet.WidthMicrons ||
                rect.MicronsY < 0 || rect.MicronsY > sheet.HeightMicrons)
            {
                yield return new ValidationError(
                    "V-003",
                    $"Опорная точка прямоугольника '{obj.Id}' выходит за пределы листа.",
                    objectId: obj.Id);
            }

            var right = rect.MicronsX + rect.WidthMicrons;
            var top = rect.MicronsY + rect.HeightMicrons;
            if (right > sheet.WidthMicrons || top > sheet.HeightMicrons)
            {
                yield return new ValidationError(
                    "V-003",
                    $"Правый верхний угол прямоугольника '{obj.Id}' выходит за пределы листа " +
                    $"({Coordinate.FormatMm(right)}, {Coordinate.FormatMm(top)}).",
                    objectId: obj.Id);
            }
        }
        else if (obj is Text text)
        {
            if (text.MicronsX < 0 || text.MicronsX > sheet.WidthMicrons ||
                text.MicronsY < 0 || text.MicronsY > sheet.HeightMicrons)
            {
                yield return new ValidationError(
                    "V-003",
                    $"Позиция текста '{obj.Id}' выходит за пределы листа.",
                    objectId: obj.Id);
            }
        }
    }

    private static IEnumerable<ValidationError> ValidatePositiveSizes(IList<TemplateObjectBase> objects)
    {
        foreach (var obj in objects)
        {
            foreach (var error in ValidateObjectPositiveSizes(obj))
                yield return error;
        }
    }

    private static IEnumerable<ValidationError> ValidateObjectPositiveSizes(TemplateObjectBase obj)
    {
        if (obj is Rectangle rect)
        {
            if (rect.WidthMicrons <= 0)
            {
                yield return new ValidationError(
                    "V-004",
                    $"Ширина прямоугольника '{obj.Id}' должна быть положительной (текущая: {Coordinate.FormatMm(rect.WidthMicrons)}).",
                    objectId: obj.Id);
            }

            if (rect.HeightMicrons <= 0)
            {
                yield return new ValidationError(
                    "V-004",
                    $"Высота прямоугольника '{obj.Id}' должна быть положительной (текущая: {Coordinate.FormatMm(rect.HeightMicrons)}).",
                    objectId: obj.Id);
            }
        }
        else if (obj is Text text)
        {
            if (text.FontSizeMicrons <= 0)
            {
                yield return new ValidationError(
                    "V-004",
                    $"Размер шрифта текста '{obj.Id}' должен быть положительным (текущий: {Coordinate.FormatMm(text.FontSizeMicrons)}).",
                    objectId: obj.Id);
            }

            if (string.IsNullOrWhiteSpace(text.Content))
            {
                yield return new ValidationError(
                    "V-004",
                    $"Содержимое текста '{obj.Id}' пустое.",
                    objectId: obj.Id,
                    severity: ValidationSeverity.Warning);
            }
        }
        else if (obj is Line line)
        {
            var dx = line.EndMicronsX - line.StartMicronsX;
            var dy = line.EndMicronsY - line.StartMicronsY;
            if (dx == 0 && dy == 0)
            {
                yield return new ValidationError(
                    "V-004",
                    $"Длина линии '{obj.Id}' равна нулю (начальная и конечная точки совпадают).",
                    objectId: obj.Id,
                    severity: ValidationSeverity.Warning);
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

        if (!ValidFormats.Contains(sheet.Format))
        {
            yield return new ValidationError(
                "V-006",
                $"Некорректный формат листа: '{sheet.Format}'. Допустимые: {string.Join(", ", ValidFormats)}.");
        }

        if (sheet.Format.Equals("Custom", StringComparison.OrdinalIgnoreCase))
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

    private static IEnumerable<ValidationError> ValidateLineTypes(IList<TemplateObjectBase> objects)
    {
        foreach (var obj in objects)
        {
            foreach (var error in ValidateObjectLineType(obj))
                yield return error;
        }
    }

    private static IEnumerable<ValidationError> ValidateObjectLineType(TemplateObjectBase obj)
    {
        LineType? lineType = null;

        if (obj is Line line)
            lineType = line.LineType;
        else if (obj is Rectangle rect)
            lineType = rect.LineType;

        if (lineType.HasValue)
        {
            if (!Enum.IsDefined(typeof(LineType), lineType.Value))
            {
                yield return new ValidationError(
                    "V-007",
                    $"Некорректный тип линии у объекта '{obj.Id}': '{lineType.Value}'.",
                    objectId: obj.Id);
            }
        }
    }

    private IEnumerable<ValidationError> ValidateColors(IList<TemplateObjectBase> objects)
    {
        foreach (var obj in objects)
        {
            switch (obj)
            {
                case Line line:
                    if (_validation.ValidateHexColor(line.StrokeColor) != null)
                        yield return new ValidationError("V-005",
                            $"Некорректный HEX-формат цвета линии '{obj.Id}': '{line.StrokeColor}'.", obj.Id);
                    break;

                case Rectangle rect:
                    if (_validation.ValidateHexColor(rect.StrokeColor) != null)
                        yield return new ValidationError("V-005",
                            $"Некорректный HEX-формат цвета обводки прямоугольника '{obj.Id}': '{rect.StrokeColor}'.", obj.Id);
                    if (_validation.ValidateHexColor(rect.FillColor) != null)
                        yield return new ValidationError("V-005",
                            $"Некорректный HEX-формат цвета заливки прямоугольника '{obj.Id}': '{rect.FillColor}'.", obj.Id);
                    break;

                case Text text:
                    if (_validation.ValidateHexColor(text.Foreground) != null)
                        yield return new ValidationError("V-005",
                            $"Некорректный HEX-формат цвета текста '{obj.Id}': '{text.Foreground}'.", obj.Id);
                    break;
            }
        }
    }
}
