using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Helpers;

/// <summary>
/// Генератор узлов сетки. Инжектируется через DI.
/// Все координаты — абсолютные микроны листа (0,0 = нижний левый угол).
/// </summary>
public interface IGridNodeGenerator
{
    /// <summary>
    /// Вычисляет оптимальный шаг отображения сетки.
    /// Если пользовательский шаг укладывается в бюджет и pixel-spacing — используется он.
    /// Иначе — coarsen через NiceSteps.
    /// </summary>
    long ComputeDisplayStep(
        double zoom,
        int maxNodes,
        long sheetWidthMicrons,
        long sheetHeightMicrons,
        long preferredStepMicrons = 0);

    /// <summary>
    /// Генерирует узлы сетки для всей площади листа (абсолютные координаты).
    /// Defense-in-depth: если cols*rows > maxNodes — укрупняет шаг, никогда не возвращает пустой список из-за бюджета.
    /// Возвращает пустой список только если stepMicrons &lt;= 0, zoom &lt;= 0 или pixelSpacing &lt; MinPixelSpacing.
    /// </summary>
    List<GridNode> GenerateGridNodes(
        long stepMicrons,
        double zoom,
        long sheetWidthMicrons,
        long sheetHeightMicrons,
        int maxNodes);
}
