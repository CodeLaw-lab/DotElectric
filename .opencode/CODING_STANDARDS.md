# Coding Standards — DotElectric Template Editor

Это **единственный источник истины** для всего кода проекта.
All agents MUST read this file before any code operation.

Если правило в AGENTS.md, skill или любом другом документе противоречит этому — побеждает этот документ.

**Build:** 0 errors, 0 warnings  
**Tests:** 1840 passed, 1 pre-existing skip  
**Coverage:** ≥75% line-rate (CI gate)

---

## 1. Coordinate System

### 1.1 Fixed-Point Microns
- Все внутренние координаты — `long` (микроны), никогда `double`
- 1 mm = 1000 microns
- `(long)` каст → `(long)Math.Round()` где нужна точность
- Round-trip сериализация без потери точности

### 1.2 Cartesian Model, WPF Y-flip
- Модель: Декартова система (0,0 = левый нижний, Y↑)
- WPF: Inverted Y (Y↓)
- Конвертация ТОЛЬКО в EditorCanvas через `FromWpfPoint()` / `ToWpfPoint()`
- ViewModel/Service НЕ ЗНАЮТ о WPF-координатах

### 1.3 Prohibited
- `double` для координат — запрещено
- Вычитание PanOffset из `e.GetPosition(canvas)` — `e.GetPosition` уже учитывает RenderTransform
- Canvas-relative координаты для панорамирования — только Window-relative

---

## 2. Model Layer

### 2.1 Inheritance
- Все модели наследуют `TemplateObjectBase : ObservableObject` (один уровень, не три)
- `ObjectBase` и `ModelBase` удалены — не использовать
- Все наследники `TemplateObjectBase`: sealed

### 2.2 INPC (INotifyPropertyChanged)
- Все persistent-свойства (LineType, координаты, цвета, толщина, размеры, RotationAngle) — INPC с backing fields
- Используй `[ObservableProperty]` source generator из CommunityToolkit.Mvvm
- `[ObservableProperty]` на reference-type — проверь, не подавлен ли PropertyChanged при re-assign (EqualityComparer == ReferenceEquals). Если да → ручной сеттер с безусловным `OnPropertyChanged()`
- Computed-свойства (RightMicronsX, BottomMicronsY, CenterMicronsX/Y) — явный `OnPropertyChanged()` в сеттерах зависимых свойств
- `[NotifyPropertyChangedFor(nameof(...))]` для зависимых свойств
- Selection state (выделение) — НЕ через INPC модели, через `SelectionVersion` + `IsObjectSelectedConverter`

### 2.3 Clone Pattern
- `Clone()` — создаёт новый экземпляр со всеми полями, кроме `Id` (новый Guid)
- Тест: "все public properties равны кроме Id" для каждого типа
- При добавлении нового поля — обязательно обновить Clone()

### 2.4 ContainsPoint/HitTest
- **Rectangle:** border-band approach (расширенные bounds минус суженный interior). Interior > LineHitToleranceMicrons от краёв — НЕ selectable
- **Text:** INVERSE RotateTransform для unrotate точки в локальное пространство (см. раздел 4.4)
- HitTest ТОЛЬКО на MouseDown, НЕ на MouseMove

### 2.5 Serialization (DTO)
- MapToObject / MapToDto — каждое public свойство модели маппится
- Round-trip тест: создать → MapToDto → MapToObject → сравнить все свойства кроме Id
- XSD: `xs:long` для координат, `xs:string` для текста/цветов, `xs:boolean` для флагов
- Новые поля — опциональны для backward compatibility (nullable в DTO)

---

## 3. ViewModel Layer

### 3.1 NO WPF Types
- ViewModel НЕ содержит: `Dispatcher`, `UIElement`, `Visual`, `SolidColorBrush`, `Brush`, `Control`, `FrameworkElement`
- Всё WPF — только во View и Converter'ах
- Исключение: `IPrintVisualProvider` — через интерфейс, не через `Func<Visual?>`

### 3.2 NO Forwarding Properties
- После R3.1 XAML биндится напрямую к manager'ам (`ZoomPanManager.ZoomPercent`, `GridManager.IsGridEnabled`)
- Forwarding-свойства — удалены. Не создавай новые
- IEditorContext — содержит только то, что нужно Tools (не EditorViewModel целиком)

### 3.3 IEditorContext (для Tools)
- `IEditorContext` — абстракция контекста для Tools
- Tools НЕ принимают `EditorViewModel` напрямую
- `OnMouseWheel` возвращает `bool` (true = обработано, zoom не применять)

