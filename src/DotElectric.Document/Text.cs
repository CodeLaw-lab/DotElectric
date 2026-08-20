using CommunityToolkit.Mvvm.ComponentModel;

namespace DotElectric.Document;

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
            NotifyAllRotatedCorners();
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WidthMicrons))]
    [NotifyPropertyChangedFor(nameof(HeightMicrons))]
    [NotifyPropertyChangedFor(nameof(LineCount))]
    [NotifyPropertyChangedFor(nameof(RightMicronsX))]
    [NotifyPropertyChangedFor(nameof(CenterMicronsX))]
    private string _content = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WidthMicrons))]
    [NotifyPropertyChangedFor(nameof(HeightMicrons))]
    [NotifyPropertyChangedFor(nameof(LineCount))]
    [NotifyPropertyChangedFor(nameof(RightMicronsX))]
    [NotifyPropertyChangedFor(nameof(BottomMicronsY))]
    [NotifyPropertyChangedFor(nameof(CenterMicronsX))]
    [NotifyPropertyChangedFor(nameof(CenterMicronsY))]
    private long _fontSizeMicrons;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WidthMicrons))]
    private string _fontName = DocumentDefaults.DefaultFontName;

    [ObservableProperty]
    private TextType _textType;

    [ObservableProperty]
    private string? _key;

    [ObservableProperty]
    private bool _isEditable;

    [ObservableProperty]
    private string? _defaultValue;

    [ObservableProperty]
    private string _foreground = DocumentDefaults.DefaultTextForeground;

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
            var heightRatio = FontMetricsProvider.Current.GetHeightRatio(FontName);
            if (lc <= 1) return (long)(FontSizeMicrons * heightRatio);
            return (long)(FontSizeMicrons * heightRatio * (1 + (lc - 1) * LineSpacingFactor));
        }
    }

    public long WidthMicrons
    {
        get
        {
            if (string.IsNullOrEmpty(Content)) return FontSizeMicrons;
            var factor = FontMetricsProvider.Current.GetAdvWidthRatio(FontName);
            var maxLen = Content.Split('\n').Max(l => l.Length);
            return (long)Math.Max(FontSizeMicrons, maxLen * FontSizeMicrons * factor);
        }
    }

    public long RightMicronsX => MicronsX + WidthMicrons;
    public long BottomMicronsY => MicronsY + HeightMicrons;
    public long CenterMicronsX => MicronsX + WidthMicrons / 2;
    public long CenterMicronsY => MicronsY + HeightMicrons / 2;

    // Повёрнутые углы (для маркеров выделения) — делегации в TextGeometry,
    // учитывающие LayoutTransform offset WPF.
    public long RotatedCorner0X => TextGeometry.Corner(this, 0).MicronsX;
    public long RotatedCorner0Y => TextGeometry.Corner(this, 0).MicronsY;
    public long RotatedCorner1X => TextGeometry.Corner(this, 1).MicronsX;
    public long RotatedCorner1Y => TextGeometry.Corner(this, 1).MicronsY;
    public long RotatedCorner2X => TextGeometry.Corner(this, 2).MicronsX;
    public long RotatedCorner2Y => TextGeometry.Corner(this, 2).MicronsY;
    public long RotatedCorner3X => TextGeometry.Corner(this, 3).MicronsX;
    public long RotatedCorner3Y => TextGeometry.Corner(this, 3).MicronsY;

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
        string? key = null, bool isEditable = true, string? defaultValue = null,
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
        Foreground = foreground ?? DocumentDefaults.DefaultTextForeground;
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

    public override bool ContainsPoint(PointMicrons point) => TextGeometry.Contains(this, point);

    public override RectMicrons GetBoundingBox() => TextGeometry.BoundingBox(this);

    public override ResizeState CaptureResizeState() =>
        new(MicronsX, MicronsY, WidthMicrons, FontSizeMicrons);

    public override void ApplyResize(ResizeState state)
    {
        MicronsX = state.X;
        MicronsY = state.Y;
        FontSizeMicrons = state.Height;
    }
}
