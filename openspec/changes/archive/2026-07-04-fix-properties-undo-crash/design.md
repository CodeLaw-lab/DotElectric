## Context

Та же проблема, что в `ResizeTool` (change `fix-resize-tool-undo`), но в PropertiesViewModels. Лямбды, переданные в `ChangePropertyCommand<T>`, захватывают поле `_line`/`_rect`/`_text` по ссылке. После вызова `UpdateObject(null)` поле обнуляется, а команда остаётся в undo-стеке.

Текущий поток:

```
Пользователь меняет LineType
  → ChangeLineType(value)
    → guard: _line != null ✓
    → SetProperty(value,
          () => _line.LineType,    // захват ПОЛЯ _line
          v => _line.LineType = v, // захват ПОЛЯ _line
          ...)
    → ChangePropertyCommand { _oldValue = _line.LineType, _setter = ... }
    → Push(cmd) → Execute() → _line.LineType = value ✓

Пользователь выделяет другой объект
  → UpdateObject(null)
    → _line = null

Пользователь жмёт Ctrl+Z
  → CommandHistory.Undo()
    → _setter(_oldValue) → null.LineType = ... → 💥 NRE
```

## Goals / Non-Goals

**Goals:**
- Undo после изменения любого свойства через панель свойств не падает с NRE
- Undo корректно восстанавливает предыдущее значение
- Redo корректно применяет новое значение

**Non-Goals:**
- Рефакторинг `SetProperty` или `ChangePropertyCommand<T>` (они корректны)
- Изменение поведения при активном выделении (гарантия `_line != null` в момент вызова команды сохраняется)
- Другие сценарии Undo (покрыты change `fix-resize-tool-undo`)

## Decisions

### 1. Локальная переменная вместо поля в лямбдах

**Решение:** В каждом relay-команде перед вызовом `SetProperty` сохранять `var line = _line;` и использовать `line` в лямбдах.

```csharp
// Было:
private void ChangeLineType(LineType value)
{
    if (_line is null) return;
    SetProperty(value, () => _line.LineType, v => _line.LineType = v,
        null, nameof(LineTypeValue), "Тип линии");
}

// Стало:
private void ChangeLineType(LineType value)
{
    if (_line is null) return;
    var line = _line;
    SetProperty(value, () => line.LineType, v => line.LineType = v,
        null, nameof(LineTypeValue), "Тип линии");
}
```

**Альтернатива:** Изменить `SetProperty` на захват объекта целиком — потребует смены сигнатуры и всех call sites. Неоправданно для batch-фикса.

### 2. Inline-команды в TextPropertiesViewModel — тот же подход

**Решение:** Три места, где `ChangePropertyCommand` создаётся вручную (без `SetProperty`):
- `ChangeContent`
- `ChangeDefaultValue`
- `ChangeFontNameFromString`

Применить тот же паттерн: `var text = _text;` перед созданием команды.

## Risks / Trade-offs

- **[Пропущенный call site] →** 28 мест, легко пропустить одно. Минимизация: свести изменения к механическому паттерну, проверить grep по `_line\.` / `_rect\.` / `_text\.` после фикса.

## Open Questions

- Нужен ли тест на каждый call site? Нет, достаточно одного теста на VM, проверяющего Undo после деселекта + смены выделения.
