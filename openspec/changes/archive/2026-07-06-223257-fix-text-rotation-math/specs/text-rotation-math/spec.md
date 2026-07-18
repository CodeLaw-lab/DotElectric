# text-rotation-math Specification

## Purpose
Define the mathematical correctness requirements for rotated text geometry — `RotatedCorner*` positions, `GetBoundingBox()`, and `ContainsPoint()` — ensuring selection markers, hit-testing, and bounding boxes match WPF's `RotateTransform` at every rotation angle.

## ADDED Requirements

### Requirement: RotatedCorner markers match WPF RotateTransform at all angles

The 4 `RotatedCorner*` properties SHALL return the correct model-space positions of the text bounding box corners after applying the equivalent of WPF `RotateTransform(θ)`, which rotates clockwise in Y-down screen space (counter-clockwise in Y-up model space).

The pivot point SHALL be the WPF top-left = model bottom-left = (`MicronsX`, `MicronsY` + `HeightMicrons`).

#### Scenario: RotatedCorner at 0° matches model dimensions

- **WHEN** `RotationAngle` = 0
- **THEN** `RotatedCorner0` = (`MicronsX`, `MicronsY` + `HeightMicrons`)
- **AND** `RotatedCorner1` = (`MicronsX` + `WidthMicrons`, `MicronsY` + `HeightMicrons`)
- **AND** `RotatedCorner2` = (`MicronsX`, `MicronsY`)
- **AND** `RotatedCorner3` = (`MicronsX` + `WidthMicrons`, `MicronsY`)

#### Scenario: RotatedCorner at 90° forms a right-angled box

- **WHEN** `RotationAngle` = 90 and width ≠ height
- **THEN** `RotatedCorner1X` SHALL equal `RotatedCorner0X` (same X as pivot, because cos(90)=0)
- **AND** `RotatedCorner1Y` SHALL be `RotatedCorner0Y` − `WidthMicrons` (W units above pivot in model Y-up, because sin(90)=1)
- **AND** `RotatedCorner2X` SHALL be `RotatedCorner0X` + `HeightMicrons` (H units right of pivot, because sin(90)=1)
- **AND** `RotatedCorner2Y` SHALL equal `RotatedCorner0Y` (same Y as pivot, because cos(90)=0)
- **AND** the 4 corners SHALL form a rectangle of width H * height W

#### Scenario: RotatedCorner at 45° forms uniform diamond

- **WHEN** `RotationAngle` = 45 and `WidthMicrons` = `HeightMicrons`
- **THEN** the 4 corners SHALL form a square rotated 45° relative to the model axes
- **AND** each corner SHALL be equidistant from the center point

#### Scenario: RotatedCorner at 180° is mirror of 0°

- **WHEN** `RotationAngle` = 180
- **THEN** `RotatedCorner0X` SHALL equal `RotatedCorner0X` at 0° (pivot unchanged)
- **AND** `RotatedCorner0Y` SHALL equal `RotatedCorner0Y` at 0°
- **AND** `RotatedCorner1` = `RotatedCorner2` at 0°
- **AND** `RotatedCorner2` = `RotatedCorner1` at 0°
- **AND** `RotatedCorner3` = `RotatedCorner0` at 0°

#### Scenario: RotatedCorner at 270° is opposite of 90°

- **WHEN** `RotationAngle` = 270
- **THEN** `RotatedCorner1X` SHALL be `RotatedCorner0X` − `WidthMicrons`
- **AND** `RotatedCorner1Y` SHALL be `RotatedCorner0Y`
- **AND** `RotatedCorner2X` SHALL be `RotatedCorner0X`
- **AND** `RotatedCorner2Y` SHALL be `RotatedCorner0Y` + `HeightMicrons`

### Requirement: GetBoundingBox returns correct axis-aligned extents

`GetBoundingBox()` SHALL return the minimal axis-aligned `RectMicrons` that encloses all 4 rotated corners. This bounding box SHALL match the extents computed from WPF `RotateTransform` at every angle.

#### Scenario: GetBoundingBox at 0° equals model extents

- **WHEN** `RotationAngle` = 0
- **THEN** `GetBoundingBox()` SHALL return `(MicronsX, MicronsY, MicronsX + WidthMicrons, MicronsY + HeightMicrons)`

