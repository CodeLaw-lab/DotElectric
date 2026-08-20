using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Helpers;

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
            var objBounds = obj.GetBoundingBox();
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
            var objBounds = obj.GetBoundingBox();
            if (box.Intersects(objBounds))
                result.Add(obj);
        }
        return result;
    }
}
