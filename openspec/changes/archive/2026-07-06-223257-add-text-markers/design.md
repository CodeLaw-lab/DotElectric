## Context

**Current state:** `EditorCanvas.xaml` содержит `ItemsControl` для маркеров выделения с `DataTemplate` для `Line` (2 круга) и `Rectangle` (8 квадратов). Для `Text` DataTemplate отсутствует — при выделении текста WPF использует дефолтный шаблон (`TextBlock` с `ToString()`) в позиции (0,0) маркерного Canvas.

Требование уже специфицировано в `session1-defects`:
> *Text selection markers follow rotation* — 4 угловых маркера на фактических повёрнутых углах текста, не на AABB.

**Готовые компоненты:**
- `Text.cs:161-171` — свойства `RotatedCorner0X/Y`–`RotatedCorner3X/Y` (4 угла в микронах, с учётом поворота)
- `MarkerPosition` attached behavior — устанавливает `Canvas.Left`/`Canvas.Top` через MultiBinding с `ModelXToCanvasLeftConverter` / `ModelYToCanvasTopConverter`
- `TextSelectionMarkerBehavior` — attached behavior на TextBlock (строка 198 XAML) — его назначение проверить

**Ограничение:** маркеры рендерятся в отдельном `ItemsControl` на `DrawingCanvas`, а не внутри DataTemplate объекта. Позиции задаются в модельных координатах (микроны), конвертируются в WPF-пиксели через `MarkerPosition`.

## Goals / Non-Goals

**Goals:**
- При выделении Text на канвасе отображаются 4 квадратных маркера по углам (с учётом поворота)
- При повороте текста маркеры следуют за повёрнутыми углами
- Визуальный стиль маркеров совпадает с Rectangle (6×6, белый фон, #0078D4 обводка)

**Non-Goals:**
- Маркеры на серединах сторон (только углы — консистентно с Line, не Rectangle)
- Поддержка resize через Text маркеры (это уже работает через HitTestHelper)
- Изменение существующих Line/Rectangle маркеров

## Decisions

| Решение | Вариант | Выбор | Обоснование |
|---------|---------|-------|-------------|
| Позиция углов | A: RotatedCorner0–3 vs B: вычисление в поведении | **A** | Свойства уже реализованы в Text.cs, переиспользуем существующую математику |
| Количество маркеров | A: 4 угла vs B: 4 угла + 4 середины | **A** | Line показывает 2 (концы), Rectangle — 8. Text по сути ближе к Rectangle, но 4 угла достаточно для визуальной идентификации выделения |
| Расположение в XAML | A: новый DataTemplate в ItemsControl vs B: внутри Grid Text DataTemplate | **A** | Все остальные типы уже там. Единый шаблон для маркеров проще поддерживать |
| TextSelectionMarkerBehavior | Проверить, не дублирует ли функциональность | Исследовать | Если behaviour уже рисует маркеры внутри DataTemplate — конфликт. Если нет — оставить как есть |

## Risks / Trade-offs

- **[Rotation formula mismatch]** Маркеры используют `RotatedCorner0–3` из Text.cs, который уже протестирован. Риск низкий.
- **[Style inconsistency]** 4 маркера у Text vs 8 у Rectangle. Это сознательное упрощение — текст обычно меньше прямоугольника.
