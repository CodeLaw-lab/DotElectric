using CommunityToolkit.Mvvm.ComponentModel;
using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Helpers;

namespace DotElectric.TemplateEditor.Models.Objects;

public abstract partial class TemplateObjectBase : ObservableObject
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public abstract long MicronsX { get; set; }
    public abstract long MicronsY { get; set; }

    public virtual double X => Coordinate.ToMm(MicronsX);
    public virtual double Y => Coordinate.ToMm(MicronsY);

    public virtual void Move(long micronsX, long micronsY)
    {
        MicronsX = micronsX;
        MicronsY = micronsY;
    }

    public abstract TemplateObjectBase Clone();
    public abstract bool ContainsPoint(PointMicrons point);
    public abstract RectMicrons GetBoundingBox();

    public abstract ResizeState CaptureResizeState();
    public abstract void ApplyResize(ResizeState state);
}

public record ResizeState(long X, long Y, long Width, long Height);
