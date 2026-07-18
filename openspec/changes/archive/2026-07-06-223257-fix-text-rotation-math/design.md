## Context

Text rotation works in WPF via `LayoutTransform` with `RotateTransform` bound to `RotationAngle`. WPF's `RotateTransform` rotates **clockwise** in a Y-down coordinate system. This is equivalent to a **counter-clockwise rotation in Y-up** (model coordinates).

The model's `RotatedCorner*` properties compute selection marker positions at each text corner. `GetBoundingBox()` computes the axis-aligned bounding box. Both use rotation math that doesn't match WPF's transform direction — they use CW-in-Y-up where CCW-in-Y-up is needed.

Additionally, `HeightMicrons` and `WidthMicrons` use simplified heuristics that don't match WPF's font-rendering metrics, compounding the visual offset.

A separate dead-code path (`TextSelectionMarkerBehavior`, 231 lines) attaches to TextBlock but never runs because no `<Canvas>` exists in the DataTemplate.

### Current rotation math (WRONG)

```
WPF RotateTransform(θ) = CW in Y-down = CCW in Y-up

Model RotatedCorner* uses:  CW in Y-up  (MISMATCH!)
  Corner1: X + W*cos    (Y+H) - W*sin    
  Corner2: X - H*sin    (Y+H) - H*cos    
  Corner3: X + W*cos - H*sin    (Y+H) - W*sin - H*cos

Model GetBoundingBox uses:  CCW in Y-up  (MISMATCH!)
  cpX = lx*cos - ly*sin        
  cpY = lx*sin + ly*cos        
```

**Proof at 90°:** WPF RotateTransform 90° CW maps (W, 0) → (0, -W) in Y-down = (Y+H-(-W)) = (Y+H+W) in model Y-up. But RotatedCorner1Y gives (Y+H-W) — two widths off!

### Correct rotation math

```
For WPF RotateTransform(θ) CW in Y-down = model CCW in Y-up:

Corner0 (0,0):     X           ,  Y+H           (pivot, unchanged)
Corner1 (W,0):     X + W*cos   ,  Y+H + W*sin    ← FIX: +W*sin
Corner2 (0,H):     X + H*sin   ,  Y+H - H*cos    ← FIX: +H*sin (corner2X)
Corner3 (W,H):     X + W*cos + H*sin ,  Y+H + W*sin - H*cos  ← FIX: +H*sin, +W*sin

ContainsPoint (reverse CW → CCW in Y-down = CW in Y-up):
  localX = cpX*cos + cpY*sin   ← CURRENT: cpX*cos - cpY*sin (WRONG)
  localY = -cpX*sin + cpY*cos  ← CURRENT: cpX*sin + cpY*cos (WRONG)

GetBoundingBox (forward transform = CW in Y-down):
  cpX = lx*cos + ly*sin        ← CURRENT: lx*cos - ly*sin (WRONG)
  cpY = -lx*sin + ly*cos       ← CURRENT: lx*sin + ly*cos (WRONG)
```

Wait — let me re-verify ContainsPoint. The purpose is to test if a model-space point is inside the rotated rectangle. We need to:

1. Convert model point to WPF-relative (Y-down) around the pivot (X, Y+H)
2. Apply inverse rotation (to unrotate the point into local text space)
3. Check if it's in [0, W] × [0, H]

Inverse of CW (Y-down) = CCW in Y-down:
  localX = cpX*cos + cpY*sin    [CCW in Y-down]
  localY = -cpX*sin + cpY*cos   [CCW in Y-down]

Current code has:
  localX = cpX*cos - cpY*sin    [CW in Y-down = CCW in Y-up]
  localY = cpX*sin + cpY*cos    [CW in Y-down]

For θ=90°, cos=0, sin=1:
  localX = cpX*0 - cpY*1 = -cpY    [CURRENT]
  localX = cpX*0 + cpY*1 = cpY     [CORRECT]

These are different. So **ContainsPoint is also wrong** — I need to correct my earlier analysis.

Let me re-derive from scratch:

Given a point P in model space (microns, Y-up):
- cpX = Px - X                 (relative to pivot in model X)
- cpY = (Y+H) - Py             (relative to pivot in WPF Y-down)

We want to check if the unrotated point falls in [0,W] × [0,H].

