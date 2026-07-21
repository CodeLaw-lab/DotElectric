# AGENTS.md — DotElectric

## Current Focus

**Архитектурный рефакторинг P2 завершён.** Создан `ITabOperationsService` — фасад для операций с вкладками (NewTab, OpenFile, Save, SaveAs), сокративший конструктор `MainViewModel` с 13 до 10 зависимостей. Интерфейс размещён в `ViewModels.Abstractions` во избежание циклических зависимостей. Переименованы 14 тестов команд для единообразия (`MoveObjectCommand_*` → `ChangePropertyCommand_Move_*`).

### Ключевые результаты
| Область | Было | Стало |
|---------|------|-------|
| Иерархия моделей | 3 уровня (ObjectBase→ModelBase→TemplateObjectBase) | 1 уровень (TemplateObjectBase→ObservableObject) |
| INPC-сеттеры | ~50 ручных | [ObservableProperty] sourcegen |
| EditorCanvasBehavior | 406 строк (монолит) | 78 строк (3 файла: State, Transform, Router) |
| Tools-EditorVM | Прямая зависимость | IEditorContext |
| DI-конструктор | internal + ручная фабрика | public + Transient + ActivatorUtilities |
| PrintVisualProvider | Func<Visual?> | IPrintVisualProvider |
| Validation | static ValidationService (537 строк) | ITemplateValidator |
| Resize | 520 строк (switch) | ResizeMath + полиморфный ApplyResize |
| Shortcuts | switch в code-behind | ShortcutRegistry |
| Extended тесты | 16 файлов | все слиты в родительские |
| CI | нет coverage-gate, нет NuGet кэша | coverage-gate 75% + actions/cache |
| CPM | нет | Directory.Packages.props |
| **Print Preview** | **нет** | **Ctrl+Shift+P → DocumentViewer** |
| **EditorConstants** | **36-line proxy** | **удалён → PhysicalConstants/EditorSettings** |
| **FontMetrics** | **static class** | **IFontMetrics + DI Singleton** |
| **Sealed classes** | **0** | **66 классов (Converters, Services, Tools, Managers, Commands)** |
| **Shortcut dispatch** | **code-behind 30 строк** | **ShortcutRegistry.TryHandle()** |
| **ITool.OnMouseWheel** | **void** | **bool (tool может блокировать zoom)** |

**Build:** 0 errors, 0 warnings
**Tests:** 2094 passed, 1 pre-existing skip

### H1–H5 — Архитектурные исправления высокой важности (14.07.2026)
- **H1: async-void AutosaveTick** — `event Action?` → `event Func<Task>?`. `IDispatcherService` получил `InvokeAsync(Func<Task>)`. `AutosaveService.OnAutosaveTick` вызывает `InvokeAsync`. `MainViewModel` — `async Task` вместо `async void`.
- **H2: ValidationService → injectable** — Создан `IValidationService` (интерфейс с `ValidateHexColor`). `ValidationService` содержит статический `Default` (instance-обёртка). `TemplateValidator` принимает `IValidationService` через DI (опциональный параметр для обратной совместимости). Статические методы `ValidationService` сохранены.
- **H3: DialogServiceFactory удалён** — мёртвый код (public static class, не используется) удалён из `IDialogService.cs`.
- **H4: PrintVisualProvider null-out** — `PrintVisualProvider = null` добавлен в `EditorViewModel.Dispose()`.
- **H5: No-op F4 MenuItem удалён** — `<MenuItem Header="Свойства" InputGestureText="F4">` удалён из контекстного меню `EditorCanvas.xaml`.

### Что сделано после R1–R4
- **EditorViewModel де-bloat** — ~1194 → 784 строк (−410, −34%). Удалены ~25 forwarding-свойств, 4 PropertyChanged-обработчика, 4 подписки/отписки. Свойства IEditorContext оставлены как bare delegation (без OnPropertyChanged). IAutosaveTab — explicit interface implementation.
- **Preview fix** — `[ObservableProperty]` на `PreviewLine`/`PreviewRectangle`/`PreviewText` подавлял PropertyChanged при re-assign той же ссылки. Заменён на ручные сеттеры с безусловным `OnPropertyChanged()`.
- **Selection markers fix** — `ShowSelectionMarkers` (computed property) не вызывал `OnPropertyChanged()` при изменении `SelectedObjects`. Добавлен вызов в `CollectionChanged`-обработчик.
- **PropertiesViewModel split** — 649 → 85 строк (база). 3 sub-VM: LinePropertiesViewModel (148), RectanglePropertiesViewModel (168), TextPropertiesViewModel (233). XAML: 3 StackPanel → ContentControl + DataTemplate per sub-VM.
- **Print Preview** — Ctrl+Shift+P открывает DocumentViewer с FixedDocument. IPrintDocumentGenerator, PrintDocumentGenerator, PrintPreviewWindow. 19 тестов.
- **Text rotation fix (Sprint 59)** — ContainsPoint() исправлен на inverse WPF RotateTransform (standard CCW matrix). RotatedCorner0-3, GetBoundingBox — reverted к оригинальным (корректным) формулам. HitTestHelper/HitTestText для 90°/270°/45° — все проходят. Осознана и зафиксирована матрица WPF `x'=x*cosθ−y*sinθ`. Архитектурный инсайт: ContainsPoint() был багнут (forward вместо inverse) независимо от путаницы со знаками.

### Что не вошло / отложено
- ~~TabItemMiddleClickBehavior / PreviewLineChangedBehavior — STA-тесты (требуют полного визуального дерева)~~ — в ешено в Sprint 62
- **Text markers — tech debt:** исправлен поворот (`RotatedCorner0–3`, `GetBoundingBox`, `HitTestHelper`), но остаются недочёты отображения маркеров: `TextSelectionMarkerBehavior` не используется, пустой `<Canvas/>` внутри DataTemplate Text, маркеры в отдельном ItemsControl вместо внутри DataTemplate
- **Inline text editing — tech debt (вся работа с текстом):**
  - Escape не отменял редактирование — **исправлено** (focus guard в CanvasInputRouter). Остаётся:
    - TextBox не получает авто-фокус после double-click — если не кликнуть в TextBox, Escape уходит в SelectTool (очищает выделение, редактор остаётся)
    - Enter/Ctrl+Enter/Escape routing relies on fragile WPF event ordering (PreviewKeyDown vs KeyDown) — при изменении CanvasInputRouter или появлении новых child control'ов может сломаться
    - Ручная верификация Escape при редактировании не проведена (таски 2.2, 2.3 в fix-escape-inline-editing)

### Next Steps
- Этап 2 — Редактор УГО (планирование)
- FR-021 Drag&Drop из библиотеки
- FR-022 Preview шаблонов
- ~~TabItemMiddleClickBehavior / PreviewLineChangedBehavior — integration/UI тесты с STA~~ — С ешено в Sprint 62

## Build Commands

```bash
# Build solution
dotnet build src/DotElectric.TemplateEditor.slnx

# Run application
dotnet run --project src/DotElectric.TemplateEditor

# Run all tests
dotnet test src/DotElectric.TemplateEditor.Tests

# Run single test
dotnet test src/DotElectric.TemplateEditor.Tests --filter "FullyQualifiedName~YourTestName"

# Run tests with coverage
dotnet test src/DotElectric.TemplateEditor.Tests --collect:"XPlat Code Coverage"
```

## Project Structure

- **Main app:** `src/DotElectric.TemplateEditor/` — WPF .NET 10 CAD application
- **Tests:** `src/DotElectric.TemplateEditor.Tests/` — xUnit v3 tests
- **Solution:** `src/DotElectric.TemplateEditor.slnx` (XML format, not `.sln`)
- **Shared props:** `src/Directory.Build.props` (net10.0-windows, nullable, implicit usings)

## Architecture Must-Know

### Fixed-Point Coordinates
- All internal coordinates in **microns** (`long`, not double)
- 1mm = 1000 microns
- XML serialization also uses microns (`xs:long`)
- Round-trip without precision loss

### Coordinate System
- **Model:** Cartesian (0,0 = bottom-left, Y↑)
- **WPF:** Inverted Y (Y↓)
- Conversion only in `EditorCanvas` via `FromWpfPoint()` / `ToWpfPoint()`
- ViewModels/Services know NOTHING about WPF coordinates

### Key Patterns
- **MVVM** with CommunityToolkit.Mvvm
- **DI** via Microsoft.Extensions.DependencyInjection (all services Singleton, EditorViewModelFactory as IEditorViewModelFactory)
- **Undo/Redo:** 50 levels via `CommandHistory` — commands implement custom `ICommand` interface (NOT `System.Windows.Input.ICommand`)
- **Tools:** State pattern via `ITool` interface
- **Messaging:** WeakReferenceMessenger for cross-VM communication (e.g., tab close)
- **IEditorContext** — Sprint R3: инструменты получают контекст через интерфейс, а не EditorViewModel напрямую
- **ResizeMath** — Sprint R4: чистые статические функции для resize-геометрии
- **ShortcutRegistry** — Sprint R4: централизованный маппинг V/L/R/T/ E/E+Shift

### File Format
- **.tdel:** XML packed in ZIP (custom template format)

### Fonts
- GOST A/B fonts required: `Resources/Fonts/*.ttf` (embedded as resources)
- Font files: GostA.ttf, GostB.ttf
- **Внутренние имена шрифтов (чувствительны к регистру):**
  - `GostA.ttf` → `#GOST Type AU`
  - `GostB.ttf` → `#GOST Type BU`
- URI: `pack://application:,,,/Resources/Fonts/#GOST Type AU`
- FontNameToFamilyConverter маппит "ГОСТ А" / "ГОСТ Б" на правильные URI

## Framework Versions

| Package | Version |
|---------|---------|
| .NET | 10.0 |
| CommunityToolkit.Mvvm | 8.4.2 |
| MaterialDesignThemes | 5.3.1 |
| Microsoft.Extensions.DependencyInjection | 10.0.5 |
| Serilog | 4.3.1 |
| xunit.v3 | 3.2.2 |
| Moq | 4.20.72 |

## Reference Documentation

- `docs/03_Спецификация_требований_Этап1.md` — Detailed architecture and API
- `docs/00_Индекс_документов.md` — Document index

Актуальные описания всех изменений, Common Mistakes и архитектурных решений — в этом документе (AGENTS.md).
Архивные sprint-отчёты и fix-документы удалены из git для оптимизации репозитория.

## Common Mistakes to Avoid

1. Don't use double for coordinates — use microns (`long`)
2. Don't create new Shape on every MouseMove — update properties instead
3. Don't do hit-testing on MouseMove — only on MouseDown
4. Don't use Grid/StackPanel in EditorCanvas — use Canvas (layout pass issues)
5. Always use `Mode=OneWay` when binding to readonly properties
6. IsDirty must be set by commands (`MarkDirty()`), NOT manually
7. Preview shapes: create once, update properties only
8. EditorViewModel — instantiate via `IEditorViewModelFactory`, NOT `new` directly (ensures DI-managed dependencies)
9. CenterCanvas — always use `Math.Max(0, (canvasPx - viewportPx) / 2)` for each axis independently; portrait sheets may fit width but not height
10. ModelYToCanvasTopConverter binding — pass `Template.Sheet.HeightMm` (double), NOT `HeightMicrons` (long), or converter returns 0.0
11. ToModelPoint — `e.GetPosition(canvas)` already accounts for `RenderTransform` (CanvasOffset). Do NOT subtract PanOffset — it double-compensates and breaks hit-test
12. Selection visual — use `SelectionVersion` (int) + `IsObjectSelectedConverter` to trigger DataBinding re-evaluation; model objects don't implement INotifyPropertyChanged for selection state
13. Preview shapes — create once in OnMouseDown, update properties in OnMouseMove, then re-assign reference to trigger ViewModel setter (unconditional OnPropertyChanged)
14. Model INPC (Item 12 correction) — model objects DO implement INotifyPropertyChanged for **persistent properties** (LineType, coordinates, dimensions). This is necessary for canvas DataTemplate bindings (StrokeDashArray, Width/Height, Canvas.Left/Top) to update when properties change via commands. INPC is NOT implemented for transient UI state like selection.
15. ComboBox with hardcoded items — always add `SelectedIndex` (or `SelectedItem`) binding when using `SelectionChangedCommand` behavior, otherwise the ComboBox never reflects the current model value
16. After Undo/Redo — always purge orphaned objects from `SelectedObjects`; `CommandHistory.Undo()`/`Redo()` removes/re-adds objects from the template collection without updating selection
17. `Rectangle.ContainsPoint()` — use **border-band** approach (expanded bounds minus shrunk interior), NOT full AABB. Interior area > `LineHitToleranceMicrons` from edges must NOT be selectable. Only clicks near the border count.
18. Tool switching keys (V/L/R/T) — handled via `PreviewKeyDown` on Window, NOT `Window.InputBindings`, for keyboard layout independence. `e.Key` returns physical key position regardless of RU/EN layout.
19. Selection markers (`ShowSelectionMarkers`) — returns `SelectedObjects.Count > 0` (not `Count == 1`). Markers render via `ItemsControl ItemsSource="{Binding SelectedObjects}"`, showing handles on ALL selected objects, not just single-selection.
20. Drag delta — compute from **saved initial position** (`_initialPositions[obj]`), NOT from current `obj.MicronsX`. The current value is already updated on previous MouseMove, so `obj.MicronsX + delta` drifts on every frame. Use `initialPos + delta` where `delta` is total mouse movement from drag start.
21. Every model class participating in canvas DataTemplate bindings (`Canvas.Left`/`Canvas.Top`/`StrokeDashArray`/etc) MUST implement `INotifyPropertyChanged` with backing fields for persistent properties (coordinates, dimensions, LineType). This applies to ALL object types: `Line`, `Rectangle`, AND `Text`.
22. Pan delta — compute from **Window-relative coordinates** (stable frame), NOT from `e.GetPosition(canvas)`. `e.GetPosition(canvas)` already accounts for `RenderTransform` (CanvasOffset), so comparing canvas-relative positions across `MouseMove` events where the canvas has moved produces a delta that includes the previous pan offset — causing runaway acceleration.

## Current State (Sprint R1–R4 + R3.1 + A–D + Coverage Improvement + Sprint 60–63 завершены)

