# P1-C5: Логическая путаница с Line в ResizeTool

## Анализ проблемы

**Файлы:** `src/DotElectric.TemplateEditor/Tools/ResizeTool.cs:86-89,145-148`

**Симптом:** Поля `_startWidth` и `_startHeight` используются для разных семантик в зависимости от типа объекта:
- Для `Rectangle`: это реальные ширина и высота
- Для `Line`: это конечные точки (EndMicronsX, EndMicronsY)

**Код (OnMouseDown, строки 82-89):**
```csharp
else if (_editor.SingleSelectedObject is Line line)
{
    _resizedObject = line;
    _startX = line.StartMicronsX;
    _startY = line.StartMicronsY;
    _startWidth = line.EndMicronsX;     // ← это не ширина!
    _startHeight = line.EndMicronsY;    // ← это не высота!
}
```

**Код (OnMouseUp, строки 142-151):**
```csharp
else if (_resizedObject is Line line)
{
    var cmd = new CustomResizeCommand(
        line,
        _startX, _startY, _startWidth - _startX, _startHeight - _startY,  // ← oldWidth = EndX - StartX
        line.StartMicronsX, line.StartMicronsY,
        line.EndMicronsX - line.StartMicronsX, line.EndMicronsY - line.StartMicronsY,
        _editor.MarkDirty);
}
```

Выражение `_startWidth - _startX` работает, потому что `_startWidth = EndMicronsX`, `_startX = StartMicronsX`, и `EndMicronsX - StartMicronsX` даёт "ширину" линии (ΔX между концами). Но это хрупко — если кто-то прочитает код, он не поймёт, что `_startWidth` содержит EndMicronsX.

**ResizeLine (строки 275-291):**
```csharp
private void ResizeLine(Line line, double dx, double dy)
{
    if (_activeHandle == ResizeHandle.BottomRight)
    {
        line.EndMicronsX = _startWidth + (long)dx;    // _startWidth = EndX
        line.EndMicronsY = _startHeight + (long)dy;   // _startHeight = EndY
    }
    else if (_activeHandle == ResizeHandle.TopLeft)
    {
        line.StartMicronsX = _startX + (long)dx;
        line.StartMicronsY = _startY + (long)dy;
    }
}
```

Здесь `_startWidth` и `_startHeight` используются как конечные точки. Работает корректно, но нечитаемо.

## Пошаговый план исправления

### Шаг 1: Добавить явные поля для Line

```csharp
// Сохранённые начальные параметры объекта
private long _startX;
private long _startY;
private long _startWidth;
private long _startHeight;

// Явные поля для Line (начальные и конечные точки)
private long _lineStartX;
private long _lineStartY;
private long _lineEndX;
private long _lineEndY;
```

### Шаг 2: Изменить OnMouseDown для Line

```csharp
else if (_editor.SingleSelectedObject is Line line)
{
    _resizedObject = line;
    _lineStartX = line.StartMicronsX;
    _lineStartY = line.StartMicronsY;
    _lineEndX = line.EndMicronsX;
    _lineEndY = line.EndMicronsY;
}
```

### Шаг 3: Изменить ResizeLine

```csharp
private void ResizeLine(Line line, double dx, double dy)
{
    if (_activeHandle == ResizeHandle.BottomRight)
    {
        line.EndMicronsX = _lineEndX + (long)dx;
        line.EndMicronsY = _lineEndY + (long)dy;
    }
    else if (_activeHandle == ResizeHandle.TopLeft)
    {
        line.StartMicronsX = _lineStartX + (long)dx;
        line.StartMicronsY = _lineStartY + (long)dy;
    }
}
```

### Шаг 4: Изменить OnMouseUp для Line

```csharp
else if (_resizedObject is Line line)
{
    var cmd = new CustomResizeCommand(
        line,
        _lineStartX, _lineStartY,
        _lineEndX - _lineStartX, _lineEndY - _lineStartY,
        line.StartMicronsX, line.StartMicronsY,
        line.EndMicronsX - line.StartMicronsX, line.EndMicronsY - line.StartMicronsY,
        _editor.MarkDirty);
    _editor.CommandHistory.Push(cmd);
}
```

### Шаг 5: Дополнительно — переименовать _startWidth/_startHeight

Для Rectangle и Text поля `_startWidth`/`_startHeight` используются корректно. Можно переименовать их в `_startRectWidth`/`_startRectHeight` для ясности, но это необязательно.

## Итоговые изменения в ResizeTool.cs

| Было | Стало |
|------|-------|
| `_startWidth = line.EndMicronsX` | `_lineEndX = line.EndMicronsX` |
| `_startHeight = line.EndMicronsY` | `_lineEndY = line.EndMicronsY` |
| `line.EndMicronsX = _startWidth + dx` | `line.EndMicronsX = _lineEndX + dx` |
| `_startWidth - _startX` в OnMouseUp | `_lineEndX - _lineStartX` |

## Проверка

```bash
dotnet build src/DotElectric.TemplateEditor.slnx
dotnet test src/DotElectric.TemplateEditor.Tests --filter "FullyQualifiedName~Resize"
dotnet test src/DotElectric.TemplateEditor.Tests --filter "FullyQualifiedName~Line"
dotnet test src/DotElectric.TemplateEditor.Tests
```

## Риски

- Изменение имён полей не затрагивает публичный API (это `private` поля)
- `ResizeTool` — временный инструмент (push/pop), не хранит состояние между вызовами
- Все изменения локализованы в одном файле
