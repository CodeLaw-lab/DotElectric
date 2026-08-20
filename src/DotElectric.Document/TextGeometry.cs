namespace DotElectric.Document;

/// <summary>
/// Геометрия повёрнутого текста. WPF LayoutTransform позиционирует повёрнутый элемент
/// по верхнему левому углу трансформированного bounding box, а не по origin (0,0):
/// все формулы учитывают это смещение, что гарантирует консистентность маркеров выделения,
/// хит-теста, рамки выделения и конвейеров рендеринга (канвас, предпросмотр, печать).
/// </summary>
public static class TextGeometry
{
    /// <summary>
    /// Смещение LayoutTransform: (minX, minY) четырёх углов локального Y-down бокса,
    /// повёрнутых вокруг origin. Применяется к anchor (MicronsX, MicronsY + HeightMicrons)
    /// как (-minX, +minY) и даёт фактический центр вращения.
    /// </summary>
    public static (long OffsetX, long OffsetY) LayoutOffset(Text text)
    {
        var (cosA, sinA) = Trig(text.RotationAngle);
        return LayoutOffset(text.WidthMicrons, text.HeightMicrons, cosA, sinA);
    }

    /// <summary>
    /// Угол повёрнутого бокса текста в модельных координатах, с учётом LayoutTransform offset.
    /// Индексы соответствуют каталогу маркеров MarkerLayout:
    /// 0 = TopLeft, 1 = TopRight, 2 = BottomLeft, 3 = BottomRight.
    /// </summary>
    public static PointMicrons Corner(Text text, int index)
    {
        var (minX, minY) = LayoutOffset(text);
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;
        var (cosA, sinA) = Trig(text.RotationAngle);
        var anchorX = text.MicronsX;
        var anchorY = text.MicronsY + text.HeightMicrons;

        return index switch
        {
            0 => new PointMicrons(anchorX - minX, anchorY + minY),
            1 => new PointMicrons(anchorX + (long)Math.Round(w * cosA) - minX, anchorY - (long)Math.Round(w * sinA) + minY),
            2 => new PointMicrons(anchorX - (long)Math.Round(h * sinA) - minX, anchorY - (long)Math.Round(h * cosA) + minY),
            3 => new PointMicrons(anchorX + (long)Math.Round(w * cosA - h * sinA) - minX, anchorY - (long)Math.Round(w * sinA + h * cosA) + minY),
            _ => throw new NotSupportedException($"Индекс угла должен быть в диапазоне 0–3, получено: {index}.")
        };
    }

    /// <summary>
    /// Попадание точки в тело повёрнутого текста: обратное вращение точки
    /// вокруг фактического центра вращения (anchor + LayoutTransform offset).
    /// </summary>
    public static bool Contains(Text text, PointMicrons point)
    {
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;
        var (cosA, sinA) = Trig(text.RotationAngle);

        var (minX, minY) = LayoutOffset(text);
        // Фактический центр вращения с учётом LayoutTransform offset
        var (centerX, centerY) = RotationCenter(text, minX, minY);

        var cpX = point.MicronsX - centerX;
        var cpY = centerY - point.MicronsY;

        var u = cpX * cosA + cpY * sinA;
        var v = -cpX * sinA + cpY * cosA;

        return u >= 0 && u <= w && v >= 0 && v <= h;
    }

    /// <summary>
    /// Bounding box повёрнутого текста в модельных координатах, с учётом LayoutTransform offset.
    /// </summary>
    public static RectMicrons BoundingBox(Text text)
    {
        var w = text.WidthMicrons;
        var h = text.HeightMicrons;
        var (cosA, sinA) = Trig(text.RotationAngle);

        var (minX, minY) = LayoutOffset(text);
        // Фактический центр вращения с учётом LayoutTransform offset
        var (centerX, centerY) = RotationCenter(text, minX, minY);

        var corners = new[] {
            (0L, 0L),
            (w, 0L),
            (0L, h),
            (w, h)
        };

        long minXbb = long.MaxValue, minYbb = long.MaxValue;
        long maxXbb = long.MinValue, maxYbb = long.MinValue;

        foreach (var (lx, ly) in corners)
        {
            var cpX = lx * cosA - ly * sinA;
            var cpY = lx * sinA + ly * cosA;

            var wx = centerX + (long)Math.Round(cpX);
            var wy = centerY - (long)Math.Round(cpY);
            if (wx < minXbb) minXbb = wx;
            if (wy < minYbb) minYbb = wy;
            if (wx > maxXbb) maxXbb = wx;
            if (wy > maxYbb) maxYbb = wy;
        }

        return new RectMicrons(minXbb, minYbb, maxXbb, maxYbb);
    }

    private static (double CosA, double SinA) Trig(int rotationAngle)
    {
        var angleRad = rotationAngle * Math.PI / 180.0;
        return (Math.Cos(angleRad), Math.Sin(angleRad));
    }

    private static (long MinX, long MinY) LayoutOffset(long w, long h, double cosA, double sinA)
    {
        // Four corners of the local Y-down box after rotation around origin (0,0):
        // (0,0), (W·cos, W·sin), (-H·sin, H·cos), (W·cos-H·sin, W·sin+H·cos)
        var c1x = w * cosA;
        var c1y = w * sinA;
        var c2x = -h * sinA;
        var c2y = h * cosA;
        var c3x = w * cosA - h * sinA;
        var c3y = w * sinA + h * cosA;

        var minX = Math.Min(0, Math.Min(Math.Min(c1x, c2x), c3x));
        var minY = Math.Min(0, Math.Min(Math.Min(c1y, c2y), c3y));

        return ((long)Math.Round(minX), (long)Math.Round(minY));
    }

    private static (long CenterX, long CenterY) RotationCenter(Text text, long minX, long minY) =>
        (text.MicronsX - minX, text.MicronsY + text.HeightMicrons + minY);
}
