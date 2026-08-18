using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.Tools;

namespace DotElectric.TemplateEditor.Helpers;

/// <summary>
/// Идентичность маркера изменения размера объекта.
/// Линия использует только TopLeft/BottomRight (начало/конец), прямоугольник — все восемь,
/// текст — четыре угловых.
/// </summary>
public enum ResizeHandle
{
    TopLeft,
    Top,
    TopRight,
    Right,
    BottomRight,
    Bottom,
    BottomLeft,
    Left
}

/// <summary>
/// Единственный источник знания «какие маркеры выделения есть у объекта и где они»:
/// каталог маркеров по типу объекта, позиции в модельных координатах, зоны попадания
/// с допуском, классификация маркеров и курсорная политика.
/// Потребители: SelectTool (hit по маркерам, hover), ResizeTool (курсоры),
/// ResizeMath (классификация в математике drag'а). Визуальная раскладка маркеров в XAML
/// читает те же свойства модели, что и позиции модуля.
/// Неизвестный тип объекта или маркер вне каталога типа — явное исключение (без silent-default).
/// </summary>
public static class MarkerLayout
{
    // Порядок каталога задаёт приоритет hit-проверки (first-match):
    // у линии конец проверяется первым — он наверху в Z-order.
    private static readonly ResizeHandle[] _lineMarkers =
    [
        ResizeHandle.BottomRight,
        ResizeHandle.TopLeft
    ];

    private static readonly ResizeHandle[] _rectangleMarkers =
    [
        ResizeHandle.TopLeft,
        ResizeHandle.Top,
        ResizeHandle.TopRight,
        ResizeHandle.Right,
        ResizeHandle.BottomRight,
        ResizeHandle.Bottom,
        ResizeHandle.BottomLeft,
        ResizeHandle.Left
    ];

    private static readonly ResizeHandle[] _textMarkers =
    [
        ResizeHandle.TopLeft,
        ResizeHandle.TopRight,
        ResizeHandle.BottomLeft,
        ResizeHandle.BottomRight
    ];

    /// <summary>
    /// Каталог маркеров объекта в порядке приоритета hit-проверки.
    /// </summary>
    public static IReadOnlyList<ResizeHandle> MarkersFor(TemplateObjectBase obj)
        => obj switch
        {
            Line => _lineMarkers,
            Rectangle => _rectangleMarkers,
            Text => _textMarkers,
            _ => throw UnknownObject(obj)
        };

    /// <summary>
    /// Позиция маркера в модельных координатах (микроны).
    /// Для текста используются RotatedCorner0-3 модели — они включают LayoutTransform offset,
    /// что гарантирует консистентность позиций маркеров (XAML) и зон попадания.
    /// </summary>
    public static PointMicrons GetPosition(TemplateObjectBase obj, ResizeHandle handle)
        => obj switch
        {
            Line line => handle switch
            {
                ResizeHandle.TopLeft => new PointMicrons(line.StartMicronsX, line.StartMicronsY),
                ResizeHandle.BottomRight => new PointMicrons(line.EndMicronsX, line.EndMicronsY),
                _ => throw UnknownMarker(obj, handle)
            },
            Rectangle rect => handle switch
            {
                ResizeHandle.TopLeft => new PointMicrons(rect.MicronsX, rect.BottomMicronsY),
                ResizeHandle.Top => new PointMicrons(rect.CenterMicronsX, rect.BottomMicronsY),
                ResizeHandle.TopRight => new PointMicrons(rect.RightMicronsX, rect.BottomMicronsY),
                ResizeHandle.Right => new PointMicrons(rect.RightMicronsX, rect.CenterMicronsY),
                ResizeHandle.BottomRight => new PointMicrons(rect.RightMicronsX, rect.MicronsY),
                ResizeHandle.Bottom => new PointMicrons(rect.CenterMicronsX, rect.MicronsY),
                ResizeHandle.BottomLeft => new PointMicrons(rect.MicronsX, rect.MicronsY),
                ResizeHandle.Left => new PointMicrons(rect.MicronsX, rect.CenterMicronsY),
                _ => throw UnknownMarker(obj, handle)
            },
            Text text => handle switch
            {
                ResizeHandle.TopLeft => new PointMicrons(text.RotatedCorner0X, text.RotatedCorner0Y),
                ResizeHandle.TopRight => new PointMicrons(text.RotatedCorner1X, text.RotatedCorner1Y),
                ResizeHandle.BottomLeft => new PointMicrons(text.RotatedCorner2X, text.RotatedCorner2Y),
                ResizeHandle.BottomRight => new PointMicrons(text.RotatedCorner3X, text.RotatedCorner3Y),
                _ => throw UnknownMarker(obj, handle)
            },
            _ => throw UnknownObject(obj)
        };

    /// <summary>
    /// Hit-тест маркеров: точка → маркер. Дистанция до позиции ≤ допуска,
    /// первый match в порядке каталога. null — ни один маркер не задет.
    /// Вызывается ПЕРЕД hit по телу, чтобы различить клик по маркеру и по телу объекта.
    /// </summary>
    public static ResizeHandle? HitHandle(PointMicrons point, TemplateObjectBase obj)
    {
        var tolerance = GetTolerance(obj);
        foreach (var handle in MarkersFor(obj))
        {
            if (point.DistanceTo(GetPosition(obj, handle)) <= tolerance)
                return handle;
        }

        return null;
    }

