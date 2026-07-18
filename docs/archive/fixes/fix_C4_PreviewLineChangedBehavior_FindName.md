# P1-C4: FindName на каждый PropertyChanged в PreviewLineChangedBehavior

## Анализ проблемы

**Файл:** `src/DotElectric.TemplateEditor/Behaviors/PreviewLineChangedBehavior.cs:62,82,100`

**Симптом:** При каждом изменении `PreviewLine`, `PreviewRectangle` или `PreviewText` в ViewModel (а это происходит на каждый `OnMouseMove` инструментов рисования) выполняется обход визуального дерева через `canvas.FindName(...)`.

**Корень:** Методы `UpdatePreviewLine`, `UpdatePreviewRectangle`, `UpdatePreviewText` каждый раз ищут WPF-элементы по имени:

```csharp
private static void UpdatePreviewLine(Canvas canvas, ...)
{
    var line = canvas.FindName("PreviewLineElement") as System.Windows.Shapes.Line;
    // ...
}

private static void UpdatePreviewRectangle(Canvas canvas, ...)
{
    var rect = canvas.FindName("PreviewRectangleElement") as System.Windows.Shapes.Rectangle;
    // ...
}

private static void UpdatePreviewText(Canvas canvas, ...)
{
    var tb = canvas.FindName("PreviewTextElement") as TextBlock;
    // ...
}
```

`FindName` выполняет рекурсивный обход визульного дерева в поисках элемента с заданным именем. На холсте с сотнями объектов это может занимать значительное время.

**Профилирование:** Для `DrawingLineTool` при 60 FPS mouse move:
- 60 вызовов `FindName("PreviewLineElement")` в секунду
- Если один обход занимает 0.05ms → 3ms/сек только на поиск
- С 3 методами (`Line`, `Rectangle`, `Text`) — до 9ms/сек

**Правильное поведение:** Найти WPF-элементы один раз в `RegisterCanvas()` и кэшировать ссылки.

## Пошаговый план исправления

### Шаг 1: Создать класс-обёртку для кэшированных элементов

В том же файле `PreviewLineChangedBehavior.cs` добавить вложенный класс:

```csharp
private sealed class CachedElements
{
    public required System.Windows.Shapes.Line PreviewLineElement { get; init; }
    public required System.Windows.Shapes.Rectangle PreviewRectangleElement { get; init; }
    public required System.Windows.Controls.TextBlock PreviewTextElement { get; init; }
}
```

### Шаг 2: Создать словарь для хранения кэша

Заменить `ConditionalWeakTable<EditorViewModel, Canvas>` на структуру, хранящую и Canvas, и элементы:

```csharp
private static readonly ConditionalWeakTable<EditorViewModel, Canvas> _canvasRefs = new();
private static readonly ConditionalWeakTable<EditorViewModel, CachedElements> _cachedElements = new();
```

### Шаг 3: Изменить RegisterCanvas — кэшировать при регистрации

```csharp
public static void RegisterCanvas(Canvas canvas, EditorViewModel vm)
{
    _canvasRefs.AddOrUpdate(vm, canvas);

    // Кэшируем WPF-элементы один раз
    var line = canvas.FindName("PreviewLineElement") as System.Windows.Shapes.Line;
    var rect = canvas.FindName("PreviewRectangleElement") as System.Windows.Shapes.Rectangle;
    var text = canvas.FindName("PreviewTextElement") as TextBlock;

    if (line != null && rect != null && text != null)
    {
        _cachedElements.AddOrUpdate(vm, new CachedElements
        {
            PreviewLineElement = line,
            PreviewRectangleElement = rect,
            PreviewTextElement = text
        });
    }

    vm.PropertyChanged += OnVmPropertyChanged;
}
```

### Шаг 4: Изменить методы обновления — использовать кэш

```csharp
private static void UpdatePreviewLine(Canvas canvas, EditorViewModel vm, double zoom, double sheetHeightMm)
{
    if (!_cachedElements.TryGetValue(vm, out var cached)) return;
    var line = cached.PreviewLineElement;

    var preview = vm.PreviewLine;
    if (preview == null)
    {
        line.Visibility = Visibility.Collapsed;
        return;
    }

    line.Visibility = Visibility.Visible;
    line.X1 = (double)_convX.Convert(new object[] { preview.StartMicronsX, zoom }, typeof(double), null, null)!;
    line.Y1 = (double)_convY.Convert(new object[] { preview.StartMicronsY, sheetHeightMm, zoom }, typeof(double), null, null)!;
    line.X2 = (double)_convX.Convert(new object[] { preview.EndMicronsX, zoom }, typeof(double), null, null)!;
    line.Y2 = (double)_convY.Convert(new object[] { preview.EndMicronsY, sheetHeightMm, zoom }, typeof(double), null, null)!;
}
```

Аналогично для `UpdatePreviewRectangle` и `UpdatePreviewText`.

### Шаг 5: Изменить Unregister — очищать кэш

```csharp
public static void Unregister(EditorViewModel vm)
{
    vm.PropertyChanged -= OnVmPropertyChanged;
    _canvasRefs.Remove(vm);
    _cachedElements.Remove(vm);
}
```

## Альтернативное решение: Dictionary<string, DependencyObject>

Если `ConditionalWeakTable` кажется избыточным, можно сделать простой `Dictionary<EditorViewModel, CachedElements>` — но это создаст утечку памяти (ViewModel не сможет быть собранной GC). `ConditionalWeakTable` безопаснее.

## Итоговые изменения

Меняются:
- `RegisterCanvas()` — добавлено кэширование
- `UpdatePreviewLine()` — убран `FindName`
- `UpdatePreviewRectangle()` — убран `FindName`
- `UpdatePreviewText()` — убран `FindName`
- `Unregister()` — очистка кэша

Добавляются:
- Поле `_cachedElements`
- Внутренний класс `CachedElements`

## Проверка

```bash
dotnet build src/DotElectric.TemplateEditor.slnx
dotnet test src/DotElectric.TemplateEditor.Tests
```

## Риски

- Если XAML-шаблон изменится и элементы с именами `PreviewLineElement` и т.д. исчезнут — `FindName` вернёт `null`, кэш не создастся, поведение сломается. Но это эквивалентно текущему коду (сейчас мы получаем `null` на каждый вызов).
- `ConditionalWeakTable` — thread-safe, но не lock-free. В UI-потоке это не имеет значения.
- Новых аллокаций практически нет: словарь заполняется один раз при регистрации.
