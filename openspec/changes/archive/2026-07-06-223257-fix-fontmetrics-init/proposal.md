## Why

`FontMetrics.Initialize()` uses `new GlyphTypeface(file:// URI)` to load GOST font metrics at startup, which throws `NullReferenceException` deep inside WPF's font cache (`FontFaceLayoutInfo.IntMap.TryGetValue`). The catch block falls back to heuristic values — so the font metrics correction never actually applies. Markers remain at uncorrected positions, defeating the purpose of `fix-text-markers-rotation`.

## What Changes

- Replace `FontMetrics.LoadFont()` temp-file + `GlyphTypeface(Uri)` approach with WPF's standard `FontFamily` → `GetTypefaces()` → `TryGetGlyphTypeface()` pattern
- Remove temp file extraction (`Application.GetResourceStream`, `File.Create`, temp directory) — the pack URI is passed directly to `FontFamily` which already resolves embedded TTF resources
- Log a warning when font loading fails instead of silently falling back
- Update the `text-font-metrics` spec requirement about initialization to reflect the new loading approach

## Capabilities

### New Capabilities
*(none — this change modifies the implementation approach within an existing capability)*

### Modified Capabilities
- `text-font-metrics`: Change **Requirement: Initialization from WPF** — replace `new GlyphTypeface(Uri)` with `FontFamily` → `TryGetGlyphTypeface()` as the loading mechanism

## Impact

- `Models/FontMetrics.cs` — `LoadFont()` method rewritten (remove temp I/O, use FontFamily API)
- No changes to `Text.cs`, `App.xaml.cs`, or any other file
- No API changes — public interface (`GetHeightRatio`, `GetAdvWidthRatio`, `IsInitialized`) unchanged
- All existing tests continue to pass unchanged
