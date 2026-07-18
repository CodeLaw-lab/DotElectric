## ADDED Requirements

### Requirement: Text rotation markers match WPF visual position
The model SHALL use clockwise (CW) rotation convention in Y-down local space for all text geometry computation, matching WPF's RotateTransform behavior. The selection markers at the four text corners SHALL appear at the same model coordinates as the corresponding corners of the WPF-rendered text.

#### Scenario: Marker at 90° aligns with visual corner
- **WHEN** a text object is rotated 90°
- **THEN** the RotatedCorner1 marker SHALL appear at the model position corresponding to the WPF text's visually rotated top-right corner

#### Scenario: Marker at 270° aligns with visual corner
- **WHEN** a text object is rotated 270°
- **THEN** the RotatedCorner1 marker SHALL appear at the model position corresponding to the WPF text's visually rotated top-right corner

#### Scenario: Hit-test succeeds at non-90° angles
- **WHEN** a user clicks on the rendered text at any rotation angle
- **THEN** ContainsPoint SHALL return true for that click point

#### Scenario: All four markers hittable at arbitrary angle
- **WHEN** a text object has a non-orthogonal rotation (e.g., 45°)
- **THEN** GetHitHandle SHALL return the correct ResizeHandle for each of the four corners
