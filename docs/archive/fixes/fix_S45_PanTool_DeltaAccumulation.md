# Fix S45 — PanTool: Delta accumulation drift (RenderTransform)

## Проблема: Дрифт дельты панорамирования

**Корень:** `EditorCanvasBehavior.State_MouseMove()` вычислял дельту панорамирования из `e.GetPosition(canvas)`, который **учитывает `RenderTransform`** canvas'а (`TranslateTransform CanvasOffsetX/Y`). После каждого `MouseMove` canvas сдвигался (через `PanCanvas`), и на следующем `MouseMove` `e.GetPosition(canvas)` возвращал координаты, уже включающие предыдущий сдвиг. Это приводило к **накоплению ошибки**: каждое движение мыши добавляло дельту предыдущего пана, и панорамирование неконтролируемо ускорялось (`runaway pan`).

### Схема накопления:
1. `MouseDown` → `e.GetPosition(canvas)` = (500, 300), пана ещё нет
2. `MouseMove` к (510, 300) → `e.GetPosition(canvas)` = (510, 300) → дельта = 10px → `PanCanvas(10)` → `CanvasOffset = -10` → canvas сдвинут влево на 10px
3. `MouseMove` к (515, 300) → `e.GetPosition(canvas)` = (515+10, 300) = (525, 300) — **лишние +10px от RenderTransform!** → дельта = 15px вместо 5px → `PanCanvas(15)`
4. На третьем `MouseMove` ошибка ещё больше — каждая итерация добавляет предыдущую дельту заново

## Исправление: Стабильная система координат для дельты

**Решение:** Использовать **Window-координаты** (`e.GetPosition(window)`) для вычисления дельты мыши вместо canvas-координат. Window не имеет `RenderTransform`, поэтому его координаты не меняются при сдвиге canvas'а.

### Изменения:

**Файл:** `Behaviors/EditorCanvasBehavior.cs`

1. **`EditorCanvasState`** — добавлены поля:
   - `PanStartWpfPoint` (Point) — стартовая позиция мыши в Window-координатах
   - `PanAppliedModelDelta` (Point) — уже применённая суммарная дельта в мм, для вычисления инкрементальной части

2. **`State_MouseDown()`** (пути средней кнопки и Space/Alt+Left):
   - Сохраняет `PanStartWpfPoint = e.GetPosition(window)` (если window != null)
   - Сбрасывает `PanAppliedModelDelta = default`
   - Остальная логика (`panTool.OnMouseDown`, `IsPanning`) без изменений

3. **`State_MouseMove()`** (путь панорамирования):
   - Вычисляет `deltaPx = e.GetPosition(window) - state.PanStartWpfPoint`
   - Конвертирует в мм: `totalModelDelta = (deltaPx.X / zoom, -deltaPx.Y / zoom)` (Y-инверсия WPF→модель)
   - Вычисляет инкрементальную дельту: `incremental = totalModelDelta - state.PanAppliedModelDelta`
   - Вызывает `state.Editor.PanCanvas(incremental.X, incremental.Y)`
   - Обновляет `state.PanAppliedModelDelta = totalModelDelta`

4. **`State_MouseUp()`** (завершение панорамирования):
   - Сбрасывает `state.PanAppliedModelDelta = default`

### Почему это работает:

- Window-координаты **не зависят от RenderTransform** canvas'а — при сдвиге canvas'а Window остаётся неподвижным
- `deltaPx = currentWindowPos - startWindowPos` = чистому перемещению мыши в пикселях
- Конверсия пикселей в мм через Zoom даёт корректную дельту для `PanCanvas()`
- Инкрементальный подход (total - applied) позволяет каждому `MouseMove` применять только новую часть дельты

## Проверка

- **Build:** 0 errors (5 pre-existing warnings)
- **Tests:** PanTool 13/13, EditorCanvas/ZoomPan 10/10 — все пройдены
