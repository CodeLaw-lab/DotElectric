# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Pipeline: увеличение покрытия тестами до 75.15% (было ~59-67%)
- ~195 новых тестов: Commands null guards, Tools Reset(), Grid edge cases, Services, Models, MainViewModel, FontMetrics, TemplateObjectBase, NonZeroToVisibilityConverter, CustomSheetDialogViewModel, ShortcutRegistry
- Template.Clone() — deep copy с Metadata, Sheet и Objects
- PointMicrons operator+ и operator- (компонентное сложение/вычитание)

### Changed
- AGENTS.md: обновлены счётчики тестов (2035) и покрытие (75.15%)

### Fixed
- README.md: повторное исправление кодировки (UTF-8 double-encoding / mojibake)
- README.md: исправлена кодировка (UTF-8 double-encoding / mojibake) — восстановлены русский текст и эмодзи
- H1: `AutosaveService` event `Action?` → `Func<Task>?` + `InvokeAsync` in `IDispatcherService` (eliminates async-void in `MainViewModel`)
- H2: `IValidationService` interface added; `TemplateValidator` uses injectable `IValidationService` instead of static `ValidationService.ValidateHexColor()`
- H3: `DialogServiceFactory` dead code removed from `IDialogService.cs`
- H4: `PrintVisualProvider` nulled in `EditorViewModel.Dispose()` (dangling reference cleanup)
- H5: No-op `Свойства (F4)` MenuItem removed from `EditorCanvas.xaml` context menu
- TemplateTests.cs: исправлен синтаксис (лишняя закрывающая скобка)