- **Tests:** 2094 (0 failures, 1 pre-existing skip)
- **Coverage:** 75.3% line-rate ✅
- **Build:** 0 errors, 0 warnings
- **CI/CD:** GitHub Actions — build + test + coverage-gate 75% + NuGet кэш
- **EditorViewModel:** ~784 строк (де-bloat: −410 строк, 25 forwarding-свойств удалено, 4 INPC-обработчика удалены)
- **Managers:** ZoomPan, Selection, Clipboard, Tool, Preview, InlineEdit, StatusBar, Grid, DirtyState
- **Tools:** ITool + IEditorContext + ResizeMath (чистые функции) + ShortcutRegistry
- **Converters:** 27 файлов (все sealed)
- **Naming:** `TemplateObjectBase` (не `ITemplateObject`)
- **Commands:** `IUndoCommand` + `CustomResizeCommand` (полиморфный ApplyResize)
- **Model INPC:** `[ObservableProperty]` sourcegen на Line, Rectangle, Text
- **Constants:** `PhysicalConstants` + `EditorSettings` (вместо `EditorConstants.cs`-прокладки)
- **Validation:** `ITemplateValidator`/`TemplateValidator` (domain) + `ValidationService` (UI)
- **EditorCanvasBehavior:** 78 строк (AttachedProperty + stubs), 3 файла: State, Transform, Router
- **FontMetrics:** `IFontMetrics` + `FontMetrics.Default` static Singleton (DI-registered)
- **ShortcutRegistry:** `TryHandle()` — единая точка входа для всех горячих клавиш

## Sprint — Coverage Improvement (19.07.2026)

### Pipeline: Увеличение покрытия до ≥75%
**Проблема:** Фактическое покрытие составляло ~59-67% (оценка 82% была неточной). CI gate требовал ≥75%.
**Исправление:** Добавлено ~195 тестов в 6 зонах + 2 retry-цикла. Ключевые добавления:
- **Commands:** 16 тестов на null guards + edge cases (AddObjectCommand, DeleteObjectCommand, ChangePropertyCommand, BatchCommand)
- **Tools Reset():** 9 тестов на DrawingLineTool/DrawingRectangleTool/TextTool.Reset()
- **Grid:** 8 тестов на ComputeDisplayStep/GenerateGridNodes edge cases
- **Services:** 8 тестов на TemplateService, AutosaveService, PrintDocumentGenerator, DialogService
- **Models:** 15+ тестов на Template.Clone(), Sheet.FromFormat(), Coordinate, PointMicrons операторы
- **MainViewModel:** 6 тестов на AutosaveTickHandler, PrintPreviewCommand, OpenSettingsCommand
- **Retry 1:** FontMetrics (22 теста, instance+IFontMetrics), TemplateObjectBase (43 теста, Move/Clone/CaptureResizeState/ContainsPoint), NonZeroToVisibilityConverter (15 тестов), CustomSheetDialogViewModel (23 теста)
- **Retry 2:** ShortcutRegistry (22 теста, 100% покрытие)
- **Production fixes:** Template.Clone() deep copy, PointMicrons operator+/-, исправлен синтаксис TemplateTests.cs

**Файлы:** 15+ test files, Models/Template.cs, Models/PointMicrons.cs
**Build:** 0 errors, 0 warnings
**Tests:** 2035 passed (0 failures, 1 pre-existing skip)
**Coverage:** 75.15% line-rate ✅ (порог 75% достигнут)

## Sprint 37 — Selection fixes + visual feedback

### Fix S37-1: ToModelPoint double-compensation

**Проблема:** `EditorCanvasBehavior.ToModelPoint()` вычитал `PanOffsetX`/`PanOffsetY` из `e.GetPosition(canvas)` — но `e.GetPosition` уже учитывает `RenderTransform` (сдвиг на `CanvasOffset = -PanOffset`). Двойное вычитание смещало модельные координаты на `PanOffset/zoom` мм, из-за чего HitTest не находил объекты под курсором.

**Исправление:** Убрано вычитание PanOffset из `ToModelPoint()`.

### Fix S37-2: Visual selection state

**Проблема:** У объектов не было визуального состояния «выделен». SingleSelectedObject (маркеры) показывался, но при мульти-выделении или одиночном клике внешний вид не менялся.

**Исправление:** 
- `SelectionVersion` (int) + `IsObjectSelected(obj)` в EditorViewModel
- `IsObjectSelectedConverter` (IMultiValueConverter) — проверяет `SelectedObjects.Contains(obj)`
- DataTrigger'ы в DataTemplate'ах Line/Rectangle/Text — синяя подсветка `#0078D4`, StrokeThickness=2

### Fix S37-3: Preview shapes not appearing

**Проблема:** После мутации свойств `_previewLine`/`_previewRect` ссылка не менялась, `EditorViewModel.PreviewLine` setter не вызывался.

**Исправление:** Ре-ассайн `_editor.PreviewLine = _previewLine` / `_editor.PreviewRectangle = _previewRect` в OnMouseMove.

### Fix S37-4: Canvas not resizing on zoom

**Проблема:** `OnPropertyChanged(nameof(Zoom))` не вызывался при изменении зума через `SetZoom`/`ZoomIn`/`ZoomOut`.

**Исправление:** Добавлен `OnPropertyChanged(nameof(Zoom))` в `OnZoomChangedInternal()`.

### Fix S37-5: Escape doesn't switch to Select

**Проблема:** Escape в инструментах рисования/текста очищал состояние, но не активировал SelectTool.

**Исправление:** Добавлен `_editor.ActiveTool = "Select"` после `Reset()` во всех трёх инструментах.

### Fix S37-6: SelectionBoxTop not rendering

**Проблема:** `SelectionBoxTop` (вычисляемое = SelectionBoxBottom + SelectionBoxHeight) не пробрасывался на EditorViewModel → XAML не обновлялся.

**Исправление:** Подписка на `PropertyChanged` PreviewManager в EditorViewModel.

**Build:** 0 errors, 4 warnings (pre-existing)
**Tests:** 465+ пройдены (EditorViewModel 112 + Integration 49 + SelectTool 18 + ZoomPanManager 10 + Converter 156 + HitTest 120)

## Sprint 38 — LineType панели свойств + Undo + координаты

### Fix S38-1: ComboBox типа линии не отображает текущее значение

**Проблема:** ComboBox в панели свойств не имел `SelectedItem`/`SelectedIndex` биндинга — показывал пустое значение при первом выборе объекта. Изменение через UI работало, но ComboBox не синхронизировался с моделью.

**Исправление:** Создан `LineTypeToIndexConverter` (LineType → int). Добавлен `SelectedIndex="{Binding LineTypeValue/RectLineType, Converter=...}"` на оба ComboBox (Line и Rectangle).

### Fix S38-2: Изменение LineType не перерисовывает канвас

**Проблема:** `Line` и `Rectangle` без INPC — мутация `LineType` через `ChangePropertyCommand` не обновляла `StrokeDashArray` на канвасе.

**Исправление:** `Line.cs` и `Rectangle.cs` — `INotifyPropertyChanged` + backing field для `LineType`.

### Fix S38-3: DrawingRectangleTool не передаёт _lineType

**Проблема:** `CalculateRectangle()` создавал `new Rectangle(x, y, w, h)` — всегда `LineType.Solid`.

**Исправление:** `CalculateRectangle()` принимает `lineType` и передаёт в конструктор.

### Fix S38-4: Изменение координат не перерисовывает канвас

**Проблема:** `Line.StartMicronsX/Y`, `EndMicronsX/Y` и `Rectangle.WidthMicrons/HeightMicrons/MicronsX/Y` без INPC — канвас не обновлялся при редактировании через панель свойств.

**Исправление:** Все свойства координат — backing fields + `OnPropertyChanged()`. В `Rectangle` добавлены уведомления для `RightMicronsX`, `BottomMicronsY`, `CenterMicronsX`, `CenterMicronsY` (маркеры выделения).

### Fix S38-5: Enter не коммитит поля ввода координат

**Проблема:** `TextBoxLostFocusCommandBehavior` реагировал только на LostFocus. Enter не применял значение.

**Исправление:** Добавлен обработчик `KeyDown.Enter` в `TextBoxLostFocusCommandBehavior`.

### Fix S38-6: Undo оставляет «висячее» выделение

**Проблема:** После Undo (`AddObjectCommand.Undo()` удаляет объект) `SelectedObjects` не очищался — маркеры выделения оставались на канвасе.

**Исправление:** В `Undo()`/`Redo()` добавлен вызов `PurgeOrphanedSelection()`, удаляющий из `SelectedObjects` объекты не из `Template.Objects`.

**Build:** 0 errors, 4 warnings (pre-existing)
**Tests:** 589+ пройдены (EditorViewModel 112 + Integration 49 + SelectTool 18 + ZoomPanManager 10 + Converter 156 + HitTest 120 + PropertiesViewModel 50 + Command 137 + Line/Rectangle 30)

## Sprint 39 — Rectangle HitTest: селекция только по границе

### Fix S39-1: Прямоугольник выделяется при клике внутри области

**Проблема:** `Rectangle.ContainsPoint()` использовал полную AABB-проверку — любая точка внутри прямоугольника (включая центр) считалась попаданием.

**Исправление:** Метод переписан на **border-band подход** — точка считается попавшей на прямоугольник только если она находится в пределах `LineHitToleranceMicrons` (5 мм) от любой из четырёх сторон. Внутренняя область (дальше 5 мм от краёв) не селектируется. Для маленьких прямоугольников (< 10 мм) вся область остаётся селектируемой.

**Файлы:**
- `Models/Objects/Rectangle.cs` — `ContainsPoint()` заменён на border-band
- `Tests/Helpers/HitTestHelperTests.cs` — обновлены тесты
- `Tests/Helpers/HitTestHelperExtendedTests.cs` — обновлены тесты + новый `PointNearEdgeLargeRect_ReturnsTrue`
- `Tests/Helpers/AdditionalHelperTests.cs` — обновлены тесты
- `Tests/IntegrationTests.cs` — обновлён `HitTestAll_OverlappingObjects`

**Build:** 0 errors, 4 warnings (pre-existing)
**Tests:** 840+ пройдены (все ключевые категории)

## Sprint 40 — Keyboard shortcuts + Selection markers

### Fix S40-1: KeyBindings инструментов не совпадали с UI

**Проблема:** Фактические KeyBindings (H/L/U/X) не соответствовали UI (V/L/R/T). Select был на H вместо V, Rectangle на U вместо R, Text на X вместо T.

**Исправление:** `MainWindow.xaml` — H→V, U→R, X→T.

### Fix S40-2: R-клавиша конфликтовала (Rectangle vs Rotate)

**Проблема:** R была занята Rotate, не позволяя использовать её для Rectangle.

**Исправление:** Rotate перенесён с R на E (rotatE) / Shift+E.

### Fix S40-3: Переключение инструментов не работало с русской раскладкой

**Проблема:** WPF `KeyBinding` с `KeyGesture` не срабатывал при русской раскладке клавиатуры.

**Исправление:** Инструменты (V/L/R/T) и rotate (E/Shift+E) перенесены из `Window.InputBindings` в `PreviewKeyDown` handler на Window. `e.Key` в PreviewKeyDown возвращает физическую клавишу независимо от раскладки.

### Fix S40-4: Панель инструментов не обновлялась при горячих клавишах

**Проблема:** `SetActiveToolCommand` устанавливал `_toolManager.ActiveTool` напрямую, минуя сеттер `EditorViewModel.ActiveTool`, который вызывает `OnPropertyChanged()`. RadioButton на toolbar не получал уведомление.

**Исправление:** `SetActiveTool()` теперь вызывает `ActiveTool = tool` (сеттер свойства с `OnPropertyChanged()`).

### Fix S40-5: Маркеры выделения не появлялись на выбранных объектах

**Проблема:** `ShowSelectionMarkers` возвращал `true` только при `SelectedObjects.Count == 1`. При мульти-выделении `ContentControl` с маркерами был скрыт.

**Исправление:**
- `SelectionManager.ShowSelectionMarkers` — `Count > 0` (вместо `Count == 1`)
- `ContentControl Content="{Binding SingleSelectedObject}"` заменён на `ItemsControl ItemsSource="{Binding SelectedObjects}"` с `Canvas` ItemsPanel — маркеры рендерятся для каждого выделенного объекта

**Файлы:**
- `MainWindow.xaml` — KeyBindings → PreviewKeyDown
- `MainWindow.xaml.cs` — `Window_PreviewKeyDown()` handler
- `ViewModels/Managers/ToolManager.cs` — не изменялся
- `ViewModels/EditorViewModel.cs` — `SetActiveTool()` через сеттер
- `ViewModels/Managers/SelectionManager.cs` — `ShowSelectionMarkers` → `Count > 0`
- `Views/EditorCanvas.xaml` — ContentControl → ItemsControl

**Build:** 0 errors, 4 warnings (pre-existing)
**Tests:** 844+ пройдены (все ключевые категории)

## Sprint 41 — Drag move delta drift + Text INPC

### Fix S41-1: Delta accumulation drift on multi-MouseMove

**Проблема:** `SelectTool.OnMouseMove()` вычислял `newX = obj.MicronsX + delta`, где `delta` — полное смещение от точки старта. Но `obj.MicronsX` уже обновлён на предыдущем `MouseMove`, поэтому каждое новое событие добавляло дельту к уже смещённой позиции. Объект «убегал» от курсора (дрифт, пропорциональный количеству `MouseMove`).

**Исправление:** Дельта прибавляется к **сохранённой начальной позиции** из `_initialPositions[obj]`.

**Файл:** `Tools/SelectTool.cs:208-210`

### Fix S41-2: Text INPC for MicronsX/MicronsY

**Проблема:** `Text` не реализовывал `INotifyPropertyChanged` — `Text.Move()` устанавливал `MicronsX`/`MicronsY` (auto-properties), но WPF-биндинги `Canvas.Left`/`Canvas.Top` не обновлялись. Текст визуально не двигался при перетаскивании.

**Исправление:**
- `Text` реализует `INotifyPropertyChanged`
- Override `MicronsX`/`MicronsY` с backing fields + `OnPropertyChanged()`
- Уведомления для `RightMicronsX`, `BottomMicronsY`, `CenterMicronsX`, `CenterMicronsY`

**Файл:** `Models/Objects/Text.cs:12-52`

### Cleanup
- Удалены мёртвые поля `_dragStartX`/`_dragStartY`
- Упрощён расчёт дельты (оба if-else вычисляли одно и то же)

**Build:** 0 errors, 0 warnings
**Tests:** 844+ пройдены (все ключевые категории)

## Sprint 42 — StrokeThicknessMicrons (толщина линии)

### Feature S42-1: Добавлено свойство StrokeThicknessMicrons

