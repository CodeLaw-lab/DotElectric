# P2-M2: Подписка на ScrollBar до инициализации в EditorCanvas

## Анализ проблемы

**Файл:** `src/DotElectric.TemplateEditor/Views/EditorCanvas.xaml.cs:23-24`

**Симптом:** В конструкторе `EditorCanvas()` выполняется подписка на события `ScrollBar.ValueChanged`:

```csharp
public EditorCanvas()
{
    InitializeComponent();
    DataContextChanged += OnDataContextChanged;
    Loaded += OnLoaded;
    SizeChanged += OnSizeChanged;

    // Подписка на события скроллбаров
    HorizontalScrollBar.ValueChanged += OnHorizontalScrollBarChanged;
    VerticalScrollBar.ValueChanged += OnVerticalScrollBarChanged;
}
```

**Проблема:** В конструкторе WPF-элементы ещё не инициализированы. `HorizontalScrollBar` и `VerticalScrollBar` — это, вероятно, именованные элементы из XAML. WPF создаёт их в процессе `InitializeComponent()`, который вызывается в конструкторе. Поэтому технически они **должны** быть не-null после `InitializeComponent()`. Однако:

1. **WPF TemplateBinding:** Если `ScrollBar` определён внутри ControlTemplate, он может быть не создан до применения шаблона (после Loaded).
2. **NullReferenceException при дизайне:** В дизайнере Visual Studio конструктор может вызываться без полной инициализации XAML.
3. **Нарушение WPF-паттернов:** Рекомендуется подписываться на события UI-элементов в `OnLoaded`, а не в конструкторе.

**Дополнительная проблема:** В `OnLoaded` (строка 81) уже выполняется `RegisterCanvas`. То же самое делается в `OnDataContextChanged` (строка 66). Получается, что `RegisterCanvas` может быть вызван дважды.

## Пошаговый план исправления

### Шаг 1: Убрать подписку ScrollBar из конструктора

```csharp
public EditorCanvas()
{
    InitializeComponent();
    DataContextChanged += OnDataContextChanged;
    Loaded += OnLoaded;
    SizeChanged += OnSizeChanged;

    // ScrollBar подписка перенесена в OnLoaded
}
```

### Шаг 2: Добавить подписку ScrollBar в OnLoaded

```csharp
private void OnLoaded(object sender, RoutedEventArgs e)
{
    if (DataContext is EditorViewModel vm)
    {
        PreviewLineChangedBehavior.RegisterCanvas(DrawingCanvas, vm);
    }

    // ScrollBar: подписываемся после полной инициализации
    HorizontalScrollBar.ValueChanged += OnHorizontalScrollBarChanged;
    VerticalScrollBar.ValueChanged += OnVerticalScrollBarChanged;
}
```

### Шаг 3: (Опционально) Добавить отписку при выгрузке

Чтобы избежать утечек памяти при частой смене DataContext:

```csharp
public EditorCanvas()
{
    InitializeComponent();
    DataContextChanged += OnDataContextChanged;
    Loaded += OnLoaded;
    Unloaded += OnUnloaded;    // ← добавить
    SizeChanged += OnSizeChanged;
}

private void OnUnloaded(object sender, RoutedEventArgs e)
{
    HorizontalScrollBar.ValueChanged -= OnHorizontalScrollBarChanged;
    VerticalScrollBar.ValueChanged -= OnVerticalScrollBarChanged;

    if (DataContext is EditorViewModel vm)
    {
        PreviewLineChangedBehavior.Unregister(vm);
    }
}
```

### Шаг 4: Исправить двойной RegisterCanvas

В текущем коде `RegisterCanvas` вызывается и в `OnDataContextChanged`, и в `OnLoaded`:

```csharp
// OnDataContextChanged, строка 66:
PreviewLineChangedBehavior.RegisterCanvas(DrawingCanvas, newVm);

// OnLoaded, строка 85:
PreviewLineChangedBehavior.RegisterCanvas(DrawingCanvas, vm);
```

Нужно определить единое место. Лучше в `OnLoaded` — так как Canvas гарантированно инициализирован:

```csharp
private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
{
    if (e.OldValue is EditorViewModel oldVm)
    {
        oldVm.GridInvalidated = null;
        PreviewLineChangedBehavior.Unregister(oldVm);   // ← важно: отписываемся при смене
    }

    if (e.NewValue is EditorViewModel newVm)
    {
        newVm.GridInvalidated = () =>
        {
            GridNodesLayer.Nodes = newVm.GridNodes;
            GridNodesLayer.InvalidateVisual();
        };

        RefreshGridNodesLayer();
    }
}

private void OnLoaded(object sender, RoutedEventArgs e)
{
    if (DataContext is EditorViewModel vm)
    {
        PreviewLineChangedBehavior.RegisterCanvas(DrawingCanvas, vm);
    }

    HorizontalScrollBar.ValueChanged += OnHorizontalScrollBarChanged;
    VerticalScrollBar.ValueChanged += OnVerticalScrollBarChanged;
}
```

## Проверка

```bash
dotnet build src/DotElectric.TemplateEditor.slnx
# Запустить приложение, проверить:
# - Панорамирование скроллбарами работает
# - Preview-элементы отображаются
# - Переключение документов не вызывает ошибок
dotnet run --project src/DotElectric.TemplateEditor
```

## Риски

- `Loaded` может сработать несколько раз в некоторых сценариях WPF (например, при повторном использовании UserControl). Нужно убедиться, что подписка не задваивается.
- Для безопасности можно проверять в `OnUnloaded` и отписываться.
- Если `DrawingCanvas` не инициализирован к моменту `Loaded` — это признак серьёзной XAML-проблемы, не связанной с нашим изменением.
