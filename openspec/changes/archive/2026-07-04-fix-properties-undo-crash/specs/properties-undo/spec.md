## ADDED Requirements

### Requirement: Properties panel Undo MUST NOT throw NullReferenceException

The properties panel ViewModels MUST support Undo after the selected object has been changed. The command setter lambda SHALL capture a local variable, not a mutable field.

#### Scenario: Undo after LineType change restores original value

- **WHEN** user changes LineType of a Line object via properties panel
- **AND** user selects a different object (line is deselected)
- **AND** user presses Ctrl+Z
- **THEN** the Line returns to its original LineType value
- **AND** no exception is thrown

#### Scenario: Undo after coordinate change restores original position

- **WHEN** user changes X/Y coordinates of any object via properties panel
- **AND** user selects a different object
- **AND** user presses Ctrl+Z
- **THEN** the object returns to its original coordinates
- **AND** no exception is thrown

#### Scenario: Undo after color change restores original color

- **WHEN** user changes StrokeColor, FillColor, or Foreground via properties panel
- **AND** user selects a different object
- **AND** user presses Ctrl+Z
- **THEN** the object returns to its original color value
- **AND** no exception is thrown

#### Scenario: Undo after text property change restores original value

- **WHEN** user changes Content, FontSize, FontName, Rotation, or other text property via properties panel
- **AND** user selects a different object
- **AND** user presses Ctrl+Z
- **THEN** the Text object returns to its original property value
- **AND** no exception is thrown

#### Scenario: Redo after undo reapplies the change

- **WHEN** user changes a property via properties panel
- **AND** user deselects and reselects the object
- **AND** user presses Ctrl+Z to undo
- **AND** user presses Ctrl+Y to redo
- **THEN** the property returns to the changed value
