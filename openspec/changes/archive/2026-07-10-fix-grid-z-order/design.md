## Context

The grid layer (`GridNodesLayer`) was moved outside `DrawingCanvas` in a previous change (grid-layer-outside-canvas) to prevent `Canvas.ClipToBounds` from clipping grid dots during pan/zoom. After the move, `GridNodesLayer` was placed as the first child of the parent `Grid`, rendering BEHIND `DrawingCanvas`. `DrawingCanvas` has `Background="White"` (opaque), which completely covers all grid dots in the sheet area.

Additionally, a `Clip` (`RectangleGeometry`) was applied to `GridNodesLayer` to keep dots off the gray margin. However, the clip rect `(0,0,w,h)` is in `GridNodesLayer`'s local coordinate space, which may not match the `DrawingCanvas` position when the viewport is larger than the sheet (centering offset in the Grid). This causes dots to appear on the gray margin or the clip to cut at the wrong position.

Current rendering pipeline:

```
GridNodesLayer (back) → Clip → RenderTransform → white DrawingCanvas covers everything
```

## Goals / Non-Goals

**Goals:**
- Grid dots visible on the white sheet area at all zoom/pan settings
- No grid dots visible on the gray border (margin) outside the sheet
- No performance regression
- No interaction change (dots must not block mouse events)

**Non-Goals:**
- Changing the grid generation algorithm (full-sheet generation stays)
- Changing the coordinate system or pixel conversion
- Adding new visual features to the grid

## Decisions

### Decision 1: Swap Z-order instead of fixing the clip

| Approach | Pros | Cons |
|----------|------|------|
| **Swap order** (GridNodesLayer after DrawingCanvas) | Simple, minimal diff, removes complexity (clip, subscriptions) | GridNodesLayer must still avoid blocking mouse input |
| **Fix clip position** (align with DrawingCanvas origin) | Keeps clip as defense-in-depth | Need to track centering offset, more complex, still covered by white background |
| **Make DrawingCanvas transparent** | Dots visible naturally | White background from Border lost, affects all object rendering |

**Chosen:** Swap Z-order + remove clip entirely. Grid nodes are already constrained by `GridManager` to `[0, sheetWidthMm*zoom] × [0, sheetHeightMm*zoom]`, so no clipping is needed. The layer is transparent except for 2×2px dots.

### Decision 2: Remove SetSheetClip() and PropertyChanged subscription

The clip was introduced to prevent dots on the gray margin. After swapping Z-order, the clip is counterproductive — if misaligned it clips visible dots. Since `GridManager` already limits generated nodes to the sheet bounds, removal is safe. The `PropertyChanged` subscription (`OnZoomPanPropertyChanged`, `UpdateGridClip`) becomes dead code.

### Decision 3: Keep IsHitTestVisible = false

Already set in `GridNodesLayer` constructor. Ensures dots don't interfere with `DrawingCanvas` mouse events (selection, pan, drawing).

## Risks / Trade-offs

- **Risk:** Grid dots render ON TOP of template objects (lines, rectangles, text). **Mitigation:** dots are small (2×2px), subtle gray (#C0C0C0), and match standard CAD behavior where grid overlay is normal.
- **Risk:** At low zoom, `SelectDisplayStep` picks a coarse step (e.g., 25mm on A4 at 20%), showing very few dots — grid appears "strange" or empty. **Mitigation:** this is a pre-existing visual characteristic of the adaptive step algorithm, not related to Z-order. Separate tuning if needed.
- **Risk:** Performance — GridNodesLayer re-renders on every grid refresh (zoom/step changes). **Mitigation:** `OnRender` iterates pre-computed pixel data with no allocations; render cost is O(n) for n dots.
