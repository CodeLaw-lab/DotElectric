using CommunityToolkit.Mvvm.ComponentModel;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.ViewModels.Managers;

/// <summary>
/// Управляет preview-элементами для рисования (линии, прямоугольники, текст)
/// и рамкой выделения (selection box).
/// </summary>
public sealed partial class PreviewManager : ObservableObject
{
    /// <summary>
    /// Preview-линия для инструмента рисования линий.
    /// NOT [ObservableProperty] — unconditional notify needed for preview re-assign.
    /// </summary>
    private Line? _previewLine;
    public Line? PreviewLine
    {
        get => _previewLine;
        set
        {
            _previewLine = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Preview-прямоугольник для инструмента рисования прямоугольников.
    /// NOT [ObservableProperty] — unconditional notify needed for preview re-assign.
    /// </summary>
    private Rectangle? _previewRectangle;
    public Rectangle? PreviewRectangle
    {
        get => _previewRectangle;
        set
        {
            _previewRectangle = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Preview-текст для инструмента добавления текста.
    /// NOT [ObservableProperty] — unconditional notify needed for preview re-assign.
    /// </summary>
    private Text? _previewText;
    public Text? PreviewText
    {
        get => _previewText;
        set
        {
            _previewText = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Preview-прямоугольник рамки выделения — левый край в микронах.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectionBoxTop))]
    private long _selectionBoxLeft;

    /// <summary>
    /// Preview-прямоугольник рамки выделения — нижний край в микронах.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectionBoxTop))]
    private long _selectionBoxBottom;

    /// <summary>
    /// Preview-прямоугольник рамки выделения — верхний край (Bottom + Height).
    /// </summary>
    public long SelectionBoxTop => SelectionBoxBottom + SelectionBoxHeight;

    /// <summary>
    /// Preview-прямоугольник рамки выделения — ширина в микронах.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectionBoxRight))]
    private long _selectionBoxWidth;

    /// <summary>
    /// Preview-прямоугольник рамки выделения — правый край (Left + Width).
    /// </summary>
    public long SelectionBoxRight => SelectionBoxLeft + SelectionBoxWidth;

    /// <summary>
    /// Preview-прямоугольник рамки выделения — высота в микронах.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectionBoxTop))]
    private long _selectionBoxHeight;

    /// <summary>
    /// Направление рамки выделения.
    /// LeftToRight = полное попадание, RightToLeft = пересечение.
    /// </summary>
    [ObservableProperty]
    private SelectionDirection _selectionBoxDirection = SelectionDirection.LeftToRight;

    /// <summary>
    /// Очистить все preview-элементы.
    /// </summary>
    public void ClearAll()
    {
        PreviewLine = null;
        PreviewRectangle = null;
        PreviewText = null;
    }

    /// <summary>
    /// Установить рамку выделения.
    /// </summary>
    public void SetSelectionBox(long left, long bottom, long width, long height, SelectionDirection direction)
    {
        SelectionBoxLeft = left;
        SelectionBoxBottom = bottom;
        SelectionBoxWidth = width;
        SelectionBoxHeight = height;
        SelectionBoxDirection = direction;
    }

    /// <summary>
    /// Очистить рамку выделения.
    /// </summary>
    public void ClearSelectionBox()
    {
        SelectionBoxLeft = 0;
        SelectionBoxBottom = 0;
        SelectionBoxWidth = 0;
        SelectionBoxHeight = 0;
        SelectionBoxDirection = SelectionDirection.LeftToRight;
    }
}