### 3.4 IDisposable
- Sub-VM (LinePropertiesVM, RectanglePropertiesVM, TextPropertiesVM): `IDisposable` с отпиской от INPC модели
- Managers: `IDisposable` с отпиской от CollectionChanged и PropertyChanged
- Lambda в конструкторе с подпиской — handler сохранён в поле для отписки
- PrintVisualProvider null-out в Dispose

### 3.5 WeakReferenceMessenger
- Подписки — обязательно отписываются в Dispose
- `WeakReferenceMessenger.Default.Unregister<MessageType>(this)`

---

## 4. View Layer (XAML)

### 4.1 Canvas
- EditorCanvas: ТОЛЬКО `Canvas`, не `Grid`/`StackPanel`/`WrapPanel`
- `Canvas.Left`/`Canvas.Top` — через `ModelXToCanvasLeftConverter`/`ModelYToCanvasTopConverter` + Zoom

### 4.2 Binding
- `Mode=OneWay` для readonly-свойств
- `Mode=TwoWay` только для редактируемых полей (TextBox, CheckBox)
- `FallbackValue`/`TargetNullValue` для nullable binding'ов
- `StringFormat` для чисел: `{}{0:N2}`, `{}{0:F2} мм`
- `x:DataType` для compile-time binding (если .NET 10)
- ContextMenu внутри Setter.Value — DataContext через `{Binding PlacementTarget.DataContext, RelativeSource={RelativeSource Self}}`
- Converter'ы — через `StaticResource`, не `{x:Static}`

### 4.3 DataTemplate
- DataTemplate по типу объекта (implicit DataType), не по имени ключа
- `ContentControl` + `ContentTemplateSelector` для переключения по типу
- Selection markers — `ItemsControl ItemsSource="{Binding SelectedObjects}"` с Canvas ItemsPanel, не ContentControl
- Маркер позиции — через `MarkerPosition` attached behavior

### 4.4 WPF RotateTransform (Text rotation)
- **WPF матрица:** `x'=x*cosθ−y*sinθ, y'=x*sinθ+y*cosθ` (STANDARD CCW Cartesian)
  В Y-down (screen) = CW вращение
- **ContainsPoint:** INVERSE transform `u = x*cos + y*sin, v = -x*sin + y*cos`
- **BoundingBox/Corner:** FORWARD transform
- **Точка вращения:** (X, Y+H) в WPF-координатах = верх текста
- GostA.ttf: `#GOST Type AU`, GostB.ttf: `#GOST Type BU` (case-sensitive)

### 4.5 Theme
- Ресурсы тем: отдельные XAML (`LightTheme.xaml`, `DarkTheme.xaml`)
- Canvas Background: всегда белый (`#FFFFFF`), не зависит от темы
- MaterialDesign accent: `#0078D4`
- Новые контролы: проверить Light/Dark стили

### 4.6 Preview Pattern
- Create один раз в OnMouseDown
- Mutate свойства в OnMouseMove
- Re-assign ссылку для триггера INPC (ручной сеттер с безусловным `OnPropertyChanged()`)

### 4.7 Маркеры выделения
- `ShowSelectionMarkers` = `SelectedObjects.Count > 0` (не `== 1`)
- `SelectionVersion` (int) + `IMultiValueConverter` для визуального состояния выделения
- `PurgeOrphanedSelection()` после Undo/Redo

---

## 5. Commands & Undo/Redo

### 5.1 IUndoCommand (not ICommand)
- Все Undo-команды: `DotElectric.TemplateEditor.Commands.IUndoCommand`
- НЕ `System.Windows.Input.ICommand` (это WPF routing)
- `IUndoCommand`: `Execute()`, `Undo()`, `Name`

### 5.2 BatchCommand
- Multi-object операции (Move, Rotate, Delete, Paste) — BatchCommand
- Undo — обратный порядок (LIFO)
- `SelectedObjects.Count > 1` → BatchCommand
- Исключение: одиночный объект → обычная команда

### 5.3 CommandHistory
- 50 уровней Undo, авто-обрезка oldest
- `PurgeOrphanedSelection()` после Undo/Redo
- Checkpoint: история НЕ очищается при сохранении

### 5.4 MarkDirty
- IsDirty только через `_dirtyStateManager.MarkDirty()` в командах
- НЕ вручную, НЕ в сеттерах свойств
- `BatchCommand.Name` используется для сообщения в статус-баре

