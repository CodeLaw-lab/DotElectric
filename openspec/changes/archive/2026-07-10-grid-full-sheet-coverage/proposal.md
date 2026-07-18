## Why

The grid-optimization change (viewport culling with margin 2.0) introduced a regression: at high zoom levels, the bottom portion of the sheet has no grid nodes, contradicting the existing `zoom-display-grid` spec requirement that "grid covers full sheet after zoom-in with scrollbars". This must be fixed to restore correct behavior.

## What Changes

- Replace `GetViewportMicrons(2.0)` with full-sheet bounds `(0, 0, sheetWidth, sheetHeight)` in `GridManager.RefreshGridNodes()`
- `SelectDisplayStep` already prevents node count overflow, so the 100K ceiling is preserved
- Add micro-optimization: reuse `List<GridNode>` buffer to avoid per-refresh allocation of temporary list
- Update `RefreshGridNodes_ViewportCulling_GeneratesLimitedNodes` test to reflect full-sheet behavior

## Capabilities

### New Capabilities

*(none — this restores an existing requirement)*

### Modified Capabilities

- `zoom-display-grid`: Requirement "Grid covers full sheet after zoom-in with scrollbars" was broken by grid-optimization viewport culling. The fix restores it while keeping performance improvements (pooling, debounce, SelectDisplayStep).

## Impact

- **GridManager.cs** (~1 line change): viewport → full-sheet bounds
- **GridHelper.cs** (~10 lines change): `GenerateGridNodes` accepts optional reusable `List<GridNode>` parameter to eliminate per-call allocation
- **GridNodesLayer.cs**: no changes (draws whatever nodes it receives)
- **GridManagerTests.cs**: rename/update viewport-culling test
- **GridHelperTests.cs**: no changes needed (existing tests pass with full-sheet)
