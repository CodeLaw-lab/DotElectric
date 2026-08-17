using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.Tools;

namespace DotElectric.TemplateEditor.Helpers;

/// <summary>
/// Helper для hit-testing объектов.
/// </summary>
public static class HitTestHelper
{
    /// <summary>
    /// Найти верхний объект под точкой (для single-selection).
    /// Возвращает верхний объект в Z-order или null.
    /// </summary>
    public static TemplateObjectBase? HitTest(
        PointMicrons point,
        IList<TemplateObjectBase> objects)
    {
        for (int i = objects.Count - 1; i >= 0; i--)
        {
            if (objects[i].ContainsPoint(point))
                return objects[i];
        }
        return null;
    }

    /// <summary>
    /// Найти все объекты под точкой (для multi-selection).
    /// Возвращает список в порядке Z-order (верхний первый).
    /// </summary>
    public static List<TemplateObjectBase> HitTestAll(
        PointMicrons point,
        IList<TemplateObjectBase> objects)
    {
        var result = new List<TemplateObjectBase>();
        for (int i = objects.Count - 1; i >= 0; i--)
        {
            var obj = objects[i];
            if (obj.ContainsPoint(point))
                result.Add(obj);
        }
        return result;
    }

    /// <summary>
    /// Проверить, попадает ли точка в линию с учётом допуска.
    /// </summary>
    public static bool HitTestLine(Line line, PointMicrons point) => line.ContainsPoint(point);

    /// <summary>
    /// Проверить, попадает ли точка в прямоугольник.
    /// </summary>
    public static bool HitTestRectangle(Rectangle rect, PointMicrons point) => rect.ContainsPoint(point);

    /// <summary>
    /// Проверить, попадает ли точка в текст.
    /// </summary>
    public static bool HitTestText(Text text, PointMicrons point) => text.ContainsPoint(point);

    /// <summary>
    /// Проверить, находится ли точка внутри/на объекте.
    /// Использует полиморфный метод ContainsPoint.
    /// </summary>
    public static bool HitTestObject(TemplateObjectBase obj, PointMicrons point)
    {
        return obj.ContainsPoint(point);
    }

    /// <summary>
    /// Расстояние от точки до отрезка (в микронах).
    /// Использует проекцию точки на прямую.
    /// </summary>
    /// <param name="point">Точка.</param>
    /// <param name="lineStart">Начало отрезка.</param>
    /// <param name="lineEnd">Конец отрезка.</param>
    /// <returns>Расстояние в микронах.</returns>
    public static long DistanceFromPointToLine(
        PointMicrons point,
        PointMicrons lineStart,
        PointMicrons lineEnd)
    {
        long dx = lineEnd.MicronsX - lineStart.MicronsX;
        long dy = lineEnd.MicronsY - lineStart.MicronsY;
        long lengthSquared = dx * dx + dy * dy;

        if (lengthSquared == 0)
            return point.DistanceTo(lineStart);

        // Проекция точки на прямую (параметр t)
        long t = (point.MicronsX - lineStart.MicronsX) * dx +
                 (point.MicronsY - lineStart.MicronsY) * dy;
        t = Math.Max(0, Math.Min(lengthSquared, t));

        // Ближайшая точка на отрезке
        long closestX = lineStart.MicronsX + (t * dx) / lengthSquared;
        long closestY = lineStart.MicronsY + (t * dy) / lengthSquared;

        return point.DistanceTo(new PointMicrons(closestX, closestY));
    }

    /// <summary>
    /// Определить, попал ли курсор на маркер изменения размера объекта.
    /// Вызывается ПЕРЕД HitTest, чтобы проверить — клик по маркеру или по телу объекта.
    /// </summary>
    /// <param name="point">Точка проверки в микронах.</param>
    /// <param name="obj">Объект для проверки маркеров.</param>
    /// <returns>ResizeHandle если попали в маркер, null если нет.</returns>
    public static ResizeHandle? GetHitHandle(PointMicrons point, TemplateObjectBase obj)
    {
        var tolerance = GetHandleTolerance(obj);
        return obj switch
        {
            Line line => GetLineHandle(line, point, tolerance),
            Rectangle rect => GetRectangleHandle(rect, point, tolerance),
            Text text => GetTextHandle(text, point, tolerance),
            _ => null
        };
    }

