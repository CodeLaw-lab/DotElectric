# text-font-metrics Specification

## Purpose
Provide accurate font geometry metrics (height ratio, advance width ratio) for GOST fonts so the model layer can compute bounding boxes that match WPF's actual text rendering.

## Requirements

### Requirement: FontMetrics class provides height ratio per font

The system SHALL provide a `FontMetrics` class that exposes the height ratio (actual rendered height / font size) for each GOST font.

#### Scenario: GOST B height ratio from GlyphTypeface

- **WHEN** `FontMetrics.GetHeightRatio("ГОСТ Б")` is called after initialization
- **THEN** the returned value SHALL equal `glyphTypeface.Height / glyphTypeface.UnitsPerEm` for the GOST B font (approximately 1.17)

#### Scenario: GOST A height ratio from GlyphTypeface

- **WHEN** `FontMetrics.GetHeightRatio("ГОСТ А")` is called after initialization
- **THEN** the returned value SHALL equal `glyphTypeface.Height / glyphTypeface.UnitsPerEm` for the GOST A font

### Requirement: FontMetrics class provides advance width ratio per font

The system SHALL provide the average advance width ratio (average glyph advance width / font size) for each GOST font, computed across a representative character set (ASCII Latin + Cyrillic uppercase/lowercase).

#### Scenario: GOST B advance width ratio

- **WHEN** `FontMetrics.GetAdvanceWidthRatio("ГОСТ Б")` is called after initialization
- **THEN** the returned value SHALL be greater than 0.4 and less than 0.8

### Requirement: Initialization from WPF GlyphTypeface API

`FontMetrics.Initialize()` SHALL load each GOST TTF font file via `GlyphTypeface`, extract the Height and AdvanceWidths metrics, and cache the per-font ratios.

#### Scenario: Successful initialization

- **WHEN** `FontMetrics.Initialize()` is called on WPF STA thread with both GOST TTF files available as embedded resources
- **THEN** both GOST А and GOST Б metrics SHALL be populated
- **AND** `FontMetrics.IsInitialized` SHALL be `true`

#### Scenario: Missing font file fallback

- **WHEN** `FontMetrics.Initialize()` is called but one or both GOST TTF files cannot be loaded
- **THEN** the system SHALL fall back to existing heuristic values (height ratio = 1.0, advance width ratio = 0.5 for GOST А, 0.65 for GOST Б)
- **AND** a warning SHALL be logged

### Requirement: Test support with custom values

`FontMetrics` SHALL provide a method `InitializeWithTestValues(double heightRatio, double advWidthRatio, string fontName)` that injects known values for testing without WPF dependency.

#### Scenario: Test with known metrics

- **WHEN** `FontMetrics.InitializeWithTestValues(1.2, 0.6, "ГОСТ А")` is called
- **THEN** `GetHeightRatio("ГОСТ А")` returns 1.2
- **AND** `GetAdvanceWidthRatio("ГОСТ А")` returns 0.6
