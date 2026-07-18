# ОТЧЁТ ПО РЕАЛИЗАЦИИ HELPERS — Спринт 2

**Проект:** DotElectric — Этап 1 (Редактор шаблонов листов)
**Дата:** 05.04.2026
**Статус:** ✅ HELPERS РЕАЛИЗОВАНЫ

---

## 1. ОБЗОР

Все 6 классов helpers реализованы и успешно компилируются.

| Метрика | Значение |
|---------|----------|
| Классов реализовано | 6/6 (100%) |
| Файлов создано | 6 |
| `dotnet build` | ✅ Проходит без ошибок |
| Архитектура | Чистые функции (static), без зависимости от WPF |

---

## 2. РЕАЛИЗОВАННЫЕ HELPERS

### 2.1. SnapHelper

| Метод | Описание |
|-------|----------|
| `SnapToGrid(PointMicrons, stepMicrons)` | Привязать точку к сетке |
| `SnapX(micronsX, stepMicrons)` | Привязать координату X |
| `SnapY(micronsY, stepMicrons)` | Привязать координату Y |
| `SnapSize(sizeMicrons, stepMicrons)` | Привязать размер (неотрицательный) |
| `SnapObject(ITemplateObject, stepMicrons)` | Привязать опорную точку объекта к сетке |

**Особенности:**
- Делегирует `Coordinate.SnapToGrid()` (центральная логика)
- `SnapSize()` гарантирует неотрицательный результат (`Math.Max(0, ...)`)
- `SnapObject()` работает через `ITemplateObject.Move()` — универсальный интерфейс

### 2.2. HitTestHelper

| Метод | Описание |
|-------|----------|
| `HitTest(PointMicrons, IList<ITemplateObject>)` | Найти верхний объект в точке (O(N) с конца) |
| `HitTestAll(PointMicrons, IList<ITemplateObject>)` | Найти все объекты в точке (multi-selection) |
| `HitTestObject(ITemplateObject, PointMicrons)` | Проверить один объект |
| `HitTestLine(Line, PointMicrons)` | Hit-test линии с tolerance 5мм |
| `HitTestRectangle(Rectangle, PointMicrons)` | Hit-test по bounding box |
| `HitTestText(Text, PointMicrons)` | Hit-test с учётом поворота (0/90/180/270) |
| `DistanceFromPointToLine(point, lineStart, lineEnd)` | Расстояние от точки до отрезка |

**Константы:**
- `LineHitTolerance = 5000` микрон (5 мм) — зона захвата линий

**Особенности HitTestText:**
- Оценивает bounding box по длине контента и размеру шрифта
- Коэффициент ширины: `length * fontSize * 0.6` (средний)
- Корректно обрабатывает повороты 0°, 90°, 180°, 270°

**Алгоритм DistanceFromPointToLine:**
- Проекция точки на прямую (параметр t)
- Ограничение t = [0, lengthSquared] — ближайшая точка на отрезке
- Расстояние до ближайшей точки (целочисленный sqrt)
- Проверка `lengthSquared == 0` — обработка вырожденной линии

### 2.3. SelectionBoxHelper

| Тип/Enum | Описание |
|----------|----------|
| `SelectionDirection` | LeftToRight (полное попадание), RightToLeft (пересечение) |
| `RectMicrons` | Immutable struct: Left, Bottom, Right, Top, Width, Height |

| Метод | Описание |
|-------|----------|
| `GetDirection(start, end)` | Определить направление рамки по X |
| `GetSelectedObjects(box, objects, direction)` | Универсальный метод (вызов нужной логики) |
| `GetFullyContained(box, objects)` | LeftToRight: только целиком внутри |
| `GetIntersecting(box, objects)` | RightToLeft: все задетые |
| `GetObjectBounds(obj)` | Bounding box для Line/Rectangle/Text |

