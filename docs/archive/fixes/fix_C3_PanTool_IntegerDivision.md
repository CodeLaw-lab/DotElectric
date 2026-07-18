# P1-C3: Потеря точности через целочисленное деление в PanTool

## Анализ проблемы

**Файл:** `src/DotElectric.TemplateEditor/Tools/PanTool.cs:35,43`

**Симптом:** При панорамировании холста средней кнопкой мыши движение может быть дёрганым, с потерей точности на уровне миллиметров.

**Корень:** В двух местах используется целочисленное деление `long / long`:

```csharp
// Строка 35:
_lastMousePosition = new Point(
    modelPoint.MicronsX / EditorConstants.MicronsPerMm,    // long / long = long (усечение!)
    modelPoint.MicronsY / EditorConstants.MicronsPerMm);

// Строка 43:
var currentPosition = new Point(
    modelPoint.MicronsX / EditorConstants.MicronsPerMm,    // long / long = long (усечение!)
    modelPoint.MicronsY / EditorConstants.MicronsPerMm);
```

`EditorConstants.MicronsPerMm = 1000`. Деление `long / long` в C# даёт `long` с отбрасыванием дробной части. Если `modelPoint.MicronsX = 1500`, то `1500 / 1000 = 1` (теряем 0.5 мм). Разница между соседними MouseMove-событиями может быть меньше 1 мм, и эта разница будет потеряна, что вызовет дёрганье.

**Правильное поведение:** Использовать `Coordinate.ToMm()`:

```csharp
public static double ToMm(long microns) => microns / 1000.0;  // деление double
```

Метод уже существует в `Models/Coordinate.cs`, но `PanTool` его не использует.

## Пошаговый план исправления

### Шаг 1: Изменить OnMouseDown (строка 35)

```csharp
_lastMousePosition = new Point(
    Coordinate.ToMm(modelPoint.MicronsX),
    Coordinate.ToMm(modelPoint.MicronsY));
```

### Шаг 2: Изменить OnMouseMove (строка 43)

```csharp
var currentPosition = new Point(
    Coordinate.ToMm(modelPoint.MicronsX),
    Coordinate.ToMm(modelPoint.MicronsY));
```

### Шаг 3: Проверить, что `Models` namespace импортирован

Проверить, что `using DotElectric.TemplateEditor.Models;` уже есть в файле (строка 5). `Coordinate` находится в этом namespace.

### Шаг 4: Удалить неиспользуемый `using EditorConstants`

Если после замены `EditorConstants.MicronsPerMm` больше нигде не используется в файле — удалить `using DotElectric.TemplateEditor.Constants;` (строка 3).

**Проверить:** используется ли `EditorConstants` где-то ещё в `PanTool`? Нет, только `MicronsPerMm` в этих двух строках. Значит, `using` можно удалить.

## Конечный результат

```csharp
using System.Windows;
using System.Windows.Controls;
using DotElectric.TemplateEditor.Models;
using DotElectric.TemplateEditor.ViewModels;

namespace DotElectric.TemplateEditor.Tools;

public class PanTool : ITool
{
    // ... без изменений ...

    public void OnMouseDown(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        // ...
        _lastMousePosition = new Point(
            Coordinate.ToMm(modelPoint.MicronsX),
            Coordinate.ToMm(modelPoint.MicronsY));
    }

    public void OnMouseMove(PointMicrons modelPoint, ToolMouseButton button, ToolModifiers modifiers)
    {
        if (!_isPanning) return;

        var currentPosition = new Point(
            Coordinate.ToMm(modelPoint.MicronsX),
            Coordinate.ToMm(modelPoint.MicronsY));
        var delta = currentPosition - _lastMousePosition;
        _editor.PanCanvas(delta.X, delta.Y);
        _lastMousePosition = currentPosition;
    }
}
```

## Проверка

```bash
dotnet build src/DotElectric.TemplateEditor.slnx

# Запустить приложение, проверить панорам. средней кнопкой:
# - Резкие движения — холст следует плавно
# - Медленные движения — без залипаний
dotnet run --project src/DotElectric.TemplateEditor
```

## Риски

- `Coordinate.ToMm()` возвращает `double`. `Point` принимает `double`. Типы совместимы.
- `System.Windows.Point` уже используется в коде (строка 1: `using System.Windows;`)
- Производительность: деление `double` сравнимо по скорости с делением `long` на современных CPU
