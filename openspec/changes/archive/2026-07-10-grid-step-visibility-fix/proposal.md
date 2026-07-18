## Why

Grid steps narrower than 5mm are invisible at most zoom levels due to `MinPixelSpacing = 5.0` — a threshold 10× coarser than practical for CAD work. At 100% zoom, 1mm and 2mm steps are indistinguishable (both converge to 10mm display step). At 50% zoom, even 5mm is hidden. Users configuring 1mm or 2mm snap see no corresponding visual grid, making precision alignment guesswork.

## What Changes

- Lower `MinPixelSpacing` from 5.0 to 0.5 mm on screen (matches user's ladder: `step × zoom ≥ 0.5`)
- Raise `MaxGridNodes` from 100 000 to 250 000 (enables 0.5mm step on A4/A3 at high zoom)
- Switch `GridManager.RefreshGridNodes()` from full-sheet generation to viewport-based generation using the existing `ZoomPanManager.GetViewportMicrons(margin)` with margin 1.5×
- Replace the multiplier-chain `SelectDisplayStep({1,2,5,10,20,50,…})` with a target-based formula: `displayStep = nearestNiceStep(0.5 / zoom)`, capped by node budget and format size
- Add pan-triggered grid regeneration when the visible area exceeds the cached margin buffer
- Expose `MinPixelSpacing` and `MaxGridNodes` as configurable constants (not UI-facing)

## Capabilities

### New Capabilities
- `grid-visibility`: Adaptive grid step selection that maintains legible visual density (`step × zoom ≥ 0.5`) across all zoom levels (10%–1000%), all GOST formats (A4–A0), and all user-configured steps (0.5mm–50mm). Includes viewport-culled node generation with pan/zoom-aware caching.

### Modified Capabilities
- *(none — no existing specs are modified)*

## Impact

- **GridHelper.cs**: `SelectDisplayStep()` rewritten; `MinPixelSpacing` constant changed; new helper `ComputeTargetDisplayStep()` added
- **GridManager.cs**: `RefreshGridNodes()` uses `_zoomPanManager.GetViewportMicrons(1.5)` instead of hardcoded full-sheet bounds; pan-triggered regeneration logic added
- **ZoomPanManager.cs**: `OnPanOffsetXChanged`/`OnPanOffsetYChanged` optionally call grid refresh when cached region is exhausted
- **EditorSettings.cs**: `MinPixelSpacing` changed from 5.0 to 0.5; `MaxGridNodes` changed from 100000 to 250000
- **EditorCanvas.xaml.cs**: No changes needed (GridNodesLayer receives updated data transparently)
- **Tests**: ~15 existing tests need updated assertions (new step selection values); ~10 new tests for viewport-culled generation, pan-triggered refresh, format-specific node budgets
