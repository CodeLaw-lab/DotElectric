> **ATTENTION:** This task list described the **original approach** (cached-region tracking + debounce + `InvalidateCacheOnPan`/`RefreshOnPanEnd`). The actual implementation replaced this with a **simpler architecture** (micron-only storage in GridManager, OnRender handles zoom/Y-flip in GridNodesLayer, pan via RenderTransform only). See `AGENTS.md` → "Grid refactoring (Points 1, 4, 5)" for the actual implementation.
>
> Sections **3.4–3.7**, **4** and **5** were resolved differently:
> - Cached-region tracking → **removed entirely** (pan via RenderTransform, no regeneration during pan)
> - Debounce → **removed** (regeneration only on zoom/step/pan-end, no MouseMove trigger)
> - `RefreshOnPanEnd` → direct `RefreshGridNodes()` call in `CanvasInputRouter.RouteMouseUp`
> - ZoomPanManager pan callback → **removed** (no pan-triggered regeneration needed)

## 1. Constants

- [ ] 1.1 Change `EditorSettings.MinPixelSpacing` from `5.0` to `0.5`
- [ ] 1.2 Change `EditorSettings.MaxGridNodes` from `100000` to `250000`
- [ ] 1.3 Build and verify no compilation errors

## 2. GridHelper — target-based step selection

- [ ] 2.1 Add nice-step sequence array `[50000, 30000, 20000, 15000, 10000, 7000, 5000, 3000, 2000, 1500, 1000, 700, 500]` (microns) as a private static field in `GridHelper`
- [ ] 2.2 Implement `ComputeDisplayStep(zoom, maxNodes, sheetWidthMicrons, sheetHeightMicrons, viewportWidthMicrons, viewportHeightMicrons)` — computes `targetStep = 0.5 / zoom` (mm), snaps to nearest nice step, then iterates coarser until node count ≤ maxNodes
- [ ] 2.3 Remove old `SelectDisplayStep(baseStepMicrons, zoom, viewportWidthMicrons, viewportHeightMicrons, maxNodes)` method
- [ ] 2.4 Remove old `CalculateOptimalStep(baseStepMicrons, zoom)` method (no longer needed anywhere, used only by `SelectDisplayStep`)
- [ ] 2.5 Update `GenerateGridNodes` and `GenerateVisibleGridLines` to use the `ComputeDisplayStep` logic internally — or keep as-is and let callers pass the already-computed step (preferred for separation of concerns)

## 3. GridManager — viewport-culled node generation

- [ ] 3.1 Add fields `_cachedRegionLeftMicrons` and `_cachedRegionBottomMicrons` to track the cached viewport origin
- [ ] 3.2 Rewrite `RefreshGridNodes()` — replace hardcoded `vpLeft=0, vpBottom=0, vpWidth=full, vpHeight=full` with `_zoomPanManager.GetViewportMicrons(1.5)`
- [ ] 3.3 Replace call to `GridHelper.SelectDisplayStep` with call to `GridHelper.ComputeDisplayStep`, passing viewport dimensions and sheet bounds
- [ ] 3.4 Add `IsWithinCachedRegion(double margin = 1.5)` — returns `true` if current viewport (expanded by margin) fits within the cached region
- [ ] 3.5 Add `InvalidateCacheOnPan()` method — checks `IsWithinCachedRegion()`, if `false` calls debounced `RefreshGridNodes()` (50ms debounce)
- [ ] 3.6 Add `RefreshOnPanEnd()` — public method called from mouse-up handlers to force a grid refresh after pan ends
- [ ] 3.7 Store the cached-region origin after each successful `RefreshGridNodes()` call

## 4. ZoomPanManager — pan triggers

- [ ] 4.1 Add optional `Action? _onPanRefresh` callback field
- [ ] 4.2 Add `SetPanRefreshCallback(Action callback)` method
- [ ] 4.3 Call `_onPanRefresh?.Invoke()` in `OnPanOffsetXChanged` and `OnPanOffsetYChanged` (the pan-triggered grid refresh method handles its own debounce)
- [ ] 4.4 Ensure `_onPanRefresh` is not called during programmatic `SetScrollX`/`SetScrollY` (only during direct `PanCanvas` or mouse-driven scroll)

## 5. CanvasInputRouter — pan-end notification

- [ ] 5.1 In `RouteMouseUp()`, after panning ends (`state.IsPanning` becomes `false`), call `state.Editor.GridManager.RefreshOnPanEnd()`
- [ ] 5.2 Expose `GridManager.RefreshOnPanEnd()` as public or internal

## 6. Test updates

- [ ] 6.1 Update `GridHelperTests.SelectDisplayStep_*` — rename to `ComputeDisplayStep_*` and update expected values for new nice-step sequence
- [ ] 6.2 Update `GridHelperTests.CalculateOptimalStep_*` — remove or repurpose these tests (method deprecated)
- [ ] 6.3 Add `ComputeDisplayStep_A0zoom100_target1mm` — verifies target selection and node budget capping
- [ ] 6.4 Add `ComputeDisplayStep_A4zoom100_target1mm` — verifies 0.5mm step fits inside 250K
- [ ] 6.5 Add `ComputeDisplayStep_allNiceSteps` — parameterized test verifying each nice step is selected at the correct zoom level
- [ ] 6.6 Add `ComputeDisplayStep_nodeBudgetExceeded` — verifies coarsening when cols×rows > maxNodes
- [ ] 6.7 Update `GenerateGridNodes_1mmStep_AtZoom1_ReturnsEmpty` — after MinPixelSpacing change, 1mm at zoom 1.0 should pass MinPixelSpacing, so this test needs a new assertion or changed params
- [ ] 6.8 Add `GenerateGridNodes_viewportCulled_A0` — verifies nodes are generated only within viewport region on large format
- [ ] 6.9 Update `GridManagerTests.RefreshGridNodes_AdaptsStepAtLowZoom` — verify new step selection values
- [ ] 6.10 Add `GridManagerTests.RefreshGridNodes_viewportCulled` — verify nodes match viewport region (not full-sheet)
- [ ] 6.11 Add `GridManagerTests.CachedRegion_inside_NoRefresh` — verify no regeneration when pan stays within margin
- [ ] 6.12 Add `GridManagerTests.CachedRegion_outside_TriggersRefresh` — verify regeneration when pan exits margin

## 7. Build and verify

- [ ] 7.1 Build solution — `dotnet build src/DotElectric.TemplateEditor.slnx` — 0 errors, 0 warnings
- [ ] 7.2 Run tests — `dotnet test src/DotElectric.TemplateEditor.Tests` — all tests pass
- [ ] 7.3 Manual verification: launch app, create A4 sheet, configure 1mm grid step, zoom to 50% — grid dots should be visible
- [ ] 7.4 Manual verification: pan across A0 sheet at 100% zoom — grid should regenerate smoothly without flickering at cache boundary
