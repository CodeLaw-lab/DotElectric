## Context

`TextSelectionMarkerBehavior` is an attached behavior that creates 4 Rectangle markers (using `SquareMarker` style) at the corners of a rotated TextBlock. It measures rendered text via `FormattedText`, computes rotated corner positions, and positions markers in a sibling Canvas overlay inside a Grid wrapper.

**Current problem:** The `TextSelectionMarkerManager` constructor (line 58-60) calls `VisualTreeHelper.GetParent(textBlock)` synchronously. This runs when the attached property `IsEnabled` is set during XAML DataTemplate parsing — before the TextBlock is added to the visual tree. `GetParent()` returns `null`, the fallback throws `InvalidOperationException`, and WPF wraps it in `XamlParseException`.

**Broader context:** This is a new behavior (added in the previous change `fix-text-markers-view-layer`). The existing behaviors in this codebase (`MarkerPosition`, `TextBoxLostFocusCommandBehavior`) never walk the visual tree synchronously and never throw on missing dependencies. `PreviewLineChangedBehavior` does walk the tree but only after `Loaded` (called from code-behind lifecycle hooks).

**Data flow:**

```
OnIsEnabledChanged (during XAML parsing)
  └→ TextSelectionMarkerManager constructor
       └→ VisualTreeHelper.GetParent() → null ← падает
```

## Goals / Non-Goals

**Goals:**
- Eliminate the crash when text objects are created from any code path (TextTool paste, template load, undo/redo)
- Follow existing codebase conventions for attached behaviors (silent no-op on missing deps)
- Markers must still appear at correct rotated corners after TextBlock is rendered
- All existing functionality preserved: reactive updates on Content/RotationAngle/FontSize change, empty-content guard, cleanup on disable

**Non-Goals:**
- No change to the marker measurement algorithm (FormattedText + CW rotation formula)
- No change to XAML DataTemplate structure (Grid + Canvas sibling stays)
- No change to the public API (`SetIsEnabled`/`GetIsEnabled`)
- No change to existing tests' behavior (only initialization timing changes)

## Decisions

### Decision 1: Loaded-based initialization (Option A) over fully deferred (Option B) or code-created Canvas (Option C)

**Chosen: Option A — deferred visual tree access until Loaded event.**

| Criterion | A (Loaded) | B (fully deferred) | C (code canvas) |
|---|---|---|---|
| Reliability | ★★★★ | ★★★★★ | ★★★ |
| Visual glitch | None (first render) | +1 frame delay | None |
| Follows codebase | Yes (PreviewLineChanged) | No | No |
| Layout cycle risk | None (Loaded after layout) | None | High |
| Complexity | Medium (~40 new lines) | Low (~25) | High (~60) |

**Rationale:**
- Option C is rejected: programmatic Canvas insertion into visual tree can cause layout cycles (same class of bug we're fixing) and breaks the XAML convention.
- Option B is rejected: introduces non-deterministic frame delay for marker appearance. While safe, it doesn't match the codebase's existing patterns and the visual glitch is unacceptable for selection markers.
- Option A follows the same dual-guard pattern as `PreviewLineChangedBehavior` (register in both Loaded and DataContextChanged) and has zero frame delay since Loaded fires before the first render.

### Decision 2: Constructor becomes a lightweight pass-through

The constructor will no longer:
- Walk the visual tree (`VisualTreeHelper.GetParent`)
- Create marker Rectangles (`CreateMarkers()`)
- Call `UpdateMarkers()`

Instead it will:
- Store `_textBlock` reference
- Subscribe to `Loaded`/`Unloaded`/`SizeChanged`/`DataContextChanged`
- Subscribe to DataContext INotifyPropertyChanged

`CreateMarkers()` and visual tree discovery move to `OnLoaded`.

### Decision 3: Silent no-op on missing Canvas sibling

If `OnLoaded` fires but the sibling Canvas is missing (shouldn't happen in normal use), the behavior silently exits — no markers, no crash. This matches `MarkerPosition` and `TextBoxLostFocusCommandBehavior`.

### Decision 4: Re-init guard via boolean flag

Add `_markersCreated` flag. `OnLoaded` checks this flag before calling `CreateMarkers()`. This prevents duplicate marker creation if `Loaded` fires multiple times (e.g., during recycling).

## Risks / Trade-offs

| Risk | Mitigation |
|---|---|
| **Loaded fires before DataContext set** — `FormattedText` can't read model properties (RotationAngle) | Already handled: `SubscribeToDataContext()` called from constructor. When DataContext arrives, `OnDataContextChanged` → `ScheduleUpdate()`. |
| **Unloaded → Reloaded** (tab switch, container recycling) — markers deleted on Unloaded, need recreation on Loaded | `_markersCreated` flag reset in `Dispose()`. `OnLoaded` will recreate markers. |
| **DataTemplate re-materialization** — ItemsControl removes and re-creates item containers | `OnUnloaded` calls `Dispose()`, new constructor called on new TextBlock, `OnLoaded` on new instance. Clean. |
| **Dispatcher queue overload** — rapid model changes (e.g., animating rotation) | Already guarded: `_updateScheduled` coalesces updates via `Dispatcher.BeginInvoke(Render)`. |
