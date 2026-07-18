## Context

The `session1-defects` change added `RotatedCorner*` properties to `Text.cs` so selection markers are positioned at the actual rotated corners of the text bounding box, rather than at AABB corners. Manual testing shows the markers are still visually misaligned:

| Rotation | User reports | Symptom |
|----------|-------------|---------|
| 90°  | Markers LEFT of text | Height underestimation becomes leftward offset |
| 180° | Markers ABOVE and LEFT of text | Both axes affected |
| 270° | Markers ABOVE of text | Height underestimation becomes upward offset |

The root cause: `Text.cs` uses geometric heuristics that don't match WPF's actual text rendering:

```
HeightMicrons = FontSizeMicrons                   // ignores ascent + descent + lineGap
WidthMicrons  = chars × fontSize × factor         // factor=0.5 for GOST A (may not match glyph advance widths)
```

WPF's `TextBlock` with `LayoutTransform/RotateTransform` renders glyphs using the font's design metrics: the bounding box is determined by `GlyphTypeface.Height` (ascender + descender + line gap) and per-glyph advance widths. The model's simplified geometry doesn't account for these, so at non-zero rotations the computed corner positions diverge from the actual rendered glyph corners.

### Current architecture

```
┌─────────────────────────────────────────────────────────┐
│  Model layer (Text.cs)                                   │
│  ┌──────────────────────────────────────────────────┐   │
│  │ HeightMicrons = FontSizeMicrons                   │   │
│  │ WidthMicrons  = chars * fontSize * 0.5 (GOST A)  │   │
│  │                                                   │   │
│  │ RotatedCorner1X = X + W * cosθ                    │   │
│  │ RotatedCorner1Y = topY - W * sinθ                 │   │
│  └──────────────────────────────────────────────────┘   │
│           ↓ bindings                                      │
│  WPF layer (EditorCanvas.xaml)                            │
│  ┌──────────────────────────────────────────────────┐   │
│  │ TextBlock.FontSize = FontSizeMicrons → WPF px    │   │
│  │ TextBlock.LayoutTransform = RotateTransform(θ)   │   │
│  │ WPF renders using GlyphTypeface metrics:          │   │
│  │   • height = FontSize * (ascent+descent+gap)/em  │   │
│  │   • width = sum of glyph advance widths           │   │
│  └──────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
         ↑ marker positions DON'T MATCH rendered glyph boundaries
```

## Goals / Non-Goals

**Goals:**
- Selection markers (4 corner squares) appear at the actual rendered glyph corners at ALL rotation angles
- Resize handle hit-test (`GetTextHandle`) detects clicks at the corrected corner positions
- `ContainsPoint()` and `GetBoundingBox()` use corrected geometry for accurate hit-testing and AABB
- No visible regression at 0° rotation (existing behavior unchanged)
- Both GOST A and GOST B fonts are handled correctly

**Non-Goals:**
- Per-glyph exact width measurement (using average advance width ratio is sufficient)
- Mid-side resize handles for rotated text (remains per existing design)
- Dynamic font loading or custom font support (only the two embedded GOST fonts)
- Changing how the text renders in WPF (the visual appearance stays identical)

## Decisions

### D1: Static FontMetrics initialization at WPF startup

**Decision:** Create a static `FontMetrics` class initialized from `App.xaml.cs` using WPF's `GlyphTypeface` API. Text.cs reads the pre-computed ratios.

```csharp
// Called once at App startup (WPF STA thread)
FontMetrics.Initialize();

// Used in Text.cs (model layer, no WPF dependency)
HeightMicrons = (long)(FontSizeMicrons * FontMetrics.GetHeightRatio(fontName));
WidthMicrons  = ComputeWidth(FontSizeMicrons, Content, FontName);
```

**Alternatives considered:**
- DI service (`IFontMetricsProvider`): more testable but overengineered for two fixed fonts
- Hardcoded constants: fragile, font files could change
- WPF `FormattedText` in production: heavy API, requires WPF context per call
- Per-glyph exact measurement via `GlyphTypeface.AdvanceWidths[]`: would need WPF call per `WidthMicrons` evaluation, impractical for INPC-driven recomputation

**Rationale:** Static init is simple, happens once at startup, and the model layer stays free of WPF references. The `FontMetrics` class lives in `Models/` namespace (no WPF dependency in its public interface), with the WPF-dependent init logic isolated to `FontMetrics.InitializeFromWpf()`.

### D2: HeightMicrons = FontSizeMicrons × heightRatio

