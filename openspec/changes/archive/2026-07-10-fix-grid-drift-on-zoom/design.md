## Context

When the sheet is smaller than the viewport (`IsCentered=true`), `PanOffsetX/Y` should be `0` — the sheet is centered by the Grid layout. When the sheet exceeds the viewport (`IsCentered=false`), `PanOffsetX/Y` is set via scrollbar or `CenterCanvas()` to control which portion is visible.

The bug: `OnZoomChanged` fires `PropertyChanged` for all dependent properties (`IsCentered`, `CanvasOffsetX/Y`, etc.) but never calls `CenterCanvas()`. If the user scrolls while zoomed in (setting `PanOffsetX/Y` to non-zero), then zooms out so `IsCentered` becomes `true`, the stale non-zero `PanOffset` is never reset. Both `DrawingCanvas` and `GridNodesLayer` receive `CanvasOffset = -PanOffset` via their shared `RenderTransform` binding, and the entire view drifts.

### State machine

```
                    Zoom in (sheet > viewport)
  ┌─────────┐      ────────────────────────────▶  ┌──────────┐
  │Centered │                                      │ Scrolled │
  │PanOffset=0│     ◀────────────────────────────  │PanOffset≠0│
  └─────────┘      Zoom out (sheet fits again)     └──────────┘
                    BUT: CenterCanvas() NOT called
                    → stale PanOffset remains
```

## Goals / Non-Goals

**Goals:**
- Eliminate visual drift of sheet content + grid after zoom/scroll/zoom cycle
- Keep implementation minimal (single method call)
- Add regression test

**Non-Goals:**
- Changing the scrollbar or centering math
- Changing the GridNodesLayer or DrawingCanvas layout
- Adding new zoom/pan features

## Decisions

### Decision 1: Call `CenterCanvas()` in `OnZoomChanged` — variant A (unconditional)

```csharp
partial void OnZoomChanged(double value)
{
    // … existing PropertyChanged notifications …
    CenterCanvas();  // always
}
```

| Pros | Cons |
|------|------|
| Simplest possible fix | Resets PanOffset even when zoom changes within non-centered range, potentially disrupting scroll position |

### Decision 2: Call `CenterCanvas()` in `OnZoomChanged` — variant B (conditional on `IsCentered`)

```csharp
partial void OnZoomChanged(double value)
{
    // … existing PropertyChanged notifications …
    if (IsCentered)
        CenterCanvas();  // only resets when sheet fits
}
```

| Pros | Cons |
|------|------|
| Does not touch PanOffset when still zoomed in (scroll position preserved) | Slightly more code |
| Fixes exactly the reported drift scenario | |

### Decision 3: Track `IsCentered` transition (false → true)

```csharp
private bool _wasCentered;
// In OnZoomChanged:
if (!_wasCentered && IsCentered) CenterCanvas();
_wasCentered = IsCentered;
```

| Pros | Cons |
|------|------|
| Most targeted — only fires at the transition | Adds state field; marginal benefit over variant B |

**Chosen: Variant B** — `if (IsCentered) CenterCanvas();`. Simple, readable, and does not interfere with scroll position while zoomed in. Variant A would snap the scroll to center on every zoom step, which is surprising behaviour.

## Risks / Trade-offs

- **Risk:** If the user prefers the off-center position after zoom-out (e.g., they were looking at a corner and want to stay there), this fix re-centers the sheet. **Mitigation:** This is the expected behaviour — when the sheet fits the viewport, it should be centered. The current drift is a bug, not a feature.
- **Risk:** If `IsCentered` transitions rapidly during zoom animation, `CenterCanvas()` is called multiple times. **Mitigation:** It's idempotent — setting `PanOffset=0` when it's already 0 does nothing.