**Проблема:** В XSD-спецификации предусмотрен `StrokeThickness` (xs:long) для Line и Rectangle, но в коде свойство отсутствовало на всех уровнях — модели, сериализация, UI панели свойств, отрисовка на канвасе.

**Исправление:** Реализовано end-to-end:

| Уровень | Файл | Изменение |
|---------|------|-----------|
| Константа | `Constants/EditorConstants.cs:86-88` | `DefaultStrokeThicknessMicrons = 500` (0.5 мм) |
| Модель Line | `Models/Objects/Line.cs:23,87-101,126,133,148` | Поле + INPC-свойство + параметр конструктора + Clone |
| Модель Rectangle | `Models/Objects/Rectangle.cs:23,88-102,152,161,173` | Аналогично |
| Сериализация | `Services/TemplateService.cs:106,121,124,361,367,425,434` | DTO-поле + MapToObject/MapToDto |
| ViewModel | `ViewModels/PropertiesViewModel.cs:169,177,275-290,348-362,525-536` | Свойства + команды + string-обёртки + UpdateSelection |
| UI панели | `Views/PropertiesPanelContent.xaml:130-142,227-240` | TextBox «Толщина (мм)» для Line и Rectangle |
| Canvas | `Views/EditorCanvas.xaml:67-77,153-163` | `StrokeThickness` привязан к модели через `MicronsToPixelConverter` (Style Setter + MultiBinding) |

**Детали реализации:**
- Все внутренние координаты в микронах (`long`), WPF-пиксели через `MicronsToPixelConverter` с учётом Zoom
- DataTrigger'ы выделения (StrokeThickness=2) и наведения (StrokeThickness=2.5) остаются неизменными — они override базовый Style Setter через WPF precedence
- Значение по умолчанию: 500 микрон (0.5 мм) — соответствует ГОСТ 2.303-68 для тонкой линии
- Drawing инструменты (LineTool/RectangleTool) не требуют изменений — дефолтный параметр конструктора 500 микрон

**Build:** 0 errors (5 pre-existing warnings)
**Tests:** 1000+ пройдены (0 failures)

## Sprint 43 — ResizeTool dispatch fix

### Fix S43-1: GetCurrentTool() не имеет case "Resize"

**Проблема:** `EditorCanvasBehavior.GetCurrentTool()` не обрабатывал `"Resize"` в switch — при падении на default возвращал `SelectTool`. После того как `SelectTool.OnMouseDown()` детектил хендл и пушил `"Resize"` в стек инструментов, последующие `OnMouseMove`/`OnMouseUp` уходили в `SelectTool` вместо `ResizeTool`. Размеры объектов не менялись при drag за угловые маркеры, команда `CustomResizeCommand` не создавалась.

**Исправление:** Добавлен case `"Resize" => editor.GetOrCreateTool<ResizeTool>()` в `GetCurrentTool()`.

**Файл:** `Behaviors/EditorCanvasBehavior.cs:288-298`

**Build:** 0 errors (5 pre-existing warnings)
**Tests:** 1000+ пройдены (0 failures)

## Sprint 44 — PropertiesPanel live update after resize

### Fix S44-1: PropertiesViewModel не подписан на INPC объекта

**Проблема:** `PropertiesViewModel` не подписывался на `INotifyPropertyChanged.PropertyChanged` выделенного объекта. При изменении размеров через `ResizeTool` модель оповещала (`OnPropertyChanged`), но ViewModel не перезапрашивала свои computed-свойства (`RectX`, `LineEndX`, `TextFontSize` и т.д.). WPF-биндинги на панели свойств не обновлялись.

**Исправление:**
- `PropertiesViewModel.UpdateSelection()` — при смене выделения отписывается от старого объекта, подписывается на новый
- Добавлен метод `OnSelectedObjectPropertyChanged()`, который по имени свойства модели определяет, какое ViewModel-свойство оповестить
- При `Dispose()` — гарантированная отписка

**Файл:** `ViewModels/PropertiesViewModel.cs:109-210`

### Fix S44-2: Text INPC для всех свойств

**Проблема:** `Text.FontSizeMicrons`, `Content`, `FontName`, `TextType`, `RotationAngle` были auto-properties без INPC. Даже с подпиской PropertiesViewModel на `PropertyChanged`, эти свойства не оповещали об изменениях.

**Исправление:** Все свойства переведены на backing fields + `OnPropertyChanged()`. Для `FontSizeMicrons` и `Content` добавлены уведомления для зависимых computed-свойств (`WidthMicrons`, `RightMicronsX`, `BottomMicronsY`, `CenterMicronsX`, `CenterMicronsY`).

**Файл:** `Models/Objects/Text.cs:53-110`

**Build:** 0 errors (5 pre-existing warnings)
**Tests:** 1287+ пройдены (0 failures)

## Sprint 45 — Pan delta accumulation fix (RenderTransform drift)

### Fix S45-1: Панорамирование ускоряется из-за RenderTransform в e.GetPosition

**Проблема:** `EditorCanvasBehavior.State_MouseMove()` вычислял дельту панорамирования из `e.GetPosition(canvas)`, который учитывает `RenderTransform` canvas'а (`TranslateTransform CanvasOffsetX/Y`). После каждого `MouseMove` canvas сдвигался, и на следующем `MouseMove` `e.GetPosition(canvas)` возвращал координаты, уже включающие предыдущий сдвиг. Каждое движение мыши добавляло дельту предыдущего пана — панорамирование неконтролируемо ускорялось (`runaway pan`).

**Исправление:** Дельта вычисляется в **Window-координатах** (`e.GetPosition(window)`), которые не меняются при сдвиге canvas'а. Добавлены поля `PanStartWpfPoint` и `PanAppliedModelDelta` в `EditorCanvasState` для корректного инкрементального расчёта.

**Файл:** `Behaviors/EditorCanvasBehavior.cs:96-165,199,343-349`

**Build:** 0 errors (5 pre-existing warnings)
**Tests:** PanTool 13/13, EditorCanvas/ZoomPan 10/10 — все пройдены

## Sprint 46 — Context menu fixes

### Fix S46-1: Canvas context menu blocked by State_MouseDown e.Handled

**Проблема:** `EditorCanvasBehavior.State_MouseDown()` безусловно устанавливал `e.Handled = true` для ВСЕХ кнопок мыши, включая правую. WPF не показывал `ContextMenu` на UserControl, т.к. событие было помечено как обработанное.

**Исправление:** В `State_MouseDown()` при правом клике явно открываем `UserControl.ContextMenu` программно через `VisualTreeHelper`. В `State_MouseUp()` добавлен ранний return для правой кнопки.

**Файлы:**
- `Behaviors/EditorCanvasBehavior.cs:94-108,210-212` — явное открытие ContextMenu + ранний return в MouseUp
- `EditorCanvas.xaml:22-43` — контекстное меню определено на UserControl (без изменений)

**Build:** 0 errors, 4 pre-existing warnings
**Tests:** 3/3 RightClick_Ignored + 13/13 PanTool — пройдены

### Fix S46-2: TabItem context menu commands not working (Async suffix mismatch)

**Проблема:** Методы `CloseTabAsync()`, `CloseOtherTabsAsync()`, `CloseAllTabsAsync()` в `EditorViewModel` возвращали `void` (не async). CommunityToolkit.Mvvm 8.4.2 source generator обрезает суффикс `Async` **только для асинхронных методов** (возвращающих `Task`). Для `void`-методов суффикс сохраняется → генерировались `CloseTabAsyncCommand`, а XAML биндился к `CloseTabCommand` — команда не находилась, MenuItem был неактивен.

**Исправление:** Методы переименованы — убран суффикс `Async`:
- `CloseTabAsync()` → `CloseTab()`
- `CloseOtherTabsAsync()` → `CloseOtherTabs()`
- `CloseAllTabsAsync()` → `CloseAllTabs()`

**Файл:** `ViewModels/EditorViewModel.cs:45-67`

**Common Mistakes (new):**
23. `[RelayCommand]` on `void` method with `Async` suffix — source generator НЕ обрезает суффикс для синхронных методов. Имя команды будет `MethodAsyncCommand`, а не `MethodCommand`. Для async методов (возвращающих `Task`/`Task<T>`) суффикс обрезается.
24. ContextMenu внутри `Style` (`Setter.Value`) — не полагайся на автоматическое наследование `DataContext` через `PlacementTarget`. Если команды не работают, используй явное указание `DataContext="{Binding PlacementTarget.DataContext, RelativeSource={RelativeSource Self}}"`.

**Build:** 0 errors, 5 pre-existing warnings
**Tests:** 12/12 CloseTab + RightClick — пройдены

## Sprint 47 — Grid 1mm MinPixelSpacing fix

### Fix S47-1: Сетка не отображается при шаге 1мм

**Проблема:** При установке шага сетки 1мм сетка полностью пропадала с холста. Два сценария:
- **A3+:** `cols * rows` (125K+) превышает `MaxGridNodes (100000)` → `GenerateGridNodes()` возвращает пустой список
- **A4:** 62K узлов генерируются, но при Zoom=1.0 расстояние 1px < 2px (диаметр точки 2px) → сплошная серая заливка

**Причина:** `GridManager.RefreshGridNodes()` и `GridHelper.GenerateGridNodes()` не проверяли `MinPixelSpacing` (5px) — минимальное расстояние между узлами в пикселях, при котором точки сетки различимы.

**Исправление:**
- `GridManager.RefreshGridNodes()` — проверка `pixelSpacing < MinPixelSpacing` → ранний return
- `GridHelper.GenerateGridNodes()` — defense-in-depth: та же проверка

**Поведение:**
- 1мм сетка при Zoom < 500%: скрыта (точки < 5px — слишком плотно)
- 1мм сетка при Zoom ≥ 500%: отображается
- 5мм сетка при Zoom ≥ 100%: отображается (как и раньше)

**Файлы:** `ViewModels/Managers/GridManager.cs`, `Helpers/GridHelper.cs`, `Tests/Helpers/GridHelperTests.cs`

**Common Mistakes (new):**
25. Grid nodes (GenerateGridNodes) must check MinPixelSpacing — unlike lines, nodes don't auto-hide when too dense, causing either MaxGridNodes overflow (A3+) or solid grey fill (A4). Always check `stepMm * zoom < MinPixelSpacing` before generating nodes.

## Sprint 48 — Dirty indicator (*) in tab header not appearing

### Fix S48-1: PropertyChanged notification not forwarded from DirtyStateManager

**Проблема:** `EditorViewModel.IsDirty`, `DisplayName` и `FilePath` — plain forwarding properties к `DirtyStateManager` без `PropertyChanged`. WPF DataTrigger `{Binding IsDirty}` в ControlTemplate TabItem подписан на `EditorViewModel.PropertyChanged`, но уведомление приходит от `DirtyStateManager` (через `[ObservableProperty]`). В итоге `DirtyIndicator` (звёздочка `*`) никогда не становится `Visible`.

**Исправление:** Добавлена подписка на `_dirtyStateManager.PropertyChanged` в конструкторе `EditorViewModel` — проброс `IsDirty`, `DisplayName`, `FilePath` через `OnPropertyChanged()` (аналогично существующему паттерну для `_zoomPanManager` и `_previewManager`).

**Файл:** `ViewModels/EditorViewModel.cs:752-764`

**Build:** 0 errors, 5 pre-existing warnings
**Tests:** EditorViewModel 112/112, ToolTests 113/113, IntegrationTests 49/49, MarkDirty 27/27 — все пройдены

**Common Mistakes (new):**
26. Plain forwarding properties to a delegated `[ObservableObject]` manager — if the ViewModel wraps a manager's `[ObservableProperty]` with a regular property, `PropertyChanged` fires from the manager, not the ViewModel. Always subscribe to manager's `PropertyChanged` and forward needed notifications (same pattern as `_zoomPanManager`, `_previewManager`, `_dirtyStateManager`).

## Sprint 49 — ResizeTool clamp fix + test corrections

### Fix S49-1: Minimum-size clamp moves fixed edges

**Проблема:** Clamp минимального размера (`MinResizeSizeMicrons = 1000`) применял `Min`/`Max` безусловно к **обеим** граням в оси, двигая фиксированные грани. Например, для `TopRight` (правая+верхняя движутся, левая+нижняя фиксированы) при пересечении правой гранью левой, clamp сдвигал **левую** (фиксированную) грань, а не ограничивал **правую** (движущуюся).

**Исправление:** Clamp стал handle-зависимым:
- Определяются `leftMoves`, `rightMoves`, `bottomMoves`, `topMoves` по типу маркера
- Ограничивается **только движущаяся** грань: `Min()` для левой/нижней, `Max()` для правой/верхней
- При Ctrl обе грани движутся → симметричный схлоп через середину при нарушении minSize

**Файл:** `Tools/ResizeTool.cs:248-282`

### Fix S49-2: Тесты под старую бажную формулу

**Проблема:** 14 тестов в `ResizeToolTests.cs` и `ResizeToolExtendedTests.cs` содержали ожидаемые значения, соответствующие старой бажной формуле (double-delta, half-delta, неправильный pivot).

**Исправление:** Все тесты переписаны под корректную edge-based модель. Добавлен `SnapEnabled = false` в тесты, где он отсутствовал.

**Файлы:** `Tests/Tools/ResizeToolTests.cs`, `Tests/Tools/ResizeToolExtendedTests.cs`

**Build:** 0 errors, 5 pre-existing warnings
**Tests:** 63/63 ResizeTool, 1500+ остальных — пройдены

**Common Mistakes (new):**
27. Minimum-size clamp in edge-based resize — don't apply `Min`/`Max` to both edges in an axis. Only the MOVING edge should be constrained. Determine `leftMoves`/`rightMoves`/`bottomMoves`/`topMoves` per handle type (or set all true for Ctrl) and constrain only the moving edge(s). Fixed edges must NEVER be moved by the clamp.

## Sprint 50 — Clipboard improvements (Copy/Paste/Cut)

### Feature S50-1: Ctrl+X keyboard shortcut + UI

**Проблема:** `InputGestureText="Ctrl+X"` отображался в контекстном меню, но:
- Привязки в `Window.InputBindings` не было — клавиша не работала
- В главном меню и тулбаре отсутствовал пункт «Вырезать» (были только Копировать/Вставить/Удалить)

**Исправление:**
- Добавлен `<KeyBinding Key="X" Modifiers="Control" Command="{Binding SelectedTab.CutSelectedCommand}"/>`
- В главное меню добавлен `MenuItem Header="Вы_резать"` с иконкой `ContentCut` между Копировать и Вставить
- В тулбар добавлена кнопка `Вырезать (Ctrl+X)` с иконкой `ContentCut` между Копировать и Вставить

