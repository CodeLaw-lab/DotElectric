# S37: Двойная компенсация PanOffset в ToModelPoint + визуальное выделение объектов

**Дата:** 23.06.2026
**Статус:** Исправлено ✅
**Связанные файлы:**
- `Behaviors/EditorCanvasBehavior.cs` (ToModelPoint)
- `ViewModels/EditorViewModel.cs` (SelectionVersion, IsObjectSelected)
- `Converters/IsObjectSelectedConverter.cs` (новый)
- `Views/EditorCanvas.xaml` (DataTriggers)
- `Tools/DrawingLineTool.cs` (Preview re-assign)
- `Tools/DrawingRectangleTool.cs` (Preview re-assign)
- `Tools/TextTool.cs` (Escape → Select)

---

## Проблема

### 1. Клик по объекту не приводит к выделению

При клике мышью по линии, прямоугольнику или тексту с активным SelectTool выделение не происходило.

**Корневая причина:** `EditorCanvasBehavior.ToModelPoint()` дважды вычитал смещение панорамирования:

```csharp
// Было: e.GetPosition(canvas) уже учёл RenderTransform = TranslateTransform(-PanOffsetX, 0)
// Повторное вычитание PanOffsetX смещало координаты на величину панорамирования
var adjustedX = (wpfPoint.X - editor.PanOffsetX) / zoom;
var adjustedY = (wpfPoint.Y - editor.PanOffsetY) / zoom;
```

`e.GetPosition(canvas)` возвращает координаты в локальном пространстве Canvas **после** применения `RenderTransform`. Поскольку `RenderTransform` использует `CanvasOffset = -PanOffset`, точка уже смещена. Вычитание `PanOffset` ещё раз приводило к ошибке, равной `PanOffset / zoom` мм. При типичном панорамировании на 100–500 px ошибка могла составлять десятки–сотни мм, и HitTest не находил объект.

### 2. Отсутствует визуальная обратная связь выделения

У объектов не было визуального состояния «выделен». `SingleSelectedObject` (маркеры) показывался, но при мульти-выделении (рамкой) или одиночном клике внешний вид объектов не менялся — пользователь не видел, что объект выделен.

### 3. Preview-фигуры не отображались при рисовании

См. `fix_C1` / `fix_C2` — ре-ассайн ссылки после мутации свойств.

### 4. Canvas не ресайзился при зуме

`OnPropertyChanged(nameof(Zoom))` не вызывался при изменении зума через `SetZoom` / `ZoomIn` / `ZoomOut`.

### 5. Escape не переключал на Select tool

Escape в инструментах рисования / текста очищал состояние, но не активировал SelectTool.

---

## Решение

### 1. ToModelPoint — `EditorCanvasBehavior.cs:269-270`

```csharp
// Было
var adjustedX = (wpfPoint.X - editor.PanOffsetX) / zoom;
var adjustedY = (wpfPoint.Y - editor.PanOffsetY) / zoom;

// Стало
var adjustedX = wpfPoint.X / zoom;
var adjustedY = wpfPoint.Y / zoom;
```

`e.GetPosition(canvas)` уже возвращает координаты после учёта `RenderTransform` (сдвиг на `CanvasOffset = -PanOffset`). Повторное вычитание не требуется. Исправление восстанавливает корректный round-trip: модель → WPF (через конвертеры `Canvas.Left`/`Top`) → обратно в модель (через `ToModelPoint`).

### 2. Визуальное выделение — `EditorViewModel.cs`, `IsObjectSelectedConverter.cs`, `EditorCanvas.xaml`

- Добавлен `SelectionVersion` (int), инкрементируемый при каждом изменении выделения
- `OnSelectionChangedInternal()` теперь уведомляет `SelectionVersion`
- Создан `IsObjectSelectedConverter` (IMultiValueConverter) — проверяет вхождение объекта в `SelectedObjects`
- DataTrigger'ы в DataTemplate'ах `Line`, `Rectangle`, `Text`:
  - Stroke → `#0078D4` (синий акцент)
  - StrokeThickness → 2px
  - Текст: Foreground → синий, FontWeight → Bold, Background → `#E0F0FF`

### 3–5: Мелкие исправления

- Preview re-assign после мутации (DrawingLineTool, DrawingRectangleTool)
- `OnPropertyChanged(nameof(Zoom))` в `OnZoomChangedInternal()`
- `ActiveTool = "Select"` после `Reset()` (DrawingLineTool, DrawingRectangleTool, TextTool)

---

## Тесты

**Build:** 0 errors, 4 warnings (pre-existing: CS0114 Clone, CS8618 Id)
**Tests:** 465+ пройдено:
- EditorViewModel: 112
- Integration: 49
- SelectTool: 18
- ZoomPanManager: 10
- Converter: 156
- HitTest: 120

Все 0 failures.
