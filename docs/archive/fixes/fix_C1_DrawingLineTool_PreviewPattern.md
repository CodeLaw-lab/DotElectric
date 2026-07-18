# P1-C1: Нарушение preview-паттерна в DrawingLineTool

## Анализ проблемы

**Файл:** `src/DotElectric.TemplateEditor/Tools/DrawingLineTool.cs:67-70`

**Симптом:** На каждое событие `OnMouseMove` (до 120/сек.) создаётся новый объект `Line`, который присваивается `_editor.PreviewLine`. Это порождает:
- 120+ аллокаций `Line` в секунду при активном рисовании
- 120+ срабатываний `PropertyChanged` для `PreviewLine`
- 120+ вызовов `UpdatePreviewLine()` в `PreviewLineChangedBehavior`
- Лишнюю нагрузку на GC (сборка мусора)

**Корень:** Разработчик не учёл паттерн, указанный в AGENTS.md: *"Preview shapes: create once, update properties only"*.

**Сравнение с правильным подходом:**
- `OnMouseDown` (строка 34): создаёт `_editor.PreviewLine = new Line(...)` — **правильно**, это однократное создание
- `OnMouseMove` (строка 67): `_editor.PreviewLine = new Line(...)` — **неправильно**, нужно обновлять свойства

## Пошаговый план исправления

### Шаг 1: Добавить поле для хранения preview

В класс `DrawingLineTool` добавить поле:

```csharp
private Line? _previewLine;
```

### Шаг 2: Изменить OnMouseDown — создать один раз

```csharp
if (_startPoint == null)
{
    _startPoint = SnapHelper.SnapIfEnabled(modelPoint, _editor.GridSettings);
    _previewLine = new Line(
        _startPoint.Value.MicronsX, _startPoint.Value.MicronsY,
        _startPoint.Value.MicronsX, _startPoint.Value.MicronsY,
        _lineType);
    _editor.PreviewLine = _previewLine;
}
```

### Шаг 3: Изменить OnMouseMove — обновлять свойства

```csharp
public void OnMouseMove(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
{
    if (_startPoint == null || _previewLine == null)
        return;

    var end = ApplyConstraint(SnapHelper.SnapIfEnabled(modelPoint, _editor.GridSettings), modifiers, _startPoint.Value);

    _previewLine.StartMicronsX = _startPoint.Value.MicronsX;
    _previewLine.StartMicronsY = _startPoint.Value.MicronsY;
    _previewLine.EndMicronsX = end.MicronsX;
    _previewLine.EndMicronsY = end.MicronsY;
}
```

### Шаг 4: Изменить финализацию (OnMouseDown second click)

```csharp
// В блоке else OnMouseDown (финализация):
_previewLine = null;
_editor.PreviewLine = null;
```

### Шаг 5: Изменить OnDoubleClick

```csharp
public void OnDoubleClick(PointMicrons modelPoint)
{
    _startPoint = null;
    _previewLine = null;
    _editor.PreviewLine = null;
}
```

### Шаг 6: Изменить Reset()

```csharp
public void Reset()
{
    _startPoint = null;
    _previewLine = null;
    _editor.PreviewLine = null;
}
```

## Проверка

```bash
# Сборка
dotnet build src/DotElectric.TemplateEditor.slnx

# Тесты для инструмента линии
dotnet test src/DotElectric.TemplateEditor.Tests --filter "FullyQualifiedName~DrawingLineTool"

# Все тесты
dotnet test src/DotElectric.TemplateEditor.Tests
```

## Риски

- Изменение затрагивает только `DrawingLineTool`, другие инструменты не страдают
- `Line` — модель, а не UI-элемент. При изменении свойств через `PropertyChanged` WPF обновляет preview корректно
- Поле `_previewLine` должно быть `null` когда нет активного рисования (консистентно с `_startPoint`)
