namespace DotElectric.TemplateEditor.Models;

/// <summary>
/// Параметры листа шаблона (формат, размеры).
/// </summary>
public class Sheet
{
    /// <summary>
    /// Формат листа (A0, A1, A2, A3, A4, Custom).
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Ширина листа в микронах (1 мм = 1000 микрон).
    /// </summary>
    public long WidthMicrons { get; set; }

    /// <summary>
    /// Высота листа в микронах.
    /// </summary>
    public long HeightMicrons { get; set; }

    /// <summary>
    /// Ориентация листа (Portrait/Landscape).
    /// </summary>
    public SheetOrientation Orientation { get; set; }

    /// <summary>
    /// Единица измерения (всегда "mm").
    /// </summary>
    public string Unit => "mm";

    /// <summary>
    /// Ширина в миллиметрах (только для чтения).
    /// </summary>
    public double WidthMm => Coordinate.ToMm(WidthMicrons);

    /// <summary>
    /// Высота в миллиметрах (только для чтения).
    /// </summary>
    public double HeightMm => Coordinate.ToMm(HeightMicrons);

    /// <summary>
    /// Создать лист стандартного формата ГОСТ с ориентацией по умолчанию.
    /// А0-А3 -> Landscape (альбомная), А4 -> Portrait (книжная).
    /// </summary>
    public static Sheet FromFormat(string format) => FromFormat(format, GetDefaultOrientation(format));

    /// <summary>
    /// Создать лист стандартного формата ГОСТ с заданной ориентацией.
    /// </summary>
    public static Sheet FromFormat(string format, SheetOrientation orientation)
    {
        return format.ToUpperInvariant() switch
        {
            "A0" => CreateSheet("A0", 1_189_000, 841_000, orientation),
            "A1" => CreateSheet("A1", 841_000, 594_000, orientation),
            "A2" => CreateSheet("A2", 594_000, 420_000, orientation),
            "A3" => CreateSheet("A3", 420_000, 297_000, orientation),
            "A4" => CreateSheet("A4", 297_000, 210_000, orientation),
            "A4×2" or "A4X2" => CreateSheet("A4×2", 594_000, 210_000, orientation),
            "A3×2" or "A3X2" => CreateSheet("A3×2", 840_000, 297_000, orientation),
            "A2×2" or "A2X2" => CreateSheet("A2×2", 1_188_000, 420_000, orientation),
            "A1×2" or "A1X2" => CreateSheet("A1×2", 1_682_000, 594_000, orientation),
            "A0×2" or "A0X2" => CreateSheet("A0×2", 2_378_000, 841_000, orientation),
            _ => throw new ArgumentException($"Неизвестный формат листа: {format}", nameof(format)),
        };
    }

    /// <summary>
    /// Создать лист с заданными размерами и ориентацией.
    /// wideMicrons — широкая сторона, narrowMicrons — узкая сторона.
    /// Portrait: Width = narrow, Height = wide.
    /// Landscape: Width = wide, Height = narrow.
    /// </summary>
    private static Sheet CreateSheet(
        string format,
        long wideMicrons,
        long narrowMicrons,
        SheetOrientation orientation)
    {
        return orientation switch
        {
            SheetOrientation.Portrait => new Sheet
            {
                Format = format,
                WidthMicrons = narrowMicrons,
                HeightMicrons = wideMicrons,
                Orientation = SheetOrientation.Portrait
            },
            SheetOrientation.Landscape => new Sheet
            {
                Format = format,
                WidthMicrons = wideMicrons,
                HeightMicrons = narrowMicrons,
                Orientation = SheetOrientation.Landscape
            },
            _ => throw new ArgumentException($"Неизвестная ориентация: {orientation}", nameof(orientation))
        };
    }

    /// <summary>
    /// Определить ориентацию по умолчанию для заданного формата.
    /// A4 → Portrait (книжная), остальные → Landscape (альбомная).
    /// </summary>
    public static SheetOrientation GetDefaultOrientation(string format)
    {
        if (string.IsNullOrWhiteSpace(format))
            return SheetOrientation.Landscape;

        return format.ToUpperInvariant() switch
        {
            "A4" => SheetOrientation.Portrait,
            "A4×2" or "A4X2" => SheetOrientation.Portrait,
            "A3×2" or "A3X2" => SheetOrientation.Portrait,
            "A2×2" or "A2X2" => SheetOrientation.Portrait,
            "A1×2" or "A1X2" => SheetOrientation.Portrait,
            "A0×2" or "A0X2" => SheetOrientation.Portrait,
            _ => SheetOrientation.Landscape
        };
    }

    /// <summary>
    /// Создать пользовательский лист.
    /// </summary>
    public static Sheet Custom(double widthMm, double heightMm)
        => new()
        {
            Format = "Custom",
            WidthMicrons = Coordinate.ToMicrons(widthMm),
            HeightMicrons = Coordinate.ToMicrons(heightMm),
            Orientation = widthMm >= heightMm
                ? SheetOrientation.Landscape
                : SheetOrientation.Portrait
        };
}