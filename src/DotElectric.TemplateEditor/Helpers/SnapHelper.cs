using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Helpers;

/// <summary>
/// Утилиты для привязки точек и размеров к сетке.
/// Все операции работают с микронными координатами (long).
/// Привязка к сетке — понятие редактора, а не документа:
/// формула округления живёт только здесь.
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
            Snap(point.MicronsX, stepMicrons),
            Snap(point.MicronsY, stepMicrons));
    }

    /// <summary>
    /// Привязать координату X к сетке.
    /// </summary>
    /// <param name="micronsX">Координата X в микронах.</param>
    /// <param name="stepMicrons">Шаг сетки в микронах.</param>
    /// <returns>Привязанная координата X.</returns>
    public static long SnapX(long micronsX, long stepMicrons)
    {
        return Snap(micronsX, stepMicrons);
    }

    /// <summary>
    /// Привязать координату Y к сетке.
    /// </summary>
    /// <param name="micronsY">Координата Y в микронах.</param>
    /// <param name="stepMicrons">Шаг сетки в микронах.</param>
    /// <returns>Привязанная координата Y.</returns>
    public static long SnapY(long micronsY, long stepMicrons)
    {
        return Snap(micronsY, stepMicrons);
    }

    /// <summary>
    /// Привязать размер к ближайшему шагу сетки.
    /// </summary>
    /// <param name="sizeMicrons">Размер в микронах.</param>
    /// <param name="stepMicrons">Шаг сетки в микронах.</param>
    /// <returns>Привязанный размер (неотрицательный).</returns>
    public static long SnapSize(long sizeMicrons, long stepMicrons)
    {
        var snapped = Snap(sizeMicrons, stepMicrons);
        return Math.Max(0, snapped);
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
            return SnapToGrid(point, gridSettings.StepMicrons);
        }
        return point;
    }

    /// <summary>
    /// Привязка значения к ближайшему шагу сетки.
    /// </summary>
    /// <param name="microns">Значение в микронах.</param>
    /// <param name="stepMicrons">Шаг сетки в микронах.</param>
    /// <returns>Ближайшее кратное шагу сетки.</returns>
    private static long Snap(long microns, long stepMicrons)
    {
        if (stepMicrons <= 0)
            throw new ArgumentOutOfRangeException(nameof(stepMicrons), "Шаг сетки должен быть положительным.");

        return ((microns + stepMicrons / 2) / stepMicrons) * stepMicrons;
    }
}
