using DotElectric.TemplateEditor.Constants;
using DotElectric.TemplateEditor.Helpers;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.Models.Objects;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Tools;

/// <summary>
/// Инструмент добавления текста.
/// Клик — создаёт preview, MouseMove — перемещает preview, MouseUp — финализирует объект.
/// </summary>
public sealed class TextTool : ITool
{
    private readonly IEditorContext _context;
    private readonly ITextToolSettings _settings;
    private TextType _textType;
    private long _fontSizeMicrons;
    private string _font;
    private string _content;
    private PointMicrons? _startPoint;

    public string Name => "Текст";

    public TextTool(IEditorContext context, ITextToolSettings? settings = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _settings = settings ?? new TextToolSettings();
        _textType = _settings.DefaultTextType;
        _fontSizeMicrons = _settings.DefaultFontSizeMicrons;
        _font = _settings.DefaultFont;
        _content = _settings.DefaultContent;
    }

    public void OnMouseDown(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        if (button != ToolMouseButton.Left)
            return;

        // Первый клик — создаём preview (НЕ добавляем в коллекцию)
        _startPoint = SnapHelper.SnapIfEnabled(modelPoint, _context.GridSettings);
        _context.PreviewText = new Text(
            _startPoint.Value.MicronsX,
            _startPoint.Value.MicronsY,
            _content,
            _fontSizeMicrons,
            _font,
            _textType,
            foreground: EditorSettings.DefaultTextForeground);
    }

    public void OnMouseMove(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        if (_startPoint == null || _context.PreviewText == null)
            return;

        var snapped = SnapHelper.SnapIfEnabled(modelPoint, _context.GridSettings);
        var preview = _context.PreviewText;
        preview.MicronsX = snapped.MicronsX;
        preview.MicronsY = snapped.MicronsY;
        _context.PreviewText = preview;
    }

    public void OnMouseUp(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        // Финализируем — создаём реальный объект
        if (_startPoint != null && _context.PreviewText != null)
        {
            var text = new Text(
                _context.ClampX(_context.PreviewText.MicronsX),
                _context.ClampY(_context.PreviewText.MicronsY),
                _context.PreviewText.Content,
                _context.PreviewText.FontSizeMicrons,
                _context.PreviewText.FontName,
                _context.PreviewText.TextType,
                foreground: EditorSettings.DefaultTextForeground);

            var cmd = new Commands.AddObjectCommand(_context.Template.Objects, text);
            _context.CommandHistory.Push(cmd);

            // Выделяем новый объект
            _context.SelectSingle(text);
        }

        _startPoint = null;
        _context.PreviewText = null;
    }

    public void OnDoubleClick(PointMicrons modelPoint)
    {
        // Двойной клик — отменить текущий preview и переключиться на Select
        _startPoint = null;
        _context.PreviewText = null;
        _context.SetActiveToolCommand.Execute("Select");
    }

    public bool OnMouseWheel(int delta, PointMicrons modelPoint) => false;

    public ToolCursor GetCursor()
    {
        return ToolCursor.IBeam;
    }

    // === Настройки текста ===

    public void SetTextType(TextType type) => _textType = type;
    public void SetFontSize(long fontSizeMicrons) { if (fontSizeMicrons > 0) _fontSizeMicrons = fontSizeMicrons; }
    public void SetDefaultContent(string content) { if (!string.IsNullOrWhiteSpace(content)) _content = content; }

    public void Reset()
    {
        _startPoint = null;
        _context.PreviewText = null;
    }

    public bool OnKeyDown(ToolKey key, ToolModifiers modifiers)
    {
        if (key == ToolKey.Escape)
        {
            Reset();
            _context.SetActiveToolCommand.Execute("Select");
            return true;
        }
        return false;
    }
}