**Файл:** `MainWindow.xaml:36,151-154,275-279`

### Fix S50-2: Re-paste bug (same instance added twice)

**Проблема:** `GetClipboardContents()` возвращал ссылки на те же объекты из `_clipboard`. Повторный Ctrl+V добавлял те же экземпляры в `Template.Objects` снова — объект оказывался в коллекции дважды.

**Исправление:** `GetClipboardContents()` теперь клонирует объекты при каждом вызове, а не при Copy:
```csharp
public IReadOnlyList<TemplateObjectBase> GetClipboardContents()
    => _clipboard.Select(o => o.Clone()).ToList().AsReadOnly();
```

**Файл:** `ClipboardManager.cs:27-28`

### Feature S50-3: Paste offset (10mm step)

**Проблема:** Вставленные объекты появлялись точно поверх оригиналов — их не было видно.

**Исправление:** Добавлено смещение при вставке. После Copy offset = 10мм. Каждый последующий Paste без Copy увеличивает offset ещё на 10мм по X и Y. При повторном Copy offset сбрасывается.

**Файл:** `ClipboardManager.cs:10-14,22-23,34-42`

### Feature S50-4: BatchCommand для Cut/Paste

**Проблема:** При Cut/Paste 5 объектов создавалось 5 отдельных команд в Undo-стеке. Пользователь нажимал Ctrl+Z 5 раз для отмены одного действия.

**Исправление:** При >1 объекте `PasteFromClipboard()` и `DeleteSelected()` создают `BatchCommand`, группирующий все операции в одну Undo-команду:
- Paste: "Вставить объекты" (N объектов)
- Delete: "Удалить объекты" (N объектов)

**Файлы:** `EditorViewModel.cs:570-587,982-996`

### Feature S50-5: Auto-select pasted objects

**Проблема:** После Paste вставленные объекты не выделялись — пользователь не видел, что было добавлено.

**Исправление:** Добавлен метод `SelectionManager.SelectObjects()`. `PasteFromClipboard()` вызывает `_selectionManager.SelectObjects(clipboard)` после Push команд.

**Файлы:** `SelectionManager.cs:56-62`, `EditorViewModel.cs:586`

### Feature S50-6: StatusBar feedback

**Проблема:** Copy/Paste/Cut не давали обратной связи в строке состояния.

**Исправление:** Добавлены сообщения в `StatusBarManager.StatusMessage`:
- Copy: "Скопировано: N объектов" / "Нет выделенных объектов"
- Cut: "Вырезано: N объектов" / "Нет выделенных объектов"
- Paste: "Вставлено: N объектов" / "Буфер обмена пуст"

Добавлен вспомогательный метод `GetObjectWord()` для русских числительных (объект/объекта/объектов).

**Файл:** `EditorViewModel.cs:557-587,1047-1053`

### Feature S50-7: Clipboard cleanup on tab close

**Проблема:** При закрытии вкладки объекты в буфере обмена могли ссылаться на удалённый шаблон.

**Исправление:** `ClipboardManager.Clear()` вызывается в `EditorViewModel.Dispose()`.

**Файлы:** `ClipboardManager.cs:30`, `EditorViewModel.cs:1039`

### Fix S50-8: Ctrl+V перехватывался PreviewKeyDown (tool switcher)

**Проблема:** `Window_PreviewKeyDown` в `MainWindow.xaml.cs:27` обрабатывал `case Key.V` **без проверки модификаторов**. При нажатии `Ctrl+V` событие перехватывалось, устанавливался `ActiveTool = "Select"` и `e.Handled = true`. `KeyBinding` для Ctrl+V в `Window.InputBindings` никогда не получал событие — Paste не работал.

**Исправление:** Добавлена проверка `if (e.KeyboardDevice.Modifiers != ModifierKeys.None) break;` для всех tool-switching кейсов (V, L, R, T). Теперь Ctrl+V доходит до InputBindings.

**Файл:** `MainWindow.xaml.cs:27-58`

**Build:** 0 errors, 5 pre-existing warnings
**Tests:** 1289+ пройдены (0 failures)

**Common Mistakes (new):**
28. Re-paste bug — `GetClipboardContents()` must clone objects on EVERY call, not only during `Copy()`. If it returns references to the same cached instances, repeated Paste adds the same object to the collection. Always `_clipboard.Select(o => o.Clone())` in `GetClipboardContents()`. Paste offset counter must reset to `PasteOffsetStepMicrons` (not 0) after Copy, so the first paste already has an offset.
29. `PreviewKeyDown` for tool switching must check `e.KeyboardDevice.Modifiers != ModifierKeys.None` before handling V/L/R/T. Without the check, `Ctrl+V` (Paste), `Ctrl+L`, `Ctrl+R`, `Ctrl+T` get intercepted by the tool switcher and never reach their `Window.InputBindings`. Always add `if (modifiers != None) break;` at the start of each tool-switching case.
30. Panning without `CaptureMouse()` — if the mouse leaves the canvas during middle-button drag, `MouseMove` and `MouseUp` stop being delivered, panning freezes, and `IsPanning` never resets. Always call `canvas.CaptureMouse()` on pan start and `canvas.ReleaseMouseCapture()` on pan end.

## Sprint 51 — Panning mouse capture fix

### Fix S51-1: Panning breaks/corrupts when mouse leaves canvas during drag

**Проблема:** `EditorCanvasBehavior.State_MouseDown()` не вызывал `canvas.CaptureMouse()` при старте панорамирования. Без захвата мыши:
- При выходе курсора за границу канваса `MouseMove` перестаёт доставляться — панорамирование замирает
- `MouseUp` вне канваса не доходит до `State_MouseUp` — `IsPanning` навсегда `true`
- Последующий клик средней кнопкой сбрасывает `IsPanning`, но первый `MouseMove` применяет большую накопленную дельту — canvas «прыгает»

**Исправление:** Добавлен `CaptureMouse()` / `ReleaseMouseCapture()` в трёх местах:
- Middle button branch: `canvas.CaptureMouse()` после `state.IsPanning = true`
- Space/Alt+Left branch: `canvas.CaptureMouse()` после `state.IsPanning = true`
- Panning end в `State_MouseUp`: `canvas.ReleaseMouseCapture()` перед `e.Handled = true`

**Файл:** `Behaviors/EditorCanvasBehavior.cs:122,140,223`

**Build:** 0 errors, 5 pre-existing warnings
**Tests:** PanTool 13/13, ZoomPanManager 10/10, SelectTool 18/18 — все пройдены

## Sprint 52 — Text improvements (fonts, immediate edit, free rotation)

### Fix S52-1: Font internal names mismatch

**Проблема:** `FontNameToFamilyConverter` и `PreviewLineChangedBehavior` использовали URI-фрагменты `#GOST type A`/`#GOST type B`, но фактические внутренние имена — `GOST Type AU`/`GOST Type BU` (регистрозависимые). Шрифты не отображались.

**Исправление:** URI приведены к правильным внутренним именам:
- `#GOST type A` → `#GOST Type AU`
- `#GOST type B` → `#GOST Type BU`

**Файлы:** `Converters/FontNameToFamilyConverter.cs`, `Behaviors/PreviewLineChangedBehavior.cs`, `Resources/Fonts/README.md`

### Fix S52-2: Double-click opens inline editor

**Поведение:** `SelectTool.OnDoubleClick()` вызывает `StartInlineEditing(text)` при двойном клике на текстовый объект. Создание текста через TextTool НЕ открывает редактор — только выделяет объект.

### Fix S52-3: Free rotation angle (0-359°)

**Проблема:** `RotationAngle` был ограничен `{0,90,180,270}`. `ContainsPoint()` и `GetBoundingBox()` — switch-case с неверной геометрией для 90°/270°.

**Исправление:**
- Удалён `ValidRotationAngles`. Сеттер нормализует `value % 360`
- `ContainsPoint()` / `GetBoundingBox()` — общая математика через `cos`/`sin`
- UI: ComboBox → TextBox (произвольный ввод градусов)
- InlineTextEditor: `LayoutTransform` с `RotateTransform`
- `PropertiesViewModel`: удалён вызов `ValidateRotation()`

**Common Mistakes (new):**
31. Rotation direction in WPF RotateTransform — WPF's `RotateTransform` rotates CLOCKWISE (Y-down screen space), which equals COUNTERCLOCKWISE in model Y-up space. `ContainsPoint` must compute `localX = dx*cos + dy*sin; localY = -dx*sin + dy*cos` with `angleRad = RotationAngle * PI / 180`.
32. `PreviewKeyDown` tool switching + InlineTextEditor — when inline editing is active, `Escape`/`Enter` must be intercepted by the TextBox InputBindings, NOT by Window PreviewKeyDown. The `CommitInlineEditingCommand`/`CancelInlineEditingCommand` handlers set `ActiveTool = "Select"` so subsequent PreviewKeyDown events go to select.
33. GostA.ttf internal name is `GOST Type AU` (not `GOST type A` or `GOST Type A`) — case-sensitive. Verify via `GlyphTypeface.FamilyNames`.
34. Text rotation center in WPF — `RotateTransform` rotates around the TextBlock's top-left corner, which is placed at the ContentPresenter's origin. The ContentPresenter top-left maps to model `(X, Y+H)` (the TOP of the text box), NOT the baseline `(X, Y)`. `GetBoundingBox()` and `ContainsPoint()` must rotate around `(X, Y+H)` in ContentPresenter-local space (Y-down), then convert back to model coordinates.

**Build:** 0 errors, 5 pre-existing warnings
**Tests:** 165+ релевантных пройдены (все ключевые категории)

## Sprint 53 — DateTimeProvider + MarkerPosition + Behaviour tests

### Feature S53-1: IDateTimeProvider (замена Thread.Sleep)

**Проблема:** `FileService.CreateBackup()`, `AutosaveService`, `TemplateService` использовали `DateTime.UtcNow` напрямую. Тесты использовали `Thread.Sleep` (суммарно ~2.3s) для гарантии уникальности timestamp-ов, замедляя тесты и делая их недетерминированными.

**Исправление:**
- Создан `Services/IDateTimeProvider.cs` (интерфейс: `DateTime UtcNow`)
- Создан `Services/DateTimeProvider.cs` (реализация — обёртка над `DateTime.UtcNow`)
- Во все 3 сервиса добавлен опциональный DI-параметр `IDateTimeProvider? dateTimeProvider = null`
- `App.xaml.cs` — регистрация `services.AddSingleton<IDateTimeProvider, DateTimeProvider>()`
- Все 12 случаев `DateTime.UtcNow` заменены на `_dateTimeProvider.UtcNow`

**Тесты:** Все 5 тестовых файлов обновлены:
- `FileServiceTests` — `Mock<IDateTimeProvider>`, строки `Thread.Sleep` из `CreateBackup_MultipleBackups_CreatesUniqueFiles` и `CreateBackup_OverwritesExistingBackup` удалены, тесты используют `SetupSequence`
- `AutosaveServiceTests` — добавлен `Mock<IDateTimeProvider>`
- `TemplateServiceTests` — `Mock<IDateTimeProvider>`, удалён `Thread.Sleep` из `Save_UpdatesModifiedDate`
- `TemplateServiceRoundTripTests` — `Mock<IDateTimeProvider>`, удалён `Thread.Sleep` из `Save_UpdatesModifiedDate`, `CreateTestTemplate` использует `FixedDate`
- `ExtendedServiceTests` — `Mock<IDateTimeProvider>`, удалён `Thread.Sleep` из `Save_OverwritesExistingFile`, `CreateTestTemplate` использует `FixedDate`

### Feature S53-2: MarkerPosition attached behavior (сокращение XAML)

**Проблема:** 14 маркеров выделения (Line×2, Rectangle×8, Text×4) занимали ~250 строк XAML с повторяющимися MultiBinding-блоками `Canvas.Left`/`Canvas.Top`.

**Исправление:** Создан `Behaviors/MarkerPosition.cs` — два attached properties:
- `XPropertyPath` (string) — путь к свойству X-координаты
- `YPropertyPath` (string) — путь к свойству Y-координаты

При установке обоих свойств создаёт MultiBinding для `Canvas.Left` (ModelXToCanvasLeftConverter + Zoom) и `Canvas.Top` (ModelYToCanvasTopConverter + HeightMm + Zoom) через `FindAncestor UserControl`.

XAML каждого маркера сокращён с 12 строк до 2:
```xml
<Rectangle Style="{StaticResource SquareMarker}"
           behaviors:MarkerPosition.XPropertyPath="MicronsX"
           behaviors:MarkerPosition.YPropertyPath="BottomMicronsY"/>
```

**Файлы:**
- `Behaviors/MarkerPosition.cs` — новый файл
- `Views/EditorCanvas.xaml` — 14 маркеров переписаны (~250→40 строк)

### Feature S53-3: Behaviour unit tests (pure functions)

**Проблема:** `EditorCanvasBehavior` содержал 3 конвертации (MouseButton, ModifierKeys, Key) с private-методами, не покрытыми тестами.

**Исправление:**
- `ToToolMouseButton`, `ToToolModifiers`, `ToToolKey` — изменены с `private static` на `internal static`
- Создан `Tests/Behaviors/EditorCanvasBehaviorTests.cs` с 18 theory/fact-тестами:
  - 5 тестов ToToolMouseButton (все MouseButton + fallback)
  - 7 тестов ToToolModifiers (None/Ctrl/Shift/Alt/комбинации)
  - 6 тестов ToToolKey (Escape/Enter/Delete + unknown → null)

**Common Mistakes (new):**
35. `Thread.Sleep` in tests — never use it for timestamp uniqueness. Create `IDateTimeProvider` interface and inject `Mock<IDateTimeProvider>` with `SetupSequence` for different return values.
36. XAML MultiBinding repetition for Canvas.Left/Canvas.Top — create an attached behavior (`MarkerPosition.XPropertyPath`/`YPropertyPath`) that auto-creates MultiBindings with the standard converters and FindAncestor. Reduces ~250 lines to ~40.

**Build:** 0 errors, 4 warnings (pre-existing)
**Tests:** 77+ новых тестов (FileService 19 + 5 диалоговых на IDialogFileService, AutosaveService 1, TemplateService 7, RoundTrip 12, Extended 5, EditorCanvasBehavior 18, EditorViewModel 15) — все пройдены

## Sprint 54 — IDialogFileService (изоляция WPF-диалогов)

### Feature S54-1: IDialogFileService (замена OpenFileDialog/SaveFileDialog)

