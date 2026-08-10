using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Helpers;

/// <summary>
/// Реализация <see cref="IGridNodeGenerator"/>.
/// Генерирует узлы для всей площади листа в абсолютных микронах (без viewport).
/// </summary>
public sealed class GridNodeGenerator : IGridNodeGenerator
{
    /// <summary>
    /// Nice step sequence in microns, coarsest to finest.
    /// Each step is a CAD-friendly round value.
    /// </summary>
    private static readonly long[] NiceStepsMicrons =
    {
        50000, 30000, 20000, 15000, 10000, 7000, 5000, 3000, 2000, 1500, 1000, 700, 500
    };

    public long ComputeDisplayStep(
        double zoom,
        int maxNodes,
        long sheetWidthMicrons,
        long sheetHeightMicrons,
        long preferredStepMicrons = 0)
    {
        if (zoom <= 0 || maxNodes <= 0 || sheetWidthMicrons <= 0 || sheetHeightMicrons <= 0)
            return NiceStepsMicrons[0];

        if (preferredStepMicrons > 0)
        {
            var preferredPixelSpacing = Coordinate.ToMm(preferredStepMicrons) * zoom;
            if (preferredPixelSpacing >= EditorSettings.MinPixelSpacing)
            {
                long cols = sheetWidthMicrons / preferredStepMicrons + 1;
                long rows = sheetHeightMicrons / preferredStepMicrons + 1;
                if (cols * rows <= maxNodes)
                    return preferredStepMicrons;
            }
        }

        double minPixelStepMm = EditorSettings.MinPixelSpacing / zoom;
        long targetStepMicrons = (long)(minPixelStepMm * Coordinate.MicronsPerMm);

        int nearestIdx = 0;
        long minDiff = long.MaxValue;
        for (int i = 0; i < NiceStepsMicrons.Length; i++)
        {
            long diff = Math.Abs(NiceStepsMicrons[i] - targetStepMicrons);
            if (diff < minDiff)
            {
                minDiff = diff;
                nearestIdx = i;
            }
        }

        for (int i = nearestIdx; i >= 0; i--)
        {
            long step = NiceStepsMicrons[i];
            long cols = sheetWidthMicrons / step + 1;
            long rows = sheetHeightMicrons / step + 1;
            if (cols * rows <= maxNodes)
                return step;
        }

        return NiceStepsMicrons[0];
    }

    public List<GridNode> GenerateGridNodes(
        long stepMicrons,
        double zoom,
        long sheetWidthMicrons,
        long sheetHeightMicrons,
        int maxNodes)
    {
        var nodes = new List<GridNode>();

        if (stepMicrons <= 0 || zoom <= 0 || maxNodes <= 0)
            return nodes;

        var pixelSpacing = Coordinate.ToMm(stepMicrons) * zoom;
        if (pixelSpacing < EditorSettings.MinPixelSpacing)
            return nodes;

        if (sheetWidthMicrons < 0 || sheetHeightMicrons < 0)
            return nodes;

        long cols = sheetWidthMicrons / stepMicrons + 1;
        long rows = sheetHeightMicrons / stepMicrons + 1;

        while (cols * rows > maxNodes)
        {
            stepMicrons *= 2;
            cols = sheetWidthMicrons / stepMicrons + 1;
            rows = sheetHeightMicrons / stepMicrons + 1;
        }

        for (long x = 0; x <= sheetWidthMicrons; x += stepMicrons)
        {
            for (long y = 0; y <= sheetHeightMicrons; y += stepMicrons)
            {
                nodes.Add(new GridNode(x, y));
            }
        }

        return nodes;
    }
}
