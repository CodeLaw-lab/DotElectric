## Why

After zooming in so scrollbars appear, scrolling, then zooming back out, the entire sheet content (including grid) is shifted down and to the right. `PanOffsetX/Y` retains a stale non-zero value because `CenterCanvas()` is never called when zoom changes back to a centered state. Both `DrawingCanvas` and `GridNodesLayer` inherit this stale offset via their shared `RenderTransform(CanvasOffsetX, CanvasOffsetY)`, causing visual drift.

## What Changes

- Call `CenterCanvas()` in `ZoomPanManager.OnZoomChanged()` whenever the zoom transition makes `IsCentered` true again, ensuring `PanOffsetX/Y` is reset to 0 for the centered state
- Add a unit test verifying that zoom from panned-out back to centered resets `PanOffsetX/Y` to 0

## Capabilities

### New Capabilities

*(none — behaviour fix, no new capability)*

### Modified Capabilities

*(none — no spec-level requirement changes)*

## Impact

- `ViewModels/Managers/ZoomPanManager.cs` — add `CenterCanvas()` call in `OnZoomChanged`
- `Tests/ViewModels/Managers/ZoomPanManagerTests.cs` — add regression test
