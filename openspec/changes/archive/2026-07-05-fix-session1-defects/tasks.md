## 1. TextTool Preview Fix

- [x] 1.1 In `Tools/TextTool.cs:52-60`, add `_context.PreviewText = _previewText;` after each coordinate mutation in `OnMouseMove` (matching DrawingLineTool/DrawingRectangleTool pattern)

## 2. ResizeTool Escape PopTool

- [x] 2.1 In `Tools/ResizeTool.cs:233-241`, add `_context.PopTool();` before `Reset()` in the Escape handler

## 3. Tool Switch Reset

- [x] 3.1 In `MainWindow.xaml.cs:26-31`, call `Reset()` on the current tool before switching — already implemented via `EditorViewModel.SetActiveTool()` at line 261 (`_toolManager.ResetTool(prevTool)`)

## 4. DrawingLineTool 45° Diagonal

- [x] 4.1 In `Tools/DrawingLineTool.cs:123-139`, add a third branch in `ApplyConstraint()`: when `|dx - dy| < 1 micron` (tolerance), fix both endpoints to create a 45° diagonal

## 5. Undo Selection Restore

- [x] 5.1 In `ViewModels/EditorViewModel.cs:441` (`Undo()`), after `PurgeOrphanedSelection()`, re-select the restored object if it was selected before deletion

## 6. DoubleClick Switches to Select

- [x] 6.1 In `Tools/DrawingLineTool.cs:84-90`, add `_context.SetActiveToolCommand.Execute("Select")` in `OnDoubleClick()`
- [x] 6.2 In `Tools/TextTool.cs:87-92`, add `_context.SetActiveToolCommand.Execute("Select")` in `OnDoubleClick()`

## 7. Rectangle Clamp at Sheet Edge

- [x] 7.1 In `Tools/DrawingRectangleTool.cs:68-78`, add position adjustment: if `x + w > sheetW`, set `x = sheetW - w`

## 8. Build and Verify

- [x] 8.1 Run `dotnet build src/DotElectric.TemplateEditor.slnx` — 0 errors, 0 warnings
- [x] 8.2 Run `dotnet test src/DotElectric.TemplateEditor.Tests` — 1796 passed, 1 pre-existing skip
