---
name: wpf-tester
description: Use when testing or reviewing WPF applications — UI analysis, MVVM verification, DataBinding debugging, memory leak detection, STA-thread test patterns, XAML validation, dependency property checks, converter testing, and WPF-specific performance analysis.
---

# WPF Tester — Инструкции для тестирования WPF-приложений

Ты — опытный тестировщик WPF-приложений. Используй эти инструкции для анализа качества, поиска багов, проверки UI-логики и тестового покрытия.

## 1. MVVM — Проверка слоёв

- **ViewModel не должен знать о WPF.** Никаких `Dispatcher`, `UIElement`, `Visual`, `SolidColorBrush` в ViewModel/Service. Всё WPF — только во View и Converter'ах.
- **INotifyPropertyChanged** — проверь что все свойства, к которым биндится XAML, оповещают. Особенно computed-свойства без `[ObservableProperty]` — они должны явно дёргать `OnPropertyChanged()`.
- **`[ObservableProperty]` на reference-type** — source-генератор использует `EqualityComparer<T>.Default` (для reference = `ReferenceEquals`). Если мутируешь объект и переприсваиваешь — `PropertyChanged` НЕ стреляет. Нужен ручной сеттер с безусловным `OnPropertyChanged()`.
- **Forwarding-свойства** — если ViewModel пробрасывает свойство из manager'а, нужна подписка на `PropertyChanged` manager'а и ре-триггер. XAML должен биндиться напрямую к manager'у, а не к forwarding.
- **IDisposable в sub-VM** — подписки на `PropertyChanged` модели должны отписываться в `Dispose()`, иначе утечка. Lambda в конструкторе без поля-ссылки — не отписать.

## 2. XAML и DataBinding

- **Output窗口中检查Binding错误.** WPF пишет binding-ошибки в Debug Output. Пройдись по ключевым операциям (открыть шаблон, выбрать объект, изменить свойство) — в Output не должно быть `BindingExpression path errors`.
- **Mode=OneWay** для readonly-свойств. Забудь про `Mode=TwoWay` на computed.
- **DataTrigger vs Style Setter** — проверь precedence. DataTrigger переопределяет Setter через WPF precedence.
- **Converter** — каждый IValueConverter должен быть `sealed`, stateless, протестирован (пару тестов: convert + convertBack для граничных значений).
- **MultiBinding** — IMultiValueConverter обязательно проверь, что все входные значения корректного типа.
- **Команды с Async суффиксом** — `[RelayCommand]` на `void` НЕ обрезает `Async`. Только на `Task`. Имя будет `MethodAsyncCommand`, а не `MethodCommand`.
- **ContextMenu DataContext** — внутри Setter.Value DataContext не наследуется. Используй `{Binding PlacementTarget.DataContext, RelativeSource={RelativeSource Self}}`.

## 3. Тесты (xUnit v3 + Moq)

### Unit-тесты
- Все ViewModel, Service, Manager, Tool, Helper должны иметь unit-тесты.
- **Moq: `MockBehavior.Strict`** — только когда реально нужно проверить, что вызвано всё и ничего лишнего. Иначе `Loose`. Strict ломается при добавлении нового метода.
- **`Thread.Sleep` в тестах** — никогда. Вынеси `DateTime.UtcNow` в `IDateTimeProvider`.
- **Event-подписки в конструкторе** — не мокаются. Тестируй через вызов метода, который триггерит событие.
- **Moq не мокает non-virtual.** Для тестов, где нужен real `EditorViewModel`, создавай его через DI-фабрику, а не через mock.

### STA-тесты (поведения, attached properties)
- `new TextBox()`, `new ComboBox()` — требуют STA. Используй `WpfContext.Execute()`.
- Для DependencyProperty get/set без создания элемента — может работать в MTA.
- Для тестов attached behaviors: handlers должны быть `internal static`, тестируй их напрямую без WPF-дерева.
- `PresentationSource` в .NET 10 — требует `GetCompositionTargetCore()`, `RootVisual` get/set, `IsDisposed`.

### Coverage
- CI gate: ≥75% line-rate. Перед пушем: `dotnet test --collect:"XPlat Code Coverage"`
- Если coverage упал, ищи не тестированные ветки в новых классах.

## 4. Типичные WPF-баги в проекте

