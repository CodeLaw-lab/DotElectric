# Отчёт — Исправление Selection Box

**Дата:** 07.04.2026
**Задача:** Исправить позиционирование рамки выделения (Selection Box)

---

## ПРОБЛЕМЫ

### Проблема 1: HeightMicrons вместо HeightMm в binding
**Файл:** `Views/EditorCanvas.xaml`

Binding использовал `Template.Sheet.HeightMicrons` (тип `long`), но `ModelYToCanvasTopConverter` ожидает `double` (мм):

```csharp
if (values[1] is not double sheetHeightMm) return 0.0;
```

`long` не соответствует `double` → конвертер возвращает `0.0` → `Canvas.Top = 0` → рамка всегда вверху Canvas.

### Проблема 2: SelectionBoxBottom вместо SelectionBoxTop
**Файлы:** `Views/EditorCanvas.xaml`, `ViewModels/EditorViewModel.cs`

`SelectionBoxBottom` = минимальный Y (нижний край). `Canvas.Top` в WPF = позиция **верхнего** края элемента. Привязка к нижнему краю давала смещение рамки вверх.

### Проблема 3: Двойной клик «съедал» каждый второй клик
**Файл:** `Behaviors/EditorCanvasBehavior.cs`

Ручное обнаружение двойного клика (Stopwatch + ClickCount) приводило к тому, что каждый второй клик в пределах 500мс определялся как двойной и вызывал `OnDoubleClick` вместо `OnMouseDown` — выделение не работало.

---

## РЕШЕНИЯ

### Решение 1: HeightMm в binding
**Файл:** `EditorCanvas.xaml`

```xml
<!-- БЫЛО -->
<Binding Path="Template.Sheet.HeightMicrons"/>
<!-- СТАЛО -->
<Binding Path="Template.Sheet.HeightMm"/>
```

### Решение 2: Добавлены SelectionBoxTop и SelectionBoxRight
**Файл:** `EditorViewModel.cs`

```csharp
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(SelectionBoxTop))]
private long _selectionBoxLeft;

[ObservableProperty]
[NotifyPropertyChangedFor(nameof(SelectionBoxTop))]
private long _selectionBoxBottom;

[ObservableProperty]
[NotifyPropertyChangedFor(nameof(SelectionBoxTop))]
private long _selectionBoxHeight;

public long SelectionBoxTop => SelectionBoxBottom + SelectionBoxHeight;

[ObservableProperty]
[NotifyPropertyChangedFor(nameof(SelectionBoxRight))]
private long _selectionBoxWidth;

public long SelectionBoxRight => SelectionBoxLeft + SelectionBoxWidth;
```

**Файл:** `EditorCanvas.xaml`

```xml
<Binding Path="SelectionBoxTop"/>   // вместо SelectionBoxBottom
```

### Решение 3: Использовать WPF e.ClickCount вместо ручного таймера
**Файл:** `EditorCanvasBehavior.cs`

**Было:**
```csharp
// Ручной таймер — каждый чётный клик = двойной
var isDoubleClick = state.LastClickTimer.ElapsedMilliseconds < 500 && state.ClickCount == 1;
if (isDoubleClick) { tool.OnDoubleClick(...); return; }
tool.OnMouseDown(...);
```

**Стало:**
```csharp
if (e.ClickCount >= 2 && e.ChangedButton == MouseButton.Left)
    tool.OnDoubleClick(modelPt);
else
    tool.OnMouseDown(modelPt, e.ChangedButton, Keyboard.Modifiers);
```

Убраны поля `Stopwatch LastClickTimer` и `int ClickCount` из `EditorCanvasState`.

---

## ИЗМЕНЁННЫЕ ФАЙЛЫ

| Файл | Изменение |
|------|-----------|
| `Views/EditorCanvas.xaml` | `HeightMicrons` → `HeightMm`, `SelectionBoxBottom` → `SelectionBoxTop` |
| `ViewModels/EditorViewModel.cs` | Добавлены `SelectionBoxTop`, `SelectionBoxRight` с `NotifyPropertyChangedFor` |
| `Behaviors/EditorCanvasBehavior.cs` | Ручной таймер двойного клика заменён на `e.ClickCount` |
| `Tools/SelectTool.cs` | (без изменений, только убран отладочный вывод) |
| `Tools/ToolTests.cs` | Обновлены тесты TextTool (OnMouseDown → OnMouseUp для финализации) |

---

## РЕЗУЛЬТАТ

- **Сборка:** ✅ без ошибок, 0 предупреждений
- **Тесты:** ✅ 801/801 прошли (0 сбоев)
- Рамка выделения появляется **точно** под курсором мыши
- Каждый клик обрабатывается (выделение работает стабильно)

---

**Документ подготовил:** AI-ассистент DotElectric
**Дата:** 07.04.2026
