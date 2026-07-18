# Rotated Text Markers

## Purpose

Selection markers for rotated text objects must appear at the correct visual corners. The inline text editor must use the same rotation parameters as the displayed text. This ensures a consistent editing experience for all rotation angles (0-359°).

## Requirements

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

### Requirement: Resize cursor direction matches visual handle position

The system SHALL update the resize cursor direction for rotated text based on the visual corner position (after rotation), not the unrotated semantic handle name.

#### Scenario: Cursor at 90° for TopRight handle
- **WHEN** the user hovers over a `TopRight` selection marker on Text with RotationAngle=90°
- **THEN** the cursor SHALL be `SizeNWSE` (the `TopRight` handle is visually at the bottom-left corner after 90° rotation)

#### Scenario: Cursor at 0° unchanged
- **WHEN** the user hovers over a selection marker on Text with RotationAngle=0°
- **THEN** the cursor SHALL match the current behavior (SizeNWSE for TopLeft/BottomRight, SizeNESW for TopRight/BottomLeft)
