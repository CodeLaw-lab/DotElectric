using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Helpers;

public static class GridHelper
{
    /// <summary>
    /// Nice step sequence in microns, coarsest to finest.
    /// Each step is a CAD-friendly round value.
    /// </summary>
    private static readonly long[] NiceStepsMicrons =
    {
        50000, 30000, 20000, 15000, 10000, 7000, 5000, 3000, 2000, 1500, 1000, 700, 500
    };

    /// <summary>
    /// Computes the optimal display step for the current zoom, viewport, and user preference.
    /// Uses the preferred step as a target, then coarsens if pixel spacing is too small
    /// (below MinPixelSpacing) or if the node budget (MaxGridNodes) would be exceeded.
    /// </summary>
    public static long ComputeDisplayStep(double zoom, int maxNodes, long sheetWidthMicrons, long sheetHeightMicrons, long viewportWidthMicrons, long viewportHeightMicrons, long preferredStepMicrons = 0)
    {
        if (zoom <= 0 || maxNodes <= 0 || viewportWidthMicrons <= 0 || viewportHeightMicrons <= 0)
            return NiceStepsMicrons[0];

        // Determine target step: use preferredStep if it gives sufficient pixel spacing, else MinPixelSpacing-based
        long targetStepMicrons;
        double preferredPixelSpacing = preferredStepMicrons > 0 ? Coordinate.ToMm(preferredStepMicrons) * zoom : 0;
        if (preferredPixelSpacing >= EditorSettings.MinPixelSpacing)
        {
            targetStepMicrons = preferredStepMicrons;
        }
        else
        {
            double minPixelStepMm = EditorSettings.MinPixelSpacing / zoom;
            targetStepMicrons = (long)(minPixelStepMm * Coordinate.MicronsPerMm);
        }

        // Find nearest nice step to the target
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

        long vpW = Math.Min(viewportWidthMicrons, sheetWidthMicrons);
        long vpH = Math.Min(viewportHeightMicrons, sheetHeightMicrons);

        // Coarsen (increase step) until node budget is satisfied
        for (int i = nearestIdx; i >= 0; i--)
        {
            long step = NiceStepsMicrons[i];
            long cols = vpW / step + 1;
            long rows = vpH / step + 1;
            if (cols * rows <= maxNodes)
                return step;
        }

        return NiceStepsMicrons[0];
    }

    public readonly struct GridNode
    {
        public long XMicrons { get; }
        public long YMicrons { get; }

        public GridNode(long xMicrons, long yMicrons)
        {
            XMicrons = xMicrons;
            YMicrons = yMicrons;
        }
    }

    public static List<GridNode> GenerateGridNodes(
        Sheet sheet,
        long stepMicrons,
        double zoom,
        long viewportLeftMicrons,
        long viewportBottomMicrons,
        long viewportWidthMicrons,
        long viewportHeightMicrons,
        List<GridNode>? reuseList = null)
    {
        var nodes = reuseList ?? new List<GridNode>();
        nodes.Clear();

        if (stepMicrons <= 0 || zoom <= 0)
            return nodes;

        var pixelSpacing = Coordinate.ToMm(stepMicrons) * zoom;
        if (pixelSpacing < EditorSettings.MinPixelSpacing)
            return nodes;

        var viewportRight = Math.Min(viewportLeftMicrons + viewportWidthMicrons, sheet.WidthMicrons);
        var viewportTop = Math.Min(viewportBottomMicrons + viewportHeightMicrons, sheet.HeightMicrons);
        var viewportLeft = Math.Max(viewportLeftMicrons, 0);
        var viewportBottom = Math.Max(viewportBottomMicrons, 0);

        const int maxNodes = EditorSettings.MaxGridNodes;
        long cols = (viewportRight - viewportLeft) / stepMicrons + 1;
        long rows = (viewportTop - viewportBottom) / stepMicrons + 1;
        if (cols * rows > maxNodes)
        {
            return nodes;
        }

        var startX = ((viewportLeft + stepMicrons - 1) / stepMicrons) * stepMicrons;
        var startY = ((viewportBottom + stepMicrons - 1) / stepMicrons) * stepMicrons;

        for (long x = startX; x <= viewportRight; x += stepMicrons)
        {
            for (long y = startY; y <= viewportTop; y += stepMicrons)
            {
                nodes.Add(new GridNode(x, y));
            }
        }

        return nodes;
    }
}
