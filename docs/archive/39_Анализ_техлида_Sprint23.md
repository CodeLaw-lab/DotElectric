# Анализ проекта DotElectric — от техлида

**Дата:** 12.04.2026  
**Автор:** Tech Lead Review  
**Метод:** Анализ исходного кода (без использования документации)  
**Статус:** Этап 1 ЗАВЕРШЁН ✅ (1249 тестов, 0 сбоев)

---

## Общая оценка

Проект в **хорошем состоянии**: чистая MVVM-архитектура, грамотное использование DI, WeakReferenceMessenger, Command Pattern. Однако выявлен ряд **критических** и **серьёзных** проблем, требующих устранения.

### Статистика кодовой базы

| Метрика | Основной проект | Тестовый проект |
|---------|----------------|-----------------|
| Файлов | 103 | 51 |
| Строк кода | 11 819 | 12 748 |
| **ВСЕГО** | **154 файла, 24 567 строк** | |

#### Крупнейшие файлы (>500 строк)

| # | Файл | Строк |
|---|------|-------|
| 1 | `ViewModels/EditorViewModel.cs` | **872** |
| 2 | `Views/EditorCanvas.xaml` | **580** |
| 3 | `MainWindow.xaml` | **544** |
| 4 | `ViewModels/PropertiesViewModel.cs` | 498 |
| 5 | `Helpers/ValidationService.cs` | 424 |
| 6 | `Tools/SelectTool.cs` | 300 |
| 7 | `Tools/ResizeTool.cs` | 259 |
| 8 | `Converters/CommonConverters.cs` | 351 |

---

## P0 — КРИТИЧЕСКИЕ ПРОБЛЕМЫ (баги, утечки, race conditions)

### 1. `MouseBehavior` — глобальное состояние double-click detection

**Файл:** `Behaviors/MouseBehavior.cs`, строки ~170-171

```csharp
private static int _clickCount;
private static System.DateTime _lastClickTime;
```

**Проблема:** Статические mutable поля — **общие для всех UI-элементов**. Двойной клик на одном элементе может сработать на другом. Это реальный баг, влияющий на все элементы с `MouseBehavior.OnMouseDoubleClick`.

**Сценарий бага:**
1. Пользователь кликает на Canvas → `_lastClickTime` обновляется
2. Пользователь кликает на другой элемент (например, кнопку) → `_lastClickTime` совпадает → срабатывает двойной клик

**Решение:** Перенести `_clickCount` / `_lastClickTime` в `Dictionary<UIElement, ClickState>`, либо полностью положиться на WPF `e.ClickCount` (который уже корректно используется в `EditorCanvasBehavior`).

**SP:** 3

---

### 2. `PreviewLineChangedBehavior` — утечка подписок на PropertyChanged

**Файл:** `Behaviors/PreviewLineChangedBehavior.cs`, строка 21

```csharp
public static void RegisterCanvas(Canvas canvas, EditorViewModel vm)
{
    _canvasRefs[vm] = new WeakReference<Canvas>(canvas);
    vm.PropertyChanged += OnVmPropertyChanged; // ← ПОДПИСКА БЕЗ ОТПИСКИ!
}
```

**Проблема:** Подписка **никогда не отписывается**. При каждом открытии/закрытии вкладки — новая подписка, которая живёт пока жив ViewModel. `WeakReference<Canvas>` защищает Canvas, но не VM.

**Утечка:** 1 подписка × N вкладок = N висячих подписок. При активной работе (сотни открытых/закрытых вкладок) — значительная утечка памяти.

**Решение:**
1. Добавить `Unregister(EditorViewModel vm)` метод
2. Вызвать из `EditorViewModel.Dispose()`
3. Использовать WeakReference для подписки на PropertyChanged (WeakEventManager)

**SP:** 3

---

### 3. `EditorCanvasBehavior._states` — утечка при unload Canvas

**Файл:** `Behaviors/EditorCanvasBehavior.cs`, строка 17

```csharp
private static readonly Dictionary<Canvas, EditorCanvasState> _states = new();
```

**Проблема:** Очистка происходит только при смене `Editor` property. Если Canvas удаляется из visual tree (закрытие вкладки) без явной очистки — entry остаётся в словаре.

**Утечка:** 1 Canvas + EditorCanvasState × N закрытых вкладок = N утечек.

**Решение:** Подписаться на `canvas.Unloaded` и очищать `_states`:

