## Why

После завершения Этапа 1 (44 FR, 1780 тестов) и рефакторинга R1–R4 (25 архитектурных замечаний устранены) в кодовой базе остался системный архитектурный долг: 5 слоёв избыточности, дублирования и непокрытых тестов, которые замедляют разработку Этапа 2 (Редактор УГО). Без устранения этого долга каждый новый функциональный блок будет множить те же паттерны.

## What Changes

### Слой 1 — Очистка команд (P1)
- **BREAKING**: `RotateObjectCommand` удаляется — заменяется на `ChangePropertyCommand<int>`
- **BREAKING**: `CustomResizeCommand` удаляется — заменяется на `ChangePropertyCommand<ResizeState>`
- **BREAKING**: `MoveObjectCommand` удаляется — заменяется на `ChangePropertyCommand<(long,long)>`
- **BREAKING**: `PasteObjectCommand` удаляется — сливается с `AddObjectCommand` (параметр nameOverride)
- `ChangePropertyCommand<T>` получает второй конструктор с явным `oldValue`
- Стек команд: 8 типов → 5 (+ инфраструктура)

### Слой 2 — Декомпозиция PropertiesViewModel (P1)
- `PropertiesViewModel` (649 строк) разбивается на 4 файла:
  - `PropertiesViewModel` (база, ~150 строк) — подписка, диспетчеризация, IDisposable
  - `LinePropertiesViewModel` (~100 строк) — Line-специфичные свойства и команды
  - `RectanglePropertiesViewModel` (~110 строк) — Rectangle-специфичные
  - `TextPropertiesViewModel` (~160 строк) — Text-специфичные
- `PropertiesPanelContent.xaml` переписывается: 3 StackPanel → ContentControl + DataTemplate

### Слой 3 — Очистка конвертеров (P2)
- Удаляется мёртвый дубль `InverseBoolConverter` (идентичен `NotConverter`)
- 4 файла `LocalX1/Y1/X2/Y2` → 1 `LineLocalConverter` (параметр)
- 4 файла EdgeMicrons → 2 файла
- 3 `EnumToIndex` → 1 generic `EnumToIndexConverter<T>`

### Слой 4 — Инфраструктура (P2)
- `IAutosaveTab` перемещается из `AutosaveService.cs` в `ViewModels/Abstractions/`
- `async void` в `MainViewModel.OnAutosaveTickHandler` → try/catch + ILogger
- `CustomResizeCommand` switch-фабрики удаляются (уже покрыты слоем 1)

### Слой 5 — Тесты (P2)
- 6 WPF-сервисов получают STA-тесты (через WpfContext)
- 7 файлов с покрытием <80% поднимаются до 80%+ (~59 тестов)

## Capabilities

### New Capabilities
— Нет новых пользовательских возможностей. Исключительно рефакторинг существующего кода.

### Modified Capabilities
— Нет изменений требований. Поведение системы сохраняется полностью.

## Impact

- **10 файлов удаляется**: RotateObjectCommand, CustomResizeCommand, MoveObjectCommand, PasteObjectCommand, InverseBoolConverter, LocalX1/X2/Y1/Y2, LeftEdgeMicronsConverter+Multi, TopEdgeMicronsConverter+Multi (частично)
- **4 файла создаётся**: LinePropertiesViewModel, RectanglePropertiesViewModel, TextPropertiesViewModel, LineLocalConverter
- **Изменения в ~25 файлах**: EditorViewModel, SelectTool, ResizeTool, PropertiesPanelContent.xaml, App.xaml, и др.
- **+105 тестов**: coverage ~82% → ~87%
- **-542 строки кода**: общее сокращение без потери функциональности
- **0 новых зависимостей**: только удаление/рефакторинг существующего кода
