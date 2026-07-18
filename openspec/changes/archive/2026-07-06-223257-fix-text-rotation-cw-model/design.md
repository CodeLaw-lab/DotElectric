## Context

**Current state:** The model's rotation math in `Text.cs` uses counter-clockwise (CCW) convention in a Y-down local coordinate system (origin at text's visual top-left corner). WPF's `RotateTransform` uses clockwise (CW) convention. For any angle where `sinθ ≠ 0` (i.e., not 0° or 180°), the model computes corner positions, hit-test areas, and bounding boxes on the opposite side of the text from where WPF renders it.

**The FontMetrics fix** (already merged) corrected the scale (height/width ratios) but did not change rotation direction. Rotation direction is an independent, more fundamental issue — it's not about precision but about sign correctness.

**Constraints:**
- WPF `RotateTransform` cannot be changed (it rotates CW by design)
- The serialized `RotationAngle` value must remain the same (no breaking file format change)
- All model-layer consumers (hit-test, selection box, resize handles) must agree with visual rendering

## Goals / Non-Goals

**Goals:**
- Fix `RotatedCorner0..3` in `Text.cs` to use CW rotation → markers appear at correct visual corners
- Fix `ContainsPoint()` in `Text.cs` to use correct inverse rotation → hit-test works at all angles
- Fix `GetBoundingBox()` in `Text.cs` to use CW rotation → selection box and collision detection correct
- Fix `HitTestHelper.GetTextHandle()` to use CW rotation → resize handles detectable
- Update all affected tests

**Non-Goals:**
- No changes to WPF XAML or `RotateTransform` binding
- No changes to serialization format
- No changes to FontMetrics or text size computation
- No changes to Line/Rectangle objects (they don't rotate)

## Decisions

### Decision 1: Change model to CW (instead of negating RotateTransform angle)

**Chosen:** Change the model's rotation math from CCW to CW convention.

**Rationale:**
- WPF is the rendering platform — the model should match its coordinate system
- CW is more intuitive for a Windows/WPF application (clockwise = positive angle)
- No runtime overhead (no negating converter in binding pipeline)
- All existing serialized data retains semantic meaning

**Rejected alternative:** Negate the angle in `RotateTransform.Angle` binding (`-{Binding RotationAngle}`). This would flip the visual meaning for users and create inconsistency with existing .tdel files.

### Decision 2: Keep `RotationAngle` semantics unchanged

The `RotationAngle` property stores 0-359 with normalization `((value % 360) + 360) % 360`. The semantic meaning remains: "degrees of clockwise rotation in screen space." Previously it was implicitly CCW in model space — by fixing the math to CW, the actual behavior now matches the explicit CW semantic.

### Decision 3: Fix all four functions in one commit

The four affected functions share the same rotation math:
- `RotatedCorner*` — forward CW from local to world
- `ContainsPoint` — inverse (world to local, = CCW forward)
- `GetBoundingBox` — forward CW for each corner
- `HitTestHelper.GetTextHandle` — forward CW for handle positions

Changing them together ensures internal consistency and avoids partial fixes.

## Math reference

**CW forward (local → world) in Y↓:**
```
x' = x·cosθ + y·sinθ
y' = -x·sinθ + y·cosθ
worldX = MicronsX + x'
worldY = (MicronsY+H) - y'
```

**CW inverse (world → local, = CCW forward):**
```
cpX = pointX - MicronsX
cpY = (MicronsY+H) - pointY
u = cpX·cosθ - cpY·sinθ
v = cpX·sinθ + cpY·cosθ
```

### Concrete changes (signs relative to current code)

| Function | Current sign on `sinθ` | Fixed sign |
|---|---|---|
| `RotatedCorner1Y` | `- W·sinθ` | `+ W·sinθ` |
| `RotatedCorner2X` | `- H·sinθ` | `+ H·sinθ` |
| `RotatedCorner3X` | `- H·sinθ` | `+ H·sinθ` |
| `RotatedCorner3Y` | `- W·sinθ - H·cosθ` | `+ W·sinθ - H·cosθ` |
| `ContainsPoint u` | `+ cpY·sinθ` | `- cpY·sinθ` |
| `ContainsPoint v` | `- cpX·sinθ` | `+ cpX·sinθ` |
| `GetBoundingBox cpY` | `+ lx·sinθ + ly·cosθ` | `- lx·sinθ + ly·cosθ` |
| `GetTextHandle` | same as RotatedCorners | same sign changes |

## Risks / Trade-offs

- **[Risk] Existing .tdel files with rotated text** will render differently (markers now correct, but visual rotation was previously wrong). Mitigation: this is a bug fix — the correct behavior is the goal.
- **[Risk] Inline TextEditor** (Textbox with RotateTransform) is independent — uses the same `RotationAngle` binding, so it will agree with the fix.
- **[Risk] Printing** (`PrintDocumentGenerator`) — verify it computes text position using `Text.MicronsX/Y` without rotation math. If it does its own rotation, it may need updating. Low likelihood.
