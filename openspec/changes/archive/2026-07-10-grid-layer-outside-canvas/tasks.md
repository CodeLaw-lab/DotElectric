## 1. EditorCanvas.xaml — Restructure visual tree

- [x] 1.1 Wrap DrawingCanvas + GridNodesLayer in a shared Grid with `HorizontalAlignment="Center" VerticalAlignment="Center"`
- [x] 1.2 Move GridNodesLayer from inside DrawingCanvas to before it (sibling order: GridNodesLayer first, DrawingCanvas second)
- [x] 1.3 Add `TranslateTransform` binding to GridNodesLayer.RenderTransform: `{Binding ZoomPanManager.CanvasOffsetX}` / `CanvasOffsetY`
- [x] 1.4 Remove `Width/Height` bindings from GridNodesLayer (no longer sized by sheet)

## 2. GridNodesLayer.cs — Add SheetClip property

- [x] 2.1 Add `SetSheetClip(double x, double y, double width, double height)` method — creates RectangleGeometry and assigns to `UIElement.Clip`

## 3. EditorCanvas.xaml.cs — Subscribe to zoom/pan for clip updates

- [x] 3.1 Subscribe to `ZoomPanManager.PropertyChanged` on VM attach, unsubscribe on detach
- [x] 3.2 Implement `UpdateGridClip()` — compute sheet rect in canvas-local pixels from Zoom + Sheet dimensions, call `GridNodesLayer.SetSheetClip()`

## 4. Verify

- [x] 4.1 `dotnet test src/DotElectric.TemplateEditor.Tests` — 1866 passed, 0 failed, 1 pre-existing skip
- [x] 4.2 `dotnet build src/DotElectric.TemplateEditor.slnx` — 0 errors, 0 warnings
