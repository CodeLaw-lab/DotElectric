using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.Serialization;
using DotElectric.Sheets;
using Microsoft.Extensions.Logging;

namespace DotElectric.Document;

/// <summary>
/// Реализация ITemplateService.
/// Отвечает за создание, загрузку и сохранение шаблонов.
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

    public TemplateService(ILogger<TemplateService>? logger = null, IDateTimeProvider? dateTimeProvider = null)
    {
        _logger = logger;
        _dateTimeProvider = dateTimeProvider ?? new DateTimeProvider();
    }

    public Template CreateNew(string format = SheetFormatCatalog.DefaultName, SheetOrientation? orientation = null)
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

        // Аварийный fallback повреждённого файла: формат по умолчанию (семантика сохранена).
        var defaultFormat = SheetFormatCatalog.Get(SheetFormatCatalog.DefaultName);
        var sheet = new Sheet
        {
            Format = dto.Sheet?.Format ?? SheetFormatCatalog.DefaultName,
            WidthMicrons = dto.Sheet?.WidthMicrons ?? defaultFormat.LongSideMicrons,
            HeightMicrons = dto.Sheet?.HeightMicrons ?? defaultFormat.ShortSideMicrons,
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

    private static TemplateObjectBase? MapToObject(ObjectDto dto)
        => ObjectTypeCatalog.TryGet(dto.ObjectType, out var descriptor)
            ? descriptor.FromDto(dto)
            : null;

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

    private static ObjectDto MapToDto(TemplateObjectBase obj)
    {
        var dto = new ObjectDto { Id = obj.Id };

        if (ObjectTypeCatalog.TryGet(obj, out var descriptor))
            descriptor.WriteToDto(obj, dto);

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
