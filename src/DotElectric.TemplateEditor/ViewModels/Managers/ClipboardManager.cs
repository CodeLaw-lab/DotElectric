using DotElectric.TemplateEditor.Models.Objects;

namespace DotElectric.TemplateEditor.ViewModels.Managers;

/// <summary>
/// Управляет буфером обмена (Copy/Paste/Cut).
/// </summary>
public sealed class ClipboardManager
{
    private const long PasteOffsetStepMicrons = 10_000; // 10 мм

    private readonly List<TemplateObjectBase> _clipboard = new();
    private long _pasteOffsetX;
    private long _pasteOffsetY;

    public int Count => _clipboard.Count;

    public void Copy(IEnumerable<TemplateObjectBase> objects)
    {
        _clipboard.Clear();
        foreach (var obj in objects)
            _clipboard.Add(obj.Clone());

        _pasteOffsetX = PasteOffsetStepMicrons;
        _pasteOffsetY = PasteOffsetStepMicrons;
    }

    public void Cut(IEnumerable<TemplateObjectBase> objects, Action<IEnumerable<TemplateObjectBase>> deleteAction)
    {
        Copy(objects);
        deleteAction(objects.ToList());
    }

    public IReadOnlyList<TemplateObjectBase> GetClipboardContents()
    {
        var result = new List<TemplateObjectBase>();
        foreach (var obj in _clipboard)
        {
            var clone = obj.Clone();
            clone.Move(clone.MicronsX + _pasteOffsetX, clone.MicronsY + _pasteOffsetY);
            result.Add(clone);
        }

        _pasteOffsetX += PasteOffsetStepMicrons;
        _pasteOffsetY += PasteOffsetStepMicrons;

        return result.AsReadOnly();
    }

    public void Clear() => _clipboard.Clear();
}
