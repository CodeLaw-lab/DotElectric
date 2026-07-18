# P2-M4: Sheet.Custom() не устанавливает Orientation

## Анализ проблемы

**Файл:** `src/DotElectric.TemplateEditor/Models/Sheet.cs:116-122`

**Симптом:** При создании пользовательского листа через `Sheet.Custom(widthMm, heightMm)` поле `Orientation` остаётся со значением по умолчанию (`Portrait` = 0), независимо от переданных размеров.

```csharp
public static Sheet Custom(double widthMm, double heightMm)
    => new()
    {
        Format = "Custom",
        WidthMicrons = Coordinate.ToMicrons(widthMm),
        HeightMicrons = Coordinate.ToMicrons(heightMm),
        // Orientation не задан → 0 = Portrait
    };
```

**Корень:** Свойство `Orientation` не инициализируется. `SheetOrientation` — это enum (int), по умолчанию 0 = `Portrait`.

**Последствия:**
1. Если пользователь укажет ширину > высоты (например, 500×300 мм), ориентация должна быть `Landscape`, но будет `Portrait`
2. При сериализации Orientation может быть неверным
3. UI может отображать неверную иконку ориентации

**Правильное поведение:** Определять ориентацию по соотношению сторон:
- `width >= height` → `Landscape`
- `width < height` → `Portrait`

## Пошаговый план исправления

### Шаг 1: Установить Orientation в Sheet.Custom()

```csharp
public static Sheet Custom(double widthMm, double heightMm)
    => new()
    {
        Format = "Custom",
        WidthMicrons = Coordinate.ToMicrons(widthMm),
        HeightMicrons = Coordinate.ToMicrons(heightMm),
        Orientation = widthMm >= heightMm
            ? SheetOrientation.Landscape
            : SheetOrientation.Portrait
    };
```

### Шаг 2: (Опционально) Проверить в TemplateService

В `TemplateService.CreateNew()` (строка 168) тоже создаётся Sheet через `Sheet.FromFormat()` — там Orientation задаётся явно, проблем нет.

В `TemplateService.Load()` (строка 304-312) Orientation восстанавливается из файла или через `DetermineOrientation` — тоже корректно.

Проблема только в `Sheet.Custom()`.

### Шаг 3: Написать тест

```csharp
[Fact]
public void Custom_ShouldSetOrientationBasedOnAspectRatio()
{
    var landscape = Sheet.Custom(500, 300);
    Assert.Equal(SheetOrientation.Landscape, landscape.Orientation);

    var portrait = Sheet.Custom(210, 297);
    Assert.Equal(SheetOrientation.Portrait, portrait.Orientation);

    var square = Sheet.Custom(100, 100);
    Assert.Equal(SheetOrientation.Landscape, square.Orientation);  // width == height → Landscape
}
```

## Проверка

```bash
dotnet build src/DotElectric.TemplateEditor.slnx
dotnet test src/DotElectric.TemplateEditor.Tests --filter "FullyQualifiedName~Sheet"
dotnet test src/DotElectric.TemplateEditor.Tests
```

## Риски

- Ширина/высота передаются в мм (`double`). При точном равенстве (`width == height`) выбирается `Landscape`. Это разумное поведение.
- Изменение не затрагивает существующие файлы — Orientation сохраняется/загружается через DTO.
