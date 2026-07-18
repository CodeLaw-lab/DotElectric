# P3-D2: Дублирование ResizeObjectCommand и CustomResizeCommand

## Анализ проблемы

**Файлы:** `Commands/ResizeObjectCommand.cs`, `Commands/CustomResizeCommand.cs`

**Симптом:** Два класса делают одно и то же — изменение размера Rectangle, Text, Line через одинаковый switch.

**ResizeObjectCommand** (138 строк):
```csharp
public class ResizeObjectCommand : ICommand
{
    // Конструкторы для Rectangle, Text, Line
    // Execute/Undo со switch по типу
}
```

**CustomResizeCommand** (112 строк):
```csharp
public class CustomResizeCommand : ICommand
{
    // Единый конструктор со всеми old/new параметрами
    // Execute/Undo со switch по типу
}
```

**Ключевое различие:**
- `ResizeObjectCommand`: специализированные конструкторы для каждого типа объектов
- `CustomResizeCommand`: универсальный конструктор с явными old/new значениями

**Где используются:**
- `ResizeObjectCommand` — вероятно, используется в PropertiesViewModel (изменение ширины/высоты через панель свойств)
- `CustomResizeCommand` — используется в ResizeTool (изменение размера через маркеры на холсте)

**Нарушение DRY:** Оба класса имеют идентичную логику в Execute/Undo (switch по _object с case для Rectangle, Text, Line).

## Пошаговый план исправления

### Рекомендуемая стратегия: удалить ResizeObjectCommand

`CustomResizeCommand` более гибкий (принимает любые old/new значения). `ResizeObjectCommand` — legacy, который можно переделать в фабричный метод.

### Шаг 1: Создать фабричный метод в CustomResizeCommand

Добавить статические методы для создания команд для конкретных типов:

```csharp
public static CustomResizeCommand ForRectangle(
    Rectangle rect,
    long newWidth,
    long newHeight,
    Action? markDirty = null)
{
    return new CustomResizeCommand(
        rect,
        rect.MicronsX, rect.MicronsY, rect.WidthMicrons, rect.HeightMicrons,
        rect.MicronsX, rect.MicronsY, newWidth, newHeight,
        markDirty);
}

public static CustomResizeCommand ForText(
    Text text,
    long newX,
    long newY,
    long newFontSize,
    Action? markDirty = null)
{
    return new CustomResizeCommand(
        text,
        text.MicronsX, text.MicronsY, text.WidthMicrons, text.FontSizeMicrons,
        newX, newFontSize,
        markDirty);
}

public static CustomResizeCommand ForLine(
    Line line,
    long newStartX,
    long newStartY,
    long newEndX,
    long newEndY,
    Action? markDirty = null)
{
    return new CustomResizeCommand(
        line,
        line.StartMicronsX, line.StartMicronsY,
        line.EndMicronsX - line.StartMicronsX, line.EndMicronsY - line.StartMicronsY,
        newStartX, newStartY,
        newEndX - newStartX, newEndY - newStartY,
        markDirty);
}
```

### Шаг 2: Удалить ResizeObjectCommand

После создания фабричных методов `ResizeObjectCommand` не нужен. Удалить файл.

### Шаг 3: Обновить места использования

Заменить `new ResizeObjectCommand(...)` на `CustomResizeCommand.ForXxx(...)`:

```bash
# Найти все использования ResizeObjectCommand
rg "new ResizeObjectCommand" src/ --include "*.cs"
```

Для каждого найденного места:
- Если Rectangle: `new ResizeObjectCommand(rect, newWidth, newHeight, markDirty)` → `CustomResizeCommand.ForRectangle(rect, newWidth, newHeight, markDirty)`
- Если Text: `new ResizeObjectCommand(text, newX, newY, newFontSize, markDirty)` → `CustomResizeCommand.ForText(text, newX, newY, newFontSize, markDirty)`
- Если Line: `new ResizeObjectCommand(line, newStartX, newStartY, newEndX, newEndY, markDirty)` → `CustomResizeCommand.ForLine(line, newStartX, newStartY, newEndX, newEndY, markDirty)`

### Шаг 4: Проверить PropertiesViewModel

Особое внимание уделить PropertiesViewModel (строка ~201-436), где есть ~20 команд изменения свойств. Убедиться, что все `ResizeObjectCommand` заменены.

### Шаг 5: Запустить тесты

```bash
dotnet build src/DotElectric.TemplateEditor.slnx
dotnet test src/DotElectric.TemplateEditor.Tests
```

## Альтернативный вариант: объединить в один класс

Вместо удаления `ResizeObjectCommand`, можно сделать его фасадом:
```csharp
[Obsolete("Use CustomResizeCommand.ForXxx() instead")]
public class ResizeObjectCommand : ICommand
{
    private readonly CustomResizeCommand _inner;

    public ResizeObjectCommand(Rectangle rect, long newWidth, long newHeight, Action? markDirty = null)
    {
        _inner = CustomResizeCommand.ForRectangle(rect, newWidth, newHeight, markDirty);
    }

    public void Execute() => _inner.Execute();
    public void Undo() => _inner.Undo();
    public string Name => _inner.Name;
}
```

Но это NOT RECOMMENDED — оставляет мёртвый код. Лучше удалить.

## Проверка

```bash
dotnet build src/DotElectric.TemplateEditor.slnx
dotnet test src/DotElectric.TemplateEditor.Tests
```

## Риски

- **Средний:** Замена конструкторов на фабрики может потребовать изменений в нескольких файлах. Возможны пропуски.
- **Низкий:** Логика Execute/Undo в `CustomResizeCommand` уже проверена тестами. Изменение затрагивает только создание команд.
- **Рекомендация:** Делать замену в одном PR/коммите, чтобы CI мог проверить все изменения.
