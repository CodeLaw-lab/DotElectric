# session1-defects Specification

## Purpose
Requirements and fixes for defects found in Session 1 manual testing of DotElectric Template Editor.

## Requirements

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

## ADDED Requirements

### Requirement: Default font size is large enough for selection

Text objects SHALL be created with a default font size of at least 14000 microns (14 mm) to ensure the text is visible and clickable at 100% zoom without requiring the user to zoom in first.

#### Scenario: New text object is selectable at 100% zoom

- **WHEN** user creates a new text object using TextTool
- **THEN** the text object SHALL be rendered at default font size 14 mm or larger
- **AND** the text object SHALL be selectable by clicking within its bounding box at 100% zoom

### Requirement: StatusBar sheet format omits trailing zeros

The StatusBar SHALL display sheet dimensions without unnecessary decimal places. For whole millimeter values, no decimal point or fractional part SHALL be shown.

#### Scenario: Standard A3 landscape shows clean format

- **WHEN** an A3 Landscape sheet is created (420000 x 297000 microns)
- **THEN** StatusBar SHALL display "A3 (алб.) 420 x 297 мм"
- **AND** SHALL NOT display "420.000 x 297.000 мм"

### Requirement: Text selection markers follow rotation

When a Text object is rotated, the 4 selection markers (corner handles) SHALL be positioned at the actual rotated corners of the text bounding box, not at the axis-aligned bounding box (AABB) corners.

#### Scenario: Rotated text markers at 45 degrees

- **WHEN** a Text object is rotated to 45 degrees
- **THEN** the 4 square markers SHALL be positioned at the 4 actual corners of the rotated text
- **AND** SHALL NOT be positioned at the AABB corners (which would be visually offset)

#### Scenario: Rotated text markers at 90 degrees

- **WHEN** a Text object is rotated to 90 degrees
- **THEN** the 4 square markers SHALL be positioned at the actual rotated corners
- **AND** SHALL NOT visually lag or lead the text corners in any direction

### Requirement: Handle hit-test on rotated text uses rotated corners

`HitTestHelper.GetTextHandle()` SHALL detect resize handle clicks on rotated Text objects by checking against the actual rotated corner positions, not against the unrotated `MicronsX`/`RightMicronsX`/`BottomMicronsY` positions.

#### Scenario: Click on rotated text corner activates resize

- **WHEN** user clicks near a corner of a rotated Text object at 45 degrees
- **THEN** the resize tool SHALL activate
- **AND** the correct resize handle (TopLeft/TopRight/BottomLeft/BottomRight) SHALL be detected

### Requirement: Text resize accounts for rotation

`ResizeMath.ComputeTextResize()` SHALL project mouse deltas `(dx, dy)` into the text's rotated local coordinate system before computing the font size scale factor and position adjustment.

#### Scenario: Resize rotated text via corner handle

- **WHEN** user drags a bottom-right corner handle of a Text object rotated 45 degrees
- **THEN** the font size SHALL scale based on the delta projected into the text's local coordinate axes (along and perpendicular to the text baseline)
- **AND** the text position SHALL adjust correctly without unexpected jumps

## MODIFIED Requirements

### Requirement: DrawingLineTool Shift constraint includes 45° diagonal

DrawingLineTool SHALL constrain the line to 45° diagonal when `dx` and `dy` are approximately equal (within 5000 micron tolerance) and Shift is held, in addition to existing horizontal and vertical constraints. The tolerance SHALL be large enough to be achievable with normal mouse movement at typical zoom levels.

#### Scenario: Shift+draw creates 45° diagonal

- **WHEN** user holds Shift and draws a line with approximately equal horizontal and vertical delta (within 5 mm)
- **THEN** the line snaps to exactly 45° (both endpoints have equal dx and dy)

#### Scenario: Shift+draw near-horizontal line stays horizontal

- **WHEN** user holds Shift and draws a predominantly horizontal line (dx exceeds dy by more than 5 mm)
- **THEN** the line snaps to exactly horizontal (same Y for both endpoints)
