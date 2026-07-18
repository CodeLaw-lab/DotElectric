## 1. Revert RotatedCorner* to original formulas (Text.cs)

- [x] 1.1 Change `RotatedCorner1Y` from `(Y+H) + W*sin` to `(Y+H) - W*sin` (line 165)
- [x] 1.2 Change `RotatedCorner2X` from `X + H*sin` to `X - H*sin` (line 167)
- [x] 1.3 Change `RotatedCorner3X` from `W*cos + H*sin` to `W*cos - H*sin` (line 170)
- [x] 1.4 Change `RotatedCorner3Y` from `(Y+H) + W*sin - H*cos` to `(Y+H) - W*sin - H*cos` (line 171)

## 2. Revert GetBoundingBox to original formulas (Text.cs)

- [x] 2.1 Change `cpX` from `lx*cos + ly*sin` to `lx*cos - ly*sin` (line 270)
- [x] 2.2 Change `cpY` from `-lx*sin + ly*cos` to `lx*sin + ly*cos` (line 271)

## 3. Fix ContainsPoint to use inverse transform (Text.cs)

- [x] 3.1 Change `u = cpX*cos - cpY*sin` to `u = cpX*cos + cpY*sin` (line 242)
- [x] 3.2 Change `v = cpX*sin + cpY*cos` to `v = -cpX*sin + cpY*cos` (line 243)

## 4. Update Tests for RotatedCorner (TextTests.cs)

- [x] 4.1 Update `RotatedCorner_90Deg_UsesCorrectedDimensions` — revert Corner1Y to `(Y+H) - W*sin90`, Corner2X to `X - H*sin90`
- [x] 4.2 Update `RotatedCorner_180Deg_UsesCorrectedDimensions` — no change needed (sin=0, formulas identical)
- [x] 4.3 Update `RotatedCorner_270Deg_UsesCorrectedDimensions` — revert Corner1Y to `(Y+H) - W*sin270`, Corner2X to `X - H*sin270`
- [x] 4.4 Update `RotatedCorner_45Deg_MatchesCwRotation` — revert to `- W*sin45`, `- H*sin45`, `W*cos - H*sin`, `- W*sin - H*cos`

## 5. Update Tests for GetBoundingBox (TextTests.cs)

- [x] 5.1 Update `GetBoundingBox_Rotated90Deg_CorrectBounds` — revert expected bounds to `Left=-h, Bottom=h-w, Right=0, Top=h`

## 6. Rewrite ContainsPoint Tests (TextTests.cs)

- [x] 6.1 Update `ContainsPoint_Rotated90Deg_HitsVisualCorner` — use correct center of rotated AABB `(-h/2, h-w/2)` with inverse transform
- [x] 6.2 Keep `ContainsPoint_CorrectedMetrics_RotatedText` — unchanged (0° test is valid for both forward and inverse)

## 7. Build and verify

- [x] 7.1 Build solution — 0 errors, 0 warnings
- [x] 7.2 Run all text-related tests — all pass
- [x] 7.3 Run full test suite — no regressions
