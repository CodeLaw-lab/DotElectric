using DotElectric.TemplateEditor.Constants;
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

        bool affectsHorizontal = handle is ResizeHandle.Left or ResizeHandle.Right
            or ResizeHandle.TopLeft or ResizeHandle.TopRight
            or ResizeHandle.BottomLeft or ResizeHandle.BottomRight;
        bool affectsVertical = handle is ResizeHandle.Top or ResizeHandle.Bottom
            or ResizeHandle.TopLeft or ResizeHandle.TopRight
            or ResizeHandle.BottomLeft or ResizeHandle.BottomRight;

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
            if (handle is ResizeHandle.Left or ResizeHandle.TopLeft or ResizeHandle.BottomLeft)
                newX = startX + deltaX;
            if (handle is ResizeHandle.Right or ResizeHandle.TopRight or ResizeHandle.BottomRight)
                newRight = startRight + deltaX;
            if (handle is ResizeHandle.Top or ResizeHandle.TopRight or ResizeHandle.TopLeft)
                newTop = startTop + deltaY;
            if (handle is ResizeHandle.Bottom or ResizeHandle.BottomRight or ResizeHandle.BottomLeft)
                newY = startY + deltaY;
        }

        var (newXc, newYc, newRightC, newTopC) = ClampMinimumSize(newX, newY, newRight, newTop, handle, ctrlPressed, minSize);
        newX = newXc; newY = newYc; newRight = newRightC; newTop = newTopC;

        long newWidth = newRight - newX;
        long newHeight = newTop - newY;

        if (shiftPressed && handle is ResizeHandle.TopLeft or ResizeHandle.TopRight
                               or ResizeHandle.BottomLeft or ResizeHandle.BottomRight)
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

        bool isCorner = handle is ResizeHandle.BottomRight or ResizeHandle.BottomLeft
                        or ResizeHandle.TopRight or ResizeHandle.TopLeft;

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

        return (0, 0);
    }

    private static (long newX, long newY, long newRight, long newTop) ClampMinimumSize(
        long newX, long newY, long newRight, long newTop,
        ResizeHandle handle, bool ctrlPressed,
        long minSize)
    {
        bool leftMoves = handle is ResizeHandle.Left or ResizeHandle.TopLeft or ResizeHandle.BottomLeft;
        bool rightMoves = handle is ResizeHandle.Right or ResizeHandle.TopRight or ResizeHandle.BottomRight;
        bool bottomMoves = handle is ResizeHandle.Bottom or ResizeHandle.BottomLeft or ResizeHandle.BottomRight;
        bool topMoves = handle is ResizeHandle.Top or ResizeHandle.TopRight or ResizeHandle.TopLeft;

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
            _ => ToolCursor.Arrow
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
                _ => ToolCursor.Arrow
            };
        }

        // 0°, 180° и прочие — стандартное отображение
        return CursorForHandle(handle, isResizing: true, isLine: false);
    }
}
