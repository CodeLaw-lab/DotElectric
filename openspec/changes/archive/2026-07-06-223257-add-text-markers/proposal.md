## Why

При выборе текстового объекта на чертеже маркеры выделения отсутствуют — в `ItemsControl` для маркеров нет `DataTemplate` для `Text`. WPF отображает стандартное представление (текст с `ToString()`) в левом верхнем углу листа, что дезориентирует пользователя.

## What Changes

- Добавить `DataTemplate DataType="{x:Type models:Text}"` в ресурсы маркеров (`EditorCanvas.xaml`)
- 4 квадратных маркера по углам текста с использованием `RotatedCorner0X/Y`–`RotatedCorner3X/Y` (уже реализованы в `Text.cs`)
- 4 квадратных маркера по серединам сторон (опционально, для консистентности с Rectangle)
- При необходимости — доработка `MarkerPosition` или `TextSelectionMarkerBehavior` для поддержки повёрнутых углов

## Capabilities

### New Capabilities
- `text-markers`: отображение маркеров выделения для текстовых объектов на канвасе

### Modified Capabilities
*(нет изменений требований существующих specs)*

## Impact

- `Views/EditorCanvas.xaml` — добавление `DataTemplate` для Text в ресурсы маркеров
- Возможно: `Behaviors/MarkerPosition.cs` — если потребуется адаптация для повёрнутых углов
- Тесты: обновление тестов выделения/маркеров
