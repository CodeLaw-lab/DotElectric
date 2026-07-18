## Why

EditorViewModel (~1194 строк) содержит 43 forwarding-свойства, которые чисто делегируют к 9 менеджерам, и 4 PropertyChanged-обработчика, которые ретранслируют INPC-уведомления от менеджеров обратно на EditorViewModel. Эта прослойка существовала, потому что XAML биндился к свойствам EditorViewModel (например, `{Binding Zoom}`), а не к менеджерам (`{Binding ZoomPanManager.Zoom}`). Спринт R3.1 уже перевёл XAML на прямые биндинги к менеджерам, но forwarding-свойства и обработчики остались — они стали мёртвым грузом. Удаление ~290 строк бойлерплейта уменьшит EditorViewModel до ~904 строк, устранит источник багов типа Sprint 48 (забытый проброс dirty indicator) и сделает добавление новых свойств в менеджеры дешёвым (не требует правки EditorViewModel).

## What Changes

- **Удалить** 43 forwarding-свойства из EditorViewModel (Zoom, PanOffset, ActiveTool, PreviewLine, SelectionBox*, IsDirty, FilePath, DisplayName и др.)
- **Удалить** 4 PropertyChanged-обработчика с подписками/отписками (перенаправляли INPC от менеджеров)
- **Перевести** 3 свойства (IsDirty, FilePath, DisplayName) на explicit implementation IAutosaveTab
- **Упростить** OnSelectionChangedInternal (убрать ретрансляцию ShowSelectionMarkers, SingleSelectedObject)
- **Удалить** OnZoomChangedInternal (ретрансляция Zoom, CanvasWidthPixels, CanvasHeightPixels, ZoomPercent больше не нужна)
- **Исправить** 2 XAML-биндинга в EditorCanvas.xaml (ScrollYRange, ScrollXRange → ZoomPanManager.ScrollYRange, ZoomPanManager.ScrollXRange)
- **Исправить** 5 вызовов в EditorCanvas.xaml.cs (vm.Zoom → vm.ZoomPanManager.Zoom и т.д.)
- **Исправить** 2 вызова в CanvasInputRouter.cs (editor.Zoom → editor.ZoomPanManager.Zoom, editor.ActiveTool → editor.ToolManager.ActiveTool)
- **Исправить** 9 вызовов в MainViewModel.cs (tab.FilePath → tab.DirtyStateManager.FilePath и т.д.)
- **Исправить** внутренние usage forwarding-свойств внутри EditorViewModel (~6 мест)

## Capabilities

### New Capabilities

- *(none — чистый рефакторинг, никакой новой функциональности)*

### Modified Capabilities

- *(none — поведение не меняется)*

## Impact

- `ViewModels/EditorViewModel.cs` — −~290 строк (1194 → ~904)
- `Views/EditorCanvas.xaml` — 2 изменённых биндинга
- `Views/EditorCanvas.xaml.cs` — 5 изменённых вызовов
- `Behaviors/CanvasInputRouter.cs` — 2 изменённых вызова
- `ViewModels/MainViewModel.cs` — 9 изменённых вызовов
- Возможно: `Tests/*.cs` — N изменений, если тесты обращаются к удаляемым forwarding-свойствам
- **BREAKING**: Никаких изменений публичного API или форматов данных. Только internal-рефакторинг.
