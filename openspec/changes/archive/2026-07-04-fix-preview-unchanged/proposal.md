## Why

Preview-объекты (Line, Rectangle, Text) перестали отображаться при рисовании после рефакторинга EditorViewModel (R3.1). `PreviewManager` использует `[ObservableProperty]`, который подавляет `PropertyChanged`, если устанавливается та же ссылка на объект. Инструменты мутируют тот же preview-объект на каждый `MouseMove` и переустанавливают его — но equality-check sourcegen-сеттера считает, что «значение не изменилось», и не шлёт событие. `PreviewLineChangedBehavior` не получает уведомление, canvas не обновляется.

## What Changes

- `PreviewManager`: заменить `[ObservableProperty]` на ручные сеттеры с **безусловным** `OnPropertyChanged()` для полей `_previewLine`, `_previewRectangle`, `_previewText`
- SelectionBox-поля (`_selectionBoxLeft`, `_selectionBoxBottom`, `_selectionBoxWidth`, `_selectionBoxHeight`, `_selectionBoxDirection`) **не трогать** — они value-типы, equality check работает корректно
- Тесты: 3 существующих теста `Preview*_RaisesPropertyChanged` и `SelectionBox*_RaisesPropertyChanged` уже проверяют `PreviewManager.PropertyChanged` — должны остаться зелёными

## Capabilities

### New Capabilities
*(none — pure implementation fix)*

### Modified Capabilities
*(none — spec-level behaviour unchanged)*

## Impact

- **Файл:** `ViewModels/Managers/PreviewManager.cs` — 3 поля (всего ~6 строк кода)
- **Никаких изменений** в инструментах, behavior, EditorViewModel или XAML
- **Тесты:** 0 изменений, все зелёные
