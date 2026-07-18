## 1. Rotation-aware cursor direction

- [x] 1.1 Add `VisualCursorForHandle(ResizeHandle handle, int rotationAngle)` to `ResizeMath.cs` that flips diagonal cursors when `rotationAngle` is 90 or 270
- [x] 1.2 Update `ResizeTool.GetCursor()` to call `VisualCursorForHandle` when selected object is `Text`
- [x] 1.3 Verify cursor correctness at 0°, 90°, 180°, 270° for all four corner handles — confirmed by 16 theory tests

## 2. Dead XAML cleanup

- [x] 2.1 Remove empty `<Canvas IsHitTestVisible="False"/>` line from Text DataTemplate in `EditorCanvas.xaml`

## 3. Widen GetTextHandle parameters

- [x] 3.1 Change `CornerX(int localX, int localY)` in `HitTestHelper.GetTextHandle` to `CornerX(long localX, long localY)` — remove the `(int)` casts at call sites

## 4. Tests

- [x] 4.1 Add cursor tests to `ResizeToolTests`: 16 theory tests for corner handles at 0°/90°/180°/270° + 4 tests for edge handles unchanged
- [x] 4.2 Verify all existing tests still pass: 191 passed (`ResizeTool|ResizeMath|HitTest` filter)

## 5. Verification

- [x] 5.1 Build solution: 0 errors, 0 warnings
- [x] 5.2 Run full test suite: 1844 passed, 1 pre-existing skip
