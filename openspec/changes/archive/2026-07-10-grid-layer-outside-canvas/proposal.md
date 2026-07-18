## Why

GridNodesLayer is inside DrawingCanvas, which has `ClipToBounds=True` and `Width/Height = sheet×zoom`. This causes grid dots near the sheet edge to be clipped by the canvas boundary. Additionally, the canvas's clip region can create visual artifacts at certain zoom levels where grid dots are cut off at the sheet edge instead of being fully visible.

Moving GridNodesLayer outside DrawingCanvas (as a sibling) decouples grid rendering from canvas clipping — no behavioral changes, pure architectural improvement.

## What Changes

- Move GridNodesLayer from inside DrawingCanvas to the parent Grid (sibling of DrawingCanvas)
- Add `TranslateTransform` on GridNodesLayer to sync pan with DrawingCanvas
- Add `UIElement.Clip` on GridNodesLayer (RectangleGeometry matching sheet bounds) to preserve sheet-only rendering
- Remove `Width/Height` binding from GridNodesLayer (no longer sized by sheet×zoom — sized by viewport or unconstrained)
- No changes to GridManager, GridHelper, GridNodesLayer.OnRender, or any test
- No behavioral changes visible to the user

## Capabilities

### New Capabilities

*(none — internal restructuring only)*

### Modified Capabilities

*(none — no requirement changes)*

## Impact

- **EditorCanvas.xaml**: GridNodesLayer moves from inside Canvas to parent Grid; receives its own RenderTransform binding
- **GridNodesLayer.cs**: Add `SheetClip` property (RectangleGeometry) updated on zoom/pan from code-behind
- **EditorCanvas.xaml.cs**: Subscribe to ZoomPanManager.PropertyChanged to update GridNodesLayer.SheetClip
- **GridManager.cs**: no changes
- **GridHelper.cs**: no changes
- **Tests**: no changes
