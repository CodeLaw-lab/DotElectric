using System.Windows;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;

namespace DotElectric.TemplateEditor.Behaviors;

public static class CoordinateTransform
{
    public static PointMicrons ToModelPoint(Point wpfPoint, double zoom, double sheetHeightMm)
    {
        var adjustedX = wpfPoint.X / zoom;
        var adjustedY = wpfPoint.Y / zoom;
        var modelMmY = sheetHeightMm - adjustedY;
        return new PointMicrons(
            Coordinate.ToMicrons(adjustedX),
            Coordinate.ToMicrons(modelMmY));
    }
}
