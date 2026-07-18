## Why

Selection markers on rotated text objects are positioned using model-dimension approximations (HeightMicrons, WidthMicrons), which don't precisely match the actual WPF-rendered text bounds. At 0° the bottom markers appear above the selection highlight; at 90°/180°/270° the misalignment compounds, making markers drift from the visual text area. This creates a visible discrepancy between markers and the filled selection background.

## What Changes

- Add a new `TextSelectionMarkerBehavior` attached behavior on the TextBlock DataTemplate
- The behavior reads `ActualWidth`/`ActualHeight` and `RotateTransform` from the rendered TextBlock after layout
- Computes 4 corner positions from the actual visual text bounds (not model dimensions)
- Creates/adjusts 4 marker Rectangle elements positioned at the visual corners
- Model `RotatedCorner*` properties remain unchanged for backward compatibility (hit-test, bounding box, resize still use model dimensions)
- Selection highlight (blue Background) on the TextBlock is removed — the markers alone indicate selection state, eliminating the visual mismatch entirely

## Capabilities

### New Capabilities
- `text-selection-markers`: Visual overlay behavior that positions selection corner markers based on the actual WPF-rendered text bounds, independent of model dimension approximations

### Modified Capabilities

*(none — no existing spec changes)*

## Impact

- **New file:** `Behaviors/TextSelectionMarkerBehavior.cs` — the core behavior
- **Modified:** `Views/EditorCanvas.xaml` — remove TextBlock Background highlight from selection DataTrigger, attach the new behavior
- **Removed:** The `Text` DataTemplate in the marker `ItemsControl` (lines 521-536) is replaced by the behavior-managed markers
- **Model unchanged:** `Text.RotatedCorner*` kept for hit-test/bounding-box/resize, but no longer used for marker visual positioning
- **Tests:** New unit tests for the behavior (STA-thread), updates to existing visual tests
