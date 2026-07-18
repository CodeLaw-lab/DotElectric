## Context

**Текущее состояние:** `Text.cs` вычисляет 4 повёрнутых угла для маркеров выделения через `RotatedCorner0–3`. Формулы используют стандартный поворот в Y-up (модельное пространство): `x' = x·cosθ - y·sinθ`, `y' = x·sinθ + y·cosθ`. Однако маркеры позиционируются через `ModelYToCanvasTopConverter`, который делает Y-flip: `Canvas.Top = (sheetHeight - Y_model)·zoom`. После Y-flip sin-компоненты меняют знак, и маркеры вращаются в обратную сторону.

**Влияние:** Маркеры и текст при повороте расходятся — чем больше угол, тем сильнее. При 0° ошибки нет. При 90° маркеры и текст находятся в противоположных положениях.

**Не затронуто:** `ContainsPoint()` работает корректно — он преобразует точку клика в Y-down ДО применения матрицы поворота. `HitTestHelper` и `ResizeMath` тоже используют правильные формулы.

## Goals / Non-Goals

**Goals:**
- Исправить 4 формулы `RotatedCorner1Y`, `RotatedCorner2X`, `RotatedCorner3X`, `RotatedCorner3Y`
- `GetBoundingBox()` исправится автоматически (переиспользует ту же арифметику)
- Все тесты проходят

**Non-Goals:**
- Изменение `ContainsPoint()` — он корректен
- Изменение `HitTestHelper` или `ResizeMath` — они не используют RotatedCorners
- Изменение конвертеров — они корректны

## Decisions

**Вывод правильных формул:**

WPF `RotateTransform(θ)` использует стандартную матрицу (counter-clockwise в Y-up, clock-wise в WPF Y-down):
```
x' = x·cosθ - y·sinθ
y' = x·sinθ + y·cosθ
```

Явные вычисления для TextBlock с шириной W и высотой H (в WPF Y-down локально от top-left):

| Угол | WPF (relative) | После Y-flip converter |
|------|----------------|----------------------|
| TR | (W·cosθ, W·sinθ) | (X + W·cosθ, Y+H - W·sinθ) |
| BL | (-H·sinθ, H·cosθ) | (X - H·sinθ, Y+H - H·cosθ) |
| BR | (W·cosθ - H·sinθ, W·sinθ + H·cosθ) | (X + W·cosθ - H·sinθ, Y+H - W·sinθ - H·cosθ) |

**Коррекция:** все sin-термы в выражениях после Y-flip converter имеют знак `-` (минус), а не `+` (плюс).

## Risks / Trade-offs

- **[ContainsPoint]** Убедиться, что `ContainsPoint` продолжает работать — его формула `cpY = (Y+H) - point.Y` делает Y-flip до поворота, поэтому он корректен. Наш фикс не влияет на `ContainsPoint`. ✓
- **[GetBoundingBox]** Использует те же неверные формулы — исправится автоматически.
