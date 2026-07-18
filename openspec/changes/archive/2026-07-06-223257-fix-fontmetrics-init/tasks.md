## 1. Rewrite LoadFont to use FontFamily + TryGetGlyphTypeface

- [x] 1.1 Change `LoadFont()` signature: replace `string ttfFileName` with `string familyName`, remove `fallback` params (keep them for backward compat or inline)
- [x] 1.2 Remove temp file extraction code (`Application.GetResourceStream`, `Directory.CreateDirectory`, `File.Create`, `File.Exists`)
- [x] 1.3 Implement new loading: construct `FontFamily` with `pack://application:,,,/Resources/Fonts/#{familyName}`, enumerate `GetTypefaces()`, call `TryGetGlyphTypeface(out var gt)`
- [x] 1.4 Extract `Height` and `AdvanceWidths` from the obtained `GlyphTypeface` (same logic as before)
- [x] 1.5 Add Serilog warning in catch block: `Log.Warning("Failed to load font {FontName}: {Message}", fontName, ex.Message)`
- [x] 1.6 Remove unused `using System.IO;` and `using System.Reflection;` (no longer needed after removing file I/O)

## 2. Update Initialize() callers

- [x] 2.1 Update `FontMetrics.Initialize()` calls to `LoadFont` — pass font family names `"GOST Type AU"` and `"GOST Type BU"` instead of TTF filenames

## 3. Build and verify

- [x] 3.1 Build solution — verify 0 errors, 0 warnings
- [x] 3.2 Run all tests — verify 1821 passed, 1 pre-existing skip, 0 new failures
- [x] 3.3 Run app — verify no first-chance NullReferenceException at startup (FontFamily + TryGetGlyphTypeface eliminates temp-file NRE path)
- [x] 3.4 Verify font metrics are loaded correctly — `FontMetrics.IsInitialized` check already in App.xaml.cs startup log
