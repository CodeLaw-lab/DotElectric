# P2-M7: Отрицательные размеры Rectangle

## Анализ проблемы

**Файл:** `src/DotElectric.TemplateEditor/Models/Objects/Rectangle.cs:73-80`

**Симптом:** Конструктор `Rectangle` принимает любые `widthMicrons` и `heightMicrons`, включая отрицательные значения.

```csharp
public Rectangle(long micronsX, long micronsY, long widthMicrons, long heightMicrons, LineType lineType = LineType.Solid) : this()
{
    MicronsX = micronsX;
    MicronsY = micronsY;
    WidthMicrons = widthMicrons;       // ← может быть отрицательным
    HeightMicrons = heightMicrons;     // ← может быть отрицательным
    LineType = lineType;
}
```

**Последствия:**
1. **Некорректный bounding box:** `RightMicronsX = MicronsX + WidthMicrons` может быть меньше `MicronsX` при WidthMicrons < 0
2. **Ошибки в SelectionBoxHelper:** `GetRectangleBounds()` вычисляет Right = X + Width, что может дать Right < Left
3. **Ошибки в GridHelper/HitTestHelper:** Код не ожидает отрицательных размеров
4. **Некорректное отображение:** Rectangle с отрицательной шириной может рендериться невидимо или с артефактами

**Откуда берутся отрицательные значения:**
- `DrawingRectangleTool.CalculateRectangle()` (строка 119-120): при Shift-модификаторе:
  ```csharp
  w = size * Math.Sign(modelPoint.MicronsX - startPoint.MicronsX);
  h = size * Math.Sign(modelPoint.MicronsY - startPoint.MicronsY);
  ```
  Если `modelPoint.X < startPoint.X`, то `Sign()` вернёт -1, и `w` будет отрицательным.

- Это "нормальное" поведение для прямоугольника, растягиваемого влево/вверх. Но модель Rectangle этого не ожидает.

## Пошаговый план исправления

### Шаг 1: Добавить валидацию в конструктор

```csharp
public Rectangle(long micronsX, long micronsY, long widthMicrons, long heightMicrons, LineType lineType = LineType.Solid) : this()
{
    if (widthMicrons < 0)
        throw new ArgumentOutOfRangeException(nameof(widthMicrons), "Width cannot be negative");
    if (heightMicrons < 0)
        throw new ArgumentOutOfRangeException(nameof(heightMicrons), "Height cannot be negative");

    MicronsX = micronsX;
    MicronsY = micronsY;
    WidthMicrons = widthMicrons;
    HeightMicrons = heightMicrons;
    LineType = lineType;
}
```

### Шаг 2: Исправить CalculateRectangle в DrawingRectangleTool

Метод `CalculateRectangle` (строка 93-142) должен гарантировать, что возвращаемый Rectangle имеет неотрицательные размеры. Вместо `Sign`-логики нужно использовать `Math.Min/Math.Abs`:

```csharp
private static Rectangle CalculateRectangle(
    PointMicrons modelPoint,
    ToolModifiers modifiers,
    PointMicrons startPoint)
{
    long x, y, w, h;

    if ((modifiers & ToolModifiers.Ctrl) != 0 && (modifiers & ToolModifiers.Shift) != 0)
    {
        // Ctrl+Shift: квадрат от центра
        var dx = modelPoint.MicronsX - startPoint.MicronsX;
        var dy = modelPoint.MicronsY - startPoint.MicronsY;
        var size = Math.Max(Math.Abs(dx), Math.Abs(dy)) * 2;
        x = startPoint.MicronsX - size / 2;
        y = startPoint.MicronsY - size / 2;
        w = size;
        h = size;
    }
    else if ((modifiers & ToolModifiers.Shift) != 0)
    {
        // Shift: квадрат — всегда от меньшего угла
        var dx = Math.Abs(modelPoint.MicronsX - startPoint.MicronsX);
        var dy = Math.Abs(modelPoint.MicronsY - startPoint.MicronsY);
        var size = Math.Max(dx, dy);
        x = Math.Min(startPoint.MicronsX, modelPoint.MicronsX);
        y = Math.Min(startPoint.MicronsY, modelPoint.MicronsY);
        w = size;
        h = size;
    }
    else if ((modifiers & ToolModifiers.Ctrl) != 0)
    {
        // Ctrl: от центра
        var dx = Math.Abs(modelPoint.MicronsX - startPoint.MicronsX);
        var dy = Math.Abs(modelPoint.MicronsY - startPoint.MicronsY);
        x = startPoint.MicronsX - dx;
        y = startPoint.MicronsY - dy;
        w = dx * 2;
        h = dy * 2;
    }
    else
    {
        // Без модификаторов: от угла к углу
        x = Math.Min(startPoint.MicronsX, modelPoint.MicronsX);
        y = Math.Min(startPoint.MicronsY, modelPoint.MicronsY);
        w = Math.Abs(modelPoint.MicronsX - startPoint.MicronsX);
        h = Math.Abs(modelPoint.MicronsY - startPoint.MicronsY);
    }

    return new Rectangle(x, y, w, h);
}
```

**Ключевые изменения:**
- `Shift` (было `w = size * Math.Sign(...)`): заменено на `Math.Min` для X/Y + `Math.Abs` для размера
- `Ctrl` (было `Math.Abs(dx)`): уже корректно, но явно используем `dx`, `dy` (не `Math.Abs(dx)` дважды)
- Без модификаторов: уже корректно (`Math.Min` + `Math.Abs`)

### Шаг 3: Проверить, не создаётся ли Rectangle с отрицательными размерами в других местах

```bash
rg "new Rectangle\(.*-\d" src/ --include "*.cs"
```

Если других мест нет — исправление завершено.

### Шаг 4: Написать тест для конструктора с отрицательными значениями

```csharp
[Fact]
public void Constructor_ShouldThrowOnNegativeWidth()
{
    Assert.Throws<ArgumentOutOfRangeException>(() =>
        new Rectangle(0, 0, -100, 100));
}

[Fact]
public void Constructor_ShouldThrowOnNegativeHeight()
{
    Assert.Throws<ArgumentOutOfRangeException>(() =>
        new Rectangle(0, 0, 100, -100));
}

[Fact]
public void Constructor_ShouldAllowZeroDimensions()
{
    var rect = new Rectangle(0, 0, 0, 0);
    Assert.Equal(0, rect.WidthMicrons);
    Assert.Equal(0, rect.HeightMicrons);
}
```

## Проверка

```bash
dotnet build src/DotElectric.TemplateEditor.slnx
dotnet test src/DotElectric.TemplateEditor.Tests --filter "FullyQualifiedName~Rectangle"
dotnet test src/DotElectric.TemplateEditor.Tests
```

## Риски

- **Средний:** `CalculateRectangle` — критический метод для DrawingRectangleTool. Любая ошибка приведёт к неправильному рисованию.
- **Низкий:** Валидация в конструкторе может сломать код, который случайно передаёт отрицательные значения (но это именно то, что нужно — обнаружить и исправить такие случаи).
- **Время:** Изменение `CalculateRectangle` требует ручного тестирования всех комбинаций модификаторов (без модификаторов, Shift, Ctrl, Ctrl+Shift).
