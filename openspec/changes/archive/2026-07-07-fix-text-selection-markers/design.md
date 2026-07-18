## Context

After the Sprint 59 rotation math fix, text selection markers are positioned correctly at all rotation angles, and hit-testing works. Three minor defects remain:

1. **Cursor direction** — `ResizeMath.CursorForHandle()` maps handle names to cursors based on unrotated semantics. At 90°, `TopRight` is visually at bottom-left but still shows `SizeNESW`.
2. **Empty `<Canvas/>`** — Dead code `EditorCanvas.xaml:235` from the deleted `TextSelectionMarkerBehavior`.
3. **`int` cast** — `GetTextHandle` casts `long w/h` to `int`, risking overflow at extreme dimensions.

All three are contained in 3 files with no cross-cutting effects.

## Goals / Non-Goals

**Goals:**
- Cursor direction respects text `RotationAngle` for all handles and all angles
- Remove dead XAML from Text DataTemplate
- Widen `GetTextHandle` parameters from `int` to `long`

**Non-Goals:**
- Marker positioning (already correct after Sprint 59)
- Hit-test tolerance values
- Line or Rectangle resize cursor behavior
- New marker types or visual changes

## Decisions

### D1: Rotation-aware cursor via a rotation-to-cursor helper

**Option A (chosen):** Create `ResizeMath.VisualCursorForHandle(ResizeHandle, int rotationAngle)` that determines the effective visual direction by checking whether the `RotationAngle` flips the handle's visual quadrant.

**Option B:** Modify `CursorForHandle()` to take rotationAngle and handle all cases inline. Rejected because the function is also called for non-text objects (lines, rectangles) where rotationAngle is always 0 — adding the parameter would require passing 0 for all non-text calls.

**Option C:** Move cursor logic into the tools (SelectTool, ResizeTool). Rejected because `CursorForHandle` is already the central cursor dispatch — splitting it would scatter rotation awareness.

**How it works:**
- At 0°/180°, visual positions match handle names → cursor unchanged
- At 90°/270°, `TopRight` and `BottomLeft` swap their quadrant → cursors must swap
- The rule: a handle with `Top` in its name flips to `Bottom` when `rotationAngle % 180 == 90`, and a handle with `Left`/`Right` flips similarly
- `SizeNS`/`SizeWE` (edge handles) are unaffected because their directions are invariant under rotation

```python
# Pseudocode for the rule
def visual_cursor(handle, rotation_angle):
    effective_handle = handle
    if rotation_angle % 180 == 90:
        # Flip vertical
        if 'Top' in handle: effective_handle.replace('Top', 'Bottom')
        elif 'Bottom' in handle: effective_handle.replace('Bottom', 'Top')
        # Flip horizontal  
        if 'Left' in handle: effective_handle.replace('Left', 'Right')
        elif 'Right' in handle: effective_handle.replace('Right', 'Left')
    return cursor_for_handle(effective_handle)
```

### D2: Simple removal of dead XAML

The empty `<Canvas IsHitTestVisible="False"/>` in the Text DataTemplate (`EditorCanvas.xaml:235`) has no bindings, no children, no purpose. Removed in one line.

### D3: `long` parameters in local function

Change the local function `CornerX(int localX, int localY)` in `GetTextHandle` to `CornerX(long localX, long localY)`. The callers already pass `long` values cast to `int` — removing the cast is a strict improvement with no behavioral change.

## Risks / Trade-offs

- **Risk:** The cursor rotation formula might be wrong at non-standard angles (15°, 73°, etc.). Only 90°/270° produce a flip. Other angles interpolate visually but neither the marker direction NOR the cursor changes at e.g. 45° — the cursor still reflects the unrotated diagonal, which is an acceptable approximation since the handle is no longer axis-aligned anyway.
- **Trade-off:** Adding `rotationAngle` to `CursorForHandle()` changes its signature. Using a separate helper avoids touching the Rectangle/Line codepath but introduces a new function to maintain. Minimal risk given the function is 15 lines and has a single caller.
