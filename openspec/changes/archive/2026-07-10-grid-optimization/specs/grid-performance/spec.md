## ADDED Requirements

### Requirement: Grid visible at all zoom levels on all sheet formats

The system SHALL display grid dots on any sheet format (A0–A4, half-formats) at any zoom level where pixel spacing ≥ 5px, regardless of the number of theoretical grid nodes. If the configured base step would produce more than 50,000 nodes in the visible viewport, the system SHALL automatically select a coarser step (from the sequence ×1, ×2, ×5, ×10, ×20, ×50, ×100, ×200, ×500) that keeps the node count under the limit.

#### Scenario: A0 sheet at 0.5× zoom with 1mm base step
- **WHEN** user opens an A0 template with grid step set to 1mm and zoom = 0.5×
- **THEN** grid dots are displayed with a coarser step (e.g. 20mm or 50mm) such that the number of visible nodes does not exceed 50,000
- **AND** the displayed step is a multiple of the base step (1mm)

#### Scenario: A4 sheet at 1× zoom with 1mm step
- **WHEN** user opens an A4 template with grid step 1mm and zoom = 1×
- **THEN** grid dots are displayed at 1mm step (no coarsening needed)

#### Scenario: Zoom in from overview to detail on A0
- **WHEN** user zooms in from ×0.5 (20mm step) to ×5 (1mm step) on an A0 sheet
- **THEN** the grid step smoothly transitions to finer levels at each threshold where pixel spacing ≥ 5px and node count ≤ 50,000

### Requirement: Viewport-constrained grid generation

The system SHALL generate grid nodes only for the visible viewport area (plus a margin of one screen width/height in each direction), NOT for the entire sheet. When the user pans to a new area, the grid SHALL be recalculated for the new viewport position within 10ms. Previously generated nodes outside the viewport SHALL be discarded.

#### Scenario: Pan after zoom-in reveals grid in new area
- **WHEN** user zooms in on an A0 sheet until scrollbars appear and pans to a previously hidden area
- **THEN** grid dots appear in the newly visible area within 16ms (1 frame at 60fps)

#### Scenario: Grid nodes only in viewport
- **WHEN** system generates grid nodes at zoom ×5 on an A0 sheet
- **THEN** the number of generated nodes corresponds to the viewport area ×2 margin, NOT the full sheet area
- **AND** no heap-allocations occur during node generation

### Requirement: Zero heap allocation on grid refresh

The grid system SHALL NOT allocate managed heap objects during a grid refresh cycle after initial buffer allocation. Node coordinates SHALL be stored in a pre-allocated `long[]` buffer. The buffer SHALL be allocated once and reused for all subsequent refresh calls.

#### Scenario: Node buffer reuse on zoom change
- **WHEN** user changes zoom level multiple times
- **THEN** the grid buffer is reused without new allocations
- **AND** Gen-0 collections do not occur due to grid refresh

### Requirement: Grid render debounced during continuous input

When the user pans or zooms continuously (e.g., mouse drag or scroll wheel), the visual grid render SHALL be debounced to no more than once per 60fps frame (≈16ms). The node position calculation SHALL still occur on every input event, but `InvalidateVisual` SHALL be deferred.

#### Scenario: Smooth pan with debounced grid
- **WHEN** user pans the canvas with a middle-mouse drag at normal speed
- **THEN** the grid dots update smoothly at display refresh rate
- **AND** CPU usage for grid rendering does not exceed one full render per frame
