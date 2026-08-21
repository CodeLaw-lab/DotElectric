using System.Diagnostics.CodeAnalysis;
using DotElectric.Sheets;

namespace DotElectric.Document;

/// <summary>
/// Каталог типов объекта документа — единственный владелец строк идентичности
/// типов («Line»/«Rectangle»/«Text»), маппинга «запись ↔ модель» и объектных
/// правил проверки документа. Сервис шаблона и валидатор делегируют каталогу;
/// добавление нового типа объекта в библиотеке = один дескриптор
/// (прецедент — <c>SheetFormatCatalog</c>). Строковая идентичность типа
/// в файле зафиксирована ADR-0004.
/// </summary>
public static class ObjectTypeCatalog
{
    private static readonly ObjectTypeDescriptor[] Descriptors =
    [
        new("Line", typeof(Line), CreateLine, WriteLine, ValidateLine),
        new("Rectangle", typeof(Rectangle), CreateRectangle, WriteRectangle, ValidateRectangle),
        new("Text", typeof(Text), CreateText, WriteText, ValidateText)
    ];

    private static readonly Dictionary<string, ObjectTypeDescriptor> ByTypeName =
        Descriptors.ToDictionary(d => d.TypeName, StringComparer.Ordinal);

    private static readonly Dictionary<Type, ObjectTypeDescriptor> ByModelType =
        Descriptors.ToDictionary(d => d.ModelType);

    /// <summary>
    /// Все дескрипторы каталога.
    /// </summary>
    public static IReadOnlyList<ObjectTypeDescriptor> All { get; } = Descriptors;

    /// <summary>
    /// Найти дескриптор по имени типа из файла. Идентичность точная
    /// (регистрозависимая); неизвестное имя — без дескриптора
    /// (молчаливый пропуск при загрузке).
    /// </summary>
    public static bool TryGet(string? typeName, [NotNullWhen(true)] out ObjectTypeDescriptor? descriptor)
    {
        descriptor = null;
        return typeName != null && ByTypeName.TryGetValue(typeName, out descriptor);
    }

    /// <summary>
    /// Найти дескриптор по типу объекта модели. Неизвестный подтип — без
    /// дескриптора (запись без типа при сохранении, отсутствие объектных
    /// правил при проверке).
    /// </summary>
    public static bool TryGet(TemplateObjectBase obj, [NotNullWhen(true)] out ObjectTypeDescriptor? descriptor)
        => ByModelType.TryGetValue(obj.GetType(), out descriptor);

    // ===== Line =====

    private static TemplateObjectBase CreateLine(ObjectDto dto)
    {
        return new Line(
            dto.StartMicronsX,
            dto.StartMicronsY,
            dto.EndMicronsX,
            dto.EndMicronsY,
            dto.LineType ?? LineType.Solid,
            dto.StrokeThicknessMicrons ?? DocumentDefaults.DefaultStrokeThicknessMicrons,
            dto.StrokeColor)
        {
            Id = ResolveId(dto.Id)
        };
    }

    private static void WriteLine(TemplateObjectBase obj, ObjectDto dto)
    {
        var line = (Line)obj;
        dto.ObjectType = "Line";
        dto.StartMicronsX = line.StartMicronsX;
        dto.StartMicronsY = line.StartMicronsY;
        dto.EndMicronsX = line.EndMicronsX;
        dto.EndMicronsY = line.EndMicronsY;
        dto.LineType = line.LineType;
        dto.StrokeThicknessMicrons = line.StrokeThicknessMicrons;
        dto.StrokeColor = line.StrokeColor;
    }

    private static IEnumerable<ValidationError> ValidateLine(
        TemplateObjectBase obj, Sheet sheet, IValidationService colorValidation)
    {
        var line = (Line)obj;

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

        if (!Enum.IsDefined(typeof(LineType), line.LineType))
        {
            yield return new ValidationError(
                "V-007",
                $"Некорректный тип линии у объекта '{obj.Id}': '{line.LineType}'.",
                objectId: obj.Id);
        }

        if (colorValidation.ValidateHexColor(line.StrokeColor) != null)
            yield return new ValidationError("V-005",
                $"Некорректный HEX-формат цвета линии '{obj.Id}': '{line.StrokeColor}'.", obj.Id);
    }

    // ===== Rectangle =====

