## Context

The grid rendering system currently generates nodes for the entire sheet using a hardcoded `MinPixelSpacing = 5.0` (mm on screen) threshold and a multiplier chain `×1,×2,×5,×10,×20,×50,×100,×200,×500` to coarsen steps at low zoom. `MaxGridNodes = 100 000` limits the node buffer. The `ZoomPanManager` already exposes `GetViewportMicrons(margin)` but it is never called — `GridManager.RefreshGridNodes()` passes full-sheet bounds unconditionally. Panning does not trigger grid regeneration (`OnPanOffsetXChanged`/`OnPanOffsetYChanged` skip `_onGridRefresh()`).

The result: 1mm and 2mm steps are invisible below 500% zoom; grid steps ≤5mm are invisible below 100% zoom. Users configuring 1mm snap see no corresponding visual grid.

## Goals / Non-Goals

**Goals:**
- Make grid steps at least `step × zoom ≥ 0.5 mm on screen` for all user-configured steps (0.5mm–50mm), all GOST formats (A4–A0), and all zoom levels (10%–1000%)
- Switch from full-sheet to viewport-based node generation using `GetViewportMicrons(1.5)` to keep node counts manageable and enable finer steps on large formats
- Raise `MaxGridNodes` to 250 000 to support 0.5mm step on A4/A3 at high zoom
- Replace the rigid multiplier chain with a target-based formula that snaps to a flexible nice-step sequence
- Add pan-triggered grid regeneration when the cached region (margin buffer) is exhausted
- Update all existing tests for new step-selection values; add new tests for viewport culling, pan triggers, and format-specific node budgets

**Non-Goals:**
- No UI for configuring `MinPixelSpacing` or `MaxGridNodes` — remain source constants
- No changes to `GridNodesLayer` rendering (DrawingContext `DrawEllipse`)
- No changes to the snap-to-grid logic (`Coordinate.SnapToGrid`)
- No changes to serialization or file format
- Grid line mode (vs node mode) is not revived

## Decisions

### Decision 1: Threshold constant — `MinPixelSpacing = 0.5`

Replace `5.0` with `0.5` (mm on screen). The current value of 5.0 corresponds to ~19px at 96 DPI; 0.5 corresponds to ~1.9px — the minimum at which 2×2px dots are visually distinguishable as a grid pattern without merging into a solid fill.

**Rationale:** This single constant change achieves the user's ladder:
- `5mm × 10% = 0.5` → visible at 10%
- `2mm × 25% = 0.5` → visible at 25%
- `1mm × 50% = 0.5` → visible at 50%
- `0.5mm × 100% = 0.5` → visible at 100%

**Alternatives considered:**
- `MinPixelSpacing = 3.0`: insufficient — 1mm at 50% = 0.5 < 3, still hidden
- `MinPixelSpacing = 2.0`: 1mm at 100% = 1.0 < 2, still hidden
- `MinPixelSpacing = 0.3`: allows 0.5mm at 50%, but node counts explode — 0.5mm on A0 at 50% would exceed even 500K budget

### Decision 2: Step selection — target-based formula

Replace `SelectDisplayStep(baseStep, zoom, vpWidth, vpHeight, maxNodes)` with `ComputeDisplayStep(zoom, maxNodes, sheetWidth, sheetHeight, viewportWidth, viewportHeight)` that:

1. Computes `targetStepMm = 0.5 / zoom` (the step that gives exactly `MinPixelSpacing` at this zoom)
2. Snaps to the nearest nice step from the sequence `[50, 30, 20, 15, 10, 7, 5, 3, 2, 1.5, 1, 0.7, 0.5]` mm — going DOWN (finer) if node budget allows, or UP (coarser) if budget is exceeded
3. Returns the selected step in microns

**Rationale:** The current multiplier chain `{1,2,5,10,20,50,100,200,500}` produces gaps (e.g., `×4` is missing so 2mm → 5mm means a 2.5× jump). A target-based formula with a dense nice-step sequence avoids this: steps are always a round CAD-friendly value, and the sequence transitions smoothly.

