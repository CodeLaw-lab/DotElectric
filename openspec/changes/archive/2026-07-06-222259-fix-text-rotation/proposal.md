## Why

Selection markers for rotated text objects are positioned incorrectly at every angle except 0°. ContainsPoint() hit-testing also fails for rotated text — clicks in the visible text area don't select the object. Both bugs stem from confusion about WPF RotateTransform matrix direction.

## What Changes

- **Revert** incorrect RotatedCorner* and GetBoundingBox changes from previous `fix-text-rotation-math` change (those formulas were correct originally)
- **Fix** ContainsPoint() — replace forward transform with inverse transform for correct unrotate of hit-test point
- **Update** tests: revert RotatedCorner/GetBoundingBox expectations, rewrite ContainsPoint tests with correct center-of-rotated-AABB points
- Remove dead code `TextSelectionMarkerBehavior` (already deleted, just ensuring no stale references)

## Capabilities

### New Capabilities
- `rotation-math`: Correct WPF RotateTransform matrix direction, forward/inverse transform application for RotatedCorner*, GetBoundingBox, and ContainsPoint()

### Modified Capabilities
(none)

## Impact

- `Models/Objects/Text.cs`: RotatedCorner* (6 lines), GetBoundingBox (2 lines), ContainsPoint (2 lines) — sign changes only
- `Tests/Models/Objects/TextTests.cs`: Update expectations in ~6 test methods
- `Behaviors/TextSelectionMarkerBehavior.cs`: Already deleted
