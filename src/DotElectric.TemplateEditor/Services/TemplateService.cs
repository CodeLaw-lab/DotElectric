using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.Serialization;
using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using Microsoft.Extensions.Logging;

namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// Реализация ITemplateService.
/// Отвечает за создание, загрузку, сохранение и валидацию шаблонов.
/// Формат .tdel = XML, упакованный в ZIP.
/// </summary>
public sealed class TemplateService : ITemplateService
{
    // Имя XML-файла внутри .tdel архива
    private const string TemplateXmlFileName = "template.xml";
    private readonly ILogger<TemplateService>? _logger;
    private const string CurrentVersion = "1.0";

    // Кэш сериализаторов (потокобезопасный, так как XmlSerializer потокобезопасен после создания)
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, System.Xml.Serialization.XmlSerializer> _serializerCache
        = new();

    private static System.Xml.Serialization.XmlSerializer GetSerializer(Type type)
    {
        return _serializerCache.GetOrAdd(type, t => new System.Xml.Serialization.XmlSerializer(t));
    }

    private static System.Xml.Serialization.XmlSerializer GetTemplateSerializer()
    {
        return GetSerializer(typeof(TemplateDto));
    }

    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ITemplateValidator? _templateValidator;

    public TemplateService(ILogger<TemplateService>? logger = null, IDateTimeProvider? dateTimeProvider = null,
        ITemplateValidator? templateValidator = null)
    {
        _logger = logger;
        _dateTimeProvider = dateTimeProvider ?? new DateTimeProvider();
        _templateValidator = templateValidator ?? new TemplateValidator();
    }

    public Template CreateNew(string format = "A3", SheetOrientation? orientation = null)
    {
        var orient = orientation ?? Sheet.GetDefaultOrientation(format);
        var sheet = Sheet.FromFormat(format, orient);
        var metadata = new Metadata
        {
            Name = $"Без имени — {format}",
            Author = Environment.UserName,
            CreatedDate = _dateTimeProvider.UtcNow,
            ModifiedDate = _dateTimeProvider.UtcNow
        };

        _logger?.LogInformation("Создан новый шаблон: format={Format}, orientation={Orientation}", format, orient);
        return new Template(metadata, sheet);
    }

    public Template CreateFromSheet(Sheet sheet)
    {
        var metadata = new Metadata
        {
            Name = $"Без имени — Custom ({Coordinate.FormatMm(sheet.WidthMicrons)}×{Coordinate.FormatMm(sheet.HeightMicrons)})",
            Author = Environment.UserName,
            CreatedDate = _dateTimeProvider.UtcNow,
            ModifiedDate = _dateTimeProvider.UtcNow
        };

        return new Template(metadata, sheet);
    }

    public Template Load(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Файл шаблона не найден: {filePath}");

        try
        {
            using var archive = ZipFile.OpenRead(filePath);
            var entry = archive.GetEntry(TemplateXmlFileName);
            if (entry == null)
                throw new InvalidDataException(
                    $"Неверный формат .tdel: отсутствует {TemplateXmlFileName}");

            using var stream = entry.Open();
            var serializer = GetTemplateSerializer();
            var dto = (TemplateDto)serializer.Deserialize(stream)!;

            // Миграция версий
            dto = MigrateDto(dto);

            var template = MapToTemplate(dto);
            _logger?.LogInformation("Загружен шаблон: filePath={FilePath}, objects={ObjectCount}", filePath, template.Objects.Count);
            return template;
        }
        catch (Exception ex) when (ex is not FileNotFoundException and not InvalidDataException)
        {
            _logger?.LogError(ex, "Ошибка загрузки шаблона: filePath={FilePath}", filePath);
            throw new InvalidDataException($"Ошибка чтения файла шаблона: {ex.Message}", ex);
        }
    }

