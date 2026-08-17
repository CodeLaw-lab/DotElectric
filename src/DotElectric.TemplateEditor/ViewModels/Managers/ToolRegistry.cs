using CommunityToolkit.Mvvm.ComponentModel;
using DotElectric.TemplateEditor.Tools;

namespace DotElectric.TemplateEditor.ViewModels.Managers;

/// <summary>
/// Реестр инструментов: идентичность, создание, кэш, активный инструмент,
/// стек инструментов и семантика переключения.
/// </summary>
public sealed partial class ToolRegistry : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveToolInstance))]
    private ToolKind _activeToolKind = ToolKind.Select;

    private readonly Stack<ToolKind> _toolStack = new();

    private static readonly Dictionary<Type, Func<IEditorContext, ITool>> ToolFactories = new()
    {
        [typeof(SelectTool)] = ctx => new SelectTool(ctx),
        [typeof(DrawingLineTool)] = ctx => new DrawingLineTool(ctx),
        [typeof(DrawingRectangleTool)] = ctx => new DrawingRectangleTool(ctx),
        [typeof(TextTool)] = ctx => new TextTool(ctx),
        [typeof(ResizeTool)] = ctx => new ResizeTool(ctx),
    };

    private static readonly Dictionary<ToolKind, Type> KindToToolType = new()
    {
        [ToolKind.Select] = typeof(SelectTool),
        [ToolKind.Line] = typeof(DrawingLineTool),
        [ToolKind.Rectangle] = typeof(DrawingRectangleTool),
        [ToolKind.Text] = typeof(TextTool),
        [ToolKind.Resize] = typeof(ResizeTool),
    };

    private readonly Dictionary<Type, ITool> _toolCache = new();
    private readonly IEditorContext _editorCtx;

    public ToolRegistry(IEditorContext editorCtx)
    {
        _editorCtx = editorCtx;
    }

    /// <summary>
    /// Экземпляр активного инструмента (создаётся и кэшируется по первому обращению).
    /// </summary>
    public ITool ActiveToolInstance => GetOrCreateTool(KindToToolType[ActiveToolKind]);

    public T GetOrCreateTool<T>() where T : class, ITool => (T)GetOrCreateTool(typeof(T));

    private ITool GetOrCreateTool(Type type)
    {
        if (_toolCache.TryGetValue(type, out var cached))
            return cached;

        if (!ToolFactories.TryGetValue(type, out var factory))
            throw new ArgumentException($"Unknown tool type: {type}");

        var tool = factory(_editorCtx);
        _toolCache[type] = tool;
        return tool;
    }

    /// <summary>
    /// Переключает активный инструмент с reset'ом предыдущего.
    /// Переключение на ту же идентичность — no-op.
    /// </summary>
    public void SwitchTo(ToolKind kind)
    {
        if (ActiveToolKind == kind)
            return;

        ResetCachedTool(KindToToolType[ActiveToolKind]);
        ActiveToolKind = kind;
    }

    public void PushTool(ToolKind kind)
    {
        _toolStack.Push(ActiveToolKind);
        ActiveToolKind = kind;
    }

    public void PopTool()
    {
        if (_toolStack.Count > 0)
        {
            ActiveToolKind = _toolStack.Pop();
        }
    }

    private void ResetCachedTool(Type type)
    {
        if (_toolCache.TryGetValue(type, out var tool))
        {
            tool.Reset();
        }
    }
}
