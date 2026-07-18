using CommunityToolkit.Mvvm.ComponentModel;
using DotElectric.TemplateEditor.Tools;

namespace DotElectric.TemplateEditor.ViewModels.Managers;

/// <summary>
/// Управляет активным инструментом и стеком инструментов.
/// </summary>
public sealed partial class ToolManager : ObservableObject
{
    [ObservableProperty]
    private string _activeTool = "Select";

    private readonly Stack<string> _toolStack = new();

    private static readonly Dictionary<Type, Func<IEditorContext, ITool>> ToolFactories = new()
    {
        [typeof(SelectTool)] = ctx => new SelectTool(ctx),
        [typeof(DrawingLineTool)] = ctx => new DrawingLineTool(ctx),
        [typeof(DrawingRectangleTool)] = ctx => new DrawingRectangleTool(ctx),
        [typeof(TextTool)] = ctx => new TextTool(ctx),
        [typeof(PanTool)] = ctx => new PanTool(ctx),
        [typeof(ResizeTool)] = ctx => new ResizeTool(ctx),
    };

    private static readonly Dictionary<string, Type> ToolNameMap = new()
    {
        ["Select"] = typeof(SelectTool),
        ["Line"] = typeof(DrawingLineTool),
        ["Rectangle"] = typeof(DrawingRectangleTool),
        ["Text"] = typeof(TextTool),
        ["Pan"] = typeof(PanTool),
        ["Resize"] = typeof(ResizeTool),
    };

    private readonly Dictionary<Type, ITool> _toolCache = new();
    private readonly IEditorContext _editorCtx;

    public ToolManager(IEditorContext editorCtx)
    {
        _editorCtx = editorCtx;
    }

    public T GetOrCreateTool<T>() where T : class, ITool
    {
        var type = typeof(T);

        if (_toolCache.TryGetValue(type, out var cached))
            return (T)cached;

        if (!ToolFactories.TryGetValue(type, out var factory))
            throw new ArgumentException($"Unknown tool type: {type}");

        var tool = factory(_editorCtx);
        _toolCache[type] = tool;
        return (T)tool;
    }

    public void PushTool(string tool)
    {
        _toolStack.Push(ActiveTool);
        ActiveTool = tool;
    }

    public void PopTool()
    {
        if (_toolStack.Count > 0)
        {
            ActiveTool = _toolStack.Pop();
        }
    }

    public void ResetTool(string toolName)
    {
        if (ToolNameMap.TryGetValue(toolName, out var type) &&
            _toolCache.TryGetValue(type, out var tool))
        {
            tool.Reset();
        }
    }
}
