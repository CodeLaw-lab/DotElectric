## 1. Fix RotatedCorner sign errors in Text.cs

- [x] 1.1 Fix `RotatedCorner1Y` (line 165): change `- (long)Math.Round(W * sin)` to `+ (long)Math.Round(W * sin)`
- [x] 1.2 Fix `RotatedCorner2X` (line 167): change `- (long)Math.Round(H * sin)` to `+ (long)Math.Round(H * sin)`
- [x] 1.3 Fix `RotatedCorner3X` (line 170): change `... - H * sin` to `... + H * sin`
- [x] 1.4 Fix `RotatedCorner3Y` (line 171): change `(Y+H) - Math.Round(W*sin + H*cos)` to `(Y+H) + Math.Round(W*sin - H*cos)`

## 2. Fix GetBoundingBox rotation math in Text.cs

- [x] 2.1 Fix `cpX` (line 270): change `lx * cosA - ly * sinA` to `lx * cosA + ly * sinA`
- [x] 2.2 Fix `cpY` (line 271): change `lx * sinA + ly * cosA` to `-lx * sinA + ly * cosA`

## 3. Remove dead code TextSelectionMarkerBehavior

- [x] 3.1 Delete `Behaviors/TextSelectionMarkerBehavior.cs`
- [x] 3.2 Remove `behaviors:TextSelectionMarkerBehavior.IsEnabled="True"` from `EditorCanvas.xaml` Text DataTemplate (line 198)

## 4. Add/update tests

- [x] 4.1 `RotatedCorner_90Deg_UsesCorrectedDimensions` — assertions updated for corrected CW math
- [x] 4.2 `RotatedCorner_180Deg_UsesCorrectedDimensions` — verified (sin=0, formulas identical)
- [x] 4.3 `RotatedCorner_270Deg_UsesCorrectedDimensions` — assertions updated for corrected CW math
- [x] 4.4 `GetBoundingBox_Rotated90Deg_CorrectBounds` — assertions updated for corrected AABB
- [x] 4.5 `RotatedCorner_45Deg_MatchesCwRotation` — assertions updated for corrected CW math
- [x] 4.6 `ContainsPoint_Rotated90Deg_HitsVisualCorner` — verified (math unaffected by fix)
- [x] 4.7 `ContainsPoint_CorrectedMetrics_RotatedText` — verified (math unaffected by fix)

## 5. Build and verify

- [x] 5.1 Build solution — 0 errors, 0 warnings
- [x] 5.2 Run all tests — 56/56 TextTests passed (0 failures)
- [ ] 5.3 Create A4 sheet, add text object, rotate to 90° — verify markers appear at text corners
- [ ] 5.4 Repeat for 45°, 180°, 270° — verify markers aligned with corners
- [ ] 5.5 Verify hit-test on rotated text at 90° — click on visible text area selects the object
