# Rotated Resize Cursor

## Purpose

The resize cursor displayed when hovering over a text selection marker must indicate the correct diagonal direction of the corner, accounting for text rotation. Without this fix, at 90° rotation the `TopRight` handle (visual bottom-left) shows `SizeNESW` instead of `SizeNWSE`, misleading the user about the drag direction.

## Requirements

### Requirement: Resize cursor matches visual corner direction

The system SHALL display a resize cursor whose diagonal direction (NWSE, NESW, NS, WE) corresponds to the visual position of the handle after applying text rotation, not the unrotated semantic handle name.

#### Scenario: Cursor at 0° rotation matches unrotated semantics
- **WHEN** the user hovers over a text selection marker at RotationAngle=0°
- **THEN** the cursor SHALL be `SizeNWSE` for `TopLeft` and `BottomRight` handles, and `SizeNESW` for `TopRight` and `BottomLeft` handles (unchanged from current behavior)

#### Scenario: Cursor at 90° rotation
- **WHEN** the user hovers over a `TopRight` handle on a Text object with RotationAngle=90°
- **THEN** the cursor SHALL be `SizeNWSE` (not `SizeNESW`), because the `TopRight` handle is visually at the bottom-left corner after 90° rotation

#### Scenario: Cursor at 90° rotation for BottomLeft handle
- **WHEN** the user hovers over a `BottomLeft` handle on a Text object with RotationAngle=90°
- **THEN** the cursor SHALL be `SizeNWSE` (not `SizeNESW`), because the `BottomLeft` handle is visually at the top-right corner after 90° rotation

#### Scenario: Cursor at 270° rotation
- **WHEN** the user hovers over a `TopRight` handle on a Text object with RotationAngle=270°
- **THEN** the cursor SHALL be `SizeNWSE`, because the `TopRight` handle is visually at the bottom-left corner after 270° rotation

#### Scenario: Cursor at 180° rotation matches unrotated
- **WHEN** the user hovers over a text selection marker at RotationAngle=180°
- **THEN** the cursor SHALL be the same as at 0° rotation (SizeNWSE for diagonal pairs), because 180° rotation preserves the visual corner order

#### Scenario: Cursor at 45° rotation
- **WHEN** the user hovers over a `TopRight` handle on a Text object with RotationAngle=45°
- **THEN** the cursor SHALL be `SizeNWSE`, because the `TopRight` handle is visually on the right side of the text after 45° CW rotation (equivalent to a bottom-right visual corner)
