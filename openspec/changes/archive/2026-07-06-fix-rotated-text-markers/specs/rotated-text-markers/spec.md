## ADDED Requirements

### Requirement: Rotated text markers match visual corners
The system SHALL display selection markers at the visual corners of rotated text objects, matching the rendered text position for all rotation angles (0-359°).

#### Scenario: Markers at correct position for 45° rotation
- **WHEN** a Text object with RotationAngle=45° is selected
- **THEN** all four selection markers (RotatedCorner0-3) SHALL appear exactly at the four visual corners of the rendered rotated text

#### Scenario: Markers at correct position for 90° rotation
- **WHEN** a Text object with RotationAngle=90° is selected
- **THEN** all four selection markers SHALL appear exactly at the four visual corners of the rendered rotated text

#### Scenario: Markers at correct position for 0° rotation
- **WHEN** a Text object with RotationAngle=0° is selected
- **THEN** all four selection markers SHALL appear exactly at the four visual corners of the rendered text (axis-aligned)

#### Scenario: Markers at correct position for 270° rotation
- **WHEN** a Text object with RotationAngle=270° is selected
- **THEN** all four selection markers SHALL appear exactly at the four visual corners of the rendered rotated text

### Requirement: InlineTextEditor rotation matches text rotation
The inline text editor (TextBox) SHALL use the same rotation origin and direction as the TextBlock, so the editing experience matches the display.

#### Scenario: Inline editor rotation matches displayed text
- **WHEN** a Text object with RotationAngle=45° is double-clicked to enter inline edit mode
- **THEN** the TextBox SHALL be rotated around the same pivot point and in the same direction as the displayed TextBlock

### Requirement: Hit-test handles match visual markers
The resize handle hit-test positions (HitTestHelper.GetTextHandle) SHALL match the visual marker positions for all rotation angles.

#### Scenario: Handle hit-test matches marker at 45°
- **WHEN** the user clicks within HandleHitToleranceMicrons of a visual marker on a Text object with RotationAngle=45°
- **THEN** GetHitHandle SHALL return the corresponding ResizeHandle

#### Scenario: Handle hit-test matches marker at 90°
- **WHEN** the user clicks within HandleHitToleranceMicrons of a visual marker on a Text object with RotationAngle=90°
- **THEN** GetHitHandle SHALL return the corresponding ResizeHandle