### 5.5 Rollback
- При failed Undo — rollback + логирование через Serilog
- `[RelayCommand]` на `void` с `Async` суффиксом — имя команды = `MethodAsyncCommand` (суффикс НЕ обрезается)

### 5.6 CustomResizeCommand
- Полиморфный `ApplyResize(CaptureResizeState())` на объекте
- НЕ switch по типу объекта
- `ResizeMath` — чистые функции, не в ResizeTool напрямую

---

## 6. Services & DI

### 6.1 Interface-First
- Каждый service = интерфейс (I*Service) + реализация (*Service)
- Все service — Singleton (stateless)
- ViewModel — Transient через фабрику (IEditorViewModelFactory)

### 6.2 Registration
- Все service/ViewModel зарегистрированы в `App.xaml.cs`
- `services.AddSingleton<I*Service, *Service>()`
- `services.AddTransient<IEditorViewModelFactory, EditorViewModelFactory>()`
- Нет циклических зависимостей

### 6.3 NO Static Services
- `ValidationService` — injectable `IValidationService`/`ITemplateValidator` (через DI)
- `FontMetrics` — `IFontMetrics` + `FontMetrics.Default` static instance
- `EditorConstants` → `PhysicalConstants` + `EditorSettings`
- WPF-диалоги: `IDialogFileService`/`WpfDialogFileService` для CI/testability

### 6.4 Dual-Write Prohibited
- Никогда два manager'а с независимыми копиями одного settings
- GridSettings — только в GridManager; StatusBarManager — делегаты (get/set lambdas)

---

## 7. Tools

### 7.1 ITool Interface
- `ITool` с методами: OnMouseDown, OnMouseMove, OnMouseUp, OnKeyDown, OnDoubleClick, OnMouseWheel, Reset
- `OnMouseWheel` возвращает `bool` (true = обработано, zoom не применять)
- Кэширование через `GetOrCreateTool<T>()`

### 7.2 IEditorContext (см. 3.3)
- Tools принимают `IEditorContext`, не `EditorViewModel`

### 7.3 Preview Pattern
- Preview фигуры: create в OnMouseDown, mutate в OnMouseMove, re-assign

### 7.4 Drag/Move
- Drag delta от сохранённой начальной позиции (`_initialPositions[obj]`), НЕ от `obj.MicronsX`
- Pan delta от Window-relative координат, НЕ от `e.GetPosition(canvas)`
- CaptureMouse / ReleaseMouseCapture для панорамирования
- Clamp к границам листа при nudge и drag

### 7.5 Resize
- 8 маркеров, Shift=пропорции, Ctrl=от центра
- Clamp минимального размера — ТОЛЬКО moving edge, не fixed
- `ResizeMath` — чистые функции для resize-геометрии

### 7.6 Keyboard
- Tool switching (V/L/R/T) — через PreviewKeyDown (раскладка-независимо)
- `e.KeyboardDevice.Modifiers != ModifierKeys.None` → пропустить (Ctrl+V, Ctrl+L и т.д.)
- Rotate: E (CW 90°), Shift+E (CCW 90°), свободный 0-359°
- ShortcutRegistry — единая точка входа, не code-behind

---

## 8. Managers

### 8.1 Single Source of Truth
- Каждый manager — ObservableObject
- Нет dual-write (см. 6.4)
- Managers — public properties на EditorViewModel, XAML биндится напрямую

### 8.2 List of Managers
| Manager | Responsibility | Key Property |
|---------|---------------|--------------|
| ZoomPanManager | Zoom, PanOffset, FitToScreen | Zoom, PanOffsetX/Y |
| SelectionManager | SelectedObjects, SelectionBox | ShowSelectionMarkers |
| ClipboardManager | Copy, Paste, Cut | GetClipboardContents() |
| ToolManager | ActiveTool, Push/Pop, GetOrCreateTool | ActiveTool |
| PreviewManager | PreviewLine/Rectangle/Text | — |
| InlineEditManager | Active inline editing state | — |
| StatusBarManager | Status message, grid/snap state | StatusMessage |
| GridManager | GridSettings (OWNER), refresh | IsGridEnabled, IsSnapEnabled, GridStepMm |
| DirtyStateManager | IsDirty, FilePath, DisplayName | IsDirty |

