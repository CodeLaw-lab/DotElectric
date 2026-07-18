## ADDED Requirements

### Requirement: Drawing Tools — full coverage
The full test suite SHALL cover all drawing tool behaviors: line creation with Shift (horizontal/vertical lock), rectangle creation with Shift (square), text default properties, preview shapes during drawing, Escape cancellation during drawing, and tool switching mid-drawing.

#### Scenario: Line with Shift constraint
- **WHEN** the tester draws a line while holding Shift
- **THEN** the line locks to 0°/45°/90° increments

#### Scenario: Rectangle with Shift constraint
- **WHEN** the tester draws a rectangle while holding Shift
- **THEN** the rectangle maintains 1:1 aspect ratio (square)

#### Scenario: Text default properties
- **WHEN** the tester creates a text object with the T tool
- **THEN** FontName="ГОСТ А", FontSize=3.5mm, Content="Текст", and 4 corner markers appear

#### Scenario: Escape cancels drawing
- **WHEN** the tester starts drawing a line and presses Escape
- **THEN** the preview line disappears and the tool reverts to Select

### Requirement: Selection — single, multi, and box selection
The full test suite SHALL cover single click selection, Shift+Click toggle, Ctrl+Click toggle, LTR selection box (fully enclosed), RTL selection box (any intersection), Rectangle border-band hit test, Z-order hit test, and selection markers visual state.

#### Scenario: Rectangle border-band hit test
- **WHEN** the tester clicks in the center of a large rectangle (over 10mm from any edge)
- **THEN** the rectangle is NOT selected

#### Scenario: Rectangle border-band selectable zone
- **WHEN** the tester clicks within 5mm of the rectangle edge
- **THEN** the rectangle IS selected

#### Scenario: Small rectangle fully selectable
- **WHEN** the tester clicks in the center of a small rectangle (under 10mm in both dimensions)
- **THEN** the rectangle IS selected (entire area is selectable)

### Requirement: Move and drag behavior
The full test suite SHALL verify single-object drag, multi-object drag, snap-constrained drag, drag threshold (objects do not move under 5px mouse movement), sheet bounds clamping during drag, and no delta drift during multi-MouseMove sequences.

#### Scenario: No delta drift during drag
- **WHEN** the tester drags an object back and forth multiple times
- **THEN** the object always stays under the cursor and never drifts away

#### Scenario: Sheet bounds clamp
- **WHEN** the tester drags an object beyond the sheet boundary
- **THEN** the object stops at the boundary (position does not go negative or exceed sheet size)

### Requirement: Nudge with dynamic step
The full test suite SHALL verify arrow key nudging, Shift+arrow big nudge, dynamic nudge step based on snap state (Snap ON → step = grid step, Snap OFF → step = 0.1mm), multi-object nudge, and sheet bounds clamping during nudge.

#### Scenario: Nudge step follows snap state
- **WHEN** Snap is ON with grid step = 5mm and the tester presses an arrow key
- **THEN** the object moves by exactly 5mm

#### Scenario: Big nudge with Shift
- **WHEN** Snap is ON and the tester presses Shift+arrow
- **THEN** the object moves by 10mm (big nudge step)

### Requirement: Resize tool — all handles and modifiers
The full test suite SHALL verify resize via corner handles, side handles, line endpoints, text corner handles, Shift-constrained proportions, Ctrl-centered resize, minimum size clamp (1000 microns), sheet bounds clamp during resize, and live property panel update during resize.

#### Scenario: Minimum size clamp preserves fixed edge
- **WHEN** the tester resizes a rectangle below 1mm by dragging a handle
- **THEN** the size stops at 1mm and the fixed (opposite) edge does NOT move

#### Scenario: Ctrl-centered resize
- **WHEN** the tester holds Ctrl and drags a corner handle
- **THEN** the rectangle center stays fixed and both edges move symmetrically

### Requirement: Rotation with arbitrary angles
The full test suite SHALL verify 90° clockwise rotation (E), 90° counter-clockwise (Shift+E), multi-object rotation, arbitrary angle input via Properties Panel (0-359°), negative angle normalization, and rotation undo/redo.

#### Scenario: Arbitrary angle input
- **WHEN** the tester enters 45 in the Rotation field of Properties Panel
- **THEN** the text rotates 45° clockwise