### 4.1. Координаты и RenderTransform
- `e.GetPosition(canvas)` **уже учитывает** `RenderTransform`. Не вычитай `PanOffset` повторно — hit-test сломается.
- Canvas-relative `MouseMove` для панорамирования — не использовать. Только Window-relative (стабильный фрейм).
- Pan delta: от сохранённой начальной позиции объекта, а не от текущей (дрифт на каждом MouseMove).
- Panning: всегда `CaptureMouse()` / `ReleaseMouseCapture()`.

### 4.2. Canvas и DataTemplate
- `Canvas.Left`/`Canvas.Top` биндятся к модельным координатам. Все persistent-свойства — INPC.
- Preview shapes: create one per tool, mutate in MouseMove, re-assign ссылку.
- HitTest: только на MouseDown, не на MouseMove.
- Rectangle hit-test: border-band (не AABB). Только ближе 5 мм от края.

### 4.3. Grid
- Микроны, не пиксели. `GridNodesLayer.OnRender` сам конвертирует.
- MinPixelSpacing (5px) — если шаг сетки даёт меньше — не рендерить.
- `(long)` каст → `(long)Math.Round()`.
- Единственный owner GridSettings — GridManager. StatusBarManager — только делегаты.

### 4.4. Поворот текста (WPF RotateTransform)
- WPF RotateTransform: CW в screen-space = CCW в model-space.
- ContainsPoint: INVERSE transform (`u = x*cos + y*sin, v = -x*sin + y*cos`).
- BoundingBox/Corner: FORWARD transform.
- Точка вращения: (X, Y+H) в WPF-координатах (верх текста).

### 4.5. SelectedBox / Маркеры
- ShowSelectionMarkers: `Count > 0`, не `== 1`. Маркеры — ItemsControl, не ContentControl.
- PurgeOrphanedSelection после Undo/Redo.
- SelectionVersion (int) + IMultiValueConverter для визуального состояния выделения.

### 4.6. Undo/Redo
- Multi-object операции (Move, Rotate, Delete, Paste) — группировать в BatchCommand.
- CustomResizeCommand: `ApplyResize`, не switch по типу объекта.
- `MarkDirty()` — только через команды. Не вручную.

## 5. Проверка утечек памяти

- **Event-подписки** — самое частое. Каждый `+=` должен иметь `-=`. Особенно:
  - `INotifyPropertyChanged.PropertyChanged` модели → ViewModel
  - `CollectionChanged` коллекции → Manager/ViewModel
  - `WeakReferenceMessenger` подписки — проверить, что отписка есть
- **Лямбды в конструкторе** — не отписать без поля-ссылки. Сохраняй handler в поле.
- **PrintVisualProvider** — всегда null-out в Dispose.
- **Dispatcher.Timer / async void** — try/catch с логом. Исключение в async void убивает процесс незаметно.
- **DependencyObject без отписки от Binding** — при закрытии вкладки.

## 6. Производительность WPF

- Не создавай Shape/DrawingVisual на каждый MouseMove. Переиспользуй.
- Canvas (не Grid/StackPanel) для EditorCanvas — layout pass дорогой.
- Grid nodes: новая аллокация `long[]` на каждый refresh — нормально. Shared mutable — нет.
- MinPixelSpacing для сетки: не регенерируй на pan-move, только RenderTransform.
- Async void в таймерах — лови и логируй Exception.

## 7. Тест-план (чеклист для новой фичи)

1. **Слой Model:** сериализация XML round-trip, Clone(), Move(), ContainsPoint(), граничные значения (отрицательные координаты, 0, Max)
2. **DTO маппинг:** MapToObject → MapToDto → MapToObject — без потерь
3. **ViewModel:** команды (execute + canExecute), computed-свойства (PropertyChanged), UpdateSelection, Dispose
4. **Manager:** state transition, property changed, collection changed, граничные (null, empty)
5. **Converter:** Convert/ConvertBack с нормальными и граничными значениями
6. **Tool:** OnMouseDown/Move/Up, OnKeyDown, OnDoubleClick, Reset, cursor, escape behaviour
7. **CommandHistory:** Undo/Redo, batch, purge orphaned
8. **STA-тесты (если attached behavior):** WpfContext, internal static handlers
9. **XAML:** Debug Output binding errors, DataContext, конвертеры
10. **Memory:** Dispose(), отписки, null-out
