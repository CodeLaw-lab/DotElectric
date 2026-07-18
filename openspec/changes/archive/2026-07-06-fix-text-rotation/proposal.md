## Why

При повороте текста маркеры выделения вращаются в противоположную сторону от самого текста, потому что формулы `RotatedCorner0–3` в `Text.cs` используют rotation math для Y-up (модель), а конвертер `ModelYToCanvasTopConverter` делает Y-flip, обращая направление sin-компонент.

## What Changes

- Исправить формулы `RotatedCorner1Y`, `RotatedCorner2X`, `RotatedCorner3X`, `RotatedCorner3Y` в `Text.cs` — sin-компоненты должны быть отрицательными (WPF Y-down rotation), а не положительными (model Y-up)
- `RotatedCorner0` и `RotatedCorner1X`, `RotatedCorner2Y` формулы корректны
- `GetBoundingBox()` использует те же формулы — будет исправлен автоматически

## Capabilities

### New Capabilities
*(нет — это багфикс существующей функциональности)*

### Modified Capabilities
*(нет — spec `session1-defects` уже требует правильного положения маркеров)*

## Impact

- `Models/Objects/Text.cs` — 4 формулы (строки 164-171)
- Тесты `RotatedCorner` свойств (если есть)
- `GetBoundingBox()` — исправится автоматически (использует те же формулы)