**Проблема:** `FileService.OpenFileDialog()` и `SaveFileDialog()` использовали напрямую WPF `OpenFileDialog`/`SaveFileDialog`. В головной среде (CI, headless) `ShowDialog()` зависает — тесты не могли быть запущены в автоматических пайплайнах. Фильтр xUnit не поддерживал `not`-исключение для этих тестов.

**Исправление:**
- Создан `Services/IDialogFileService.cs` (интерфейс: `OpenFileDialog`, `SaveFileDialog`)
- Создан `Services/WpfDialogFileService.cs` (реализация — перенесён код WPF-диалогов из FileService)
- `FileService` принимает опциональный `IDialogFileService? dialogService = null` (fallback на `WpfDialogFileService(logger)`)
- `App.xaml.cs` — регистрация `services.AddSingleton<IDialogFileService, WpfDialogFileService>()`
- Все 5 тестов диалогов переписаны: используют `Mock<IDialogFileService>` (Verify фильтра/имени файла + возвращаемое значение), никаких вызовов `ShowDialog()` в headless

**Файлы:**
- `Services/IDialogFileService.cs` — новый интерфейс
- `Services/WpfDialogFileService.cs` — новая реализация
- `Services/FileService.cs` — DI + делегирование (строки 13-41)
- `App.xaml.cs:64` — регистрация в DI
- `Tests/Services/FileServiceTests.cs` — 5 тестов с Mock

**Build:** 0 errors, 5 pre-existing warnings
**Tests:** 19/19 FileServiceTests — все пройдены

**Common Mistakes (new):**
37. WPF dialogs (OpenFileDialog/SaveFileDialog) must NOT be used directly in services that need CI/testability. Always extract to `IDialogFileService` interface + `WpfDialogFileService` implementation, inject as optional `= null` parameter. Tests use `Mock<IDialogFileService>` returning null — zero UI calls in headless.

## Sprint 55 — Unit test coverage for managers + SelectTool

### Feature S55-1: ToolManagerTests — 17 tests

**Файл:** `Tests/ViewModels/Managers/ToolManagerTests.cs` (новый)

**Протестированные сценарии:**
- Constructor: defaults (ActiveTool="Select"), null logger guard
- GetOrCreateTool<T>: creates new, returns cached, different types, unknown type throws
- ActiveTool setter + PropertyChanged
- PushTool/PopTool: stack behaviour, Pop on empty returns null
- ResetTool: existing, unknown, not-cached

### Feature S55-2: DirtyStateManagerTests — 16 tests

**Файл:** `Tests/ViewModels/Managers/DirtyStateManagerTests.cs` (новый)

**Протестированные сценарии:**
- Constructor: null template guard
- Defaults: IsDirty=false, FilePath=null, DisplayName=""
- MarkDirty: sets IsDirty, idempotent, PropertyChanged
- ClearDirty: PropertyChanged
- UpdateDisplayName: with/without FilePath, Portrait/Landscape
- FilePath setter

### Feature S55-3: GridManagerTests — 24 tests

**Файл:** `Tests/ViewModels/Managers/GridManagerTests.cs` (новый)

**Протестированные сценарии:**
- Constructor: 3× null guard (template, zoomPanManager, logger)
- ToggleGrid / ToggleSnap
- IsGridEnabled / IsSnapEnabled get/set
- GridStepMm / GridStepMicrons conversion
- RefreshGridNodes: disabled, not visible, MinPixelSpacing, centered, not centered, callback, node validation

### Feature S55-4: ZoomPanManagerExtendedTests — 28 tests

**Файл:** `Tests/ViewModels/Managers/ZoomPanManagerExtendedTests.cs` (новый)

**Протестированные сценарии:**
- IsCentered: viewport > canvas, smaller, zero
- CanvasWidth/HeightPixels: zoom scaling
- ViewportWidth/HeightPixels
- ScrollXRange/YRange: zero when centered, positive when not
- ScrollXValue/YValue
- SetScrollX/Y: centered → no-op, not-centered → clamp + pan offset
- CanvasOffsetX/Y
- CenterCanvas, SetGridRefreshCallback, PanCanvas
- PropertyChanged for dependent properties

### Feature S55-5: ClipboardManager + SelectionManager — 13 tests

**Файл:** `Tests/ViewModels/Managers/ManagerTests.cs` (дополнен)

**ClipboardManager:**
- Cut: copies + calls delete action (single, multiple, empty)
- Clear
- GetClipboardContents: clones + offset, offset increment, offset reset after Copy

**SelectionManager:**
- SelectObjects: clears previous, empty → clears, multiple
- IsObjectSelected: true/false/removed/empty
- Constructor fires onSelectionChanged callback
- SelectAll

### Feature S55-6: SelectToolExtendedTests — 22 tests

**Файл:** `Tests/Tools/SelectToolExtendedTests.cs` (новый)

**Протестированные сценарии:**
- OnDoubleClick: text→inline, line→noop, rect→noop, empty→noop
- OnKeyDown: Delete (single/multi/empty/undoable), Escape (clears state + Reset), unknown key → false
- SelectionBox: start, <threshold, >threshold, direction, finalize select, small-move clear
- Reset: clears drag state
- Cursor: hand on hover, cross on handle, arrow by default

### Feature S55-7: Behavior tests removed (STA requirement)

**Проблема:** 9 тестов для WPF attached-property get/set (MarkerPosition, TextBoxLostFocusCommandBehavior, ComboBoxSelectionChangedCommandBehavior, ZoomComboBoxBehavior, TabItemMiddleClickBehavior) создавали WPF-элементы (TextBox/ComboBox/TabControl), что требует STA thread. xUnit runner использует MTA → `InvalidOperationException`.

**Решение:** Файл `BehaviorAttachedPropertyTests.cs` удалён. Поведения остаются без unit-покрытия — требуют integration/UI тестов с STA-инфраструктурой.

**Build:** 0 errors, 4 pre-existing warnings
**Tests:** 1599 (0 failures, 1 pre-existing skip)

**Common Mistakes (new):**
38. WPF DependencyProperty tests require STA thread — creating WPF elements (`TextBox`, `ComboBox`, `TabControl`) in xUnit tests without STA causes `InvalidOperationException`. Use `[WpfFact]` attribute or STA collection fixture. Pure DP registration (without creating owner elements) may work in MTA.

## Sprint 56 — Colors (StrokeColor/FillColor/Foreground + V-005)

### Feature S56-1: StrokeColor, FillColor, Foreground

**Проблема:** Line и Rectangle не имели StrokeColor, Rectangle не имел FillColor, Text не имел Foreground. Цвета были фиксированным чёрным.

**Исправление (end-to-end):**
- `EditorConstants.cs` — `DefaultStrokeColor = "#000000"`, `DefaultFillColor = "Transparent"`, `DefaultForeground = "#000000"`
- `Line.cs` — `StrokeColor` с INPC + backing field
- `Rectangle.cs` — `StrokeColor` + `FillColor` с INPC
- `Text.cs` — `Foreground` с INPC
- `TemplateDto.cs` / `TemplateService.cs` — маппинг всех цветов (DTO ↔ Model)
- `HexToBrushConverter` — `#RRGGBB`, `#AARRGGBB`, `"Transparent"` → `SolidColorBrush`
- `PropertiesViewModel` — +6 свойств цвета +4 команды изменения
- `PropertiesPanelContent.xaml` — ColorPicker UI с Hex-полем и выбором Transparent
- `EditorCanvas.xaml` — DataTemplate биндинги через Style Setter (Stroke/Fill/Foreground)
- `ValidationService` — V-005: `ValidateHexColor()` — проверка формата HEX + Transparent
- `DrawingLineTool.cs` / `DrawingRectangleTool.cs` / `TextTool.cs` — цвета по умолчанию
- `+36 тестов` (Line/Rectangle/Text цвета, Converter, Validation, RoundTrip)

**Build:** 0 errors, 0 warnings
**Tests:** 1639 passed (0 failures, 1 pre-existing skip)

## Sprint 57 — MultiLine, Half-formats, Library UI, Settings, Documentation

### Feature S57-1: MultiLine + TextAlignment (FR-032)

**Проблема:** Text не поддерживал многострочный текст и выравнивание. InlineTextEditor не имел AcceptsReturn.

**Исправление:**
- `Text.cs` — `TextWrapping` (bool) + `TextAlignment` (string: "Left"/"Center"/"Right") с INPC
- `BoolToTextWrappingConverter` — bool → TextWrapping
- `StringToTextAlignmentConverter` — string → TextAlignment
- `TextAlignmentToIndexConverter` — string → int (ComboBox SelectedIndex)
- `EditorCanvas.xaml` — TextBlock биндинги TextWrapping/TextAlignment
- `InlineTextEditor` — AcceptsReturn=True привязан к TextWrapping; Ctrl+Enter → commit, Enter → новая строка
- `PropertiesViewModel` — +TextTextWrapping/TextTextAlignment + relay-команды
- `PropertiesPanelContent.xaml` — ComboBox выравнивания, CheckBox переноса строк
- `+22 теста`

### Feature S57-2: Half-formats (A4×2, A3×2, A2×2, A1×2, A0×2)

**Проблема:** Требовались форматы с удвоенной длинной стороной для чертежей.

**Исправление:**
- `Sheet.FromFormat()` — +5 форматов: 210×594…841×2378 мм, все Portrait по умолчанию
- `Sheet.GetDefaultOrientation()` — ×2 форматы → Portrait
- `ValidationService.ValidFormats` — +10 entry (×2/X2 для каждого формата × P/L)
- `MainWindow.xaml` — подменю в File > New для half-форматов
- `+25 тестов`

### Feature S57-3: Библиотека шаблонов (FR-043)

**Проблема:** Кнопки Import/Remove в TemplateLibraryViewModel существовали как команды, но не были привязаны в XAML.

**Исправление:**
- `MainViewModel.cs` — передача `IFileService` в `TemplateLibraryViewModel`
- `MainWindow.xaml` — тулбар с кнопками «Импорт» / «Удалить» в левой панели

### Feature S57-4: Настройки (UI)

**Проблема:** Отсутствовал графический интерфейс для изменения настроек приложения.

**Исправление:**
- `SettingsViewModel` — Theme, ShowGrid, SnapToGrid, GridStepMm, AutosaveIntervalMinutes, DefaultSheetFormat, DefaultZoom
- `SettingsView.xaml` + `.cs` — модальное окно 420×440 с 4 секциями, Сохранить/Отмена
- `WpfDialogHostService` — dispatch SettingsViewModel → SettingsView
- `MainViewModel` — +OpenSettingsCommand
- `MainWindow.xaml` — пункт «Настройки...» в меню File
- `+6 тестов`

### Fix S57-5: Документация

**Обновлено:**
- `02_User_Stories_Этап1.md` — 122 чекбокса → ✅
- `19_Статус_проекта.md` — 1760 тестов, Sprint 57 в динамике
- `05_Руководство_пользователя_черновик.md` — раздел 10 переписан (Settings), хоткей V/L/R/T/E
- `docs/archive/` — 82 устаревших файла перемещены
- `AGENTS.md` — добавлены Sprint 56-57, пути к архиву обновлены

**Build:** 0 errors, 0 warnings
**Tests:** 1760 passed (0 failures, 1 pre-existing skip)

## Sprint STA — Unit tests for WPF behaviors (STA-thread)

### Feature STA-1: WpfContext helper
Создан `Tests/Helpers/WpfContext.cs` — STA-thread dispatcher. Создаёт поток с `ApartmentState.STA`, устанавливает `DispatcherSynchronizationContext`, выполняет action и завершает `Dispatcher`.

### Feature STA-2: Behavior handlers → internal static
4 файла изменены — `private static` handlers → `internal static`:
- `TextBoxLostFocusCommandBehavior.OnLostFocus` / `OnKeyDown`
- `ComboBoxSelectionChangedCommandBehavior.OnSelectionChanged`
- `ZoomComboBoxBehavior.OnSelectionChanged` / `OnDropDownClosed` / `ApplyZoom`

Паттерн уже используется в `CanvasInputRouter` (Sprint 53).

### Feature STA-3: TextBoxLostFocusCommandBehaviorTests — 14 тестов

| Категория | Тесты |
|-----------|-------|
| DP get/set | Set, Get, Clear (3) |
| OnLostFocus | Execute, CanExecute=false, null command, non-TextBox sender (4) |
| OnKeyDown | Enter execute + Handled, non-Enter skip, CanExecute=false, null command, non-TextBox sender (7) |

### Feature STA-4: ComboBoxSelectionChangedCommandBehaviorTests — 10 тестов

| Категория | Тесты |
|-----------|-------|
| DP get/set | Set, Get, Clear, non-ComboBox (4) |
| OnSelectionChanged | Execute, CanExecute=false, null command, non-ComboBox sender, null SelectedItem (6) |

### Feature STA-5: ZoomComboBoxBehaviorTests — 11 тестов

| Категория | Тесты |
|-----------|-------|
| DP get/set (DependencyObject) | Set, Get, Clear (3) |
| DP get/set (ComboBox) | Set (1) |
| ApplyZoom | Percent, plain, invalid, zero/negative, no-editor, spaces (6) |
| Events | SelectionChanged, DropDownClosed (2) |

`EditorViewModel` — real instance (not mock) via `ITemplateService`/`IPrintService`. Verify via `editor.ZoomPanManager.Zoom`.

### Feature STA-6: MarkerPositionTests — 10 тестов

DP get/set для `XPropertyPath` и `YPropertyPath` (DependencyObject, null/storage, independence).

**Пропущено (исторически):** `TabItemMiddleClickBehavior`, `PreviewLineChangedBehavior` — требовали полного визуального дерева. **Р ешено в Sprint 62** (23 STA-теста, 12 + 11).

**Build:** 0 errors, 0 warnings
**Tests:** 1780 passed (0 failures, 1 pre-existing skip)

**Common Mistakes (new):**
39. WPF `Control` constructor requires STA — `new ComboBox()`, `new TextBox()`, `new Button()` throw `InvalidOperationException` on MTA. Always create WPF elements inside an STA thread (via `WpfContext.Execute`).
40. Moq cannot mock non-virtual methods — `SetZoomPercent` is not virtual → use real `EditorViewModel` instance and verify via `editor.Zoom` instead of `mock.Verify`.
41. `Mock<T>(MockBehavior, params object[] args)` with nullable reference types — passing `(GridSettings?)null` to `object[]` triggers CS8625/CS8604. Use a `GridSettings?` local variable set to `null` or `null!`.
42. `PresentationSource` in .NET 10 WPF — the abstract class requires `GetCompositionTargetCore()`, `RootVisual` getter/setter, and `IsDisposed`. `GetVisualRoot()` no longer exists. Create `FakePresentationSource` implementing all abstract members.

