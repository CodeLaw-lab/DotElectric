## Why

The model's rotation math (`RotatedCorner*`, `ContainsPoint`, `GetBoundingBox`) uses counter-clockwise (CCW) rotation convention, but WPF's `RotateTransform` rotates clockwise (CW). At 0° and 180° the formulas agree (sin=0), but at any other angle the selection markers appear on the **opposite side** of the text, hit-test fails, and the bounding box is wrong. This makes rotated text effectively unusable — users cannot select, resize, or reliably interact with text at non-right-angle rotations.

## What Changes

- Change all rotation math in `Text.cs` from CCW to CW convention to match WPF rendering
- Update `HitTestHelper.GetTextHandle()` to use the corrected CW convention
- All existing tests for rotated corner positions, hit-testing, and bounding boxes must be updated to reflect the corrected geometry

**Not in scope:** Font metrics (already fixed), WPF XAML changes (RotateTransform stays as-is), serialization format (RotationAngle semantics unchanged — only internal math changes).

## Capabilities

### New Capabilities
- *(none — this is a bug fix, not a new capability)*

### Modified Capabilities
- *(none — requirements unchanged, only math implementation corrected)*

## Impact

- **Models/Objects/Text.cs**: `RotatedCorner0..3` — sign change on `sinθ` terms; `ContainsPoint()` — sign change; `GetBoundingBox()` — sign change
- **Helpers/HitTestHelper.cs**: `GetTextHandle()` — sign change on corner math
- **Tests/Models/Objects/TextTests.cs**: expected values for rotated corners, ContainsPoint, GetBoundingBox
- **Tests/Helpers/HitTestHelperTests.cs**: expected handle positions for rotated text
- **No impact on**: serialization, XAML, WPF rendering, FontMetrics, other object types (Line, Rectangle)
