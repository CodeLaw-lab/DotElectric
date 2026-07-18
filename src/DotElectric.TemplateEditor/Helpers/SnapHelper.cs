using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Helpers;

/// <summary>
/// Утилиты для привязки точек и размеров к сетке.
/// Все операции работают с микронными координатами (long).
/// </summary>
public static class SnapHelper
{
    /// <summary>
    /// Привязать точку к сетке.
    /// </summary>
    /// <param name="point">Исходная точка в микронах.</param>
    /// <param name="stepMicrons">Шаг сетки в микронах.</param>
    /// <returns>Новая точка, привязанная к сетке.</returns>
    public static PointMicrons SnapToGrid(PointMicrons point, long stepMicrons)
    {
        return new PointMicrons(
            Coordinate.SnapToGrid(point.MicronsX, stepMicrons),
            Coordinate.SnapToGrid(point.MicronsY, stepMicrons));
    }

    /// <summary>
    /// Привязать координату X к сетке.
    /// </summary>
    /// <param name="micronsX">Координата X в микронах.</param>
    /// <param name="stepMicrons">Шаг сетки в микронах.</param>
    /// <returns>Привязанная координата X.</returns>
    public static long SnapX(long micronsX, long stepMicrons)
    {
        return Coordinate.SnapToGrid(micronsX, stepMicrons);
    }

    /// <summary>
    /// Привязать координату Y к сетке.
    /// </summary>
    /// <param name="micronsY">Координата Y в микронах.</param>
    /// <param name="stepMicrons">Шаг сетки в микронах.</param>
    /// <returns>Привязанная координата Y.</param>
    public static long SnapY(long micronsY, long stepMicrons)
    {
        return Coordinate.SnapToGrid(micronsY, stepMicrons);
    }

    /// <summary>
    /// Привязать размер к ближайшему шагу сетки.
    /// </summary>
    /// <param name="sizeMicrons">Размер в микронах.</param>
    /// <param name="stepMicrons">Шаг сетки в микронах.</param>
    /// <returns>Привязанный размер (неотрицательный).</returns>
    public static long SnapSize(long sizeMicrons, long stepMicrons)
    {
        var snapped = Coordinate.SnapToGrid(sizeMicrons, stepMicrons);
        return Math.Max(0, snapped);
    }

    /// <summary>
    /// Привязать объект TemplateObjectBase к сетке (перемещает опорную точку).
    /// </summary>
    /// <param name="obj">Объект для привязки.</param>
    /// <param name="stepMicrons">Шаг сетки в микронах.</param>
    public static void SnapObject(TemplateObjectBase obj, long stepMicrons)
    {
        var snappedX = Coordinate.SnapToGrid(obj.MicronsX, stepMicrons);
        var snappedY = Coordinate.SnapToGrid(obj.MicronsY, stepMicrons);
        obj.Move(snappedX, snappedY);
    }

    /// <summary>
    /// Привязать точку к сетке если привязка включена в настройках.
    /// </summary>
    /// <param name="point">Исходная точка.</param>
    /// <param name="gridSettings">Настройки сетки.</param>
    /// <returns>Точка, привязанная к сетке (если включено), или исходная.</returns>
    public static PointMicrons SnapIfEnabled(PointMicrons point, GridSettings gridSettings)
    {
        if (gridSettings.Enabled && gridSettings.SnapEnabled)
        {
            return point.SnapToGrid(gridSettings.StepMicrons);
        }
        return point;
    }
}
