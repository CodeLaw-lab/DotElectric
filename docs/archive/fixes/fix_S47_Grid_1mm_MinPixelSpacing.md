# Sprint 47 — Grid 1mm: MinPixelSpacing fix

## Fix S47-1: Сетка не отображается при шаге 1мм

### Проблема

При установке шага сетки 1мм сетка полностью пропадала с холста.

**Два сценария:**

1. **Для листов A3+ (420×297мм и больше):** `GridHelper.GenerateGridNodes()` вычисляет `cols × rows > MaxGridNodes (100000)` — 125K+ узлов для A3 при 1мм. Функция возвращает пустой список, сетка не рисуется.

2. **Для A4 (210×297мм):** 62 878 узлов генерируются, но при Zoom=1.0 расстояние между точками = 1px, а диаметр точки = 2px. Точки полностью перекрываются, канвас заливается сплошным серым цветом RGB(192,192,192) — «сетка» нечитаема.

### Причина

`GridManager.RefreshGridNodes()` и `GridHelper.GenerateGridNodes()` не проверяли `MinPixelSpacing` (5px) — минимальное расстояние между узлами в пикселях, при котором точки сетки различимы.

В отличие от `GenerateGridLines()` и `GenerateVisibleGridLines()` (которые имеют проверку `pixelSpacing < MinPixelSpacing` → return empty), `GenerateGridNodes()` была изначально написана без этой проверки с намерением «точки видны даже при перекрытии». Однако это привело к двум проблемам:
- Перегрузка счётчика `MaxGridNodes` на больших листах
- Визуально нечитаемая заливка на малых листах

### Исправление

Добавлена проверка `MinPixelSpacing` на двух уровнях:

| Уровень | Файл | Изменение |
|---------|------|-----------|
| Manager | `ViewModels/Managers/GridManager.cs:83-90` | В `RefreshGridNodes()` после проверки `Enabled && Visible` — если `pixelSpacing = stepMm * zoom < MinPixelSpacing (5px)` — очищаем список и выходим (без генерации узлов) |
| Helper | `Helpers/GridHelper.cs:203-207` | Defense-in-depth: в `GenerateGridNodes()` добавлена та же проверка перед расчётом узлов |

### Поведение после фикса

| Шаг сетки | Zoom | PixelSpacing | Результат |
|-----------|------|-------------|-----------|
| 1мм | 1.0 (100%) | 1px < 5px | ❌ Скрыта (слишком плотно) |
| 1мм | 5.0 (500%) | 5px = MinPixelSpacing | ✅ Отображается |
| 2мм | 1.0 (100%) | 2px < 5px | ❌ Скрыта |
| 2мм | 2.5 (250%) | 5px = MinPixelSpacing | ✅ Отображается |
| 5мм | 1.0 (100%) | 5px = MinPixelSpacing | ✅ Отображается (как и раньше) |
| 10мм | 1.0 (100%) | 10px > 5px | ✅ Отображается |

Это стандартное CAD-поведение: сетка автоматически скрывается при шаге, меньшем 5 пикселей, и появляется при достаточном зуме.

### Файлы

- `ViewModels/Managers/GridManager.cs` — `RefreshGridNodes()`: MinPixelSpacing check
- `Helpers/GridHelper.cs` — `GenerateGridNodes()`: MinPixelSpacing check (defense-in-depth)
- `Tests/Helpers/GridHelperTests.cs` — обновлены и добавлены тесты

### Тесты

| Тест | Статус |
|------|--------|
| `GenerateGridNodes_TooSmallZoom_ReturnsEmpty` (был `StillGeneratesNodes`) | Updated |
| `GenerateGridNodes_1mmStep_AtZoom1_ReturnsEmpty` | New |
| `GenerateGridNodes_1mmStep_AtZoom5_GeneratesNodes` | New |
| Все GridHelper — 36 тестов | ✅ Passed |
| Integration — 49 тестов | ✅ Passed |

### Build

0 errors, 5 pre-existing warnings (CS0114 Clone + CS8618 Id)
