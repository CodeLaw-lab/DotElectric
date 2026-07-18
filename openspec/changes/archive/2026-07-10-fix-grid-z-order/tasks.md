## 1. XAML — Swap Z-order

- [x] 1.1 In `EditorCanvas.xaml`, move `GridNodesLayer` element AFTER `DrawingCanvas` in the shared Grid (GridNodesLayer renders on top)

## 2. Code-behind — Remove clip subscriptions

- [x] 2.1 In `EditorCanvas.xaml.cs`, remove `OnZoomPanPropertyChanged` method
- [x] 2.2 In `EditorCanvas.xaml.cs`, remove `UpdateGridClip` method
- [x] 2.3 In `EditorCanvas.xaml.cs`, remove `oldVm.ZoomPanManager.PropertyChanged -= OnZoomPanPropertyChanged` from `OnDataContextChanged`
- [x] 2.4 In `EditorCanvas.xaml.cs`, remove `newVm.ZoomPanManager.PropertyChanged += OnZoomPanPropertyChanged` from `OnDataContextChanged`
- [x] 2.5 In `EditorCanvas.xaml.cs`, remove `UpdateGridClip(newVm)` call from `OnDataContextChanged`
- [x] 2.6 Clean up unused `using DotElectric.TemplateEditor.Models` import (Coordinate no longer used)

## 3. GridNodesLayer — Remove SetSheetClip

- [x] 3.1 In `GridNodesLayer.cs`, remove `SetSheetClip()` method

## 4. Verify

- [x] 4.1 `dotnet build src/DotElectric.TemplateEditor.slnx` — 0 errors, 0 warnings
- [x] 4.2 `dotnet test src/DotElectric.TemplateEditor.Tests` — 1866 passed, 0 failed, 1 pre-existing skip
