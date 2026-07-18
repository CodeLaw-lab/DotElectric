## Why

Selection markers for rotated text have three remaining defects after the Sprint 59 math fix: the resize cursor shows the wrong diagonal direction at non-zero rotation angles, dead XAML code pollutes the Text DataTemplate, and a narrow int-cast risks overflow on extreme dimensions. These are minor but felt — wrong cursor undermines user confidence in the rotate/resize interaction.

## What Changes

1. **Resize cursor respects text rotation** — `CursorForHandle()` computes cursor direction from the actual visual position of each handle (accounting for `RotationAngle`), not from the unrotated semantic name.
2. **Remove dead `<Canvas/>`** — delete the empty `<Canvas IsHitTestVisible="False"/>` from the Text DataTemplate in `EditorCanvas.xaml`.
3. **Widen `GetTextHandle` parameters** — change `(int w, int h)` to `(long w, long h)` to eliminate the theoretical overflow risk.

## Capabilities

### New Capabilities
- `rotated-resize-cursor`: Cursor visual feedback for rotated text resize handles — the cursor direction (NWSE/NESW) matches the visual corner of the selected text, not the unrotated semantic name.

### Modified Capabilities
- `rotated-text-markers`: Add requirement that resize cursor direction matches the visual corner position for all rotation angles.

## Impact

- `Tools/ResizeMath.cs` — `CursorForHandle()` signature unchanged, internal logic gains rotation-based cursor selection.
- `Helpers/HitTestHelper.cs` — `GetTextHandle()` local function parameter types widened from `int` to `long`.
- `Views/EditorCanvas.xaml` — one line removed from Text DataTemplate.
- No new dependencies. No breaking changes. 3 files modified.
