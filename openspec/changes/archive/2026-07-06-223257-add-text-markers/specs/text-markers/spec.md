# text-markers Specification

## Purpose
Маркеры выделения для текстовых объектов на канвасе — визуальная индикация выбранного Text.

## ADDED Requirements

### Requirement: Text selection markers at 4 corners

When a Text object is selected, 4 square markers SHALL appear at the 4 corners of the text's visual bounding box. For rotated text, markers SHALL follow the rotation to the actual corner positions (not AABB corners).

#### Scenario: New text shows 4 square markers on selection

- **WHEN** user creates a new Text object via TextTool
- **AND** the object becomes selected (auto-select after creation)
- **THEN** 4 square markers (6×6 px, white fill, #0078D4 stroke) SHALL appear
- **AND** the markers SHALL be positioned at the 4 corners of the text bounding box

#### Scenario: Rotated text markers at actual corners

- **WHEN** a Text object at rotation angle 45° is selected
- **THEN** the 4 markers SHALL be at the rotated corner positions (`RotatedCorner0–3`)
- **AND** SHALL NOT be at the axis-aligned bounding box corners

#### Scenario: Multiple objects selected includes Text

- **WHEN** user selects a Line AND a Text object simultaneously
- **THEN** Line SHALL show its 2 circle markers
- **AND** Text SHALL show its 4 square markers
- **AND** both marker sets SHALL be visible simultaneously

### Requirement: Visual style matches Rectangle markers

Text selection markers SHALL use the same visual style as Rectangle markers: `SquareMarker` style (6×6, white fill, #0078D4 stroke, -3/-3 offset).

#### Scenario: Marker appearance

- **WHEN** a Text object is selected
- **THEN** each corner marker SHALL be a Rectangle with `Style="{StaticResource SquareMarker}"`
