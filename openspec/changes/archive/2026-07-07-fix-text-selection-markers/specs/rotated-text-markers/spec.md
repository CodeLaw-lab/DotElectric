# Rotated Text Markers

## ADDED Requirements

### Requirement: Resize cursor direction matches visual handle position

The system SHALL update the resize cursor direction for rotated text based on the visual corner position (after rotation), not the unrotated semantic handle name.

#### Scenario: Cursor at 90° for TopRight handle
- **WHEN** the user hovers over a `TopRight` selection marker on Text with RotationAngle=90°
- **THEN** the cursor SHALL be `SizeNWSE` (the `TopRight` handle is visually at the bottom-left corner after 90° rotation)

#### Scenario: Cursor at 0° unchanged
- **WHEN** the user hovers over a selection marker on Text with RotationAngle=0°
- **THEN** the cursor SHALL match the current behavior (SizeNWSE for TopLeft/BottomRight, SizeNESW for TopRight/BottomLeft)