The rotation applied by RotateTransform is CW in Y-down:
  [x']   [cos  sin] [x]     where (x,y) is pre-rotation, (x',y') is WPF-visible.
  [y'] = [-sin cos] [y]

The inverse (from visible back to pre-rotated):
  [x]   [cos  -sin] [x']
  [y] = [sin   cos] [y']

So:
  localX = cpX*cos - cpY*sin    [INVERSE of CW in Y-down]
  localY = cpX*sin + cpY*cos    [INVERSE of CW in Y-down]

And the CURRENT code has:
  localX = cpX*cos - cpY*sin    ✓  CORRECT!
  localY = cpX*sin + cpY*cos    ✓  CORRECT!

**ContainsPoint is CORRECT after all.** I was wrong to doubt it before. The inverse of CW in Y-down IS the correct formula here.

OK so let me re-verify GetBoundingBox:

GetBoundingBox transforms the 4 corners forward to find the axis-aligned bounding box.

Forward CW in Y-down:
  cpX = lx*cos + ly*sin    [FORWARD CW in Y-down]
  cpY = -lx*sin + ly*cos   [FORWARD CW in Y-down]

Current code:
  cpX = lx*cos - ly*sin    [FORWARD CCW in Y-down = FORWARD CW in Y-up]
  cpY = lx*sin + ly*cos    [FORWARD CCW in Y-down]

For θ=90°, corner (W,0):
  Forward CW: cpX = W*0 + 0*1 = 0, cpY = -W*1 + 0*0 = -W
  Current:    cpX = W*0 - 0*1 = 0, cpY = W*1 + 0*0 = W

In model Y-up: pivot = (X, Y+H)
  CW:    x = X+0 = X,                y = (Y+H) - (-W) = Y+H+W
  Current: x = X+0 = X,                y = (Y+H) - W = Y+H-W

For a rectangle of width W and height H rotated 90° CW:
- Corner1 should be at (X, Y+H+W) (W units below pivot in WPF = above in model)
- Current gives (X, Y+H-W) (W units above pivot in WPF = below in model)

**GetBoundingBox IS wrong.** It uses CCW in Y-down where CW is needed.

Now, let me also re-verify the RotatedCorner formulas carefully.

RotatedCorner uses the direct forward CW transform and converts to model Y-up:

Corner1 (local W, 0):
  Forward CW (Y-down): cpX = W*cos + 0*sin = W*cos, cpY = -W*sin + 0*cos = -W*sin
  Model Y-up: x = X + W*cos, y = (Y+H) - cpY = (Y+H) - (-W*sin) = Y+H + W*sin

Current RotatedCorner1Y = (Y+H) - W*sin

**RotatedCorner1Y IS wrong.** It has -W*sin, should be +W*sin.

Corner2 (local 0, H):
  Forward CW (Y-down): cpX = 0*cos + H*sin = H*sin, cpY = -0*sin + H*cos = H*cos
  Model Y-up: x = X + H*sin, y = (Y+H) - H*cos

Current RotatedCorner2X = X - H*sin, RotatedCorner2Y = (Y+H) - H*cos

**RotatedCorner2X IS wrong** (should be +H*sin), **RotatedCorner2Y is correct**.

Corner3 (local W, H):
  Forward CW (Y-down): cpX = W*cos + H*sin, cpY = -W*sin + H*cos
  Model Y-up: x = X + W*cos + H*sin, y = (Y+H) - cpY = (Y+H) + W*sin - H*cos

Current RotatedCorner3X = X + W*cos - H*sin, RotatedCorner3Y = (Y+H) - W*sin - H*cos

**Both RotatedCorner3X and RotatedCorner3Y are wrong.**
- X: should be +H*sin, currently -H*sin
- Y: should be +W*sin, currently -W*sin

### Summary of findings

| Method | Line(s) | Correct? | Fix |
|--------|---------|----------|-----|
| `RotatedCorner1Y` | 165 | ❌ | `- W*sin` → `+ W*sin` |
| `RotatedCorner2X` | 167 | ❌ | `- H*sin` → `+ H*sin` |
| `RotatedCorner3X` | 170 | ❌ | `... - H*sin` → `... + H*sin` |
| `RotatedCorner3Y` | 171 | ❌ | `- W*sin` → `+ W*sin` |
| `GetBoundingBox` cpX | 270 | ❌ | `lx*cos - ly*sin` → `lx*cos + ly*sin` |
| `GetBoundingBox` cpY | 271 | ❌ | `lx*sin + ly*cos` → `-lx*sin + ly*cos` |
| `ContainsPoint` u | 242 | ✅ | — |
| `ContainsPoint` v | 243 | ✅ | — |
| `HeightMicrons` | 127-136 | ❌ | Use `FontMetrics.GetHeightRatio()` |
| `WidthMicrons` | 138-147 | ❌ | Use `FontMetrics.GetAdvWidthRatio()` |

## Goals / Non-Goals

**Goals:**
- Selection markers (4 corner squares) appear at the actual rotated text corners at ALL rotation angles
- `GetBoundingBox()` returns correct AABB matching WPF visual rendering
- `ContainsPoint()` verified correct (no change needed)
- `HeightMicrons`/`WidthMicrons` use WPF font metrics for accurate sizing
- Remove dead code `TextSelectionMarkerBehavior` (231 lines)
- Both GOST A and GOST B fonts handled correctly

**Non-Goals:**
- Per-glyph exact width measurement (average advance width ratio is sufficient)
- Mid-side resize handles for rotated text (remains per existing design)
- Dynamic font loading or custom font support (only the two embedded GOST fonts)
- Changing how the text renders in WPF (the visual appearance stays identical)

## Decisions

### D1: Sign corrections in Text.cs

Six lines need sign changes. All follow the same pattern: `- sin` → `+ sin` in model Y-up expressions:

```csharp
// RotatedCorner1Y (line 165)
(long)Math.Round(WidthMicrons * Math.Sin(...))     // was: - Math.Round(...)

// RotatedCorner2X (line 167)
(long)Math.Round(HeightMicrons * Math.Sin(...))     // was: - Math.Round(...)

// RotatedCorner3X (line 170)
(long)Math.Round(WidthMicrons * Math.Cos(...) + HeightMicrons * Math.Sin(...))  // was: ... - HeightMicrons * Math.Sin(...)

// RotatedCorner3Y (line 171)
(long)Math.Round(WidthMicrons * Math.Sin(...) + HeightMicrons * Math.Cos(...))  // was: - WidthMicrons * Math.Sin(...) ... - HeightMicrons * Math.Cos(...)
// Wait, let me re-check RotatedCorner3Y:
// Current: (Y+H) - Math.Round(W*sin + H*cos)
// Correct: (Y+H) + W*sin - H*cos = (Y+H) - (H*cos - W*sin) = (Y+H) - Math.Round(H*cos - W*sin)
// Or equivalently: (Y+H) + Math.Round(W*sin) - Math.Round(H*cos)
// Let me express it as:  Y+H - H*cos + W*sin  =  Y+H + (W*sin - H*cos)
// In code terms: (Y+H) + Math.Round(W*sin) - Math.Round(H*cos)

// GetBoundingBox cpX (line 270)
var cpX = lx * cosA + ly * sinA;      // was: lx * cosA - ly * sinA

// GetBoundingBox cpY (line 271)
var cpY = -lx * sinA + ly * cosA;     // was: lx * sinA + ly * cosA
```

### D2: FontMetrics class (static, initialized at WPF startup)

Same approach as the existing change's D1: static `FontMetrics` class initialized once at `App.xaml.cs` startup via `GlyphTypeface` API. WPF-free public interface (`GetHeightRatio`, `GetAdvWidthRatio`). Testable via `InitializeWithTestValues()`.

### D3: HeightMicrons and WidthMicrons font-aware

```csharp
// HeightMicrons — single line
(long)(FontSizeMicrons * FontMetrics.GetHeightRatio(fontName))

// WidthMicrons
(long)Math.Max(FontSizeMicrons, maxLen * FontSizeMicrons * FontMetrics.GetAdvWidthRatio(fontName))
```

### D4: Remove TextSelectionMarkerBehavior entirely

Delete `Behaviors/TextSelectionMarkerBehavior.cs` (231 lines). Remove `behaviors:TextSelectionMarkerBehavior.IsEnabled="True"` from `EditorCanvas.xaml` Text DataTemplate. The ItemsControl-based markers in `EditorCanvas.xaml` already handle text markers correctly once RotatedCorner math is fixed.

## Risks / Trade-offs

| Risk | Impact | Mitigation |
|------|--------|------------|
| Sign fix breaks existing 0° behavior | cos(0)=1, sin(0)=0, all terms with sin vanish — no change at 0° | Test at 0°, 45°, 90°, 135°, 180°, 270° |
| FontMetrics.GlyphTypeface init fails on non-WPF thread | Test crashes | Static fallback to heuristic values; `InitializeWithTestValues()` for tests |
| Width change affects text rendering on canvas | TextBlock width binding may look different | Width binding is only used for marker positioning, not rendering — TextBlock auto-sizes independently |
| Multi-line height changes | Line spacing may shift slightly | Apply same font ratio to line spacing; visual impact is minimal |
