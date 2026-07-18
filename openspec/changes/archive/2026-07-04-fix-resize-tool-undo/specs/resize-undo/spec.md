## ADDED Requirements

### Requirement: ResizeTool Undo restores original dimensions

The ResizeTool MUST support Undo/Redo that correctly toggles between the pre-resize and post-resize state of the object.

#### Scenario: Undo after rectangle resize restores original position and size

- **WHEN** user resizes a Rectangle object using ResizeTool
- **AND** user presses Ctrl+Z
- **THEN** the Rectangle returns to its MicronsX, MicronsY, WidthMicrons, HeightMicrons values before the resize started

#### Scenario: Undo after line resize restores original endpoints

- **WHEN** user resizes a Line object using ResizeTool
- **AND** user presses Ctrl+Z
- **THEN** the Line returns to its StartMicronsX, StartMicronsY, EndMicronsX, EndMicronsY values before the resize started

#### Scenario: Undo after text resize restores original position and font size

- **WHEN** user resizes a Text object using ResizeTool
- **AND** user presses Ctrl+Z
- **THEN** the Text returns to its MicronsX, MicronsY, FontSizeMicrons values before the resize started

#### Scenario: Redo applies the resize again

- **WHEN** user resizes any object
- **AND** user presses Ctrl+Z to undo
- **AND** user presses Ctrl+Y or Ctrl+Shift+Z to redo
- **THEN** the object returns to the resized dimensions

#### Scenario: NullReferenceException MUST NOT occur on Undo after resize

- **WHEN** user resizes any object using ResizeTool
- **AND** user presses Ctrl+Z
- **THEN** no exception is thrown
- **AND** the object state is correctly restored

### Requirement: ResizeTool Undo preserves separate old/new state snapshots

The ResizeTool MUST capture the pre-resize state at OnMouseDown and the post-resize state at OnMouseUp, and pass them as distinct oldValue/newValue to ChangePropertyCommand.

#### Scenario: initial state captured at OnMouseDown

- **WHEN** ResizeTool.OnMouseDown is called
- **THEN** the current ResizeState of the object is saved as the undo baseline

#### Scenario: final state captured at OnMouseUp

- **WHEN** ResizeTool.OnMouseUp is called
- **THEN** the current ResizeState of the object is captured as the new (redo) value
- **AND** it is different from the initial state captured at OnMouseDown (unless no resize occurred)

### Requirement: Lambda does not capture mutable field

The setter lambda passed to ChangePropertyCommand MUST capture a local variable, not the mutable `_resizedObject` field, to prevent NullReferenceException when `_resizedObject` is later set to null.

#### Scenario: lambda uses local captured reference

- **WHEN** ResizeTool.OnMouseUp creates ChangePropertyCommand
- **THEN** the setter lambda captures a local copy of `_resizedObject` reference
- **AND** setting `_resizedObject = null` after Push does not affect the lambda
- **AND** calling Undo on the command does not throw NullReferenceException
