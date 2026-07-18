## ADDED Requirements

### Requirement: Regression tests cover historically fixed bugs
The regression test suite SHALL verify that bugs fixed in Sprint 37–52 do not reappear. It SHALL include tests for: ToModelPoint double-compensation (S37), visual selection state (S37), preview shape re-assign (S37), Canvas zoom resize (S37), Escape tool reset (S37), selection box rendering (S37), ComboBox LineType sync (S38), coordinate INPC for canvas redraw (S38), Undo orphaned selection cleanup (S38), Rectangle border-band hit test (S39), keyboard layout independence (S40), marker visibility for multi-select (S40), drag delta drift (S41), pan delta runaway (S45), context menu blocked by e.Handled (S46), TabItem context menu async naming (S46), 1mm grid MinPixelSpacing (S47), dirty indicator forwarding (S48), resize clamp fixed-edge preservation (S49), re-paste instance duplication (S50), Ctrl+V intercepted by tool switcher (S50), pan mouse capture (S51), and font internal name mismatch (S52).

#### Scenario: Pan delta does not accumulate (S45 regression)
- **WHEN** the tester middle-button pans in one direction, then back to the starting point
- **THEN** the canvas returns exactly to its original position

#### Scenario: Drag delta does not drift (S41 regression)
- **WHEN** the tester drags an object in a fast circular motion for 5 seconds
- **THEN** the object stays under the cursor and does not drift away

#### Scenario: Re-paste does not duplicate references (S50 regression)
- **WHEN** the tester copies an object, pastes it, selects the original, and deletes it
- **THEN** the pasted copy remains intact and was not affected by the original's deletion

#### Scenario: Ctrl+V does not activate Select tool (S50 regression)
- **WHEN** the tester copies an object and presses Ctrl+V
- **THEN** the object is pasted and the tool remains Select (does not switch to another tool)

### Requirement: Heavy load and stress tests
The regression test suite SHALL verify the application handles 50+ objects, batch operations (select all → delete → undo), and rapid sequence of diverse operations without performance degradation or crashes.

#### Scenario: Batch delete and undo
- **WHEN** the tester creates 50 objects, presses Ctrl+A, Delete, then Ctrl+Z
- **THEN** all 50 objects are deleted, then all 50 are restored in a single undo action

### Requirement: Font rendering verification
The regression test suite SHALL verify that GOST fonts render correctly with their exact internal names (GOST Type AU, GOST Type BU), that font changes via Properties Panel update the canvas, and that fonts survive save/load round-trip.

#### Scenario: GOST A font rendering
- **WHEN** the tester creates a text object with FontName "ГОСТ А"
- **THEN** the text renders in GOST Type AU font face (verified visually)
