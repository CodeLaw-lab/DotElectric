## Why

Маркеры выделения не отображаются при выборе объектов. `SelectionManager.ShowSelectionMarkers` — computed property без `PropertyChanged`. XAML биндится к нему напрямую (после R3.1), но уведомление никогда не приходит — `ItemsControl` с маркерами навсегда `Collapsed`.

## What Changes

- `SelectionManager.cs`: добавить `OnPropertyChanged(nameof(ShowSelectionMarkers))` в обработчик `CollectionChanged` коллекции `SelectedObjects`
- Никаких изменений в XAML, EditorViewModel, инструментах или тестах

## Capabilities

### New Capabilities
*(none — pure implementation fix)*

### Modified Capabilities
*(none — spec-level behaviour unchanged)*

## Impact

- **Файл:** `ViewModels/Managers/SelectionManager.cs` — 1 строка
- **Никаких изменений** в XAML, EditorViewModel, инструментах
- **Тесты:** без изменений, все зелёные
