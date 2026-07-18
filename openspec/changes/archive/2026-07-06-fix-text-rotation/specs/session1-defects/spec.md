# session1-defects — Delta for fix-text-rotation

This delta documents that the existing requirement *Text selection markers follow rotation* is already correctly specified — only the implementation formulas are wrong.

## MODIFIED Requirements

### Requirement: Text selection markers follow rotation

When a Text object is rotated, the 4 selection markers (corner handles) SHALL be positioned at the actual rotated corners of the text bounding box, not at the axis-aligned bounding box (AABB) corners.

The 4 corner formulas in `Text.cs` SHALL use the correct rotation math so that markers match WPF's `RotateTransform` direction (Y-down clockwise = standard matrix `x' = x·cosθ - y·sinθ, y' = x·sinθ + y·cosθ` applied after Y-flip).

#### Scenario: Rotated text markers at 45 degrees

- **WHEN** a Text object is rotated to 45 degrees
- **THEN** the 4 square markers SHALL be positioned at the 4 actual corners of the rotated text
- **AND** SHALL NOT visually lag or lead the text corners in either direction

#### Scenario: Rotated text markers at 90 degrees

- **WHEN** a Text object is rotated to 90 degrees
- **THEN** the top-right marker SHALL be at the model position representing WPF-coordinate `(X + W, Y)` (below the origin, not above)
- **AND** the bottom-left marker SHALL be at `(X, Y + H)` (to the left of origin, not to the right)
