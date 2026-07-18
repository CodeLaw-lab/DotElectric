# Fix S41 — Drag Move: Delta accumulation drift + Text INPC

## Проблема 1 (SelectTool): Дрифт дельты при перетаскивании

**Корень:** В `SelectTool.OnMouseMove()` дельта перемещения вычислялась как `obj.MicronsX + delta`, где `delta` — полное смещение мыши от точки старта. Но `obj.MicronsX` уже обновлялся на предыдущем `MouseMove`. При каждом новом `MouseMove` дельта прибавлялась к уже смещённой позиции, а не к исходной, из-за чего объект «убегал» от курсора (дрифт, пропорциональный количеству событий `MouseMove`).

**Исправление:** Дельта прибавляется к **сохранённой начальной позиции** из `_initialPositions[obj]`, а не к текущей `obj.MicronsX`.

**Файл:** `Tools/SelectTool.cs:208-210`

## Проблема 2 (Text): Нет INPC для MicronsX/MicronsY

**Корень:** `Text` наследовал `MicronsX`/`MicronsY` как auto-properties от `TemplateObjectBase` без INotifyPropertyChanged. При вызове `Text.Move(newX, newY)` свойства устанавливались, но WPF-биндинги (`Canvas.Left`/`Canvas.Top` через `LeftEdgeMicronsMultiConverter`/`TopEdgeMicronsMultiConverter`) не обновлялись — текст визуально не двигался.

**Исправление:**
- `Text` реализует `INotifyPropertyChanged`
- Добавлены backing fields `_micronsX`/`_micronsY`
- Override `MicronsX`/`MicronsY` с `OnPropertyChanged()` + уведомления для `RightMicronsX`, `BottomMicronsY`, `CenterMicronsX`, `CenterMicronsY`

**Файл:** `Models/Objects/Text.cs:12-52`

## Cleanup (SelectTool)
- Удалены мёртвые поля `_dragStartX`/`_dragStartY` (устанавливались, но не использовались)
- Упрощён расчёт дельты (оба if-else вычисляли одно и то же)

## Tests
- Build: 0 errors, 0 warnings
- All 5 SelectToolDragTests pass
- All 62 ToolTests pass
- All 163 Text-related tests pass
- All 156 Converter tests pass
