# zoom-display-grid Specification

## Purpose
Requirements for zoom percentage display in status bar, grid rendering at all zoom levels and pan positions, and toolbar ComboBox zoom input without binding errors.

## Requirements

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

The grid dots SHALL cover the entire sheet area, not just the visible viewport, even when the canvas is larger than the viewport (i.e., when scrollbars are visible after zooming in). Grid nodes are generated for the full sheet at all zoom levels, so panning to any area immediately shows grid dots without requiring a separate refresh.

Node count SHALL be bounded by MaxGridNodes (100,000) via adaptive step selection. If the configured grid step at the current zoom would produce more than 100,000 nodes, a coarser step SHALL be used automatically from the sequence ×1, ×2, ×5, ×10, ×20, ×50, ×100, ×200, ×500.

The adaptive step SHALL NOT cause any portion of the sheet to lose grid dots — at minimum the grid step SHALL increase uniformly, preserving full-sheet coverage.

#### Scenario: Grid visible on full sheet after zoom-in

- **WHEN** user zooms in past the threshold where scrollbars appear
- **THEN** grid dots are rendered across the entire sheet, not only in the visible viewport area

#### Scenario: Grid dots appear in newly panned area

- **WHEN** user pans the canvas (middle-mouse drag or space+left drag) after zooming in
- **THEN** grid dots are visible in the newly exposed area immediately, without requiring a zoom change or grid toggle

#### Scenario: Grid covers full sheet at high zoom where sheet exceeds viewport

- **WHEN** user sets zoom to 751% (7.51×) on A4 portrait sheet with 5mm grid step
- **THEN** grid dots are rendered across the full 210×297mm sheet
- **AND** panning to any area of the sheet shows grid dots immediately

#### Scenario: Large sheet at max zoom uses coarser step to stay within limit

- **WHEN** user sets zoom to 1000% (10×) on A0 sheet (841×1189mm) with 1mm grid step
- **THEN** the display step is automatically increased (e.g., to 5mm or coarser) to keep node count below 100,000
- **AND** grid dots still cover the entire sheet

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
