## Context

`ResizeTool` содержит две ошибки, из-за которых Undo после изменения размера объекта падает с NullReferenceException, а в случае перехвата исключения — ничего не отменяет.

Текущий поток:

```
OnMouseDown                     OnMouseUp
    │                               │
    ├─ _startX/Y/W/H = ...         ├─ var state = CaptureResizeState()
    │  (поля, только для           ├─ new ChangePropertyCommand(
    │   расчёта дельты)             │      state,  ← oldValue И newValue — один объект
    │                               │      s => _resizedObject.ApplyResize(s),
    │  ... resize logic ...         │      state,
    │                               │      "размер", ...)
    │                               ├─ _resizedObject = null  ← лямбда видит null
```

`ChangePropertyCommand<T>` сам по себе корректен — тесты подтверждают, что с разными old/new значениями Undo работает. Проблема в caller.

## Goals / Non-Goals

**Goals:**
- Undo после ресайза Rectangle/Line/Text отменяет изменение размера (возвращает исходные координаты/размеры)
- Redo повторяет ресайз
- Никаких NullReferenceException при Undo/Redo

**Non-Goals:**
- Рефакторинг `ChangePropertyCommand<T>` (он не требуется)
- Изменение схемы Undo/Redo для других инструментов (SelectTool.Move — отдельная история)
- Изменение `ResizeState` record

## Decisions

### 1. Сохранять `_initialState` в OnMouseDown вместо OnMouseUp

**Решение:** Добавить поле `_initialState: ResizeState?` в `ResizeTool`. В `OnMouseDown()` после `_resizedObject = ...` сохранять `_initialState = _resizedObject.CaptureResizeState()`.

**Альтернатива:** Захватывать оба состояния в OnMouseUp — первое через повторный CaptureResizeState после сброса объекта в начальное состояние. Отвергнуто: мутация объекта для снятия слепка — опасный побочный эффект.

### 2. Лямбда захватывает локальную переменную, не поле

**Решение:** В `OnMouseUp()` замкнуть `s => captured.ApplyResize(s)` на локальную `var captured = _resizedObject`.

**Альтернатива:** Снимать `_resizedObject` с nulling-защитой внутри лямбды. Отвергнуто: костыль, маскирующий проблему.

### 3. Два разных ResizeState: old = _initialState, new = _finalState

**Решение:** В `OnMouseUp()`:
```csharp
var finalState = _resizedObject.CaptureResizeState();  // текущее (new)
var captured = _resizedObject;
var cmd = new ChangePropertyCommand<ResizeState>(
    _initialState!,     // сохранённое в OnMouseDown (old)
    s => captured.ApplyResize(s),
    finalState,         // текущее (new)
    "размер",
    _context.MarkDirty);
```

Новый поток:

```
OnMouseDown                         OnMouseUp
    │                                   │
    ├─ _initialState =                  ├─ finalState = CaptureResizeState()
    │    CaptureResizeState()           ├─ cmd = new ChangePropertyCommand(
    │                                   │      _initialState,   ← old
    │  ... resize logic (мутирует       │      s => captured.ApplyResize(s),
    │   _resizedObject in-place) ...    │      finalState,      ← new
    │                                   │      "размер", ...)
    │                                   ├─ Push(cmd) → Execute() → finalState
    │                                   ├─ _resizedObject = null
```

## Risks / Trade-offs

- **[_initialState null при повторном вызове OnMouseUp без OnMouseDown] →** Защита: `if (!_isResizing || _resizedObject == null) return;` уже есть в OnMouseUp, плюс `_initialState` всегда устанавливается при входе в OnMouseDown. Если `_initialState` всё же null — `_initialState!` кинет meaningful exception (должен быть non-null).
- **[_initialState устарел при мульти-редакции] →** ResizeTool — одноразовый инструмент. После PopTool() он не используется до следующего Push. Конфликта нет.
- **[(null forgiving operator) `_initialState!`] →** Можно заменить на `_initialState ?? throw new InvalidOperationException(...)`, но `!` лаконичнее и семантически верен (postcondition OnMouseDown).

## Open Questions

- Нужен ли тест на `CommandHistory.Push` через реальный `ResizeTool`? Да — добавить интеграционный тест, проверяющий Undo после полного цикла OnMouseDown → OnMouseMove → OnMouseUp.
