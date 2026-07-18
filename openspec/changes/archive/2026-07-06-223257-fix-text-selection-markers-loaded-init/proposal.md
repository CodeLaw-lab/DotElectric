## Why

`TextSelectionMarkerBehavior` crashes with `InvalidOperationException` when a text object is inserted into the canvas. The constructor calls `VisualTreeHelper.GetParent()` before the TextBlock is attached to the visual tree, so the parent Grid and sibling Canvas are not yet findable. This crashes the application with a XAML parse exception during DataTemplate materialization.

## What Changes

1. **Defer visual tree access to `Loaded` event** — the constructor will NOT walk the tree. All sibling Canvas discovery will happen when the TextBlock is loaded and fully attached.
2. **Add null-guard pattern** — missing Canvas sibling will be a silent no-op (consistent with existing behaviors like `MarkerPosition` and `TextBoxLostFocusCommandBehavior`), not a crash.
3. **Add re-init guard** — prevent duplicate marker creation if `Loaded` fires multiple times.
4. **Update tests** — ensure all behavior tests account for the deferred initialization (need dispatcher pump after Loaded).

## Capabilities

### New Capabilities
- `text-selection-markers`: On-canvas selection marker overlay for Text objects, positioned via `FormattedText` measurement at the actual rendered text bounds.

### Modified Capabilities
*(none)*

## Impact

- **File:** `Behaviors/TextSelectionMarkerBehavior.cs` — rewrite constructor and Loaded handler (Option A: Loaded-based init)
- **Tests:** `Tests/Behaviors/TextSelectionMarkerBehaviorTests.cs` — update all STA tests (already have dispatcher pump pattern)
- **XAML:** No changes needed (Grid + Canvas sibling already in DataTemplate, behavior attribute already attached)
- **No breaking changes:** API surface unchanged
