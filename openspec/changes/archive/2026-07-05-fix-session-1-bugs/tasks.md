## 1. Quick fixes (Suggestion severity)

- [x] 1.1 Increase `diagonalTolerance` from 1 to 5000 in `DrawingLineTool.ApplyConstraint()` — makes 45° diagonal achievable with Shift
- [x] 1.2 Change `Coordinate.FormatMm()` from `"F3"` to `"0.###"` — removes trailing `.000` for whole mm values

## 2. Default font size (Critical)

- [x] 2.1 Change `DefaultFontSizeMicrons` from 3500 to 14000 in `EditorSettings.cs`
- [x] 2.2 Search and fix any tests that hardcode `DefaultFontSizeMicrons = 3500` or assert the old value
- [x] 2.3 Build: 0 errors, 0 warnings

## 3. Rotated text markers — visual positions (Critical, F4a)

- [x] 3.1 Add 4 pairs of computed properties to `Text.cs`: `RotatedCorner0X/Y` through `RotatedCorner3X/Y` with INPC via dependency properties
- [x] 3.2 Update `Text.cs` `OnPropertyChanged()` cascade: when `RotationAngle`, `MicronsX`, `MicronsY`, `WidthMicrons`, or `HeightMicrons` change, notify all `RotatedCorner*` properties
- [x] 3.3 Update `EditorCanvas.xaml` Text DataTemplate: bind the 4 `SquareMarker` rectangles to `RotatedCornerNX/NY` instead of `VisualLeft/Right/Top/Bottom`
- [x] 3.4 Build: 0 errors, 0 warnings

## 4. Rotated text markers — hit-test (Critical, F4b)

- [x] 4.1 Rewrite `HitTestHelper.GetTextHandle()` to compute rotated corner positions (same math as `Text.GetBoundingBox()` corner 1-4) and check distance to those
- [x] 4.2 Build: 0 errors, 0 warnings

## 5. Rotated text resize (Critical, F4c)

- [x] 5.1 In `ResizeMath.ComputeTextResize()`, add rotation angle parameter and project `(dx, dy)` into text-local coordinate space before computing scale
- [x] 5.2 Update `ResizeTool.ResizeText()` call to pass `text.RotationAngle`
- [x] 5.3 Build: 0 errors, 0 warnings

## 6. Tests and verification

- [x] 6.1 Full test suite: 1796 passed, 0 failed, 1 pre-existing skip
- [ ] 6.2 Manual verification: run app, create A3+text, rotate 45°, check markers at corners
- [ ] 6.3 Manual regression: open .tdel with rotated text from before the fix
