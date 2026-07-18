# zoom-display-grid Specification

## Purpose
Requirements for zoom percentage display in status bar, grid rendering at all zoom levels and pan positions, and toolbar ComboBox zoom input without binding errors.

## ADDED Requirements

### Requirement: Status bar zoom percent updates on any zoom change

The status bar SHALL display the current zoom percentage, and this value SHALL update in real time regardless of how the zoom was changed (mouse wheel, toolbar buttons, ComboBox dropdown, keyboard shortcuts, FitToScreen).

#### Scenario: Mouse wheel zoom updates status bar

- **WHEN** user scrolls the mouse wheel on the canvas to zoom in or out
- **THEN** the zoom percentage shown in the status bar updates to match the new zoom level within 100 ms

#### Scenario: Toolbar zoom button updates status bar

- **WHEN** user clicks the zoom-in or zoom-out toolbar button
- **THEN** the status bar zoom percentage updates to match the new zoom level

#### Scenario: ComboBox zoom selection updates status bar

- **WHEN** user selects a zoom value from the toolbar ComboBox
- **THEN** the status bar zoom percentage updates to match the selected zoom level

### Requirement: Grid covers full sheet after zoom-in with scrollbars

The grid dots SHALL cover the entire sheet area, not just the visible viewport, even when the canvas is larger than the viewport (i.e., when scrollbars are visible after zooming in). The grid SHALL refresh when the user pans to display dots in newly visible areas.

#### Scenario: Grid visible on full sheet after zoom-in

- **WHEN** user zooms in past the threshold where scrollbars appear
- **THEN** grid dots are rendered across the entire sheet, not only in the visible viewport area

#### Scenario: Grid dots appear when panning to new area

- **WHEN** user pans the canvas (middle-mouse drag or space+left drag) after zooming in
- **THEN** grid dots appear in the newly visible area without requiring a zoom change

### Requirement: Zoom ComboBox accepts values without binding errors

The toolbar zoom ComboBox SHALL accept zoom values typed or selected by the user without producing WPF binding conversion errors. Selecting "150%" from the dropdown or typing "150" SHALL change the zoom to 150% without any error messages.

#### Scenario: Select zoom from dropdown without error

- **WHEN** user opens the zoom ComboBox and selects "150%"
- **THEN** the zoom changes to 150%
- **AND** no binding conversion error is produced

#### Scenario: Type zoom value without error

- **WHEN** user types "150" into the editable zoom ComboBox and presses Enter
- **THEN** the zoom changes to 150%
- **AND** no binding conversion error is produced

### Requirement: Grid refresh on pan does not degrade performance

When the grid refreshes during panning, the operation SHALL complete in under 50 ms for sheets up to A0 size (2378 x 841 mm) with grid step as small as 5 mm, to avoid visible stutter during pan.

#### Scenario: Panning over A0 sheet with 5 mm grid

- **WHEN** user pans across an A0 sheet with 5 mm grid step
- **THEN** grid nodes for the newly visible area are generated and rendered in under 50 ms
- **AND** the panning motion remains smooth (no dropped frames perceptible to the user)