    private static TemplateObjectBase CreateRectangle(ObjectDto dto)
    {
        return new Rectangle(
            dto.MicronsX ?? 0,
            dto.MicronsY ?? 0,
            dto.WidthMicrons ?? 0,
            dto.HeightMicrons ?? 0,
            dto.LineType ?? LineType.Solid,
            dto.StrokeThicknessMicrons ?? DocumentDefaults.DefaultStrokeThicknessMicrons,
            dto.StrokeColor,
            dto.FillColor)
        {
            Id = ResolveId(dto.Id)
        };
    }

    private static void WriteRectangle(TemplateObjectBase obj, ObjectDto dto)
    {
        var rect = (Rectangle)obj;
        dto.ObjectType = "Rectangle";
        dto.MicronsX = rect.MicronsX;
        dto.MicronsY = rect.MicronsY;
        dto.WidthMicrons = rect.WidthMicrons;
        dto.HeightMicrons = rect.HeightMicrons;
        dto.LineType = rect.LineType;
        dto.StrokeThicknessMicrons = rect.StrokeThicknessMicrons;
        dto.StrokeColor = rect.StrokeColor;
        dto.FillColor = rect.FillColor;
    }

    private static IEnumerable<ValidationError> ValidateRectangle(
        TemplateObjectBase obj, Sheet sheet, IValidationService colorValidation)
    {
        var rect = (Rectangle)obj;

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

        if (!Enum.IsDefined(typeof(LineType), rect.LineType))
        {
            yield return new ValidationError(
                "V-007",
                $"Некорректный тип линии у объекта '{obj.Id}': '{rect.LineType}'.",
                objectId: obj.Id);
        }

        if (colorValidation.ValidateHexColor(rect.StrokeColor) != null)
            yield return new ValidationError("V-005",
                $"Некорректный HEX-формат цвета обводки прямоугольника '{obj.Id}': '{rect.StrokeColor}'.", obj.Id);
        if (colorValidation.ValidateHexColor(rect.FillColor) != null)
            yield return new ValidationError("V-005",
                $"Некорректный HEX-формат цвета заливки прямоугольника '{obj.Id}': '{rect.FillColor}'.", obj.Id);
    }

    // ===== Text =====

    private static TemplateObjectBase CreateText(ObjectDto dto)
    {
        return new Text(
            dto.MicronsX ?? 0,
            dto.MicronsY ?? 0,
            dto.Content ?? string.Empty,
            dto.FontSizeMicrons ?? DocumentDefaults.DefaultFontSizeMicrons,
            dto.FontName ?? DocumentDefaults.DefaultFontName,
            dto.TextType ?? TextType.Text,
            dto.RotationAngle ?? 0,
            dto.Key,
            dto.IsEditable,
            dto.DefaultValue,
            dto.Foreground,
            dto.TextWrapping,
            dto.TextAlignment ?? "Left")
        {
            Id = ResolveId(dto.Id)
        };
    }

    private static void WriteText(TemplateObjectBase obj, ObjectDto dto)
    {
        var text = (Text)obj;
        dto.ObjectType = "Text";
        dto.MicronsX = text.MicronsX;
        dto.MicronsY = text.MicronsY;
        dto.Content = text.Content;
        dto.FontSizeMicrons = text.FontSizeMicrons;
        dto.FontName = text.FontName;
        dto.TextType = text.TextType;
        dto.RotationAngle = text.RotationAngle;
        dto.Key = text.Key;
        dto.IsEditable = text.IsEditable;
        dto.DefaultValue = text.DefaultValue;
        dto.Foreground = text.Foreground;
        dto.TextWrapping = text.TextWrapping;
        dto.TextAlignment = text.TextAlignment;
    }

    private static IEnumerable<ValidationError> ValidateText(
        TemplateObjectBase obj, Sheet sheet, IValidationService colorValidation)
    {
        var text = (Text)obj;

        if (text.MicronsX < 0 || text.MicronsX > sheet.WidthMicrons ||
            text.MicronsY < 0 || text.MicronsY > sheet.HeightMicrons)
        {
            yield return new ValidationError(
                "V-003",
                $"Позиция текста '{obj.Id}' выходит за пределы листа.",
                objectId: obj.Id);
        }

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

        if (colorValidation.ValidateHexColor(text.Foreground) != null)
            yield return new ValidationError("V-005",
                $"Некорректный HEX-формат цвета текста '{obj.Id}': '{text.Foreground}'.", obj.Id);
    }

    // Идентификатор из файла сохраняется; отсутствующий или пустой — новый
    // (формат существующего генератора).
    private static string ResolveId(string? id)
        => string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString() : id;
}
