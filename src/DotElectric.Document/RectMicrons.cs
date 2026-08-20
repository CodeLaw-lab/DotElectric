namespace DotElectric.Document;

/// <summary>
/// Immutable-структура для хранения прямоугольника в микронах.
/// Возвращается полиморфным <c>GetBoundingBox()</c> объектов шаблона.
/// </summary>
public readonly struct RectMicrons
{
    public long Left { get; }
    public long Bottom { get; }
    public long Right { get; }
    public long Top { get; }
    public long Width => Right - Left;
    public long Height => Top - Bottom;

    public RectMicrons(long left, long bottom, long right, long top)
    {
        Left = Math.Min(left, right);
        Bottom = Math.Min(bottom, top);
        Right = Math.Max(left, right);
        Top = Math.Max(bottom, top);
    }

    /// <summary>
    /// Создать RectMicrons из двух точек (начало и конец перетаскивания).
    /// </summary>
    public static RectMicrons FromPoints(PointMicrons start, PointMicrons end)
    {
        return new RectMicrons(
            Math.Min(start.MicronsX, end.MicronsX),
            Math.Min(start.MicronsY, end.MicronsY),
            Math.Max(start.MicronsX, end.MicronsX),
            Math.Max(start.MicronsY, end.MicronsY));
    }

    /// <summary>
    /// Проверить, пересекается ли этот прямоугольник с другим.
    /// </summary>
    public bool Intersects(RectMicrons other)
    {
        return Left < other.Right &&
               Right > other.Left &&
               Bottom < other.Top &&
               Top > other.Bottom;
    }

    /// <summary>
    /// Проверить, содержит ли этот прямоугольник другой целиком.
    /// </summary>
    public bool Contains(RectMicrons other)
    {
        return Left <= other.Left &&
               Right >= other.Right &&
               Bottom <= other.Bottom &&
               Top >= other.Top;
    }
}
