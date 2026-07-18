## Context

Session 1 прогона v6 тест-плана выявил 7 дефектов в инструментах рисования/редактирования. Все они — перечисленные в Common Mistakes баги, пропущенные в предыдущих спринтах, или неконсистентности между инструментами.

Исправления точечные: 1–5 строк на каждый дефект. Ни один не требует изменения архитектуры, моделей данных или публичного API.

## Goals / Non-Goals

**Goals:**
- TextTool preview следует за мышью (аналогично Line/Rectangle)
- Escape при Resize корректно разблокирует стек инструментов
- Смена инструмента во время рисования сбрасывает состояние предыдущего
- Shift+линия даёт 45° диагональ при равных dx/dy
- Undo удаления восстанавливает выделение объекта
- DoubleClick в Line/Text инструментах переключает на Select
- Clamp при создании прямоугольника у границы листа не даёт вылезти за край

**Non-Goals:**
- Рефакторинг архитектуры инструментов
- Изменение тест-плана
- Добавление новых фич

## Decisions

### Decision 1: TextTool preview — ре-ассайн ссылки
**Проблема:** `OnMouseMove` мутирует поля `MicronsX/Y` preview-объекта, но не переустанавливает `_context.PreviewText`. Без ре-ассайна сеттер не вызывается, `PropertyChanged` не стреляет.
**Решение:** Добавить `_context.PreviewText = _previewText;` после каждой мутации координат в `TextTool.OnMouseMove()`, аналогично `DrawingLineTool` и `DrawingRectangleTool`.

### Decision 2: ResizeTool Escape — PopTool
**Проблема:** `ResizeTool.OnKeyDown(Escape)` вызывает `Reset()`, но оставляет `"Resize"` в стеке инструментов. Все последующие события мыши уходят в мёртвый ResizeTool.
**Решение:** Добавить `_context.PopTool();` перед `Reset()` в обработчике Escape. Изменение: 1 строка.

### Decision 3: Tool switch mid-drawing — сброс состояния
**Проблема:** При смене инструмента (через `PreviewKeyDown` в `MainWindow.xaml.cs`) старый инструмент остаётся в памяти с `_startPoint != null`. Возврат на него дает ранний return в `OnMouseDown`.
**Решение:** Вызывать `Reset()` на активном инструменте через `ITool.Reset()` перед переключением. Либо создать метод `ToolManager.SwitchTo(name)`, который делает Reset текущего + устанавливает новый.

### Decision 4: DrawingLineTool 45° — третья ветка
**Проблема:** `ApplyConstraint()` имеет 2 ветки: `dx > dy → горизонталь`, `else → вертикаль`. При `dx ≈ dy` линия становится вертикальной.
**Решение:** Добавить 3-ю ветку: если `|dx - dy| < tolerance` (1 микрон), зафиксировать оба конца — диагональ 45°.

### Decision 5: Undo selection restore
**Проблема:** После Undo удаления объект восстанавливается, но не выделяется.
**Решение:** В `EditorViewModel.Undo()` после `PurgeOrphanedSelection()` проверить, был ли удалённый объект единственным выделенным, и если да — выделить его после восстановления.

### Decision 6: DoubleClick → Select
**Проблема:** `DrawingLineTool.OnDoubleClick()` и `TextTool.OnDoubleClick()` не вызывают `SetActiveToolCommand.Execute("Select")`, хотя Escape вызывает.
**Решение:** Добавить переключение на Select в OnDoubleClick обоих инструментов.

### Decision 7: Rectangle clamp at sheet edge
**Проблема:** При создании прямоугольника у правой границы `w = Math.Max(minSize, Math.Min(rect.WidthMicrons, sheetW - x))`. Если `sheetW - x < minSize`, ширина clampится к minSize, но x не сдвигается — правый край вылезает за границу.
**Решение:** Добавить сдвиг x: `if (x + w > sheetW) x = sheetW - w;`

## Risks / Trade-offs

- **[Low] Tool switch reset** — вызов `Reset()` на активном инструменте может отменить начатое рисование без ведома пользователя. Но это лучше, чем «залипший» инструмент. Текущее поведение (сломанный инструмент) — хуже.
- **[Low] Undo selection restore** — может выделить объект, который пользователь не хотел выделять. Но Undo возвращает к предыдущему состоянию, а предыдущее состояние включало выделение.
