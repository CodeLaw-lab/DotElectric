namespace DotElectric.TemplateEditor.Models;

/// <summary>
/// Immutable-структура для хранения пары координат в микронах.
/// Используется для точек (начало/конец линии, привязка и т.д.).
/// </summary>
public readonly struct PointMicrons
{
    /// <summary>
    /// Координата X в микронах.
    /// </summary>
    public long MicronsX { get; }

    /// <summary>
    /// Координата Y в микронах.
    /// </summary>
    public long MicronsY { get; }

    /// <summary>
    /// Координата X в миллиметрах (только чтение).
    /// </summary>
    public double X => Coordinate.ToMm(MicronsX);

    /// <summary>
    /// Координата Y в миллиметрах (только чтение).
    /// </summary>
    public double Y => Coordinate.ToMm(MicronsY);

    /// <summary>
    /// Конструктор из микрон.
    /// </summary>
    /// <param name="micronsX">X в микронах.</param>
    /// <param name="micronsY">Y в микронах.</param>
    public PointMicrons(long micronsX, long micronsY)
    {
        MicronsX = micronsX;
        MicronsY = micronsY;
    }

    /// <summary>
    /// Создать точку из миллиметров (с округлением до ближайших микрон).
    /// </summary>
    /// <param name="mmX">X в миллиметрах.</param>
    /// <param name="mmY">Y в миллиметрах.</param>
    /// <returns>Точка в микронах.</returns>
    public static PointMicrons FromMm(double mmX, double mmY)
        => new(Coordinate.ToMicrons(mmX), Coordinate.ToMicrons(mmY));

    /// <summary>
    /// Привязка точки к сетке.
    /// </summary>
    /// <param name="stepMicrons">Шаг сетки в микронах.</param>
    /// <returns>Новая точка, привязанная к сетке.</returns>
    public PointMicrons SnapToGrid(long stepMicrons)
        => new(
            Coordinate.SnapToGrid(MicronsX, stepMicrons),
            Coordinate.SnapToGrid(MicronsY, stepMicrons));

    /// <summary>
    /// Расстояние до другой точки (в микронах).
    /// </summary>
    public long DistanceTo(PointMicrons other)
    {
        long dx = MicronsX - other.MicronsX;
        long dy = MicronsY - other.MicronsY;
        return (long)Math.Sqrt(dx * dx + dy * dy);
    }

    /// <summary>
    /// Сравнение двух точек.
    /// </summary>
    public bool Equals(PointMicrons other)
        => MicronsX == other.MicronsX && MicronsY == other.MicronsY;

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is PointMicrons other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(MicronsX, MicronsY);

    /// <summary>
    /// Оператор равенства.
    /// </summary>
    public static bool operator ==(PointMicrons left, PointMicrons right)
        => left.Equals(right);

    /// <summary>
    /// Оператор неравенства.
    /// </summary>
    public static bool operator !=(PointMicrons left, PointMicrons right)
        => !left.Equals(right);

    /// <summary>
    /// Оператор сложения двух точек (покомпонентно).
    /// </summary>
    public static PointMicrons operator +(PointMicrons left, PointMicrons right)
        => new(left.MicronsX + right.MicronsX, left.MicronsY + right.MicronsY);

    /// <summary>
    /// Оператор вычитания двух точек (покомпонентно).
    /// </summary>
    public static PointMicrons operator -(PointMicrons left, PointMicrons right)
        => new(left.MicronsX - right.MicronsX, left.MicronsY - right.MicronsY);

    /// <inheritdoc/>
    public override string ToString()
        => $"({Coordinate.FormatMm(MicronsX)}, {Coordinate.FormatMm(MicronsY)}) мм";
}