**Formula:**
```csharp
HeightMicrons = (long)(FontSizeMicrons * FontMetrics.GetHeightRatio(fontName))
```

Where `heightRatio = glyphTypeface.Height / glyphTypeface.UnitsPerEm`. This accounts for the font's ascender, descender, and line gap, matching WPF's `TextBlock` vertical allocation.

For single-line text (LineCount == 1), this replaces the current `HeightMicrons = FontSizeMicrons`. For multi-line, the same ratio applies to the line spacing factor.

**Impact on lineHeight for multi-line:**
```csharp
var lineHeight = (long)(FontSizeMicrons * FontMetrics.GetHeightRatio(fontName));
if (lc <= 1) return lineHeight;
return (long)(lineHeight + (lc - 1) * lineHeight * LineSpacingFactor);
```

### D3: WidthMicrons with font-aware advance width factor

**Decision:** Derive an `advanceWidthRatio` from the font metrics at startup, then:
```csharp
var ratio = FontMetrics.GetAdvanceWidthRatio(fontName);
return (long)Math.Max(FontSizeMicrons, maxLen * FontSizeMicrons * ratio);
```

The `advanceWidthRatio` = `avgAdvanceWidthInFontUnits / unitsPerEm`, computed as the average advance width across the standard ASCII + Cyrillic character set.

**Rationale:** The existing hardcoded factors (0.5 for GOST A, 0.65 for GOST B) are rough approximations from the GOST 2.304-81 standard. The actual TTF files may have different advance widths. Computing the true average advance width at startup ensures the model matches WPF.

### D4: No changes to rotation math or RotateTransform

The rotation formulas in `RotatedCorner*`, `ContainsPoint()`, `GetBoundingBox()`, and `GetTextHandle()` all use `HeightMicrons` and `WidthMicrons`. They don't need changes — correcting these two inputs propagates correctly to all consumers.

WPF `RotateTransform` binding to `RotationAngle` in the DataTemplate stays as-is. The visual text rotation is already correct — only the model's size estimation is wrong.

### D5: ResizeState captures FontSizeMicrons (unchanged)

`CaptureResizeState()` returns `new ResizeState(X, Y, WidthMicrons, FontSizeMicrons)` — the `Height` field stores `FontSizeMicrons`, not `HeightMicrons`. This is correct because resize changes the font size directly (not the computed height).

`ResizeTool` passes `text.FontSizeMicrons` as `_startHeight`, and `ComputeTextResize` computes scale relative to font size. No changes needed.

## FontMetrics API

```
┌──────────────────────────────────────┐
│  static class FontMetrics             │
│  (Models namespace)                   │
│                                      │
│  + Initialize()                      │
│  + GetHeightRatio(fontName): double  │
│  + GetAdvWidthRatio(fontName): double│
│                                      │
│  - _initialized: bool                 │
│  - _heightRatios: Dictionary<string,  │
│                   double>             │
│  - _widthRatios: Dictionary<string,   │
│                   double>             │
└──────────────────────────────────────┘
         ↕ uses
┌──────────────────────────────────────┐
│  App.xaml.cs                          │
│  FontMetrics.Initialize();           │
│  (WPF STA thread)                     │
└──────────────────────────────────────┘
```

`Initialize()` is idempotent. It loads `GostA.ttf` and `GostB.ttf` via `GlyphTypeface`, extracts metrics, and populates the dictionaries. If called outside WPF (e.g., tests), it falls back to the existing heuristic values (backward compatibility).

**Test support:** Tests can call `FontMetrics.InitializeWithTestValues(heightRatio, widthRatio)` to inject known values without WPF dependency.

## Risks / Trade-offs

| Risk | Impact | Mitigation |
|------|--------|------------|
| `GlyphTypeface` init throws on missing font file | App crash at startup | Wrap in try/catch, fall back to existing heuristic values, log warning |
| Font metrics differ between WPF versions or OS DPI | Slight marker offset on different systems | The ratios are font-specific (not DPI-dependent); static init reads the actual TTF file, so OS shouldn't matter |
| Multi-line text spacing changes | Line spacing factor combined with corrected line height may look different | Keep `LineSpacingFactor` (1.3) but apply to corrected line height; visual impact is minimal and consistent |
| Performance of advance width computation | Average across 100+ chars at startup — negligible one-time cost | Compute at init, cache results |
| Tests without WPF context cannot initialize FontMetrics | TestNewText_WidthAt45Degrees would fail | Provide `InitializeWithTestValues()` for tests; existing heuristic-based tests continue passing with fallback |