### Added
- CI workflow: GitHub Actions (build, test, coverage gate 75%) на `windows-latest`
- Sprint STA: `WpfContext` helper — STA-thread dispatcher for WPF unit tests
- Sprint STA: `TextBoxLostFocusCommandBehaviorTests` — 14 tests (DP get/set, OnLostFocus, OnKeyDown Enter via STA)
- Sprint STA: `ComboBoxSelectionChangedCommandBehaviorTests` — 10 tests (DP get/set, OnSelectionChanged via STA)
- Sprint STA: `ZoomComboBoxBehaviorTests` — 11 tests (DP get/set, ApplyZoom parsing, events via real EditorViewModel)
- Sprint STA: `MarkerPositionTests` — 10 tests (DP get/set for XPropertyPath/YPropertyPath)
- Sprint STA: 4 behavior files made handlers `internal static` for testability (matching existing CanvasInputRouter pattern)
- Sprint 54: `IDialogFileService`/`WpfDialogFileService` — WPF dialog isolation from FileService for CI/testability
- Sprint 54: FileService now delegates `OpenFileDialog()`/`SaveFileDialog()` to `IDialogFileService` (optional DI)
- Sprint 54: 5 dialog tests rewritten with `Mock<IDialogFileService>` (zero UI calls in headless)
- Sprint 53: `IDateTimeProvider`/`DateTimeProvider` — abstraction over `DateTime.UtcNow`, injected into 3 services
- Sprint 53: `MarkerPosition` attached behavior — `XPropertyPath`/`YPropertyPath` auto-create Canvas.Left/Top MultiBindings (XAML markers reduced from 250→40 lines)
- Sprint 53: `EditorCanvasBehaviorTests` — 18 unit tests for `ToToolMouseButton`, `ToToolModifiers`, `ToToolKey`
- Sprint 53: `[InternalsVisibleTo]` — methods changed from `private` to `internal static` for testability
- Sprint 53: All `Thread.Sleep` removed from test code (5 files, ~2310ms total), replaced with `Mock<IDateTimeProvider>`
- Sprint 52: Free rotation for text (0-359°, `cos`/`sin` math in `ContainsPoint`/`GetBoundingBox`)
- Sprint 52: GOST font names fixed (`#GOST Type AU`/`BU`) with FontNameToFamilyConverter
- Sprint 52: Double-click inline text editing with rotation-aligned TextBox
- Sprint 51: Panning `CaptureMouse()`/`ReleaseMouseCapture()` for stable drag outside canvas
- Sprint 50: Clipboard (Copy/Paste/Cut) with 10mm offset, BatchCommand, auto-select, statusbar
- Sprint 50: Ctrl+X shortcut, toolbar/menu buttons for Cut
- Sprint 49: Edge-based minimum-size clamp in ResizeTool (moving edges only)
- Sprint 48: Dirty indicator `*` in tab header via PropertyChanged forwarding
- Sprint 47: Grid 1mm step hidden at low zoom (MinPixelSpacing=5px in GridManager + GridHelper)
- Sprint 46: Right-click context menu on canvas, tab context menu close commands
- Sprint 46: `CloseTab`/`CloseOtherTabs`/`CloseAllTabs` (renamed from `*Async` for RelayCommand)
- Sprint 45: Pan delta from Window-relative coordinates (fixes runaway acceleration)
- Sprint 44: `PropertiesViewModel` subscribes to `INotifyPropertyChanged` on selected object
- Sprint 44: Text INPC for all properties (FontSizeMicrons, Content, FontName, TextType, RotationAngle)
- Sprint 43: `GetCurrentTool()` `case "Resize"` dispatch fix
- Sprint 42: `StrokeThicknessMicrons` end-to-end (default 500µ = 0.5mm, converter + INPC)
- Sprint 41: Drag delta from saved initial position (`_initialPositions[obj]`), fixes drift
- Sprint 41: Text INPC implementations (MicronsX/Y backing fields + Right/Bottom/Center notifications)
- Sprint 40: Layout-independent keyboard shortcuts via `PreviewKeyDown` (V/L/R/T, E/Shift+E)
- Sprint 40: Selection markers via `ItemsControl ItemsSource="{Binding SelectedObjects}"` (multi-select)
- Sprint 39: Rectangle border-band hit-test (not full AABB, LineHitToleranceMicrons=5mm)
- Sprint 38: INPC for Line/Rectangle (LineType, coordinates, dimensions)
- Sprint 38: `LineTypeToIndexConverter` for ComboBox binding
- Sprint 38: `PurgeOrphanedSelection()` after Undo/Redo
- Sprint 38: `TextBoxLostFocusCommandBehavior` with Enter key handling
- Sprint 38: DrawingRectangleTool passes `_lineType` to `CalculateRectangle()`
- Sprint 37: `IsObjectSelectedConverter` + DataTrigger visual selection (#0078D4 highlight)
- Sprint 37: Preview shape re-assign pattern (create once, update in MouseMove, re-assign reference)
- Sprint 37: `SelectionVersion` (int) for binding re-evaluation
- Sprint 37: `OnPropertyChanged(nameof(Zoom))` in `OnZoomChangedInternal`
- Sprint 37: Escape → `ActiveTool = "Select"` in all tools
- Sprint 31: Decomposed EditorViewModel into 9 managers (ZoomPan, Selection, Clipboard, Tool, Preview, InlineEdit, StatusBar, Grid, DirtyState)
- Sprint 31: 21 new manager unit tests

### Changed
- Sprint 38: DrawingRectangleTool `CalculateRectangle()` accepts lineType parameter
- Sprint 37: `ToModelPoint()` no longer subtracts PanOffset (e.GetPosition already accounts for RenderTransform)
- Sprint 28: Split `CommonConverters.cs` into 16 individual converter files
- Sprint 28: Renamed `ITemplateObject` → `TemplateObjectBase` across 50+ files
- Sprint 27: `EditorViewModel` reduced from ~1037 to ~700 lines (-32%)

### Fixed
- Sprint 41: Drag delta accumulation (was `obj.MicronsX + delta`, now `initialPos + delta`)
- Sprint 40: Tool switching not working with Russian keyboard layout (InputBindings → PreviewKeyDown)
- Sprint 40: `SetActiveTool()` bypassing `ActiveTool` setter (no `OnPropertyChanged`)
- Sprint 39: Rectangle interior area selectable (now border-band only)
- Sprint 38: ComboBox not showing current LineType value (missing SelectedIndex binding)
- Sprint 38: Undo leaving orphaned selection markers
- Sprint 37: Preview shapes not appearing (reference not re-assigned)
- Sprint 37: Canvas not resizing on zoom (missing OnPropertyChanged)
- Sprint 27: Restored missing `HitTest` method in `HitTestHelper`
- Sprint 27: Fixed `ResizeHandle` namespace import

### Metrics
- **Tests:** 2035 (0 failures, 1 pre-existing skip)
- **Coverage:** 75.15% line-rate ✅
- **Build:** 0 errors, 0 warnings
- **P0/P1 bugs:** 0
- **EditorViewModel:** ~1194 lines (9 managers, ~60 forwarding properties)
- **DI services:** IDateTimeProvider, IDialogFileService, IPrintVisualProvider, ITemplateValidator, IEditorContext

---

## [0.1.0] — 2026-04-01

### Added
- Initial release of DotElectric Template Editor
- WPF .NET 10 CAD application for electrical template design
- MVVM architecture with CommunityToolkit.Mvvm
- DI via Microsoft.Extensions.DependencyInjection
- Fixed-point coordinate system (microns)
- `.tdel` file format (XML in ZIP)
- Undo/Redo with 50 levels via `CommandHistory`
- Tools: Select, Rectangle, Line, Text, Resize
- Grid with snap-to-grid functionality
- Zoom/Pan with mouse wheel and drag
- Properties panel for object editing
- Template library with drag & drop
- Autosave service with session recovery
- Settings service with JSON persistence
- Print service with FitToPage scaling
- xUnit v3 test suite (1,394 tests)

[Unreleased]: https://github.com/anomalyco/dotelectric/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/anomalyco/dotelectric/releases/tag/v0.1.0
