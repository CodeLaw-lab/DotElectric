# Sprint 38 — LineType панели свойств + Undo + координаты

**Дата:** 24.06.2026
**Ветка:** —
**Статус:** Готово

---

## Список исправлений

### Fix S38-1: ComboBox типа линии не отображает текущее значение

**Проблема:** ComboBox в панели свойств для типа линии и прямоугольника не имел биндинга `SelectedItem`/`SelectedIndex`. При выборе объекта ComboBox оставался пустым (или показывал предыдущее выбранное значение). Изменение типа работало только в одну сторону: UI → модель через `SelectionChangedCommand` без обратной синхронизации.

**Исправление:**
- Создан `Converters/LineTypeToIndexConverter.cs` — IValueConverter (`LineType?` → `int`)
- Добавлен `SelectedIndex="{Binding LineTypeValue, Converter=..., Mode=OneWay}"` для Line ComboBox
- Добавлен `SelectedIndex="{Binding RectLineType, Converter=..., Mode=OneWay}"` для Rectangle ComboBox
- Конвертер зарегистрирован в `App.xaml`

**Файлы:**
- `Converters/LineTypeToIndexConverter.cs` (новый)
- `App.xaml` (регистрация конвертера)
- `Views/PropertiesPanelContent.xaml` (SelectedIndex на обоих ComboBox)

---

### Fix S38-2: Изменение LineType не перерисовывает канвас

**Проблема:** `Line` и `Rectangle` не реализовывали `INotifyPropertyChanged`. При мутации `line.LineType` через `ChangePropertyCommand` WPF-биндинг `StrokeDashArray="{Binding LineType, Converter=...}"` в DataTemplate не получал уведомления — визуал линии/прямоугольника на канвасе не обновлялся.

**Исправление:**
- `Line.cs`: implements `INotifyPropertyChanged`, `LineType` → backing field + `OnPropertyChanged()`
- `Rectangle.cs`: implements `INotifyPropertyChanged`, `LineType` → backing field + `OnPropertyChanged()`

**Файлы:**
- `Models/Objects/Line.cs`
- `Models/Objects/Rectangle.cs`

---

### Fix S38-3: DrawingRectangleTool не передаёт _lineType

**Проблема:** `CalculateRectangle()` создавал `new Rectangle(x, y, w, h)` без параметра `_lineType` — итоговый прямоугольник всегда получал `LineType.Solid` независимо от `_lineType`.

**Исправление:** `CalculateRectangle()` теперь принимает `lineType` и передаёт его в конструктор: `new Rectangle(x, y, w, h, lineType)`. Вызовы обновлены.

**Файлы:**
- `Tools/DrawingRectangleTool.cs`

---

### Fix S38-4: Изменение координат не перерисовывает канвас

**Проблема:** При редактировании X1/Y1/X2/Y2 линии (и X/Y/Width/Height прямоугольника) через панель свойств, `ChangePropertyCommand` мутировал model-свойства без INotifyPropertyChanged. Канвас не перерисовывал линию/прямоугольник — обновлялись только текстовые поля в панели свойств.

**Исправление:**
- `Line.cs`: `StartMicronsX/Y`, `EndMicronsX/Y` → backing fields + `OnPropertyChanged()`
- `Rectangle.cs`: `WidthMicrons`, `HeightMicrons` → backing fields + `OnPropertyChanged()`. Добавлены override `MicronsX`, `MicronsY` с INPC.
- `Rectangle.cs`: сеттеры `MicronsX/WidthMicrons` уведомляют `RightMicronsX` + `CenterMicronsX`; сеттеры `MicronsY/HeightMicrons` уведомляют `BottomMicronsY` + `CenterMicronsY` (для обновления маркеров выделения).

**Файлы:**
- `Models/Objects/Line.cs`
- `Models/Objects/Rectangle.cs`

---

### Fix S38-5: Enter не коммитит поля ввода координат

**Проблема:** Все TextBox'ы координат используют `TextBoxLostFocusCommandBehavior`, который выполняет команду только при `LostFocus`. Enter никак не обрабатывался — нужно было уводить фокус (Tab/клик) для применения значения.

**Исправление:** В `TextBoxLostFocusCommandBehavior` добавлен обработчик `KeyDown`. При `Key.Enter` выполняется та же команда с текущим текстом TextBox. `e.Handled = true` предотвращает всплытие.

**Файлы:**
- `Behaviors/TextBoxLostFocusCommandBehavior.cs`

---

### Fix S38-6: Undo удаляет объект, но выделение остаётся

**Проблема:** При Undo (Ctrl+Z) `AddObjectCommand.Undo()` удаляет объект из `Template.Objects`, но `SelectedObjects` не очищается. Объект остаётся «висячей» ссылкой — маркеры выделения продолжают рендериться для удалённого объекта.

**Исправление:** В `EditorViewModel.Undo()` и `Redo()` добавлен вызов `PurgeOrphanedSelection()` — удаляет из `SelectedObjects` все объекты, отсутствующие в `Template.Objects`.

**Файлы:**
- `ViewModels/EditorViewModel.cs`

---

## Итог

| Метрика | До | После |
|---------|----|-------|
| **Build** | 0 errors, 4 warnings | 0 errors, 4 warnings |
| **Tests** | 465+ passed | 589+ passed |

**Изменённые файлы (9):**
- `Converters/LineTypeToIndexConverter.cs` (новый)
- `App.xaml`
- `Views/PropertiesPanelContent.xaml`
- `Models/Objects/Line.cs`
- `Models/Objects/Rectangle.cs`
- `Tools/DrawingRectangleTool.cs`
- `Behaviors/TextBoxLostFocusCommandBehavior.cs`
- `ViewModels/EditorViewModel.cs`