```csharp
canvas.Unloaded += (s, e) => {
    canvas.Unloaded -= ...;
    canvas.MouseDown -= State_MouseDown;
    // ... остальные unsubscribe
    _states.Remove(canvas);
};
```

**SP:** 2

---

### 4. `SettingsService` — молчаливое проглатывание исключений

**Файл:** `Services/SettingsService.cs`, строки 43, 69

```csharp
catch (Exception)
{
    // Файл повреждён — используем настройки по умолчанию
}
```

и

```csharp
catch (Exception)
{
    // Не удалось сохранить настройки — молча игнорируем
}
```

**Проблема:** 
- Пользователь может потерять настройки без какого-либо уведомления
- Нет логирования факта повреждения файла
- Невозможно диагностировать проблему в production

**Решение:**
1. Внедрить `ILogger<SettingsService>`
2. Логировать `LogWarning` при повреждении файла
3. Логировать `LogError` при неудачном сохранении
4. Добавить fallback на default settings с уведомлением

**SP:** 3

---

### 5. `AutosaveService.AutosaveTick` — вызов из ThreadPool потока

**Файл:** `Services/AutosaveService.cs`, строка 132

```csharp
private void OnAutosaveTick(object? state)
{
    try
    {
        AutosaveTick?.Invoke(); // ← ThreadPool thread!
    }
    ...
}
```

**Проблема:** Событие вызывается из фонового потока Timer. Подписчик (MainViewModel) может обратиться к `ObservableCollection<EditorViewModel>` → cross-thread exception.

**Решение:** Маршалировать через `IDispatcherService`:

```csharp
private void OnAutosaveTick(object? state)
{
    _dispatcherService?.Invoke(() => {
        AutosaveTick?.Invoke();
    });
}
```

Или использовать `Timer` с `SynchronizationContext`.

**SP:** 3

---

### 6. `PreviewLineChangedBehavior._canvasRefs` — Dictionary без синхронизации

**Файл:** `Behaviors/PreviewLineChangedBehavior.cs`, строка 17

```csharp
private static readonly Dictionary<EditorViewModel, WeakReference<Canvas>> _canvasRefs = new();
```

**Проблема:** Статический Dictionary, доступный из всех вкладок (multi-tab). При одновременном открытии/закрытии вкладок — race condition при чтении/записи.

**Решение:** Заменить на `ConcurrentDictionary<EditorViewModel, WeakReference<Canvas>>`.

**SP:** 1

---

## P1 — СЕРЬЁЗНЫЕ ПРОБЛЕМЫ

### 7. `ITool` зависит от WPF (`MouseButton`, `ModifierKeys`)

**Файл:** `Tools/ITool.cs`

```csharp
void OnMouseDown(PointMicrons modelPoint, MouseButton button, ModifierKeys modifiers);
void OnMouseMove(PointMicrons modelPoint, MouseButton button, ModifierKeys modifiers);
```

**Проблема:** Нарушение чистой архитектуры. Tools находятся в отдельной папке и не должны зависеть от `System.Windows.Input`. Невозможно тестировать Tools без WPF.

**Решение:** Создать свои enum:

```csharp
// Tools/ToolMouseButton.cs
public enum ToolMouseButton { Left, Right, Middle }

// Tools/ToolModifiers.cs
[Flags]
public enum ToolModifiers
{
    None = 0,
    Ctrl = 1,
    Shift = 2,
    Alt = 4,
}
```

Конвертация на уровне `EditorCanvasBehavior`:

```csharp
var toolButton = e.ChangedButton switch
{
    MouseButton.Left => ToolMouseButton.Left,
    MouseButton.Right => ToolMouseButton.Right,
    MouseButton.Middle => ToolMouseButton.Middle,
};
var toolMods = Keyboard.Modifiers.ToToolModifiers(); // extension method
tool.OnMouseDown(modelPt, toolButton, toolMods);
```

**SP:** 5

---

### 8. `ThemeService` использует `Serilog.Log` напрямую

**Файл:** `Services/ThemeService.cs`, строки 33, 44

```csharp
Log.Warning("Попытка установить пустую тему. Используется Light.");
Log.Error(ex, "Ошибка при переключении темы на {Theme}", theme);
```

**Проблема:** Статический вызов Serilog — невозможно мокать в тестах, нарушение DI. `ThemeServiceTests` не может проверить что логирование произошло.

**Решение:** Внедрить `ILogger<ThemeService>`:

