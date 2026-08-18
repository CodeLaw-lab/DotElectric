using DotElectric.TemplateEditor.Helpers;

namespace DotElectric.TemplateEditor.Tools;

public static class ResizeMath
{
    public static (long newX, long newY, long newWidth, long newHeight) ComputeRectangleResize(
        long startX, long startY, long startWidth, long startHeight,
        double dx, double dy,
        ResizeHandle handle,
        bool shiftPressed, bool ctrlPressed,
        bool snapEnabled, long stepMicrons,
        long sheetW, long sheetH,
        long minSize)
    {
        long startRight = startX + startWidth;
        long startTop = startY + startHeight;
        long deltaX = (long)dx;
        long deltaY = (long)dy;

        bool affectsHorizontal = MarkerLayout.TouchesLeft(handle) || MarkerLayout.TouchesRight(handle);
        bool affectsVertical = MarkerLayout.TouchesTop(handle) || MarkerLayout.TouchesBottom(handle);

        long newX = startX;
        long newY = startY;
        long newRight = startRight;
        long newTop = startTop;

        if (ctrlPressed)
        {
            if (affectsHorizontal)
            {
                newX = startX - deltaX;
                newRight = startRight + deltaX;
            }
            if (affectsVertical)
            {
                newY = startY - deltaY;
                newTop = startTop + deltaY;
            }
        }
        else
        {
            if (MarkerLayout.TouchesLeft(handle))
                newX = startX + deltaX;
            if (MarkerLayout.TouchesRight(handle))
                newRight = startRight + deltaX;
            if (MarkerLayout.TouchesTop(handle))
                newTop = startTop + deltaY;
            if (MarkerLayout.TouchesBottom(handle))
                newY = startY + deltaY;
        }

        var (newXc, newYc, newRightC, newTopC) = ClampMinimumSize(newX, newY, newRight, newTop, handle, ctrlPressed, minSize);
        newX = newXc; newY = newYc; newRight = newRightC; newTop = newTopC;

        long newWidth = newRight - newX;
        long newHeight = newTop - newY;

        if (shiftPressed && MarkerLayout.IsCorner(handle))
        {
            double aspect = (double)startWidth / startHeight;

            if (Math.Abs(deltaX) >= Math.Abs(deltaY))
                newHeight = Math.Max(minSize, (long)(newWidth / aspect));
            else
                newWidth = Math.Max(minSize, (long)(newHeight * aspect));

            switch (handle)
            {
                case ResizeHandle.BottomRight:
                    newX = startX;
                    newY = startTop - newHeight;
                    newRight = newX + newWidth;
                    newTop = startTop;
                    break;
                case ResizeHandle.BottomLeft:
                    newX = startRight - newWidth;
                    newY = startTop - newHeight;
                    newRight = startRight;
                    newTop = startTop;
                    break;
                case ResizeHandle.TopRight:
                    newX = startX;
                    newY = startY;
                    newRight = newX + newWidth;
                    newTop = newY + newHeight;
                    break;
                case ResizeHandle.TopLeft:
                    newX = startRight - newWidth;
                    newY = startY;
                    newRight = startRight;
                    newTop = newY + newHeight;
                    break;
            }
        }

        if (snapEnabled && stepMicrons > 0)
        {
            newX = SnapHelper.SnapX(newX, stepMicrons);
            newY = SnapHelper.SnapY(newY, stepMicrons);
            newWidth = SnapHelper.SnapSize(newWidth, stepMicrons);
            newHeight = SnapHelper.SnapSize(newHeight, stepMicrons);
        }

        newWidth = Math.Max(minSize, newWidth);
        newHeight = Math.Max(minSize, newHeight);

        newX = Math.Clamp(newX, 0, sheetW);
        newY = Math.Clamp(newY, 0, sheetH);
        newWidth = Math.Max(minSize, Math.Min(newWidth, sheetW - newX));
        newHeight = Math.Max(minSize, Math.Min(newHeight, sheetH - newY));

        return (newX, newY, newWidth, newHeight);
    }

