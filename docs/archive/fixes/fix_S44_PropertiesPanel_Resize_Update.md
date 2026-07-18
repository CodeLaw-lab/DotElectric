# Sprint 44 — PropertiesPanel live update after resize

**Дата:** 25.06.2026
**Статус:** Готово
**Build:** 0 errors (5 pre-existing warnings)
**Tests:** 1287+ пройдены (0 failures)

---

## Fix S44-1: PropertiesViewModel не подписан на INPC объекта

### Проблема

`PropertiesViewModel` не подписывался на `INotifyPropertyChanged.PropertyChanged` выделенного объекта. При изменении размеров через `ResizeTool` модель оповещала (`OnPropertyChanged`), но ViewModel не перезапрашивала свои computed-свойства (`RectX`, `LineEndX`, `TextFontSize` и т.д.). WPF-биндинги на панели свойств не обновлялись.

**Цепочка (было):**
```
ResizeTool.OnMouseMove
  → rect.MicronsX = newX       (Rectangle.cs setter)
    → OnPropertyChanged()       ✓ FIRES on model
      → ???                     ✗ NOBODY LISTENS
        → OnPropertyChanged(nameof(RectX))  ✗ NEVER CALLED
          → WPF Binding update              ✗ NEVER HAPPENS
```

### Исправление

**Файл:** `ViewModels/PropertiesViewModel.cs`

1. Добавлено поле `_previousSelectedObject` для отслеживания текущего объекта подписки
2. `UpdateSelection()` — при смене выделения:
   - Отписывается от старого объекта: `oldInpc.PropertyChanged -= OnSelectedObjectPropertyChanged`
   - Подписывается на новый: `newInpc.PropertyChanged += OnSelectedObjectPropertyChanged`
3. `OnSelectedObjectPropertyChanged()` — маппинг имён свойств модели на ViewModel-свойства:

| Модель | Property | ViewModel |
|--------|----------|-----------|
| Line | `StartMicronsX` → | `LineStartX` |
| Line | `StartMicronsY` → | `LineStartY` |
| Line | `EndMicronsX` → | `LineEndX` |
| Line | `EndMicronsY` → | `LineEndY` |
| Line | `LineType` → | `LineTypeValue` |
| Line | `StrokeThicknessMicrons` → | `LineStrokeThickness` |
| Rectangle | `MicronsX` → | `RectX` |
| Rectangle | `MicronsY` → | `RectY` |
| Rectangle | `WidthMicrons` → | `RectWidth` |
| Rectangle | `HeightMicrons` → | `RectHeight` |
| Rectangle | `LineType` → | `RectLineType` |
| Rectangle | `StrokeThicknessMicrons` → | `RectStrokeThickness` |
| Text | `MicronsX` → | `TextX` |
| Text | `MicronsY` → | `TextY` |
| Text | `Content` → | `TextContent` |
| Text | `FontSizeMicrons` → | `TextFontSize` |
| Text | `FontName` → | `TextFontName` |
| Text | `TextType` → | `TextTypeValue` |
| Text | `RotationAngle` → | `TextRotation` |

4. `Dispose()` — гарантированная отписка.

Для свойств с одинаковыми именами в разных моделях (`LineType`, `StrokeThicknessMicrons`, `MicronsX`, `MicronsY`) используется `when sender is Type` в switch.

**Цепочка (стало):**
```
ResizeTool.OnMouseMove
  → rect.MicronsX = newX
    → Rectangle.OnPropertyChanged("MicronsX")   ✓ FIRES
      → PropertiesViewModel.OnSelectedObjectPropertyChanged
        → OnPropertyChanged("RectX")            ✓ FIRES
          → WPF Binding update                  ✓ UPDATES
```

---

## Fix S44-2: Text INPC для всех свойств

### Проблема

`Text.FontSizeMicrons`, `Content`, `FontName`, `TextType`, `RotationAngle` были auto-properties — не оповещали об изменениях. Даже с подпиской PropertiesViewModel на `PropertyChanged` (Fix S44-1), эти свойства не оповещали об изменениях, поэтому панель свойств не обновлялась.

### Исправление

**Файл:** `Models/Objects/Text.cs:53-110`

| Свойство | Было | Стало |
|----------|------|-------|
| `Content` | auto-property | backing field + OnPropertyChanged + уведомления WidthMicrons, RightMicronsX, CenterMicronsX |
| `FontSizeMicrons` | auto-property | backing field + OnPropertyChanged + уведомления WidthMicrons, RightMicronsX, BottomMicronsY, CenterMicronsX, CenterMicronsY |
| `FontName` | auto-property | backing field + OnPropertyChanged |
| `TextType` | auto-property | backing field + OnPropertyChanged |
| `RotationAngle` | backing field (set только throw) | + guard + OnPropertyChanged + уведомление RotationAngleValid |

---

## Файлы

| Файл | Изменения |
|------|-----------|
| `ViewModels/PropertiesViewModel.cs:109-210` | Подписка на INPC объекта, маппинг свойств, отписка в Dispose |
| `Models/Objects/Text.cs:53-110` | Все свойства переведены на backing fields + INPC |
