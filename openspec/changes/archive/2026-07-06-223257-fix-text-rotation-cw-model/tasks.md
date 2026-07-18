## 1. Fix rotation math in Text.cs

- [x] 1.1 Fix `RotatedCorner1Y`: change `- W·sinθ` to `+ W·sinθ`
- [x] 1.2 Fix `RotatedCorner2X`: change `- H·sinθ` to `+ H·sinθ`
- [x] 1.3 Fix `RotatedCorner3X`: change `- H·sinθ` to `+ H·sinθ` (within `W·cos - H·sin` → `W·cos + H·sin`)
- [x] 1.4 Fix `RotatedCorner3Y`: change `- W·sin - H·cos` to `+ W·sin - H·cos`
- [x] 1.5 Fix `ContainsPoint()`: change `u = cpX·cos + cpY·sin` to `u = cpX·cos - cpY·sin`, and `v = -cpX·sin + cpY·cos` to `v = cpX·sin + cpY·cos`
- [x] 1.6 Fix `GetBoundingBox()` forward rotation: change `cpX = lx·cos - ly·sin` to `cpX = lx·cos + ly·sin`, and `cpY = lx·sin + ly·cos` to `cpY = -lx·sin + ly·cos`

## 2. Fix HitTestHelper.cs

- [x] 2.1 Fix `GetTextHandle()` CornerX: change `localX·cos - localY·sin` to `localX·cos + localY·sin`
- [x] 2.2 Fix `GetTextHandle()` CornerY: change `localX·sin + localY·cos` to `-localX·sin + localY·cos`

## 3. Update unit tests

- [x] 3.1 Update `RotatedCorner_90Deg_UsesCorrectedDimensions` in `TextTests.cs` with new expected values
- [x] 3.2 Update `RotatedCorner_270Deg_UsesCorrectedDimensions` in `TextTests.cs` with new expected values
- [x] 3.3 Update `RotatedCorner_180Deg_UsesCorrectedDimensions` — verify no change needed (sin=0)
- [x] 3.4 Add test `RotatedCorner_45Deg_MatchesCwRotation` in `TextTests.cs` to verify non-orthogonal angles
- [x] 3.5 Add test `ContainsPoint_Rotated90Deg_HitsVisualCorner` in `TextTests.cs`
- [x] 3.6 Add test `GetBoundingBox_Rotated90Deg_CorrectBounds` in `TextTests.cs`
- [x] 3.7 Update hit-test helper tests for 90° and 270° expected handle positions

## 4. Build and verify

- [ ] 4.1 Build solution — 0 errors, 0 warnings
- [ ] 4.2 Run all tests — all pass
- [ ] 4.3 Manual visual verification: create text at 90°, check markers align with visual corners
- [ ] 4.4 Manual visual verification: repeat at 45°, 180°, 270°
- [ ] 4.5 Manual verification: hit-test on rotated text handles works (click near marker → resize activates)
