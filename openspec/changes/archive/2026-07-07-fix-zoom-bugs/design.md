## Context

Three independent bugs in the zoom notification and binding chain:

1. **ZoomPercent not notified**: `ZoomPercent` (computed property on `ZoomPanManager`) has no `OnPropertyChanged()` call in `OnZoomChanged()`. The status bar `{Binding ZoomPanManager.ZoomPercent}` never re-evaluates.

2. **Grid viewport-only mode**: `GridManager.RefreshGridNodes()` has two branches — full-sheet when `IsCentered=true`, viewport-only when `IsCentered=false`. At the moment scrollbars appear (canvas > viewport), the grid switches to viewport-only and never refreshes on pan.

3. **ComboBox two-way binding error**: The toolbar ComboBox has `Text="{Binding ZoomPercent, StringFormat={}{0}%}"` which is two-way by default on editable ComboBoxes. Selecting "150%" writes the string to `int ZoomPercent` → WPF binding conversion error.

## Goals / Non-Goals

**Goals:**
- Status bar zoom display updates on all zoom change methods
- Grid dots cover the full sheet at all zoom levels and update on pan
- Zoom ComboBox works without binding errors

**Non-Goals:**
- Changing the zoom architecture, zoom computation, or mouse wheel handling
- Optimizing grid performance beyond what's needed for correctness
- Adding new zoom-related UI features

## Decisions

### Bug 1: Add OnPropertyChanged for ZoomPercent

| Approach | Verdict |
|----------|---------|
| Add `OnPropertyChanged(nameof(ZoomPercent))` in `OnZoomChanged` | ✅ Adopted — 1 line, matches existing pattern |
| Restore forwarding property on EditorViewModel | ❌ Rejected — Sprint R3.1 intentionally removed forwarding |
| Change status bar to bind `Zoom` with a converter | ❌ Unnecessary — ZoomPercent already exists, just needs notification |

`ZoomPanManager.cs:109-122` — add one line:
```csharp
partial void OnZoomChanged(double value)
{
    OnPropertyChanged(nameof(ZoomPercent));  // ← ADD
    OnPropertyChanged(nameof(CanvasWidthPixels));
    // ... existing notifications ...
}
```

### Bug 2: Full-sheet grid nodes always

**Root cause**: `GridManager.RefreshGridNodes()` (lines 92-115) branches on `IsCentered`. When `IsCentered=false` (scrollbars visible), it generates nodes only for the visible viewport. This was an optimization that breaks the user expectation of a full-sheet grid.

**Investigation of full-sheet performance:**

| Sheet | Grid step | Nodes | Under 100K limit? |
|-------|-----------|-------|-------------------|
| A0 (2378×841 mm) | 5 mm | ~80K | ✅ (80K < 100K) |
| A0 (2378×841 mm) | 1 mm | ~2M | ❌ → GridHelper returns empty |
| A4 (297×210 mm) | 5 mm | ~2.5K | ✅ |
| A4 (297×210 mm) | 1 mm | ~62K | ✅ |

The `GridHelper.MaxGridNodes = 100000` check is a safety net for the extreme case (A0 + 1mm grid would be empty, but MinPixelSpacing would hide it at any sane zoom unless zoom > 500%).

| Approach | Verdict |
|----------|---------|
| Always generate full-sheet nodes (remove `IsCentered` branch) | ✅ Adopted — simple, correct, safe |
| Generate viewport + buffer zone + refresh on pan | ❌ More complex, same practical result |
| Keep viewport-only but refresh on every pan MouseMove | ❌ Performance risk — regenerating nodes on every pixel of mouse move |

**Implementation:**
- `GridManager.cs:92-115` — remove the `if/else`, keep only the full-sheet branch
- No pan refresh needed — full-sheet nodes cover everything, canvas `RenderTransform` handles movement

### Bug 3: ComboBox binding mode OneWay

**Root cause** (line 436):
```xml
Text="{Binding SelectedTab.ZoomPanManager.ZoomPercent, StringFormat={}{0}%}"
```
For editable ComboBox, `Text` defaults to `TwoWay` binding. WPF tries to convert "150%" → int → error.

| Approach | Verdict |
|----------|---------|
| Change to `Mode=OneWay` | ✅ Adopted — 1 attribute, zero behavioral change. ZoomComboBoxBehavior already handles user input |
| Add `IValueConverter` that strips % on write | ❌ More code, same effect |
| Suppress binding errors in trace | ❌ Hides the problem instead of fixing it |

**Implementation:**
- `MainWindow.xaml:436` — add `Mode=OneWay` to the Text binding

### Why Bug 1 + Bug 3 are independent

Bug 1 (no `ZoomPercent` notification) affects ALL UI bindings to `ZoomPercent`:
- Status bar (line 641)
- Toolbar ComboBox display (line 436)

Bug 3 (two-way binding error) only affects the ComboBox on user input.

Without fixing Bug 1, neither the status bar nor the ComboBox would update when zoom changes from mouse wheel/buttons. The ComboBox would only display correctly when the user selects a value from the dropdown (because SelectionChanged fires → ApplyZoom → ZoomPercent setter → Zoom setter → ZoomPercent getter returns new value → OneWay binding reads it). But wait — with OneWay binding, the binding reads `ZoomPercent` getter. Since `ZoomPercent` still doesn't fire `PropertyChanged`, the binding won't re-read. So the ComboBox Text won't update after a mouse wheel zoom either.

**Both fixes are needed** for the ComboBox to work correctly:
1. `Mode=OneWay` fixes the error on user input
2. `OnPropertyChanged(nameof(ZoomPercent))` fixes the display update

## Risks / Trade-offs

- **[Full-sheet grid on large sheet]** On A0 with 5mm grid step at high zoom, 80K nodes may cause a brief freeze during refresh (on zoom change only, not on every frame). Mitigation: this is 80K ellipses in `OnRender` — WPF handles this without issue. The freeze during `RefreshGridNodes` (node generation) is well under 50ms for 80K iterations of simple math.
- **[GridHelper MaxGridNodes empties grid]** At 1mm grid step on A0, GridHelper returns empty (2M > 100K). But at 1mm, MinPixelSpacing check hides the grid anyway unless zoom > 500%. At 500% zoom on A0 with 1mm grid, viewport shows ~200mm × 150mm → ~30K nodes — well under 100K. Edge case: user could create a custom 2mm grid on A0 with FitToScreen at 500%+, producing ~160K nodes. GridHelper returns empty → grid disappears. Acceptable trade-off: the user can reduce grid step or change zoom.
- **[ComboBox OneWay]** With OneWay binding, if `ZoomComboBoxBehavior` fails for any reason, the user has no feedback that their input was ignored. Mitigation: the behavior is well-tested (11 unit tests in Sprint STA) and handles all reasonable inputs.
