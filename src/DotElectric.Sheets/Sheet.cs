namespace DotElectric.Sheets;

/// <summary>
/// Параметры листа шаблона (формат, размеры).
/// </summary>
public class Sheet
{
    /// <summary>
    /// Количество микрон в 1 мм. Библиотека листа не зависит от документной библиотеки,
    /// поэтому конвертация инлайнится (формулы байт-в-байт совпадают с Coordinate).
    /// </summary>
    private const long MicronsPerMm = 1000;

    /// <summary>
    /// Имя пользовательского формата (произвольные размеры, вне каталога).
    /// </summary>
    public const string CustomName = "Custom";

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
    public double WidthMm => WidthMicrons / (double)MicronsPerMm;

    /// <summary>
    /// Высота в миллиметрах (только для чтения).
    /// </summary>
    public double HeightMm => HeightMicrons / (double)MicronsPerMm;

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
        var entry = SheetFormatCatalog.Get(format);
        return CreateSheet(entry.Name, entry.LongSideMicrons, entry.ShortSideMicrons, orientation);
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
    /// A4 и полуформаты → Portrait (книжная), A0–A3 → Landscape (альбомная).
    /// Неизвестный формат — <see cref="ArgumentException"/> (едино с FromFormat).
    /// </summary>
    public static SheetOrientation GetDefaultOrientation(string format)
        => SheetFormatCatalog.Get(format).DefaultOrientation;

    /// <summary>
    /// Создать пользовательский лист.
    /// </summary>
    public static Sheet Custom(double widthMm, double heightMm)
        => new()
        {
            Format = CustomName,
            WidthMicrons = (long)Math.Round(widthMm * MicronsPerMm),
            HeightMicrons = (long)Math.Round(heightMm * MicronsPerMm),
            Orientation = widthMm >= heightMm
                ? SheetOrientation.Landscape
                : SheetOrientation.Portrait
        };
}