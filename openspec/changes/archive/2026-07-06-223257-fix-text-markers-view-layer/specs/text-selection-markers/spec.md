## ADDED Requirements

### Requirement: Text selection markers align with rendered text bounds
When a Text object is selected, 4 corner markers SHALL appear at the visual corners of the actually rendered text, NOT at model-dimension-predicted positions. This applies at all rotation angles (0°, 45°, 90°, 135°, 180°, 270°, 315°, arbitrary).

#### Scenario: Markers at correct positions at 0°
- **WHEN** a Text object with rotation 0° is selected
- **THEN** the 4 markers coincide with the visual corners of the rendered text

#### Scenario: Markers at correct positions at 90°
- **WHEN** a Text object with rotation 90° is selected
- **THEN** the 4 markers coincide with the visual corners of the rotated rendered text

#### Scenario: Markers at correct positions at 45°
- **WHEN** a Text object with rotation 45° is selected
- **THEN** the 4 markers coincide with the visual corners of the rotated rendered text

#### Scenario: Markers update on content change
- **WHEN** text content changes (e.g., via inline editor or property panel)
- **THEN** markers reposition to match the new text extents

#### Scenario: Markers update on font size change
- **WHEN** FontSizeMicrons changes
- **THEN** markers reposition to match the new text extents

#### Scenario: Markers update on rotation change
- **WHEN** RotationAngle changes
- **THEN** markers reposition to match the new rotated text extents

### Requirement: Overlay Canvas manages markers in DataTemplate
The Text DataTemplate SHALL wrap TextBlock and a Canvas in a Grid. The Canvas (IsHitTestVisible=False) hosts the 4 Rectangle markers created by the behavior.

#### Scenario: Overlay Canvas exists in DataTemplate
- **WHEN** a Text object is rendered
- **THEN** the visual tree contains a Grid with both TextBlock and an overlay Canvas as children

### Requirement: Background highlight removed from selection DataTrigger
The TextBlock SHALL NOT have a Background setter (#E0F0FF) in its selection DataTrigger. Selection state is indicated by Foreground change (#0078D4, Bold) and the 4 corner markers.

#### Scenario: No background fill on selected text
- **WHEN** a Text object is selected
- **THEN** the TextBlock Background remains transparent (no #E0F0FF fill)

### Requirement: Marker appearance matches existing style
The 4 markers SHALL use the same `SquareMarker` style as Line and Rectangle markers, for visual consistency.

#### Scenario: Markers use SquareMarker style
- **WHEN** markers are created
- **THEN** each marker Rectangle uses the `SquareMarker` StaticResource style

### Requirement: Markers are axis-aligned (not rotated)
Markers SHALL remain axis-aligned squares even when the text is rotated. They do NOT rotate with the text.

#### Scenario: Markers axis-aligned at 45°
- **WHEN** a Text object with rotation 45° is selected
- **THEN** each of the 4 marker squares is axis-aligned (rectangles parallel to screen edges)

### Requirement: Markers hide when text is deselected
When a Text object is not selected, markers SHALL be hidden (no visual markers).

#### Scenario: Markers hidden on deselection
- **WHEN** a Text object is deselected
- **THEN** the 4 markers are removed or hidden
- **THEN** no marker elements exist for that text in the visual tree

### Requirement: Compatible with InlineTextEditor
Markers SHALL NOT interfere with double-click inline editing. During inline editing, markers may be hidden.

#### Scenario: Markers hidden during inline editing
- **WHEN** inline editing is active on a text object
- **THEN** markers are hidden until editing completes
