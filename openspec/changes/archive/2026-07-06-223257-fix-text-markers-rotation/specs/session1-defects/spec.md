# session1-defects Specification — Delta

## MODIFIED Requirements

### Requirement: Text selection markers follow rotation

When a Text object is rotated, the 4 selection markers (corner handles) SHALL be positioned at the actual rotated corners of the text bounding box, not at the axis-aligned bounding box (AABB) corners and not at positions computed solely from `FontSizeMicrons` as the text height.

The model's `HeightMicrons` SHALL account for the font's actual ascent + descent + line gap by multiplying `FontSizeMicrons` by the font's height ratio from `FontMetrics`. The model's `WidthMicrons` SHALL account for the font's actual average glyph advance width.

#### Scenario: Rotated text markers at 90 degrees

- **WHEN** a Text object with content "Test" at font size 3500µ is rotated to 90 degrees
- **THEN** the 4 square markers SHALL be positioned at the actual corners of the rendered text
- **AND** the markers SHALL NOT appear visually offset to the left of the text

#### Scenario: Rotated text markers at 180 degrees

- **WHEN** a Text object is rotated to 180 degrees
- **THEN** the 4 square markers SHALL be positioned at the actual corners of the rendered text
- **AND** the markers SHALL NOT appear visually above or to the left of the text

#### Scenario: Rotated text markers at 270 degrees

- **WHEN** a Text object is rotated to 270 degrees
- **THEN** the 4 square markers SHALL be positioned at the actual corners of the rendered text
- **AND** the markers SHALL NOT appear visually above the text

### Requirement: Handle hit-test on rotated text uses corrected geometry

`HitTestHelper.GetTextHandle()` SHALL detect resize handle clicks on rotated Text objects using the corrected `HeightMicrons` and `WidthMicrons` values that account for font metrics.

#### Scenario: Click on rotated text corner at 90 degrees activates resize

- **WHEN** user clicks near a corner of a Text object rotated 90 degrees
- **THEN** the correct resize handle SHALL be detected
- **AND** the handle position SHALL match the visual corner of the rotated text

### Requirement: Text resize accounts for corrected bounding box

`ResizeMath.ComputeTextResize()` and the resize workflow SHALL continue to work correctly with the corrected `HeightMicrons` and `WidthMicrons`. The `ResizeState` SHALL continue capturing `FontSizeMicrons` as the `Height` field (unchanged).

#### Scenario: Resize rotated text after font metrics correction

- **WHEN** user drags a corner handle of a rotated Text object
- **THEN** the font size SHALL scale correctly
- **AND** the text position SHALL not jump unexpectedly
