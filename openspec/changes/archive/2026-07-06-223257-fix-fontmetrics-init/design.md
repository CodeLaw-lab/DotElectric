## Context

The `fix-text-markers-rotation` change added `FontMetrics.cs` — a static class that loads GOST font metrics at startup to correct `HeightMicrons` and `WidthMicrons` in `Text.cs`. The `LoadFont()` method uses `new GlyphTypeface(file:// URI)` where the URI points to a temp file extracted from assembly resources via `Application.GetResourceStream`.

At runtime, `GlyphTypeface.Initialize()` throws `NullReferenceException` deep inside WPF's font cache (`MS.Internal.FontCache.FontFaceLayoutInfo.IntMap.TryGetValue`). The `catch (Exception)` block catches this and falls back to heuristic values — meaning the font metrics correction never applies. The NRE is a first-chance exception that the debugger breaks on, and even in production the fallback silently degrades the fix.

**Root cause:** `GlyphTypeface(Uri)` uses a different internal path to open font files than `FontFamily`. When given a `file:///` URI pointing to a temp copy of an embedded TTF, WPF's font cache pipeline (`FontFaceLayoutInfo` → `IntMap`) encounters a null internal dictionary. This is likely a WPF internal issue with how the font's name table or OpenType layout tables are parsed from a standalone file vs. from a registered font.

The existing WPF code already loads these fonts successfully via:
```csharp
new FontFamily("pack://application:,,,/Resources/Fonts/#GOST Type AU")
```
This path goes through `FontSource` resolution which reads from the assembly's embedded resources directly, not from a temp file.

## Goals / Non-Goals

**Goals:**
- `FontMetrics.Initialize()` successfully loads GOST font metrics without throwing
- Use WPF's proven font resolution path (`FontFamily` → `GetTypefaces()` → `TryGetGlyphTypeface()`) instead of `GlyphTypeface(Uri)` with temp files
- Remove temp file I/O (eliminates potential for corrupted/stale temp files)
- Log warnings on failure (instead of silent fallback)
- All existing tests continue to pass

**Non-Goals:**
- Changing the `FontMetrics` public API (`GetHeightRatio`, `GetAdvWidthRatio`, `IsInitialized`)
- Changing how `Text.cs` uses the metrics
- Adding new font support beyond GOST A / B

## Decisions

### D1: Replace GlyphTypeface(Uri) with FontFamily + TryGetGlyphTypeface

**Decision:** Change `LoadFont()` to use WPF's standard `FontFamily` → `GetTypefaces()` → `TryGetGlyphTypeface()` pipeline instead of extracting the font to a temp file and constructing `GlyphTypeface(Uri)`.

```csharp
// Before (throws NRE):
var tempPath = ExtractToTemp(ttfFileName);
var glyphUri = new Uri(tempPath, UriKind.Absolute);
var glyphTypeface = new GlyphTypeface(glyphUri);

// After (uses WPF's proven font resolution):
var familyName = fontName == "ГОСТ А" ? "GOST Type AU" : "GOST Type BU";
var family = new FontFamily($"pack://application:,,,/Resources/Fonts/#{familyName}");
foreach (var typeface in family.GetTypefaces())
    if (typeface.TryGetGlyphTypeface(out var gt))
        return gt;
```

**Alternatives considered:**
- Hardcoded constants from PowerShell analysis: faster, no WPF dependency, but requires manual sync if TTF files change
- `Application.GetResourceStream` + `GlyphTypeface(Stream)`: `GlyphTypeface` has no `Stream` constructor
- Keep temp file approach with integrity check: still depends on `GlyphTypeface(Uri)` which has the internal NRE

**Rationale:** The `FontFamily` constructor with `pack://application:,,,/Resources/Fonts/#GOST Type AU` already works throughout the app (`FontNameToFamilyConverter`, `PreviewLineChangedBehavior`, `PrintDocumentGenerator`). WPF resolves the font from the embedded assembly resource directly. `GetTypefaces()` returns the font's typefaces, and `TryGetGlyphTypeface(out glyphTypeface)` gives us the same `GlyphTypeface` instance with all its metrics — without any file I/O or temp files.

### D2: Add warning logging on failure

**Decision:** Log a Serilog warning when font loading fails, instead of silently falling back.

**Rationale:** If initialization fails, the developer should know. A warning in the log helps diagnose font-related issues without crashing the app.

### D3: No changes to spec scenarios for height/width ratios

The existing spec scenarios (`GOST B height ratio from GlyphTypeface`, `GOST A height ratio from GlyphTypeface`) already use `glyphTypeface.Height` as the source of truth. The only change is HOW we get the `glyphTypeface` instance — the ratio computation stays the same (`glyphTypeface.Height`).

## FontMetrics flow (after fix)

```
FontMetrics.Initialize()
  → FontFamily("pack://...#GOST Type AU")  // resolves embedded TTF
    → .GetTypefaces()
      → [0].TryGetGlyphTypeface(out gt)    // returns false on failure
        → gt.Height                        // reads from WPF font cache
        → gt.AdvanceWidths[]               // per-glyph widths
  → cache in _heightRatios / _widthRatios
```

## Risks / Trade-offs

| Risk | Impact | Mitigation |
|------|--------|------------|
| `TryGetGlyphTypeface` returns false (font not found in assembly) | Metrics fall back to heuristics | Already handled by existing try/catch |
| `FontFamily` constructor throws for pack URI | Metrics fall back | Same try/catch covers this |
| Multi-threaded access to FontMetrics during init | Race condition | `lock(_lock)` already in `Initialize()` |
| Tests depend on changed loading mechanism | Test behavior unaffected | Tests use `InitializeWithTestValues()` — no WPF dependency |