#### Scenario: GetBoundingBox at 90° swaps width and height

- **WHEN** `RotationAngle` = 90
- **THEN** `GetBoundingBox().Width` SHALL equal `HeightMicrons`
- **AND** `GetBoundingBox().Height` SHALL equal `WidthMicrons`
- **AND** `GetBoundingBox().Left` SHALL be `MicronsX`
- **AND** `GetBoundingBox().Top` SHALL be `MicronsY`

#### Scenario: GetBoundingBox at 45° is larger than model extents

- **WHEN** `RotationAngle` = 45
- **THEN** `GetBoundingBox().Width` SHALL be greater than both `WidthMicrons` and `HeightMicrons`
- **AND** `GetBoundingBox().Height` SHALL equal `GetBoundingBox().Width`

### Requirement: ContainsPoint correctly detects hits on rotated text

`ContainsPoint()` SHALL return `true` when a model-space point falls within the rotated text bounding box (in the text's local coordinate system after inverse rotation), and `false` otherwise.

#### Scenario: ContainsPoint at 90° hits within rotated area

- **WHEN** a `Text` object is at `(10000, 10000)` with `WidthMicrons = 20000`, `HeightMicrons = 10000`, `RotationAngle = 90`
- **AND** the test point is at the center of the visual rotated area (e.g., above the original position)
- **THEN** `ContainsPoint` SHALL return `true`

#### Scenario: ContainsPoint at 90° misses outside rotated area

- **WHEN** the same rotated text object at 90°
- **AND** the test point is at the original unrotated bounding box center
- **THEN** `ContainsPoint` SHALL return `false`

### Requirement: HeightMicrons accounts for font ascent/descent

`HeightMicrons` SHALL compute the visual height of the text using `FontMetrics.GetHeightRatio(fontName)`, accounting for the font's ascender and descender. The raw `FontSizeMicrons` alone is insufficient.

#### Scenario: HeightMicrons is greater than FontSizeMicrons

- **WHEN** `FontSizeMicrons = 10000` and font is GOST A or B
- **THEN** `HeightMicrons` SHALL be greater than `10000`
- **AND** `HeightMicrons / FontSizeMicrons` SHALL equal `FontMetrics.GetHeightRatio(fontName)`

### Requirement: WidthMicrons uses font advance width ratio

`WidthMicrons` SHALL compute the text width using `FontMetrics.GetAdvWidthRatio(fontName)`, derived from the font's actual glyph advance widths, replacing the current hardcoded per-font factors.

#### Scenario: WidthMicrons scales linearly with font size

- **WHEN** `Content = "TEST"` and `FontSizeMicrons` doubles
- **THEN** `WidthMicrons` SHALL also double (within rounding tolerance)

### Requirement: Dead code TextSelectionMarkerBehavior removed

`TextSelectionMarkerBehavior.cs` SHALL be deleted entirely. The `behaviors:TextSelectionMarkerBehavior.IsEnabled="True"` attached property SHALL be removed from `EditorCanvas.xaml`. All text marker positioning SHALL be handled by the `ItemsControl`-based markers using `RotatedCorner*` properties.

#### Scenario: Text selection markers render without behavior

- **WHEN** a text object is selected and rotated
- **THEN** the 4 corner markers SHALL render at correct rotated positions
- **AND** no `TextSelectionMarkerManager` SHALL be instantiated

### Requirement: Visual properties (VisualLeft/Right/Top/Bottom) match bounding box

`VisualLeft`, `VisualRight`, `VisualTop`, `VisualBottom` SHALL derive from `GetBoundingBox()` and SHALL return correct axis-aligned extents of the rotated text, suitable for AABB-based operations.

#### Scenario: Visual properties at 0° match simple extents

- **WHEN** `RotationAngle` = 0
- **THEN** `VisualLeft = MicronsX`, `VisualRight = MicronsX + WidthMicrons`, `VisualBottom = MicronsY + HeightMicrons`, `VisualTop = MicronsY`

#### Scenario: Visual properties at 90° reflect rotated extents

- **WHEN** `RotationAngle` = 90
- **THEN** `VisualRight - VisualLeft` SHALL equal `GetBoundingBox().Width` (approximately `HeightMicrons`)
