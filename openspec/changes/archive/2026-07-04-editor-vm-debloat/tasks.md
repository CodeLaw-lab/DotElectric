## 1. External references (breaking changes first)

- [x] 1.1 EditorCanvas.xaml: перенаправить 2 оставшихся биндинга (`ScrollYRange`, `ScrollXRange` → `ZoomPanManager.ScrollYRange`, `ZoomPanManager.ScrollXRange`)
- [x] 1.2 CanvasInputRouter.cs: заменить `editor.Zoom` на `editor.ZoomPanManager.Zoom`, `editor.ActiveTool` на `editor.ToolManager.ActiveTool`
- [x] 1.3 EditorCanvas.xaml.cs: заменить `vm.Zoom` → `vm.ZoomPanManager.Zoom`, `vm.ViewportWidthMm` → `vm.ZoomPanManager.ViewportWidthMm`, `vm.ViewportHeightMm` → `vm.ZoomPanManager.ViewportHeightMm`
- [x] 1.4 EditorCanvas.xaml.cs: заменить `vm.GridInvalidated` → `vm.GridManager.GridInvalidated`, `vm.GridNodes` → `vm.GridManager.GridNodes`
- [x] 1.5 MainViewModel.cs: заменить `tab.FilePath` → `tab.DirtyStateManager.FilePath` (6 мест)
- [x] 1.6 MainViewModel.cs: заменить `tab.DisplayName` → `tab.DirtyStateManager.DisplayName` (3 места)
- [x] 1.7 MainViewModel.cs: заменить `tab.IsDirty` → `tab.DirtyStateManager.IsDirty` (1 место)

## 2. EditorViewModel: внутренние замены перед удалением

- [x] 2.1 Заменить `SelectedObjects` на `_selectionManager.SelectedObjects` внутри EditorViewModel
- [x] 2.2 Заменить `ActiveTool = tool` на `_toolManager.ActiveTool = tool`
- [x] 2.3 Заменить `StatusMessage =` на `_statusBarManager.StatusMessage =`
- [x] 2.4 Заменить `DisplayName =`, `FilePath =`, `IsDirty =` на `_dirtyStateManager.* =`

## 3. EditorViewModel: удаление forwarding-свойств

- [x] 3.1 Удалить ZoomPanManager-свойства (кроме `Zoom` — нужен IEditorContext): `CanvasWidthPixels`, `CanvasHeightPixels`, `PanOffsetX/Y`, `ZoomPercent`, `ViewportWidth/HeightMm`, `ViewportWidth/HeightPixels`, `ScrollX/YRange`, `ScrollX/YValue`, `IsCentered`, `CanvasOffsetX/Y` — 15 удалено, 1 (`Zoom`) упрощён до get-only
- [x] 3.2 Удалить `ShowSelectionMarkers`; `SelectedObjects`, `SingleSelectedObject` оставлены для IEditorContext
- [x] 3.3 Удалить `GridNodes`, `GridInvalidated`
- [x] 3.4 Удалить `StatusBarSheetFormat`, `StatusBarGridEnabled`, `StatusBarGridStepMm`, `StatusBarSnapEnabled`; `StatusMessage` оставлен для IEditorContext
- [x] 3.5 Удалить `ActiveTool` (XAML биндится к `ToolManager.ActiveTool`)
- [x] 3.6 Упростить `PreviewLine`, `PreviewRectangle`, `PreviewText`, `SelectionBox*`, `SelectionDirection` — убраны `OnPropertyChanged()` (XAML биндится к `PreviewManager.*`)
- [x] 3.7 Удалить `InlineEditingText`, `InlineEditText`

## 4. EditorViewModel: explicit IAutosaveTab + удаление обработчиков

- [x] 4.1 Удалить 4 поля-обработчика: `_zoomPanHandler`, `_previewHandler`, `_dirtyStateHandler`, `_toolManagerHandler`
- [x] 4.2 Удалить все подписки в конструкторе на PropertyChanged (кроме `_dirtyStateHandler` — тоже удалён как ненужный: XAML биндится к `DirtyStateManager.*`)
- [x] 4.3 Удалить отписки в Dispose
- [x] 4.4 Добавить explicit IAutosaveTab: `FilePath`, `DisplayName`, `IsDirty` (делегируют в DirtyStateManager)
- [x] 4.5 Удалить `OnZoomChangedInternal`; коллбэк в конструкторе заменён на `() => { }`
- [x] 4.6 Упростить `OnSelectionChangedInternal`: убрана ретрансляция `ShowSelectionMarkers`, `SingleSelectedObject`
- [x] 4.7 Удалить `OnPropertyChanged(nameof(StatusBarGridEnabled))` из ToggleGrid
- [x] 4.8 Удалить `OnPropertyChanged(nameof(StatusBarSnapEnabled))` из ToggleSnap

## 5. Тесты

- [x] 5.1 Исправить ~90 обращений к forwarding-свойствам в тестах на manager-свойства
- [x] 5.2 `dotnet test src/DotElectric.TemplateEditor.Tests` — 1780 passed, 1 skip
- [x] 5.3 5 упавших тестов PropertyChanged исправлены на проверку PreviewManager.PropertyChanged

## 6. Финальная проверка

- [x] 6.1 `dotnet build src/DotElectric.TemplateEditor.slnx` — 0 errors, 0 warnings
- [x] 6.2 `dotnet test src/DotElectric.TemplateEditor.Tests` — 1780 passed, 1 skip
- [ ] 6.3 UI smoke test: создать шаблон → нарисовать линию → zoom/pan → выделить → undo/redo → сохранить → открыть

## Summary

**EditorViewModel.cs: ~1194 → 784 строк (−410, −34%)**

| Удалено | Количество |
|---------|-----------|
| Forwarding-свойств (полностью) | ~25 |
| Forwarding-свойств (упрощено, без OnPropertyChanged) | ~15 |
| Поля-обработчики PropertyChanged | 4 |
| Подписки PropertyChanged в конструкторе | 4 |
| Блоки отписки в Dispose | 4 |
| `OnZoomChangedInternal` (весь метод) | 1 |
| `OnPropertyChanged` вызовов в ToggleGrid/ToggleSnap | 2 |
| `OnPropertyChanged` вызовов в OnSelectionChangedInternal | 2 |

**Сохранено (IEditorContext / IAutosaveTab):**
- 15 свойств как bare delegation (без `OnPropertyChanged()`)
- 3 explicit IAutosaveTab (делегируют в DirtyStateManager)

**Файлы, изменённые вне EditorViewModel:**
- `MainViewModel.cs` — 9 замен (DirtyStateManager)
- `EditorCanvas.xaml` — 2 замены (ZoomPanManager)
- `EditorCanvas.xaml.cs` — 5 замен (ZoomPanManager, GridManager)
- `CanvasInputRouter.cs` — 2 замены (ZoomPanManager, ToolManager)
- `PreviewLineChangedBehavior.cs` — переписан на PreviewManager.PropertyChanged
- `EditorViewModelTests.cs` — ~90 замен + using
