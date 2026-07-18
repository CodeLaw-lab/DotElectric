## Why

Three distinct zoom-related bugs degrade the user experience: (1) the zoom percentage in the status bar never updates regardless of zoom method, (2) the grid dots only cover the visible viewport area (not the full sheet) after zooming in past the scrollbar threshold, and (3) selecting a zoom value from the toolbar ComboBox produces a WPF binding conversion error ("Не удалось преобразовать значение '150%'") despite the zoom actually changing. These are all small, isolated bugs in the zoom notification and binding chain that collectively make the zoom feature feel broken.

## What Changes

- **Bug 1 — Status bar zoom display**: `ZoomPercent` computed property on `ZoomPanManager` will fire `PropertyChanged` when `Zoom` changes, so the status bar `TextBlock` binding re-evaluates.
- **Bug 2 — Grid not covering full sheet after zoom**: Grid node generation will switch from viewport-only back to full-sheet mode when the sheet is larger than the viewport, and will refresh on pan so scrolled-to areas get grid dots.
- **Bug 3 — Zoom ComboBox binding error**: The `Text` binding on the zoom ComboBox will be changed from two-way to one-way, since `ZoomComboBoxBehavior` already handles user input. This eliminates the binding conversion error while keeping all functionality.

## Capabilities

### New Capabilities
- `zoom-display-grid`: Covers zoom display updates in status bar, grid rendering at all zoom levels and pan positions, and toolbar ComboBox zoom input without binding errors.

### Modified Capabilities
- *(none — existing capabilities have no spec-level requirement changes)*

## Impact

**Files modified:**
- `ViewModels/Managers/ZoomPanManager.cs` — add `OnPropertyChanged(nameof(ZoomPercent))` in `OnZoomChanged`
- `ViewModels/Managers/GridManager.cs` — change grid generation from viewport-only to full-sheet when `IsCentered=false`; add refresh-on-pan hook
- `Behaviors/ZoomComboBoxBehavior.cs` — any adjustments needed for one-way binding
- `MainWindow.xaml` — change `ComboBox.Text` binding mode from default (two-way) to `OneWay`
- `Views/EditorCanvas.xaml.cs` — wire pan-end event to grid refresh

**Tests:**
- Existing zoom/grid tests updated for new behavior
- New tests for status bar zoom percent notification and ComboBox binding
