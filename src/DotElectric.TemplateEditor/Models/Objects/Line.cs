using CommunityToolkit.Mvvm.ComponentModel;
using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Helpers;

namespace DotElectric.TemplateEditor.Models.Objects;

public partial class Line : TemplateObjectBase
{
    [ObservableProperty]
    private long _startMicronsX;

    [ObservableProperty]
    private long _startMicronsY;

    [ObservableProperty]
    private long _endMicronsX;

    [ObservableProperty]
    private long _endMicronsY;

    [ObservableProperty]
    private long _strokeThicknessMicrons = EditorSettings.DefaultStrokeThicknessMicrons;

    [ObservableProperty]
    private LineType _lineType;

    [ObservableProperty]
    private string _strokeColor = EditorSettings.DefaultStrokeColor;

    public override long MicronsX
    {
        get => StartMicronsX;
        set => StartMicronsX = value;
    }

    public override long MicronsY
    {
        get => StartMicronsY;
        set => StartMicronsY = value;
    }

    public Line()
    {
        Id = Guid.NewGuid().ToString();
    }

    public Line(long startMicronsX, long startMicronsY, long endMicronsX, long endMicronsY,
        LineType lineType = LineType.Solid,
        long strokeThicknessMicrons = EditorSettings.DefaultStrokeThicknessMicrons,
        string? strokeColor = null) : this()
    {
        StartMicronsX = startMicronsX;
        StartMicronsY = startMicronsY;
        EndMicronsX = endMicronsX;
        EndMicronsY = endMicronsY;
        LineType = lineType;
        StrokeThicknessMicrons = strokeThicknessMicrons;
        StrokeColor = strokeColor ?? EditorSettings.DefaultStrokeColor;
    }

    public override void Move(long micronsX, long micronsY)
    {
        var deltaX = micronsX - StartMicronsX;
        var deltaY = micronsY - StartMicronsY;

        StartMicronsX = micronsX;
        StartMicronsY = micronsY;
        EndMicronsX += deltaX;
        EndMicronsY += deltaY;
    }

    public override TemplateObjectBase Clone()
    {
        return new Line(StartMicronsX, StartMicronsY, EndMicronsX, EndMicronsY, LineType, StrokeThicknessMicrons, StrokeColor)
        {
            Id = Guid.NewGuid().ToString()
        };
    }

    public override bool ContainsPoint(PointMicrons point)
    {
        var distance = DistanceFromPointToLine(
            point,
            new PointMicrons(StartMicronsX, StartMicronsY),
            new PointMicrons(EndMicronsX, EndMicronsY));
        return distance <= PhysicalConstants.LineHitToleranceMicrons;
    }

    public override RectMicrons GetBoundingBox()
    {
        return new RectMicrons(
            Math.Min(StartMicronsX, EndMicronsX),
            Math.Min(StartMicronsY, EndMicronsY),
            Math.Max(StartMicronsX, EndMicronsX),
            Math.Max(StartMicronsY, EndMicronsY));
    }

    public override ResizeState CaptureResizeState() =>
        new(StartMicronsX, StartMicronsY, EndMicronsX - StartMicronsX, EndMicronsY - StartMicronsY);

    public override void ApplyResize(ResizeState state)
    {
        StartMicronsX = state.X;
        StartMicronsY = state.Y;
        EndMicronsX = state.X + state.Width;
        EndMicronsY = state.Y + state.Height;
    }

    private static long DistanceFromPointToLine(
        PointMicrons point,
        PointMicrons lineStart,
        PointMicrons lineEnd)
    {
        long dx = lineEnd.MicronsX - lineStart.MicronsX;
        long dy = lineEnd.MicronsY - lineStart.MicronsY;
        long lengthSquared = dx * dx + dy * dy;

        if (lengthSquared == 0)
            return point.DistanceTo(lineStart);

        long t = (point.MicronsX - lineStart.MicronsX) * dx +
                 (point.MicronsY - lineStart.MicronsY) * dy;
        t = Math.Max(0, Math.Min(lengthSquared, t));

        long closestX = lineStart.MicronsX + (t * dx) / lengthSquared;
        long closestY = lineStart.MicronsY + (t * dy) / lengthSquared;

        return point.DistanceTo(new PointMicrons(closestX, closestY));
    }
}
