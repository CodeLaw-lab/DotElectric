# P3-D5: Дублирование SelectionDirection и SelectionBoxDirection

## Анализ проблемы

**Файлы:**
- `Helpers/SelectionBoxHelper.cs:9-20` — `SelectionDirection` enum
- `Models/SelectionBoxDirection.cs:7-11` — `SelectionBoxDirection` enum

**Дубликат 1 (в SelectionBoxHelper):**
```csharp
public enum SelectionDirection
{
    LeftToRight,   // полное попадание в рамку
    RightToLeft    // частичное пересечение
}
```

**Дубликат 2 (в Models):**
```csharp
public enum SelectionBoxDirection
{
    LTR,    // Left-to-Right
    RTL     // Right-to-Left
}
```

**Проблема:** Два enum с одинаковой семантикой:
1. `SelectionDirection` имеет читаемые имена (`LeftToRight`/`RightToLeft`)
2. `SelectionBoxDirection` имеет краткие имена (`LTR`/`RTL`)

**Где используются:**
- `SelectionDirection` — используется в `SelectionBoxHelper.GetDirection()` и `GetSelectedObjects()`
- `SelectionBoxDirection` — вероятно, используется где-то ещё (возможно, в SelectTool или SelectionManager)

## Пошаговый план исправления

### Шаг 1: Выбрать, какой enum оставить

**Рекомендация:** Оставить `SelectionDirection` из Helpers, так как:
- Более читаемые имена (`LeftToRight` vs `LTR`)
- Уже используется в `SelectionBoxHelper` как часть его API
- В папке `Helpers` логичнее держать тип, связанный с хелпером

### Шаг 2: Найти все использования обоих enum

```bash
# Поиск использования SelectionBoxDirection
rg "SelectionBoxDirection" src/ --include "*.cs" --include "*.xaml"

# Поиск использования SelectionDirection
rg "SelectionDirection" src/ --include "*.cs" --include "*.xaml"
```

### Шаг 3: Заменить SelectionBoxDirection на SelectionDirection

Для каждого найденного вхождения `SelectionBoxDirection`:
- `LTR` → `LeftToRight`
- `RTL` → `RightToLeft`

### Шаг 4: Удалить SelectionBoxDirection.cs

```bash
# Удалить файл дублирующего enum
rm src/DotElectric.TemplateEditor/Models/SelectionBoxDirection.cs
```

### Шаг 5: Проверить namespace

Если `SelectionBoxDirection` импортировался через `using DotElectric.TemplateEditor.Models;`, а `SelectionDirection` находится в `DotElectric.TemplateEditor.Helpers;` — нужно обновить `using` в файлах, где была замена.

## Альтернатива: объединить в Models

Если по логике направление рамки — это модель, а не хелпер, можно перенести `SelectionDirection` в `Models/SelectionDirection.cs` (переименовав из `SelectionBoxDirection` с обновлением имён полей):

```csharp
namespace DotElectric.TemplateEditor.Models;

public enum SelectionDirection
{
    LeftToRight,
    RightToLeft
}
```

Тогда `SelectionBoxHelper.cs` будет импортировать его из `Models`.

## Рекомендуемый вариант

**Перенести в Models** (сохраняя имена `LeftToRight`/`RightToLeft`):

1. Создать `Models/SelectionDirection.cs`:
   ```csharp
   namespace DotElectric.TemplateEditor.Models;

   /// <summary>
   /// Направление рамки выделения.
   /// LeftToRight = полное попадание, RightToLeft = частичное пересечение.
   /// </summary>
   public enum SelectionDirection
   {
       LeftToRight,
       RightToLeft
   }
   ```

2. В `SelectionBoxHelper.cs` заменить свой `SelectionDirection` на `using DotElectric.TemplateEditor.Models.SelectionDirection`

3. Удалить `SelectionBoxDirection.cs` из `Models`

4. Удалить `enum SelectionDirection` из `SelectionBoxHelper.cs`

## Итоговые изменения

| Файл | Изменение |
|------|-----------|
| `Models/SelectionBoxDirection.cs` | Удалить |
| `Models/SelectionDirection.cs` | Создать (с именами LeftToRight/RightToLeft) |
| `Helpers/SelectionBoxHelper.cs` | Удалить enum, использовать из Models |
| Другие файлы | Обновить using и имена полей |

## Проверка

```bash
dotnet build src/DotElectric.TemplateEditor.slnx
dotnet test src/DotElectric.TemplateEditor.Tests --filter "FullyQualifiedName~SelectionBox"
dotnet test src/DotElectric.TemplateEditor.Tests
```

## Риски

- **Средний:** Если `LTR`/`RTL` используются в XAML (например, как параметр команды) — нужно явно обновить XAML-файлы или добавить конвертер
- **Низкий:** Переименование enum-значений ломает бинарную совместимость, но не source-совместимость (C# компилятор подхватит новые имена)
