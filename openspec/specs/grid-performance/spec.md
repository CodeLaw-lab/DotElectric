# grid-performance Specification

## Purpose
Requirements for grid rendering performance: adaptive step selection, zero-allocation node buffer, and debounced rendering during continuous input.

## Requirements

### Requirement: Grid visible at all zoom levels on all sheet formats

The system SHALL display grid dots on any sheet format (A0–A4, half-formats) at any zoom level where pixel spacing ≥ 5px, regardless of the number of theoretical grid nodes. If the configured base step would produce more than 100,000 nodes for the full sheet, the system SHALL automatically select a coarser step (from the sequence ×1, ×2, ×5, ×10, ×20, ×50, ×100, ×200, ×500) that keeps the node count under the limit.

#### Scenario: A0 sheet at 0.5× zoom with 1mm base step

- **WHEN** user opens an A0 template with grid step set to 1mm and zoom = 0.5×
- **THEN** grid dots are displayed with a coarser step (e.g. 20mm or 50mm) such that the number of nodes does not exceed 100,000
- **AND** the displayed step is a multiple of the base step (1mm)

#### Scenario: A4 sheet at 1× zoom with 1mm step

- **WHEN** user opens an A4 template with grid step 1mm and zoom = 1×
- **THEN** grid dots are displayed at 5mm step (next step meeting pixel spacing ≥ 5px)

#### Scenario: Zoom in from overview to detail on A0

- **WHEN** user zooms in from ×0.5 (20mm step) to ×5 (1mm step) on an A0 sheet
- **THEN** the grid step smoothly transitions to finer levels at each threshold where pixel spacing ≥ 5px and node count ≤ 100,000

### Requirement: Zero heap allocation on grid refresh

The grid system SHALL NOT allocate managed heap objects during a grid refresh cycle after initial buffer allocation. Node coordinates SHALL be stored in a pre-allocated `long[]` buffer. The buffer SHALL be allocated once and reused for all subsequent refresh calls. The intermediate node list SHALL also be reused via an optional `reuseList` parameter.

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