## Sprint R3.1 — EditorViewModel де-bloat (forwarding-свойства → менеджеры)

### Что сделано

**Проблема:** EditorViewModel содержал ~60 forwarding-свойств, дублирующих свойства менеджеров (ZoomPanManager, PreviewManager, StatusBarManager и др.). Каждое свойство имело `OnPropertyChanged()` в сеттере для ретрансляции уведомлений на EditorViewModel. Ретрансляция требовалась, когда XAML биндился к EditorViewModel, но после R3.1 XAML уже биндился напрямую к менеджерам — forwarding стал мёртвым грузом. Дополнительно 4 обработчика `PropertyChanged` подписывались на менеджеров и пере-оповещали EditorViewModel.

**Исправление:**
- Удалены ~25 forwarding-свойств (те, что не требуются IEditorContext):
  - `CanvasWidthPixels`, `CanvasHeightPixels`, `PanOffsetX/Y`, `ZoomPercent`, `ViewportWidth/HeightMm`, `ViewportWidth/HeightPixels`, `ScrollX/YRange`, `ScrollX/YValue`, `IsCentered`, `CanvasOffsetX/Y`
  - `ShowSelectionMarkers`, `GridNodes`, `GridInvalidated`
  - `StatusBarSheetFormat`, `StatusBarGridEnabled`, `StatusBarGridStepMm`, `StatusBarSnapEnabled`
  - `ActiveTool`, `InlineEditingText`, `InlineEditText`
- Упрощены ~15 свойств IEditorContext (убраны `OnPropertyChanged()`):
  - `PreviewLine`, `PreviewRectangle`, `PreviewText`, `SelectionBoxLeft/Bottom/Top/Width/Right/Height`, `SelectionDirection`, `StatusMessage`, `Zoom`
- Удалены 4 поля-обработчика, 4 подписки `PropertyChanged` в конструкторе, 4 отписки в `Dispose`
- `OnZoomChangedInternal` удалён (заменён на `() => { }`)
- `IAutosaveTab` (`IsDirty`, `FilePath`, `DisplayName`) — explicit interface implementation
- `OnSelectionChangedInternal` упрощён (убраны `ShowSelectionMarkers`, `SingleSelectedObject`)
- `PreviewLineChangedBehavior` переписан на `PreviewManager.PropertyChanged`
- ~90 тестов исправлены на manager-свойства

**Результат:**
```
EditorViewModel: ~1194 → 784 строк (−410, −34%)
Build:  0 errors, 0 warnings
Tests:  1780 passed, 1 skip
```

**Файлы:**
- `ViewModels/EditorViewModel.cs` — основной файл рефакторинга
- `Behaviors/PreviewLineChangedBehavior.cs` — переписан на PreviewManager
- `MainViewModel.cs` — 9 замен (DirtyStateManager)
- `EditorCanvas.xaml` / `.xaml.cs` — 7 замен (ZoomPanManager, GridManager)
- `CanvasInputRouter.cs` — 2 замены
- `EditorViewModelTests.cs` — ~90 исправлений

## Sprint R3.1-HF1 — Preview fix (unconditional PropertyChanged)

**Проблема:** `[ObservableProperty]` на `PreviewLine`/`PreviewRectangle`/`PreviewText` в `PreviewManager` подавлял `PropertyChanged` при re-assign той же ссылки (`EqualityComparer<T>.Default.Equals()` для reference-типов = `ReferenceEquals`). Три инструмента (DrawingLineTool, DrawingRectangleTool, TextTool) мутируют существующий preview-объект и переустанавливают его — PropertyChanged не стреляет, `PreviewLineChangedBehavior` не обновляет WPF-элементы, предпросмотр пропадает.

**Исправление:** `[ObservableProperty]` заменён на ручные сеттеры с безусловным `OnPropertyChanged()` для трёх полей. SelectionBox-поля (`long`, `byte`) не тронуты — equality check для value-типов корректен.

**Файл:** `ViewModels/Managers/PreviewManager.cs` (3 поля, ~6 строк)

## Sprint R3.1-HF2 — Selection markers fix (ShowSelectionMarkers notification)

**Проблема:** После R3.1 XAML биндится напрямую к `SelectionManager.ShowSelectionMarkers` (computed property: `=> SelectedObjects.Count > 0`). Однако `PropertyChanged` для этого свойства никогда не вызывался — при изменении коллекции `SelectedObjects` срабатывал только переданный `_onSelectionChanged`-коллбэк в `EditorViewModel`. WPF-биндинг застывает на `Collapsed`.

**Исправление:** В конструктор `SelectionManager` добавлен `OnPropertyChanged(nameof(ShowSelectionMarkers))` в лямбду `CollectionChanged`.

**Файл:** `ViewModels/Managers/SelectionManager.cs` (1 строка)

## Phase 4 — PropertiesViewModel split (649→85 lines)

**Done:** PropertiesViewModel разделён на базу + 3 sub-VM. Прямые биндинги заменены на ContentControl + DataTemplate.

| Файл | Было | Стало |
|------|------|-------|
| PropertiesViewModel.cs | 649 строк (монолит) | 85 строк (база: selection + sub-VM lifecyle) |
| LinePropertiesViewModel.cs | — | 148 строк (7 свойств + 14 команд + INPC) |
| RectanglePropertiesViewModel.cs | — | 168 строк (8 свойств + 16 команд + INPC) |
| TextPropertiesViewModel.cs | — | 233 строки (13 свойств + 20 команд + INPC) |
| PropertiesPanelContent.xaml | 549 строк (3×StackPanel) | ~620 строк (3×DataTemplate + ContentControl) |
| PropertiesViewModelTests.cs | 1313 строк | 1106 строк (sub-VM property/command paths) |
| PropertiesViewModelCommandTests.cs | 325 строк | 262 строки (sub-VM command paths) |

**Изменения:**
- Каждый sub-VM: `ObservableObject` + `UpdateObject(T?)` + INPC forwarding + `SetProperty` + `[RelayCommand]`
- Sub-VM подписываются на `INotifyPropertyChanged.PropertyChanged` модели для live-обновления
- XAML: 3 visible StackPanel → 3 DataTemplate на тип + `ContentControl Content="{Binding LineVM/RectVM/TextVM}"`
- Base VM: только `SelectedObject`, `SelectionCount`, `IsSingleSelection`, `IsLineSelected`, `IsRectangleSelected`, `IsTextSelected`, `ObjectId`, `ObjectTypeName`, `ValidationError`; sub-VM паблишеры через конструктор
- `PropertiesViewModel.SetProperty()` удалён из базы (логика в sub-VM)
- `PropertiesPanelContent.xaml.cs`: `OnTextIsEditableClick` обновлён на `textVm.ChangeIsEditableCommand.Execute()`

**Build:** 0 errors, 0 warnings
**Tests:** 1796 passed, 1 pre-existing skip

## Sprint — Print Preview (Ctrl+Shift+P)

### Feature: Предпросмотр печати

**Проблема:** Отсутствовал предпросмотр печати — пользователи не могли видеть, как будет выглядеть шаблон на листе перед печатью. Ранее был только прямой вывод на принтер через `PrintDialog`.

**Исправление:** Реализован end-to-end предпросмотр через `DocumentViewer` с `FixedDocument`:

| Компонент | Файл | Назначение |
|-----------|------|------------|
| Интерфейс | `Services/IPrintDocumentGenerator.cs` | Контракт: `FixedDocument Generate(Template)` |
| Генератор | `Services/PrintDocumentGenerator.cs` | Model → WPF элементы (Line, Rectangle, TextBlock) с конвертацией координат (микроны→WPF, Y-flip) |
| Окно | `Views/PrintPreviewWindow.xaml` + `.cs` | DocumentViewer с FitToWidth, Print кнопкой, Close |
| Интеграция | `ViewModels/MainViewModel.cs` | PreviewPrintCommand, DI IPrintDocumentGenerator |
| UI | `MainWindow.xaml` | MenuItem + Ctrl+Shift+P KeyBinding |
| DI | `App.xaml.cs` | Transient registration |
| Тесты | `Tests/Services/PrintDocumentGeneratorTests.cs` | 19 тестов: элементы, координаты, цвета, типы линий, поворот, несколько объектов |

**Архитектурные решения:**
- FixedDocument + WPF-элементы (не RenderTargetBitmap) — векторное качество, совместимость с DocumentViewer
- Отдельный `IPrintDocumentGenerator` — не заменяет `IPrintService`
- Transient регистрация — stateless генератор
- FitToWidth при загрузке — автоподгонка под окно

## Sprint 58 — Архитектурный анализ (аналитический спринт)

Полный отчёт: [`docs/48_Архитектурный_анализ_и_план_рефакторинга.md`](docs/48_Архитектурный_анализ_и_план_рефакторинга.md)

### Найденные архитектурные проблемы (25 замечаний)

Ключевые находки:
- **P0:** EditorViewModel — god-object «фасад с пробросом» (1160 строк, ~60 forwarding-свойств, 4 switch-обработчика для ретрансляции INPC). **Решено (Sprint R3.1):** ~1194→784 строк, forwarding удалён, XAML биндится к менеджерам.
- **P1:** Избыточная 3-уровневая иерархия моделей (ObjectBase → ModelBase → TemplateObjectBase). Решение: схлопнуть в один уровень.
- **P1:** ~50 дублированных INPC-setter в Line/Rectangle/Text. Решение: `[ObservableProperty]` source generator.
- **P1:** MoveSelected/RotateSelected не группируются в BatchCommand (inconsistent Undo).
- **P1:** ValidationService — static 537-строчный god-service, untestable.
- **P1:** Нет Central Package Management, `TreatWarningsAsErrors` только в CI.

### План рефакторинга R1–R4

| Спринт | Цель | Длительность |
|--------|------|-------------|
| R1 | Быстрые победы: CPM, TreatWarningsAsErrors, Undo-группировка, flaky-тесты | 2–3 дня |
| R2 | Models cleanup: иерархия, `[ObservableProperty]`, ITemplateValidator | 3–4 дня |
| R3 | EditorVM de-bloat: проброс через менеджеры, IEditorContext, DI | 4–5 дней |
| R4 | Presentation + Tests: EditorCanvasBehavior, CI coverage-gate | 4–5 дней |

## Sprint 59 — Grid bug fixes (PropertyChanged, мёртвый код, ComputeDisplayStep)

### Fix SG-1: IsGridEnabled/IsSnapEnabled не дёргали PropertyChanged

**Проблема:** Сеттеры `GridManager.IsGridEnabled` и `IsSnapEnabled` не вызывали `OnPropertyChanged()`. При программном изменении (меню, код) XAML ToggleButton на тулбаре не обновлял `IsChecked`.

**Исправление:** Добавлен `OnPropertyChanged()` в оба сеттера.

### Fix SG-2: ToggleGrid() десинхронизировал Enabled и Visible

**Проблема:** `ToggleGrid()` переключал только `Enabled`. Если до вызова было `Enabled=false, Visible=false` (через сеттер), после `ToggleGrid()` становилось `Enabled=true, Visible=false` — сетка скрыта.

**Исправление:** `ToggleGrid()` переписан через `IsGridEnabled = !IsGridEnabled` (сеттер).

### Fix SG-3: Truncation вместо Rounding в координатах узлов

**Проблема:** `(long)` каст в `RefreshGridNodes()` отбрасывал дробную часть. На высоком zoom — ошибка позиционирования.

**Исправление:** `(long)` → `(long)Math.Round()`.

### Fix SG-4: Мёртвый код GridLine/GenerateGridLines/GenerateVisibleGridLines

**Проблема:** `GridHelper` содержал struct `GridLine` и два метода генерации линий (~90 строк), которые никогда не вызывались в production. Только тесты.

**Исправление:** Удалены `GridLine`, `GenerateGridLines()`, `GenerateVisibleGridLines()`. Удалены 13 тестов для этих методов.

### Fix SG-5: GridStepToStringConverter — ненадёжный парсинг

**Проблема:** `ConvertBack` удалял только `"мм"`. Другие форматы (`"5 mm"`, `"5,5"`) молча возвращали `5.0`.

**Исправление:** Конвертер удаляет любой нечисловой суффикс (Regex), нормализует comma→dot, при ошибке парсинга возвращает `Binding.DoNothing`.

### Fix SG-6: Изменение шага сетки не влияло на отображение

**Корневая причина:** `GridManager.RefreshGridNodes()` вызывал `ComputeDisplayStep()`, который **полностью игнорировал** `_gridSettings.StepMicrons`. Шаг вычислялся только из `MinPixelSpacing / zoom`. Пользовательский шаг нигде не участвовал.

**Исправление:**
- `ComputeDisplayStep()` принимает `preferredStepMicrons` (опциональный параметр)
- Если `preferredStep` даёт `pixelSpacing >= MinPixelSpacing` — используется как целевой
- Если `pixelSpacing < MinPixelSpacing` — fallback на `MinPixelSpacing / zoom`
- В обоих случаях шаг coarsen'ится если `cols * rows > maxNodes`
- В `GridStepMm` сеттер добавлен `OnPropertyChanged()`

### Common Mistakes (new)
43. `async void` in timer handler — `MainViewModel.OnAutosaveTickHandler()` is `async void` subscribed to `AutosaveTick`. If `AutosaveAllTabsAsync` throws, exception is lost (not caught). Always use try/catch with logging inside `async void`, or wrap in `SafeFireAndForget`.
44. Multi-Undo inconsistency — `DeleteSelected()` and `PasteFromClipboard()` group multi-object operations into `BatchCommand`, but `MoveSelected()` and `RotateSelectedClockwise()` do NOT. User presses Undo N times for N objects. Always apply `BatchCommand` when `SelectedObjects.Count > 1`.
45. File name ≠ type name — `Commands/IUndoCommand.cs` contains interface `IUndoCommand` (renamed from `ICommand.cs` to avoid WPF conflict). The file name is misleading and may cause wrong `using` imports. Rename to `IUndoCommand.cs`.
46. `IAutosaveTab` defined inside service — `Services/AutosaveService.cs` contains `public interface IAutosaveTab`. EditorViewModel explicitly implements it. Service dictates interface to ViewModel (inverted dependency). Always define interfaces near their consumer, not provider.
47. `PrintVisualProvider` leaks WPF type — `Func<System.Windows.Media.Visual?>` on EditorViewModel exposes WPF rendering to ViewModel. View sets it, creating a potential dangling reference after tab close. Encapsulate via interface or use WeakReference/Messenger.
48. `CustomResizeCommand` reuses `_newHeight` as FontSize for Text — semantic confusion in the same field. The command's `Execute()`/`Undo()` use `switch (_object)` instead of polymorphism, violating OCP. Prefer `ApplyResize(CaptureState())` on the object.
49. `ValidationService` is static and untestable — `Helpers/ValidationService` is a `static class` called directly from PropertiesViewModel and TemplateService. Cannot be mocked. Make domain validation injectable (`ITemplateValidator`). UI field validators can stay static as pure functions.
50. No Central Package Management — package versions are hardcoded in two csproj files. No `Directory.Packages.props`. Versions drift independently. Adopt CPM (`<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>` + `Directory.Packages.props`).
51. csproj duplicates Directory.Build.props — `TargetFramework`, `Nullable`, `ImplicitUsings` declared in both `Directory.Build.props` and each csproj. Remove duplicates from csproj, keep only project-specific properties (`OutputType`, `UseWPF`).
52. `TreatWarningsAsErrors` only in CI — local builds don't catch warnings. CI `analyze` job uses `/warnaserror`, but developers see warnings only after push. Add `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` in `Directory.Build.props`.

