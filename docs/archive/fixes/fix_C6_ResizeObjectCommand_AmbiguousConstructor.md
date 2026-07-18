# P1-C6: Двусмысленный конструктор ResizeObjectCommand для Line

## Анализ проблемы

**Файл:** `src/DotElectric.TemplateEditor/Commands/ResizeObjectCommand.cs:54-62`

**Симптом:** Конструктор для `Line` принимает параметры `newWidth`/`newHeight`, которые на самом деле являются не шириной/высотой, а разностью между конечной и начальной точками (`EndX - StartX`, `EndY - StartY`).

```csharp
public ResizeObjectCommand(
    Line line,
    long newStartX,
    long newStartY,
    long newEndX,
    long newEndY,
    Action? markDirty = null)
    : this((TemplateObjectBase)line,
          line.StartMicronsX, line.StartMicronsY,
          line.EndMicronsX - line.StartMicronsX,
          line.EndMicronsY - line.StartMicronsY,
          newEndX - newStartX,          // ← передаётся как newWidth!
          newEndY - newStartY,          // ← передаётся как newHeight!
          markDirty)
{
    _newX = newStartX;
    _newY = newStartY;
}
```

Приватный конструктор (строка 70) принимает `newWidth`/`newHeight`, и в Execute/Undo для Line использует:
```csharp
case Line line:
    line.StartMicronsX = _newX;
    line.StartMicronsY = _newY;
    line.EndMicronsX = _newX + _newWidth;     // _newWidth = (newEndX - newStartX)
    line.EndMicronsY = _newY + _newHeight;    // _newHeight = (newEndY - newStartY)
    break;
```

Это работает, но **вводит в заблуждение** разработчика, читающего код. Параметр называется `newWidth`, но фактически это дельта.

## Пошаговый план исправления

### Шаг 1: Выбрать стратегию

Есть два пути:
1. **Лёгкий:** Переименовать `newWidth` → `newDeltaX`, `newHeight` → `newDeltaY` в приватном конструкторе
2. **Полный:** Создать специализированный фабричный метод или изменить архитектуру (рекомендуется в паре с P3-D2/удалением ResizeObjectCommand)

Выбираем **лёгкий путь** + рекомендация удалить ResizeObjectCommand (уже покрыто P3-D2).

### Шаг 2: Переименовать параметры приватного конструктора

```csharp
private ResizeObjectCommand(
    TemplateObjectBase obj,
    long oldX,
    long oldY,
    long oldWidth,
    long oldHeight,
    long newDeltaX,      // ← вместо newWidth
    long newDeltaY,      // ← вместо newHeight
    Action? markDirty)
{
    // ...
    _newWidth = newDeltaX;
    _newHeight = newDeltaY;
}
```

### Шаг 3: Обновить XML-комментарии публичного конструктора

```csharp
/// <summary>
/// Создаёт команду изменения размера линии.
/// </summary>
/// <param name="line">Линия.</param>
/// <param name="newStartX">Новая координата X начальной точки.</param>
/// <param name="newStartY">Новая координата Y начальной точки.</param>
/// <param name="newEndX">Новая координата X конечной точки.</param>
/// <param name="newEndY">Новая координата Y конечной точки.</param>
/// <param name="markDirty">Callback для пометки dirty.</param>
public ResizeObjectCommand(
    Line line,
    long newStartX,
    long newStartY,
    long newEndX,
    long newEndY,
    Action? markDirty = null)
```

Параметры публичного конструктора остаются без изменений (они ясны: `newStartX`, `newEndX`). Меняется только приватная часть.

### Шаг 4: Добавить комментарии к полям _newWidth/_newHeight

```csharp
/// <summary>Новая ширина (для Rectangle/Text) или delta EndX-StartX (для Line).</summary>
private readonly long _newWidth;
/// <summary>Новая высота (для Rectangle/Text) или delta EndY-StartY (для Line).</summary>
private readonly long _newHeight;
```

## Альтернатива: не исправлять, удалить класс

В P3-D2 предлагается удалить `ResizeObjectCommand` и оставить только `CustomResizeCommand`. Если это будет сделано, то C6 отпадёт автоматически.

**Рекомендация:** Отложить C6, выполнить сначала P3-D2, затем проверить, что `ResizeObjectCommand` удалён.

## Если D2 не планируется — выполнить шаги 1-4

## Проверка

```bash
dotnet build src/DotElectric.TemplateEditor.slnx
dotnet test src/DotElectric.TemplateEditor.Tests
```

## Риски

- Переименование приватного конструктора не ломает внешний API
- Поля `_newWidth`/`_newHeight` используются в Execute/Undo — нужно убедиться, что переименование не затронет логику