**RectMicrons:**
- Конструктор автоматически нормализует (min → Left/Bottom, max → Right/Top)
- `FromPoints(start, end)` — создание из двух точек перетаскивания
- `Intersects(other)` — проверка пересечения
- `Contains(other)` — проверка полного содержания

**GetObjectBounds для каждого типа:**
- **Line:** min/max Start и End
- **Rectangle:** MicronsX/Y + Width/Height
- **Text:** оценка по Content.Length * FontSize * 0.6, с учётом RotationAngle

### 2.4. GridHelper

> **⚠️ Примечание (Sprint 59):** `GridLine`, `GenerateGridLines()`, `GenerateVisibleGridLines()` удалены как мёртвый код — никогда не вызывались в production. `CalculateOptimalStep()` заменён на `ComputeDisplayStep()`, который теперь принимает `preferredStepMicrons` (желаемый шаг пользователя).

| Тип | Описание |
|-----|----------|
| ~~`GridLine`~~ | *(удалён)* |

| Метод | Описание |
|-------|----------|
| ~~`GenerateGridLines(sheet, stepMicrons, zoom)`~~ | *(удалён)* |
| ~~`GenerateVisibleGridLines(...)`~~ | *(удалён)* |
| `CalculateOptimalStep(baseStepMicrons, zoom)` | *(заменён на `ComputeDisplayStep`)* |
| `ComputeDisplayStep(zoom, maxNodes, ...)` | Автоматический подбор шага с учётом zoom, viewport и предпочтений пользователя |

**Константы:**
- `MinPixelSpacing = 5.0` px — минимальное расстояние между узлами на экране

**Логика ComputeDisplayStep:**
- Принимает `preferredStepMicrons` (желаемый шаг пользователя)
- Если `preferredStep` даёт `pixelSpacing >= MinPixelSpacing` — используется как целевой
- Если `pixelSpacing < MinPixelSpacing` — fallback на `MinPixelSpacing / zoom`
- В обоих случаях шаг coarsen'ится (увеличивается) если `cols * rows > MaxGridNodes`**
- Если базовый шаг слишком мелкий при текущем зуме — умножает на 2, 4, 8...
- Гарантирует, что линии сетки видны (>= 5px)

### 2.5. ValidationService + ValidationError

| Тип | Описание |
|-----|----------|
| `ValidationSeverity` | Enum: Error (блокирует), Warning (предупреждение) |
| `ValidationError` | RuleId, Message, ObjectId?, Severity |

| Правило | ID | Описание | Статус |
|---------|----|----------|--------|
| V-001 | Уникальность ID | Проверка HashSet, пустые ID, дубли | ✅ |
| V-002 | Уникальность ключей | Author не пустой (Metadata) | ✅ |
| V-003 | Координаты в пределах листа | Проверка всех точек Line, Rectangle, Text | ✅ |
| V-004 | Положительные размеры | Width/Height > 0, FontSize > 0, Content не пустой | ✅ |
| V-005 | HEX-цвета | Зарезервировано (модель пока не имеет полей цвета) | ✅ |
| V-006 | Формат листа | A0-A4, Custom (с проверкой размеров) | ✅ |
| V-007 | Тип линии | Enum.IsDefined(typeof(LineType)) | ✅ |

| Метод | Описание |
|-------|----------|
| `Validate(Template)` | Полная валидация всех правил |
| `ValidateObject(ITemplateObject, Sheet)` | Валидация отдельного объекта |
| `ValidateMetadataKeys(Metadata)` | Проверка ключевых полей (V-002) |
| `HasErrors(errors)` | Есть ли ошибки (без warnings) |
| `HasWarnings(errors)` | Есть ли предупреждения |
| `FormatErrors(errors)` | Строковое представление всех ошибок |
| `IsValidHexColor(color)` | Проверка формата #RRGGBB (Regex) |

---

## 3. КЛЮЧЕВЫЕ АРХИТЕКТУРНЫЕ РЕШЕНИЯ