    public void Save(Template template, string filePath)
    {
        if (template == null)
            throw new ArgumentNullException(nameof(template));
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Путь к файлу не может быть пустым.", nameof(filePath));

        // Обновляем дату модификации
        template.Metadata.ModifiedDate = _dateTimeProvider.UtcNow;

        var dto = MapToDto(template);

        // Создаём директорию, если не существует
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        // Пишем во временный файл для атомарной замены
        var tempPath = filePath + ".tmp";
        try
        {
            using (var archive = ZipFile.Open(tempPath, ZipArchiveMode.Create))
            {
                var entry = archive.CreateEntry(TemplateXmlFileName, CompressionLevel.Optimal);

                using var entryStream = entry.Open();
                var serializer = GetTemplateSerializer();

                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    Encoding = System.Text.Encoding.UTF8,
                    OmitXmlDeclaration = false
                };

                using var xmlWriter = XmlWriter.Create(entryStream, settings);
                serializer.Serialize(xmlWriter, dto);
            }

            // Атомарно заменяем целевой файл
            if (File.Exists(filePath))
                File.Replace(tempPath, filePath, null);
            else
                File.Move(tempPath, filePath);
        }
        catch
        {
            try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }
            throw;
        }

        _logger?.LogInformation("Сохранён шаблон: filePath={FilePath}, objects={ObjectCount}", filePath, template.Objects.Count);
    }

    public IEnumerable<string> Validate(Template template)
    {
        var errors = new List<string>();

        if (template == null)
        {
            errors.Add("Шаблон не может быть null.");
            return errors;
        }

        // Валидация через ITemplateValidator
        var validator = _templateValidator;
        if (validator != null)
        {
            var validationErrors = validator.Validate(template);
            foreach (var error in validationErrors)
            {
                if (error.Severity == ValidationSeverity.Error)
                    errors.Add($"[{error.RuleId}] {error.Message}");
            }
        }

        return errors;
    }

    #region Mapping

    private Template MapToTemplate(TemplateDto dto)
    {
        var metadata = new Metadata
        {
            Name = dto.Metadata?.Name ?? string.Empty,
            Description = dto.Metadata?.Description ?? string.Empty,
            Author = dto.Metadata?.Author ?? string.Empty,
            CreatedDate = dto.Metadata?.CreatedDate ?? _dateTimeProvider.UtcNow,
            ModifiedDate = dto.Metadata?.ModifiedDate ?? _dateTimeProvider.UtcNow
        };

        var sheetDto = dto.Sheet;
        var orientation = sheetDto?.Orientation ?? DetermineOrientation(sheetDto);

        var sheet = new Sheet
        {
            Format = dto.Sheet?.Format ?? "A3",
            WidthMicrons = dto.Sheet?.WidthMicrons ?? 420_000,
            HeightMicrons = dto.Sheet?.HeightMicrons ?? 297_000,
            Orientation = orientation
        };

        var template = new Template(metadata, sheet)
        {
            Version = dto.Version ?? "1.0"
        };

        foreach (var objDto in dto.Objects)
        {
            var obj = MapToObject(objDto);
            if (obj != null)
                template.Objects.Add(obj);
        }

        return template;
    }

     private static SheetOrientation DetermineOrientation(SheetDto? sheetDto)
     {
         if (sheetDto == null)
            return SheetOrientation.Landscape;

         return sheetDto.WidthMicrons > sheetDto.HeightMicrons
            ? SheetOrientation.Landscape
            : SheetOrientation.Portrait;
     }

    private TemplateObjectBase? MapToObject(ObjectDto dto)
    {
        return dto.ObjectType switch
        {
            "Line" => new Line(
                dto.StartMicronsX,
                dto.StartMicronsY,
                dto.EndMicronsX,
                dto.EndMicronsY,
                dto.LineType ?? LineType.Solid,
                dto.StrokeThicknessMicrons ?? EditorSettings.DefaultStrokeThicknessMicrons,
                dto.StrokeColor),

            "Rectangle" => new Rectangle(
                dto.MicronsX ?? 0,
                dto.MicronsY ?? 0,
                dto.WidthMicrons ?? 0,
                dto.HeightMicrons ?? 0,
                dto.LineType ?? LineType.Solid,
                dto.StrokeThicknessMicrons ?? EditorSettings.DefaultStrokeThicknessMicrons,
                dto.StrokeColor,
                dto.FillColor),

            "Text" => new Text(
                dto.MicronsX ?? 0,
                dto.MicronsY ?? 0,
                dto.Content ?? string.Empty,
                dto.FontSizeMicrons ?? EditorSettings.DefaultFontSizeMicrons,
                dto.FontName ?? EditorSettings.DefaultFontName,
                dto.TextType ?? TextType.Text,
                dto.RotationAngle ?? 0,
                dto.Key,
                dto.IsEditable,
                dto.DefaultValue,
                dto.Foreground,
                dto.TextWrapping,
                dto.TextAlignment ?? "Left"),

            _ => null
        };
    }

    private TemplateDto MapToDto(Template template)
    {
        var dto = new TemplateDto
        {
            Version = template.Version,
            Metadata = new MetadataDto
            {
                Name = template.Metadata.Name,
                Description = template.Metadata.Description,
                Author = template.Metadata.Author,
                CreatedDate = template.Metadata.CreatedDate,
                ModifiedDate = template.Metadata.ModifiedDate
            },
            Sheet = new SheetDto
            {
                Format = template.Sheet.Format,
                WidthMicrons = template.Sheet.WidthMicrons,
                HeightMicrons = template.Sheet.HeightMicrons,
                Orientation = template.Sheet.Orientation
            }
        };

        foreach (var obj in template.Objects)
        {
            dto.Objects.Add(MapToDto(obj));
        }

        return dto;
    }

    private ObjectDto MapToDto(TemplateObjectBase obj)
    {
        var dto = new ObjectDto { Id = obj.Id };

        switch (obj)
        {
            case Line line:
                dto.ObjectType = "Line";
                dto.StartMicronsX = line.StartMicronsX;
                dto.StartMicronsY = line.StartMicronsY;
                dto.EndMicronsX = line.EndMicronsX;
                dto.EndMicronsY = line.EndMicronsY;
                dto.LineType = line.LineType;
                dto.StrokeThicknessMicrons = line.StrokeThicknessMicrons;
                dto.StrokeColor = line.StrokeColor;
                break;

            case Rectangle rect:
                dto.ObjectType = "Rectangle";
                dto.MicronsX = rect.MicronsX;
                dto.MicronsY = rect.MicronsY;
                dto.WidthMicrons = rect.WidthMicrons;
                dto.HeightMicrons = rect.HeightMicrons;
                dto.LineType = rect.LineType;
                dto.StrokeThicknessMicrons = rect.StrokeThicknessMicrons;
                dto.StrokeColor = rect.StrokeColor;
                dto.FillColor = rect.FillColor;
                break;

            case Text text:
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
                break;
        }

        return dto;
    }

    #endregion

    /// <summary>
    /// Миграция DTO из старой версии к текущей.
    /// В будущем можно добавить преобразования между различными версиями.
    /// </summary>
    private static TemplateDto MigrateDto(TemplateDto dto)
    {
        // Если версия не указана — считаем версией 1.0
        if (string.IsNullOrEmpty(dto.Version))
        {
            dto.Version = CurrentVersion;
        }

        // Здесь можно добавить миграцию для будущих версий
        // if (dto.Version == "1.0") { migrate to 1.1 ... }

        return dto;
    }
}