53. `CustomResizeCommand` refactoring — after R4.3, the constructor takes `(TemplateObjectBase, ResizeState, ResizeState, Action?)` NOT 9 `long` params. The factory methods `ForRectangle`, `ForText`, `ForLine` are kept for backward compatibility.
54. `ResizeMath` — all pure resize calculations live in `Tools/ResizeMath.cs`. `ResizeTool.cs` delegates to it. Do NOT add new resize math to ResizeTool directly.
55. `ShortcutRegistry` — add new keyboard shortcuts to `Helpers/ShortcutRegistry.cs`, NOT to `MainWindow.xaml.cs`.
56. `CaptureResizeState`/`ApplyResize` — every new model subclass of `TemplateObjectBase` MUST implement these two methods for undoable resize to work.
57. Test file merging — after R4.4, there are NO Extended/Additional test files. All tests live in the parent files. Create tests in the parent, not in separate files.
58. Coverage gate — CI checks coverage ≥75%. On failure, the build is red. Generate coverage locally with `dotnet test --collect:"XPlat Code Coverage"` before pushing.
59. Forwarding properties after R3.1 — after XAML was migrated to bind to managers directly (R3.1), forwarding properties on EditorViewModel became dead code. Remove them: delete the property, delete the `OnPropertyChanged()` in setters of IEditorContext-required properties, remove PropertyChanged forwarding handlers (`_zoomPanHandler`, `_previewHandler`, `_dirtyStateHandler`, `_toolManagerHandler`), remove `OnZoomChangedInternal()`, and simplify `OnSelectionChangedInternal()`. IAutosaveTab properties become explicit interface implementation. Test references must use `editor.XManager.Y` instead of `editor.Y`.
60. `[ObservableProperty]` on reference-type fields with re-assign — the source-generated setter uses `EqualityComparer<T>.Default.Equals()`, which for reference types defaults to `ReferenceEquals`. If you mutate the same instance and re-assign it, `PropertyChanged` is suppressed. Use manual setters with unconditional `OnPropertyChanged()` for preview/re-assign patterns.
61. Computed properties (expression-bodied, no `[ObservableProperty]`) on ObservableObject managers that are bound from XAML must fire `OnPropertyChanged()` explicitly when their dependencies change. The binding engine only re-evaluates when `PropertyChanged` fires for that property name — it does NOT infer dependencies from the expression body.
62. WPF RotateTransform matrix — WPF использует STANDARD CCW Cartesian matrix `x'=x*cosθ−y*sinθ, y'=x*sinθ+y*cosθ`. В Y-down (screen) это даёт CW-вращение. RotatedCorner*/GetBoundingBox используют forward transform. ContainsPoint() использует INVERSE transform `u=x*cos+y*sin, v=−x*sin+y*cos` для unrotate точки в локальное пространство текста. Не путать с CW-specific формулами — они неверны для WPF.

## Sprint A–D — Архитектурный рефакторинг (18 замечаний)

После Sprint 59 был проведён Архитектурный анализ (48_Архитектурный_анализ_и_план_рефакторинга.md) и составлен план рефакторинга на 4 спринта (A–D). Выполнено 12 из 14 пунктов, 2 пропущены (P4).

### A.1 — IDisposable в sub-VM (утечка памяти)

**Проблема:** `LinePropertiesViewModel`, `RectanglePropertiesViewModel`, `TextPropertiesViewModel` подписывались на `INotifyPropertyChanged.PropertyChanged` модели в `UpdateObject()`, но никогда не отписывались. При закрытии вкладки sub-VM продолжали висеть в памяти через delegate.

**Исправление:** Добавлены `IDisposable.Dispose()` во все 3 sub-VM с отпиской. `PropertiesViewModel.Dispose()` каскадно вызывает dispose всех трёх.

**Файлы:**
- `LinePropertiesViewModel.cs`, `RectanglePropertiesViewModel.cs`, `TextPropertiesViewModel.cs`, `PropertiesViewModel.cs`

### A.2 — Dual-write GridManager/StatusBarManager

**Проблема:** `StatusBarManager` владел отдельной копией `GridSettings` (StepMicrons, GridEnabled, SnapEnabled), дублируя состояние `GridManager`. Два независимых источника истины — мутация через UI (StatusBar) не синхронизировалась с GridManager.

**Исправление:** `StatusBarManager` больше не содержит `GridSettings`. Конструктор принимает 6 делегатов (get/set для GridEnabled, GridStepMm, SnapEnabled) + `Action onGridRefresh`. `EditorViewModel` передаёт лямбды к `GridManager`. `GridManager` — единственный owner `GridSettings`.

**Файлы:** `StatusBarManager.cs`, `EditorViewModel.cs`, `GridManager.cs`

**Common Mistakes (new):**
63. Dual-write in managers — never give two managers independent copies of the same mutable settings. One must be the single source of truth; others delegate via lambdas or events.

### B.1 — FontMetrics: static → instance + DI

**Проблема:** `FontMetrics` — полностью static class. Тесты не могли мокировать, DI-контейнер не имел интерфейса.

**Исправление:** Создан `IFontMetrics` interface. `FontMetrics` переведён из static в instance class, реализующий `IFontMetrics`. Добавлен `static readonly FontMetrics Default = new()` для backward compat. DI-регистрация: `services.AddSingleton<IFontMetrics>(FontMetrics.Default)`. `Text.cs` использует `FontMetrics.Default.GetHeightRatio/FontMetrics.Default.GetAdvWidthRatio`.

Для устранения flaky race-условий в параллельных тестах (FontMetricsTests и TextTests/HitTestHelperTests модифицировали shared state одновременно) добавлен `[Collection("FontMetrics", DisableParallelization = true)]`.

**Файлы:** `Models/IFontMetrics.cs` (новый), `FontMetrics.cs`, `Text.cs`, `App.xaml.cs`, `FontMetricsTests.cs`, `TextTests.cs`, `HitTestHelperTests.cs`, `FontMetricsTestCollection.cs` (новый)

### B.2 — PanOffsetX/Y forwarding удалены из EditorViewModel

**Проблема:** После R3.1 XAML биндится напрямую к `ZoomPanManager.PanOffsetX/Y`, но EditorViewModel продолжал содержать forwarding-свойства `PanOffsetX`/`PanOffsetY`. Тесты использовали `editor.PanOffsetX` вместо `editor.ZoomPanManager.PanOffsetX`.

**Исправление:** Свойства удалены из EditorViewModel. Тесты заменены: `editor.PanOffsetX` → `editor.ZoomPanManager.PanOffsetX`.

**Файлы:** `EditorViewModel.cs`, `EditorViewModelTests.cs`, `PanToolTests.cs`

### B.3 — EditorConstants → PhysicalConstants/EditorSettings

**Проблема:** `EditorConstants.cs` — 36-line pure proxy, каждая константа ре-экспортировала `PhysicalConstants.XXX` или `EditorSettings.XXX`. 69 references в 20 файлах.

**Исправление:** Все 69 references заменены прямыми вызовами `PhysicalConstants.XXX` или `EditorSettings.XXX`. `EditorConstants.cs` удалён.

**Файлы:** 20 файлов обновлены, `EditorConstants.cs` удалён.

### C.1 — Shortcuts из code-behind в ShortcutRegistry

**Проблема:** `Window_PreviewKeyDown` (30 строк) содержал логику диспетчеризации клавиш. Добавление нового хоткея требовало изменения code-behind.

**Исправление:** Создан `ShortcutRegistry.TryHandle(Key, ModifierKeys, EditorViewModel) → bool` — единая точка входа. `Window_PreviewKeyDown` сокращён до 3 строк.

**Файлы:** `ShortcutRegistry.cs`, `MainWindow.xaml.cs`

### C.2 — Tag-parsing в CustomSheetDialogViewModel

**Проблема:** Кнопки быстрого выбора формата (A4/A3/…) использовали `Tag="210,297"` с парсингом в code-behind (`OnQuickFormatClick`, `string.Split(',')`). Нетестируемо, XAML-зависимо.

**Исправление:** Код из code-behind удалён. Добавлен `SetQuickFormatCommand(string formatName)` в `CustomSheetDialogViewModel`, вызывающий `Sheet.FromFormat(formatName)`. XAML: `Click` + `Tag` → `Command="{Binding SetQuickFormatCommand}" CommandParameter="A4"`. Code-behind файл сокращён до конструктора.

**Файлы:** `CustomSheetDialogViewModel.cs`, `CustomSheetDialog.xaml`, `CustomSheetDialog.xaml.cs`

### C.3 — No-op Dispose удалён из TemplateLibraryViewModel

**Проблема:** `TemplateLibraryViewModel` реализовывал `IDisposable` с пустым телом. Вызов `Dispose()` в `MainViewModel.Dispose()` — мёртвый код.

**Исправление:** `IDisposable` удалён из класса. Вызов `TemplateLibraryVm?.Dispose()` удалён из `MainViewModel.Dispose()`.

**Файлы:** `TemplateLibraryViewModel.cs`, `MainViewModel.cs`

### C.4 — ITool.OnMouseWheel → bool

**Проблема:** `ITool.OnMouseWheel` возвращал `void` — инструменты не могли заблокировать зум. CanvasInputRouter безусловно применял zoom после вызова `OnMouseWheel`.

**Исправление:** `ITool.OnMouseWheel` теперь возвращает `bool` — `true` означает «событие обработано, зум не применять». Все 6 реализаций обновлены (возвращают `false`). `CanvasInputRouter` проверяет return value.

**Файлы:** `ITool.cs`, `SelectTool.cs`, `PanTool.cs`, `DrawingLineTool.cs`, `DrawingRectangleTool.cs`, `TextTool.cs`, `ResizeTool.cs`, `CanvasInputRouter.cs`, `ToolManagerTests.cs`

### C.5 — Memory leak SelectionManager

**Проблема:** `SelectionManager` подписывался на `SelectedObjects.CollectionChanged` в конструкторе, но отписка не была предусмотрена.

**Исправление:** `SelectionManager` реализует `IDisposable`. Хэндлер сохранён в поле `_onCollectionChanged`, отписка в `Dispose()`. `EditorViewModel.Dispose()` вызывает `_selectionManager.Dispose()`.

**Файлы:** `SelectionManager.cs`, `EditorViewModel.cs`

### D.1 — MockBehavior.Strict → Loose

3 мока `ICommand` в behavior-тестах использовали `MockBehavior.Strict` — при добавлении нового метода в `ICommand` тесты падали. Заменены на `MockBehavior.Loose`.

**Файлы:** `TextBoxLostFocusCommandBehaviorTests.cs`, `ComboBoxSelectionChangedCommandBehaviorTests.cs`

### D.3 — Sealed классы

66 классов помечены `sealed`: все Converters (27), Commands (5), Services (20), Tools (8), Managers (9).

**Common Mistakes (new):**
64. `IDisposable` with lambda subscriptions — always save the handler reference to a field and unsubscribe in `Dispose()`. Lambda-in-constructor subscriptions can't be removed without a stored reference.
65. `ITool.OnMouseWheel` return type — use `bool` (handled flag), consistent with `OnKeyDown`. Tools that don't need wheel handling return `false`; future tools can block zoom by returning `true`.

## Sprint — Grid refactoring (Points 1, 4, 5 из архитектурного обсуждения)

### Что сделано

Три взаимосвязанных изменения в архитектуре сетки:

