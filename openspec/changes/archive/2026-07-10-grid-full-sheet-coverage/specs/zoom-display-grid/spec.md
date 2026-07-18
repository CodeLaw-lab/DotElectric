# zoom-display-grid Specification (Delta)

## MODIFIED Requirements

### Requirement: Grid covers full sheet after zoom-in with scrollbars

The grid dots SHALL cover the entire sheet area, not just the visible viewport, even when the canvas is larger than the viewport (i.e., when scrollbars are visible after zooming in). Grid nodes are generated for the full sheet at all zoom levels, so panning to any area immediately shows grid dots without requiring a separate refresh.

Node count SHALL be bounded by MaxGridNodes (100,000) via adaptive step selection. If the grid step at the current zoom would produce more than 100,000 nodes, a coarser step SHALL be used automatically.

The adaptive step SHALL NOT cause any portion of the sheet to lose grid dots — at minimum the grid step SHALL increase uniformly, preserving coverage.

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