**Alternatives considered:**
- Keep multiplier chain but add `{1,1.5,2,3,5,7,10,15,20,30,50}` multipliers: simpler but less flexible — cannot handle base steps that are already non-standard
- Linear interpolation between fixed points: over-engineered for this use case

### Decision 3: Viewport margin `1.5×`

`GridManager.RefreshGridNodes()` calls `_zoomPanManager.GetViewportMicrons(1.5)` instead of full-sheet bounds.

**Rationale:**
- A 1.5× margin means the generated region extends 25% beyond each edge of the viewport
- During panning, the user can scroll 25% of viewport in any direction before hitting un-cached area
- This reduces node count by ~4–10× on large formats compared to full-sheet generation (e.g., A0 at 100% zoom: ~500×280mm viewport vs 1189×841mm full sheet)
- At high zoom on large formats, viewport is much smaller than the sheet, so margin approach yields dramatic node savings

**Alternatives considered:**
- Margin 2.0×: double buffer size, but halves available node budget for a single frame
- No margin (exact viewport): requires regeneration on every pixel of pan — too expensive for smooth dragging
- Full-sheet with raised `MaxGridNodes`: impractical — A0 at 0.5mm would still need 4M nodes

### Decision 4: Pan-triggered regeneration

Add an `(LeftMicrons, BottomMicrons)` cached-region origin to `GridManager`. On pan, check if the current viewport (expanded by margin) is still within the cached region. If not, call `RefreshGridNodes()` with a 50ms debounce. On pan end (MouseUp), always refresh once.

**Rationale:**
- Current code does not refresh on pan at all (full-sheet made it unnecessary). With viewport mode, pan reveals areas not yet in the buffer.
- Debounce prevents grid regeneration on every 60fps MouseMove frame — only regenerates when pan actually needs new data.
- Pan-end refresh catches the edge case where the user stops exactly at the cache boundary.

**Alternatives considered:**
- Regenerate on every pan frame: too expensive (SelectDisplayStep + GenerateGridNodes + pixel conversion + InvalidateVisual)
- Regenerate only on MouseUp: visible missing-dot grid during drag

### Decision 5: `MaxGridNodes = 250 000`

Raise from 100 000 to 250 000. Buffer grows from 200 000 × 8 bytes = 1.6 MB to 500 000 × 8 bytes = 4.0 MB.

**Rationale:**
- A4 with 0.5mm step at 100% zoom: `cols×rows = 421×595 = 250 495` — fits with ~2000 nodes to spare
- A3 with 1mm step at 100% zoom: `cols×rows = 299×421 = 125 879` — fits comfortably
- A2 with 2mm step at 100% zoom: `cols×rows = 211×298 = 62 878` — fits
- Combined with viewport culling, this covers the user's ladder for A4 and A3 at high zoom, and provides fine steps for larger formats within the viewport

**Alternatives considered:**
- 500 000 (8 MB): would cover A3 at 0.5mm, but memory doubling is hard to justify for the edge case
- 100 000 (current): keeps 0.5mm hidden on A4 at 100% — defeats the purpose of the change

## Risks / Trade-offs

- **[Performance] Grid regeneration during pan**: If debounce is too short, regeneration fires on every few frames causing stutter during fast panning. → Mitigation: 50ms debounce + margin buffer means regeneration happens at most once every ~200ms of panning (after moving 25% viewport width).
- **[Memory] 4 MB node buffer**: On low-end hardware (2 GB RAM), a permanent 4 MB allocation is negligible. → No mitigation needed.
- **[Correctness] Viewport-culled nodes may flicker at cache boundary**: If the margin is too small, nodes pop in at the edge during slow panning. → Mitigation: 1.5× margin provides 25% buffer; increase to 2.0× if flickering observed.
- **[Test] Existing step-selection tests will break**: All `SelectDisplayStep` assertions assume the old multiplier chain. → Mitigation: rewrite tests to use `ComputeDisplayStep` with the nice-step sequence. New expected values derived from the target formula.
- **[Edge] Zero or negative viewport**: `GetViewportMicrons` already handles `_viewportWidthPx ≤ 0` returning `(0,0,0,0)`. `GenerateGridNodes` returns empty for zero-sized viewport. → Safe.
