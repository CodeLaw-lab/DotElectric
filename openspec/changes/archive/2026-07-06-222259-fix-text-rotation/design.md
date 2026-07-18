## Context

Text rotation is implemented via WPF `LayoutTransform` with `RotateTransform(Angle)` on the TextBlock. Selection markers (4 corner squares) are positioned via `RotatedCorner*` properties that compute model-space corner coordinates after rotation. Hit-testing (`ContainsPoint()`) unrotates a model point into local text space.

A prior change (`fix-text-rotation-math`) incorrectly assumed WPF `RotateTransform` uses a non-standard matrix and introduced sign errors in 6 formulas. The original `RotatedCorner*` and `GetBoundingBox` formulas were correct — only `ContainsPoint()` was genuinely buggy (using forward transform instead of inverse for unrotation).

### WPF RotateTransform matrix (verified)

WPF uses the **standard Cartesian CCW matrix**, which in Y-down produces CW rotation:

```
| x' |   | cosθ  -sinθ | | x |
| y' | = | sinθ   cosθ | | y |
```

Proof: at θ=90°, local point (W, 0) → WPF screen point (0, W) = downward in Y-down = CW.

### Coordinate convention

```
Model Y-up:                         WPF Y-down:
  Y                                 (0,0) ── X
  ↑                                   │
  │                                   │ Y
  │                                   ↓
  └── X
```

Pivot for text rotation: `(MicronsX, MicronsY + HeightMicrons)` — the top-left corner in model Y-up, which maps to the TextBlock's top-left in WPF.

## Goals / Non-Goals

**Goals:**
- Selection markers appear at correct visual corners for ALL rotation angles (0°, 45°, 90°, 180°, 270°)
- `GetBoundingBox()` returns correct AABB matching WPF visual rendering
- `ContainsPoint()` correctly hit-tests rotated text using inverse transform
- All tests pass with correct expectations

**Non-Goals:**
- Changing font metrics (WidthMicrons/HeightMicrons) — those are a separate concern
- Mid-side resize handles for rotated text
- Changing how text renders visually
- Per-glyph exact width measurement

## Decisions

### D1: WPF RotateTransform uses standard Cartesian CCW matrix

**Choice:** Use `x' = x*cosθ - y*sinθ`, `y' = x*sinθ + y*cosθ` as the forward transform.

**Rationale:** This is the standard mathematical rotation matrix. In WPF's Y-down coordinate system, it produces clockwise rotation — matching the documented behavior of `RotateTransform`.

**Alternative considered:** `x' = x*cosθ + y*sinθ`, `y' = -x*sinθ + y*cosθ` (used in the prior broken change). This was derived from incorrectly assuming WPF uses a CW-specific matrix.

### D2: RotatedCorner* uses forward transform

**Choice:** `RotatedCorner1Y = (Y+H) - W*sinθ`, `RotatedCorner2X = X - H*sinθ`, etc.

**Rationale:** The forward transform maps local text coordinates (lx, ly) → WPF screen offset → model Y-up coordinates. The `- sin` terms correctly reflect the standard matrix's `-sin` in the x' equation.

**Proof at 90°:** (W, 0) → (0, W) in WPF Y-down → model Y-up: (X, Y+H-W). The `-W*sin90 = -W` correctly places the corner BELOW the pivot.

### D3: GetBoundingBox uses forward transform

**Choice:** `cpX = lx*cos - ly*sin`, `cpY = lx*sin + ly*cos`

**Rationale:** Same as D2. Each corner is transformed forward, then converted to model Y-up. Matches RotatedCorner* coordinate output.

### D4: ContainsPoint uses inverse transform

**Choice:** `u = cpX*cos + cpY*sin`, `v = -cpX*sin + cpY*cos`

**Rationale:** To test if a point P is inside the rotated rectangle, we need to unrotate P into local text space. The inverse of the standard forward matrix is:

```
| x |   |  cosθ  sinθ | | x' |
| y | = | -sinθ  cosθ | | y' |
```

**Proof at 90°:** Center of rotated AABB at (-h/2, h-w/2) → cpX=-h/2, cpY=w/2 → inverse: u=w/2, v=h/2 → inside [0,w]×[0,h] ✓

**Alternative considered:** Forward transform (current buggy code). At 90°, forward maps center (-h/2, h-w/2) → u=-w/2, v=-h/2 → outside. Incorrect.

## Risks / Trade-offs

| Risk | Impact | Mitigation |
|------|--------|------------|
| WidthMicrons/HeightMicrons from FontMetrics don't match WPF rendering | Markers slightly misaligned even with correct rotation math | Separate concern; FontMetrics was introduced in prior change and needs independent verification |
| ContainsPoint fix changes hit-test behavior for rotated text | Existing tests may fail | Rewrite ContainsPoint tests with correct center-of-AABB points; verify manually |
| Previous fix-text-rotation-math change's test assertions conflict | Tests fail until reverted | Revert test expectations alongside code changes |
| 270° spec says `- sin(270)` where sin(270) = -1 | Sign confusion | At 270°, `(Y+H) - W*(-1) = Y+H+W` — correct for 270° CW (corner below pivot + to the left) |
