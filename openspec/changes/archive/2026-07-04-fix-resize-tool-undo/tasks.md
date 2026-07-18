## 1. Core fix in ResizeTool

- [x] 1.1 Add `_initialState` field (`ResizeState?`) to `ResizeTool`
- [x] 1.2 In `OnMouseDown()`, save `_initialState = _resizedObject.CaptureResizeState()` after assigning `_resizedObject`
- [x] 1.3 In `OnMouseUp()`, capture `finalState = _resizedObject.CaptureResizeState()` and local `var captured = _resizedObject`
- [x] 1.4 Pass `_initialState!` as `oldValue`, `finalState` as `newValue`, and `s => captured.ApplyResize(s)` as setter to `ChangePropertyCommand<ResizeState>`

## 2. Tests

- [x] 2.1 Add unit test: `ResizeTool_Undo_Rectangle_RestoresInitialState` — verify Undo restores original MicronsX/Y/Width/Height
- [x] 2.2 Add unit test: `ResizeTool_Undo_Line_RestoresInitialState` — verify Undo restores original Start/End points
- [x] 2.3 Add unit test: `ResizeTool_Undo_Text_RestoresInitialState` — verify Undo restores original position and FontSize
- [x] 2.4 Add unit test: `ResizeTool_Undo_DoesNotThrow` — verify Ctrl+Z after resize produces no exception
- [x] 2.5 Verify all existing ResizeTool tests (63 tests) still pass

## 3. Validation

- [x] 3.1 Run `dotnet build` — 0 errors, 0 warnings
- [x] 3.2 Run `dotnet test` — all 1700+ tests pass, no regression
