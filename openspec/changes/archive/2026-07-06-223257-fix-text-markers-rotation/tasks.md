## 1. FontMetrics class

- [x] 1.1 Create `Models/FontMetrics.cs` with static class, `_heightRatios` and `_widthRatios` dictionaries, `IsInitialized` flag
- [x] 1.2 Add `Initialize()` method that loads GOST TTF files via `GlyphTypeface` and extracts `Height` ratio per font
- [x] 1.3 Add `Initialize()` logic to compute average advance width across ASCII Latin + Cyrillic character set per font
- [x] 1.4 Add try/catch fallback — if font loading fails, use existing heuristic ratios (1.0 for height, 0.5/0.65 for width)
- [x] 1.5 Add `InitializeWithTestValues(double hRatio, double wRatio, string fontName)` for testability
- [x] 1.6 Add `GetHeightRatio(string fontName)` and `GetAdvWidthRatio(string fontName)` accessors
- [x] 1.7 Call `FontMetrics.Initialize()` in `App.xaml.cs` startup (WPF STA thread)

## 2. Text geometry correction

- [x] 2.1 Update `Text.HeightMicrons`: multiply `FontSizeMicrons` by `FontMetrics.GetHeightRatio(fontName)` for single-line height; apply same ratio to line spacing for multi-line
- [x] 2.2 Update `Text.WidthMicrons`: use `FontMetrics.GetAdvWidthRatio(fontName)` in the computation instead of hardcoded factor
- [x] 2.3 Remove `GetCharWidthFactor()` — replaced entirely by `FontMetrics.GetAdvWidthRatio()`
- [ ] 2.4 Verify `Text.BottomMicronsY`, `CenterMicronsY` etc. still correct with new HeightMicrons (they use `MicronsY + HeightMicrons` — no change needed, just verify)
- [ ] 2.5 Verify `Text.RotatedCorner*` properties use the corrected Width/Height (they do via `WidthMicrons`/`HeightMicrons` — no change needed)
- [ ] 2.6 Verify `Text.ContainsPoint()` uses corrected Width/Height (it does via local w/h variables — no change needed)
- [ ] 2.7 Verify `Text.GetBoundingBox()` uses corrected Width/Height (it does — no change needed)
- [ ] 2.8 Verify `HitTestHelper.GetTextHandle()` uses corrected geometry (via `text.WidthMicrons`/`text.HeightMicrons` — no change needed)
- [ ] 2.9 Verify `ResizeMath.ComputeTextResize()` works correctly with corrected dimensions (caller passes `text.FontSizeMicrons` as startHeight — unchanged; width projection uses corrected `WidthMicrons` — unchanged API)

## 3. Test updates

- [x] 3.1 Add unit tests for `FontMetrics` initialization with test values
- [x] 3.2 Add unit tests for `Text.HeightMicrons` and `Text.WidthMicrons` with corrected font metrics
- [x] 3.3 Add unit tests for `Text.RotatedCorner*` positions at 90°, 180°, 270° with corrected dimensions
- [x] 3.4 Add unit tests for `HitTestHelper.GetTextHandle()` at 90°, 180°, 270° (existing tests only test 0°)
- [x] 3.5 Run full test suite — 1821 passed, 1 pre-existing skip, 0 new failures

## 4. Build and verification

- [x] 4.1 Build solution — 0 errors, 0 warnings
- [x] 4.2 Run all tests — 1821 passed, 1 pre-existing skip, 0 new failures
- [ ] 4.3 Create A4 sheet, add text object, rotate to 90° — verify markers appear at text corners
- [ ] 4.4 Repeat for 180° and 270° — verify markers aligned with corners
- [ ] 4.5 Verify hit-test on rotated text handles works (click near marker → resize activates)
- [ ] 4.6 Verify resize via corner handle on rotated text works correctly
