## Context

Selection markers on Text objects are currently positioned using model-computed `RotatedCorner*` properties (`Text.cs:161-171`), which derive positions from `HeightMicrons` and `WidthMicrons`. These are font-metric approximations that don't precisely match the actual WPF-rendered text extent. The discrepancy grows with rotation — the visual selection highlight (blue Background set by DataTrigger) shows rendered bounds, while markers show model-predicted bounds.

The existing marker system uses a separate `ItemsControl` (`EditorCanvas.xaml:521-536`) with a `MarkerPosition` attached behavior that converts model coordinates to WPF pixels via `ModelXToCanvasLeftConverter`/`ModelYToCanvasTopConverter`. This pipeline is inherently tied to model dimensions.

## Goals / Non-Goals

**Goals:**
- Selection markers on text objects always align with the visual text bounds at any rotation angle
- Markers update when text content, font size, or rotation angle changes
- Keep model unchanged — `RotatedCorner*`, `HeightMicrons`, `WidthMicrons` remain for hit-test, bounding box, resize
- Remove the blue Background highlight from text selection (markers are the sole selection indicator)

**Non-Goals:**
- Changing Text model properties (`RotatedCorner*`, `HeightMicrons`, `WidthMicrons`)
- Changing hit-test logic (`ContainsPoint`, `GetBoundingBox`)
- Changing resize behavior
- Changing marker appearance (same `SquareMarker` style)

## Decisions

### Decision 1: Attached behavior on TextBlock, not on marker ItemsControl

**Chosen:** `TextSelectionMarkerBehavior` attached to the TextBlock in its DataTemplate.

**Alternatives considered:**
- Keep in marker ItemsControl, compute corners from model → discarded (same model-vs-view mismatch)
- New standalone ItemsControl with pixel-level positioning → complex, redundant with existing Infra

**Rationale:** The behavior has direct access to the rendered TextBlock (`ActualWidth`, `ActualHeight`, `LayoutTransform`), making corner computation pixel-accurate.

### Decision 2: `FormattedText` for pre-transform dimension measurement

**Chosen:** Use `System.Windows.Media.FormattedText` to measure the text before rotation.

**Alternatives considered:**
- Read `TextBlock.DesiredSize` before transform → not accessible after layout
- Temporary remove `LayoutTransform`, measure, restore → causes flicker
- Reverse-engineer from `ActualWidth`/`ActualHeight` and rotation angle → ambiguous at 45°

**Rationale:** `FormattedText` uses the same WPF text layout engine as `TextBlock`, so dimensions match exactly. No layout side effects.

### Decision 3: Overlay Canvas inside DataTemplate Grid wrapper

**Chosen:** Wrap TextBlock and an overlay Canvas in a Grid, behavior manages markers in the overlay Canvas.

```
<DataTemplate>
  <Grid>
    <TextBlock behaviors:TextSelectionMarkerBehavior.IsEnabled="True" />
    <Canvas x:Name="MarkerLayer" IsHitTestVisible="False" />
  </Grid>
</DataTemplate>
```

**Alternatives considered:**
- Position markers on DrawingCanvas via ancestor transform → complicates Z-order and hit-test
- Position markers inside the TextBlock → rotate with text (markers should stay axis-aligned)

**Rationale:** Grid shares the same origin as the ContentPresenter (model position). Overlay Canvas is a sibling, so markers are in the same coordinate space as the TextBlock but don't rotate with it. Z-order is correct (markers above text in the same container).

### Decision 4: Remove Background highlight from selection DataTrigger

**Chosen:** Keep blue Foreground (#0078D4) and Bold on selection, remove the Background (#E0F0FF) setter from the DataTrigger.

**Rationale:** With markers correctly tracking visual bounds, the Background highlight is redundant and creates a visual comparison that reveals any remaining sub-pixel discrepancy. The markers alone are sufficient to indicate selection.

### Decision 5: Marker corner computation in WPF Y↓ space

**Chosen:** Compute corners in WPF Y↓ coordinates using CW rotation matrix, `FormattedText` for pre-transform dimensions.

```
Given: W = ft.Width, H = ft.Height, θ (CW, degrees)
angleRad = θ × π / 180
cos = Cos(angleRad), sin = Sin(angleRad)

CW rotation in Y↓:
  x' = x·cosθ + y·sinθ
  y' = -x·sinθ + y·cosθ

Corners (relative to Grid origin):
  TL = (0, 0)
  TR = (W·cosθ, -W·sinθ)
  BL = (H·sinθ, H·cosθ)
  BR = (W·cosθ + H·sinθ, -W·sinθ + H·cosθ)

Markers positioned at these coords in the overlay Canvas.
```

This matches WPF's `RotateTransform` behavior exactly.

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| `FormattedText` measurement diverges from `TextBlock` layout | Both use the same WPF layout engine; test at common rotation angles (0°, 45°, 90°, 135°, 180°, 270°) |
| Behavior lifecycle — markers not created if TextBlock never loads | Safe: `Loaded` event fires once on first render |
| Performance — reformatting on every property change | Coalesce updates via `Dispatcher.BeginInvoke` (priority `Render`); only measure when visible |
| Empty/null content — `FormattedText` throws | Guard: skip measurement and hide markers when `Content` is null/empty |
| STA requirement — `FormattedText` needs STA | Already satisfied: WPF tests use `WpfContext`; production always runs on STA |
| Interaction with InlineTextEditor (double-click edit) | Behavior subscribes to `DataContext` property changes via `INotifyPropertyChanged`; after edit commits, `Content` fires → trigger remeasure + reposition |