#### Scenario: 360 normalizes to 0
- **WHEN** the tester enters 360 in the Rotation field
- **THEN** the rotation displays as 0 (normalized)

### Requirement: Properties Panel — all object types
The full test suite SHALL verify that selecting each object type (Line, Rectangle, Text) shows the correct subset of properties, that all property changes apply to the canvas, that ComboBox values sync with model state, that Enter commits text field changes, and that Undo/Redo works for property changes.

#### Scenario: ComboBox sync
- **WHEN** the tester selects a line with LineType=Dash and then selects a line with LineType=Solid
- **THEN** the ComboBox changes from "Dash" to "Solid" accordingly

#### Scenario: Enter commits field change
- **WHEN** the tester changes a coordinate value in a TextBox and presses Enter
- **THEN** the object position updates and an undo command is created

### Requirement: Text inline editing
The full test suite SHALL verify double-click to open inline editor, Ctrl+Enter to commit, Escape to cancel, multi-line editing (Enter = new line, Ctrl+Enter = commit) when TextWrapping is enabled, and inline editing on rotated text.

#### Scenario: Inline editor commit
- **WHEN** the tester double-clicks text, changes the content, and presses Ctrl+Enter
- **THEN** the canvas text is updated and Properties Panel shows the new content

#### Scenario: Inline editor cancel
- **WHEN** the tester double-clicks text, changes the content, and presses Escape
- **THEN** the text reverts to its original content

