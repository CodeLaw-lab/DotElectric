## Why

After the `session1-defects` fix (Sprint 37–), rotated text selection markers are positioned using the model's `RotatedCorner*` properties based on `WidthMicrons`/`HeightMicrons` heuristics. Manual testing reveals the markers are systematically offset from the actual rendered text corners at 90°, 180°, and 270° rotations. The root cause is a mismatch between the model's geometric heuristics and WPF's actual text measurement — the model doesn't account for font ascent/descent metrics, so the computed corners don't match where WPF renders the glyphs.

This makes rotated text appear incorrectly selected and resize handles unreachable at their expected positions.

## What Changes

- Fix `Text.HeightMicrons` computation to account for actual WPF font ascent/descent, so the model's bounding box matches WPF's rendered text bounds
- Fix `Text.RotatedCorner*` positioning to use the corrected height, making selection markers appear at the actual text corners at all rotation angles
- Add font metric constants for GOST AU/BU (ascent ratio, descent ratio) derived from the TTF files
- Update `HitTestHelper.GetTextHandle()` to use the corrected geometry
- Adjust `ContainsPoint()` and `GetBoundingBox()` for consistency
- Add tests at 90°, 180°, 270° (and possibly 45°) verifying marker positions match expected visual corners

## Capabilities

### New Capabilities
- `text-font-metrics`: WPF font metric extraction and compensation — provide ascent/descent ratios for GOST fonts so the model can compute geometrically accurate bounding boxes

### Modified Capabilities
- `session1-defects`: the "Text selection markers follow rotation" requirement is NOT fully met — markers are at computed positions that don't match WPF rendering. The spec for that requirement needs updating to include ascent/descent compensation.

## Impact

- **Text.cs**: `HeightMicrons` computation, `RotatedCorner*` properties, `ContainsPoint()`, `GetBoundingBox()`
- **Models**: new `FontMetrics` record or constants for ascent/descent
- **Tests**: `TextTests.cs`, `HitTestHelperTests.cs` — new test cases for rotated corner positions at non-zero angles
- **No WPF/XAML changes**: the fix is entirely in model-layer geometry
