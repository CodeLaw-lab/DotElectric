## Why

Selection markers for rotated text objects appear systematically offset from the actual rendered text at 90°, 180°, and 270° rotation angles. Only 0° works correctly. The root cause is a sign error in the rotation math across three `Text.cs` methods — `RotatedCorner*`, `GetBoundingBox()`, `HeightMicrons` — combined with 231 lines of dead code in `TextSelectionMarkerBehavior`. This makes rotated text appear incorrectly selected and its resize handles unreachable.

## What Changes

- Fix sign errors in `Text.RotatedCorner1Y`, `RotatedCorner2X`, `RotatedCorner3X`, `RotatedCorner3Y` — CW-in-Y-up formulas where CCW-in-Y-up needed
- Fix `Text.GetBoundingBox()` — same CW/CCW mismatch propagates to `VisualLeft/Right/Top/Bottom`
- Remove `TextSelectionMarkerBehavior.cs` (231 lines) — dead code: attached to TextBlock but never executes because no `<Canvas>` exists in the DataTemplate
- Remove `behaviors:TextSelectionMarkerBehavior.IsEnabled="True"` from `EditorCanvas.xaml` DataTemplate
- Fix `Text.HeightMicrons` to use `FontMetrics.GetHeightRatio()` — currently uses raw `FontSizeMicrons`, ignoring font ascent/descent (mismatch with WPF GlyphTypeface rendering)
- Fix `Text.WidthMicrons` to use `FontMetrics.GetAdvWidthRatio()` — currently uses hardcoded factors

## Capabilities

### New Capabilities
- `text-rotation-math`: Correct rotation geometry for text bounding boxes — ensure `RotatedCorner*`, `ContainsPoint()`, and `GetBoundingBox()` use the correct CW-in-WPF/CCW-in-Y-up transformation so selection markers, hit-testing, and bounding boxes match the visual WPF `RotateTransform` at every angle.

### Modified Capabilities
- `session1-defects`: The "Text selection markers follow rotation" requirement from Sprint 52 is not fully met — the implementation contains sign errors in corner computation. This change fixes the math without altering the requirement.

## Impact

- **Models/Objects/Text.cs**: `RotatedCorner1-3` (6 lines), `GetBoundingBox()` (~16 lines), `HeightMicrons`/`WidthMicrons` (depends on FontMetrics)
- **Models/FontMetrics.cs** (new): static font metric class, WPF-free public API
- **Behaviors/TextSelectionMarkerBehavior.cs** (removed): 231 lines of dead code
- **Views/EditorCanvas.xaml**: remove `behaviors:TextSelectionMarkerBehavior.IsEnabled="True"` from Text DataTemplate
- **App.xaml.cs**: add `FontMetrics.Initialize()` call
- **Tests**: new test cases for `RotatedCorner*`, `GetBoundingBox()`, `ContainsPoint()` at 90°, 180°, 270° (currently only test 0°)
