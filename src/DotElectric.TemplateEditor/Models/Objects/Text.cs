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

    public long VisualLeft => GetBoundingBox().Left;
    public long VisualBottom => GetBoundingBox().Bottom;
    public long VisualRight => GetBoundingBox().Right;
    public long VisualTop => GetBoundingBox().Top;

    // Повёрнутые углы (для маркеров выделения)
    public long RotatedCorner0X => MicronsX;
    public long RotatedCorner0Y => MicronsY + HeightMicrons;

    public long RotatedCorner1X => MicronsX + (long)Math.Round(WidthMicrons * Math.Cos(RotationAngle * Math.PI / 180.0));
    public long RotatedCorner1Y => (MicronsY + HeightMicrons) - (long)Math.Round(WidthMicrons * Math.Sin(RotationAngle * Math.PI / 180.0));

    public long RotatedCorner2X => MicronsX - (long)Math.Round(HeightMicrons * Math.Sin(RotationAngle * Math.PI / 180.0));
    public long RotatedCorner2Y => (MicronsY + HeightMicrons) - (long)Math.Round(HeightMicrons * Math.Cos(RotationAngle * Math.PI / 180.0));

    public long RotatedCorner3X => MicronsX + (long)Math.Round(WidthMicrons * Math.Cos(RotationAngle * Math.PI / 180.0) - HeightMicrons * Math.Sin(RotationAngle * Math.PI / 180.0));
    public long RotatedCorner3Y => (MicronsY + HeightMicrons) - (long)Math.Round(WidthMicrons * Math.Sin(RotationAngle * Math.PI / 180.0) + HeightMicrons * Math.Cos(RotationAngle * Math.PI / 180.0));

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

        var cpX = point.MicronsX - MicronsX;
        var cpY = (MicronsY + HeightMicrons) - point.MicronsY;

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

        var topY = MicronsY + HeightMicrons;

        var corners = new[] {
            (0L, 0L),
            (w, 0L),
            (0L, h),
            (w, h)
        };

        long minX = long.MaxValue, minY = long.MaxValue;
        long maxX = long.MinValue, maxY = long.MinValue;

        foreach (var (lx, ly) in corners)
        {
            var cpX = lx * cosA - ly * sinA;
            var cpY = lx * sinA + ly * cosA;

            var wx = MicronsX + (long)Math.Round(cpX);
            var wy = topY - (long)Math.Round(cpY);
            if (wx < minX) minX = wx;
            if (wy < minY) minY = wy;
            if (wx > maxX) maxX = wx;
            if (wy > maxY) maxY = wy;
        }

        return new RectMicrons(minX, minY, maxX, maxY);
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
