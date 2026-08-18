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
    /// Контракт: уведомление только при смене ссылки (создание/очистка предпросмотра);
    /// мутации свойств объекта рендерер получает через INPC самого объекта.
    /// </summary>
    [ObservableProperty]
    private Line? _previewLine;

    /// <summary>
    /// Preview-прямоугольник для инструмента рисования прямоугольников.
    /// Контракт: уведомление только при смене ссылки (создание/очистка предпросмотра);
    /// мутации свойств объекта рендерер получает через INPC самого объекта.
    /// </summary>
    [ObservableProperty]
    private Rectangle? _previewRectangle;

    /// <summary>
    /// Preview-текст для инструмента добавления текста.
    /// Контракт: уведомление только при смене ссылки (создание/очистка предпросмотра);
    /// мутации свойств объекта рендерер получает через INPC самого объекта.
    /// </summary>
    [ObservableProperty]
    private Text? _previewText;

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
    private long _selectionBoxWidth;

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