```csharp
public class ThemeService : IThemeService
{
    private readonly ILogger<ThemeService> _logger;
    
    public ThemeService(ISettingsService settingsService, 
                        IThemeDictionaryManager dictionaryManager,
                        ILogger<ThemeService> logger)
    {
        _logger = logger;
        ...
    }
    
    public void SetTheme(string theme)
    {
        ...
        _logger.LogWarning("Попытка установить пустую тему. Используется Light.");
        ...
    }
}
```

**SP:** 2

---

### 9. `IPrintService.Print()` — нарушение контракта (LSP)

**Файл:** `Services/IPrintService.cs` + `PrintService.cs`

```csharp
// IPrintService.cs
void Print(Template template, PrintSettings settings);

// PrintService.cs — реализация:
public void Print(Template template, PrintSettings settings)
{
    throw new InvalidOperationException(
        "Печать шаблона требует визуального представления (Canvas)...");
}
```

**Проблема:** Метод интерфейса **всегда** бросает исключение. Нарушение Liskov Substitution Principle — любой вызов `IPrintService.Print()` приведёт к runtime exception.

**Решение:** Удалить `Print(Template, PrintSettings)` из `IPrintService`. Оставить только:
- `PrintWithVisual(Visual visual, string description, PrintSettings settings)`
- `ShowPrintDialog()`

Если callers нужен метод без Visual — создать отдельный `IPrintTemplateService` с явной семантикой.

**SP:** 1

---

### 10. `SettingsService` — нет thread safety

**Файл:** `Services/SettingsService.cs`

```csharp
private AppSettings? _cachedSettings;

public AppSettings Load()
{
    if (_cachedSettings != null)  // ← check
        return _cachedSettings;    // ← act (race condition!)
    ...
}
```

**Проблема:** Race condition при одновременном `Load()`/`Save()` из разных потоков (например, AutosaveService + UI thread).

**Решение:** Добавить `lock`:

```csharp
private readonly object _lock = new();

public AppSettings Load()
{
    lock (_lock)
    {
        if (_cachedSettings != null) return _cachedSettings;
        ...
    }
}

public void Save(AppSettings settings)
{
    lock (_lock)
    {
        ...
        _cachedSettings = settings;
    }
}
```