### 3.1. Без зависимости от WPF

Все Helpers — **чистые static-классы**, работают только с моделями. Никаких импортов `System.Windows.*`.

### 3.2. Fixed-Point (микронная точность)

Все координаты и размеры — в `long` (микроны). Конвертация в мм только для сообщений об ошибках (`Coordinate.FormatMm()`).

### 3.3. Iterator Pattern (yield return)

ValidationService использует `yield return` — ленивая генерация ошибок. Остановка при первой ошибке возможна через `.FirstOrDefault()`.

### 3.4. Z-order при hit-testing

Поиск с конца списка (`objects.Count - 1 → 0`). Последний нарисованный объект = верхний = приоритетнее.

### 3.5. LeftToRight vs RightToLeft

| Направление | Условие | Логика |
|-------------|---------|--------|
| LeftToRight | start.X <= end.X | Только полное попадание (`box.Contains(objBounds)`) |
| RightToLeft | start.X > end.X | Любое пересечение (`box.Intersects(objBounds)`) |

### 3.6. Bounding Box текста

Текст не имеет явных Width/Height в модели — оценивается приблизительно:
- Ширина = `Content.Length * FontSizeMicrons * 0.6`
- Высота = `FontSizeMicrons`
- Коэффициент 0.6 — средняя ширина символа для пропорциональных шрифтов

---

## 4. СТРУКТУРА ФАЙЛОВ

```
src/DotElectric.TemplateEditor/Helpers/
├── SnapHelper.cs              ✅ Привязка к сетке (5 методов)
├── HitTestHelper.cs           ✅ Hit-testing (7 методов, константа)
├── SelectionBoxHelper.cs      ✅ Рамка выделения (+RectMicrons, SelectionDirection)
├── GridHelper.cs              ✅ Генерация сетки (+GridLine, 3 метода)
├── ValidationError.cs         ✅ Модель ошибки (+ValidationSeverity)
└── ValidationService.cs       ✅ 7 правил валидации (15+ методов)
```

---

## 5. СЛЕДУЮЩИЕ ШАГИ (Спринт 2)

| Блок | Задач | Статус |
|------|-------|--------|
| Models | ✅ 7/7 | Готово |
| Commands | ✅ 3/3 | Готово |
| Helpers | ✅ 5/5 | ✅ Готово |
| Services | ⬜ 0/7 | Ожидает |
| Тесты | ⬜ 0/7 | Ожидает |

**Всего:** 15/27 задач выполнено (56%)

---

## 6. ИЗВЕСТНЫЕ ОГРАНИЧЕНИЯ

1. **V-005 (HEX-цвета)** — модель пока не имеет полей цвета, правило зарезервировано
2. **Bounding box текста** — приблизительная оценка (0.6 коэффициент), не учитывает конкретный шрифт
3. **Hit-test повёрнутого Rectangle** — не реализован (прямоугольник в модели всегда axis-aligned)
4. **Hit-test повёрнутого Text** — реализован для 0/90/180/270°, но bounding box приблизительный
5. **Нет кэширования** — HitTest каждый раз пересчитывает bounding box (для N ≤ 1000 — приемлемо)

---

## 7. ПОКРЫТИЕ ТЕСТАМИ (план)

| Helper | Целевое покрытие | Статус |
|--------|-----------------|--------|
| SnapHelper | 90%+ | ⬜ Ожидает |
| HitTestHelper | 90%+ | ⬜ Ожидает |
| SelectionBoxHelper | 90%+ | ⬜ Ожидает |
| GridHelper | 90%+ | ⬜ Ожидает |
| ValidationService | 100% | ⬜ Ожидает |

---

**Отчёт составил:** AI-ассистент DotElectric
**Дата:** 05.04.2026
**Статус:** ✅ HELPERS РЕАЛИЗОВАНЫ И КОМПИЛИРУЮТСЯ
