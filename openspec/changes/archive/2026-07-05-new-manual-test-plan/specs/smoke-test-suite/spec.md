## ADDED Requirements

### Requirement: Smoke tests cover critical application path
The smoke test suite SHALL verify the application's critical functionality in under 10 minutes. It SHALL include tests for: application launch, template creation, all three drawing tools (line, rectangle, text), basic selection, undo/redo, file save/load, and print preview.

#### Scenario: Application launches without errors
- **WHEN** the tester runs `dotnet run --project src/DotElectric.TemplateEditor`
- **THEN** the main window appears with title "DotElectric Template Editor" and no console errors

#### Scenario: Create A3 landscape template
- **WHEN** the tester selects File → New → A3 Landscape
- **THEN** a new tab "A3 (алб.) - Без имени" is created with canvas showing 420x297 mm sheet

#### Scenario: Draw all three object types
- **WHEN** the tester uses L (line), R (rectangle), and T (text) tools to create one object each
- **THEN** each object is created, visible on canvas, and auto-selected after creation

#### Scenario: Basic selection and move
- **WHEN** the tester clicks an object and drags it
- **THEN** the object follows the cursor without drift and stays at the new position on release

#### Scenario: Undo and redo
- **WHEN** the tester presses Ctrl+Z then Ctrl+Y
- **THEN** the last action is undone and then redone correctly

#### Scenario: Save and reload round-trip
- **WHEN** the tester saves the file as .tdel, closes the tab, and reopens it
- **THEN** all objects, positions, colors, and properties match the saved state

#### Scenario: Print Preview opens
- **WHEN** the tester presses Ctrl+Shift+P
- **THEN** a PrintPreviewWindow opens with DocumentViewer showing the template with all objects

### Requirement: Smoke test prerequisites
The smoke test suite SHALL run on Windows 10/11 with .NET 10 SDK. The tester SHALL build the solution with `dotnet build` before running tests and confirm 0 errors, 0 warnings.

#### Scenario: Build succeeds
- **WHEN** the tester runs `dotnet build src/DotElectric.TemplateEditor.slnx`
- **THEN** the build completes with 0 errors and 0 warnings

#### Scenario: Unit tests pass
- **WHEN** the tester runs `dotnet test src/DotElectric.TemplateEditor.Tests`
- **THEN** all tests pass with 1796+ passed and 0 failures
