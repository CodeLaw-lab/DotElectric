## Purpose

Escape handling during inline text editing. When the user is editing text inline (double-click on a text object), Escape SHALL cancel the editing and close the editor. When no editing is active, Escape SHALL clear the selection.

## Requirements

### Requirement: Escape cancels inline text editing

When inline text editing is active (TextBox is visible and focused), pressing Escape SHALL cancel the editing and close the TextBox, discarding any uncommitted changes.

When inline text editing is NOT active, pressing Escape SHALL clear the current selection and reset tool state (existing behavior preserved).

#### Scenario: Escape cancels editing when TextBox is focused

- **WHEN** user double-clicks a text object to start inline editing
- **AND** the TextBox has focus
- **AND** user presses Escape
- **THEN** the TextBox is closed
- **AND** the original text content is unchanged (changes discarded)
- **AND** the tool state resets

#### Scenario: Escape clears selection when TextBox is not focused

- **WHEN** user has objects selected on the canvas
- **AND** no inline editing is active
- **AND** user presses Escape
- **THEN** the selection is cleared
- **AND** the tool state resets
- **AND** no TextBox appears
