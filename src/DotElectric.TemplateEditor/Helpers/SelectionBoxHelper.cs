using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.Helpers;

/// <summary>
/// Immutable-структура для хранения прямоугольника в микронах.
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

/// <summary>
/// Утилиты для выделения объектов рамкой (Selection Box).
/// LeftToRight — полное попадание, RightToLeft — частичное пересечение.
/// </summary>
public static class SelectionBoxHelper
{
    /// <summary>
    /// Определить направление рамки выделения по начальной и конечной точкам.
    /// </summary>
    /// <param name="start">Точка начала перетаскивания.</param>
    /// <param name="end">Точка конца перетаскивания.</param>
    /// <returns>Направление рамки.</returns>
    public static SelectionDirection GetDirection(PointMicrons start, PointMicrons end)
    {
        // LeftToRight: начальный X < конечного X
        // RightToLeft: начальный X > конечного X
        return start.MicronsX <= end.MicronsX
            ? SelectionDirection.LeftToRight
            : SelectionDirection.RightToLeft;
    }

    /// <summary>
    /// Получить объекты, попавшие в рамку выделения.
    /// </summary>
    /// <param name="selectionBox">Прямоугольник выделения.</param>
    /// <param name="objects">Все объекты шаблона.</param>
    /// <param name="direction">Направление рамки.</param>
    /// <returns>Список выделенных объектов.</returns>
    public static List<TemplateObjectBase> GetSelectedObjects(
        RectMicrons selectionBox,
        IList<TemplateObjectBase> objects,
        SelectionDirection direction)
    {
        return direction == SelectionDirection.LeftToRight
            ? GetFullyContained(selectionBox, objects)
            : GetIntersecting(selectionBox, objects);
    }

    /// <summary>
    /// LeftToRight: выделить только объекты, целиком попавшие в рамку.
    /// </summary>
    public static List<TemplateObjectBase> GetFullyContained(
        RectMicrons box,
        IList<TemplateObjectBase> objects)
    {
        var result = new List<TemplateObjectBase>();
        foreach (var obj in objects)
        {
            var objBounds = GetObjectBounds(obj);
            if (box.Contains(objBounds))
                result.Add(obj);
        }
        return result;
    }

    /// <summary>
    /// RightToLeft: выделить все задетые объекты.
    /// </summary>
    public static List<TemplateObjectBase> GetIntersecting(
        RectMicrons box,
        IList<TemplateObjectBase> objects)
    {
        var result = new List<TemplateObjectBase>();
        foreach (var obj in objects)
        {
            var objBounds = GetObjectBounds(obj);
            if (box.Intersects(objBounds))
                result.Add(obj);
        }
        return result;
    }

    /// <summary>
    /// Получить bounding box объекта в микронах.
    /// </summary>
    private static RectMicrons GetObjectBounds(TemplateObjectBase obj)
    {
        return obj switch
        {
            Line line => GetLineBounds(line),
            Rectangle rect => GetRectangleBounds(rect),
            Text text => GetTextBounds(text),
            _ => new RectMicrons(obj.MicronsX, obj.MicronsY, obj.MicronsX, obj.MicronsY)
        };
    }

    /// <summary>
    /// Bounding box линии: охватывающий прямоугольник от Start до End.
    /// </summary>
    private static RectMicrons GetLineBounds(Line line)
    {
        return new RectMicrons(
            Math.Min(line.StartMicronsX, line.EndMicronsX),
            Math.Min(line.StartMicronsY, line.EndMicronsY),
            Math.Max(line.StartMicronsX, line.EndMicronsX),
            Math.Max(line.StartMicronsY, line.EndMicronsY));
    }

    /// <summary>
    /// Bounding box прямоугольника: его собственные координаты.
    /// </summary>
    private static RectMicrons GetRectangleBounds(Rectangle rect)
    {
        return new RectMicrons(
            rect.MicronsX,
            rect.MicronsY,
            rect.MicronsX + rect.WidthMicrons,
            rect.MicronsY + rect.HeightMicrons);
    }

    /// <summary>
    /// Bounding box текста (делегирует модели).
    /// </summary>
    private static RectMicrons GetTextBounds(Text text) => text.GetBoundingBox();
}
