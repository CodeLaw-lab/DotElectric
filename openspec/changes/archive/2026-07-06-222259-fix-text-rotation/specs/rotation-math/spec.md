## ADDED Requirements

### Requirement: Selection markers at rotated text corners
Text selection markers (4 corner squares) SHALL appear at the actual visual corners of a rotated Text object for any RotationAngle.

#### Scenario: No rotation — markers at model corners
- **WHEN** Text has RotationAngle = 0
- **THEN** RotatedCorner0 is at (MicronsX, MicronsY + HeightMicrons),
  RotatedCorner1 at (MicronsX + WidthMicrons, MicronsY + HeightMicrons),
  RotatedCorner2 at (MicronsX, MicronsY),
  RotatedCorner3 at (MicronsX + WidthMicrons, MicronsY)

#### Scenario: 90° CW rotation — markers match WPF RotateTransform
- **WHEN** Text has RotationAngle = 90
- **THEN** RotatedCorner1 SHALL be at W pixels below the pivot (CW: right edge → downward),
  RotatedCorner2 SHALL be at H pixels to the left of the pivot

#### Scenario: 45° CW rotation — markers match WPF RotateTransform
- **WHEN** Text has RotationAngle = 45
- **THEN** RotatedCorner1-Y SHALL be (Y+H) - WidthMicrons * sin(45°),
  RotatedCorner2-X SHALL be X - HeightMicrons * sin(45°)

#### Scenario: 270° CW rotation — opposite of 90°
- **WHEN** Text has RotationAngle = 270
- **THEN** RotatedCorner1-Y SHALL be (Y+H) - WidthMicrons * sin(270),
  RotatedCorner2-X SHALL be X - HeightMicrons * sin(270)
  (sin270 = -1, so Corner1Y = Y+H+W — W pixels above the pivot)

### Requirement: Correctly compute axis-aligned bounding box
GetBoundingBox() SHALL return the AABB that encloses all 4 rotated corners using forward WPF RotateTransform matrix.

#### Scenario: 0° — equals model extents
- **WHEN** Text has RotationAngle = 0
- **THEN** GetBoundingBox() returns RectMicrons(X, Y, X+W, Y+H)

#### Scenario: 90° CW — width and height swapped
- **WHEN** Text has RotationAngle = 90
- **THEN** GetBoundingBox() width equals HeightMicrons, height equals WidthMicrons

### Requirement: Hit-test rotated text correctly
ContainsPoint() SHALL correctly determine whether a model-space point falls inside the rotated text rectangle using inverse WPF RotateTransform.

#### Scenario: Center of rotated text at 90° — hits
- **WHEN** point is the center of the rotated AABB at RotationAngle = 90
- **THEN** ContainsPoint() returns True

#### Scenario: Point outside rotated text at 90° — misses
- **WHEN** point is clearly outside the rotated AABB at RotationAngle = 90
- **THEN** ContainsPoint() returns False

#### Scenario: Center at 0° — hits
- **WHEN** point is the center of the unrotated text at RotationAngle = 0
- **THEN** ContainsPoint() returns True