    public static (long newX, long newY, long newFontSize) ComputeTextResize(
        long startX, long startY, long startWidth, long startHeight,
        double dx, double dy,
        ResizeHandle handle,
        bool ctrlPressed,
        bool snapEnabled, long stepMicrons,
        long sheetW, long sheetH,
        long minFontSize,
        int rotationAngle = 0)
    {
        // Проекция dx/dy в локальную СК текста (обратная матрица поворота)
        var angleRad = rotationAngle * Math.PI / 180.0;
        var cosA = Math.Cos(angleRad);
        var sinA = Math.Sin(angleRad);
        var dxLocal = dx * cosA + dy * sinA;
        var dyLocal = -dx * sinA + dy * cosA;

        bool isCorner = MarkerLayout.IsCorner(handle);

        if (!isCorner)
            return (Math.Clamp(startX + (long)dx, 0, sheetW), Math.Clamp(startY + (long)dy, 0, sheetH), startHeight);

        double scale;
        if (ctrlPressed)
        {
            var absDx = Math.Abs(dxLocal);
            var absDy = Math.Abs(dyLocal);
            var maxDelta = Math.Max(absDx, absDy);
            if (maxDelta < 1)
                scale = 1.0;
            else
            {
                var sign = (dxLocal + dyLocal) >= 0 ? 1 : -1;
                scale = (startHeight + sign * maxDelta) / (double)startHeight;
            }
        }
        else
        {
            if (Math.Abs(dyLocal) >= Math.Abs(dxLocal))
                scale = (startHeight + dyLocal * 1) / (double)startHeight;
            else
                scale = (startWidth + dxLocal * 1) / (double)startWidth;
        }

        scale = Math.Max(scale, (double)minFontSize / Math.Max(startHeight, 1));

        var newFontSize = (long)Math.Round(startHeight * scale);

        if (snapEnabled && stepMicrons > 0)
            newFontSize = Math.Max(minFontSize, SnapHelper.SnapSize(newFontSize, stepMicrons));

        newFontSize = Math.Max(minFontSize, newFontSize);

        long newX = startX;
        long newY = startY;

        if (!ctrlPressed)
        {
            long newWidth = (long)Math.Max(minFontSize, startWidth * (newFontSize / (double)startHeight));
            long deltaW = newWidth - startWidth;
            long deltaH = newFontSize - startHeight;

            switch (handle)
            {
                case ResizeHandle.BottomRight:
                    newX = startX;
                    newY = startY;
                    break;
                case ResizeHandle.TopRight:
                    newX = startX;
                    newY = startY - deltaH;
                    break;
                case ResizeHandle.BottomLeft:
                    newX = startX - deltaW;
                    newY = startY;
                    break;
                case ResizeHandle.TopLeft:
                    newX = startX - deltaW;
                    newY = startY - deltaH;
                    break;
            }
        }

        newX = Math.Clamp(newX, 0, sheetW);
        newY = Math.Clamp(newY, 0, sheetH);

        return (newX, newY, newFontSize);
    }

    public static (long newX, long newY) ComputeLineEndpoint(
        double dx, double dy,
        ResizeHandle handle,
        long lineStartX, long lineStartY,
        long lineEndX, long lineEndY,
        bool snapEnabled, long stepMicrons,
        long sheetW, long sheetH)
    {
        if (handle == ResizeHandle.BottomRight)
        {
            var newX = lineEndX + (long)dx;
            var newY = lineEndY + (long)dy;
            if (snapEnabled && stepMicrons > 0)
            {
                newX = SnapHelper.SnapX(newX, stepMicrons);
                newY = SnapHelper.SnapY(newY, stepMicrons);
            }
            return (Math.Clamp(newX, 0, sheetW), Math.Clamp(newY, 0, sheetH));
        }

        if (handle == ResizeHandle.TopLeft)
        {
            var newX = lineStartX + (long)dx;
            var newY = lineStartY + (long)dy;
            if (snapEnabled && stepMicrons > 0)
            {
                newX = SnapHelper.SnapX(newX, stepMicrons);
                newY = SnapHelper.SnapY(newY, stepMicrons);
            }
            return (Math.Clamp(newX, 0, sheetW), Math.Clamp(newY, 0, sheetH));
        }

        // У линии только два маркера — начало (TopLeft) и конец (BottomRight)
        throw new NotSupportedException($"У линии нет маркера {handle}.");
    }

    private static (long newX, long newY, long newRight, long newTop) ClampMinimumSize(
        long newX, long newY, long newRight, long newTop,
        ResizeHandle handle, bool ctrlPressed,
        long minSize)
    {
        bool leftMoves = MarkerLayout.TouchesLeft(handle);
        bool rightMoves = MarkerLayout.TouchesRight(handle);
        bool bottomMoves = MarkerLayout.TouchesBottom(handle);
        bool topMoves = MarkerLayout.TouchesTop(handle);

        if (ctrlPressed)
        {
            leftMoves = true;
            rightMoves = true;
            bottomMoves = true;
            topMoves = true;
        }

        if (leftMoves && !rightMoves)
            newX = Math.Min(newX, newRight - minSize);
        else if (rightMoves && !leftMoves)
            newRight = Math.Max(newRight, newX + minSize);
        else if (leftMoves && rightMoves && newRight < newX + minSize)
        {
            long mid = (newX + newRight) / 2;
            newX = mid - minSize / 2;
            newRight = mid + minSize / 2;
        }

        if (bottomMoves && !topMoves)
            newY = Math.Min(newY, newTop - minSize);
        else if (topMoves && !bottomMoves)
            newTop = Math.Max(newTop, newY + minSize);
        else if (topMoves && bottomMoves && newTop < newY + minSize)
        {
            long mid = (newY + newTop) / 2;
            newY = mid - minSize / 2;
            newTop = mid + minSize / 2;
        }

        return (newX, newY, newRight, newTop);
    }
}