### 8.3 GridManager Specific
- `IsGridEnabled`/`IsSnapEnabled` — вызывают OnPropertyChanged()
- `ComputeDisplayStep(preferredStepMicrons)` — учитывает пользовательский шаг
- `MinPixelSpacing` (5px) — если шаг даёт меньше — не рендерить
- `MaxGridNodes` (100000) — защита от переполнения
- GridNodes: новая аллокация `long[]` на каждый refresh (не shared mutable)
- Pan-move: только RenderTransform (без регенерации)
- Pan-end: прямой вызов RefreshGridNodes() (без debounce)

---

## 9. Converters

### 9.1 General Rules
- Все converter'ы: `sealed`, stateless
- `[ValueConversion(typeof(TFrom), typeof(TTo))]` attribute
- Зарегистрированы в ResourceDictionary (App.xaml или UserControl.Resources)
- НЕ через `{x:Static}`

### 9.2 IValueConverter
```csharp
[ValueConversion(typeof(long), typeof(double))]
public sealed class MicronsToPixelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is long microns) return microns * zoom / 1000.0;
        return 0.0;
    }
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
```

### 9.3 Testing Matrix
| Test | Input | Expected |
|------|-------|----------|
| Normal | valid value | correct conversion |
| Null | null | 0.0 / false / DependencyProperty.UnsetValue |
| Wrong type | string, object | 0.0 / false / fallback |
| Boundary | 0, long.MinValue, long.MaxValue | no exception |
| ConvertBack | any value | NotSupportedException (if readonly) |

### 9.4 MultiBinding (IMultiValueConverter)
- Проверить, что каждая позиция values[] корректного типа
- `IsObjectSelectedConverter` — IMultiValueConverter для визуального выделения

---

## 10. Testing

### 10.1 Framework
- xUnit v3, Moq 4.20+
- `MockBehavior.Loose` (не Strict — Strict ломается при добавлении новых методов)
- Нет `Thread.Sleep` → `IDateTimeProvider` + `SetupSequence`

### 10.2 Coverage
- CI gate: ≥75% line-rate
- Новый код: ≥80%
- Coverage exclusions: `.xaml`, `.xaml.cs`, `App.xaml.cs`
- `dotnet test --collect:"XPlat Code Coverage"` перед push

### 10.3 Test Naming
```csharp
[Fact]
public void MethodName_Scenario_ExpectedResult()
{
    // Arrange
    // Act
    // Assert
}
```

### 10.4 STA Tests
- WPF-элементы (TextBox, ComboBox, Button) — через `WpfContext.Execute(action)`
- internal static handlers — тестировать напрямую без WPF-дерева
- `[Collection("FontMetrics", DisableParallelization = true)]` для flaky
- DependencyProperty get/set без элемента — может работать в MTA
- Нет `new WpfElement()` вне STA

### 10.5 Moq Rules
- Non-virtual методы — не мокать, использовать real instance
- `SetupSequence` для IDateTimeProvider (вместо Thread.Sleep)
- `mock.Verify()` — только для критических вызовов

### 10.6 What to Test by Layer
| Layer | What | Tool |
|-------|------|------|
| Model | Clone, Move, ContainsPoint, round-trip | xUnit |
| DTO | MapToObject→MapToDto—no loss | xUnit |
| ViewModel | Commands, computed properties, PropertyChanged, Dispose | xUnit + Moq |
| Manager | State transitions, PropertyChanged, Dispose | xUnit + Moq |
| Converter | Convert/ConvertBack (normal, null, wrong type, boundary) | xUnit |
| Tool | MouseDown/Move/Up, KeyDown, DoubleClick, Reset, Cursor | xUnit + Moq |
| Behavior (WPF) | internal static handlers, WpfContext | xUnit + WpfContext |

---

## 11. Serialization

### 11.1 File Format (.tdel)
- ZIP-архив с `template.xml` внутри
- XML namespace: `http://dotelectric.local/schema/template`
- Все координаты в микронах (`xs:long`)
- Цвета: HEX `#RRGGBB` / `#AARRGGBB` / `Transparent`
- FontSize: в пунктах (`double`), не микроны

### 11.2 Round-Trip
- DTO → Model → DTO — без потери данных
- Тест на каждый тип объекта: создать → сериализовать → десериализовать → сравнить
- Новое поле в DTO: `[XmlIgnore]` если не нужно / nullable для backward compat

---

## 12. Naming Conventions

### 12.1 Types
| Convention | Example |
|------------|---------|
| Abstract class (not I-prefix) | `TemplateObjectBase` (not `ITemplateObject`) |
| Interface prefix I | `ITool`, `IEditorContext`, `IUndoCommand` |
| Sealed by default | `sealed class MicronsToPixelConverter` |
| Manager suffix | `ZoomPanManager`, `SelectionManager` |
| Tool suffix | `SelectTool`, `ResizeTool` |
| View suffix | `MainWindow`, `SettingsView` |
| ViewModel suffix | `MainViewModel`, `EditorViewModel` |

