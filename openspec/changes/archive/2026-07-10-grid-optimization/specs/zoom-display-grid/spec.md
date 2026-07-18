## MODIFIED Requirements

### Requirement: Grid covers full sheet after zoom-in with scrollbars

The grid dots SHALL cover the visible viewport area (plus a one-screen margin), not the entire sheet. Grid nodes are generated for the viewport at all zoom levels and recalculated automatically during pan/zoom. The recalculation SHALL complete within 10ms, and the visual render SHALL update within 16ms. The user SHALL see grid dots in any panned area within one frame of arriving there.

#### Scenario: Grid visible in viewport after zoom-in
- **WHEN** user zooms in past the threshold where scrollbars appear
- **THEN** grid dots are rendered in the visible viewport area
- **AND** the step is automatically coarsened if the node count exceeds 50,000

#### Scenario: Grid dots appear in newly panned area
- **WHEN** user pans the canvas (middle-mouse drag or space+left drag) after zooming in
- **THEN** grid dots are recalculated for the new viewport position and appear within 16ms (1 frame)
- **AND** no heap allocations occur during recalculation
