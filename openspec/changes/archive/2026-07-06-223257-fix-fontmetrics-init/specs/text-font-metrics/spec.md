## MODIFIED Requirements

### Requirement: Initialization from WPF GlyphTypeface API

`FontMetrics.Initialize()` SHALL load each GOST font via the `FontFamily` → `GetTypefaces()` → `TryGetGlyphTypeface()` pipeline, using the existing pack URI format that resolves embedded TTF resources.

#### Scenario: Successful initialization via FontFamily

- **WHEN** `FontMetrics.Initialize()` is called on WPF STA thread
- **THEN** the system SHALL construct `FontFamily` with `pack://application:,,,/Resources/Fonts/#GOST Type AU` and `pack://application:,,,/Resources/Fonts/#GOST Type BU`
- **AND** enumerate typefaces via `GetTypefaces()`
- **AND** obtain `GlyphTypeface` via `TryGetGlyphTypeface(out var gt)`
- **AND** `FontMetrics.IsInitialized` SHALL be `true`

#### Scenario: Font load failure falls back to heuristics with warning

- **WHEN** `FontMetrics.Initialize()` is called but `TryGetGlyphTypeface` returns `false` for one or both fonts
- **THEN** the system SHALL fall back to existing heuristic values (height ratio = 1.0, advance width ratio = 0.5 for GOST А, 0.65 for GOST Б)
- **AND** a warning SHALL be logged via Serilog

#### Scenario: FontFamily constructor throws

- **WHEN** `FontMetrics.Initialize()` is called and `FontFamily` constructor throws
- **THEN** the system SHALL fall back to heuristic values
- **AND** a warning SHALL be logged
