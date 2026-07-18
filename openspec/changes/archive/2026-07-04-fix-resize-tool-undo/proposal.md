## Why

`ResizeTool.OnMouseUp()` передаёт один и тот же `ResizeState` как `oldValue` и `newValue` в `ChangePropertyCommand`, из-за чего Undo не отменяет изменение размера — команда всегда no-op. Кроме того, лямбда `s => _resizedObject.ApplyResize(s)` захватывает поле `_resizedObject` по ссылке, а после `_resizedObject = null` вызов Undo падает с `NullReferenceException`.

Нажмите Ctrl+Z после ресайза — получите исключение. Если исключение перехвачено (rollback в CommandHistory), размер не откатывается.

## What Changes

- `ResizeTool.OnMouseDown()` — сохранять `_initialState = _resizedObject.CaptureResizeState()` для использования как `oldValue` в Undo-команде
- `ResizeTool.OnMouseUp()` — захватывать `_finalState` отдельно, использовать два разных `ResizeState` в `ChangePropertyCommand`, лямбду замкнуть на локальную переменную
- `ChangePropertyCommand<ResizeState>` — без изменений (API корректен, проблема в caller)
- Тесты — добавить тест на round-trip через реальный `ResizeTool` или `CommandHistory.Push`

## Capabilities

### New Capabilities
- `resize-undo`: Корректный Undo/Redo для операции изменения размера объектов (Rectangle, Line, Text) через ResizeTool

### Modified Capabilities

(нет изменений существующих spec-уровней)

## Impact

- `Tools/ResizeTool.cs` — поля `_initialState` (новое), доработка `OnMouseDown`/`OnMouseUp`
- `Tests/Tools/ResizeToolTests.cs` — добавить тест Undo после реального ресайза