### Requirement: Colors — all color properties
The full test suite SHALL verify default colors for each object type, HEX color change for StrokeColor/FillColor/Foreground, Transparent fill, ARGB hex (#AARRGGBB) support, Save/Load round-trip for colors, and validation error for invalid hex.

#### Scenario: Fill color transparency
- **WHEN** the tester changes Rectangle FillColor to "Transparent"
- **THEN** the rectangle has no visible fill

#### Scenario: Invalid hex validation
- **WHEN** the tester enters "#XYZ" in a color field
- **THEN** a validation error is shown

### Requirement: Clipboard — copy, paste, cut
The full test suite SHALL verify single and multi-object copy, paste with offset accumulation, re-paste without reference reuse (clone on every get), paste offset reset after new copy, cut with delete, multi-object undo as single BatchCommand, and clipboard clearing on tab close.

#### Scenario: Re-paste creates independent copies
- **WHEN** the tester copies one object and pastes 5 times
- **THEN** 5 independent copies appear at increasing offsets, and deleting one does not affect the others

#### Scenario: Multi-object undo is batched
- **WHEN** the tester cuts 3 objects and presses Ctrl+Z once
- **THEN** all 3 objects are restored in a single undo action

### Requirement: Undo/Redo — all operations
The full test suite SHALL verify single-operation undo/redo, 50-level multi-undo, undo after redo with new action (clears redo stack), Undo display names (ОТМЕНИТЬ: Добавить объект, Удалить объект, Переместить, Изменить размер, Повернуть), BatchCommand undo for multi-object operations, undo after Save, and orphaned selection cleanup after undo.

#### Scenario: 50-level undo stack
- **WHEN** the tester creates 50 objects and presses Ctrl+Z 50 times
- **THEN** all objects are removed one by one

#### Scenario: Orphaned selection cleanup
- **WHEN** the tester selects an object, deletes it, then undoes the deletion
- **THEN** the object is restored but the selection is cleared (no orphaned markers)

### Requirement: Grid and Snap
The full test suite SHALL verify grid toggle (F7), snap toggle (F8), multiple grid steps (1mm, 2mm, 5mm, 10mm), MinPixelSpacing behavior (1mm grid hidden below 500% zoom), snap-constrained drawing, snap-constrained dragging, and grid autoscroll during pan.

#### Scenario: 1mm grid at 500% zoom
- **WHEN** the tester sets grid step to 1mm and zoom to 500%
- **THEN** grid nodes become visible (pixel spacing reaches MinPixelSpacing threshold)

#### Scenario: Snap constraint during drawing
- **WHEN** the tester draws a line with Snap ON and grid step 10mm
- **THEN** the line start and end points snap to 10mm grid nodes

### Requirement: Zoom and Pan
The full test suite SHALL verify mouse wheel zoom (10%-1000% range), zoom via ComboBox (select and type), Fit to Screen (Ctrl+0), middle-button pan, Space+left-button pan, mouse capture during pan (works outside canvas bounds), scrollbar interaction, and no runaway acceleration during pan.

#### Scenario: Pan without runaway acceleration
- **WHEN** the tester pans rapidly back and forth with the middle mouse button
- **THEN** the canvas returns to the original position without accumulated offset

#### Scenario: Mouse capture outside canvas
- **WHEN** the tester starts panning and moves the cursor outside the canvas boundary
- **THEN** panning continues and releasing the button stops panning correctly

### Requirement: File operations
The full test suite SHALL verify new template creation (all standard formats, half-formats, custom format, orientation), save new file, save existing file, Save As (F12), open file, close with unsaved changes dialog (Save/Discard/Cancel), autosave and restore, template library import/remove, and XML coordinate precision (microns, no loss).

#### Scenario: All standard format sizes
- **WHEN** the tester creates each format (A0–A4) in both landscape and portrait orientations
- **THEN** each template has the correct dimensions per GOST specification

#### Scenario: Half-format dimensions
- **WHEN** the tester creates A4x2, A3x2, A2x2, A1x2, A0x2
- **THEN** each has the correct doubled long-side dimension

#### Scenario: Micron coordinate precision round-trip
- **WHEN** the tester saves a file with fractional mm coordinates and reopens it
- **THEN** the micrometer-precision coordinates are preserved (no floating-point loss)

### Requirement: Print Preview
The full test suite SHALL verify Ctrl+Shift+P opens DocumentViewer window, all object types render (line, rectangle, text), colors render correctly, dashed line types render correctly, rotated text renders correctly at the correct angle, and the Print button in the preview window works.

#### Scenario: Object types in print preview
- **WHEN** the tester creates one object of each type (line, rectangle, text) and presses Ctrl+Shift+P
- **THEN** all three objects are visible in the DocumentViewer preview

#### Scenario: Colors in print preview
- **WHEN** the tester creates objects with non-default colors and opens print preview
- **THEN** the preview shows the correct colors (red line, blue fill, etc.)

### Requirement: Keyboard shortcuts
The full test suite SHALL verify all documented shortcuts work (V/L/R/T/E, Ctrl+N/O/S/Z/Y/C/V/X/A/Z, Delete, Ctrl+0/+/-, F7/F8/F9, Ctrl+Shift+P), shortcuts work with Russian keyboard layout (physical key position), Ctrl+modifier shortcuts are not intercepted by tool switcher, Escape routing (cancel drawing, cancel inline edit, cancel resize), and arrow key nudging.

#### Scenario: Russian layout tool switching
- **WHEN** the tester switches to Russian keyboard layout and presses M (physical V key)
- **THEN** the Select tool is activated

#### Scenario: Ctrl+V not intercepted by tool switcher
- **WHEN** the tester presses Ctrl+V
- **THEN** paste is executed, NOT Select tool activation

### Requirement: Context menus
The full test suite SHALL verify right-click on canvas shows context menu with Cut/Copy/Paste/Delete/Select All, right-click on object shows context menu with working commands, right-click on tab shows Close/Close Others/Close All, and middle-click on tab closes it.

### Requirement: Themes and Settings
The full test suite SHALL verify F9 theme toggle (Light/Dark), theme persistence across restart, Settings dialog (File → Settings) with four sections (ВНЕШНИЙ ВИД, СЕТКА, АВТОСОХРАНЕНИЕ, НОВЫЙ ШАБЛОН), settings persistence across sessions, and zoom/grid format defaults applied to new templates.

#### Scenario: Settings persistence
- **WHEN** the tester changes theme to Dark, grid step to 10mm, default format to A4, then restarts the app
- **THEN** theme is Dark, grid step is 10mm, and Ctrl+N creates A4

### Requirement: StatusBar
The full test suite SHALL verify StatusBar shows sheet format/size/orientation, current zoom percentage, Grid and Snap toggle state, and feedback messages (copy/cut/paste results, buffer empty).

#### Scenario: StatusBar after copy
- **WHEN** the tester copies an object
- **THEN** the StatusBar shows "Скопировано: 1 объект"
