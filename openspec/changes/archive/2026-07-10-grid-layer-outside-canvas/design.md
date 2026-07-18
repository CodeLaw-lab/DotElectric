## Context

GridNodesLayer is currently a child of DrawingCanvas. DrawingCanvas has:
- `Width/Height` = `sheet_mm × zoom` (bound through MicronsToPixelConverter)
- `ClipToBounds = True`
- `RenderTransform` with `TranslateTransform(CanvasOffsetX, Y)` for pan
- `HorizontalAlignment = VerticalAlignment = Center`

Since GridNodesLayer is inside the Canvas, its rendering is clipped to the Canvas bounds. At certain zoom levels, grid dots at the sheet edges are partially clipped by the Canvas border. This is a purely visual artifact — the grid data is correct, but rendering is clipped.

### Current visual tree

```
Border (ClipToBounds=True)
└── Canvas (DrawingCanvas)
    ├── ClipToBounds=True, W×H = sheet×zoom
    ├── RenderTransform (pan)
    │
    ├── GridNodesLayer  ← clipped by Canvas bounds
    ├── ItemsControl (objects)
    ├── Preview elements
    ├── Selection box
    ├── InlineTextEditor
    └── Selection markers
```

## Goals / Non-Goals

**Goals:**
- GridNodesLayer renders without being clipped by Canvas bounds
- Grid dots still visually constrained to the sheet area (not spilling into gray margins)
- Grid moves in sync with Canvas during pan (same coordinate space)
- No changes to GridManager, GridHelper, or grid generation logic
- Zero behavioral changes visible to the user

**Non-Goals:**
- Changing grid generation (not touching GridHelper, GridManager)
- Changing grid rendering approach (GridNodesLayer.OnRender unchanged)
- Changing object/selection/preview layers — they stay inside Canvas
- Moving any other element outside Canvas

## Decisions

### Decision 1: GridNodesLayer as sibling of DrawingCanvas

**Choice:** Move GridNodesLayer from inside DrawingCanvas to the Grid wrapper, making it a sibling of DrawingCanvas.

**Layout:**
```xml
<Grid HorizontalAlignment="Center" VerticalAlignment="Center">
    <local:GridNodesLayer x:Name="GridNodesLayer">
        <local:GridNodesLayer.RenderTransform>
            <TranslateTransform X="{Binding ZoomPanManager.CanvasOffsetX}"
                                Y="{Binding ZoomPanManager.CanvasOffsetY}"/>
        </local:GridNodesLayer.RenderTransform>
    </local:GridNodesLayer>
    
    <Canvas x:Name="DrawingCanvas" ClipToBounds="True" ...>
        <Canvas.RenderTransform>
            <TranslateTransform X="{Binding ZoomPanManager.CanvasOffsetX}"
                                Y="{Binding ZoomPanManager.CanvasOffsetY}"/>
        </Canvas.RenderTransform>
        <!-- objects, preview, selection, markers — unchanged -->
    </Canvas>
</Grid>
```

Both GridNodesLayer and DrawingCanvas share the same `TranslateTransform` with `CanvasOffsetX/Y`, so they move identically during pan. GridNodesLayer is NOT clipped by DrawingCanvas because it's a sibling, not a child.

**Rationale:**
- Minimal XAML change (restructure Grid wrapper, add Transform on GridNodesLayer)
- Zero changes to GridManager/GridHelper/GridNodesLayer.OnRender — coordinates are the same canvas-local pixels
- Both layers share the same pan transform → grid dots stay aligned with objects

**Rejected alternatives:**
- **Keep inside Canvas, remove ClipToBounds**: Canvas clips by design (for objects); removing it would let objects spill outside the sheet
- **Adorner layer**: WPF Adorners require special infrastructure and STA threading — overkill

### Decision 2: Clip GridNodesLayer to sheet bounds via RectangleGeometry

**Choice:** Add a `SheetClip` property on GridNodesLayer and set `UIElement.Clip` to a `RectangleGeometry` matching the sheet rectangle in canvas-local pixels.

```csharp
// In EditorCanvas.xaml.cs, subscribed to ZoomPanManager.PropertyChanged
void UpdateGridClip()
{
    var zoom = _zoomPanManager.Zoom;
    var w = Coordinate.ToMm(_template.Sheet.WidthMicrons) * zoom;
    var h = Coordinate.ToMm(_template.Sheet.HeightMicrons) * zoom;
    GridNodesLayer.SetSheetClip(0, 0, w, h);
}
```

The Clip is set in canvas-local pixels (same coordinate space as node data and the RenderTransform). This keeps the grid visually constrained to the sheet — no dots in the gray margin.

**Rationale:**
- Preserves current visual behavior (grid only on sheet, not in margins)
- Clip is applied at the WPF rendering level (GPU, no CPU overhead)
- Updated reactively on zoom change (via PropertyChanged handler)

### Decision 3: Remove Width/Height bindings from GridNodesLayer

**Current:** GridNodesLayer has no explicit Width/Height — it inherits from Canvas.

**After move:** GridNodesLayer is in a Grid wrapper with no size constraints. Its `OnRender` only draws `_nodeCount` dots, so it doesn't need explicit sizing. The `Clip` geometry is the only bounds it needs.

## Risks / Trade-offs

- **[Risk]** GridNodesLayer no longer inherits the centered position from Canvas. **Mitigation:** The parent Grid has `HorizontalAlignment=VerticalAlignment=Center`, so both siblings are centered identically.
- **[Risk]** CanvasOffsetX/Y binding on GridNodesLayer.RenderTransform creates a second binding source. **Mitigation:** Both bind to the same ViewModel property — no synchronization issue.
- **[Risk]** SheetClip needs to be updated on zoom AND pan changes. **Mitigation:** Subscribe to ZoomPanManager.PropertyChanged once for both updates.
- **[Trade-off]** Adding a Grid wrapper around DrawingCanvas increases visual tree depth by 1 level. Negligible performance impact.
