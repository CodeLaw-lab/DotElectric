## Why

Кнопка «Вписать в экран» (и Ctrl+0) не работают — команда не выполняется. Причина: метод команды назван `FitToScreenCommand`, и CommunityToolkit.Mvvm source generator добавляет ещё один суффикс `Command`, создавая свойство `FitToScreenCommandCommand`. XAML биндится к `FitToScreenCommand`, но такого свойства нет — binding молча падает.

## What Changes

- Переименовать `EditorViewModel.FitToScreenCommand(string? parameter)` → `FitToScreen(string? parameter)`
- После переименования source generator создаст `FitToScreenCommand` — XAML биндинги **не требуют изменений**, т.к. уже используют `FitToScreenCommand`
- Удалить `FitToScreen(double, double)` public overload (используется только внутри, логика будет в одном методе)

## Capabilities

### New Capabilities
(нет новых capability — это багфикс существующей функциональности)

### Modified Capabilities
(нет изменений spec-level требований)

## Impact

- **EditorViewModel.cs:633** — переименовать метод
- **EditorViewModel.cs:611** — удалить public `FitToScreen(double, double)` делегирование
- **XAML** — без изменений
- **ZoomPanManager.cs** — без изменений
