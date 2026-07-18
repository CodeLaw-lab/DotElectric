## ADDED Requirements

### Requirement: TextTool preview follows mouse

TextTool SHALL update preview position on every MouseMove by re-assigning `_context.PreviewText` after mutating coordinates, matching the behavior of DrawingLineTool and DrawingRectangleTool.

#### Scenario: Text preview moves with cursor

- **WHEN** user clicks TextTool on canvas and drags mouse
- **THEN** the preview text indicator follows the cursor position in real time

### Requirement: ResizeTool Escape pops tool stack

ResizeTool SHALL call `_context.PopTool()` when handling Escape key, removing `"Resize"` from the tool stack and returning control to SelectTool.

#### Scenario: Escape during resize returns to normal operation

- **WHEN** user presses Escape during an active resize operation
- **THEN** the resize is cancelled
- **AND** subsequent mouse clicks on canvas perform normal selection, not resize

### Requirement: Tool switch resets current tool state

Switching tools via keyboard shortcut SHALL call `Reset()` on the currently active tool before activating the new tool, preventing stale state (`_startPoint`, `_isResizing`) from persisting in cached tool instances.

#### Scenario: Switch tool mid-drawing clears old tool state

- **WHEN** user starts drawing a line (first click, preview visible)
- **AND** presses another tool shortcut (e.g., R for Rectangle)
- **AND** presses the original tool shortcut again (e.g., L for Line)
- **THEN** the Line tool is in clean state, ready to draw a new line starting from the next click

### Requirement: DrawingLineTool Shift constraint includes 45° diagonal

DrawingLineTool SHALL constrain the line to 45° diagonal when `dx` and `dy` are approximately equal (within 1 micron tolerance) and Shift is held, in addition to existing horizontal and vertical constraints.

#### Scenario: Shift+draw creates 45° diagonal

- **WHEN** user holds Shift and draws a line with approximately equal horizontal and vertical delta
- **THEN** the line snaps to exactly 45° (both endpoints have equal dx and dy)

### Requirement: Undo restores selection of deleted objects

EditorViewModel.Undo() SHALL re-select objects that were restored by the undo operation if they were selected before deletion.

#### Scenario: Undo restore shows selection markers

- **WHEN** user creates and selects an object
- **AND** presses Delete
- **AND** presses Ctrl+Z
- **THEN** the object is restored and displayed with selection markers (handles)

### Requirement: DoubleClick in drawing tools switches to Select

DrawingLineTool and TextTool SHALL switch active tool to `"Select"` when handling DoubleClick, consistent with Escape key behavior.

#### Scenario: DoubleClick during drawing cancels and selects

- **WHEN** user starts drawing a line (first click)
- **AND** double-clicks instead of single-clicking to finish
- **THEN** the line preview is cancelled
- **AND** active tool switches to Select

### Requirement: Rectangle clamp prevents overflow beyond sheet edge

DrawingRectangleTool SHALL adjust the rectangle's X position leftward when the clamped minimum width (`minSize`) would cause the right edge to exceed the sheet boundary.

#### Scenario: Create rectangle at sheet edge stays within bounds

- **WHEN** user creates a small rectangle at the far right edge of the sheet (within `minSize` distance)
- **THEN** the rectangle's right edge does not extend beyond the sheet boundary
- **AND** the rectangle is fully visible within the sheet
