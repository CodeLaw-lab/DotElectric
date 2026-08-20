using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Tools;

/// <summary>
/// Инструмент добавления текста.
/// Клик — создаёт preview, MouseMove — перемещает preview, MouseUp — финализирует объект.
/// </summary>
public sealed class TextTool : ITool
{
    private const string DefaultContent = "Текст";

    private readonly IEditorContext _context;
    private TextType _textType;
    private long _fontSizeMicrons;
    private string _font;
    private string _content;
    private PointMicrons? _startPoint;

    public TextTool(IEditorContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _textType = TextType.Text;
        _fontSizeMicrons = DocumentDefaults.DefaultFontSizeMicrons;
        _font = DocumentDefaults.DefaultFontName;
        _content = DefaultContent;
    }

    public void OnMouseDown(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        if (button != ToolMouseButton.Left)
            return;

        // Первый клик — создаём preview (НЕ добавляем в коллекцию)
        _startPoint = SnapHelper.SnapIfEnabled(modelPoint, _context.GridSettings);
        _context.PreviewManager.PreviewText = new Text(
            _startPoint.Value.MicronsX,
            _startPoint.Value.MicronsY,
            _content,
            _fontSizeMicrons,
            _font,
            _textType,
            foreground: DocumentDefaults.DefaultTextForeground);
    }

    public void OnMouseMove(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        if (_startPoint == null || _context.PreviewManager.PreviewText == null)
            return;

        var snapped = SnapHelper.SnapIfEnabled(modelPoint, _context.GridSettings);
        var preview = _context.PreviewManager.PreviewText;

        // Мутация свойств без ре-ассайна: рендерер подписан на INPC preview-объекта
        preview.MicronsX = snapped.MicronsX;
        preview.MicronsY = snapped.MicronsY;
    }

    public void OnMouseUp(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        // Финализируем — создаём реальный объект
        if (_startPoint != null && _context.PreviewManager.PreviewText != null)
        {
            var preview = _context.PreviewManager.PreviewText;
            var text = new Text(
                _context.ClampX(preview.MicronsX),
                _context.ClampY(preview.MicronsY),
                preview.Content,
                preview.FontSizeMicrons,
                preview.FontName,
                preview.TextType,
                foreground: DocumentDefaults.DefaultTextForeground);

            var cmd = new Commands.AddObjectCommand(_context.Template.Objects, text);
            _context.CommandHistory.Push(cmd);

            // Выделяем новый объект
            _context.SelectSingle(text);
        }

        _startPoint = null;
        _context.PreviewManager.PreviewText = null;
    }

    public void OnDoubleClick(PointMicrons modelPoint)
    {
        // Двойной клик — отменить текущий preview и переключиться на Select
        _startPoint = null;
        _context.PreviewManager.PreviewText = null;
        _context.ActivateTool(ToolKind.Select);
    }

    public bool OnMouseWheel(int delta, PointMicrons modelPoint) => false;

    public ToolCursor GetCursor()
    {
        return ToolCursor.IBeam;
    }

    public void Reset()
    {
        _startPoint = null;
        _context.PreviewManager.PreviewText = null;
    }

    public bool OnKeyDown(ToolKey key, ToolModifiers modifiers)
    {
        if (key == ToolKey.Escape)
        {
            Reset();
            _context.ActivateTool(ToolKind.Select);
            return true;
        }
        return false;
    }
}
