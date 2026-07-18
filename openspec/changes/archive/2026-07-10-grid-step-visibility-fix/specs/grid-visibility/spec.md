## ADDED Requirements

### Requirement: Adaptive grid step selection
The system SHALL select a display step such that `Coordinate.ToMm(selectedStepMicrons) * zoom >= 0.5` (mm on screen), subject to node budget and format bounds. The selected step SHALL be one of the nice steps: `[50, 30, 20, 15, 10, 7, 5, 3, 2, 1.5, 1, 0.7, 0.5]` mm (or their micron equivalents).

#### Scenario: A0 at 100% zoom with 1mm user step
- **WHEN** sheet is A0 (1189×841mm), zoom is 1.0, user step is 1000 microns (1mm)
- **THEN** display step SHALL be 500 microns (0.5mm) — the result of `0.5 / 1.0 = 0.5mm`, snapped to the nice-step sequence, and checked against node budget (viewport at 1.5× margin: ~500×280mm, `1001×561 = 561K > 250K` → coarsen to next nice step: 0.7mm → `715×401 = 287K > 250K` → coarsen to 1mm → `501×281 = 141K ≤ 250K` — **1mm** is the final selection)

#### Scenario: A4 at 100% zoom with 1mm user step
- **WHEN** sheet is A4 (210×297mm), zoom is 1.0, user step is 1000 microns (1mm)
- **THEN** display step SHALL be 500 microns (0.5mm) — viewport at margin 1.5× covers the full sheet (`421×595 = 250K ≤ 250K`)

#### Scenario: A4 at 50% zoom with 1mm user step
- **WHEN** sheet is A4 (210×297mm), zoom is 0.5, user step is 1000 microns (1mm)
- **THEN** display step SHALL be 1000 microns (1mm) — `0.5 / 0.5 = 1.0mm`, snapped to 1mm, node check: `211×298 = 62K ≤ 250K`

#### Scenario: A0 at 10% zoom with 5mm user step
- **WHEN** sheet is A0, zoom is 0.1, user step is 5000 microns (5mm)
- **THEN** display step SHALL be 5000 microns (5mm) — `0.5 / 0.1 = 5.0mm`, 5mm is in the nice-step sequence, node check passes

#### Scenario: Maximum zoom with fine step
- **WHEN** zoom is 10.0 (1000%), user step is 500 microns (0.5mm)
- **THEN** display step SHALL be 500 microns (0.5mm) — `0.5 / 10.0 = 0.05mm`, snapped to 0.5mm (the finest step in the sequence, since `0.5 > 0.05` and it's the finest available)

### Requirement: Viewport-culled node generation
The system SHALL generate grid nodes only for the visible viewport region expanded by a margin factor of 1.5×, instead of the full sheet.

#### Scenario: A0 at 100% zoom — viewport is smaller than sheet
- **WHEN** sheet is A0 (1189×841mm), viewport is 1920×1080px (~500×280mm at zoom 1.0), margin is 1.5
- **THEN** generated nodes SHALL cover the region `(left-25%px, top-25%px)` to `(right+25%px, bottom+25%px)`, clamped to sheet bounds, NOT the full 1189×841mm sheet

#### Scenario: A4 at 50% zoom — viewport exceeds sheet
- **WHEN** sheet is A4 (210×297mm), viewport is 1920×1080px (~1000×560mm at zoom 0.5), margin is 1.5, expanded region exceeds sheet bounds
- **THEN** generated nodes SHALL cover only the sheet area (0..210mm × 0..297mm), clamped to sheet bounds

### Requirement: Pan-triggered grid regeneration
The system SHALL regenerate grid nodes when the visible viewport (expanded by margin) extends beyond the currently cached region.

#### Scenario: Pan within cached region — no regeneration
- **WHEN** user pans by 10% of viewport size (within the 25% margin buffer)
- **THEN** grid SHALL NOT regenerate — existing nodes are still valid

#### Scenario: Pan beyond cached region — regeneration triggered
- **WHEN** user pans by 30% of viewport size (beyond the 25% margin buffer)
- **THEN** grid SHALL regenerate with new viewport bounds, with a debounce of 50ms

#### Scenario: Pan end — always refresh
- **WHEN** user releases the mouse button after panning (MouseUp)
- **THEN** grid SHALL regenerate once to ensure the cached region is centered on the final viewport position

### Requirement: Step selection respects MaxGridNodes
The system SHALL NOT select a display step that would generate more than `MaxGridNodes` (250 000) nodes for the current viewport region.

#### Scenario: A4 at 100% zoom, 0.5mm step — at the limit
- **WHEN** sheet is A4 (210×297mm), zoom is 1.0, 0.5mm step is selected
- **THEN** `cols×rows = 421×595 = 250 495 ≤ 250 000` SHALL be **false** (exceeds budget) → the system SHALL coarsen to 0.7mm step or higher

*(Note: the actual calculation yields 250 495 which exceeds 250 000 by 495. Depending on viewport margin, the clamped region may be slightly smaller. If the margin-culled region is 297×210mm exactly, the system SHALL use 0.7mm instead of 0.5mm.)*

#### Scenario: Full-sheet generation prevented on large format
- **WHEN** sheet is A0, display step is 0.5mm, even viewport-culled region would produce > 250 000 nodes
- **THEN** system SHALL coarsen to a step that satisfies the node budget

### Requirement: Configurable constants
`MinPixelSpacing` and `MaxGridNodes` SHALL remain compile-time constants in `EditorSettings.cs`, configurable by developers but not exposed in the UI.

#### Scenario: Constant values verified
- **WHEN** inspecting `EditorSettings.MinPixelSpacing`
- **THEN** it SHALL equal `0.5`

#### Scenario: MaxGridNodes verified
- **WHEN** inspecting `EditorSettings.MaxGridNodes`
- **THEN** it SHALL equal `250000`
