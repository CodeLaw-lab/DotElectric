using System.Xml.Serialization;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Services;

/// <summary>
/// DTO для сериализации шаблона в XML.
/// </summary>
[XmlRoot("Template")]
public class TemplateDto
{
    [XmlElement("Version")]
    public string Version { get; set; } = "1.0";

    [XmlElement("Metadata")]
    public MetadataDto? Metadata { get; set; }

    [XmlElement("Sheet")]
    public SheetDto? Sheet { get; set; }

    [XmlArray("Objects")]
    [XmlArrayItem("Object")]
    public List<ObjectDto> Objects { get; set; } = new();
}

[XmlType("Metadata")]
public class MetadataDto
{
    [XmlElement("Name")]
    public string? Name { get; set; }

    [XmlElement("Description")]
    public string? Description { get; set; }

    [XmlElement("Author")]
    public string? Author { get; set; }

    [XmlElement("CreatedDate")]
    public DateTime CreatedDate { get; set; }

    [XmlElement("ModifiedDate")]
    public DateTime ModifiedDate { get; set; }
}

[XmlType("Sheet")]
public class SheetDto
{
    [XmlElement("Format")]
    public string? Format { get; set; }

    [XmlElement("WidthMicrons")]
    public long WidthMicrons { get; set; }

    [XmlElement("HeightMicrons")]
    public long HeightMicrons { get; set; }

    [XmlIgnore]
    public SheetOrientation? Orientation { get; set; }

    [XmlElement("Orientation")]
    public string? OrientationValue
    {
        get => Orientation?.ToString();
        set
        {
            if (value != null && Enum.TryParse<SheetOrientation>(value, out var orient))
                Orientation = orient;
            else
                Orientation = null;
        }
    }
}

[XmlType("Object")]
public class ObjectDto
{
    [XmlElement("Type")]
    public string? ObjectType { get; set; }

    [XmlElement("Id")]
    public string? Id { get; set; }

    // Line
    [XmlElement("StartMicronsX")]
    public long StartMicronsX { get; set; }

    [XmlElement("StartMicronsY")]
    public long StartMicronsY { get; set; }

    [XmlElement("EndMicronsX")]
    public long EndMicronsX { get; set; }

    [XmlElement("EndMicronsY")]
    public long EndMicronsY { get; set; }

    [XmlElement("LineType")]
    public LineType? LineType { get; set; }

    [XmlElement("StrokeThicknessMicrons")]
    public long? StrokeThicknessMicrons { get; set; }

    [XmlElement("StrokeColor")]
    public string? StrokeColor { get; set; }

    // Rectangle
    [XmlElement("MicronsX")]
    public long? MicronsX { get; set; }

    [XmlElement("MicronsY")]
    public long? MicronsY { get; set; }

    [XmlElement("WidthMicrons")]
    public long? WidthMicrons { get; set; }

    [XmlElement("HeightMicrons")]
    public long? HeightMicrons { get; set; }

    // Text
    [XmlElement("Content")]
    public string? Content { get; set; }

    [XmlElement("FontSizeMicrons")]
    public long? FontSizeMicrons { get; set; }

    [XmlElement("FontName")]
    public string? FontName { get; set; }

    [XmlElement("TextType")]
    public TextType? TextType { get; set; }

    [XmlElement("RotationAngle")]
    public int? RotationAngle { get; set; }

    [XmlElement("Key")]
    public string? Key { get; set; }

    [XmlElement("IsEditable")]
    public bool IsEditable { get; set; }

    [XmlElement("DefaultValue")]
    public string? DefaultValue { get; set; }

    [XmlElement("FillColor")]
    public string? FillColor { get; set; }

    [XmlElement("Foreground")]
    public string? Foreground { get; set; }

    [XmlElement("TextWrapping")]
    public bool TextWrapping { get; set; }

    [XmlElement("TextAlignment")]
    public string? TextAlignment { get; set; }
}
