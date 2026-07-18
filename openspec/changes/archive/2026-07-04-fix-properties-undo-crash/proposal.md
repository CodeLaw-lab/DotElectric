## Why

Все три sub-ViewModel (LinePropertiesViewModel, RectanglePropertiesViewModel, TextPropertiesViewModel) создают `ChangePropertyCommand` с лямбдами, захватывающими поле (`_line`/`_rect`/`_text`) по ссылке. После вызова `UpdateObject(null)` при смене выделения поле обнуляется, но команда остаётся в undo-стеке. Ctrl+Z вызывает `_setter(_oldValue)` на null — NullReferenceException.

Та же системная проблема, что была в `ResizeTool` (зафиксирована в `fix-resize-tool-undo`), повторяется в ~28 лямбда-парах по трём файлам.

## What Changes

- `LinePropertiesViewModel` — 7 call sites: заменить захват `_line` на локальную переменную
- `RectanglePropertiesViewModel` — 8 call sites: заменить захват `_rect` на локальную переменную
- `TextPropertiesViewModel` — 10 call sites через `SetProperty` + 3 inline: заменить захват `_text` на локальную переменную
- Тесты — добавить тесты на Undo после смены выделения для всех трёх VM

## Capabilities

### New Capabilities
- `properties-undo`: Корректный Undo для изменений свойств объектов (LineType, координаты, цвета, шрифты и др.) через панель свойств

### Modified Capabilities

(нет изменений существующих spec-уровней)

## Impact

- `ViewModels/LinePropertiesViewModel.cs` — все 7 relay-команд
- `ViewModels/RectanglePropertiesViewModel.cs` — все 8 relay-команд
- `ViewModels/TextPropertiesViewModel.cs` — все 10 relay-команд + 3 inline-команды
- `Tests/ViewModels/PropertiesViewModelUndoTests.cs` — новый файл с тестами
