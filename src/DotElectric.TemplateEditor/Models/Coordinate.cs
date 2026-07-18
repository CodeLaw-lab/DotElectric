using System.Globalization;

namespace DotElectric.TemplateEditor.Models;

/// <summary>
/// Утилитарный класс для работы с Fixed-Point координатами (микроны).
/// 1 мм = 1000 микрон. Все вычисления в long для исключения погрешностей.
/// Сериализация в XML — xs:long, round-trip без потерь.
/// </summary>
public static class Coordinate
{
    /// <summary>
    /// Количество микрон в 1 мм.
    /// </summary>
    public const long MicronsPerMm = 1000;

    /// <summary>
    /// Количество микрон в 1 см.
    /// </summary>
    public const long MicronsPerCm = 10_000;

    /// <summary>
    /// Конвертация миллиметров в микроны (с округлением).
    /// </summary>
    /// <param name="mm">Значение в мм.</param>
    /// <returns>Значение в микронах.</returns>
    public static long ToMicrons(double mm)
        => (long)Math.Round(mm * MicronsPerMm);

    /// <summary>
    /// Конвертация микрон в миллиметры.
    /// </summary>
    /// <param name="microns">Значение в микронах.</param>
    /// <returns>Значение в мм.</returns>
    public static double ToMm(long microns)
        => microns / (double)MicronsPerMm;

    /// <summary>
    /// Привязка значения к ближайшему шагу сетки.
    /// </summary>
    /// <param name="microns">Значение в микронах.</param>
    /// <param name="stepMicrons">Шаг сетки в микронах.</param>
    /// <returns>Ближайшее кратное шагу сетки.</returns>
    public static long SnapToGrid(long microns, long stepMicrons)
    {
        if (stepMicrons <= 0)
            throw new ArgumentOutOfRangeException(nameof(stepMicrons), "Шаг сетки должен быть положительным.");

        return ((microns + stepMicrons / 2) / stepMicrons) * stepMicrons;
    }

    /// <summary>
    /// Форматирование значения в мм для отображения (без лишних нулей).
    /// </summary>
    /// <param name="microns">Значение в микронах.</param>
    /// <returns>Строка в формате "X.XXX" (без trailing zeros).</returns>
    public static string FormatMm(long microns)
        => ToMm(microns).ToString("0.###", CultureInfo.InvariantCulture);

    /// <summary>
    /// Парсинг строки (мм) в микроны.
    /// </summary>
    /// <param name="value">Строка с числом в мм (например, "42.500").</param>
    /// <returns>Значение в микронах.</returns>
    public static long ParseMm(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Значение не может быть пустым.", nameof(value));

        return ToMicrons(double.Parse(value, CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Сериализация микрон в строку XML (целое число, xs:long).
    /// </summary>
    /// <param name="microns">Значение в микронах.</param>
    /// <returns>Строка для XML.</returns>
    public static string SerializeMicrons(long microns)
        => microns.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Десериализация строки XML в микроны.
    /// </summary>
    /// <param name="value">Строка из XML (целое число).</param>
    /// <returns>Значение в микронах.</returns>
    public static long DeserializeMicrons(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Значение не может быть пустым.", nameof(value));

        return long.Parse(value, CultureInfo.InvariantCulture);
    }
}