    /// <summary>
    /// Эффективный tolerance маркеров изменения размера: HandleHitToleranceMicrons,
    /// ограниченный третью минимального габарита объекта. Без ограничения зоны маркеров
    /// на маленьких объектах (текст сразу после размещения, короткие линии) поглощают
    /// тело целиком, и клик по телу уходит в resize вместо перетаскивания (#82).
    /// </summary>
    internal static long GetHandleTolerance(TemplateObjectBase obj)
    {
        long minDim = obj switch
        {
            Line line => new PointMicrons(line.StartMicronsX, line.StartMicronsY)
                .DistanceTo(new PointMicrons(line.EndMicronsX, line.EndMicronsY)),
            Rectangle rect => Math.Min(rect.WidthMicrons, rect.HeightMicrons),
            Text text => Math.Min(text.WidthMicrons, text.HeightMicrons),
            _ => 0
        };
        return Math.Min(PhysicalConstants.HandleHitToleranceMicrons, minDim / 3);
    }

    /// <summary>
    /// Hit-test маркеров линии — начало (TopLeft) и конец (BottomRight).
    /// </summary>
    private static ResizeHandle? GetLineHandle(Line line, PointMicrons point, long tolerance)
    {
        var start = new PointMicrons(line.StartMicronsX, line.StartMicronsY);
        var end = new PointMicrons(line.EndMicronsX, line.EndMicronsY);

        // Проверяем конец первым (он наверху в Z-order)
        if (point.DistanceTo(end) <= tolerance)
            return ResizeHandle.BottomRight;
        if (point.DistanceTo(start) <= tolerance)
            return ResizeHandle.TopLeft;

        return null;
    }

    /// <summary>
    /// Hit-test маркеров прямоугольника — 8 маркеров.
    /// </summary>
    private static ResizeHandle? GetRectangleHandle(Rectangle rect, PointMicrons point, long tolerance)
    {
        var handles = new[]
        {
            (ResizeHandle.TopLeft, rect.MicronsX, rect.BottomMicronsY),
            (ResizeHandle.Top, rect.CenterMicronsX, rect.BottomMicronsY),
            (ResizeHandle.TopRight, rect.RightMicronsX, rect.BottomMicronsY),
            (ResizeHandle.Right, rect.RightMicronsX, rect.CenterMicronsY),
            (ResizeHandle.BottomRight, rect.RightMicronsX, rect.MicronsY),
            (ResizeHandle.Bottom, rect.CenterMicronsX, rect.MicronsY),
            (ResizeHandle.BottomLeft, rect.MicronsX, rect.MicronsY),
            (ResizeHandle.Left, rect.MicronsX, rect.CenterMicronsY),
        };

        foreach (var (handle, hx, hy) in handles)
        {
            if (point.DistanceTo(new PointMicrons(hx, hy)) <= tolerance)
                return handle;
        }

        return null;
    }

    /// <summary>
    /// Hit-test маркеров текста — 4 угловых маркера на повёрнутых углах.
    /// </summary>
    private static ResizeHandle? GetTextHandle(Text text, PointMicrons point, long tolerance)
    {
        // Используем RotatedCorner0-3 из модели — они включают LayoutTransform offset,
        // что гарантирует консистентность позиций маркеров (XAML) и hit-test хэндлов.
        var handles = new[]
        {
            (ResizeHandle.TopLeft,     text.RotatedCorner0X, text.RotatedCorner0Y),
            (ResizeHandle.TopRight,    text.RotatedCorner1X, text.RotatedCorner1Y),
            (ResizeHandle.BottomLeft,  text.RotatedCorner2X, text.RotatedCorner2Y),
            (ResizeHandle.BottomRight, text.RotatedCorner3X, text.RotatedCorner3Y),
        };

        foreach (var (handle, hx, hy) in handles)
        {
            if (point.DistanceTo(new PointMicrons(hx, hy)) <= tolerance)
                return handle;
        }

        return null;
    }
}
