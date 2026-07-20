using CommunityToolkit.Mvvm.ComponentModel;
using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Helpers;

namespace DotElectric.TemplateEditor.Models.Objects;

public partial class Text : TemplateObjectBase
{
    private const double LineSpacingFactor = 1.3;
    private long _micronsX;
    private long _micronsY;
    private int _rotationAngle;

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
            OnPropertyChanged(nameof(VisualLeft));
            OnPropertyChanged(nameof(VisualRight));
            NotifyAllRotatedCorners();
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
            OnPropertyChanged(nameof(VisualBottom));
            OnPropertyChanged(nameof(VisualTop));
            NotifyAllRotatedCorners();
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WidthMicrons))]
    [NotifyPropertyChangedFor(nameof(HeightMicrons))]
    [NotifyPropertyChangedFor(nameof(LineCount))]
    [NotifyPropertyChangedFor(nameof(RightMicronsX))]
    [NotifyPropertyChangedFor(nameof(CenterMicronsX))]
    [NotifyPropertyChangedFor(nameof(VisualLeft))]
    [NotifyPropertyChangedFor(nameof(VisualRight))]
    [NotifyPropertyChangedFor(nameof(VisualBottom))]
    [NotifyPropertyChangedFor(nameof(VisualTop))]
    private string _content = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WidthMicrons))]
    [NotifyPropertyChangedFor(nameof(HeightMicrons))]
    [NotifyPropertyChangedFor(nameof(LineCount))]
    [NotifyPropertyChangedFor(nameof(RightMicronsX))]
    [NotifyPropertyChangedFor(nameof(BottomMicronsY))]
    [NotifyPropertyChangedFor(nameof(CenterMicronsX))]
    [NotifyPropertyChangedFor(nameof(CenterMicronsY))]
    [NotifyPropertyChangedFor(nameof(VisualLeft))]
    [NotifyPropertyChangedFor(nameof(VisualRight))]
    [NotifyPropertyChangedFor(nameof(VisualBottom))]
    [NotifyPropertyChangedFor(nameof(VisualTop))]
    private long _fontSizeMicrons;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WidthMicrons))]
    private string _fontName = EditorSettings.DefaultFontName;

    [ObservableProperty]
    private TextType _textType;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(VisualLeft))]
    [NotifyPropertyChangedFor(nameof(VisualRight))]
    [NotifyPropertyChangedFor(nameof(VisualBottom))]
    [NotifyPropertyChangedFor(nameof(VisualTop))]
    private string? _key;

    [ObservableProperty]
    private bool _isEditable;

    [ObservableProperty]
    private string? _defaultValue;

    [ObservableProperty]
    private string _foreground = EditorSettings.DefaultTextForeground;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WidthMicrons))]
    [NotifyPropertyChangedFor(nameof(HeightMicrons))]
    [NotifyPropertyChangedFor(nameof(RightMicronsX))]
    [NotifyPropertyChangedFor(nameof(BottomMicronsY))]
    private bool _textWrapping;

    [ObservableProperty]
    private string _textAlignment = "Left";

    public int RotationAngle
    {
        get => _rotationAngle;
        set
        {
            var normalized = ((value % 360) + 360) % 360;
            if (_rotationAngle == normalized) return;
            _rotationAngle = normalized;
            OnPropertyChanged();
            OnPropertyChanged(nameof(VisualLeft));
            OnPropertyChanged(nameof(VisualRight));
            OnPropertyChanged(nameof(VisualBottom));
            OnPropertyChanged(nameof(VisualTop));
            NotifyAllRotatedCorners();
        }
    }

    public override double X => Coordinate.ToMm(MicronsX);
    public override double Y => Coordinate.ToMm(MicronsY);

    public int LineCount => string.IsNullOrEmpty(Content) ? 1 : Content.Count(c => c == '\n') + 1;

    public long HeightMicrons
    {
        get
        {
            var lc = LineCount;
            var heightRatio = FontMetrics.Default.GetHeightRatio(FontName);
            if (lc <= 1) return (long)(FontSizeMicrons * heightRatio);
            return (long)(FontSizeMicrons * heightRatio * (1 + (lc - 1) * LineSpacingFactor));
        }
    }

    public long WidthMicrons
    {
        get
        {
            if (string.IsNullOrEmpty(Content)) return FontSizeMicrons;
            var factor = FontMetrics.Default.GetAdvWidthRatio(FontName);
            var maxLen = Content.Split('\n').Max(l => l.Length);
            return (long)Math.Max(FontSizeMicrons, maxLen * FontSizeMicrons * factor);
        }
    }

    public long RightMicronsX => MicronsX + WidthMicrons;
    public long BottomMicronsY => MicronsY + HeightMicrons;
    public long CenterMicronsX => MicronsX + WidthMicrons / 2;
    public long CenterMicronsY => MicronsY + HeightMicrons / 2;
    public bool RotationAngleValid => true;

    /// <summary>
    /// WPF LayoutTransform offset: the framework positions a transformed element so
    /// the top-left of the transformed bounding box (not the local origin) lands at
    /// the layout position. This computes the offset between the anchor
    /// (MicronsX, MicronsY+HeightMicrons) and the actual visual rotation center.
    /// Returns (minX, minY) in microns; the offset is applied as (-minX, +minY).
    /// </summary>
    private (long offsetX, long offsetY) GetLayoutTransformOffset()
    {
        var w = WidthMicrons;
        var h = HeightMicrons;
        var angleRad = RotationAngle * Math.PI / 180.0;
        var cosA = Math.Cos(angleRad);
        var sinA = Math.Sin(angleRad);

        // Four corners of the local Y-down box after rotation around origin (0,0):
        // (0,0), (W·cos, W·sin), (-H·sin, H·cos), (W·cos-H·sin, W·sin+H·cos)
        var c1x = w * cosA;
        var c1y = w * sinA;
        var c2x = -h * sinA;
        var c2y = h * cosA;
        var c3x = w * cosA - h * sinA;
        var c3y = w * sinA + h * cosA;

        var minX = Math.Min(0, Math.Min(Math.Min(c1x, c2x), c3x));
        var minY = Math.Min(0, Math.Min(Math.Min(c1y, c2y), c3y));

        return ((long)Math.Round(minX), (long)Math.Round(minY));
    }

    public long VisualLeft => GetBoundingBox().Left;
    public long VisualBottom => GetBoundingBox().Bottom;
    public long VisualRight => GetBoundingBox().Right;
    public long VisualTop => GetBoundingBox().Top;

    // Повёрнутые углы (для маркеров выделения) — с учётом LayoutTransform offset.
    // WPF позиционирует трансформированный элемент по верхнему левому углу
    // bounding box, а не по origin (0,0), поэтому добавляем смещение (-minX, +minY).
    public long RotatedCorner0X
    {
        get
        {
            var (minX, _) = GetLayoutTransformOffset();
            return MicronsX - minX;
        }
    }
    public long RotatedCorner0Y
    {
        get
        {
            var (_, minY) = GetLayoutTransformOffset();
            return MicronsY + HeightMicrons + minY;
        }
    }

    public long RotatedCorner1X
    {
        get
        {
            var (minX, _) = GetLayoutTransformOffset();
            var angleRad = RotationAngle * Math.PI / 180.0;
            return MicronsX + (long)Math.Round(WidthMicrons * Math.Cos(angleRad)) - minX;
        }
    }
    public long RotatedCorner1Y
    {
        get
        {
            var (_, minY) = GetLayoutTransformOffset();
            var angleRad = RotationAngle * Math.PI / 180.0;
            return (MicronsY + HeightMicrons) - (long)Math.Round(WidthMicrons * Math.Sin(angleRad)) + minY;
        }
    }

    public long RotatedCorner2X
    {
        get
        {
            var (minX, _) = GetLayoutTransformOffset();
            var angleRad = RotationAngle * Math.PI / 180.0;
            return MicronsX - (long)Math.Round(HeightMicrons * Math.Sin(angleRad)) - minX;
        }
    }
    public long RotatedCorner2Y
    {
        get
        {
            var (_, minY) = GetLayoutTransformOffset();
            var angleRad = RotationAngle * Math.PI / 180.0;
            return (MicronsY + HeightMicrons) - (long)Math.Round(HeightMicrons * Math.Cos(angleRad)) + minY;
        }
    }

    public long RotatedCorner3X
    {
        get
        {
            var (minX, _) = GetLayoutTransformOffset();
            var angleRad = RotationAngle * Math.PI / 180.0;
            return MicronsX + (long)Math.Round(WidthMicrons * Math.Cos(angleRad) - HeightMicrons * Math.Sin(angleRad)) - minX;
        }
    }
    public long RotatedCorner3Y
    {
        get
        {
            var (_, minY) = GetLayoutTransformOffset();
            var angleRad = RotationAngle * Math.PI / 180.0;
            return (MicronsY + HeightMicrons) - (long)Math.Round(WidthMicrons * Math.Sin(angleRad) + HeightMicrons * Math.Cos(angleRad)) + minY;
        }
    }

    private void NotifyAllRotatedCorners()
    {
        OnPropertyChanged(nameof(RotatedCorner0X));
        OnPropertyChanged(nameof(RotatedCorner0Y));
        OnPropertyChanged(nameof(RotatedCorner1X));
        OnPropertyChanged(nameof(RotatedCorner1Y));
        OnPropertyChanged(nameof(RotatedCorner2X));
        OnPropertyChanged(nameof(RotatedCorner2Y));
        OnPropertyChanged(nameof(RotatedCorner3X));
        OnPropertyChanged(nameof(RotatedCorner3Y));
    }

    partial void OnFontSizeMicronsChanged(long value) => NotifyAllRotatedCorners();
    partial void OnContentChanged(string value) => NotifyAllRotatedCorners();
    partial void OnFontNameChanged(string value) => NotifyAllRotatedCorners();
    partial void OnTextWrappingChanged(bool value) => NotifyAllRotatedCorners();

    public Text()
    {
        Id = Guid.NewGuid().ToString();
    }

    public Text(long micronsX, long micronsY, string content, long fontSizeMicrons,
        string fontName = "ГОСТ А", TextType textType = TextType.Text, int rotationAngle = 0,
        string? key = null, bool isEditable = false, string? defaultValue = null,
        string? foreground = null, bool textWrapping = false,
        string textAlignment = "Left") : this()
    {
        MicronsX = micronsX;
        MicronsY = micronsY;
        Content = content;
        FontSizeMicrons = fontSizeMicrons;
        FontName = fontName;
        TextType = textType;
        RotationAngle = rotationAngle;
        Key = key;
        IsEditable = isEditable;
        DefaultValue = defaultValue;
        Foreground = foreground ?? EditorSettings.DefaultTextForeground;
        TextWrapping = textWrapping;
        TextAlignment = textAlignment;
    }

    public override void Move(long micronsX, long micronsY)
    {
        MicronsX = micronsX;
        MicronsY = micronsY;
    }

    public override TemplateObjectBase Clone()
    {
        return new Text(MicronsX, MicronsY, Content, FontSizeMicrons, FontName, TextType, RotationAngle,
            Key, IsEditable, DefaultValue, Foreground, TextWrapping, TextAlignment)
        {
            Id = Guid.NewGuid().ToString()
        };
    }

    public override bool ContainsPoint(PointMicrons point)
    {
        var w = WidthMicrons;
        var h = HeightMicrons;
        var angleRad = RotationAngle * Math.PI / 180.0;
        var cosA = Math.Cos(angleRad);
        var sinA = Math.Sin(angleRad);

        var (minX, minY) = GetLayoutTransformOffset();
        // Фактический центр вращения с учётом LayoutTransform offset
        var centerX = MicronsX - minX;
        var centerY = MicronsY + HeightMicrons + minY;

        var cpX = point.MicronsX - centerX;
        var cpY = centerY - point.MicronsY;

        var u = cpX * cosA + cpY * sinA;
        var v = -cpX * sinA + cpY * cosA;

        return u >= 0 && u <= w && v >= 0 && v <= h;
    }

    public override RectMicrons GetBoundingBox()
    {
        var w = WidthMicrons;
        var h = HeightMicrons;
        var angleRad = RotationAngle * Math.PI / 180.0;
        var cosA = Math.Cos(angleRad);
        var sinA = Math.Sin(angleRad);

        var (minX, minY) = GetLayoutTransformOffset();
        // Фактический центр вращения с учётом LayoutTransform offset
        var centerX = MicronsX - minX;
        var centerY = MicronsY + HeightMicrons + minY;

        var corners = new[] {
            (0L, 0L),
            (w, 0L),
            (0L, h),
            (w, h)
        };

        long minXbb = long.MaxValue, minYbb = long.MaxValue;
        long maxXbb = long.MinValue, maxYbb = long.MinValue;

        foreach (var (lx, ly) in corners)
        {
            var cpX = lx * cosA - ly * sinA;
            var cpY = lx * sinA + ly * cosA;

            var wx = centerX + (long)Math.Round(cpX);
            var wy = centerY - (long)Math.Round(cpY);
            if (wx < minXbb) minXbb = wx;
            if (wy < minYbb) minYbb = wy;
            if (wx > maxXbb) maxXbb = wx;
            if (wy > maxYbb) maxYbb = wy;
        }

        return new RectMicrons(minXbb, minYbb, maxXbb, maxYbb);
    }

    public override ResizeState CaptureResizeState() =>
        new(MicronsX, MicronsY, WidthMicrons, FontSizeMicrons);

    public override void ApplyResize(ResizeState state)
    {
        MicronsX = state.X;
        MicronsY = state.Y;
        FontSizeMicrons = state.Height;
    }
}