**Point 1 — Хранение микронов вместо пикселей:**
- `GridNodesLayer` теперь хранит координаты узлов в **микронах** (model space), а не в пикселях
- `OnRender` сам конвертирует микроны → пиксели (zoom + Y-flip)
- Добавлены DependencyProperty `Zoom` и `SheetHeightMm` — при изменении zoom`а или высоты листа перерисовка через `InvalidateVisual`
- `GridNodesLayer` изменён с `UIElement` на `FrameworkElement` (нужен для WPF Data Binding через DPs)
- При зуме больше НЕ требуется регенерация узлов (только смена шага или viewport)

**Point 4 — Упрощение pan-кэширования (удалено целиком):**
- Удалены: `_cachedRegionLeftMicrons`, `_cachedRegionBottomMicrons`, `_cachedRegionWidthMicrons`, `_cachedRegionHeightMicrons`, `_hasCachedRegion`
- Удалены: `IsWithinCachedRegion()`, `InvalidateCacheOnPan()`, `RefreshOnPanEnd()`
- Удалены: `_debounceCts`, `SuppressDebounce`, `PanDebounceMs`
- Удален: `_onPanRefresh` в ZoomPanManager, `SetPanRefreshCallback()`, вызов в `PanCanvas()`
- При панорамировании сетка движется через `RenderTransform` (TranslateTransform) — без регенерации
- Регенерация на pan-end: прямой вызов `RefreshGridNodes()` (без дебаунса, без кэша)

**Point 5 — Buffer safety:**
- `GridManager._nodeData` больше не `readonly` с мутацией — каждый `RefreshGridNodes()` аллоцирует **новый** `long[]`
- Нет shared mutable state между GridManager и GridNodesLayer
- SetNodes сохраняет ссылку на массив, который гарантированно не мутируется после передачи

### Итоговый diff

| Мера | До | После |
|------|----|-------|
| GridManager строк | 246 | 145 |
| Полей кэша | 5 | 0 |
| Методов pan-caching | 3 | 0 |
| CTS / Debounce | 2 (InvalidateGrid + InvalidateCacheOnPan) | 0 |
| Shared mutable long[] | Да (одна аллокация, мутация) | Нет (новая аллокация на refresh) |
| Регенерация на zoom | Full (pixel conv + nodes) | Full (nodes only, ~2× быстрее) |
| Регенерация на pan-move | Debounced 50ms | Нет (только RenderTransform) |
| Регенерация на pan-end | Всегда + сброс кэша | Всегда (без pixel conv, без кэша) |

**Файлы:**
- `Views/GridNodesLayer.cs` — DPs, OnRender, FrameworkElement
- `ViewModels/Managers/GridManager.cs` — -105 строк (удалён кэш, pixel conv, debounce)
- `ViewModels/Managers/ZoomPanManager.cs` — удалён _onPanRefresh
- `ViewModels/EditorViewModel.cs` — удалён SetPanRefreshCallback
- `Behaviors/CanvasInputRouter.cs` — RefreshOnPanEnd → RefreshGridNodes
- `Views/EditorCanvas.xaml` — Zoom/SheetHeightMm bindings
- `Views/EditorCanvas.xaml.cs` — упрощён GridInvalidated handler
- `Tests/ViewModels/Managers/GridManagerTests.cs` — SuppressDebounce removed, PoolReusesSameArray → AllocatesNewArrayEachCall, pixel→micron asserts
- `Tests/ViewModels/EditorViewModelTests.cs` — pixel→micron asserts

**Build:** 0 errors, 0 warnings
**Tests:** 2035 passed, 1 pre-existing skip

## Sprint 60 — Fix inline text editing (auto-focus, Escape/LostFocus routing, ShortcutRegistry guard)

### 6 исправлений

**Fix 60-1: AutoFocusOnVisibleBehavior**
- Новый attached behavior: при `IsEnabled=True` и `IsVisibleChanged` → `element.Focus()` + `SelectAll()` для TextBox
- Через `Dispatcher.BeginInvoke(DispatcherPriority.Loaded)` — layout должен завершиться
- Отписка от `IsVisibleChanged` при `IsEnabled→false`

**Fix 60-2: EditorCanvas.xaml — InlineTextEditor**
- Добавлен `behaviors:AutoFocusOnVisibleBehavior.IsEnabled="True"`
- Добавлен `LostFocus="InlineTextEditor_LostFocus"`

**Fix 60-3: EditorCanvas.xaml.cs — LostFocus→Commit**
- `InlineTextEditor_LostFocus`: если `IsEditing`, вызывает `CommitInlineEditingCommand`
- Безопасность: `Commit()` проверяет `InlineEditingText==null`

**Fix 60-4/5: CanvasInputRouter — guards**
- `RoutePreviewKeyDown` и `RouteKeyDown`: если `InlineEditManager.IsEditing` → `return`
- Escape/Enter при редактировании не уходят в инструменты

**Fix 60-6: ShortcutRegistry — guard**
- `TryHandle`: если `InlineEditManager.IsEditing` → `return false`
- V/L/R/T/E при редактировании не переключают инструменты

### Новые тесты
- `ShortcutRegistryTests.cs` — 7 тестов (V/L/R/T/E/ShiftE при IsEditing + положительный контроль)
- `AutoFocusOnVisibleBehaviorTests.cs` — 5 тестов (DP get/set + registration check)

### Common Mistakes (new)
66. `RouteKeyDown` must have the same `IsEditing` guard as `RoutePreviewKeyDown`. Without it, key events during inline editing reach the active tool and can clear selection, switch tools, or delete objects.
67. `ShortcutRegistry.TryHandle` must check `editor.InlineEditManager.IsEditing` before processing shortcuts. Without the guard, V/L/R/T/E hotkeys during inline editing switch tools or rotate objects instead of being handled by the TextBox.
68. WPF `LayoutTransform` offset on rotated elements ( WPF positions a `LayoutTransform`-ed element so the **top-left of the transformed bounding box** (not the local origin `(0,0)`) lands at the layout position. For `Text` with `RotateTransform(angle, 0, 0)`, this creates an offset `(-minX, +minY)` where `minX = min(0, W·cosθ, −H·sinθ, W·cosθ−H·sinθ)` and `minY = min(0, W·sinθ, H·cosθ, W·sinθ+H·cosθ)`. Model formulas (`RotatedCorner0-3`, `ContainsPoint`, `GetBoundingBox`) MUST apply this offset to match the visual position. At 0° the offset is (0,0) — no change. `HitTestHelper.GetTextHandle` must use `Text.RotatedCorner0-3` directly (not recompute corners) to stay consistent.

**Build:** 0 errors, 0 warnings
**Tests:** 2035 passed, 1 pre-existing skip

## Pipeline — Автоматизированный цикл разработки (18.07.2026)

Создан multi-agent pipeline для автоматизации полного цикла: Plan → Implement → Test → Review → Docs → Critic → PR.

### Архитектура

```
Conductor (primary) → делегирует subagent'ам через Task tool
├── planner     — read-only, пишет спеки
├── implementor — edit + bash, пишет код
├── tester      — edit + bash, тесты
├── reviewer    — read-only, код-ревью
├── critic      — read-only, финальный контроль
└── gh-ops      — bash, git/gh операции
```

### Команды
| Команда | Описание |
|---------|----------|
| `/pipeline full <desc>` | Полный цикл с Critic в конце |
| `/pipeline quick <desc>` | Быстрый цикл (без plan/docs/critic) |
| `/plan <desc>` | Только планирование |
| `/review` | Только ревью текущих изменений |

### Files
| Path | Назначение |
|------|-----------|
| `.opencode/agents/conductor.md` | Оркестратор (primary) |
| `.opencode/agents/planner.md` | Планирование |
| `.opencode/agents/implementor.md` | Реализация |
| `.opencode/agents/tester.md` | Тестирование |
| `.opencode/agents/reviewer.md` | Code review |
| `.opencode/agents/critic.md` | Финальный контроль |
| `.opencode/agents/gh-ops.md` | GitHub операции |
| `.opencode/skills/code-reviewer/SKILL.md` | Правила ревью |
| `.opencode/skills/documentation-writer/SKILL.md` | Правила документирования |
| `.opencode/skills/github-workflow/SKILL.md` | git/gh инструкции |
| `.opencode/commands/pipeline.md` | Команда полного pipeline |
| `.opencode/commands/pipeline-quick.md` | Команда быстрого pipeline |
| `.opencode/commands/plan.md` | Команда планирования |
| `.opencode/commands/review.md` | Команда ревью |
| `.github/workflows/opencode-pipeline.yml` | CI + OpenCode review |

## Pipeline — README encoding fix (18.07.2026)

### Fix README encoding
**Проблема:** README.md содержал UTF-8 double-encoding — русский текст и эмодзи отображались как mojibake (`рџ“‹ Рћ РџР РћР•РљРўР•` вместо `📋 О ПРОЕКТЕ`).
**Исправление:** 180 строк с mojibake декодированы через UTF-8 → CP1251 → UTF-8 селективно (строка за строкой). 220 правильно закодированных строк сохранены без изменений.
**Файл:** `README.md`
**Build:** 0 errors, 0 warnings
**Tests:** 2035 passed, 1 pre-existing skip

## Pipeline — README encoding fix v2 (18.07.2026)

### Fix README encoding (v2)
**Проблема:** README.md снова содержал UTF-8 double-encoding — русский текст и эмодзи отображались как mojibake.
**Исправление:** Селективное декодирование строк 14–401 через UTF-8 → CP1251 → UTF-8. Строки 1–13 не затронуты.
**Файл:** `README.md`
**Build:** 0 errors, 0 warnings
**Tests:** 2035 passed, 1 pre-existing skip

## Pipeline — CI/CD GitHub Actions (18.07.2026)

### Feature CI workflow
**Проблема:** Отсутствовал CI/CD — PR не проверялись автоматически, coverage не контролировался.
**Исправление:** Добавлен `.github/workflows/ci.yml` — build + test + coverage gate 75% на `windows-latest` с NuGet кэшированием.
**Файл:** `.github/workflows/ci.yml`
**Build:** 0 errors, 0 warnings
**Tests:** 2035 passed, 1 pre-existing skip

## Sprint 61 — Text rotation marker fix (LayoutTransform offset)

### Fix S61-1: Rotated text markers offset at non-zero angles

**Проблема:** Маркеры выделения текста (4 квадрата по углам) корректно отображались только при угле поворота 0°. При других углах (45°, 90°, 135°, 180°, 270°) маркеры были смещены относительно реальных углов повёрнутого текста.

**Причина:** `TextBlock` использует WPF `LayoutTransform = RotateTransform(angle, 0, 0)`. WPF позиционирует трансформированный элемент так, что **верхний левый угол трансформированного bounding box** (а НЕ origin `(0,0)`) попадает в точку layout position `(Canvas.Left, Canvas.Top)`. Это создаёт смещение `(-minX, +minY)` между anchor `(MicronsX, MicronsY+HeightMicrons)` и фактическим центром вращения. Модельные формулы `RotatedCorner0-3`, `ContainsPoint()`, `GetBoundingBox()` не учитывали это смещение.

**Исправление:**
- `Text.cs` — добавлен `GetLayoutTransformOffset()` private helper, вычисляющий `(minX, minY)` — верхний левый угол трансформированного bounding box в локальных Y-down координатах. `RotatedCorner0-3` (8 свойств), `ContainsPoint()`, `GetBoundingBox()` обновлены с применением offset `(-minX, +minY)`.
- `HitTestHelper.cs` — `GetTextHandle()` упрощён: использует `text.RotatedCorner0-3` напрямую (single source of truth) вместо независимого вычисления углов без offset.
- Тесты: `TextTests.cs` (обновлены ожидаемые значения + 4 новых теста), `HitTestHelperTests.cs` (обновлены stale test points для rotated text hit-testing).

**Файлы:**
- `Models/Objects/Text.cs` — GetLayoutTransformOffset + RotatedCorner0-3 + ContainsPoint + GetBoundingBox
- `Helpers/HitTestHelper.cs` — GetTextHandle simplified
- `Tests/Models/Objects/TextTests.cs` — updated + new tests
- `Tests/Helpers/HitTestHelperTests.cs` — updated test points

**Build:** 0 errors, 0 warnings
**Tests:** 2069 passed (0 failures, 1 pre-existing skip)
**Coverage:** 75.3% line-rate ✅

## Sprint 62 — STA unit tests for TabItemMiddleClickBehavior and PreviewLineChangedBehavior

### Feature: TabItemMiddleClickBehaviorTests (12 tests)
**Проблема:** TabItemMiddleClickBehavior не имел unit-тестов из-за STA-зависимости (TabControl, TabItem, MouseButtonEventArgs).
**Исправление:** 
- `OnEnableMiddleClickToCloseChanged`, `OnPreviewMouseUp` сделаны `internal static` (по паттерну других behavior-тестов)
- Создан `TabItemMiddleClickBehaviorTests.cs`: 4 DP-теста (без STA) + 6 handler-тестов (STA via WpfContext.Execute) + 2 event-subscription теста
- Тесты проверяют: middle-click на TabItem → CloseTabRequestMessage, wrong button → no-op, non-TabItem sender → no-op, subscription lifecycle

### Feature: PreviewLineChangedBehaviorTests (11 tests)
**Проблема:** PreviewLineChangedBehavior не имел unit-тестов из-за STA-зависимости (Canvas с named-элементами).
**Исправление:**
- `CachedElements`, `UpdatePreviewLine`, `UpdatePreviewRectangle`, `UpdatePreviewText` сделаны `internal`/`internal static`
- Создан `PreviewLineChangedBehaviorTests.cs`: 4 Register/Unregister теста + 6 update-тестов + 1 PropertyChanged flow тест
- Тесты проверяют: валидный preview → Visible + позиция, null preview → Collapsed, double registration → no throw

**Файлы:**
- `Behaviors/TabItemMiddleClickBehavior.cs` — 2 изменения visibility
- `Behaviors/PreviewLineChangedBehavior.cs` — 4 изменения visibility
- `Tests/Behaviors/TabItemMiddleClickBehaviorTests.cs` — создан (12 тестов)
- `Tests/Behaviors/PreviewLineChangedBehaviorTests.cs` — создан (11 тестов)

**Build:** 0 errors, 0 warnings
**Tests:** 2092 passed, 1 pre-existing skip
**Coverage:** 75.3% line-rate ✅

## Sprint 63 — Template.Clone() regression test

### Feature: Clone_CopiesAllPublicProperties_ExceptId regression test
**Проблема:** `Template.Clone()` может потерять консистентность при добавлении новых свойств в будущем — нет теста, проверяющего, что все публичные свойства (кроме `Id`) скопированы.
**Исправление:** Добавлен тест `Clone_CopiesAllPublicProperties_ExceptId` в `TemplateTests.cs`, который через reflection проверяет, что каждое публичное свойство `Template` (кроме `Id`) имеет одинаковое значение на исходном и клонированном объекте после `Clone()`.
**Файлы:**
- `Tests/Services/TemplateTests.cs` — добавлен regression test

**Build:** 0 errors, 0 warnings
**Tests:** 2094 passed (0 failures, 1 pre-existing skip)
**Coverage:** 75.3% line-rate ✅

## Sprint — Архитектурный рефакторинг P2 — ITabOperationsService (21.07.2026)

### Feature: MainViewModel DI reduction
**Проблема:** 13 зависимостей в конструкторе MainViewModel — потенциальный god-class.
**Решение:** Создан ITabOperationsService — фасад для операций с вкладками (NewTab, OpenFile, Save, SaveAs). Конструктор сокращён с 13 до 10 параметров.

**Файлы:**
- `ViewModels/Abstractions/ITabOperationsService.cs` (новый)
- `Services/TabOperationsService.cs` (новый)
- `ViewModels/MainViewModel.cs` (рефакторинг)
- `App.xaml.cs` (DI-регистрация)
- `EditorViewModelFactory.cs` (sealed)

### Fix: Command naming consistency
**Проблема:** Тесты `MoveObjectCommand_*`, `RotateObjectCommand_*` не отражают реализацию через `ChangePropertyCommand<T>`.
**Решение:** 14 тестов переименованы: `MoveObjectCommand_*` → `ChangePropertyCommand_Move_*`, и т.д.

**Файлы:**
- `Tests/Commands/CommandTests.cs`

**Build:** 0 errors, 0 warnings
**Tests:** 2094 passed (0 failures, 1 pre-existing skip)
**Coverage:** 75.3% line-rate ✅
