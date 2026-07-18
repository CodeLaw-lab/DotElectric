## Context

The grid-optimization change introduced viewport culling in `GridManager.RefreshGridNodes()`: grid nodes are only generated for the area returned by `GetViewportMicrons(2.0)` (viewport + ½-screen margin in each direction). While this saves nodes during zoomed-in panning, it breaks the `zoom-display-grid` requirement that the grid covers the **full sheet** at all times.

**Current call chain:**
```
RefreshGridNodes()
  → GetViewportMicrons(2.0)     ← viewport + margin
  → SelectDisplayStep(...)      ← adaptive step
  → GenerateGridNodes(...)      ← generates only for viewport area
  → long[] pool copy            ← node data to shared buffer
  → InvalidateVisual()          ← GridNodesLayer.OnRender
```

**Problem scenario (A4 portrait, zoom 7.51×, 5mm step):**
- `GetViewportMicrons(2.0)` returns Y-range [81.3mm, 297mm]
- Bottom 81.3mm (27%) of sheet has no grid nodes
- At max scroll, a 63px strip at viewport bottom lacks grid

## Goals / Non-Goals

**Goals:**
- Grid nodes cover the full sheet at all zoom levels and pan positions
- Keep performance improvements from grid-optimization (long[] pooling, debounce, SelectDisplayStep, GridNodeVm removal)
- Node count not to exceed `MaxGridNodes` (100,000) — guaranteed by `SelectDisplayStep`
- Minimize code changes (ideally 1–2 files)

**Non-Goals:**
- Eliminating all temporary allocations in the grid pipeline (separate optimization)
- Changing `GridNodesLayer` rendering approach
- Modifying grid line rendering (`GenerateGridLines`/`GenerateVisibleGridLines` not affected)

## Decisions

### Decision 1: Replace viewport bounds with full-sheet bounds

**Choice:** Change line 101 in `GridManager.cs` from:
```csharp
var (vpLeft, vpBottom, vpWidth, vpHeight) = _zoomPanManager.GetViewportMicrons(2.0);
```
to:
```csharp
long vpLeft = 0, vpBottom = 0;
long vpWidth = _template.Sheet.WidthMicrons;
long vpHeight = _template.Sheet.HeightMicrons;
```

**Rationale:**
- Simplest possible fix — 1 line changed in 1 file
- `SelectDisplayStep` prevents overflow (keeps nodes ≤ 100K)
- `GenerateGridNodes` already clamps to sheet bounds
- All existing tests pass with this change

**Rejected alternatives:**
- **Increase margin beyond 2.0**: doesn't scale — at zoom 10× on A0, margin 14× would produce 700K nodes → empty list anyway
- **Hybrid gap-filling**: generate viewport nodes + missing sheet portion → overcomplex for no benefit over full-sheet
- **Clip-based rendering in GridNodesLayer**: keeps viewport-limited generation but clips in GPU → still allocates full node buffer; doesn't reduce allocation pressure

### Decision 2: Add reusable List<GridNode> parameter

**Choice:** Add an optional `List<GridNode>? reuseList` parameter to `GenerateGridNodes`. When provided, the list is reused instead of allocating a new one. GridManager holds a permanent `_nodeBuffer` and passes it on each call.

```csharp
public static List<GridNode> GenerateGridNodes(
    Sheet sheet, long stepMicrons, double zoom,
    long vpLeft, long vpBottom, long vpWidth, long vpHeight,
    List<GridNode>? reuseList = null)
```

**Rationale:**
- Eliminates temporary `List<GridNode>` allocation on every refresh
- For worst case (A0, zoom 10×, 5mm step): saves 640KB allocation per frame
- Backward-compatible change (optional parameter)

**Risks:**
- The returned list is still read by GridManager after generation. `List.Clear()` preserves capacity, so the underlying array is reused.

## Data / Performance

**Worst case (A0 landscape, zoom 10×, 5mm step):**
- `SelectDisplayStep` picks 5mm (50px spacing ≥ 5, cols×rows = 239×169 = 40,391 ≤ 100K ✓)
- `long[]` pool: 80,782 elements × 8 bytes = 646KB (reused)
- `List<GridNode>`: 40,391 × 16 bytes = 646KB (eliminated by Decision 2)
- `OnRender`: 40K `DrawEllipse()` calls → ~2ms per frame

**Typical case (A4 portrait, zoom 1×, 5mm step):**
- 43×60 = 2,580 nodes
- `OnRender`: ~0.2ms

**Refresh rate:**
- Debounce at `DispatcherPriority.Render` (~16ms between refreshes)
- Grid is also invalidated during scroll/zoom animation (WPF scroll events fire continuously)
- Full-sheet + reuse buffer keeps per-frame allocation at 0 bytes after initial pool fill

## Risks / Trade-offs

- **[Rare edge case]** A0 at zoom 10× with 5mm step → 40K nodes → 2ms render time. Acceptable because: (a) debounce limits to 60fps, (b) scenario requires A0 + max zoom simultaneously (unusual for template editing).
- **[No regression]** Existing `GridHelperTests` and `GridManagerTests` verify correctness numerically. One test needs renaming (`RefreshGridNodes_ViewportCulling_GeneratesLimitedNodes` → `RefreshGridNodes_FullSheet_GeneratesExpectedNodes`). Assertions pass without changes.
- **[Semantic coupling]** `GenerateGridNodes` parameter `reuseList` adds optional coupling to caller. The alternative (separate overload) would double the API surface.
