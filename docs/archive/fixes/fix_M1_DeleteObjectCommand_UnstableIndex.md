# P2-M1: Нестабильный индекс в DeleteObjectCommand

## Анализ проблемы

**Файл:** `src/DotElectric.TemplateEditor/Commands/DeleteObjectCommand.cs:32`

**Симптом:** При отмене удаления (Undo) объект может вставиться не на ту позицию в коллекции.

**Корень:** Индекс захватывается в конструкторе:

```csharp
public DeleteObjectCommand(
    ObservableCollection<TemplateObjectBase> objects,
    TemplateObjectBase obj,
    Action? markDirty = null)
{
    _objects = objects;
    _object = obj;
    _index = _objects.IndexOf(_object);   // ← захват здесь
    _markDirty = markDirty;
}
```

Проблема: команда может быть создана за несколько шагов до вызова `Execute()`. Между конструктором и Execute коллекция может измениться (например, другой операцией). Тогда `_index` уже не актуален.

**Сценарий сбоя:**
1. Пользователь добавляет объекты A, B, C (индексы 0, 1, 2)
2. Создаётся `DeleteObjectCommand` для объекта B — `_index = 1`
3. Пользователь удаляет объект A (другой командой) — коллекция: [B, C]
4. Выполняется `DeleteObjectCommand` для B — `_objects.Remove(B)` — коллекция: [C]
5. Undo: `_objects.Insert(1, B)` — результат: [C, B] — **неправильно!** Ожидалось [B, C]

**Правильное поведение:** Захватывать индекс в момент Execute, когда объект гарантированно находится в коллекции.

## Пошаговый план исправления

### Шаг 1: Изменить поле _index на nullable + захват в Execute

```csharp
public class DeleteObjectCommand : ICommand
{
    private readonly ObservableCollection<TemplateObjectBase> _objects;
    private readonly TemplateObjectBase _object;
    private int? _executedIndex;    // ← nullable, заполняется в Execute
    private readonly Action? _markDirty;

    public string Name => "Удалить объект";

    public DeleteObjectCommand(
        ObservableCollection<TemplateObjectBase> objects,
        TemplateObjectBase obj,
        Action? markDirty = null)
    {
        _objects = objects ?? throw new ArgumentNullException(nameof(objects));
        _object = obj ?? throw new ArgumentNullException(nameof(obj));
        _markDirty = markDirty;
        // _index НЕ захватывается здесь
    }

    public void Execute()
    {
        _executedIndex = _objects.IndexOf(_object);    // ← захват здесь
        if (_executedIndex < 0)
            throw new InvalidOperationException("Object not found in collection");
        _objects.Remove(_object);
        _markDirty?.Invoke();
    }

    public void Undo()
    {
        if (_executedIndex == null)
            return;

        var insertAt = Math.Clamp(_executedIndex.Value, 0, _objects.Count);
        _objects.Insert(insertAt, _object);
    }
}
```

### Шаг 2: (Опционально) Логировать ошибку, если индекс не найден

```csharp
public void Execute()
{
    _executedIndex = _objects.IndexOf(_object);
    if (_executedIndex < 0)
    {
        // Объект уже удалён из коллекции — это может быть баг
        // Логируем, но не кидаем исключение (не хотим ломать UI)
        _executedIndex = null;
        _markDirty?.Invoke();
        return;
    }
    _objects.Remove(_object);
    _markDirty?.Invoke();
}
```

### Шаг 3: Проверить все места создания DeleteObjectCommand

Найти все вызовы `new DeleteObjectCommand(...)` и убедиться, что они не ожидают захвата индекса в конструкторе:

```bash
rg "new DeleteObjectCommand" src/
```

Если какие-то места полагались на то, что _index захвачен до реального удаления — их нужно обновить.

## Итоговые изменения

| Было | Стало |
|------|-------|
| `_index` захвачен в конструкторе | `_executedIndex` захвачен в `Execute()` |
| `_index` — `int` | `_executedIndex` — `int?` |
| `Undo: Insert(_index, obj)` | `Undo: Insert(Clamp(...), obj)` |

## Проверка

```bash
dotnet build src/DotElectric.TemplateEditor.slnx
dotnet test src/DotElectric.TemplateEditor.Tests --filter "FullyQualifiedName~DeleteObjectCommand"
dotnet test src/DotElectric.TemplateEditor.Tests
```

## Риски

- Если `Execute()` не вызывается перед `Undo()`, `_executedIndex` будет `null` — Undo не сделает ничего. Но это нормально для паттерна Command.
- Изменение в `DeleteObjectCommand` может повлиять на любое место, которое использует undo/redo для удаления.
- `InvalidOperationException` в Execute — это защита от багов. Если объект уже удалён до вызова Execute — это ошибка программиста.
