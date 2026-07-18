# P3-D3: ToolManager.GetOrCreateTool — цепочка if на словарь

## Анализ проблемы

**Файл:** `src/DotElectric.TemplateEditor/ViewModels/Managers/ToolManager.cs:30-45`

**Симптом:** Метод `GetOrCreateTool<T>()` использует 6 последовательных `if (typeof(T) == typeof(X))` для определения типа инструмента.

```csharp
public T GetOrCreateTool<T>() where T : class, ITool
{
    if (typeof(T) == typeof(SelectTool))
        return (_selectTool ??= new SelectTool(_editorVm)) as T ?? throw new InvalidOperationException();
    if (typeof(T) == typeof(DrawingLineTool))
        return (_drawingLineTool ??= new DrawingLineTool(_editorVm)) as T ?? throw new InvalidOperationException();
    if (typeof(T) == typeof(DrawingRectangleTool))
        return (_drawingRectangleTool ??= new DrawingRectangleTool(_editorVm)) as T ?? throw new InvalidOperationException();
    if (typeof(T) == typeof(TextTool))
        return (_textTool ??= new TextTool(_editorVm)) as T ?? throw new InvalidOperationException();
    if (typeof(T) == typeof(PanTool))
        return (_panTool ??= new PanTool(_editorVm)) as T ?? throw new InvalidOperationException();
    if (typeof(T) == typeof(ResizeTool))
        return (_resizeTool ??= new ResizeTool(_editorVm)) as T ?? throw new InvalidOperationException();
    throw new ArgumentException($"Unknown tool type: {typeof(T)}");
}
```

**Проблемы:**
1. **Производительность:** O(n) для каждого вызова — в среднем 3 сравнения типов
2. **Не расширяемо:** При добавлении нового инструмента нужно:
   - Добавить `_newTool` поле
   - Добавить новый `if` блок в `GetOrCreateTool`
   - Добавить case в `ResetTool`
3. **Дублирование:** В `ResetTool` (строка 61) — ещё один switch по строке для сброса инструментов

**Правильное решение:** Использовать `Dictionary<Type, Func<ITool>>` для фабрик и `Dictionary<Type, ITool>` для кэша.

## Пошаговый план исправления

### Шаг 1: Заменить поля инструментов на словарь

```csharp
public partial class ToolManager : ObservableObject
{
    [ObservableProperty]
    private string _activeTool = "Select";

    private readonly Stack<string> _toolStack = new();
    private readonly EditorViewModel _editorVm;

    // Словарь фабрик инструментов
    private static readonly Dictionary<Type, Func<EditorViewModel, ITool>> ToolFactories = new()
    {
        [typeof(SelectTool)] = vm => new SelectTool(vm),
        [typeof(DrawingLineTool)] = vm => new DrawingLineTool(vm),
        [typeof(DrawingRectangleTool)] = vm => new DrawingRectangleTool(vm),
        [typeof(TextTool)] = vm => new TextTool(vm),
        [typeof(PanTool)] = vm => new PanTool(vm),
        [typeof(ResizeTool)] = vm => new ResizeTool(vm),
    };

    // Кэш созданных инструментов
    private readonly Dictionary<Type, ITool> _toolCache = new();
```

### Шаг 2: Переписать GetOrCreateTool

```csharp
public T GetOrCreateTool<T>() where T : class, ITool
{
    var type = typeof(T);

    // Проверяем кэш
    if (_toolCache.TryGetValue(type, out var cached))
        return (T)cached;

    // Создаём через фабрику
    if (!ToolFactories.TryGetValue(type, out var factory))
        throw new ArgumentException($"Unknown tool type: {type}");

    var tool = factory(_editorVm);
    _toolCache[type] = tool;
    return (T)tool;
}
```

### Шаг 3: Переписать ResetTool

```csharp
public void ResetTool(string toolName)
{
    var tool = _toolCache.Values.FirstOrDefault(t =>
    {
        return t switch
        {
            SelectTool => toolName == "Select",
            DrawingLineTool => toolName == "Line",
            DrawingRectangleTool => toolName == "Rectangle",
            TextTool => toolName == "Text",
            ResizeTool => toolName == "Resize",
            _ => false
        };
    });
    tool?.Reset();
}
```

Или ещё проще — заменить строковые имена на типы:

```csharp
public void ResetTool<T>() where T : class, ITool
{
    var type = typeof(T);
    if (_toolCache.TryGetValue(type, out var tool))
        tool.Reset();
}
```

Это требует обновления вызовов `ResetTool("Select")` → `ResetTool<SelectTool>()`.

## Преимущества

| Аспект | Было (if chain) | Стало (Dictionary) |
|--------|----------------|-------------------|
| Производительность | O(n) в среднем 3 сравнения | O(1) хэш-таблица |
| Добавление инструмента | 3 места (+поле, +if, +case) | 1 строка в ToolFactories |
| Расширяемость | ❌ Нужно редактировать класс | ✅ Просто добавить фабрику |
| Читаемость | Много шума | Чистый и понятный |

## Проверка

```bash
dotnet build src/DotElectric.TemplateEditor.slnx
dotnet test src/DotElectric.TemplateEditor.Tests --filter "FullyQualifiedName~ToolManager"
dotnet test src/DotElectric.TemplateEditor.Tests
```

## Риски

- **Низкий:** Изменение `ResetTool` с `string` на generic может потребовать обновления вызовов. Нужно проверить все места вызова.
- **Очень низкий:** Dictionary не thread-safe, но ToolManager используется только из UI-потока.
