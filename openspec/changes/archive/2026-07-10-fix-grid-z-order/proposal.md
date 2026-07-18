## Why

Grid nodes (cross-dot pattern at sheet grid intersections) are invisible within the sheet area. The `GridNodesLayer` was moved outside `DrawingCanvas` in a previous refactoring to avoid `Canvas.ClipToBounds` clipping the dots, but the layer renders BEFORE `DrawingCanvas` in the visual tree. `DrawingCanvas` has an opaque `Background="White"` that completely covers all grid dots within the sheet. Grid dots should be visible on the sheet (like any CAD editor).

## What Changes

- Swap sibling order in `EditorCanvas.xaml`: `GridNodesLayer` moves AFTER `DrawingCanvas` so it renders on top of the white background
- Remove `SetSheetClip()` and the clip subscription (`OnZoomPanPropertyChanged`, `UpdateGridClip`) — dots are already constrained by `GridManager` to the sheet extent; the clip rect position may not match `DrawingCanvas` when the viewport is larger than the sheet, causing dots on the gray margin
- Ensure `GridNodesLayer.IsHitTestVisible = false` (already set) so dots don't interfere with mouse events

## Capabilities

### New Capabilities

*(none — purely a rendering fix, no new capability)*

### Modified Capabilities

*(none — no spec-level requirement changes)*

## Impact

- `Views/EditorCanvas.xaml` — swap GridNodesLayer / DrawingCanvas order, remove clip bindings
- `Views/EditorCanvas.xaml.cs` — remove `OnZoomPanPropertyChanged`, `UpdateGridClip`, unsubscribe in `OnDataContextChanged`
- `Views/GridNodesLayer.cs` — remove `SetSheetClip()` method (no longer needed)
