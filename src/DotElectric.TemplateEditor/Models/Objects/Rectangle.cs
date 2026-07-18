using CommunityToolkit.Mvvm.ComponentModel;
using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Helpers;

namespace DotElectric.TemplateEditor.Models.Objects;

public partial class Rectangle : TemplateObjectBase
{
    private long _micronsX;
    private long _micronsY;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RightMicronsX))]
    [NotifyPropertyChangedFor(nameof(CenterMicronsX))]
    private long _widthMicrons;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BottomMicronsY))]
    [NotifyPropertyChangedFor(nameof(CenterMicronsY))]
    private long _heightMicrons;

    [ObservableProperty]
    private long _strokeThicknessMicrons = EditorSettings.DefaultStrokeThicknessMicrons;

    [ObservableProperty]
    private LineType _lineType;

    [ObservableProperty]
    private string _strokeColor = EditorSettings.DefaultStrokeColor;

    [ObservableProperty]
    private string _fillColor = EditorSettings.DefaultFillColor;

    public override long MicronsX
    {
        get => _micronsX;
        set
        {
            if (_micronsX == value) return;
            _micronsX = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(RightMicronsX));
            OnPropertyChanged(nameof(CenterMicronsX));
        }
    }

    public override long MicronsY
    {
        get => _micronsY;
        set
        {
            if (_micronsY == value) return;
            _micronsY = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BottomMicronsY));
            OnPropertyChanged(nameof(CenterMicronsY));
        }
    }

    public override double X => Coordinate.ToMm(MicronsX);
    public override double Y => Coordinate.ToMm(MicronsY);

    public long RightMicronsX => MicronsX + WidthMicrons;
    public long BottomMicronsY => MicronsY + HeightMicrons;
    public long CenterMicronsX => MicronsX + WidthMicrons / 2;
    public long CenterMicronsY => MicronsY + HeightMicrons / 2;
    public long HalfWidthMicrons => WidthMicrons / 2;
    public long HalfHeightMicrons => HeightMicrons / 2;

    public Rectangle()
    {
        Id = Guid.NewGuid().ToString();
    }

    public Rectangle(long micronsX, long micronsY, long widthMicrons, long heightMicrons,
        LineType lineType = LineType.Solid,
        long strokeThicknessMicrons = EditorSettings.DefaultStrokeThicknessMicrons,
        string? strokeColor = null, string? fillColor = null) : this()
    {
        if (widthMicrons < 0)
            throw new ArgumentOutOfRangeException(nameof(widthMicrons), "Width cannot be negative");
        if (heightMicrons < 0)
            throw new ArgumentOutOfRangeException(nameof(heightMicrons), "Height cannot be negative");
        MicronsX = micronsX;
        MicronsY = micronsY;
        WidthMicrons = widthMicrons;
        HeightMicrons = heightMicrons;
        LineType = lineType;
        StrokeThicknessMicrons = strokeThicknessMicrons;
        StrokeColor = strokeColor ?? EditorSettings.DefaultStrokeColor;
        FillColor = fillColor ?? EditorSettings.DefaultFillColor;
    }

    public override void Move(long micronsX, long micronsY)
    {
        MicronsX = micronsX;
        MicronsY = micronsY;
    }

    public override TemplateObjectBase Clone()
    {
        return new Rectangle(MicronsX, MicronsY, WidthMicrons, HeightMicrons, LineType, StrokeThicknessMicrons, StrokeColor, FillColor)
        {
            Id = Guid.NewGuid().ToString()
        };
    }

    public override bool ContainsPoint(PointMicrons point)
    {
        long tol = PhysicalConstants.LineHitToleranceMicrons;

        bool insideExpanded = point.MicronsX >= MicronsX - tol &&
                              point.MicronsX <= RightMicronsX + tol &&
                              point.MicronsY >= MicronsY - tol &&
                              point.MicronsY <= BottomMicronsY + tol;

        if (!insideExpanded)
            return false;

        bool insideInterior = point.MicronsX >= MicronsX + tol &&
                              point.MicronsX <= RightMicronsX - tol &&
                              point.MicronsY >= MicronsY + tol &&
                              point.MicronsY <= BottomMicronsY - tol;

        return !insideInterior;
    }

    public override RectMicrons GetBoundingBox()
    {
        return new RectMicrons(MicronsX, MicronsY, RightMicronsX, BottomMicronsY);
    }

    public override ResizeState CaptureResizeState() =>
        new(MicronsX, MicronsY, WidthMicrons, HeightMicrons);

    public override void ApplyResize(ResizeState state)
    {
        MicronsX = state.X;
        MicronsY = state.Y;
        WidthMicrons = state.Width;
        HeightMicrons = state.Height;
    }
}
