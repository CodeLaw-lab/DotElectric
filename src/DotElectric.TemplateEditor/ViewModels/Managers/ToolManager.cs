using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using DotElectric.TemplateEditor.Tools;

namespace DotElectric.TemplateEditor.ViewModels.Managers;

/// <summary>
/// Управляет активным инструментом и стеком инструментов.
/// Каноническое состояние — типизированная идентичность <see cref="ToolKind"/>;
/// строковая поверхность (<see cref="ActiveTool"/>, <see cref="PushTool(string)"/>) — шим совместимости.
/// </summary>
public sealed partial class ToolManager : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ActiveTool))]
    [NotifyPropertyChangedFor(nameof(ActiveToolInstance))]
    private ToolKind _activeToolKind = ToolKind.Select;

    private readonly Stack<ToolKind> _toolStack = new();

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

    private static readonly Dictionary<ToolKind, Type> KindToToolType = new()
    {
        [ToolKind.Select] = typeof(SelectTool),
        [ToolKind.Line] = typeof(DrawingLineTool),
        [ToolKind.Rectangle] = typeof(DrawingRectangleTool),
        [ToolKind.Text] = typeof(TextTool),
        [ToolKind.Resize] = typeof(ResizeTool),
    };

    /// <summary>
    /// Карта горячих клавиш: клавиша → идентичность инструмента.
    /// </summary>
    public static IReadOnlyDictionary<Key, ToolKind> ShortcutMap { get; } = new Dictionary<Key, ToolKind>
    {
        [Key.V] = ToolKind.Select,
        [Key.L] = ToolKind.Line,
        [Key.R] = ToolKind.Rectangle,
        [Key.T] = ToolKind.Text,
    };

    private readonly Dictionary<Type, ITool> _toolCache = new();
    private readonly IEditorContext _editorCtx;

    public ToolManager(IEditorContext editorCtx)
    {
        _editorCtx = editorCtx;
    }

    /// <summary>
    /// Имя активного инструмента. Шим совместимости: производно от идентичности,
    /// при записи парсит строку в идентичность (неизвестные значения игнорируются).
    /// </summary>
    public string ActiveTool
    {
        get => ActiveToolKind.ToString();
        set
        {
            if (Enum.TryParse<ToolKind>(value, out var kind))
                ActiveToolKind = kind;
        }
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

    public void PushTool(string tool) => PushTool(ParseKind(tool));

    public void PopTool()
    {
        if (_toolStack.Count > 0)
        {
            ActiveToolKind = _toolStack.Pop();
        }
    }

    public void ResetTool(string toolName)
    {
        if (ToolNameMap.TryGetValue(toolName, out var type))
        {
            ResetCachedTool(type);
        }
    }

    private void ResetCachedTool(Type type)
    {
        if (_toolCache.TryGetValue(type, out var tool))
        {
            tool.Reset();
        }
    }

    private static ToolKind ParseKind(string tool) => Enum.Parse<ToolKind>(tool);
}