### 12.2 Files
- One type per file (кроме small enums/records)
- File name = type name (case-sensitive)
- `Commands/IUndoCommand.cs` (not ICommand.cs — конфликт с WPF)

### 12.3 Namespaces
- `DotElectric.TemplateEditor.Models.Objects`
- `DotElectric.TemplateEditor.ViewModels.Managers`
- `DotElectric.TemplateEditor.Converters`
- `DotElectric.TemplateEditor.Helpers`

---

## 13. Error Handling

### 13.1 Logging
- Serilog, structured logging
- `_logger.Error(ex, "Message {Context}", ctx)` — не `_logger.Error(ex.Message)`

### 13.2 async void
- ВСЕГДА try/catch внутри async void
- Timer handlers: try/catch + _logger.Error
- `OnAutosaveTickHandler` — async Task, не async void

### 13.3 User Feedback
- Критические ошибки: IDialogService / IMessageBoxProvider
- Статус-бар: StatusBarManager.StatusMessage
- Ошибки валидации: ValidationService → PropertiesViewModel

### 13.4 Global Handlers
- `App.xaml.cs`: `DispatcherUnhandledException`, `TaskScheduler.UnobservedTaskException`
- Mutex для одного экземпляра

---

## 14. Performance

### 14.1 WPF Rendering
- Не создавай Shape на каждый MouseMove — переиспользуй preview
- Canvas для EditorCanvas (Grid/StackPanel дорогие в layout pass)
- Freezable: `Freeze()` если объект shared между потоками
- MinPixelSpacing (5px) для сетки — не рендерить слишком плотно

### 14.2 Memory
- Event подписки — всегда отписывать в Dispose
- ConditionalWeakTable для attached behavior подписок (не strong reference)
- PrintVisualProvider null-out в Dispose

### 14.3 Grid
- Новая аллокация `long[]` на RefreshGridNodes — нормально
- Shared mutable `long[]` — запрещено
- Панорамирование: только RenderTransform, без регенерации
- Pan-end: прямой вызов (без debounce, без кэша)

### 14.4 Tools
- Кэширование через `GetOrCreateTool<T>()`
- HitTest только на MouseDown (не MouseMove)

### 14.5 Collections
- `.ToList()` в hot paths — избегать
- `SelectedObjects` — ObservableCollection<TemplateObjectBase>
- `_clipboard` — List<TemplateObjectBase>

---

## 15. Prohibited Patterns (Anti-Patterns)

Эти паттерны ЗАПРЕЩЕНЫ в любой форме:

| # | Anti-Pattern | Why | Replacement |
|---|-------------|-----|-------------|
| 1 | `double` для координат | Потеря точности при round-trip | `long` microns |
| 2 | WPF-типы в ViewModel | Нарушение MVVM | Converter / interface |
| 3 | Static Service (ValidationService) | Нетестируемо | DI interface |
| 4 | Forwarding-свойства на EditorViewModel | Хрупкость, дублирование | XAML → manager напрямую |
| 5 | EditorConstants | Смешивание физики и UI | PhysicalConstants + EditorSettings |
| 6 | `Thread.Sleep` в тестах | Flaky, медленно | IDateTimeProvider |
| 7 | `MockBehavior.Strict` | Ломается при новых методах | Loose |
| 8 | Shape на каждый MouseMove | Утечка памяти, GC pressure | Preview pattern |
| 9 | Grid/StackPanel в EditorCanvas | Layout pass overhead | Canvas |
| 10 | Switch по типу в Execute/Undo | Нарушение OCP | Полиморфный ApplyResize |
| 11 | Dual-write settings | Рассинхрон | Single source of truth |
| 12 | async void без try/catch | Silent process death | try/catch + logger |
| 13 | IAutosaveTab внутри AutosaveService | Инверсия зависимости | Отдельный файл |
| 14 | KeyBinding в Window.InputBindings | Зависит от раскладки | PreviewKeyDown |
| 15 | Switch по типу в `GetCurrentTool()` | Нарушение OCP | Case для каждого инструмента |
| 16 | `IsDirty` вручную | Рассинхрон с CommandHistory | MarkDirty() только в командах |
| 17 | `obj.MicronsX + delta` для drag | Дрифт на каждом MouseMove | `initialPos + delta` |
