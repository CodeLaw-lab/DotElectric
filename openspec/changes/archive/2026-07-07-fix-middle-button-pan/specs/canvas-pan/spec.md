## ADDED Requirements

### Requirement: Middle-click panning works at all zoom levels

The system SHALL support middle-click drag panning regardless of whether the canvas fits within the viewport. CanvasOffsetX/Y SHALL always reflect the accumulated pan offset, allowing the canvas to be moved freely at any zoom level and viewport size.

#### Scenario: Pan at zoom=100% with IsCentered=true
- **WHEN** the user presses and holds the middle mouse button on a canvas where the sheet fits entirely within the viewport (IsCentered=true)
- **AND** moves the mouse
- **THEN** the canvas SHALL scroll in the direction of mouse movement, and ZoomPanManager.CanvasOffsetX/Y SHALL reflect the change

#### Scenario: Pan at zoom=400% with IsCentered=false
- **WHEN** the user presses and holds the middle mouse button on a canvas where the sheet is larger than the viewport (IsCentered=false)
- **AND** moves the mouse
- **THEN** the canvas SHALL scroll in the direction of mouse movement

#### Scenario: CenterCanvas resets position after pan with IsCentered=true
- **WHEN** the user pans the canvas at IsCentered=true
- **AND** CenterCanvas() is called (e.g., on zoom change)
- **THEN** PanOffsetX/Y SHALL be reset to 0, and the canvas SHALL return to its centered position

### Requirement: CanvasOffset always equals -PanOffset

The system SHALL set ZoomPanManager.CanvasOffsetX to -ZoomPanManager.PanOffsetX and ZoomPanManager.CanvasOffsetY to -ZoomPanManager.PanOffsetY at all times, without conditional guards based on viewport size.

#### Scenario: CanvasOffset at IsCentered=true after pan
- **WHEN** the canvas fits within the viewport (IsCentered=true)
- **AND** PanOffsetX is set to 50 (via PanCanvas or direct assignment)
- **THEN** CanvasOffsetX SHALL equal -50

#### Scenario: CanvasOffset at IsCentered=true before pan
- **WHEN** the canvas fits within the viewport (IsCentered=true)
- **AND** no panning has occurred
- **THEN** CanvasOffsetX SHALL equal 0 (because PanOffsetX is 0)
