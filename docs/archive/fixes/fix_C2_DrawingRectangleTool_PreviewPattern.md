# P1-C2: Нарушение preview-паттерна в DrawingRectangleTool

## Анализ проблемы

**Файл:** `src/DotElectric.TemplateEditor/Tools/DrawingRectangleTool.cs:57-59`

**Симптом:** На каждое событие `OnMouseMove` создаётся новый объект `Rectangle` для preview. Аналогично C1 — проблема в `DrawingLineTool`, но здесь для прямоугольника.

**Корень:** На `OnMouseMove` (строка 59) вызывается:
```csharp
_editor.PreviewRectangle = new Rectangle(rect.MicronsX, rect.MicronsY, rect.WidthMicrons, rect.HeightMicrons);
```
При этом `rect` — результат `CalculateRectangle()`. Значения копируются из временной переменной `rect` в новый `Rectangle`.

**Парадокс:** `CalculateRectangle()` уже возвращает готовый `Rectangle` с правильными значениями. Автор делает лишнюю копию вместо того, чтобы присвоить его напрямую. А если присвоить напрямую — будет всё та же проблема (новый объект на каждый вызов). Нужно создать один раз и обновлять свойства.

## Пошаговый план исправления

### Шаг 1: Добавить поле для хранения preview

```csharp
private Rectangle? _previewRect;
```

### Шаг 2: Изменить OnMouseDown — создать один раз

```csharp
if (_startPoint == null)
{
    _startPoint = SnapHelper.SnapIfEnabled(modelPoint, _editor.GridSettings);
    _previewRect = new Rectangle(
        _startPoint.Value.MicronsX, _startPoint.Value.MicronsY, 0, 0, _lineType);
    _editor.PreviewRectangle = _previewRect;
}
```

### Шаг 3: Изменить OnMouseMove — обновлять свойства

```csharp
public void OnMouseMove(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
{
    if (_startPoint == null || _previewRect == null)
        return;

    var rect = CalculateRectangle(SnapHelper.SnapIfEnabled(modelPoint, _editor.GridSettings), modifiers, _startPoint.Value);

    _previewRect.MicronsX = rect.MicronsX;
    _previewRect.MicronsY = rect.MicronsY;
    _previewRect.WidthMicrons = rect.WidthMicrons;
    _previewRect.HeightMicrons = rect.HeightMicrons;
}
```

### Шаг 4: Изменить финализацию (OnMouseDown second click)

```csharp
// В блоке else OnMouseDown:
_previewRect = null;
_editor.PreviewRectangle = null;
```

### Шаг 5: Изменить OnDoubleClick

```csharp
public void OnDoubleClick(PointMicrons modelPoint)
{
    _startPoint = null;
    _previewRect = null;
    _editor.PreviewRectangle = null;
}
```

### Шаг 6: Изменить Reset()

```csharp
public void Reset()
{
    _startPoint = null;
    _previewRect = null;
    _editor.PreviewRectangle = null;
}
```

## Оптимизация: можно ли обновлять в CalculateRectangle

Вместо копирования полей из временного `rect` в `_previewRect` можно передать `_previewRect` в `CalculateRectangle` и заполнять его напрямую. Но это нарушит чистоту метода (side effect). Оставляем копирование — оно дешёвое (4 присваивания `long`).

## Проверка

```bash
dotnet build src/DotElectric.TemplateEditor.slnx
dotnet test src/DotElectric.TemplateEditor.Tests --filter "FullyQualifiedName~DrawingRectangleTool"
dotnet test src/DotElectric.TemplateEditor.Tests
```

## Риски

- Поле `_previewRect` должно быть синхронизировано с `_startPoint` (null/not null)
- Все места, где `_previewRect` обнуляется, должны также обнулять `_editor.PreviewRectangle`