**SP:** 1 (в рамках задачи #4)

---

### 11. `TabItemMiddleClickBehavior` — reflection вместо binding

**Файл:** `Behaviors/TabItemMiddleClickBehavior.cs`

```csharp
var command = tabControl.DataContext?
    .GetType()
    .GetProperty("CloseTabCommand")?
    .GetValue(tabControl.DataContext);
```

**Проблема:** 
- Reflection — хрупкий, не рефакторинг-френдли
- Нет compile-time проверки
- Если свойство переименуют — ошибка только в runtime

**Решение:** Использовать `ICommand` из DataContext EditorViewModel напрямую:

```xaml
<!-- В TabItem DataTemplate -->
<Button Command="{Binding CloseTabCommand}" CommandParameter="{Binding}" />
```

Или использовать WeakReferenceMessenger (уже есть `CloseTabRequestMessage`).

**SP:** 3

---

### 12. `TextTool` — hardcoded значения

**Файл:** `Tools/TextTool.cs`, строки 17-19

```csharp
private long _fontSizeMicrons = 3500;  // 3.5 мм
private string _font = "ГОСТ Б";
private string _content = "Текст";
```

**Проблема:** Значения захардкождены, не настраиваются пользователем. При смене шрифтов или размеров текста — потребуется перекомпиляция.

**Решение:**
1. Добавить в `AppSettings`: `DefaultFontSizeMicrons`, `DefaultFontName`
2. Читать из `ISettingsService` при создании TextTool
3. Обновить через сеттеры (уже есть `SetFontSize`, `SetDefaultContent`)

```csharp
public TextTool(EditorViewModel editor, ISettingsService settings)
{
    _editor = editor;
    _fontSizeMicrons = settings.Get("DefaultFontSizeMicrons", 3500L);
    _font = settings.Get("DefaultFontName", "ГОСТ Б");
}
```

**SP:** 3

---

## P2 — УЛУЧШЕНИЯ

### 13. Дублирование `SnapIfEnabled` в 3 инструментах

**Файлы:** `DrawingLineTool.cs`, `DrawingRectangleTool.cs`, `TextTool.cs`

Каждый инструмент имеет идентичный метод:

```csharp
private PointMicrons SnapIfEnabled(PointMicrons point)
{
    if (_editor.GridSettings.Enabled && _editor.GridSettings.SnapEnabled)
        return point.SnapToGrid(_editor.GridSettings.StepMicrons);
    return point;
}
```

**Решение:** Вынести в extension method:

```csharp
public static class SnapExtensions
{
    public static PointMicrons SnapIfEnabled(this PointMicrons point, GridSettings settings)
    {
        if (settings.Enabled && settings.SnapEnabled)
            return point.SnapToGrid(settings.StepMicrons);
        return point;
    }
}
```

Или создать базовый класс `DrawingToolBase`.

**SP:** 2

---

### 14. Дублирование bounding box текста

**Файлы:** `Helpers/HitTestHelper.cs`, `Helpers/SelectionBoxHelper.cs`

Идентичная логика расчёта bounding box текста с поворотами (коэффициент 0.6, обработка 0/90/180/270):

```csharp
// HitTestHelper.TestText():
var width = text.Content.Length * text.FontSizeMicrons * 0.6;

// SelectionBoxHelper.GetTextBounds():
var width = text.Content.Length * text.FontSizeMicrons * 0.6;
```

**Решение:** Вынести в `TextGeometryHelper`:

```csharp
public static class TextGeometryHelper
{
    public static RectMicrons GetTextBounds(Text text) { ... }
    public static bool HitTestText(PointMicrons point, Text text) { ... }
}
```

**SP:** 2

---

### 15. `ValidationService.ValidateHexColors()` — мёртвый код

**Файл:** `Helpers/ValidationService.cs`, V-005

```csharp
private static IEnumerable<ValidationError> ValidateHexColors(Template template)
{
    // TODO: Валидация HEX-цветов объектов
    yield break; // ← ВСЕГДА пустая коллекция
}
```

**Проблема:** Метод всегда возвращает пустую коллекцию. Создаёт ложное ощущение функциональности.

**Решение:** Либо реализовать валидацию (у объектов Line/Rectangle/Text нет цветовых свойств), либо удалить правило V-005.

**SP:** 1

---

### 16. Нет логирования в ключевых сервисах

**Сервисы без ILogger:**
- `TemplateService.cs` (333 строки)
- `FileService.cs` (76 строк)
- `SettingsService.cs` (170 строк — частично покрыто задачей #4)
- `PrintService.cs` (100 строк)
- `TemplateLibraryService.cs` (33 строки)

**Решение:** Внедрить `ILogger<T>` в каждый сервис, логировать критические операции (сохранение, загрузка, ошибки).

**SP:** 3

---

### 17. `IDispatcherService` — нет non-generic `Invoke(Action)`

**Файл:** `Services/IDispatcherService.cs`

```csharp
public interface IDispatcherService
{
    T Invoke<T>(Func<T> func);  // Только generic!
}
```

**Проблема:** Для void-делегатов приходится писать: `Invoke<bool>(() => { ...; return true; })`.

**Решение:** Добавить overload:

```csharp
public interface IDispatcherService
{
    void Invoke(Action action);
    T Invoke<T>(Func<T> func);
}
```

**SP:** 1

---

### 18. `EditorViewModel` — 872 строки

**Проблема:** Файл слишком большой для комфортной поддержки. Содержит:
- Управление состоянием (IsDirty, FilePath, DisplayName)
- Zoom/Pan навигация
- Selection management (SelectedObjects, clipboard, copy/paste)
- Inline text editing
- Tool caching
- Preview management
- Grid nodes generation
- StatusBar properties
- Keyboard commands (24+ RelayCommand)

**Рекомендация на Этап 2:** Выделить в отдельные классы:
- `SelectionManager` — SelectedObjects, clipboard, copy/paste, select all
- `ZoomNavigator` — Zoom, PanOffset, FitToScreen
- `InlineTextEditor` — inline editing state и команды
- `ToolCache` — кэширование инструментов

Цель: сократить EditorViewModel до ~300 строк.

**SP:** 13 (отдельный спринт)

---

## ПЛАН СПРИНТА 23 — Устранение критических проблем

### Сводка

| Приоритет | Задач | Story Points |
|-----------|-------|--------------|
| P0 (Критические) | 6 | 15 SP |
| P1 (Серьёзные) | 6 | 16 SP |
| P2 (Улучшения) | 4 | 8 SP |
| **Всего** | **16** | **39 SP** |

### Детальный план

| # | Задача | Приоритет | SP | Затронутые файлы |
|---|--------|-----------|----|-----------------|
| 1 | **Fix MouseBehavior static state** — перенести в per-element Dictionary<UIElement> | P0 | 3 | MouseBehavior.cs, MouseBehaviorTests.cs |
| 2 | **Fix PreviewLineChangedBehavior leak** — добавить Unregister + вызов в Dispose | P0 | 3 | PreviewLineChangedBehavior.cs, EditorViewModel.cs |
| 3 | **Fix EditorCanvasBehavior._states leak** — подписка на Unloaded | P0 | 2 | EditorCanvasBehavior.cs |
| 4 | **SettingsService + ILogger + lock** — логирование ошибок + thread safety | P0 | 3 | SettingsService.cs, App.xaml.cs, SettingsServiceTests.cs |
| 5 | **AutosaveService tick dispatcher** — маршалинг в UI thread | P0 | 3 | AutosaveService.cs, IDispatcherService.cs |
| 6 | **PreviewLineChangedBehavior → ConcurrentDictionary** | P0 | 1 | PreviewLineChangedBehavior.cs |
| 7 | **ITool — свои enum** вместо WPF MouseButton/ModifierKeys | P1 | 5 | ITool.cs, все 6 Tools, EditorCanvasBehavior.cs |
| 8 | **ThemeService → ILogger** вместо Serilog.Log | P1 | 2 | ThemeService.cs, App.xaml.cs, ThemeServiceTests.cs |
| 9 | **Удалить IPrintService.Print(Template, ...)** из интерфейса | P1 | 1 | IPrintService.cs, PrintService.cs |
| 10 | **TextTool — настройки из ISettingsService** | P1 | 3 | TextTool.cs, AppSettings.cs, SettingsService.cs |
| 11 | **TabItemMiddleClickBehavior — без reflection** | P1 | 2 | TabItemMiddleClickBehavior.cs |
| 12 | **SnapIfEnabled → DrawingToolBase** (DRY, 3 инструмента) | P2 | 2 | DrawingLineTool, DrawingRectangleTool, TextTool, SnapExtensions.cs |
| 13 | **ILogger в TemplateService, FileService** | P2 | 3 | TemplateService.cs, FileService.cs |
| 14 | **IDispatcherService.Invoke(Action)** | P2 | 1 | IDispatcherService.cs, WpfDispatcherService.cs |
| 15 | **ValidationService.ValidateHexColors** — удалить или реализовать | P2 | 1 | ValidationService.cs |
| 16 | **+50 тестов** для покрытия P0/P1 фиксов | P2 | 5 | Тестовые файлы |

---

## СТРАТЕГИЧЕСКИЕ РЕКОМЕНДАЦИИ (Этап 2)

### 1. Декомпозиция EditorViewModel
- Выделить 3-4 подкласса-менеджера
- Цель: ~300 строк вместо текущих 872
- Улучшит тестируемость и читаемость

### 2. Spatial index для hit-testing
- Текущий линейный поиск O(N) будет тормозить при 1000+ объектах
- Рассмотреть QuadTree или Grid-based spatial hashing
- Особенно критично для больших схем

### 3. Performance profiling
- Замерить UI latency при А0 с 40 000 узлами сетки
- Проверить frequency layout pass'ов
- Оптимизировать GridNodesLayer (уже используется List вместо ObservableCollection)

### 4. CI/CD pipeline
- Настроить GitHub Actions для авто-сборки и тестов
- Gate: все тесты должны проходить, покрытие >= 70%
- Автоматический анализ кода (Roslyn analyzers)

### 5. Source Generators для конвертеров
- Уменьшить boilerplate в IMultiValueConverter
- Автоматическая генерация конвертеров из атрибутов

### 6. XML doc comments
- Документировать все public классы, методы, свойства
- Включить `<GenerateDocumentationFile>true</GenerateDocumentationFile>`
- Предупреждения CS1591 обрабатывать как warnings

---

## КРИТЕРИИ ПРИЁМКИ СПРИНТА 23

| Критерий | Требование |
|----------|-----------|
| Все 1249 существующих тестов проходят | ✅ |
| Новые тесты: минимум 50 | ✅ |
| Общее покрытие: >= 70% | ✅ |
| 0 memory leak (verified by profiler) | ✅ |
| 0 race condition (verified by stress test) | ✅ |
| Сборка: 0 ошибок, 0 предупреждений | ✅ |
| Все P0 проблемы устранены | ✅ |

---

**Итого:** 16 задач, 39 SP, ориентировочно 2 недели работы (1 разработчик).