    /// <summary>
    /// Эффективный допуск маркеров: HandleHitToleranceMicrons,
    /// ограниченный третью минимального габарита объекта. Без ограничения зоны маркеров
    /// на маленьких объектах (текст сразу после размещения, короткие линии) поглощают
    /// тело целиком, и клик по телу уходит в resize вместо перетаскивания (#82).
    /// </summary>
    public static long GetTolerance(TemplateObjectBase obj)
    {
        long minDim = obj switch
        {
            Line line => new PointMicrons(line.StartMicronsX, line.StartMicronsY)
                .DistanceTo(new PointMicrons(line.EndMicronsX, line.EndMicronsY)),
            Rectangle rect => Math.Min(rect.WidthMicrons, rect.HeightMicrons),
            Text text => Math.Min(text.WidthMicrons, text.HeightMicrons),
            _ => throw UnknownObject(obj)
        };
        return Math.Min(PhysicalConstants.HandleHitToleranceMicrons, minDim / 3);
    }

    /// <summary>Маркер на левой стороне объекта (левое ребро или угол при ней).</summary>
    public static bool TouchesLeft(ResizeHandle handle)
        => handle is ResizeHandle.Left or ResizeHandle.TopLeft or ResizeHandle.BottomLeft;

    /// <summary>Маркер на правой стороне объекта (правое ребро или угол при ней).</summary>
    public static bool TouchesRight(ResizeHandle handle)
        => handle is ResizeHandle.Right or ResizeHandle.TopRight or ResizeHandle.BottomRight;

    /// <summary>Маркер на верхней стороне объекта (верхнее ребро или угол при ней).</summary>
    public static bool TouchesTop(ResizeHandle handle)
        => handle is ResizeHandle.Top or ResizeHandle.TopLeft or ResizeHandle.TopRight;

    /// <summary>Маркер на нижней стороне объекта (нижнее ребро или угол при ней).</summary>
    public static bool TouchesBottom(ResizeHandle handle)
        => handle is ResizeHandle.Bottom or ResizeHandle.BottomLeft or ResizeHandle.BottomRight;

    /// <summary>Маркер является углом объекта.</summary>
    public static bool IsCorner(ResizeHandle handle)
        => handle is ResizeHandle.TopLeft or ResizeHandle.TopRight
            or ResizeHandle.BottomLeft or ResizeHandle.BottomRight;

    /// <summary>
    /// Курсор для маркера: линия — перекрестие, вне ресайза — стрелка,
    /// иначе направление по маркеру.
    /// </summary>
    public static ToolCursor CursorForHandle(ResizeHandle handle, bool isResizing, bool isLine)
    {
        if (isLine) return ToolCursor.Cross;
        if (!isResizing) return ToolCursor.Arrow;

        return handle switch
        {
            ResizeHandle.TopLeft or ResizeHandle.BottomRight => ToolCursor.SizeNWSE,
            ResizeHandle.TopRight or ResizeHandle.BottomLeft => ToolCursor.SizeNESW,
            ResizeHandle.Top or ResizeHandle.Bottom => ToolCursor.SizeNS,
            ResizeHandle.Left or ResizeHandle.Right => ToolCursor.SizeWE,
            _ => throw UnknownHandle(handle)
        };
    }

    /// <summary>
    /// Возвращает курсор ресайза с учётом поворота текста.
    /// При 90°/270° визуальные углы смещены относительно имён маркеров,
    /// поэтому курсоры диагональных пар меняются местами.
    /// </summary>
    public static ToolCursor VisualCursorForHandle(ResizeHandle handle, int rotationAngle)
    {
        var angle = ((rotationAngle % 360) + 360) % 360;

        if (angle is 90 or 270)
        {
            return handle switch
            {
                ResizeHandle.TopLeft or ResizeHandle.BottomRight => ToolCursor.SizeNESW,
                ResizeHandle.TopRight or ResizeHandle.BottomLeft => ToolCursor.SizeNWSE,
                ResizeHandle.Top or ResizeHandle.Bottom => ToolCursor.SizeNS,
                ResizeHandle.Left or ResizeHandle.Right => ToolCursor.SizeWE,
                _ => throw UnknownHandle(handle)
            };
        }

        // 0°, 180° и прочие — стандартное отображение
        return CursorForHandle(handle, isResizing: true, isLine: false);
    }

    private static NotSupportedException UnknownObject(TemplateObjectBase obj)
        => new($"Каталог маркеров не определён для типа {obj.GetType().Name}.");

    private static NotSupportedException UnknownMarker(TemplateObjectBase obj, ResizeHandle handle)
        => new($"У типа {obj.GetType().Name} нет маркера {handle}.");

    private static NotSupportedException UnknownHandle(ResizeHandle handle)
        => new($"Неизвестный маркер изменения размера: {(int)handle}.");
}
